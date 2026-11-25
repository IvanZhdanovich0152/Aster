using System.IO;
using Aster.Core.Models;
using Aster.Core.Services.Shell;

namespace Aster.Linux.Services.Shell;

public class LinuxBinInteraction : IBinInteraction
{
    private readonly IShellCommandHandler _shellHandler;

    public LinuxBinInteraction(IShellCommandHandler shellHandler) => _shellHandler = shellHandler;

    public IResult RestoreFile(string originalFilePath) => RestoreEntry(originalFilePath);

    public IResult RestoreDir(string originalDirPath) => RestoreEntry(originalDirPath);

    private IResult RestoreEntry(string originalPath)
    {
        ValueResult<string> result = _shellHandler.ExecuteCommand("gio", "trash", "--list");
        if (!result.IsOk)
            return SimpleResult.Error(
                $"Failed to access system trash: {string.Join(" ", result.Errors)}"
            );

        if (string.IsNullOrEmpty(result.Value))
            return SimpleResult.Error("Trash list is empty.");

        string normalizedOriginalPath = LocalPathTools.NormalizePath(originalPath);
        string? trashUri = null;
        using StringReader reader = new(result.Value);
        while (reader.ReadLine() is string line)
        {
            string[] fields = line.Split('\t', 2);
            if (
                fields.Length == 2
                && LocalPathTools.PathsAreEqualNormalized(
                    LocalPathTools.NormalizePath(fields[1]),
                    normalizedOriginalPath
                )
            )
            {
                trashUri = fields[0];
                break;
            }
        }

        if (trashUri is null)
            return SimpleResult.Error($"Could not find \"{originalPath}\" in trash.");

        return _shellHandler.ExecuteCommand("gio", "trash", "--restore", trashUri);
    }

    public IResult MoveFileToTrash(string filePath) => MoveEntryToTrash(filePath);

    public IResult MoveDirToTrash(string dirPath) => MoveEntryToTrash(dirPath);

    private ValueResult<string> MoveEntryToTrash(string path) =>
        _shellHandler.ExecuteCommand("gio", "trash", path);

    public void Dispose() { }
}
