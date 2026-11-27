using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Extensions;
using Aster.Core.Models;
using Aster.Core.Models.FileInformation;

namespace Aster.Core.Services.FileInformation;

public enum FileSystemEventType
{
    Created, Deleted, Updated, Moved, Copied,
}

public sealed record FileSystemEvent(
    string OriginalPath,
    bool IsDirectory,
    FileSystemEventType EventType,
    string? NewPath = null
);

public interface IDirectoryWatcher
{
    public event Func<FileSystemEvent, Task>? ChangeDetected;
    
    public Task<IResult> StartAsync(
        TimeSpan pollingInterval,
        bool syncHidden,
        CancellationToken token
    );
}

public sealed class DirectoryWatcher : IDirectoryWatcher
{
    private sealed record FsEntryMeta(bool IsDirectory, DateTime LastWriteTimeUtc, long Length);

    private readonly Location _root;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayEngine;
    private Dictionary<string, FsEntryMeta> _snapshot = new();

    public event Func<FileSystemEvent, Task>? ChangeDetected;

    public DirectoryWatcher(Location root, Func<TimeSpan, CancellationToken, Task> delayEngine)
    {
        _root = root;
        _delayEngine = delayEngine;
    }

    public async Task<IResult> StartAsync(
        TimeSpan pollingInterval,
        bool syncHidden,
        CancellationToken token
    )
    {
        var firstSnapshotResult = await TakeSnapshot(syncHidden);
        if (!firstSnapshotResult.IsOk)
            return firstSnapshotResult;

        _snapshot = firstSnapshotResult.Value;

        Task<IResult> comparisonTask = Task.FromResult<IResult>(SimpleResult.Ok());
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _delayEngine(pollingInterval, token);
            }
            catch
            {
                break; // Задача отменена 
            }

            if (comparisonTask.IsCompleted)
            {
                if (!comparisonTask.Result.IsOk)
                    return comparisonTask.Result;

                comparisonTask = Task.Run(() => DiffOnceAsync(syncHidden));
            }
        }
        return await comparisonTask;
    }

    private async Task<IResult> DiffOnceAsync(bool syncHidden)
    {
        var snapshotResult = await TakeSnapshot(syncHidden);
        if (!snapshotResult.IsOk)
            return snapshotResult;

        await DiffSnapshotsAsync(_snapshot, snapshotResult.Value);
        _snapshot = snapshotResult.Value;
        return SimpleResult.Ok();
    }

    private async Task<ValueResult<Dictionary<string, FsEntryMeta>>> TakeSnapshot(bool syncHidden)
    {
        IFileSystem fs = _root.FileSystem;
        Dictionary<string, FsEntryMeta> snapshot = new();

        Queue<string> queue = new();
        queue.Enqueue(_root.Path);

        while (queue.Count > 0)
        {
            string path = queue.Dequeue();

            ValueResult<DirectoryContents> entriesR = await fs.FileInfoProvider.GetPathEntriesAsync(
                path,
                syncHidden,
                false,
                CancellationToken.None
            );
            if (!entriesR.IsOk)
                return ValueResult<Dictionary<string, FsEntryMeta>>.Error(entriesR);

            foreach (DirectoryEntryInfo dir in entriesR.Value.Dirs)
            {
                snapshot[dir.PathToEntry] = new FsEntryMeta(true, dir.LastModifiedUtc, 0);
                queue.Enqueue(dir.PathToEntry);
            }

            foreach (FileEntryInfo file in entriesR.Value.Files)
                snapshot[file.PathToEntry] = new FsEntryMeta(
                    false,
                    file.LastModifiedUtc,
                    file.SizeB
                );
        }

        return snapshot.OkResult();
    }

    private static IEnumerable<KeyValuePair<string, FsEntryMeta>> OnlyDirs(
        Dictionary<string, FsEntryMeta> snapshot
    ) => snapshot.Where(kp => kp.Value.IsDirectory);

    private static IEnumerable<KeyValuePair<string, FsEntryMeta>> OnlyFiles(
        Dictionary<string, FsEntryMeta> snapshot
    ) => snapshot.Where(kp => !kp.Value.IsDirectory);

    private static IEnumerable<KeyValuePair<string, FsEntryMeta>> DirsFirst(
        Dictionary<string, FsEntryMeta> snapshot
    ) => OnlyDirs(snapshot).Concat(OnlyFiles(snapshot));

    private static IEnumerable<KeyValuePair<string, FsEntryMeta>> FilesFirst(
        Dictionary<string, FsEntryMeta> snapshot
    ) => OnlyFiles(snapshot).Concat(OnlyDirs(snapshot));

    private static bool Modified(FsEntryMeta a, FsEntryMeta b) =>
        a.LastWriteTimeUtc != b.LastWriteTimeUtc || a.Length != b.Length;

    private async Task DiffSnapshotsAsync(
        Dictionary<string, FsEntryMeta> oldSnapshot,
        Dictionary<string, FsEntryMeta> newSnapshot
    )
    {
        foreach ((string path, FsEntryMeta entry) in DirsFirst(newSnapshot))
            if (!oldSnapshot.ContainsKey(path))
                await RaiseAsync(
                    new FileSystemEvent(path, entry.IsDirectory, FileSystemEventType.Created)
                );

        foreach ((string path, FsEntryMeta entry) in FilesFirst(oldSnapshot))
            if (!newSnapshot.ContainsKey(path))
                await RaiseAsync(
                    new FileSystemEvent(path, entry.IsDirectory, FileSystemEventType.Deleted)
                );

        foreach ((string filePath, FsEntryMeta entry) in OnlyFiles(newSnapshot))
            if (
                oldSnapshot.TryGetValue(filePath, out FsEntryMeta? oldEntry)
                && Modified(entry, oldEntry)
            )
                await RaiseAsync(new FileSystemEvent(filePath, false, FileSystemEventType.Updated));
    }

    private Task RaiseAsync(FileSystemEvent fsEvent)
    {
        Func<FileSystemEvent, Task>? eventMethod = ChangeDetected;

        if (eventMethod is null)
            return Task.CompletedTask;

        return eventMethod(fsEvent);
    }
}
