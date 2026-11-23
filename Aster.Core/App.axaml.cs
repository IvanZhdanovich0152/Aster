using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Aster.Core.Models;
using Aster.Core.ViewModels;
using Aster.Core.Views;

namespace Aster.Core;

public interface IPlatformBootstrap
{
    public MainWindowViewModel GetViewModel(
        string initialDir,
        MainWindow mainWindow,
        Action<bool> setDarkMode
    );
    
    public IDefaultSettingsProvider GetDefaultSettingsProvider();
}

public class App : Application
{
    public static IPlatformBootstrap? Bootstrap { get; set; }
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (Bootstrap is null)
            throw new InvalidOperationException("Platform bootstrap is not initialized.");

        AsterSettings.Initialize(Bootstrap.GetDefaultSettingsProvider());
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SimpleResult result = ValidateArgs(desktop.Args, out string? initialDir);
            if (!result.IsOk)
            {
                desktop.Shutdown(1);
                return;
            }

            MainWindow mainWindow = new();
            mainWindow.DataContext = Bootstrap.GetViewModel(
                initialDir ?? AsterSettings.OpenIn,
                mainWindow,
                SetDarkMode
            );
            desktop.MainWindow = mainWindow;
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void SetDarkMode(bool darkMode) =>
        RequestedThemeVariant = darkMode ? ThemeVariant.Dark : ThemeVariant.Light;

    private static SimpleResult ValidateArgs(string[]? args, out string? directory)
    {
        directory = null;

        if (args?.Length > 1)
            return SimpleResult.Error("Incorrect number of arguments.");

        if (args?.Length == 1)
            directory = args[0];

        return SimpleResult.Ok();
    }
}
