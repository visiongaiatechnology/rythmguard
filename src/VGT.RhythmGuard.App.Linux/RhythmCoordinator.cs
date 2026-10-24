using Avalonia.Threading;
using VGT.RhythmGuard.Linux.Infrastructure;
using VGT.RhythmGuard.Linux.Ui;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux;

/// <summary>
/// Zentrale Orchestrierung: liest Engine-Zustand, steuert UI, Persistenz und Energie-Aktionen.
/// Alle Zustandsübergänge werden hier protokolliert.
/// </summary>
internal sealed class RhythmCoordinator : IDisposable
{
    private readonly AppLogger _logger;
    private readonly JsonConfigStore _configStore;
    private readonly JsonStateStore _stateStore;
    private readonly EventLog _eventLog;
    private readonly IRhythmUi _ui;
    private readonly IPowerService _power;
    private readonly AutostartService _autostart;

    private readonly DispatcherTimer _timer;
    private RhythmSettings _settings;
    private RuntimeState _state;

    private DateTime? _breakShownPhaseStart;
    private DateTime? _suppressedBreakPhaseStart;
    private bool _breakAborted;
    private DateTime? _manualBreakEnd;
    private DateTime? _lastEveningReminderSlot;
    private bool _bedWarningVisible;
    private bool _shutdownWarningVisible;
    private int _lastBedWarningThreshold = int.MaxValue;
    private bool _disposed;

    public RhythmCoordinator(
        JsonConfigStore configStore,
        JsonStateStore stateStore,
        EventLog eventLog,
        RhythmSettings settings,
        RuntimeState state,
        IRhythmUi ui,
        IPowerService power,
        AutostartService autostart,
        AppLogger logger)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _eventLog = eventLog;
        _settings = settings.Validate();
        _state = state;
        _ui = ui;
        _power = power;
        _autostart = autostart;
        _logger = logger;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _timer.Tick += OnTimerTick;
    }

    public event EventHandler<RhythmSnapshot>? SnapshotChanged;

    public event EventHandler? SettingsChanged;

    public RhythmSettings Settings => _settings;

    public RhythmSnapshot? CurrentSnapshot { get; private set; }

    public bool IsBedModeActive => _state.BedSession is not null;

    public void Start()
    {
        _ui.EmergencyExitRequested += OnEmergencyExitRequested;
        _ui.EveningChoiceReceived += OnEveningChoiceReceived;
        _timer.Start();
        Tick();
    }

    public void PauseNow()
    {
        if (_state.BedSession is not null)
        {
            return;
        }

        var now = DateTime.Now;
        _manualBreakEnd = now.AddMinutes(_settings.BreakMinutes);
        _ui.HideEveningReminder();
        LogEvent(RhythmEventKind.BreakStarted, "manuell");
        _ui.ShowBreak(BuildManualBreakDisplay(TimeSpan.FromMinutes(_settings.BreakMinutes), _manualBreakEnd.Value));
    }

    public void StartBedMode()
    {
        var now = DateTime.Now;
        var session = new BedSession
        {
            StartedAtLocal = now,
            Duration = TimeSpan.FromMinutes(_settings.BedModeMinutes),
            Action = _settings.BedModePowerAction,
        };

        _state = _state with { BedSession = session, BedPowerActionExecuted = false };
        _manualBreakEnd = null;
        _ui.HideBreak();
        _ui.HideEveningReminder();
        SaveStateSafe();
        LogEvent(RhythmEventKind.BedModeStarted, session.Action.ToString());
    }

    public void CancelBedMode()
    {
        if (_state.BedSession is null)
        {
            return;
        }

        _state = _state with { BedSession = null, BedPowerActionExecuted = false };
        SaveStateSafe();
        HideBedUi();
        LogEvent(RhythmEventKind.BedModeCancelled);
        Tick();
    }

    public void ApplySettings(RhythmSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Validate();
        _configStore.Save(_settings);

        if (_autostart.IsEnabled() != _settings.StartWithWindows)
        {
            _autostart.TrySetEnabled(_settings.StartWithWindows);
        }

        LogEvent(RhythmEventKind.SettingsChanged);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        Tick();
    }

    public RhythmStatistics GetStatistics()
        => StatisticsCalculator.Calculate(_eventLog.ReadAll(), DateTime.Now);

    public bool IsAutostartEnabled => _autostart.IsEnabled();

    public IReadOnlyList<RhythmEvent> GetRecentEvents(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<RhythmEvent>();
        }

        return _eventLog.ReadAll()
            .OrderByDescending(rhythmEvent => rhythmEvent.AtLocal)
            .Take(count)
            .ToList();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _ui.EmergencyExitRequested -= OnEmergencyExitRequested;
        _ui.EveningChoiceReceived -= OnEveningChoiceReceived;
    }

    private void OnTimerTick(object? sender, EventArgs e) => Tick();

    private void Tick()
    {
        if (_disposed)
        {
            return;
        }

        RhythmSnapshot snapshot;
        try
        {
            snapshot = RhythmEngine.Evaluate(DateTime.Now, _settings, _state.BedSession);
        }
        catch (Exception ex) when (ex is RhythmGuardException or InvalidOperationException)
        {
            _logger.Error("Rhythmus-Berechnung fehlgeschlagen.", ex);
            return;
        }

        CurrentSnapshot = snapshot;
        try
        {
            switch (snapshot.Mode)
            {
                case RhythmMode.Bed:
                    HandleBedActive(snapshot);
                    break;
                case RhythmMode.BedExpired:
                    HandleBedExpired(snapshot);
                    break;
                default:
                    HideBedUi();
                    HandleDayOrEvening(snapshot);
                    break;
            }

            SnapshotChanged?.Invoke(this, snapshot);
        }
        catch (Exception ex) when (ex is RhythmGuardException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            _logger.Error("UI-Tick fehlgeschlagen.", ex);
        }
    }

    private void HandleDayOrEvening(RhythmSnapshot snapshot)
    {
        var now = DateTime.Now;

        if (_manualBreakEnd is { } manualEnd)
        {
            if (now < manualEnd)
            {
                _ui.ShowBreak(BuildManualBreakDisplay(manualEnd - now, manualEnd));
                return;
            }

            _ui.HideBreak();
            LogEvent(RhythmEventKind.BreakCompleted, "manuell");
            _manualBreakEnd = null;
        }

        if (snapshot.Mode == RhythmMode.Break)
        {
            _ui.HideEveningReminder();
            if (_suppressedBreakPhaseStart == snapshot.PhaseStart)
            {
                return;
            }

            var display = new BreakDisplay(
                snapshot.Remaining,
                snapshot.PhaseEnd,
                snapshot.PhaseProgress,
                EmergencyExitEnabled: _settings.EmergencyHoldSeconds > 0,
                EmergencyHoldSeconds: _settings.EmergencyHoldSeconds,
                IsManual: false);

            if (_breakShownPhaseStart != snapshot.PhaseStart)
            {
                _breakShownPhaseStart = snapshot.PhaseStart;
                _breakAborted = false;
                LogEvent(RhythmEventKind.BreakStarted);
                _ui.ShowBreak(display);
            }
            else
            {
                _ui.UpdateBreak(display);
            }

            return;
        }

        if (_breakShownPhaseStart is not null && snapshot.PhaseStart != _breakShownPhaseStart)
        {
            if (!_breakAborted)
            {
                LogEvent(RhythmEventKind.BreakCompleted);
            }

            _breakShownPhaseStart = null;
            _breakAborted = false;
            _ui.HideBreak();
        }

        if (snapshot.Mode == RhythmMode.Evening)
        {
            if (snapshot.NextEveningReminder is { } due
                && now >= due
                && _lastEveningReminderSlot != due)
            {
                _lastEveningReminderSlot = due;
                LogEvent(RhythmEventKind.EveningReminderShown);
                _ui.ShowEveningReminder(new EveningReminderDisplay(now, _settings.BedModeMinutes));
            }
        }
        else
        {
            _ui.HideEveningReminder();
        }
    }

    private void HandleBedActive(RhythmSnapshot snapshot)
    {
        var remaining = snapshot.Remaining;
        var action = snapshot.BedSession?.Action ?? _settings.BedModePowerAction;

        _ui.HideBreak();
        _ui.HideEveningReminder();
        _ui.ShowBedCountdown(new BedCountdownDisplay(remaining, action));

        if (remaining <= TimeSpan.FromMinutes(5))
        {
            var threshold = remaining <= TimeSpan.FromSeconds(60) ? 60 : 300;
            if (threshold < _lastBedWarningThreshold)
            {
                _lastBedWarningThreshold = threshold;
            }

            var warning = new BedWarningDisplay(remaining, action);
            if (!_bedWarningVisible)
            {
                _bedWarningVisible = true;
                _ui.ShowBedWarning(warning);
            }
            else
            {
                _ui.UpdateBedWarning(warning);
            }
        }

        if (remaining <= TimeSpan.FromSeconds(60)
            && action is PowerAction.ShutdownGraceful or PowerAction.ShutdownHard)
        {
            var seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
            if (!_shutdownWarningVisible)
            {
                _shutdownWarningVisible = true;
                _ui.ShowShutdownWarning(new ShutdownWarningDisplay(seconds, action));
            }
            else
            {
                _ui.UpdateShutdownWarning(new ShutdownWarningDisplay(seconds, action));
            }
        }
    }

    private void HandleBedExpired(RhythmSnapshot snapshot)
    {
        HideBedUi();
        var session = snapshot.BedSession;

        if (_state.BedPowerActionExecuted)
        {
            // Aktion wurde bereits ausgeführt (z. B. nach Resume): Sitzung sauber abschließen.
            _state = _state with { BedSession = null, BedPowerActionExecuted = false };
            SaveStateSafe();
            return;
        }

        var action = snapshot.PendingPowerAction ?? session?.Action ?? _settings.BedModePowerAction;
        var success = _power.Execute(action);
        LogEvent(RhythmEventKind.PowerActionExecuted, string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"{action};success={success}"));

        _state = _state with { BedPowerActionExecuted = true };
        SaveStateSafe();

        if (!success)
        {
            _logger.Warn("Energie-Aktion konnte nicht ausgeführt werden.");
        }
    }

    private void HideBedUi()
    {
        _ui.HideBedCountdown();
        _ui.HideBedWarning();
        _ui.HideShutdownWarning();
        _bedWarningVisible = false;
        _shutdownWarningVisible = false;
        _lastBedWarningThreshold = int.MaxValue;
    }

    private BreakDisplay BuildManualBreakDisplay(TimeSpan remaining, DateTime endsAt)
        => new(
            remaining,
            endsAt,
            Progress: 0d,
            EmergencyExitEnabled: _settings.EmergencyHoldSeconds > 0,
            EmergencyHoldSeconds: _settings.EmergencyHoldSeconds,
            IsManual: true);

    private void OnEmergencyExitRequested(object? sender, EventArgs e)
    {
        LogEvent(RhythmEventKind.EmergencyExitUsed);

        if (_manualBreakEnd is not null)
        {
            _manualBreakEnd = null;
            _ui.HideBreak();
            LogEvent(RhythmEventKind.BreakAborted, "manuell");
            return;
        }

        if (_breakShownPhaseStart is { } phase)
        {
            _breakAborted = true;
            _suppressedBreakPhaseStart = phase;
            _ui.HideBreak();
            LogEvent(RhythmEventKind.BreakAborted);
        }
    }

    private void OnEveningChoiceReceived(EveningChoice choice)
    {
        if (choice == EveningChoice.StartBedMode)
        {
            StartBedMode();
        }
    }

    private void LogEvent(RhythmEventKind kind, string? detail = null)
    {
        try
        {
            _eventLog.Append(RhythmEvent.Create(kind, DateTime.Now, detail));
        }
        catch (RhythmStorageException ex)
        {
            _logger.Error("Ereignis konnte nicht protokolliert werden.", ex);
        }
    }

    private void SaveStateSafe()
    {
        try
        {
            _stateStore.Save(_state);
        }
        catch (RhythmStorageException ex)
        {
            _logger.Error("Laufzeitzustand konnte nicht gespeichert werden.", ex);
        }
    }
}
