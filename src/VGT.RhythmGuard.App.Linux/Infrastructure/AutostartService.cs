namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>XDG-Autostart über ~/.config/autostart/vgt-rhythmg-ard.desktop.</summary>
internal sealed class AutostartService
{
    private readonly AppLogger _logger;

    public AutostartService(AppLogger logger)
    {
        _logger = logger;
    }

    public string ExecutablePath => Environment.ProcessPath ?? "VGT.RhythmGuard";

    public string ExecLine => $"\"{ExecutablePath}\" --logon";

    public bool IsEnabled()
    {
        try
        {
            return File.Exists(AppPaths.AutostartFile);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Warn("Autostart-Status nicht lesbar: " + ex.Message);
            return false;
        }
    }

    public bool TrySetEnabled(bool enabled)
    {
        try
        {
            if (!enabled)
            {
                if (File.Exists(AppPaths.AutostartFile))
                {
                    File.Delete(AppPaths.AutostartFile);
                }

                return !File.Exists(AppPaths.AutostartFile);
            }

            Directory.CreateDirectory(AppPaths.AutostartDirectory);
            var content = string.Join(
                '\n',
                "[Desktop Entry]",
                "Type=Application",
                "Version=1.0",
                "Name=VGT RythmGuard",
                "Comment=Mental health rhythm system: forced breaks, evening wind-down, bed mode",
                $"Exec={ExecLine}",
                "Terminal=false",
                "X-GNOME-Autostart-enabled=true",
                "Categories=Utility;Health;",
                string.Empty);
            File.WriteAllText(AppPaths.AutostartFile, content);
            return File.Exists(AppPaths.AutostartFile);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.Warn("Autostart konnte nicht geändert werden: " + ex.Message);
            return false;
        }
    }

    public void StartProcessStopped()
    {
        // Linux-Autostart erfolgt über die Desktop-Datei; kein Prozessstart nötig.
    }
}
