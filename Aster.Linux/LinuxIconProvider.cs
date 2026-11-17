using System;
using Aster.Core.Models;
using Aster.Core.Services;

namespace Aster.Linux;

internal sealed class LinuxIconProvider : IconProvider
{
    private const string AssetPrefix = "avares://Aster.Core/Assets/Icons/";

    protected override string GetAssetPath(IFileSystemEntry entry)
    {
        if (entry is DirectoryEntry)
            return $"{AssetPrefix}folder.png";

        return entry.Extension.ToLowerInvariant() switch
        {
            ".xml" or ".json" or ".yaml" or ".yml" or ".cs" or ".js" or ".ts" =>
                $"{AssetPrefix}file-code.png",
            ".txt" or ".md" or ".log" =>
                $"{AssetPrefix}file-text.png",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" =>
                $"{AssetPrefix}file-image.png",
            ".zip" or ".tar" or ".gz" or ".7z" or ".rar" =>
                $"{AssetPrefix}file-archive.png",
            _ => $"{AssetPrefix}file.png"
        };
    }
}
