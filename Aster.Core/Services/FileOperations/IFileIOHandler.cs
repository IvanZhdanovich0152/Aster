using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;

namespace Aster.Core.Services.FileOperations;

public interface IFileIoHandler
{
    public IResult NewFileAt(string dirPath, string fileName);

    public IResult NewDirAt(string dirPath, string dirName);

    public IResult RenameFileAt(string filePath, string newName);

    public IResult RenameDirAt(string dirPath, string newName);

    public IResult MoveFileTo(string filePath, string destinationDir);

    public IResult MoveDirTo(string dirPath, string destinationDir);

    public IResult CopyFileTo(string filePath, string destinationDir);

    public IResult CopyDirTo(string dirPath, string destinationDir);

    public IResult DuplicateFile(string filePath, string copyName);

    public IResult DuplicateDir(string dirPath, string copyName);

    public IResult DeleteFile(string filePath);

    public IResult DeleteDir(string dirPath);

    public Task<IResult> WriteFileStreamAsync(
        FileTransferStream fileStream,
        string dirPath,
        ProgressReporter reporter,
        CancellationToken ct
    );
    
    public Task<IResult> WriteDirStreamAsync(
        DirTransferStream dirStream,
        string dirPath,
        ProgressReporter reporter,
        CancellationToken ct
    );
}
