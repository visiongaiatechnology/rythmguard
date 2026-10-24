namespace VGT.RhythmGuard.Core;

/// <summary>Unveränderlicher Zustands-Snapshot des Rhythmus-Systems zu einem Zeitpunkt.</summary>
public sealed record RhythmSnapshot
{
    public required RhythmMode Mode { get; init; }

    /// <summary>Beginn der aktuellen Phase (lokale Wanduhr).</summary>
    public required DateTime PhaseStart { get; init; }

    /// <summary>Ende der aktuellen Phase (lokale Wanduhr); bei Pausen der Beginn der nächsten Arbeit.</summary>
    public required DateTime PhaseEnd { get; init; }

    /// <summary>Restzeit der aktuellen Phase; nie negativ.</summary>
    public required TimeSpan Remaining { get; init; }

    /// <summary>Fortschritt der aktuellen Phase zwischen 0 und 1.</summary>
    public required double PhaseProgress { get; init; }

    /// <summary>Nächster geplanter Pausenbeginn, falls vorhanden.</summary>
    public DateTime? NextBreakStart { get; init; }

    /// <summary>Nächstes fälliges Abend-Erinnerungsintervall, falls vorhanden.</summary>
    public DateTime? NextEveningReminder { get; init; }

    /// <summary>Nummer des Tageszyklus (1-basiert), falls im Tagesfenster.</summary>
    public int? CycleNumber { get; init; }

    /// <summary>Aktive Bett-/Schau-Sitzung, falls vorhanden.</summary>
    public BedSession? BedSession { get; init; }

    /// <summary>Anstehende Energie-Aktion nach Ablauf des Bett-Modus.</summary>
    public PowerAction? PendingPowerAction { get; init; }

    public required DateTime DayStart { get; init; }

    public required DateTime DayEnd { get; init; }

    public bool IsDayWindow => Mode is RhythmMode.Focus or RhythmMode.Break;

    public bool IsInBreak => Mode == RhythmMode.Break;

    public bool IsEvening => Mode == RhythmMode.Evening;
}
