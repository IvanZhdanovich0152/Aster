using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Aster.Core.Views.Dialogs;

public partial class InfoDialogWindow : Window
{
    public string Message { get; init; } = string.Empty;

    public InfoDialogWindow() => InitializeComponent();

    protected override void OnOpened(EventArgs e)
    {
        InfoBlock.Text = Message;
        base.OnOpened(e);
    }

    private void KeyPressed(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void CloseWindow(object? sender, RoutedEventArgs e) => Close();
}
