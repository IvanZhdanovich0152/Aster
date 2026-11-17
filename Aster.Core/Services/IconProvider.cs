using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Aster.Core.Models;

namespace Aster.Core.Services;

public abstract class IconProvider : IIconProvider
{
    private readonly Dictionary<string, Bitmap> _cache = new(StringComparer.OrdinalIgnoreCase);

    public Bitmap? GetIcon(IFileSystemEntry entry)
    {
        string? assetPath = GetAssetPath(entry);
        if (assetPath is null)
            return null;

        if (_cache.TryGetValue(assetPath, out Bitmap? cached))
            return cached;

        try
        {
            string bitmapPath = Path.ChangeExtension(assetPath, ".png");
            using Stream bitmapStream = AssetLoader.Open(new Uri(bitmapPath));
            Bitmap bitmap = new(bitmapStream);
            _cache[assetPath] = bitmap;
            return bitmap;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }

    protected abstract string? GetAssetPath(IFileSystemEntry entry);

}
