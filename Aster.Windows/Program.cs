using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aster.Core;
using Aster.Core.Models;
using Aster.Core.Views;
using Aster.Core.ViewModels;


namespace Aster.Windows;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.Bootstrap = new WindowsPlatformBootstrap();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { });
}


public class WindowsPlatformBootstrap : IPlatformBootstrap
{
    public IFileSystem CreateFileSystem() => new WindowsFileSystem();
    public string GetDefaultRootPath() => "C:/";
}

public class WindowsFileSystem : IFileSystem
{
    public IEnumerable<string> GetFiles(string path)
    {
        return new ObservableCollection<string> { "Windows", "User" };
    }
}







