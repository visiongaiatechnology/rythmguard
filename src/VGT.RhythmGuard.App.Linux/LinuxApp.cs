using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using VGT.RhythmGuard.Core;
using VGT.RhythmGuard.Linux.Infrastructure;
using VGT.RhythmGuard.Linux.Localization;
using VGT.RhythmGuard.Linux.Ui;

namespace VGT.RhythmGuard.Linux;

/// <summary>Linux-Shell: Dashboard, Tray, Coordinator, XDG-Persistenz und systemd-Power.</summary>
public sealed class LinuxApp : Application
{
    private SingleInstanceGuard? _single;
    private RhythmCoordinator? _coordinator;
    private AvaloniaUiService? _ui;
    private DashboardWindow? _dashboard;
    private TrayIcon? _tray;
    private AppLogger? _logger;
    private string? _screenshotDirectory;

    public override void Initialize()
    {
        Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
        RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        desktop.Exit += (_, _) => Dispose();

        File.WriteAllText(Path.Combine(Path.GetTempPath(), "rg-linux-entered.txt"), "entered " + DateTime.Now.ToString("O"));
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        var screenshotIndex = Array.IndexOf(args, "--screenshot");
        _screenshotDirectory = screenshotIndex >= 0 && screenshotIndex + 1 < args.Length ? args[screenshotIndex + 1] : null;

        try
        {
            AppPaths.EnsureRoot();
            _logger = new AppLogger(AppPaths.LogFilePath);
            _logger.Info("Start Linux · v" + typeof(LinuxApp).Assembly.GetName().Version + " · " + string.Join(' ', args));

            _single = new SingleInstanceGuard();
            if (!_single.TryAcquire())
            {
                _logger.Info("Zweite Instanz beendet.");
                desktop.Shutdown(1);
                return;
            }

            var configStore = new JsonConfigStore(AppPaths.ConfigFilePath, _logger);
            var stateStore = new JsonStateStore(AppPaths.StateFilePath, _logger);
            var eventLog = new EventLog(AppPaths.EventsFilePath);
            var hadConfig = File.Exists(AppPaths.ConfigFilePath);
            var settings = configStore.Load();
            if (!hadConfig)
            {
                configStore.Save(settings);
            }

            var state = stateStore.Load();
            eventLog.Prune(DateTime.Now, settings.StatsRetentionDays);
            Localizer.Initialize(settings.UiLanguage);

            _ui = new AvaloniaUiService();
            _coordinator = new RhythmCoordinator(
                configStore, stateStore, eventLog, settings, state, _ui,
                new LinuxPowerService(), new AutostartService(_logger), _logger);
            _coordinator.Start();
            eventLog.Append(RhythmEvent.Create(RhythmEventKind.AppStarted, DateTime.Now));

            _dashboard = new DashboardWindow(_coordinator);
            desktop.MainWindow = _dashboard;
            _dashboard.Closing += (_, e) =>
            {
                if (_screenshotDirectory is not null)
                {
                    return;
                }

                e.Cancel = true;
                _dashboard!.Hide();
            };

            if (!hadConfig || args.Contains("--settings") || args.Contains("--dashboard") || _screenshotDirectory is not null)
            {
                _dashboard.Show();
            }

            TryCreateTray();

            if (args.Contains("--demo-break"))
            {
                _coordinator.PauseNow();
            }

            if (args.Contains("--selftest"))
            {
                var selfTestTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                selfTestTimer.Tick += (_, _) =>
                {
                    selfTestTimer.Stop();
                    RunSelfTest();
                    Console.WriteLine("SELFTEST OK (Linux Avalonia UI constructed)");
                    desktop.Shutdown(0);
                };
                selfTestTimer.Start();
            }

            if (_screenshotDirectory is not null)
            {
                RunScreenshots(desktop, _screenshotDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error("Start fehlgeschlagen.", ex);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "rg-linux-fatal.txt"), ex.ToString());
            Console.Error.WriteLine(ex);
            desktop.Shutdown(2);
            return;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RunSelfTest()
    {
        _ = new OverlayWindow(OverlayKind.Break, 5);
        _ = new OverlayWindow(OverlayKind.ShutdownWarning, 5);
        _ = new EveningReminderWindow(new EveningReminderDisplay(DateTime.Now, 120));
        _ = new BedCountdownWindow(new BedCountdownDisplay(TimeSpan.FromMinutes(90), PowerAction.Sleep));
        _ = new BedWarningWindow(new BedWarningDisplay(TimeSpan.FromMinutes(4), PowerAction.Sleep));
        _logger?.Info("Linux-Selftest: alle Fenster konstruiert.");
    }

    private void RunScreenshots(IClassicDesktopStyleApplicationLifetime desktop, string directory)
    {
        Directory.CreateDirectory(directory);
        var windows = new List<Window>();

        var breakOverlay = new OverlayWindow(OverlayKind.Break, 5);
        breakOverlay.ApplyBreak(new BreakDisplay(
            TimeSpan.FromMinutes(29) + TimeSpan.FromSeconds(12), DateTime.Now.AddMinutes(29), 0.08, true, 5, false));
        breakOverlay.Show();
        windows.Add(breakOverlay);

        var evening = new EveningReminderWindow(new EveningReminderDisplay(DateTime.Now, 120));
        evening.Show();
        windows.Add(evening);

        var bed = new BedCountdownWindow(new BedCountdownDisplay(TimeSpan.FromMinutes(118), PowerAction.Sleep));
        bed.Show();
        windows.Add(bed);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(900) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                Localizer.TranslateTree(_dashboard!);
                if (_dashboard?.Content is Control dashboardContent)
                {
                    var wrapped = new Border
                    {
                        Width = 1280,
                        Height = 820,
                        Background = UiKit.AuroraGradient(),
                        Child = dashboardContent,
                    };
                    _dashboard.Content = wrapped;
                    Capture(wrapped, Path.Combine(directory, "linux-dashboard.png"), 1280, 820);
                }
                Capture(breakOverlay.Content as Control, Path.Combine(directory, "linux-break-overlay.png"), 1600, 900);
                evening.Hide();
                Capture(evening.Content as Control, Path.Combine(directory, "linux-evening-reminder.png"), 440, 280);
                bed.Hide();
                Capture(bed.Content as Control, Path.Combine(directory, "linux-bed-countdown.png"), 330, 84);
                _logger?.Info("Screenshots erzeugt: " + directory);
            }
            catch (Exception ex)
            {
                _logger?.Error("Screenshot fehlgeschlagen.", ex);
                Console.Error.WriteLine(ex);
            }
            finally
            {
                foreach (var window in windows)
                {
                    window.Close();
                }

                _dashboard?.Close();
                desktop.Shutdown(0);
            }
        };
        timer.Start();
    }

    private static void Capture(Avalonia.Visual? visual, string path, int width, int height)
    {
        if (visual is null)
        {
            return;
        }

        if (visual is Control control)
        {
            control.Measure(new Size(width, height));
            control.Arrange(new Rect(0, 0, width, height));
            control.UpdateLayout();
        }

        var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        bitmap.Render(visual);
        bitmap.Save(path);
    }

    private void TryCreateTray()
    {
        if (_coordinator is null || _logger is null)
        {
            return;
        }

        try
        {
            _tray = new TrayIcon
            {
                Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://VGT.RhythmGuard/Assets/app.ico"))),
                ToolTipText = "VGT RythmGuard",
                IsVisible = true,
            };

            var menu = new NativeMenu();
            var open = new NativeMenuItem("Dashboard öffnen");
            open.Click += (_, _) => ShowDashboard();
            menu.Add(open);
            menu.Add(new NativeMenuItemSeparator());

            var pause = new NativeMenuItem(Localizer.T("Pause jetzt"));
            pause.Click += (_, _) => _coordinator.PauseNow();
            menu.Add(pause);

            var bed = new NativeMenuItem("Bett-Modus starten");
            bed.Click += (_, _) => _coordinator.StartBedMode();
            menu.Add(bed);

            var autostart = new NativeMenuItem("Autostart (mit Windows starten)")
            {
                ToggleType = NativeMenuItemToggleType.CheckBox,
                IsChecked = new AutostartService(_logger).IsEnabled(),
            };
            autostart.Click += (_, _) =>
            {
                var service = new AutostartService(_logger);
                _ = service.TrySetEnabled(!service.IsEnabled());
                _coordinator.ApplySettings(_coordinator.Settings with { StartWithWindows = service.IsEnabled() });
            };
            menu.Add(autostart);
            menu.Add(new NativeMenuItemSeparator());

            var quit = new NativeMenuItem("Beenden");
            quit.Click += (_, _) => (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            menu.Add(quit);
            _tray.Menu = menu;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or IOException)
        {
            _logger.Warn("Tray nicht verfügbar (AppIndicator fehlt): " + ex.Message);
        }
    }

    private void ShowDashboard()
    {
        if (_dashboard is null)
        {
            return;
        }

        if (_dashboard.IsVisible)
        {
            _dashboard.Activate();
            return;
        }

        _dashboard.Show();
        _dashboard.Activate();
    }

    private void Dispose()
    {
        _tray?.Dispose();
        _tray = null;
        _ui?.Dispose();
        _ui = null;
        _coordinator?.Dispose();
        _coordinator = null;
        _single?.Dispose();
        _single = null;
    }
}
