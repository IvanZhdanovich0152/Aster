using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Aster.Core.ViewModels;
using Aster.Core.Models;
using Aster.Core.Views;

namespace Aster.Core;

public interface IPlatformBootstrap
{
    public IFileSystem CreateFileSystem();
    
    public string GetDefaultRootPath();
}






public partial class App : Application
{
    
    public static IPlatformBootstrap? Bootstrap { get; set; }
    
    
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
            
        }

        base.OnFrameworkInitializationCompleted();
    }
}