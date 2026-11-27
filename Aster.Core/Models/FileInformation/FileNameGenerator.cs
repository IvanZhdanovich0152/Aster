using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aster.Core.Models.FileInformation;

public static class FileNameGenerator
{
    public static string GetAvailableName(
        IFileInfoProvider fileInfoProvider,
        string directory,
        string newName
    )
    {
        IPathTools pathTools = fileInfoProvider.PathTools;
        if (!fileInfoProvider.Exists(pathTools.Combine(directory, newName)).AsPath)
            return newName;

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(newName);
        string extension = Path.GetExtension(newName);
        for (int index = 1; ; index++)
        {
            string newFileName = $"{nameWithoutExtension} ({index}){extension}";

            if (!fileInfoProvider.Exists(pathTools.Combine(directory, newFileName)).AsPath)
                return newFileName;
        }
    }

    public static async Task<string> GetAvailableNameAsync(
        IFileInfoProvider fileInfoProvider,
        string directory,
        string newName
    )
    {
        IPathTools pathTools = fileInfoProvider.PathTools;
        if (!(await fileInfoProvider.ExistsAsync(pathTools.Combine(directory, newName))).AsPath)
            return newName;

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(newName);
        string extension = Path.GetExtension(newName);
        for (int index = 1; ; index++)
        {
            string newFileName = $"{nameWithoutExtension} ({index}){extension}";

            string newPath = pathTools.Combine(directory, newFileName);
            if (!(await fileInfoProvider.ExistsAsync(newPath)).AsPath)
                return newFileName;
        }
    }

    public static string GetNameMultipleDirs(
        IFileInfoProvider fileInfoProvider,
        string newName,
        params string[] dirPaths
    )
    {
        IPathTools pathTools = fileInfoProvider.PathTools;
        if (dirPaths.All(d => !fileInfoProvider.Exists(pathTools.Combine(d, newName)).AsPath))
            return newName;

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(newName);
        string extension = Path.GetExtension(newName);
        for (int index = 1; ; index++)
        {
            string newFileName = $"{nameWithoutExtension} ({index}){extension}";

            if (
                dirPaths.All(d =>
                    !fileInfoProvider.Exists(pathTools.Combine(d, newFileName)).AsPath
                )
            )
                return newFileName;
        }
    }

    public static string GetCopyName(
        IFileInfoProvider fileInfoProvider,
        string directory,
        IFileSystemEntry entry
    ) =>
        GetAvailableName(
            fileInfoProvider,
            directory,
            string.IsNullOrEmpty(entry.NameWoExtension)
                ? $"{entry.Name} - Copy"
                : $"{entry.NameWoExtension} - Copy{entry.Extension}"
        );

    public static string[] GetAvailableNames(
        IFileInfoProvider fileInfoProvider,
        IList<IFileSystemEntry> entries,
        string namingPattern
    )
    {
        IPathTools pathTools = fileInfoProvider.PathTools;

        if (entries.Count == 0)
            return Array.Empty<string>();

        string[] newNames = new string[entries.Count];
        string extension = Path.GetExtension(namingPattern);
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(namingPattern);
        string directory = pathTools.GetParentDir(entries.First().PathToEntry);

        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException(entries.First().PathToEntry);

        int lastIndex = 1;
        for (int i = 0; i < newNames.Length; i++)
        {
            for (int index = lastIndex; ; index++)
            {
                string newFileName = $"{nameWithoutExtension} ({index}){extension}";
                lastIndex++;

                if (!fileInfoProvider.Exists(pathTools.Combine(directory, newFileName)).AsPath)
                {
                    newNames[i] = newFileName;
                    break;
                }
            }
        }
        return newNames;
    }
}
