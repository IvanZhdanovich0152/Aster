using System;
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
    Justification = "Properties are used by the window"
)]
public class SideBarEntryViewModel : ReactiveObject
{
    private const double HiddenOpacity = 0.5;

    public string PathToEntry { get; }

    public Bitmap? Icon
    {
        get => _icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }
    private Bitmap? _icon = null;

    public string Name { get; }

    public double Opacity { get; } = 1;

    public bool IsDirectory { get; }

    public SideBarEntryViewModel(IFileSystem fileSystem, IFileSystemEntry fileSystemEntry)
    {
        Name = fileSystemEntry.Name;
        PathToEntry = fileSystemEntry.PathToEntry;
        IsDirectory = fileSystemEntry is DirectoryEntry;

        if (fileSystem.FileInfoProvider.IsHidden(fileSystemEntry.PathToEntry, IsDirectory))
            Opacity = HiddenOpacity;

        _ = Task.Run(async () =>
            Icon = await LoadIconAsync(fileSystemEntry, fileSystem.IconProvider)
        );
    }

    private static Task<Bitmap> LoadIconAsync(IFileSystemEntry entry, IIconProvider iconProvider) =>
        entry switch
        {
            FileEntry => iconProvider.GetFileIconAsync(entry.PathToEntry),
            DirectoryEntry => iconProvider.GetDirectoryIconAsync(entry.PathToEntry),
            _ => throw new NotSupportedException(),
        };

    public SideBarEntryViewModel(IFileSystem fileSystem, DriveEntryInfo driveEntryInfo)
    {
        Name = driveEntryInfo.Name;
        PathToEntry = driveEntryInfo.PathToEntry;
        IsDirectory = true;

        _ = Task.Run(async () =>
            Icon = await fileSystem.IconProvider.GetDriveIconAsync(driveEntryInfo)
        );
    }
}
