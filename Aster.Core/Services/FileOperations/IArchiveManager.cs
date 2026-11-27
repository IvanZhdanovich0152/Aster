using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;

namespace Aster.Core.Services.FileOperations;

public sealed record ArchivePreviewEntry(string Name, int Depth, bool IsDirectory, long Size);

public sealed record ArchivePreview(
    IReadOnlyList<ArchivePreviewEntry> Entries,
    int EntryCount,
    long TotalUnpackedSize,
    bool IsTruncated
);

public interface IArchiveManager
{ 
    bool IsArchived(string filePath);

    Task<ArchivePreview> GetArchivePreviewAsync(string archivePath, CancellationToken ct);

    Task<IResult> ArchiveEntriesAsync(
        IList<IFileSystemEntry> entries,
        string destinationDir,
        string archiveName,
        ProgressReporter reporter,
        CancellationToken ct
    );
    Task<IResult> ExtractArchiveAsync(
        string archivePath,
        string destinationPath,
        ProgressReporter reporter,
        CancellationToken ct
    );
}
