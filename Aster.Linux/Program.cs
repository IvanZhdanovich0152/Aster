using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using Aster.Core;
using Aster.Core.Models;
using Aster.Core.Views;
using Aster.Core.ViewModels;


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
    public string GetDefaultRootPath() => "/";
}

public class LinuxFileSystem : IFileSystem
{
    public IEnumerable<string> GetFiles(string path) => new List<string>();

}
