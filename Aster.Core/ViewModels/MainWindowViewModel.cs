using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Aster.Core.Extensions;
using Aster.Core.Models;
using Aster.Core.Models.FileInformation;
using Aster.Core.Services.Dialogs;
using Aster.Core.Services.FileOperations;
using Aster.Core.Services.FileOperations.Undoable;
using Aster.Core.ViewModels;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Aster.Core.ViewModels;

public record struct SortInfo(SortBy SortBy, bool SortReversed);

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record struct RepoStateInfo(string CommitsToPull, string CommitsToPush);

public record LocationDisplay
{
    private readonly Location _location;

    public string Label { get; init; }

    public string Path { get; init; }

    public LocationDisplay(Location location)
    {
        _location = location;
        Label = location.FileSystem.GetLabel();
        Path = location.Path;
    }

    public Location GetLocation() => _location;

    public virtual bool Equals(LocationDisplay? other) =>
        ReferenceEquals(_location, other?._location);

    public override int GetHashCode() => HashCode.Combine(Label, Path);
}

#pragma warning disable CA1822 // Member can be marked as static.

public sealed partial class MainWindowViewModel : ReactiveObject, IDisposable
{
    private const string EmptyDirMessage = "This directory is empty.";
    private const string EmptySearchMessage = "No items match your query";
    private const string NoRemoteMark = "-";
    private const string ArchiveName = "Archive";

    private readonly UndoRedoHandler<IUndoableFileOperation> _undoRedoHistory;
    private readonly UndoRedoHandler<Location> _locationHistory;
    private readonly SearchManager _searchManager;
    private readonly LocalFileSystem _localFs;
    private readonly IDialogService _dialogService;
    private readonly Action<bool> _setDarkMode;

    private readonly List<string> _localPathsOrdered = new();
    private readonly HashSet<string> _localPaths = new();
    
    public required IClipboardManager ClipboardManager { private get; init; }

    private DateTime _lastRefreshedUtc;
    private DispatcherTimer? _refreshTimer;

    public SortInfo SortInfo =>
        new(AsterSettings.SortingMode, AsterSettings.SortReversed);

    public IEnumerable<LocationDisplay> LocationsBack =>
        _locationHistory.EnumerateFromCurrentBack().Select(l => new LocationDisplay(l));

    public IEnumerable<LocationDisplay> LocationsForward =>
        _locationHistory.EnumerateFromCurrentForward().Select(l => new LocationDisplay(l));

    public ObservableCollection<FileSystemEntryViewModel> FileEntries { get; } = new();

    public ObservableCollection<FileSystemEntryViewModel> SelectedFiles { get; } = new();

    public ObservableCollection<SideBarEntryViewModel> QuickAccess { get; } = new();

    public ObservableCollection<SideBarEntryViewModel> SpecialFolders { get; } = [];

    public ObservableCollection<SideBarEntryViewModel> Drives { get; } = [];
    
    public string CurrentFsLabel
    {
        get => _currentFsLabel;
        set => this.RaiseAndSetIfChanged(ref _currentFsLabel, value);
    }
    private string _currentFsLabel = string.Empty;

    public string PathBoxText
    {
        get => _pathBoxText;
        set => this.RaiseAndSetIfChanged(ref _pathBoxText, value);
    }
    private string _pathBoxText = string.Empty;

    private Location CurrentLocation
    {
        get =>
            _currentLocation
            ?? throw new InvalidOperationException("Location was not initialized.");
        set
        {
            _currentLocation = value;
            PathBoxText = value.Path;
            CurrentFsLabel = value.FileSystem.GetLabel();
            if (AsterSettings.OpenInLastLocation && value.FileSystem.IsLocal())
                AsterSettings.OpenIn = value.Path;

            if (value.FileSystem.IsLocal() && _localPaths.Add(value.Path))
                _localPathsOrdered.Add(value.Path);

            SetSearchWaterMark(value);
            this.RaisePropertyChanged(nameof(IsLocal));
        }
    }
    private Location? _currentLocation;

    public string? CurrentInfoMessage
    {
        get => _currentInfoMessage;
        set => this.RaiseAndSetIfChanged(ref _currentInfoMessage, value);
    }
    private string? _currentInfoMessage = null;

    public string SearchWaterMark
    {
        get => _searchWaterMark;
        set => this.RaiseAndSetIfChanged(ref _searchWaterMark, value);
    }
    private string _searchWaterMark = string.Empty;

    public string SelectionInfo
    {
        get => _selectionInfo;
        set => this.RaiseAndSetIfChanged(ref _selectionInfo, value);
    }
    private string _selectionInfo = string.Empty;

    public bool Searching
    {
        get => _searching;
        set
        {
            this.RaiseAndSetIfChanged(ref _searching, value);
            this.RaisePropertyChanged(nameof(NotSearching));
        }
    }
    private bool _searching;

    public string CurrentQuery
    {
        get => _currentQuery;
        set => this.RaiseAndSetIfChanged(ref _currentQuery, value);
    }
    private string _currentQuery = string.Empty;
    
    public ReactiveCommand<RxVoid, RxVoid> OpenEntriesCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel, RxVoid> OpenEntryCommand { get; }

    public ReactiveCommand<SideBarEntryViewModel, RxVoid> OpenSideBarEntryCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel, RxVoid> OpenAsCommand { get; }

    public ReactiveCommand<string, RxVoid> SetNewLocationCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> OpenInNotepadCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel?, RxVoid> AddToQuickAccessCommand { get; }

    public ReactiveCommand<RxVoid, Task> AddToArchiveCommand { get; }

    public ReactiveCommand<RxVoid, Task> ExtractArchiveCommand { get; }

    public ReactiveCommand<LocationDisplay?, RxVoid> GoBackCommand { get; }

    public ReactiveCommand<LocationDisplay?, RxVoid> GoForwardCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> GoUpCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> ReloadCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> OpenTerminalCommand { get; }

    public ReactiveCommand<string, Task> SearchCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> CancelSearchCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> NewFileCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> NewDirCommand { get; }

    public ReactiveCommand<string, RxVoid> RenameCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> CutCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> CopyCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel?, Task> CopyPathCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel, RxVoid> CreateShortcutCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel, Task> FlattenFolderCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel, RxVoid> ShowPropertiesCommand { get; }

    public ReactiveCommand<FileSystemEntryViewModel?, Task> SyncDirCommand { get; }

    public ReactiveCommand<RxVoid, Task> PasteCommand { get; }

    public ReactiveCommand<RxVoid, Task> MoveToTrashCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> DeleteCommand { get; }

    public ReactiveCommand<SortBy, RxVoid> SetSortByCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> UndoCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> RedoCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> SelectAllCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> SelectNoneCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> InvertSelectionCommand { get; }

    public bool SelectionNotEmpty => SelectedFiles.Count > 0;

    public bool CanGoBack => _locationHistory.GetPrevious() is not null;

    public bool CanGoForward => _locationHistory.GetNext() is not null;

    public bool IsLocal => CurrentLocation.FileSystem.IsLocal();

    public bool NotSearching => !Searching;

    public MainWindowViewModel(
        string initialDir,
        LocalFileSystem localFs,
        IDialogService dialogService,
        Action<bool> setDarkMode
    )
    {
        _localFs = localFs;
        CurrentLocation = new Location(_localFs, _localFs.LocalFileInfoProvider.GetRoot()); // Placeholder
        _dialogService = dialogService;
        _setDarkMode = setDarkMode;

        _searchManager = new SearchManager(s => PathBoxText = s, entry => FileEntries.Add(entry));
        _undoRedoHistory = new UndoRedoHandler<IUndoableFileOperation>();
        _locationHistory = new UndoRedoHandler<Location>(() =>
        {
            this.RaisePropertyChanged(nameof(CanGoBack));
            this.RaisePropertyChanged(nameof(CanGoForward));
            this.RaisePropertyChanged(nameof(LocationsBack));
            this.RaisePropertyChanged(nameof(LocationsForward));
        });

        IObservable<bool> canGoForward = this.WhenAnyValue(x => x.CanGoForward);
        IObservable<bool> canGoBack = this.WhenAnyValue(x => x.CanGoBack);
        IObservable<bool> local = this.WhenAnyValue(x => x.IsLocal);
        IObservable<bool> notSearching = this.WhenAnyValue(x => x.NotSearching);
        IObservable<bool> selection = this.WhenAnyValue(x => x.SelectionNotEmpty);
        IObservable<bool> localNotSearching = local.CombineLatest(notSearching, (l, nS) => l && nS);
        IObservable<bool> localSelection = local.CombineLatest(selection, (l, sl) => l && sl);
        IObservable<bool> notLocalNotSearching = local.CombineLatest(
            notSearching,
            (loc, nSearch) => !loc && nSearch
        );

        OpenEntriesCommand = ReactiveCommand.CreateFromTask(
            () => SelectedFiles.Count == 1 ? OpenEntryAsync(SelectedFiles[0]) : OpenEntries(),
            local
        );
        OpenEntryCommand = ReactiveCommand.CreateFromTask<FileSystemEntryViewModel>(OpenEntryAsync);
        OpenSideBarEntryCommand = ReactiveCommand.CreateFromTask<SideBarEntryViewModel>(
            OpenSideBarEntryAsync
        );
        OpenInNotepadCommand = ReactiveCommand.Create(OpenInNotepad, local);
        OpenAsCommand = ReactiveCommand.Create<FileSystemEntryViewModel>(OpenAs, local);
        SetNewLocationCommand = ReactiveCommand.CreateFromTask<string>(SetNewLocationAsync);
        AddToQuickAccessCommand = ReactiveCommand.Create<FileSystemEntryViewModel?>(
            AddToQuickAccess,
            local
        );
        AddToArchiveCommand = ReactiveCommand.Create(AddToArchiveAsync, localNotSearching);
        ExtractArchiveCommand = ReactiveCommand.Create(ExtractArchiveAsync, localNotSearching);
        GoBackCommand = ReactiveCommand.CreateFromTask<LocationDisplay?>(
            l => l is null ? GoBackAsync() : GoBackToLocationAsync(l),
            canGoBack
        );
        GoForwardCommand = ReactiveCommand.CreateFromTask<LocationDisplay?>(
            l => l is null ? GoForwardAsync() : GoForwardToLocationAsync(l),
            canGoForward
        );
        GoUpCommand = ReactiveCommand.CreateFromTask(GoUpAsync);
        ReloadCommand = ReactiveCommand.Create(HardReload, notSearching);
        OpenTerminalCommand = ReactiveCommand.Create(OpenTerminal, localNotSearching);
        SearchCommand = ReactiveCommand.Create<string, Task>(SearchAsync);
        CancelSearchCommand = ReactiveCommand.CreateFromTask(CancelSearchAndGoBackAsync);
        NewFileCommand = ReactiveCommand.CreateFromTask(NewFileAsync, notSearching);
        NewDirCommand = ReactiveCommand.CreateFromTask(NewDirAsync, notSearching);
        RenameCommand = ReactiveCommand.CreateFromTask<string>(RenameAsync, selection);
        CutCommand = ReactiveCommand.CreateFromTask(CutAsync, selection);
        CopyCommand = ReactiveCommand.CreateFromTask(CopyAsync, selection);
        CopyPathCommand = ReactiveCommand.Create<FileSystemEntryViewModel?, Task>(CopyPathAsync);
        CreateShortcutCommand = ReactiveCommand.Create<FileSystemEntryViewModel>(CreateShortcut);
        FlattenFolderCommand = ReactiveCommand.Create<FileSystemEntryViewModel, Task>(
            FlattenFolderAsync
        );
        ShowPropertiesCommand = ReactiveCommand.Create<FileSystemEntryViewModel>(ShowProperties);
        
        PasteCommand = ReactiveCommand.Create(PasteAsync, notSearching);
        MoveToTrashCommand = ReactiveCommand.Create(MoveToTrashAsync, localSelection);
        DeleteCommand = ReactiveCommand.CreateFromTask(Delete, selection);
        SetSortByCommand = ReactiveCommand.CreateFromTask<SortBy>(SetSortBy);
        UndoCommand = ReactiveCommand.CreateFromTask(UndoAsync);
        RedoCommand = ReactiveCommand.CreateFromTask(RedoAsync);
        SelectAllCommand = ReactiveCommand.Create(SelectAll);
        SelectNoneCommand = ReactiveCommand.Create(SelectNone);
        InvertSelectionCommand = ReactiveCommand.Create(InvertSelection);

        LoadQuickAccess();
        LoadDrives();
        LoadSettings(false);
        SetInitialLocation(initialDir);
        AsterSettings.OnSettingsChange = () => LoadSettings(true);
        SelectedFiles.CollectionChanged += UpdateSelectionInfo;
        FileEntries.CollectionChanged += UpdateSelectionInfo;
    }

    private void LoadQuickAccess()
    {
        foreach (string path in AsterSettings.QuickAccess)
        {
            IFileSystemEntry? entry = null;

            ExistsInfo exists = _localFs.LocalFileInfoProvider.Exists(path);
            if (exists.AsDir)
                entry = new DirectoryEntry(path, LocalPathTools.Instance);
            else if (exists.AsFile)
                entry = new FileEntry(path, LocalPathTools.Instance);

            if (entry is not null)
                QuickAccess.Add(new SideBarEntryViewModel(_localFs, entry));
        }
    }

    private void LoadDrives()
    {
        Drives.Clear();
        foreach (DriveEntryInfo driveEntry in _localFs.LocalFileInfoProvider.GetDrives())
            Drives.Add(new SideBarEntryViewModel(_localFs, driveEntry));
    }

    private void SetInitialLocation(string localPath)
    {
        Location requestedLocation = new(_localFs, localPath);
        Task t = Task.CompletedTask;
        if (!requestedLocation.Exists())
        {
            Location root = new(_localFs, _localFs.LocalFileInfoProvider.GetRoot());
            t = SetLocationAsync(root);
        }
        t.ContinueWith(
            _ => SetLocationAsync(requestedLocation),
            TaskScheduler.FromCurrentSynchronizationContext()
        );
    }

    private void LoadSettings(bool reload)
    {
        if (!AsterSettings.ShowSpecialFolders)
            SpecialFolders.Clear();
        else if (SpecialFolders.Count == 0)
            foreach (SideBarEntryViewModel entry in GetSpecialFolders())
                SpecialFolders.Add(entry);

        if (!AsterSettings.AutomaticRefresh)
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
        }
        else if (_refreshTimer is null)
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(AsterSettings.AutomaticRefreshInterval),
            };
            _lastRefreshedUtc = DateTime.UtcNow;
            _refreshTimer.Tick += (_, _) => _ = CheckForUpdatesAsync();
            _refreshTimer.Start();
        }

        _setDarkMode(AsterSettings.UseDarkMode);
        if (reload)
            HardReload();
    }

    private IEnumerable<SideBarEntryViewModel> GetSpecialFolders() =>
        _localFs
            .LocalFileInfoProvider.GetSpecialFolders()
            .Where(dirPath => !string.IsNullOrEmpty(dirPath))
            .Select(path => new SideBarEntryViewModel(
                _localFs,
                new DirectoryEntry(path, LocalPathTools.Instance)
            ));

    private void UpdateSelectionInfo(object? sender, NotifyCollectionChangedEventArgs? args)
    {
        string selectionInfo = FileEntries.Count == 1 ? "1 item" : $"{FileEntries.Count} items";

        if (SelectedFiles.Count == 1)
            selectionInfo += "  |  1 item selected";
        else if (SelectedFiles.Count > 1)
            selectionInfo += $"  |  {SelectedFiles.Count} items selected";

        bool displaySize = SelectedFiles.Count >= 1;
        long sizeSum = 0;
        foreach (FileSystemEntryViewModel entry in SelectedFiles)
        {
            if (entry.IsDirectory)
            {
                displaySize = false;
                break;
            }
            if (entry.SizeB is long sizeB)
                sizeSum += sizeB;
        }
        if (displaySize)
            selectionInfo += "  " + FileSystemEntryViewModel.GetSizeString(sizeSum);

        SelectionInfo = selectionInfo;
        this.RaisePropertyChanged(nameof(SelectionNotEmpty));
    }

    private void SetSearchWaterMark(Location location)
    {
        string dirName = location.PathTools().GetFileName(location.Path);
        if (string.IsNullOrEmpty(dirName))
            dirName = location.FileSystem.FileInfoProvider.GetRoot();

        SearchWaterMark = $"Search {dirName}";
    }

    private async Task CheckForUpdatesAsync()
    {
        Location current = CurrentLocation;
        if (await current.ExistsAsync())
        {
            if (await CompareSetLastWriteTimeAsync(current))
                HardReload();
            else 
                SoftReload();
        }
    }

    private async Task<bool> CompareSetLastWriteTimeAsync(Location current)
    {
        DateTime lastWriteTimeUtc =
            await current.FileSystem.FileInfoProvider.GetDirLastWriteUtcAsync(current.Path)
            ?? DateTime.UtcNow;

        if (_lastRefreshedUtc >= lastWriteTimeUtc)
            return false;

        _lastRefreshedUtc = lastWriteTimeUtc;
        return true;
    }

    private void SaveSettings()
    {
        AsterSettings.QuickAccess = QuickAccess.Select(entry => entry.PathToEntry).ToList();
    }

    public void Dispose()
    {
        SaveSettings();
        _refreshTimer?.Stop();

        _searchManager.Dispose();

        _localFs.Dispose();
    }
}
