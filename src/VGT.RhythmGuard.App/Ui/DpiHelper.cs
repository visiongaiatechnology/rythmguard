using System.Runtime.InteropServices;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>DPI-Skalierung für die Fensterpositionierung (Per-Monitor-v2-Manifest).</summary>
internal static class DpiHelper
{
    public static double PrimaryScale { get; } = GetPrimaryScale();

    private static double GetPrimaryScale()
    {
        try
        {
            var dpi = GetDpiForSystem();
            return dpi > 0 ? dpi / 96d : 1d;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            return 1d;
        }
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForSystem();
}
