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

    public FileEntry(string pathToEntry, IPathTool pathTool)
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

    public DirectoryEntry(string dirPath, IPathTool pathTool)
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
    public DateTime LastModified { get; }
    public long SizeBytes { get; }

    public FileEntryInfo(
        string pathToEntry,
        string name,
        string extension,
        long sizeBytes,
        DateTime lastModified
        ) 
        : base(pathToEntry, name, extension)
    {
        LastModified = lastModified;
        SizeBytes = sizeBytes;
    }

    public FileEntryInfo(FileInfo fileInfo)
    :base(fileInfo.FullName, fileInfo.Name, fileInfo.Extension)
    {
        LastModified = fileInfo.LastWriteTime;
        SizeBytes = fileInfo.Length;
    }
}

public class DirectoryEntryInfo : DirectoryEntry
{
    public DateTime LastModified { get; }

    public DirectoryEntryInfo(
        string dirPath,
        string name,
        DateTime lastModified
    )
        : base(dirPath, name)
    {
        LastModified = lastModified;
    }

    private protected DirectoryEntryInfo(DirectoryInfo dirInfo) 
        : base(dirInfo.FullName, dirInfo.Extension)
    {
        LastModified = dirInfo.LastWriteTime;
    }
}



