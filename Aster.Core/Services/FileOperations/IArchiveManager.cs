using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;

namespace Aster.Core.Services.FileOperations;

public interface IArchiveManager
{ 
    bool IsArchived(string filePath);

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
