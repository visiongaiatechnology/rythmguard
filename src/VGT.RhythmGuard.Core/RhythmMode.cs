namespace VGT.RhythmGuard.Core;

/// <summary>Aktueller Rhythmus-Modus des Systems.</summary>
public enum RhythmMode
{
    /// <summary>Arbeitsphase innerhalb des Tagesfensters.</summary>
    Focus = 0,

    /// <summary>Erzwungene Pause (Bildschirm blockiert).</summary>
    Break = 1,

    /// <summary>Abendmodus: regelmäßige Erinnerungen, nicht zu arbeiten.</summary>
    Evening = 2,

    /// <summary>Bett-/Schau-Modus mit laufendem Countdown bis zur Energie-Aktion.</summary>
    Bed = 3,

    /// <summary>Bett-/Schau-Modus ist abgelaufen; die konfigurierte Energie-Aktion steht an.</summary>
    BedExpired = 4,
}
