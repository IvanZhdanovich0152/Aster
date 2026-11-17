using System;
using Avalonia.Media.Imaging;
using Aster.Core.Models;
using ReactiveUI;

namespace Aster.Core.ViewModels;

public sealed class FileSystemEntryViewModel : ReactiveObject
{
    public IFileSystemEntry FileSystemEntry { get; }

    public string PathToEntry => FileSystemEntry.PathToEntry;
    public bool IsDirectory => FileSystemEntry is DirectoryEntry;
    public string Name => FileSystemEntry.Name;
    public string Icon => IsDirectory ? "📁" : "📄";
    public Bitmap? IconImage { get; }
    public string Type => IsDirectory
        ? "Directory"
        : string.IsNullOrEmpty(FileSystemEntry.Extension)
            ? "File"
            : $"{FileSystemEntry.Extension.TrimStart('.').ToUpperInvariant()} file";
    public DateTime? LastModTime { get; }
    public string ModifiedDate => LastModTime?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "—";
    public long? SizeB { get; }
    public string FormattedSize => SizeB.HasValue ? FormatSize(SizeB.Value) : "—";

    public FileSystemEntryViewModel(IFileSystemEntry entry, IIconProvider iconProvider)
    {
        FileSystemEntry = entry;
        IconImage = iconProvider.GetIcon(entry);
        LastModTime = entry switch
        {
            FileEntryInfo file => file.LastModifiedUtc,
            DirectoryEntryInfo directory => directory.LastModifiedUtc,
            _ => null
        };
        SizeB = entry is FileEntryInfo fileEntry ? fileEntry.SizeBytes : null;
    }

    private static string FormatSize(long size)
    {
        string[] units = ["B", "KiB", "MiB", "GiB", "TiB"];
        double value = size;
        int unitIndex = 0;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value:0.##} {units[unitIndex]}";
    }
}