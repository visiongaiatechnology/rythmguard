namespace VGT.RhythmGuard.Core;

/// <summary>Persistierbare Bett-/Schau-Sitzung. Überlebt App-Neustarts.</summary>
public sealed record BedSession
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Lokale Wanduhr-Zeit des Starts (ohne Zeitzonen-Verschiebung).</summary>
    public DateTime StartedAtLocal { get; init; } = DateTime.Now;

    public TimeSpan Duration { get; init; } = TimeSpan.FromMinutes(120);

    public PowerAction Action { get; init; } = PowerAction.Sleep;

    public DateTime EndsAtLocal => StartedAtLocal + Duration;

    public bool IsExpired(DateTime localNow) => localNow >= EndsAtLocal;
}
