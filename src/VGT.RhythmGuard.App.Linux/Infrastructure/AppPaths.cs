namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>XDG-konforme Pfade: ~/.local/share/vgt-rhythmg-ard bzw. $XDG_DATA_HOME.</summary>
internal static class AppPaths
{
    private static readonly Lazy<string> RootLazy = new(ResolveRoot, LazyThreadSafetyMode.ExecutionAndPublication);

    public static string RootDirectory => RootLazy.Value;

    public static string ConfigFilePath => Path.Combine(RootDirectory, "config.json");

    public static string StateFilePath => Path.Combine(RootDirectory, "state.json");

    public static string EventsFilePath => Path.Combine(RootDirectory, "events.jsonl");

    public static string LogFilePath => Path.Combine(RootDirectory, "app.log");

    public static string AutostartDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "autostart");

    public static string AutostartFile => Path.Combine(AutostartDirectory, "vgt-rhythmg-ard.desktop");

    public static void EnsureRoot() => _ = RootDirectory;

    private static string ResolveRoot()
    {
        var candidates = new[]
        {
            Path.Combine(GetXdgDataHome(), "vgt-rhythmg-ard"),
            Path.Combine(Path.GetTempPath(), "vgt-rhythmg-ard"),
        };

        foreach (var candidate in candidates)
        {
            try
            {
                Directory.CreateDirectory(candidate);
                return candidate;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // next candidate
            }
        }

        return candidates[^1];
    }

    private static string GetXdgDataHome()
    {
        var xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(xdg))
        {
            return xdg;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return string.IsNullOrWhiteSpace(home) ? Path.GetTempPath() : Path.Combine(home, ".local", "share");
    }
}
