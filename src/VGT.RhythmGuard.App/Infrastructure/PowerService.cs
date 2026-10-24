using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.App.Infrastructure;

internal interface IPowerService
{
    /// <summary>Führt die Energie-Aktion aus. Rückgabe: true, wenn ein Befehl erfolgreich angestoßen wurde.</summary>
    bool Execute(PowerAction action);
}

/// <summary>Windows-Implementierung von Energie-Aktionen ohne Admin-Rechte.</summary>
internal sealed partial class WindowsPowerService : IPowerService
{
    public bool Execute(PowerAction action) => action switch
    {
        PowerAction.Sleep => TrySuspend(hibernate: false),
        PowerAction.Hibernate => TrySuspend(hibernate: true),
        PowerAction.ShutdownGraceful => RunShutdown(force: false),
        PowerAction.ShutdownHard => RunShutdown(force: true),
        _ => false,
    };

    private static bool TrySuspend(bool hibernate)
    {
        try
        {
            if (SetSuspendState(hibernate, true, false))
            {
                return true;
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException)
        {
            // Fallback unten: reguläres Herunterfahren.
        }

        return RunShutdown(force: true);
    }

    private static bool RunShutdown(bool force)
    {
        try
        {
            var arguments = force ? "/s /f /t 0" : "/s /t 0";
            using var process = Process.Start(new ProcessStartInfo("shutdown.exe", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            });
            return process is not null;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or PlatformNotSupportedException)
        {
            return false;
        }
    }

    [DllImport("powrprof.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetSuspendState(
        [MarshalAs(UnmanagedType.Bool)] bool hibernate,
        [MarshalAs(UnmanagedType.Bool)] bool forceCritical,
        [MarshalAs(UnmanagedType.Bool)] bool disableWakeEvent);
}
