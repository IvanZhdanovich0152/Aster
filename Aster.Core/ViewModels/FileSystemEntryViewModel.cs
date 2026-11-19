using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Aster.Core.Models;
using Aster.Core.Models.FileInformation;
using ReactiveUI;

namespace Aster.Core.ViewModels;

[SuppressMessage(
    "ReSharper",
    "UnusedAutoPropertyAccessor.Global",
    Justification = "Properties are used by the window."
)]
public sealed class FileSystemEntryViewModel : ReactiveObject
{
    private const double HiddenOpacity = 0.5;
    private const string DirectoryLabel = "Directory";
    private static readonly IReadOnlyList<string> ByteUnits =
    [
        "B",
        "KiB",
        "MiB",
        "GiB",
        "TiB",
        "PiB",
    ];

    public string PathToEntry => FileSystemEntry.PathToEntry;

    public bool IsDirectory => FileSystemEntry is DirectoryEntry;

    public Bitmap? Icon
    {
        get => _icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }
    private Bitmap? _icon = null;

    public string Name => FileSystemEntry.Name;

    public DateTime LastModTime { get; }

    public string LastModified { get; }

    public long? SizeB { get; }

    public string Size { get; }

    public string Type { get; }

    public double Opacity { get; }
    

    public bool Staged
    {
        get => _staged;
        private set => this.RaiseAndSetIfChanged(ref _staged, value);
    }
    private bool _staged;

    public bool IsArchived { get; } = false;

    public bool SupportsOpenAs { get; }

    public IFileSystemEntry FileSystemEntry { get; }

    public FileSystemEntryViewModel(IFileSystem fileSystem, FileEntryInfo entry)
    {
        FileSystemEntry = entry;
        LastModTime = entry.LastModified;
        LastModified = GetLastModified(LastModTime);
        SizeB = entry.SizeB;
        Size = GetSizeString(entry.SizeB);
        string extension = entry.Extension.TrimStart('.').ToUpperInvariant();
        Type = string.IsNullOrEmpty(extension) ? "File" : $"{extension} File";

        Opacity = fileSystem.FileInfoProvider.IsHidden(entry.PathToEntry, IsDirectory)
            ? HiddenOpacity
            : 1;
        IsArchived = fileSystem.ArchiveManager.IsArchived(entry.PathToEntry);
        SupportsOpenAs = fileSystem.FileProperties.SupportsOpenAs(entry);

        _ = LoadIconAsync(entry, fileSystem.IconProvider);
    }

    private async Task LoadIconAsync(FileEntryInfo entry, IIconProvider iconProvider) =>
        Icon = await Task.Run(() => iconProvider.GetFileIconAsync(entry.PathToEntry));

    public FileSystemEntryViewModel(IFileSystem fileSystem, DirectoryEntryInfo entry)
    {
        FileSystemEntry = entry;
        LastModTime = entry.LastModified;
        LastModified = GetLastModified(LastModTime);
        SizeB = null;
        Size = string.Empty;
        Type = DirectoryLabel;

        Opacity = fileSystem.FileInfoProvider.IsHidden(entry.PathToEntry, IsDirectory)
            ? HiddenOpacity
            : 1;
        IsArchived = fileSystem.ArchiveManager.IsArchived(entry.PathToEntry);
        SupportsOpenAs = fileSystem.FileProperties.SupportsOpenAs(entry);

        _ = LoadIconAsync(entry, fileSystem.IconProvider);
    }

    private async Task LoadIconAsync(DirectoryEntryInfo entry, IIconProvider iconProvider) =>
        Icon = await Task.Run(() => iconProvider.GetDirectoryIconAsync(entry.PathToEntry));

    private static string GetLastModified(DateTime time) =>
        $"{time.ToShortDateString()} {time.ToShortTimeString()}";

    public static string GetSizeString(long sizeInB)
    {
        long size = sizeInB;
        foreach (string notation in ByteUnits)
        {
            if (size <= AsterSettings.FileSizeUnitLimit)
                return $"{size} {notation}";

            size = (size + 1023) / 1024;
        }
        return $"{size * 1024} {ByteUnits[^1]}";
    }
}
