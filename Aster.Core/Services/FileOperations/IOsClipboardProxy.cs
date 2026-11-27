using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Aster.Core.Models;

namespace Aster.Core.Services.FileOperations;

public interface IOsClipboardProxy
{
    Task<IResult> SetTextAsync(string text);

    Task<Bitmap?> TryGetBitmapAsync();

    Task<string?> TryGetTextAsync();

    Task<IFileSystemEntry[]?> TryGetFilesAsync();

    Task<IResult> ClearAsync();

    Task<IResult> CopyToOsClipboardAsync(IFileSystemEntry[] entries);
}
