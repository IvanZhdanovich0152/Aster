using Aster.Core;

namespace Aster.Windows;

public class WindowsDefaultSettingsProvider : IDefaultSettingsProvider
{
    public void PopulateDefaults(SettingsRecord settingsRecord)
    {
        settingsRecord.notepadApp = "notepad.exe";
        settingsRecord.notepadAppArgs = string.Empty;
        settingsRecord.terminal = "powershell";
        settingsRecord.terminalArgs = "-NoExit -Command Set-Location";
    }
}
