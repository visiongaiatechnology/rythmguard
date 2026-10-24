namespace VGT.RhythmGuard.Core;

/// <summary>
/// Typisierte Fehlerhierarchie gemäß VGT-Standard.
/// Validierungsfehler dürfen dem Nutzer verständlich gezeigt werden;
/// Zustands- und Speicherfehler bleiben intern und werden nur geloggt.
/// </summary>
public abstract class RhythmGuardException : Exception
{
    protected RhythmGuardException(string message) : base(message)
    {
    }

    protected RhythmGuardException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>Fehlerhafte Nutzereingabe oder Konfiguration; darf verständlich angezeigt werden.</summary>
public sealed class RhythmValidationException : RhythmGuardException
{
    public RhythmValidationException(string message) : base(message)
    {
    }

    public RhythmValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>Ungültiger interner Zustand (Persistenz, State-Machine); Details bleiben intern.</summary>
public sealed class RhythmStateException : RhythmGuardException
{
    public RhythmStateException(string message) : base(message)
    {
    }

    public RhythmStateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>Fehler beim Lesen/Schreiben persistenter Daten; Details bleiben intern.</summary>
public sealed class RhythmStorageException : RhythmGuardException
{
    public RhythmStorageException(string message) : base(message)
    {
    }

    public RhythmStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
