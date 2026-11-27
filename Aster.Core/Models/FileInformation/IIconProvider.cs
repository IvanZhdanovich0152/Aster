using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Aster.Core.Models.FileInformation;

public interface IIconProvider : IDisposable
{ 
    public Task<Bitmap> GetFileIconAsync(string filePath);

    public Task<Bitmap> GetDirectoryIconAsync(string dirPath);

    public Task<Bitmap> GetDriveIconAsync(DriveEntryInfo driveEntryInfo);
}

public class BaseIconProvider : IIconProvider
{
    private static readonly Bitmap GenericFileIcon = new(
        AssetLoader.Open(new Uri("avares://Aster.Core/Assets/GenericFileIcon.png"))
    );
    private static readonly Bitmap DirectoryIcon = new(
        AssetLoader.Open(new Uri("avares://Aster.Core/Assets/FolderIcon.png"))
    );
    private static readonly Bitmap DriveIcon = new(
        AssetLoader.Open(new Uri("avares://Aster.Core/Assets/DriveIcon.png"))
    );

    public virtual Task<Bitmap> GetFileIconAsync(string filePath) =>
        Task.FromResult(GenericFileIcon);

    public virtual Task<Bitmap> GetDirectoryIconAsync(string dirPath) =>
        Task.FromResult(DirectoryIcon);

    public virtual Task<Bitmap> GetDriveIconAsync(DriveEntryInfo driveEntryInfo) =>
        Task.FromResult(DriveIcon);

    public virtual void Dispose() { }
}
