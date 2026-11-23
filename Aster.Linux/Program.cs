using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Input.Platform;
using Aster.Core;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;
using Aster.Core.Services.FileOperations;
using Aster.Core.ViewModels;
using Aster.Core.Views;
using Aster.Linux.Models.FileInformation;
using Aster.Linux.Services.FileOperations;
using Aster.Linux.Services.Shell;
using ReactiveUI.Avalonia;

namespace Aster.Linux;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        App.Bootstrap = new LinuxPlatformBootstrap();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
}

public class LinuxPlatformBootstrap : IPlatformBootstrap
{
    private static readonly LinuxShellHandler LinuxShellHandler = new();

    private static LinuxIconProvider GetIconProvider()
    {
        Dictionary<string, string> extToMime;
        try
        {
            using StreamReader reader = File.OpenText(GlobsParser.GlobsPath);
            extToMime = GlobsParser.Parse(reader);
        }
        catch
        {
            // Parsing failed, continuing without _extToMime
            extToMime = new Dictionary<string, string>();
        }

        return new LinuxIconProvider(
            LinuxShellHandler,
            IconPathResolver.GetSearchPaths(LinuxShellHandler),
            extToMime
        );
    }

    public MainWindowViewModel GetViewModel(
        string initialDir,
        MainWindow mainWindow,
        Action<bool> setDarkMode
    )
    {
        LinuxFileInfoProvider fileInfoProvider = new(LinuxShellHandler);
        LinuxFileIoHandler fileIoHandler = new();
        LocalFileSystem localFileSystem = new()
        {
            LocalFileInfoProvider = fileInfoProvider,
            IconProvider = GetIconProvider(),
            ArchiveManager = new LocalArchiveManager(fileInfoProvider, fileIoHandler),
            FileIoHandler = fileIoHandler,
            BinInteraction = new LinuxBinInteraction(LinuxShellHandler),
            FileProperties = new LinuxFileProperties(new LinuxPropertiesVmFactory()),
            LocalShellHandler = LinuxShellHandler,
        };

        IClipboard clipboard = mainWindow.Clipboard ?? throw new InvalidDataException();
        AvaloniaDialogService dialogService = new(mainWindow);
        ClipboardManager clipboardManager = new(
            new AvaloniaClipboardProxy(clipboard, mainWindow.StorageProvider),
            localFileSystem
        );
        return new MainWindowViewModel(initialDir, localFileSystem, dialogService, setDarkMode)
        {
            ClipboardManager = clipboardManager,
        };
    }

    public IDefaultSettingsProvider GetDefaultSettingsProvider() =>
        new LinuxDefaultSettingsProvider(LinuxShellHandler);
}
