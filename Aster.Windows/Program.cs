using System;
using System.IO;
using Avalonia;
using Avalonia.Input.Platform;
using Aster.Core;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;
using Aster.Core.Services.FileOperations;
using Aster.Core.ViewModels;
using Aster.Core.Views;
using Aster.Windows.Models;
using Aster.Windows.Services.FileOperations;
using Aster.Windows.Services.Shell;
using ReactiveUI.Avalonia;

namespace Aster.Windows;

public class WindowsPlatformBootstrap : IPlatformBootstrap
{
    public MainWindowViewModel GetViewModel(
        string initialDir,
        MainWindow mainWindow,
        Action<AppThemeMode> setThemeMode
    )
    {
        WindowsFileInfoProvider fileInfoProvider = new();
        WindowsFileIoHandler fileIoHandler = new(fileInfoProvider);
        WindowsShellHandler shellHandler = new();

        LocalFileSystem localFileSystem = new()
        {
            LocalFileInfoProvider = fileInfoProvider,
            IconProvider = new WindowsIconProvider(),
            ArchiveManager = new LocalArchiveManager(fileInfoProvider, fileIoHandler),
            FileIoHandler = fileIoHandler,
            BinInteraction = new WindowsBinInteraction(),
            FileProperties = new WindowsFileProperties(),
            LocalShellHandler = shellHandler,
        };

        AvaloniaDialogService dialogService = new(mainWindow);
        IClipboard clipboard = mainWindow.Clipboard ?? throw new InvalidDataException();
        ClipboardManager clipboardManager = new(
            new AvaloniaClipboardProxy(clipboard, mainWindow.StorageProvider),
            localFileSystem
        );
        return new MainWindowViewModel(initialDir, localFileSystem, dialogService, setThemeMode)
        {
            ClipboardManager = clipboardManager,
        };
    }
    

    public IDefaultSettingsProvider GetDefaultSettingsProvider() =>
        new WindowsDefaultSettingsProvider();
}

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        App.Bootstrap = new WindowsPlatformBootstrap();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI( _ => { });
}