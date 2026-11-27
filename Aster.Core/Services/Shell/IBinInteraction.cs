using System;
using Aster.Core.Models;

namespace Aster.Core.Services.Shell;

public interface IBinInteraction : IDisposable
{
    public IResult MoveFileToTrash(string filePath);

    public IResult MoveDirToTrash(string dirPath);

    public IResult RestoreFile(string originalFilePath);

    public IResult RestoreDir(string originalDirPath);
}
