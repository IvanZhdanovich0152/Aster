using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Aster.Core.ViewModels;

namespace Aster.Core;

public enum DisplayMode
{
    ListView, IconView,
}

public enum SortBy
{
    Name, Date, Type, Size,
}

[SuppressMessage(
    "ReSharper",
    "MemberCanBePrivate.Global",
    Justification = "Members represent app settings"
)]
public static class AsterSettings
{
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
    private static readonly string FileSurferDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Aster"
    );

    public static readonly string SettingsFilePath = Path.Combine(
        FileSurferDataDir,
        "settings.json"
    );

    public static Action? OnSettingsChange { private get; set; }

    public static SettingsRecord DefaultSettings => new();

    public static SettingsRecord CurrentSettings =>
        new()
        {
            newImageName = NewImageName,
            newTextFileName = NewTextFileName,
            newFileName = NewFileName,
            newDirectoryName = NewDirectoryName,
            notepadApp = NotepadApp,
            notepadAppArgs = NotepadAppArgs,
            terminal = Terminal,
            terminalArgs = TerminalArgs,
            openInLastLocation = OpenInLastLocation,
            openIn = OpenIn,
            useDarkMode = UseDarkMode,
            displayMode = DisplayMode.ToString(),
            defaultSort = SortingMode.ToString(),
            fileSizeUnitLimit = FileSizeUnitLimit,
            sortReversed = SortReversed,
            showSpecialFolders = ShowSpecialFolders,
            showProtectedFiles = ShowProtectedFiles,
            showHiddenFiles = ShowHiddenFiles,
            treatDotFilesAsHidden = TreatDotFilesAsHidden,
            showUndoRedoErrorDialogs = ShowUndoRedoErrorDialogs,
            automaticRefresh = AutomaticRefresh,
            automaticRefreshInterval = AutomaticRefreshInterval,
            synchronizerPollingInterval = SynchronizerPollingInterval,
            quickAccess = QuickAccess,
            syncHiddenFiles = SyncHiddenFiles,
        };

    private static string _previousSettingsJson = string.Empty;

    public static string NewImageName { get; set; }

    public static string NewTextFileName { get; set; }

    public static string NewFileName { get; set; }

    public static string NewDirectoryName { get; set; }

    public static string NotepadApp { get; set; }

    public static string NotepadAppArgs { get; set; }

    public static string Terminal { get; set; }

    public static string TerminalArgs { get; set; }

    public static bool OpenInLastLocation { get; set; }

    public static string OpenIn { get; set; }

    public static bool UseDarkMode { get; set; }

    public static DisplayMode DisplayMode { get; set; }

    public static SortBy SortingMode { get; set; }

    public static int FileSizeUnitLimit { get; set; }
    public const int FileSizeUnitLimitLowerBound = 512;
    public const int FileSizeUnitLimitUpperBound = 9999;

    public static bool SortReversed { get; set; }

    public static bool ShowSpecialFolders { get; set; }

    public static bool ShowProtectedFiles { get; set; }

    public static bool ShowHiddenFiles { get; set; }

    public static bool TreatDotFilesAsHidden { get; set; }


    public static bool ShowUndoRedoErrorDialogs { get; set; }

    public static bool AutomaticRefresh { get; set; }

    public static int AutomaticRefreshInterval { get; set; }
    public const int AutomaticRefreshIntervalLowerBound = 100;
    public const int AutomaticRefreshIntervalUpperBound = 60 * 1000;

    public static int SynchronizerPollingInterval { get; set; }
    public const int SynchronizerPollingIntervalLowerBound = 250;
    public const int SynchronizerPollingIntervalUpperBound = 5 * 60 * 1000;

    public static List<string> QuickAccess { get; set; }

    public static bool SyncHiddenFiles { get; set; }
    
    public static void Initialize(IDefaultSettingsProvider settingsProvider)
    {
       SettingsRecord.Initialize(settingsProvider);
        try
        {
            _previousSettingsJson = File.ReadAllText(SettingsFilePath, Encoding.UTF8);
            SettingsRecord settings =
                JsonSerializer.Deserialize<SettingsRecord>(
                    _previousSettingsJson,
                    SettingsRecord.SerializerOptions
                ) ?? throw new InvalidDataException();

            ImportSettings(settings);
        }
        catch
        {
            ImportSettings(DefaultSettings);
        }
    }

    public static void ImportSettings(SettingsRecord settings)
    {
        SettingsRecord defaultSettings = DefaultSettings;

        NewImageName = SanitizeName(
            settings.newImageName,
            InvalidFileNameChars,
            defaultSettings.newImageName
        );
        NewTextFileName = SanitizeName(
            settings.newTextFileName,
            InvalidFileNameChars,
            defaultSettings.newTextFileName
        );
        NewFileName = SanitizeName(
            settings.newFileName,
            InvalidFileNameChars,
            defaultSettings.newFileName
        );
        NewDirectoryName = SanitizeName(
            settings.newDirectoryName,
            InvalidFileNameChars,
            defaultSettings.newDirectoryName
        );
        NotepadApp = SanitizeName(
            settings.notepadApp,
            InvalidPathChars,
            defaultSettings.notepadApp
        );
        NotepadAppArgs = settings.notepadAppArgs.Trim();
        Terminal = SanitizeName(settings.terminal, InvalidPathChars, defaultSettings.terminal);
        TerminalArgs = settings.terminalArgs.Trim();

        OpenInLastLocation = settings.openInLastLocation;
        OpenIn = SanitizeName(settings.openIn, InvalidPathChars, defaultSettings.openIn);
        UseDarkMode = settings.useDarkMode;
        DisplayMode = SafeParseEnum<DisplayMode>(settings.displayMode);
        SortingMode = SafeParseEnum<SortBy>(settings.defaultSort);

        FileSizeUnitLimit = ClampValue(
            settings.fileSizeUnitLimit,
            FileSizeUnitLimitLowerBound,
            FileSizeUnitLimitUpperBound
        );

        SortReversed = settings.sortReversed;
        ShowSpecialFolders = settings.showSpecialFolders;
        ShowProtectedFiles = settings.showProtectedFiles;
        ShowHiddenFiles = settings.showHiddenFiles;
        TreatDotFilesAsHidden = settings.treatDotFilesAsHidden;
        ShowUndoRedoErrorDialogs = settings.showUndoRedoErrorDialogs;
        AutomaticRefresh = settings.automaticRefresh;

        AutomaticRefreshInterval = ClampValue(
            settings.automaticRefreshInterval,
            AutomaticRefreshIntervalLowerBound,
            AutomaticRefreshIntervalUpperBound
        );
        SynchronizerPollingInterval = ClampValue(
            settings.synchronizerPollingInterval,
            SynchronizerPollingIntervalLowerBound,
            SynchronizerPollingIntervalUpperBound
        );

        QuickAccess = settings.quickAccess ?? new List<string>();
        SyncHiddenFiles = settings.syncHiddenFiles;

        OnSettingsChange?.Invoke();
    }

    private static TEnum SafeParseEnum<TEnum>(string? enumValueName)
        where TEnum : struct, Enum =>
        enumValueName is not null && Enum.TryParse(enumValueName, true, out TEnum result)
            ? result
            : default;

    private static string SanitizeName(string? fileName, char[] invalidChars, string defaultName) =>
        !string.IsNullOrEmpty(fileName) && fileName.All(ch => !invalidChars.Contains(ch))
            ? fileName
            : defaultName;

    private static T ClampValue<T>(T value, T lowerBound, T upperBound)
        where T : IComparable<T>
    {
        if (value.CompareTo(lowerBound) < 0)
            return lowerBound;

        if (value.CompareTo(upperBound) > 0)
            return upperBound;

        return value;
    }

    public static void SaveSettings()
    {
        if (!Directory.Exists(FileSurferDataDir))
            Directory.CreateDirectory(FileSurferDataDir);

        SettingsRecord settings = CurrentSettings;
        string settingsJson = JsonSerializer.Serialize(settings, SettingsRecord.SerializerOptions);

        if (_previousSettingsJson != settingsJson)
            File.WriteAllText(SettingsFilePath, settingsJson, Encoding.UTF8);
    }
}
