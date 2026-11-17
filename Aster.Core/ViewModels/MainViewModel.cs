using System;
using System.Collections.ObjectModel;
using System.IO;
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
    public ReactiveCommand<RxVoid, RxVoid> GoHomeCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoRootCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoDesktopCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoDocumentsCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoDownloadsCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoPicturesCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoMusicCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> GoVideosCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> NavigatePathCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> OpenSelectedCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> ToggleViewCommand { get; }

    public bool IsGridView
    {
        get => _isGridView;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isGridView, value);
            this.RaisePropertyChanged(nameof(IsListView));
        }
    }
    private bool _isGridView;

    public bool IsListView => !IsGridView;

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
        GoHomeCommand = ReactiveCommand.Create(GoHome);
        GoRootCommand = ReactiveCommand.Create(GoRoot);
        GoDesktopCommand = ReactiveCommand.Create(GoDesktop);
        GoDocumentsCommand = ReactiveCommand.Create(GoDocuments);
        GoDownloadsCommand = ReactiveCommand.Create(GoDownloads);
        GoPicturesCommand = ReactiveCommand.Create(GoPictures);
        GoMusicCommand = ReactiveCommand.Create(GoMusic);
        GoVideosCommand = ReactiveCommand.Create(GoVideos);
        NavigatePathCommand = ReactiveCommand.Create(NavigatePath);
        OpenSelectedCommand = ReactiveCommand.Create(OpenSelectedEntry);
        ToggleViewCommand = ReactiveCommand.Create(ToggleView);

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
            Entries.Add(new FileSystemEntryViewModel(directory, _fileSystem!.IconProvider));
        foreach (FileEntryInfo file in result.Value.Files)
            Entries.Add(new FileSystemEntryViewModel(file, _fileSystem!.IconProvider));
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

    private void GoHome()
    {
        if (_bootstrap is not null)
            NavigateTo(_bootstrap.GetDefaultRootPath());
    }

    private void GoRoot()
    {
        if (_fileSystem is not null)
            NavigateTo(_fileSystem.FileInfoProvider.GetRoot());
    }

    private void GoDesktop() => GoSpecialFolder(Environment.SpecialFolder.Desktop);
    private void GoDocuments() => GoSpecialFolder(Environment.SpecialFolder.MyDocuments);
    private void GoDownloads() => NavigateTo(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
    private void GoPictures() => GoSpecialFolder(Environment.SpecialFolder.MyPictures);
    private void GoMusic() => GoSpecialFolder(Environment.SpecialFolder.MyMusic);
    private void GoVideos() => GoSpecialFolder(Environment.SpecialFolder.MyVideos);

    private void GoSpecialFolder(Environment.SpecialFolder folder)
    {
        string path = Environment.GetFolderPath(folder);
        if (!string.IsNullOrWhiteSpace(path))
            NavigateTo(path);
    }

    private void NavigatePath()
    {
        NavigateTo(CurrentPath);
    }

    public void OpenSelectedEntry()
    {
        if (SelectedEntry?.IsDirectory == true)
            NavigateTo(SelectedEntry.PathToEntry);
    }

    private void ToggleView()
    {
        IsGridView = !IsGridView;
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