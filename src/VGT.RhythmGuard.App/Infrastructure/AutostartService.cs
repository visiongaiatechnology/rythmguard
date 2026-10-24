using System.Diagnostics;
using System.Security;
using Microsoft.Win32;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.App.Infrastructure;

/// <summary>Verwaltet den Autostart-Eintrag in HKCU (kein Admin-Recht nötig).</summary>
internal sealed class AutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "VGT.RhythmGuard";

    private readonly AppLogger _logger;

    public AutostartService(AppLogger logger)
    {
        _logger = logger;
    }

    public string ExecutablePath => Environment.ProcessPath ?? throw new RhythmStateException("Programmpfad ist nicht ermittelbar.");

    public string CommandLine => $"\"{ExecutablePath}\" --logon";

    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            var value = key?.GetValue(ValueName) as string;
            return string.Equals(value, CommandLine, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException)
        {
            _logger.Warn("Autostart-Status nicht lesbar: " + ex.Message);
            return false;
        }
    }

    public bool TrySetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                _logger.Warn("Autostart-Registryschlüssel konnte nicht geöffnet werden.");
                return false;
            }

            if (enabled)
            {
                key.SetValue(ValueName, CommandLine, RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            var result = IsEnabled() == enabled;
            if (!result)
            {
                _logger.Warn("Autostart konnte nicht gesetzt werden (Verifikation fehlgeschlagen).");
            }

            return result;
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException)
        {
            _logger.Warn("Autostart konnte nicht geändert werden: " + ex.Message);
            return false;
        }
    }

    public void StartProcessStopped()
    {
        // Kein Prozessstart nötig; Methode dokumentiert die bewusste Trennung von Autostart und Start.
        Debug.WriteLine("AutostartService: kein direkter Prozessstart.");
    }
}
