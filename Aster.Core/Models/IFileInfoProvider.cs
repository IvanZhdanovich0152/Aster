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
    public required IReadOnlyList<DirectoryEntryInfo> Dirs { get; init; }
    public required IReadOnlyList<FileEntryInfo> Files { get; init; }
}


public interface IFileInfoProvider
{
    IPathTools PathTools { get; }
    
    ValueResult<DirectoryContents> GetPathEntries(
        string path,
        bool includeHidden,
        bool includeOs);

    DateTime? GetFileLastWriteUtc(string path);
    DateTime? GetDirLastWriteUtc(string path);
    bool IsHidden(string path, bool isDirectory);

    string GetRoot();

    ExistsInfo Exists(string path);
}

public interface ILocalFileInfoProvider : IFileInfoProvider
{
    DriveEntryInfo[] GetDrives();
}