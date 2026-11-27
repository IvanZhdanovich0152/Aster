using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Aster.Core.ViewModels;
using Aster.Core.Views.Helpers; 
using ICommand = System.Windows.Input.ICommand;

namespace Aster.Core.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    private readonly Dictionary<Button, KeyGesture> _buttonHotKeys = new();
    private readonly List<KeyBinding> _keyBindings = new();
    private readonly KeyBinding? _backupDeleteKeyBinding;

    private readonly DataTemplate _iconViewTemplate;
    private readonly DataTemplate _listViewTemplate;
    private readonly DataTemplate _searchViewTemplate;
    private readonly ItemsPanelTemplate _listViewPanel;
    private readonly ItemsPanelTemplate _iconViewPanel;

    private DataTemplate _previousTemplate;
    private ItemsPanelTemplate _previousPanel;

    public MainWindow()
    {
        InitializeComponent();

        if (
            Resources["ListViewTemplate"] is not DataTemplate listViewTemplate
            || Resources["ListViewTemplateLinux"] is not DataTemplate listViewTemplateLinux
            || Resources["IconViewTemplate"] is not DataTemplate iconViewTemplate
            || Resources["SearchViewTemplate"] is not DataTemplate searchViewTemplate
            || Resources["ListViewPanel"] is not ItemsPanelTemplate listViewPanel
            || Resources["IconViewPanel"] is not ItemsPanelTemplate iconViewPanel
        )
            throw new UnreachableException();

        _listViewTemplate = OperatingSystem.IsLinux() ? listViewTemplateLinux : listViewTemplate;
        _iconViewTemplate = iconViewTemplate;
        _searchViewTemplate = searchViewTemplate;
        _listViewPanel = listViewPanel;
        _iconViewPanel = iconViewPanel;

        _previousPanel = listViewPanel;
        _previousTemplate = listViewTemplate;

        _backupDeleteKeyBinding = KeyBindings.FirstOrDefault(kb => kb.Gesture.Key is Key.None);

        AddHandler(KeyDownEvent, TunnelKeyDown, RoutingStrategies.Tunnel);
    }

    private void ViewModelLoaded(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && _viewModel is null)
            _viewModel = viewModel;

        ClearFocus();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainWindowViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel is null)
            return;

        switch (e.PropertyName)
        {
            case nameof(MainWindowViewModel.Searching) when _viewModel.Searching:
                FileDisplay.ItemTemplate = _searchViewTemplate;
                FileDisplay.ItemsPanel = _listViewPanel;
                break;

            case nameof(MainWindowViewModel.Searching):
                FileDisplay.ItemTemplate = _previousTemplate;
                FileDisplay.ItemsPanel = _previousPanel;
                SearchBox.Text = string.Empty;
                break;

            case nameof(MainWindowViewModel.IsLocal) when _backupDeleteKeyBinding is not null:
                _backupDeleteKeyBinding.Gesture = _viewModel.IsLocal
                    ? new KeyGesture(Key.None)
                    : new KeyGesture(Key.Delete);
                break;
        }
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        foreach (Button button in this.GetVisualDescendants().OfType<Button>())
        {
            ConstructToolTip(button);
            if (button.HotKey is not null)
                _buttonHotKeys[button] = button.HotKey;
        }

        _keyBindings.AddRange(KeyBindings);

        if (AsterSettings.DisplayMode is DisplayMode.IconView)
            IconView();
        else
            ListView();
    }

    private static void ConstructToolTip(Button button)
    {
        KeyGesture? gesture = button.HotKey;
        if (gesture is null)
            return;

        object? tooltip = ToolTip.GetTip(button);
        object? content = button.Content;

        switch (tooltip)
        {
            case not null:
                ToolTip.SetTip(button, $"{tooltip} ({gesture})");
                break;
            case null when content is not null:
                ToolTip.SetTip(button, $"{content} ({gesture})");
                break;
            case null:
                ToolTip.SetTip(button, gesture.ToString());
                break;
        }
    }
    
    private void GoBackTapped(object? sender, SelectionChangedEventArgs e)
    {
        GoBackButton.ContextFlyout?.Hide();
        GoForwardButton.ContextFlyout?.Hide();
        if (e.AddedItems is [LocationDisplay location])
            ExecuteCommand(_viewModel?.GoBackCommand, location);
    }
    
    private void GoForwardTapped(object? sender, SelectionChangedEventArgs e)
    {
        GoBackButton.ContextFlyout?.Hide();
        GoForwardButton.ContextFlyout?.Hide();
        if (e.AddedItems is [LocationDisplay location])
            ExecuteCommand(_viewModel?.GoForwardCommand, location);
    }

    private void FilesDoubleTapped(object sender, TappedEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            Visual? hitElement = (Visual?)listBox.InputHitTest(e.GetPosition(listBox));
            while (hitElement is not null)
            {
                if (hitElement is CheckBox)
                    return;

                hitElement = hitElement.GetVisualParent();
            }

            if (listBox.SelectedItem is FileSystemEntryViewModel entry)
                ExecuteCommand(_viewModel?.OpenEntryCommand, entry);
            else
                ExecuteCommand(_viewModel?.GoUpCommand);
        }
    }


    private void MoveUpQuickAccess(object sender, RoutedEventArgs e) =>
        ListBoxHelper.MoveUp(QuickAccessListBox, _viewModel!.QuickAccess);
    
    private void MoveDownQuickAccess(object sender, RoutedEventArgs e) =>
        ListBoxHelper.MoveDown(QuickAccessListBox, _viewModel!.QuickAccess);

    private void RemoveFromQuickAccess(object sender, RoutedEventArgs e) =>
        ListBoxHelper.Remove(QuickAccessListBox, _viewModel!.QuickAccess);

    private void FilesTapped(object sender, TappedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;

        Visual? hitElement = (Visual?)listBox.InputHitTest(e.GetPosition(listBox));
        while (hitElement is not null)
        {
            if (hitElement is ListBoxItem)
                return;

            hitElement = hitElement.GetVisualParent();
        }
        _viewModel?.SelectedFiles.Clear();
        NewNameBar.IsVisible = false;
    }

    private void SideBarEntryClicked(object sender, TappedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: SideBarEntryViewModel entry })
            ExecuteCommand(_viewModel?.OpenSideBarEntryCommand, entry);

        ClearFocus();
    }

    private void MouseButtonPressed(object sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsXButton1Pressed)
            ExecuteCommand(_viewModel?.GoBackCommand);

        if (properties.IsXButton2Pressed)
            ExecuteCommand(_viewModel?.GoForwardCommand);
    }

    private void OnRenameClicked(object sender, RoutedEventArgs e)
    {
        if (
            _viewModel is null
            || _viewModel.SelectedFiles.Count == 0
            || FileDisplay.SelectedItems is not IList list
            || list.Count == 0
            || list[^1] is not FileSystemEntryViewModel entry
        )
            return;

        NewNameBar.IsVisible = true;
        NameInputBox.Focus();
        NameInputBox.Text = entry.Name;

        NameInputBox.SelectionStart = 0;
        NameInputBox.SelectionEnd = entry.FileSystemEntry.NameWoExtension.Length;
    }

    private void InputBarLostFocus(object sender, RoutedEventArgs e) =>
        Dispatcher.UIThread.Post(() =>
        {
            for (
                Visual? current = FocusManager?.GetFocusedElement() as Visual;
                current is not null;
                current = current.GetVisualParent()
            )
                if (ReferenceEquals(current, sender))
                    return;

            NewNameBar.IsVisible = false;
        });

    private void PathBoxLostFocus(object sender, RoutedEventArgs e) =>
        PathBox.Text = _viewModel?.PathBoxText ?? string.Empty;

    private void NameEntered(object? sender = null, RoutedEventArgs? e = null)
    {
        if (NameInputBox.Text is string newName)
        {
            ExecuteCommand(_viewModel?.RenameCommand, newName);
            NewNameBar.IsVisible = false;
            NameInputBox.Text = string.Empty;
        }
    }
    

    private void ListView(object? sender = null, RoutedEventArgs? e = null)
    {
        FileDisplay.ItemsPanel = _previousPanel = _listViewPanel;
        FileDisplay.ItemTemplate = _previousTemplate = _listViewTemplate;
        AsterSettings.DisplayMode = DisplayMode.ListView;
    }
    
    private void IconView(object? sender = null, RoutedEventArgs? e = null)
    {
        FileDisplay.ItemsPanel = _previousPanel = _iconViewPanel;
        FileDisplay.ItemTemplate = _previousTemplate = _iconViewTemplate;
        AsterSettings.DisplayMode = DisplayMode.IconView;
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new();
        settingsWindow.ShowDialog(this);
    }

    private void SuppressHotKeys()
    {
        KeyBindings.Clear();
        foreach (Button button in _buttonHotKeys.Keys)
            button.HotKey = null;
    }
    
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.Source is TextBox)
            SuppressHotKeys();
    }

    private void RestoreHotKeys()
    {
        KeyBindings.AddRange(_keyBindings);
        foreach ((Button button, KeyGesture gesture) in _buttonHotKeys)
            button.HotKey = gesture;
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (e.Source is TextBox)
            RestoreHotKeys();
    }

    private void TunnelKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e)
        {
            case { Key: Key.Enter }:
                OnEnterPressed(e);
                return;
            case { Key: Key.Escape }:
                OnEscapePressed(e);
                return;
            case { KeyModifiers: KeyModifiers.Control, Key: Key.F }:
                OnCtrlFPressed(e);
                return;
        }

        if (
            e.Key is not (Key.Up or Key.Down or Key.Left or Key.Right)
            || FileDisplay.IsKeyboardFocusWithin
            || FocusManager?.GetFocusedElement() is TextBox
        )
            return;

        if (FileDisplay.ItemCount > 0)
        {
            FileDisplay.SelectedIndex = 0;
            FileDisplay.ContainerFromIndex(0)?.Focus();
        }
        e.Handled = true;
    }

    private void OnCtrlFPressed(KeyEventArgs e)
    {
        e.Handled = true;
        if (SearchBox.IsFocused)
            ClearFocus();
        else
            SearchBox.Focus();
    }

    private void ClearFocus()
    {
        FocusSink.Focus();
        SpecialsListBox.SelectedItems?.Clear();
        QuickAccessListBox.SelectedItems?.Clear();
        DrivesListBox.SelectedItems?.Clear();
    }

    private void OnEnterPressed(KeyEventArgs e)
    {
        e.Handled = true;
        if (NewNameBar.IsVisible)
        {
            if (NewNameBar.IsVisible)
                NameEntered();

            return;
        }

        if (PathBox.IsFocused)
        {
            if (PathBox.Text is string path)
                ExecuteCommand(_viewModel?.SetNewLocationCommand, path);
            ClearFocus();
            return;
        }

        if (_viewModel is not null)
        {
            if (SearchBox.IsFocused && !string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                ExecuteCommand(_viewModel.SearchCommand, SearchBox.Text);
                ClearFocus();
            }
            else if (_viewModel.SelectedFiles.Count == 1)
                ExecuteCommand(_viewModel.OpenEntryCommand, _viewModel.SelectedFiles[0]);
            else if (_viewModel.SelectedFiles.Count > 1)
                ExecuteCommand(_viewModel.OpenEntriesCommand);
        }
    }

    private void OnEscapePressed(KeyEventArgs e)
    {
        e.Handled = true;
        _viewModel?.SelectedFiles.Clear();
        ClearFocus();
    }

    private static void ExecuteCommand(ICommand? command, object? parameter = null)
    {
        if (command?.CanExecute(parameter) == true)
            command.Execute(parameter);
    }
    
    private void OnClosing(object? sender, WindowClosingEventArgs? e)
    {
        _viewModel?.Dispose();
        AsterSettings.SaveSettings();
    }
}
