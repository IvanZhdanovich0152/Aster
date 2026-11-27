using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Aster.Core.ViewModels;

namespace Aster.Core.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsWindowViewModel(AsterSettings.CurrentSettings);
    }

    private void ResetToDefaults(object sender, RoutedEventArgs args)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
        {
            DataContext = null;
            viewModel.ResetToDefault();
            DataContext = viewModel;
        }
    }

    private void CloseWindow(object? sender = null, RoutedEventArgs? args = null) => Close();

    private void SaveAndClose(object? sender = null, RoutedEventArgs? args = null)
    {
        if (DataContext is SettingsWindowViewModel viewModel)
            viewModel.Save(viewModel.ExtractSettings());

        CloseWindow();
    }

    private void KeyPressed(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            CloseWindow();

        if (e.Key == Key.Enter)
            SaveAndClose();
    }
}
