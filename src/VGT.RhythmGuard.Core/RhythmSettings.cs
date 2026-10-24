namespace VGT.RhythmGuard.Core;

/// <summary>
/// Validierte, unveränderliche Konfiguration des Rhythmus-Systems.
/// Grenzwerte sind bewusst konservativ und werden in <see cref="Validate"/> erzwungen.
/// </summary>
public sealed record RhythmSettings
{
    public static RhythmSettings Default { get; } = new();

    /// <summary>Beginn des aktiven Tagesfensters.</summary>
    public TimeOnly DayStart { get; init; } = new(7, 0);

    /// <summary>Ende des aktiven Tagesfensters (danach Abendmodus).</summary>
    public TimeOnly DayEnd { get; init; } = new(23, 0);

    /// <summary>Länge einer Fokusphase in Minuten.</summary>
    public int WorkMinutes { get; init; } = 120;

    /// <summary>Länge einer erzwungenen Pause in Minuten.</summary>
    public int BreakMinutes { get; init; } = 30;

    /// <summary>Intervall der Abend-Erinnerungen in Minuten.</summary>
    public int EveningReminderMinutes { get; init; } = 20;

    /// <summary>Standarddauer des Bett-/Schau-Modus in Minuten.</summary>
    public int BedModeMinutes { get; init; } = 120;

    /// <summary>Energie-Aktion nach Ablauf des Bett-/Schau-Modus.</summary>
    public PowerAction BedModePowerAction { get; init; } = PowerAction.Sleep;

    /// <summary>Haltezeit der Notausstiegs-Taste in Sekunden; 0 deaktiviert den Notausstieg.</summary>
    public int EmergencyHoldSeconds { get; init; } = 5;

    /// <summary>Abend-Erinnerungen aktiv.</summary>
    public bool EveningRemindersEnabled { get; init; } = true;

    /// <summary>Autostart mit Windows.</summary>
    public bool StartWithWindows { get; init; }

    /// <summary>Aufbewahrung der Ereignis-Historie in Tagen.</summary>
    public int StatsRetentionDays { get; init; } = 90;

    /// <summary>UI-Sprache: system, de, en, es, it, ru, zh, ja.</summary>
    public string UiLanguage { get; init; } = "system";

    /// <summary>
    /// Prüft und normalisiert die Konfiguration. Ungültige Werte werden als
    /// <see cref="RhythmValidationException"/> zurückgewiesen (fail-fast).
    /// </summary>
    public RhythmSettings Validate()
    {
        if (DayStart >= DayEnd)
        {
            throw new RhythmValidationException("DayStart muss vor DayEnd liegen.");
        }

        if (WorkMinutes is < 30 or > 480)
        {
            throw new RhythmValidationException("WorkMinutes muss zwischen 30 und 480 liegen.");
        }

        if (BreakMinutes is < 5 or > 120)
        {
            throw new RhythmValidationException("BreakMinutes muss zwischen 5 und 120 liegen.");
        }

        if (EveningReminderMinutes is < 5 or > 120)
        {
            throw new RhythmValidationException("EveningReminderMinutes muss zwischen 5 und 120 liegen.");
        }

        if (BedModeMinutes is < 15 or > 480)
        {
            throw new RhythmValidationException("BedModeMinutes muss zwischen 15 und 480 liegen.");
        }

        if (EmergencyHoldSeconds is < 0 or > 15)
        {
            throw new RhythmValidationException("EmergencyHoldSeconds muss zwischen 0 und 15 liegen.");
        }

        if (StatsRetentionDays is < 7 or > 365)
        {
            throw new RhythmValidationException("StatsRetentionDays muss zwischen 7 und 365 liegen.");
        }

        if (!Enum.IsDefined(BedModePowerAction))
        {
            throw new RhythmValidationException("Unbekannte Energie-Aktion.");
        }

        if (UiLanguage is not ("system" or "de" or "en" or "es" or "it" or "ru" or "zh" or "ja"))
        {
            throw new RhythmValidationException("Unbekannte UI-Sprache.");
        }

        var dayLength = DayEnd.ToTimeSpan() - DayStart.ToTimeSpan();
        var minimumCycle = TimeSpan.FromMinutes(WorkMinutes + BreakMinutes);
        if (minimumCycle > dayLength)
        {
            throw new RhythmValidationException("Arbeits- und Pausenlänge passen nicht in das Tagesfenster.");
        }

        return this;
    }
}
