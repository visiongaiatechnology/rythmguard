using Avalonia.Controls;
using VGT.RhythmGuard.Core;
using VGT.RhythmGuard.Linux.Localization;

namespace VGT.RhythmGuard.Linux.Ui;

/// <summary>Avalonia-Umsetzung der UI-Abstraktion (1:1-Funktionalität zur Windows-Version).</summary>
internal sealed class AvaloniaUiService : IRhythmUi, IDisposable
{
    private readonly List<OverlayWindow> _overlays = new();
    private readonly List<OverlayWindow> _shutdownOverlays = new();
    private EveningReminderWindow? _evening;
    private BedCountdownWindow? _countdown;
    private BedWarningWindow? _warning;
    private bool _disposed;

    public event EventHandler? EmergencyExitRequested;

    public event EveningChoiceHandler? EveningChoiceReceived;

    public void ShowBreak(BreakDisplay display)
    {
        CloseShutdownOverlays();
        if (_overlays.Count == 0)
        {
            CreateOverlays(OverlayKind.Break);
        }

        foreach (var overlay in _overlays)
        {
            overlay.ApplyBreak(display);
        }
    }

    public void UpdateBreak(BreakDisplay display) => ShowBreak(display);

    public void HideBreak() => CloseAll(_overlays);

    public void ShowEveningReminder(EveningReminderDisplay display)
    {
        HideEveningReminder();
        _evening = new EveningReminderWindow(display);
        _evening.ChoiceMade += choice =>
        {
            HideEveningReminder();
            EveningChoiceReceived?.Invoke(choice);
        };
        _evening.Closed += (_, _) => _evening = null;
        _evening.Show();
    }

    public void HideEveningReminder()
    {
        if (_evening is null)
        {
            return;
        }

        var window = _evening;
        _evening = null;
        window.Close();
    }

    public void ShowBedCountdown(BedCountdownDisplay display)
    {
        if (_countdown is null)
        {
            _countdown = new BedCountdownWindow(display);
            _countdown.Closed += (_, _) => _countdown = null;
            _countdown.Show();
            return;
        }

        _countdown.Apply(display);
    }

    public void HideBedCountdown()
    {
        _countdown?.Close();
        _countdown = null;
    }

    public void ShowBedWarning(BedWarningDisplay display)
    {
        if (_warning is null)
        {
            _warning = new BedWarningWindow(display);
            _warning.Closed += (_, _) => _warning = null;
            _warning.Show();
            return;
        }

        _warning.Apply(display);
    }

    public void UpdateBedWarning(BedWarningDisplay display) => ShowBedWarning(display);

    public void HideBedWarning()
    {
        _warning?.Close();
        _warning = null;
    }

    public void ShowShutdownWarning(ShutdownWarningDisplay display)
    {
        HideBreak();
        if (_shutdownOverlays.Count == 0)
        {
            CreateOverlays(OverlayKind.ShutdownWarning);
        }

        foreach (var overlay in _shutdownOverlays)
        {
            overlay.ApplyShutdown(display);
        }
    }

    public void UpdateShutdownWarning(ShutdownWarningDisplay display) => ShowShutdownWarning(display);

    public void HideShutdownWarning() => CloseShutdownOverlays();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        HideBreak();
        HideShutdownWarning();
        HideBedCountdown();
        HideBedWarning();
        HideEveningReminder();
    }

    private void CreateOverlays(OverlayKind kind)
    {
        var target = kind == OverlayKind.Break ? _overlays : _shutdownOverlays;
        var overlay = new OverlayWindow(kind, 5);
            if (kind == OverlayKind.Break)
            {
                overlay.EmergencyExitRequested += (_, _) => EmergencyExitRequested?.Invoke(this, EventArgs.Empty);
            }
        overlay.ActivateWindow();
        target.Add(overlay);
    }

    private void CloseShutdownOverlays() => CloseAll(_shutdownOverlays);

    private static void CloseAll(List<OverlayWindow> windows)
    {
        foreach (var window in windows)
        {
            window.CloseWithoutNotification();
        }

        windows.Clear();
    }
}
