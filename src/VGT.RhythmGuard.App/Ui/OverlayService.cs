using System.Windows.Threading;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>Verwaltet alle Overlays; kennt keine Coordinator-Logik.</summary>
internal sealed class OverlayService : IRhythmUi, IDisposable
{
    private readonly List<FullscreenOverlayWindow> _breakOverlays = new();
    private readonly List<FullscreenOverlayWindow> _shutdownOverlays = new();
    private readonly DispatcherTimer _eveningAutoHide;
    private EveningReminderWindow? _eveningWindow;
    private BedCountdownWindow? _countdownWindow;
    private BedWarningWindow? _bedWarningWindow;
    private bool _disposed;

    public OverlayService()
    {
        _eveningAutoHide = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(90),
        };
        _eveningAutoHide.Tick += (_, _) => HideEveningReminder();
    }

    public event EventHandler? EmergencyExitRequested;

    public event EveningChoiceHandler? EveningChoiceReceived;

    public void ShowBreak(BreakDisplay display)
    {
        EnsureShutdownOverlaysClosed();
        if (_breakOverlays.Count == 0)
        {
            CreateBreakOverlays();
        }

        foreach (var overlay in _breakOverlays)
        {
            overlay.ApplyBreak(display);
        }
    }

    public void UpdateBreak(BreakDisplay display) => ShowBreak(display);

    public void HideBreak()
    {
        foreach (var overlay in _breakOverlays)
        {
            overlay.CloseWithoutNotification();
        }

        _breakOverlays.Clear();
    }

    public void ShowEveningReminder(EveningReminderDisplay display)
    {
        if (_eveningWindow is null)
        {
            _eveningWindow = new EveningReminderWindow();
            _eveningWindow.ChoiceMade += OnEveningWindowChoice;
            _eveningWindow.Closed += (_, _) =>
            {
                _eveningWindow = null;
                _eveningAutoHide.Stop();
            };
        }

        _eveningWindow.Apply(display);
        _eveningWindow.Show();
        _eveningWindow.PositionBottomRight();
        _eveningAutoHide.Stop();
        _eveningAutoHide.Start();
    }

    public void HideEveningReminder()
    {
        _eveningAutoHide.Stop();
        if (_eveningWindow is null)
        {
            return;
        }

        var window = _eveningWindow;
        _eveningWindow = null;
        window.CloseWithoutNotification();
    }

    public void ShowBedCountdown(BedCountdownDisplay display)
    {
        if (_countdownWindow is null)
        {
            _countdownWindow = new BedCountdownWindow();
            _countdownWindow.Closed += (_, _) => _countdownWindow = null;
        }

        _countdownWindow.Apply(display);
        if (!_countdownWindow.IsVisible)
        {
            _countdownWindow.Show();
        }

        _countdownWindow.PositionBottomRight();
    }

    public void HideBedCountdown()
    {
        if (_countdownWindow is null)
        {
            return;
        }

        var window = _countdownWindow;
        _countdownWindow = null;
        window.CloseWithoutNotification();
    }

    public void ShowBedWarning(BedWarningDisplay display)
    {
        if (_bedWarningWindow is null)
        {
            _bedWarningWindow = new BedWarningWindow();
            _bedWarningWindow.Closed += (_, _) => _bedWarningWindow = null;
        }

        _bedWarningWindow.Apply(display);
        if (!_bedWarningWindow.IsVisible)
        {
            _bedWarningWindow.Show();
        }

        _bedWarningWindow.PositionTopCenter();
    }

    public void UpdateBedWarning(BedWarningDisplay display) => ShowBedWarning(display);

    public void HideBedWarning()
    {
        if (_bedWarningWindow is null)
        {
            return;
        }

        var window = _bedWarningWindow;
        _bedWarningWindow = null;
        window.CloseWithoutNotification();
    }

    public void ShowShutdownWarning(ShutdownWarningDisplay display)
    {
        HideBreak();
        if (_shutdownOverlays.Count == 0)
        {
            CreateShutdownOverlays();
        }

        foreach (var overlay in _shutdownOverlays)
        {
            overlay.ApplyShutdown(display);
        }
    }

    public void UpdateShutdownWarning(ShutdownWarningDisplay display) => ShowShutdownWarning(display);

    public void HideShutdownWarning()
    {
        foreach (var overlay in _shutdownOverlays)
        {
            overlay.CloseWithoutNotification();
        }

        _shutdownOverlays.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _eveningAutoHide.Stop();
        HideBreak();
        HideShutdownWarning();
        HideBedCountdown();
        HideBedWarning();
        HideEveningReminder();
    }

    private void EnsureShutdownOverlaysClosed()
    {
        if (_shutdownOverlays.Count > 0)
        {
            HideShutdownWarning();
        }
    }

    private void CreateBreakOverlays()
    {
        var scale = DpiHelper.PrimaryScale;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var overlay = new FullscreenOverlayWindow(OverlayKind.Break, screen, scale);
            overlay.EmergencyExitRequested += OnOverlayEmergencyExitRequested;
            overlay.Show();
            overlay.ActivateWindow();
            _breakOverlays.Add(overlay);
        }
    }

    private void CreateShutdownOverlays()
    {
        var scale = DpiHelper.PrimaryScale;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var overlay = new FullscreenOverlayWindow(OverlayKind.ShutdownWarning, screen, scale);
            overlay.Show();
            overlay.ActivateWindow();
            _shutdownOverlays.Add(overlay);
        }
    }

    private void OnOverlayEmergencyExitRequested(object? sender, EventArgs e)
        => EmergencyExitRequested?.Invoke(this, EventArgs.Empty);

    private void OnEveningWindowChoice(EveningChoice choice)
    {
        HideEveningReminder();
        EveningChoiceReceived?.Invoke(choice);
    }
}
