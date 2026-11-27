using System;
using System.IO;

namespace Aster.Core.Models;

public interface IFileSystemEntry
{
    public string PathToEntry { get; }

    public string Name { get; }
    
    public string Extension { get; }

    public string NameWoExtension { get; }
}

public class FileEntry : IFileSystemEntry
{
    public string PathToEntry { get; }

    public string Name { get; }

    public string Extension { get; }

    public string NameWoExtension { get; }

    public FileEntry(string pathToFile, IPathTools pathTools)
    {
        PathToEntry = pathToFile;
        Name = pathTools.GetFileName(pathToFile);
        Extension = pathTools.GetExtension(pathToFile);

        NameWoExtension = string.IsNullOrEmpty(Extension) ? Name : Name[..^Extension.Length];
    }
    
    private protected FileEntry(string pathToFile, string name, string extension)
    {
        PathToEntry = pathToFile;
        Name = name;
        Extension = extension;

        NameWoExtension = string.IsNullOrEmpty(Extension) ? name : name[..^Extension.Length];
    }
}

public class DirectoryEntry : IFileSystemEntry
{
    public string PathToEntry { get; }

    public string Name { get; }

    string IFileSystemEntry.Extension => string.Empty;
    string IFileSystemEntry.NameWoExtension => Name;
    
    public DirectoryEntry(string dirPath, IPathTools pathTools)
    {
        PathToEntry = dirPath;
        Name = pathTools.GetFileName(dirPath);
    }

    private protected DirectoryEntry(string dirPath, string name)
    {
        PathToEntry = dirPath;
        Name = name;
    }
}

public class FileEntryInfo : FileEntry
{
    public DateTime LastModified { get; }

    public DateTime LastModifiedUtc { get; }

    public long SizeB { get; }

    public UnixFileMode? UnixMode { get; }
    
    public FileEntryInfo(
        string pathToFile,
        string name,
        string extension,
        long sizeB,
        DateTime lastModified,
        DateTime lastModifiedUtc,
        UnixFileMode? unixMode = null
    )
        : base(pathToFile, name, extension)
    {
        LastModified = lastModified;
        LastModifiedUtc = lastModifiedUtc;
        SizeB = sizeB;
        UnixMode = unixMode;
    }
    
    public FileEntryInfo(FileInfo fileInfo, UnixFileMode? unixMode = null)
        : base(fileInfo.FullName, fileInfo.Name, fileInfo.Extension)
    {
        LastModified = fileInfo.LastWriteTime;
        LastModifiedUtc = fileInfo.LastWriteTimeUtc;
        SizeB = fileInfo.Length;
        UnixMode = unixMode;
    }
}

public class DirectoryEntryInfo : DirectoryEntry
{
    public DateTime LastModified { get; }
    
    public DateTime LastModifiedUtc { get; }

    public UnixFileMode? UnixMode { get; }
    
    public DirectoryEntryInfo(
        string dirPath,
        string name,
        DateTime lastModified,
        DateTime lastModifiedUtc,
        UnixFileMode? unixMode = null
    )
        : base(dirPath, name)
    {
        LastModified = lastModified;
        LastModifiedUtc = lastModifiedUtc;
        UnixMode = unixMode;
    }

    public DirectoryEntryInfo(DirectoryInfo dirInfo, UnixFileMode? unixMode = null)
        : base(dirInfo.FullName, dirInfo.Name)
    {
        LastModified = dirInfo.LastWriteTime;
        LastModifiedUtc = dirInfo.LastWriteTimeUtc;
        UnixMode = unixMode;
    }
}

public sealed class DriveEntryInfo
{
    public string PathToEntry { get; }

    public string Name { get; }

    public DriveEntryInfo(string pathToEntry, string name)
    {
        PathToEntry = pathToEntry;
        Name = name;
    }

    public DriveEntryInfo(DriveInfo driveInfo)
    {
        PathToEntry = driveInfo.Name;
        Name = !string.IsNullOrEmpty(driveInfo.VolumeLabel)
            ? $"{driveInfo.VolumeLabel} ({driveInfo.Name.TrimEnd(Path.DirectorySeparatorChar)})"
            : driveInfo.Name;
    }
}
