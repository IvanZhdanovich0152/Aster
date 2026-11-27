using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Aster.Core.Models;
using Aster.Core.ViewModels;
using Aster.Core.Views;
using Aster.Core.Views.Dialogs;
using Avalonia.Threading;

namespace Aster.Core;

public interface IPlatformBootstrap
{
    public MainWindowViewModel GetViewModel(
        string initialDir,
        MainWindow mainWindow,
        Action<AppThemeMode> setThemeMode
    );
    
    public IDefaultSettingsProvider GetDefaultSettingsProvider();
}

public class App : Application
{
    private static int _exceptionDialogOpen;

    public static IPlatformBootstrap? Bootstrap { get; set; }
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (Bootstrap is null)
        {
            if (!Design.IsDesignMode)
                throw new InvalidOperationException("Platform bootstrap is not initialized.");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime designerLifetime)
                designerLifetime.MainWindow = new MainWindow();

            base.OnFrameworkInitializationCompleted();
            return;
        }

        RegisterExceptionHandlers();

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
            MainWindowViewModel viewModel = Bootstrap.GetViewModel(
                initialDir ?? AsterSettings.OpenIn,
                mainWindow,
                SetThemeMode
            );
            viewModel.ObserveCommandExceptions(ReportException);
            mainWindow.DataContext = viewModel;
            desktop.MainWindow = mainWindow;
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterExceptionHandlers()
    {
        Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
    }

    private static void OnDispatcherUnhandledException(
        object? sender,
        DispatcherUnhandledExceptionEventArgs e
    )
    {
        e.Handled = true;
        ReportException(e.Exception);
    }

    private static void OnUnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e
    )
    {
        e.SetObserved();
        ReportException(e.Exception);
    }

    private static void OnAppDomainUnhandledException(
        object? sender,
        UnhandledExceptionEventArgs e
    )
    {
        if (e.ExceptionObject is Exception exception)
        {
            Trace.TraceError("Fatal unhandled exception:{0}{1}", Environment.NewLine, exception);
            if (!e.IsTerminating)
                ReportException(exception);
        }
        else
            Trace.TraceError("Fatal unhandled exception: {0}", e.ExceptionObject);
    }

    private static void ReportException(Exception exception)
    {
        Trace.TraceError("Unhandled application exception:{0}{1}", Environment.NewLine, exception);
        Dispatcher.UIThread.Post(() =>
        {
            if (Interlocked.Exchange(ref _exceptionDialogOpen, 1) != 0)
                return;

            InfoDialogWindow dialog = new()
            {
                Title = "Unexpected error",
                Message =
                    "An unexpected error occurred. Details are shown below."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception,
            };
            dialog.Closed += (_, _) => Interlocked.Exchange(ref _exceptionDialogOpen, 0);

            try
            {
                if (
                    Current?.ApplicationLifetime
                    is IClassicDesktopStyleApplicationLifetime { MainWindow: { } owner }
                )
                    dialog.ShowDialog(owner);
                else
                    dialog.Show();
            }
            catch (Exception dialogException)
            {
                Interlocked.Exchange(ref _exceptionDialogOpen, 0);
                Trace.TraceError(
                    "Failed to show the exception dialog:{0}{1}",
                    Environment.NewLine,
                    dialogException
                );
            }
        });
    }

    private void SetThemeMode(AppThemeMode themeMode) =>
        RequestedThemeVariant = themeMode switch
        {
            AppThemeMode.Light => ThemeVariant.Light,
            AppThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };

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
