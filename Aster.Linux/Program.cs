using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    public IFileSystem CreateFileSystem() => new LinuxFileSystem();
    public string GetDefaultRootPath()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrWhiteSpace(homePath) ? "/" : homePath;
    }
}

public class LinuxFileSystem : IFileSystem;

