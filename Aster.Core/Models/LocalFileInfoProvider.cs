using System;
using System.Collections.Generic;
using System.IO;

namespace Aster.Core.Models;

public abstract class LocalFileInfoProvider : ILocalFileInfoProvider
{
    public virtual IPathTools PathTools => LocalPathTools.Instance;

    public IPathTools PathTool { get; }

    public ValueResult<DirectoryContents> GetPathEntries(
        string path,
        bool includeOs,
        bool includeHidden)
    {
        try
        {
            IEnumerable<FileSystemInfo> entries = new DirectoryInfo(
                path).EnumerateFileSystemInfos();

            List<DirectoryEntryInfo> dirs = new();
            List<FileEntryInfo> files = new();

            foreach (FileSystemInfo entry in entries)
            {
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
        catch (Exception e)
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
        catch (Exception e)
        {
            return null;
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