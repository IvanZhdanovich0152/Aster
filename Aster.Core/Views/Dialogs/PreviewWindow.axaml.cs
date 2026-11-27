using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Aster.Core.Services.Dialogs;
using Aster.Core.Services.FileOperations;
using Aster.Core.ViewModels;

namespace Aster.Core.Views.Dialogs;

public sealed record ArchivePreviewRow(
    string Name,
    int Depth,
    bool IsDirectory,
    long Size
)
{
    public string DisplayName => IsDirectory ? $"{Name}/" : Name;

    public string SizeText =>
        IsDirectory ? string.Empty : FileSystemEntryViewModel.GetSizeString(Size);

    public Thickness Indentation => new(Depth * 16, 0, 0, 0);
}

public partial class PreviewWindow : Window
{
    private readonly string? _copyText;

    public PreviewWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    public PreviewWindow(FilePreviewData preview)
        : this()
    {
        Title = preview.Name;
        FileNameText.Text = preview.Name;
        MetadataText.Text = preview.Metadata;
        SummaryText.Text = preview.Summary;

        switch (preview.Kind)
        {
            case PreviewContentKind.Text:
                _copyText = preview.TextContent ?? string.Empty;
                TextContent.Text = NumberLines(_copyText);
                TextContent.IsVisible = true;
                CopyTextButton.IsVisible = true;
                break;

            case PreviewContentKind.Archive:
                ArchiveEntries.ItemsSource = preview
                    .ArchiveEntries.Select(entry => new ArchivePreviewRow(
                        entry.Name,
                        entry.Depth,
                        entry.IsDirectory,
                        entry.Size
                    ))
                    .ToArray();
                ArchiveContent.IsVisible = true;
                break;

            case PreviewContentKind.Image:
                ImageContent.Source = preview.Image
                    ?? throw new ArgumentException(
                        "Image preview is missing its bitmap.",
                        nameof(preview)
                    );
                ImageContent.IsVisible = true;
                break;

            case PreviewContentKind.Unsupported:
                UnsupportedText.Text = preview.Summary;
                UnsupportedText.IsVisible = true;
                SummaryText.Text = string.Empty;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(preview));
        }

        if (preview.IsTruncated && preview.Kind is PreviewContentKind.Archive)
            SummaryText.Text += " · List truncated";
    }

    protected override void OnClosed(EventArgs e)
    {
        if (ImageContent.Source is IDisposable disposableImage)
            disposableImage.Dispose();
        base.OnClosed(e);
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Escape or Key.Space)
        {
            e.Handled = true;
            Close();
        }
    }

    private void ClosePreview(object? sender, RoutedEventArgs e) => Close();

    private async void CopyText(object? sender, RoutedEventArgs e)
    {
        if (Clipboard is null || _copyText is null)
            throw new InvalidOperationException("Text preview clipboard is unavailable.");

        await Clipboard.SetTextAsync(_copyText);
        CopyTextButton.Content = "Copied";
    }

    private static string NumberLines(string text) =>
        string.Join(
            Environment.NewLine,
            text.Replace("\r\n", "\n").Split('\n').Select((line, index) => $"{index + 1,4} {line}")
        );
}
