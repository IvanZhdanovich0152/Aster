using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Aster.Core.ViewModels;

namespace Aster.Core.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnEntryDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
            viewModel.OpenSelectedEntry();
    }
}