using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    public string Permissions { get; }

    public double Opacity { get; }
    

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
        Permissions = FormatPermissions(entry.UnixMode, isDirectory: false);
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
        Permissions = FormatPermissions(entry.UnixMode, isDirectory: true);
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

    private static string FormatPermissions(UnixFileMode? unixMode, bool isDirectory)
    {
        if (unixMode is not { } mode)
            return "??????????";

        static bool Has(UnixFileMode value, UnixFileMode flag) => (value & flag) != 0;

        char[] permissions =
        [
            isDirectory ? 'd' : '-',
            Has(mode, UnixFileMode.UserRead) ? 'r' : '-',
            Has(mode, UnixFileMode.UserWrite) ? 'w' : '-',
            Has(mode, UnixFileMode.UserExecute) ? 'x' : '-',
            Has(mode, UnixFileMode.GroupRead) ? 'r' : '-',
            Has(mode, UnixFileMode.GroupWrite) ? 'w' : '-',
            Has(mode, UnixFileMode.GroupExecute) ? 'x' : '-',
            Has(mode, UnixFileMode.OtherRead) ? 'r' : '-',
            Has(mode, UnixFileMode.OtherWrite) ? 'w' : '-',
            Has(mode, UnixFileMode.OtherExecute) ? 'x' : '-',
        ];

        if (Has(mode, UnixFileMode.SetUser))
            permissions[3] = Has(mode, UnixFileMode.UserExecute) ? 's' : 'S';
        if (Has(mode, UnixFileMode.SetGroup))
            permissions[6] = Has(mode, UnixFileMode.GroupExecute) ? 's' : 'S';
        if (Has(mode, UnixFileMode.StickyBit))
            permissions[9] = Has(mode, UnixFileMode.OtherExecute) ? 't' : 'T';

        return new string(permissions);
    }

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
