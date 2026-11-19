using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace Aster.Core.ViewModels;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class SettingsWindowViewModel : ReactiveObject
{
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public bool IsWindows { get; } = OperatingSystem.IsWindows();

    public string NewImageName
    {
        get => _newImageName;
        set =>
            this.RaiseAndSetIfChanged(
                ref _newImageName,
                SanitizeInput(value, InvalidFileNameChars)
            );
    }
    private string _newImageName;

    public string NewTextFileName
    {
        get => _newTextFileName;
        set =>
            this.RaiseAndSetIfChanged(
                ref _newTextFileName,
                SanitizeInput(value, InvalidFileNameChars)
            );
    }
    private string _newTextFileName;

    public string NewFileName
    {
        get => _newFileName;
        set =>
            this.RaiseAndSetIfChanged(ref _newFileName, SanitizeInput(value, InvalidFileNameChars));
    }
    private string _newFileName;

    public string NewDirectoryName
    {
        get => _newDirectoryName;
        set =>
            this.RaiseAndSetIfChanged(
                ref _newDirectoryName,
                SanitizeInput(value, InvalidFileNameChars)
            );
    }
    private string _newDirectoryName;

    public string NotepadApp
    {
        get => _notepadApp;
        set => this.RaiseAndSetIfChanged(ref _notepadApp, SanitizeInput(value, InvalidPathChars));
    }
    private string _notepadApp;

    public string NotepadAppArgs
    {
        get => _notepadAppArgs;
        set => this.RaiseAndSetIfChanged(ref _notepadAppArgs, value);
    }
    private string _notepadAppArgs;

    public string Terminal
    {
        get => _terminal;
        set => this.RaiseAndSetIfChanged(ref _terminal, SanitizeInput(value, InvalidPathChars));
    }
    private string _terminal;

    public string TerminalArgs
    {
        get => _terminalArgs;
        set => this.RaiseAndSetIfChanged(ref _terminalArgs, value);
    }
    private string _terminalArgs;

    public bool OpenInLastLocation
    {
        get => _openInLastLocation;
        set => this.RaiseAndSetIfChanged(ref _openInLastLocation, value);
    }
    private bool _openInLastLocation;

    public string OpenIn
    {
        get => _openIn;
        set => this.RaiseAndSetIfChanged(ref _openIn, SanitizeInput(value, InvalidPathChars));
    }
    private string _openIn;

    public bool UseDarkMode { get; set; }

    public string DisplayMode { get; set; }

    public IEnumerable<string> DisplayModeOptions { get; } =
        Enum.GetValues<DisplayMode>().Select(option => option.ToString());

    public string DefaultSort { get; set; }

    public IEnumerable<string> SortOptions { get; } =
        Enum.GetValues<SortBy>().Select(option => option.ToString());

    public int? FileSizeUnitLimit { get; set; }

    public int FileSizeUnitLimitLowerBound => AsterSettings.FileSizeUnitLimitLowerBound;

    public int FileSizeUnitLimitUpperBound => AsterSettings.FileSizeUnitLimitUpperBound;

    public bool SortReversed { get; set; }

    public bool ShowSpecialFolders { get; set; }

    public bool ShowProtectedFiles { get; set; }

    public bool ShowHiddenFiles { get; set; }

    public bool TreatDotFilesAsHidden { get; set; }

    public bool ShowUndoRedoErrorDialogs { get; set; }

    public bool AutomaticRefresh { get; set; }

    public int? AutomaticRefreshInterval { get; set; }

    public int AutomaticRefreshIntervalLowerBound =>
        AsterSettings.AutomaticRefreshIntervalLowerBound;

    public int AutomaticRefreshIntervalUpperBound =>
        AsterSettings.AutomaticRefreshIntervalUpperBound;

    public int? SynchronizerPollingInterval { get; set; }

    public int SynchronizerPollingIntervalLowerBound =>
        AsterSettings.SynchronizerPollingIntervalLowerBound;

    public int SynchronizerPollingIntervalUpperBound =>
        AsterSettings.SynchronizerPollingIntervalUpperBound;

    public SettingsWindowViewModel(SettingsRecord settings) => SetValues(settings);

    public SettingsWindowViewModel() => SetValues(AsterSettings.CurrentSettings);

    private void SetValues(SettingsRecord settings)
    {
        NewImageName = settings.newImageName;
        NewTextFileName = settings.newTextFileName;
        NewFileName = settings.newFileName;
        NewDirectoryName = settings.newDirectoryName;
        NotepadApp = settings.notepadApp;
        NotepadAppArgs = settings.notepadAppArgs;
        Terminal = settings.terminal;
        TerminalArgs = settings.terminalArgs;
        OpenInLastLocation = settings.openInLastLocation;
        OpenIn = settings.openIn;
        UseDarkMode = settings.useDarkMode;
        DisplayMode = settings.displayMode;
        DefaultSort = settings.defaultSort;
        FileSizeUnitLimit = settings.fileSizeUnitLimit;
        SortReversed = settings.sortReversed;
        ShowSpecialFolders = settings.showSpecialFolders;
        ShowProtectedFiles = settings.showProtectedFiles;
        ShowHiddenFiles = settings.showHiddenFiles;
        TreatDotFilesAsHidden = settings.treatDotFilesAsHidden;
        ShowUndoRedoErrorDialogs = settings.showUndoRedoErrorDialogs;
        AutomaticRefresh = settings.automaticRefresh;
        AutomaticRefreshInterval = settings.automaticRefreshInterval;
        SynchronizerPollingInterval = settings.synchronizerPollingInterval;
    }

    public SettingsRecord ExtractSettings() =>
        new()
        {
            newImageName = NewImageName.Trim(),
            newTextFileName = NewTextFileName.Trim(),
            newFileName = NewFileName.Trim(),
            newDirectoryName = NewDirectoryName.Trim(),
            notepadApp = NotepadApp.Trim(),
            notepadAppArgs = NotepadAppArgs.Trim(),
            terminal = Terminal.Trim(),
            terminalArgs = TerminalArgs.Trim(),
            openInLastLocation = OpenInLastLocation,
            openIn = OpenIn.Trim(),
            useDarkMode = UseDarkMode,
            displayMode = DisplayMode,
            defaultSort = DefaultSort,
            fileSizeUnitLimit = FileSizeUnitLimit ?? FileSizeUnitLimitLowerBound,
            sortReversed = SortReversed,
            showSpecialFolders = ShowSpecialFolders,
            showProtectedFiles = ShowProtectedFiles,
            showHiddenFiles = ShowHiddenFiles,
            treatDotFilesAsHidden = TreatDotFilesAsHidden,
            showUndoRedoErrorDialogs = ShowUndoRedoErrorDialogs,
            automaticRefresh = AutomaticRefresh,
            automaticRefreshInterval =
                AutomaticRefreshInterval ?? AutomaticRefreshIntervalLowerBound,
            synchronizerPollingInterval =
                SynchronizerPollingInterval ?? SynchronizerPollingIntervalLowerBound,
            quickAccess = AsterSettings.QuickAccess,
        };

    public void Save(SettingsRecord settings) => AsterSettings.ImportSettings(settings);

    public void ResetToDefault() => SetValues(new SettingsRecord());

    private static string SanitizeInput(string input, char[] invalidChars)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        StringBuilder sb = new(input.Length);
        foreach (char c in input.Where(ch => !invalidChars.Contains(ch)))
            sb.Append(c);

        return sb.ToString();
    }
}
