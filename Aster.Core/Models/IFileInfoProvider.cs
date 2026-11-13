using System.Collections.Generic;
using System;


namespace Aster.Core.Models;

public readonly struct ExistsInfo
{
    public bool AsFile { get; }
    public bool AsDir { get; }
    
    public bool AsPath => AsFile || AsDir;

    private ExistsInfo(bool asFile, bool asDir)
    {
        AsFile = asFile;
        AsDir = asDir;
    }

    public static ExistsInfo ExistsAsFile() => new(true, false);

    public static ExistsInfo ExistsAsDir() => new(false, true);

    public static ExistsInfo DoesNotExist() => new(false, false);
}

public sealed class DirectoryContents
{
    public required IReadOnlyList<DirectoryEntryInfo> Dirs { get; set; }
    public required IReadOnlyList<FileEntryInfo> Files { get; set; }
}


public interface IFileInfoProvider
{
    public IPathTools PathTool { get; }
    
    public ValueResult<DirectoryContents> GetPathEntries(
        string path,
        bool includeOs,
        bool includeHidden);

    public DateTime? GetFileLastWriteUtc(string path);
    public DateTime? GetDirLastWriteUtc(string path);

    public string GetRoot();

    public ExistsInfo Exists(string path);
}

public interface ILocalFileInfoProvider : IFileInfoProvider
{
    public DriveEntryInfo[] GetDrives();
}