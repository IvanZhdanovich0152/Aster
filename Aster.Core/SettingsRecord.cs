using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aster.Core;

public interface IDefaultSettingsProvider
{
    public void PopulateDefaults(SettingsRecord settingsRecord);
}

[
    JsonObjectCreationHandling(JsonObjectCreationHandling.Populate),
    SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Json naming convention"),
    SuppressMessage(
        "ReSharper",
        "PropertyCanBeMadeInitOnly.Global",
        Justification = $"Values are set in {nameof(IDefaultSettingsProvider)}."
    ),
]
public record SettingsRecord
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        AllowTrailingCommas = true,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
    };
    private static readonly string RootDir;
    private static IDefaultSettingsProvider? DefaultSettingsProvider;

    public string newImageName { get; set; } = "New Image";
    public string newTextFileName { get; set; } = "New Text File";
    public string newFileName { get; set; } = "New File";
    public string newDirectoryName { get; set; } = "New Folder";
    public string notepadApp { get; set; } = string.Empty;
    public string notepadAppArgs { get; set; } = string.Empty;
    public string terminal { get; set; } = string.Empty;
    public string terminalArgs { get; set; } = string.Empty;
    public bool openInLastLocation { get; set; } = true;
    public string openIn { get; set; } = RootDir;
    public bool useDarkMode { get; set; } = true;
    public string displayMode { get; set; } = nameof(DisplayMode.ListView);
    public string defaultSort { get; set; } = nameof(SortBy.Name);
    public int fileSizeUnitLimit { get; set; } = 4096;
    public bool sortReversed { get; set; } = false;
    public bool showSpecialFolders { get; set; } = true;
    public bool showProtectedFiles { get; set; } = false;
    public bool showHiddenFiles { get; set; } = true;
    public bool treatDotFilesAsHidden { get; set; } = true;
    public bool showUndoRedoErrorDialogs { get; set; } = true;
    public bool automaticRefresh { get; set; } = true;
    public int automaticRefreshInterval { get; set; } = 3000;
    public int synchronizerPollingInterval { get; set; } = 3000;
    public List<string> quickAccess { get; set; } = new();
    public bool syncHiddenFiles { get; set; } = false;

    static SettingsRecord()
    {
        try
        {
            string? rootDir = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            RootDir = rootDir ?? string.Empty;
        }
        catch // Неподдерживаемая ОС
        {
            RootDir = string.Empty;
        }
    }

    public SettingsRecord()
    {
        if (DefaultSettingsProvider is null)
            throw new InvalidDataException("Settings need to be initialized during startup.");

        DefaultSettingsProvider.PopulateDefaults(this);
    }

    public static void Initialize(IDefaultSettingsProvider defaultSettingsProvider) =>
        DefaultSettingsProvider = defaultSettingsProvider;
}
