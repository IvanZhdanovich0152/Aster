using System;
using System.Collections.ObjectModel;
using Aster.Core.Models;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Aster.Core.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IPlatformBootstrap? _bootstrap;
    private readonly IFileSystem? _fileSystem;
    private string _currentPath = string.Empty;

    public ObservableCollection<FileSystemEntryViewModel> Entries { get; } = new();

    public ReactiveCommand<RxVoid, RxVoid> RefreshCommand { get; }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public string CurrentPath
    {
        get => _currentPath;
        private set => this.RaiseAndSetIfChanged(ref _currentPath, value);
    }

    public MainViewModel()
    {
        _bootstrap = App.Bootstrap;
        _fileSystem = _bootstrap?.CreateFileSystem();
        RefreshCommand = ReactiveCommand.Create(LoadEntries);

        if (_bootstrap is null || _fileSystem is null)
            return;

        CurrentPath = _bootstrap.GetDefaultRootPath();
        LoadEntries();
    }

    private void LoadEntries()
    {
        if (_fileSystem is null || string.IsNullOrWhiteSpace(CurrentPath))
            return;

        ErrorMessage = null;
        ValueResult<DirectoryContents> result = _fileSystem.FileInfoProvider.GetPathEntries(
            CurrentPath,
            includeOs: true,
            includeHidden: false);

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

}