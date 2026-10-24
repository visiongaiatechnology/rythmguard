using System.Threading;

namespace VGT.RhythmGuard.App.Infrastructure;

/// <summary>Verhindert mehrere gleichzeitig laufende Instanzen pro Benutzersitzung.</summary>
internal sealed class SingleInstanceGuard : IDisposable
{
    private const string MutexName = @"Local\VGT.RhythmGuard.SingleInstance.v1";

    private Mutex? _mutex;
    private bool _owned;

    public bool TryAcquire()
    {
        _mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
            return false;
        }

        _owned = true;
        return true;
    }

    public void Dispose()
    {
        if (_mutex is null)
        {
            return;
        }

        try
        {
            if (_owned)
            {
                _mutex.ReleaseMutex();
            }
        }
        catch (ApplicationException)
        {
            // Mutex wurde bereits freigegeben.
        }
        finally
        {
            _mutex.Dispose();
            _mutex = null;
        }
    }
}
