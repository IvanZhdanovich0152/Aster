using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Aster.Core.Models;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Aster.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IPlatformBootstrap _bootstrap;
    private readonly IFileSystem _fileSystem;
    private string _currentPath =  string.Empty;
    
    public ObservableCollection<IFileSystemEntry> Entries { get; } = new();

    public ReactiveCommand<RxVoid, RxVoid> RefreshCommand { get; }
    
    public string CurrentPath
    {
        get => _currentPath;
        set => this.RaiseAndSetIfChanged(ref _currentPath, value);
    }
        

    public MainViewModel()
    {
        _bootstrap = App.Bootstrap;
        _fileSystem = _bootstrap.CreateFileSystem();

        RefreshCommand = ReactiveCommand.Create(LoadEntries);

        CurrentPath = _bootstrap.GetDefaultRootPath();
        LoadEntries();
    }

    private void LoadEntries()
    {
        Entries.Clear();
        foreach (var entry in _fileSystem.GetEntries(CurrentPath))
        {
            Entries.Add(entry);
        }
        Debug.WriteLine("Loaded " + Entries.Count);
    }
}