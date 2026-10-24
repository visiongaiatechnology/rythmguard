using System.Windows;
using System.Windows.Threading;
using VGT.RhythmGuard.App.Infrastructure;
using VGT.RhythmGuard.App.Ui;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.App;

public partial class App : System.Windows.Application
{
    private SingleInstanceGuard? _singleInstance;
    private RhythmCoordinator? _coordinator;
    private TrayService? _tray;
    private SettingsWindow? _settingsWindow;
    private AppLogger? _logger;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppPaths.EnsureRoot();
        _logger = new AppLogger(AppPaths.LogFilePath);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

        var arguments = AppArguments.Parse(e.Args);
        if (arguments.ScreenshotDirectory is not null)
        {
            try
            {
                ScreenshotRunner.CaptureAll(arguments.ScreenshotDirectory, _logger);
            }
            catch (Exception ex)
            {
                _logger.Error("Screenshot-Modus fehlgeschlagen.", ex);
            }

            Shutdown();
            return;
        }

        // Marken-Ladescreen: erst rendern lassen, dann initialisieren.
        var splash = new SplashWindow();
        splash.Show();
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            try
            {
                InitializeApplication(arguments);
            }
            catch (Exception ex)
            {
                _logger?.Error("Start fehlgeschlagen.", ex);
                ShowFatalError(ex);
                Shutdown();
            }
            finally
            {
                splash.CloseSplash();
            }
        }));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _coordinator?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }

    private void InitializeApplication(AppArguments arguments)
    {
        _singleInstance = new SingleInstanceGuard();
        if (!_singleInstance.TryAcquire())
        {
            _logger!.Info("Zweite Instanz erkannt und beendet.");
            System.Windows.MessageBox.Show(
                "VGT RythmGuard läuft bereits. Das Dashboard öffnest du über das Tray-Symbol.",
                "VGT RythmGuard",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _logger!.Info("Start · Version " + typeof(App).Assembly.GetName().Version + " · Argumente: " + string.Join(' ', arguments.SourceArguments));

        var configStore = new JsonConfigStore(AppPaths.ConfigFilePath, _logger);
        var stateStore = new JsonStateStore(AppPaths.StateFilePath, _logger);
        var eventLog = new EventLog(AppPaths.EventsFilePath);
        var hadConfig = File.Exists(AppPaths.ConfigFilePath);
        var settings = configStore.Load();
        if (!hadConfig)
        {
            // Standardkonfiguration einmalig persistieren: Dashboard erscheint nur beim ersten Start.
            try
            {
                configStore.Save(settings);
            }
            catch (RhythmStorageException ex)
            {
                _logger.Error("Standardkonfiguration konnte nicht gespeichert werden.", ex);
            }
        }

        var state = stateStore.Load();
        eventLog.Prune(DateTime.Now, settings.StatsRetentionDays);

        Localization.Localizer.Initialize(settings.UiLanguage);

        var ui = new OverlayService();
        _coordinator = new RhythmCoordinator(
            configStore,
            stateStore,
            eventLog,
            settings,
            state,
            ui,
            new WindowsPowerService(),
            new AutostartService(_logger),
            _logger);

        _tray = new TrayService(_coordinator, OpenSettings, ExitApplication);
        _coordinator.Start();

        try
        {
            eventLog.Append(RhythmEvent.Create(RhythmEventKind.AppStarted, DateTime.Now));
        }
        catch (RhythmStorageException ex)
        {
            _logger.Error("AppStarted konnte nicht protokolliert werden.", ex);
        }

        if (!hadConfig || arguments.OpenSettings)
        {
            OpenSettings();
        }

        if (arguments.DemoBreak)
        {
            _coordinator.PauseNow();
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.Error("Unbehandelte UI-Ausnahme.", e.Exception);
        e.Handled = true;
        ShowFatalError(e.Exception);
        Shutdown();
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        => _ = e;

    private void ShowFatalError(Exception exception)
    {
        try
        {
            System.Windows.MessageBox.Show(
                "VGT RythmGuard ist auf einen unerwarteten Fehler gestoßen und wird beendet.\n\n"
                + exception.Message
                + "\n\nDetails: " + AppPaths.LogFilePath,
                "VGT RythmGuard",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (InvalidOperationException)
        {
            // Kein Dispatcher verfügbar (z. B. während des Herunterfahrens).
        }
    }

    private void OpenSettings()
    {
        Dispatcher.Invoke(() =>
        {
            if (_coordinator is null)
            {
                return;
            }

            if (_settingsWindow is { IsLoaded: true })
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow(_coordinator);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        });
    }

    private void ExitApplication()
    {
        Dispatcher.Invoke(() =>
        {
            _logger?.Info("Beenden angefordert.");
            _tray?.Dispose();
            _tray = null;
            _coordinator?.Dispose();
            _coordinator = null;
            Shutdown();
        });
    }
}

/// <summary>CLI-Argumente für Startmodi und QA-Screenshots.</summary>
internal sealed record AppArguments(
    bool Logon,
    bool OpenSettings,
    bool DemoBreak,
    string? ScreenshotDirectory,
    IReadOnlyList<string> SourceArguments)
{
    public static AppArguments Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var logon = false;
        var openSettings = false;
        var demoBreak = false;
        string? screenshotDirectory = null;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--logon":
                    logon = true;
                    break;
                case "--settings":
                    openSettings = true;
                    break;
                case "--demo-break":
                    demoBreak = true;
                    break;
                case "--screenshot":
                    if (index + 1 < args.Count)
                    {
                        screenshotDirectory = args[index + 1];
                        index++;
                    }

                    break;
                default:
                    break;
            }
        }

        return new AppArguments(logon, openSettings, demoBreak, screenshotDirectory, args.ToList());
    }
}
