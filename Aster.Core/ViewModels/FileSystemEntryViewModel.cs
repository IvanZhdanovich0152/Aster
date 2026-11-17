using System;
using Aster.Core.Models;
using ReactiveUI;

namespace Aster.Core.ViewModels;

public sealed class FileSystemEntryViewModel : ReactiveObject
{
    public IFileSystemEntry FileSystemEntry { get; }

    public string PathToEntry => FileSystemEntry.PathToEntry;
    public bool IsDirectory => FileSystemEntry is DirectoryEntry;
    public string Name => FileSystemEntry.Name;
    public DateTime? LastModTime { get; }
    public long? SizeB { get; }

    public FileSystemEntryViewModel(IFileSystemEntry entry)
    {
        FileSystemEntry = entry;
        LastModTime = entry switch
        {
            FileEntryInfo file => file.LastModifiedUtc,
            DirectoryEntryInfo directory => directory.LastModifiedUtc,
            _ => null
        };
        SizeB = entry is FileEntryInfo fileEntry ? fileEntry.SizeBytes : null;
    }
}