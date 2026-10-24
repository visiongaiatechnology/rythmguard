namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>Single-Instance-Schutz über exklusives Datei-Lock (Unix-freundlich, kein benannter Mutex).</summary>
internal sealed class SingleInstanceGuard : IDisposable
{
    private FileStream? _lock;

    public bool TryAcquire()
    {
        try
        {
            var path = Path.Combine(AppPaths.RootDirectory, "instance.lock");
            _lock = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        _lock?.Dispose();
        _lock = null;
    }
}
