namespace VGT.RhythmGuard.Core;

/// <summary>Ereignisarten für Historie und Statistiken.</summary>
public enum RhythmEventKind
{
    AppStarted = 0,
    BreakStarted = 1,
    BreakCompleted = 2,
    BreakAborted = 3,
    EveningReminderShown = 4,
    BedModeStarted = 5,
    BedModeCancelled = 6,
    PowerActionExecuted = 7,
    EmergencyExitUsed = 8,
    SettingsChanged = 9,
}

/// <summary>Unveränderliches, persistierbares Ereignis der Rhythmus-Historie.</summary>
public sealed record RhythmEvent
{
    public required RhythmEventKind Kind { get; init; }

    /// <summary>Lokale Wanduhr-Zeit des Ereignisses.</summary>
    public required DateTime AtLocal { get; init; }

    /// <summary>Optionale, nicht-sensible Zusatzinformation (z. B. Dauer oder Aktion).</summary>
    public string? Detail { get; init; }

    public static RhythmEvent Create(RhythmEventKind kind, DateTime atLocal, string? detail = null)
        => new() { Kind = kind, AtLocal = atLocal, Detail = detail };
}
