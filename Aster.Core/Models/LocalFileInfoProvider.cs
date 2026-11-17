using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aster.Core.Models;

public abstract class LocalFileInfoProvider : ILocalFileInfoProvider
{
    public IPathTools PathTools => LocalPathTools.Instance;

    public ValueResult<DirectoryContents> GetPathEntries(
        string path,
        bool includeHidden,
        bool includeOs)
    {
        try
        {
            IEnumerable<FileSystemInfo> entries = new DirectoryInfo(
                path).EnumerateFileSystemInfos();

            List<DirectoryEntryInfo> dirs = new();
            List<FileEntryInfo> files = new();

            foreach (FileSystemInfo entry in entries)
            {
                if (!includeHidden && IsHidden(entry.FullName, entry is DirectoryInfo))
                    continue;
                if (!includeOs && IsOsProtected(entry.FullName, entry is DirectoryInfo))
                    continue;

                if (entry is DirectoryInfo dEI)
                    dirs.Add(new DirectoryEntryInfo(dEI));
                if (entry is FileInfo fEI)
                    files.Add(new FileEntryInfo(fEI));
            }

            return ValueResult<DirectoryContents>.Ok(new DirectoryContents { Files = files, Dirs = dirs });
        }
        catch (Exception ex)
        {
            return ValueResult<DirectoryContents>.Error(ex.Message);
        }
    }

    public DateTime? GetFileLastWriteUtc(string path)
    {
        if (!File.Exists(path))
            return null;
        try
        {
            return File.GetLastWriteTimeUtc(path);
        }
        catch
        {
            return null;
        }
    }

    public DateTime? GetDirLastWriteUtc(string path)
    {
        if (!Directory.Exists(path))
            return null;
        try
        {
            return Directory.GetLastWriteTimeUtc(path);
        }
        catch
        {
            return null;
        }
    }

    public virtual bool IsHidden(string path, bool isDirectory)
    {
        try
        {
            return path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                .Any(part => part.StartsWith(".", StringComparison.Ordinal)) ||
                (isDirectory
                    ? new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.Hidden)
                    : new FileInfo(path).Attributes.HasFlag(FileAttributes.Hidden));
        }
        catch
        {
            return true;
        }
    }

    protected virtual bool IsOsProtected(string path, bool isDirectory)
    {
        try
        {
            return isDirectory
                ? new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.System)
                : new FileInfo(path).Attributes.HasFlag(FileAttributes.System);
        }
        catch
        {
            return true;
        }
    }

    public abstract string GetRoot();


    public ExistsInfo Exists(string path)
    {
        if (Directory.Exists(path))
            return ExistsInfo.ExistsAsDir();

        if (File.Exists(path))
            return ExistsInfo.ExistsAsFile();

        return ExistsInfo.DoesNotExist();
    }

    public abstract DriveEntryInfo[] GetDrives();
}