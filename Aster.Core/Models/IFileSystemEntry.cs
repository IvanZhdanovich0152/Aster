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

    public FileEntry(string pathToEntry, IPathTools pathTool)
    {
        PathToEntry = pathToEntry;
        Name = pathTool.GetFileName(pathToEntry);
        Extension = pathTool.GetExtension(pathToEntry);
        NameWoExtension = string.IsNullOrEmpty(Extension) ? Name : Name[.. ^Extension.Length];
    }

    private protected FileEntry(string pathToEntry, string name, string extension)
    {
        PathToEntry = pathToEntry;
        Name = name;
        Extension = extension;
        NameWoExtension = string.IsNullOrEmpty(Extension) ? Name : Name[.. ^Extension.Length];
    }
}

public class DirectoryEntry : IFileSystemEntry
{
    public string PathToEntry { get; }
    public string Name { get; }
    
    string IFileSystemEntry.Extension => string.Empty; 
    string IFileSystemEntry.NameWoExtension => Name;

    public DirectoryEntry(string dirPath, IPathTools pathTool)
    {
        PathToEntry = dirPath;
        Name = pathTool.GetFileName(dirPath);
    }

    private protected DirectoryEntry(string dirPath, string name)
    {
        PathToEntry = dirPath;
        Name = name;
    }
}

public class FileEntryInfo : FileEntry
{
    public DateTime LastModifiedUtc { get; }
    public long SizeBytes { get; }

    public FileEntryInfo(
        string pathToEntry,
        string name,
        string extension,
        long sizeBytes,
        DateTime lastModifiedUtc
        ) 
        : base(pathToEntry, name, extension)
    {
        LastModifiedUtc = lastModifiedUtc;
        SizeBytes = sizeBytes;
    }

    public FileEntryInfo(FileInfo fileInfo)
    :base(fileInfo.FullName, fileInfo.Name, fileInfo.Extension)
    {
        LastModifiedUtc = fileInfo.LastWriteTimeUtc;
        SizeBytes = fileInfo.Length;
    }
}

public class DirectoryEntryInfo : DirectoryEntry
{
    public DateTime LastModifiedUtc { get; }

    public DirectoryEntryInfo(
        string dirPath,
        string name,
        DateTime lastModifiedUtc
    )
        : base(dirPath, name)
    {
        LastModifiedUtc = lastModifiedUtc;
    }

    public DirectoryEntryInfo(DirectoryInfo dirInfo) 
        : base(dirInfo.FullName, dirInfo.Name)
    {
        LastModifiedUtc = dirInfo.LastWriteTimeUtc;
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
        Name = !string.IsNullOrWhiteSpace(driveInfo.VolumeLabel)
            ? $"{driveInfo.VolumeLabel} ({driveInfo.Name.TrimEnd(Path.DirectorySeparatorChar)}"
            : driveInfo.Name;
    }
}



