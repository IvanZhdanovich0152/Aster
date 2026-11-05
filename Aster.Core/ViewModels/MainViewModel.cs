using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Aster.Core.Models;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Aster.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static IFileSystem _fileSystem;
    public ObservableCollection<string> Entries { get; set; } = new();

    public ReactiveCommand<RxVoid, RxVoid> RefreshCommand { get; }
    

    public MainViewModel()
    {
        LoadEntries();
        RefreshCommand = ReactiveCommand.Create(LoadEntries);
    }
    private void LoadEntries()
    {
        Entries.Clear();
        _fileSystem = App.Bootstrap!.CreateFileSystem();
        
        var result = _fileSystem.GetFiles("/");
        foreach (var name in result)
        {
            Entries.Add(name);
        }
        
        Debug.WriteLine("Loaded " + Entries.Count);
    }
}