namespace VGT.RhythmGuard.Core;

/// <summary>Ausführbare Energie-Aktion am Ende des Bett-/Schau-Modus.</summary>
public enum PowerAction
{
    /// <summary>Energie sparen (Standard).</summary>
    Sleep = 0,

    /// <summary>Ruhezustand (Hibernate).</summary>
    Hibernate = 1,

    /// <summary>Sanftes Herunterfahren mit Vorwarnung.</summary>
    ShutdownGraceful = 2,

    /// <summary>Hartes Herunterfahren nach Countdown (nur nach ausdrücklicher Warnung).</summary>
    ShutdownHard = 3,
}
