using Avalonia.Media.Imaging;

namespace Aster.Core.Models;

public interface IIconProvider
{
    Bitmap? GetIcon(IFileSystemEntry entry);
}
