using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Aster.Core.Services.FileOperations;

namespace Aster.Core.Services.Dialogs;

public enum PreviewContentKind
{
    Text,
    Archive,
    Image,
    Unsupported,
}

public sealed record FilePreviewData(
    string Name,
    string Metadata,
    PreviewContentKind Kind,
    string Summary,
    string? TextContent,
    IReadOnlyList<ArchivePreviewEntry> ArchiveEntries,
    bool IsTruncated
)
{
    public Bitmap? Image { get; init; }
}
