namespace VGT.RhythmGuard.App.Infrastructure;

/// <summary>
/// Kanonische Pfade der Anwendung. Bevorzugt %LOCALAPPDATA%\VGT\RhythmGuard
/// (Laufzeitdaten gehören nicht ins Roaming-Profil); Fallback auf das Temp-Verzeichnis,
/// falls das Profil durch Richtlinien gesperrt ist.
/// </summary>
internal static class AppPaths
{
    private static readonly Lazy<string> RootLazy = new(ResolveRoot, LazyThreadSafetyMode.ExecutionAndPublication);

    public static string RootDirectory => RootLazy.Value;

    public static string ConfigFilePath => Path.Combine(RootDirectory, "config.json");

    public static string StateFilePath => Path.Combine(RootDirectory, "state.json");

    public static string EventsFilePath => Path.Combine(RootDirectory, "events.jsonl");

    public static string LogFilePath => Path.Combine(RootDirectory, "app.log");

    public static void EnsureRoot() => _ = RootDirectory;

    private static string ResolveRoot()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VGT", "RhythmGuard"),
            Path.Combine(Path.GetTempPath(), "VGT", "RhythmGuard"),
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
                // Nächsten Kandidaten versuchen.
            }
        }

        return candidates[^1];
    }
}
