using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux.Infrastructure;

/// <summary>Linux-Power-Aktionen über systemd (logind): suspend, hibernate, poweroff.</summary>
internal interface IPowerService
{
    bool Execute(PowerAction action);
}

internal sealed class LinuxPowerService : IPowerService
{
    public bool Execute(PowerAction action)
    {
        var arguments = action switch
        {
            PowerAction.Sleep => "suspend",
            PowerAction.Hibernate => "hibernate",
            PowerAction.ShutdownGraceful => "poweroff",
            PowerAction.ShutdownHard => "poweroff --force",
            _ => null,
        };

        if (arguments is null)
        {
            return false;
        }

        return RunSystemctl(arguments);
    }

    private static bool RunSystemctl(string arguments)
    {
        try
        {
            using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                return false;
            }

            if (!process.WaitForExit(10_000))
            {
                return true;
            }

            return process.ExitCode == 0;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or PlatformNotSupportedException)
        {
            return false;
        }
    }
}
