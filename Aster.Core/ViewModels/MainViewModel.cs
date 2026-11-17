using System;
using System.Collections.ObjectModel;
using Aster.Core.Models;
using Aster.Core.Services;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Aster.Core.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IPlatformBootstrap? _bootstrap;
    private readonly IFileSystem? _fileSystem;
    private readonly IDirectoryNavigationService? _navigationService;
    private string _currentPath = string.Empty;

    public ObservableCollection<FileSystemEntryViewModel> Entries { get; } = new();

    public ReactiveCommand<RxVoid, RxVoid> RefreshCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoUpCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> OpenSelectedCommand { get; }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public string CurrentPath
    {
        get => _currentPath;
        set => this.RaiseAndSetIfChanged(ref _currentPath, value);
    }

    public FileSystemEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => this.RaiseAndSetIfChanged(ref _selectedEntry, value);
    }
    private FileSystemEntryViewModel? _selectedEntry;

    public MainViewModel()
    {
        _bootstrap = App.Bootstrap;
        _fileSystem = _bootstrap?.CreateFileSystem();
        _navigationService = _fileSystem is null
            ? null
            : new DirectoryNavigationService(_fileSystem);
        RefreshCommand = ReactiveCommand.Create(LoadEntries);
        GoUpCommand = ReactiveCommand.Create(GoUp);
        OpenSelectedCommand = ReactiveCommand.Create(OpenSelectedEntry);

        if (_bootstrap is null || _fileSystem is null)
            return;

        CurrentPath = _bootstrap.GetDefaultRootPath();
        LoadEntries();
    }

    private void LoadEntries()
    {
        if (_navigationService is null)
            return;

        ErrorMessage = null;
        ValueResult<DirectoryContents> result = _navigationService.Load(
            CurrentPath,
            includeHidden: false,
            includeOs: true);

        if (!result.IsOk)
        {
            ErrorMessage = string.Join(Environment.NewLine, result.Errors);
            return;
        }

        Entries.Clear();
        foreach (DirectoryEntryInfo directory in result.Value.Dirs)
            Entries.Add(new FileSystemEntryViewModel(directory));
        foreach (FileEntryInfo file in result.Value.Files)
            Entries.Add(new FileSystemEntryViewModel(file));
    }

    private void GoUp()
    {
        if (_fileSystem is null ||
            string.IsNullOrWhiteSpace(CurrentPath) ||
            _fileSystem.PathTools.IsRoot(CurrentPath))
            return;

        string parentPath = _fileSystem.PathTools.GetParentDir(CurrentPath);
        if (!string.IsNullOrWhiteSpace(parentPath))
            NavigateTo(parentPath);
    }

    private void OpenSelectedEntry()
    {
        if (SelectedEntry?.IsDirectory == true)
            NavigateTo(SelectedEntry.PathToEntry);
    }

    private void NavigateTo(string path)
    {
        if (_fileSystem is null)
            return;

        try
        {
            CurrentPath = _fileSystem.PathTools.GetFullPath(path);
            LoadEntries();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

}