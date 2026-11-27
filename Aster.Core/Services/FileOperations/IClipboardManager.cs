using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;
using Aster.Core.Services.FileOperations.Undoable;

namespace Aster.Core.Services.FileOperations;

public interface IClipboardManager
{
    public Task<IResult> CutAsync(IFileSystemEntry[] selectedFiles, Location currentLocation);

    public Task<IResult> CopyAsync(IFileSystemEntry[] selectedFiles, Location currentLocation);

    public Task<ValueResult<IUndoableFileOperation?>> PasteAsync(
        Location destination,
        ProgressReporter reporter,
        CancellationToken ct
    );

    public Task<IResult> CopyPathToFileAsync(string filePath);
}
