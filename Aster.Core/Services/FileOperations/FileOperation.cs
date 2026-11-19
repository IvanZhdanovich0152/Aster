using System;
using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;

namespace Aster.Core.Services.FileOperations;

public interface IFileOperation
{
    public Task<IResult> InvokeAsync(ProgressReporter reporter, CancellationToken ct);
}

public abstract class FileOperation : IFileOperation
{
    private const int WaitBetweenMs = 5;

    protected IFileIoHandler FileIoHandler { get; }
    
    protected IFileSystemEntry[] Entries { get; }

    protected abstract string InvokeVerb { get; }

    protected FileOperation(IFileIoHandler fileIoHandler, IFileSystemEntry[] entries)
    {
        FileIoHandler = fileIoHandler;
        Entries = entries;
    }

    public Task<IResult> InvokeAsync(ProgressReporter reporter, CancellationToken ct) =>
        Task.Run(() => InvokeInternal(InvokeAction, InvokeVerb, reporter, ct), ct);

    private protected async Task<IResult> InvokeInternal(
        Func<IFileSystemEntry, int, IResult> action,
        string opVerb,
        ProgressReporter reporter,
        CancellationToken ct
    )
    {
        CountingReporter rep = new(reporter, Entries.Length);

        Result result = Result.Ok();
        for (int i = 0; i < Entries.Length; i++)
        {
            rep.ReportItem($"{opVerb}: \"{Entries[i].Name}\"");
            result.MergeResult(action(Entries[i], i));
            if (i == Entries.Length - 1)
                return result;

            try
            {
                await Task.Delay(WaitBetweenMs, ct).ConfigureAwait(false);
                ct.ThrowIfCancellationRequested();
            }
            catch
            {
                return SimpleResult.Error("Operation was cancelled.");
            }
        }
        return result;
    }

    protected abstract IResult InvokeAction(IFileSystemEntry entry, int index);
}
