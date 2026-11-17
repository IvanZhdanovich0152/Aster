using Avalonia;
using ReactiveUI.Avalonia;
using System;
using Aster.Core;
using Aster.Core.Models;


namespace Aster.Linux;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.Bootstrap = new LinuxPlatformBootstrap();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}


public class LinuxPlatformBootstrap : IPlatformBootstrap
{

    public IFileSystem CreateFileSystem()
    {
        LinuxFileInfoProvider fileInfoProvider = new();
        return new LocalFileSystem
        {
            LocalFileInfoProvider = fileInfoProvider,
            FileOperations = new LocalFileOperations(fileInfoProvider.PathTools),
            IconProvider = new LinuxIconProvider()
        };
    }
    public string GetDefaultRootPath()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrWhiteSpace(homePath) ? "/" : homePath;
    }
    
}

internal sealed class LinuxFileInfoProvider : LocalFileInfoProvider
{
    public override string GetRoot() => "/";

    public override DriveEntryInfo[] GetDrives() => Array.Empty<DriveEntryInfo>();
}

