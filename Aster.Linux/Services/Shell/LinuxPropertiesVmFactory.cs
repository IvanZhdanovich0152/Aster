using System.Globalization;
using System.IO;
using Aster.Core.ViewModels;

namespace Aster.Linux.Services.Shell;

public interface IPropertiesVmFactory
{
    public IDisplayable GetPropertiesVm(
        FileSystemEntryViewModel entry,
        FileSystemInfo info,
        string permissions,
        string owner
    );
}

public sealed class LinuxPropertiesVmFactory : IPropertiesVmFactory
{
    private static string GetSize(FileSystemEntryViewModel entry, FileSystemInfo fsInfo)
    {
        if (!entry.IsDirectory)
        {
            if (entry.Size is [.., ' ', 'B'])
                return entry.Size;

            string bytesFormatted = (entry.SizeB ?? 0).ToString("N0", CultureInfo.InvariantCulture);
            return $"{entry.Size} ({bytesFormatted} B)";
        }

        DirectoryInfo dirInfo = (DirectoryInfo)fsInfo;
        try
        {
            int fileCount = dirInfo.GetFiles().Length;
            int dirCount = dirInfo.GetDirectories().Length;
            return $"{fileCount} files, {dirCount} sub-directories";
        }
        catch
        {
            return "_";
        }
    }

    public IDisplayable GetPropertiesVm(
        FileSystemEntryViewModel entry,
        FileSystemInfo info,
        string permissions,
        string owner
    )
    {
        PropertiesWindowViewModel value = new(entry, permissions)
        {
            Size = GetSize(entry, info),
            DateCreated = info.CreationTime,
            DateAccessed = info.LastAccessTime,
            DateModified = info.LastWriteTime,
            Owner = owner,
        };
        return value;
    }
}
