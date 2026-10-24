using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using VGT.RhythmGuard.Core;
using VGT.RhythmGuard.Linux.Localization;

namespace VGT.RhythmGuard.Linux.Ui;

/// <summary>Aurora-Glass-Dashboard (Linux) — Übersicht, Einstellungen, Verlauf.</summary>
internal sealed class DashboardWindow : Window
{
    private readonly RhythmCoordinator _coordinator;
    private readonly TextBlock _statusMode = UiKit.Caption("");
    private readonly TextBlock _statusCountdown = UiKit.TextBlock("--:--", 38, FontWeight.Light);
    private readonly TextBlock _kpiNext = UiKit.TextBlock("--:--", 30, FontWeight.Light);
    private readonly TextBlock _kpiBreaks = UiKit.TextBlock("0", 30, FontWeight.Light);
    private readonly TextBlock _kpiEvening = UiKit.TextBlock("--", 30, FontWeight.Light);
    private readonly TextBlock _kpiStreak = UiKit.TextBlock("0", 30, FontWeight.Light);
    private readonly TextBlock _userName = UiKit.TextBlock(Environment.UserName, 13.5, FontWeight.SemiBold);
    private readonly StackPanel _events = new() { Spacing = 6 };
    private readonly StackPanel _history = new() { Spacing = 6 };
    private readonly RhythmChart _chart = new() { Height = 190 };
    private readonly ScoreRing _ring = new() { Width = 170, Height = 170 };
    private readonly ComboBox _language = new() { Width = 190 };
    private readonly TextBox _dayStart = new() { Width = 80 };
    private readonly TextBox _dayEnd = new() { Width = 80 };
    private readonly Slider _work = Slider(30, 240, 120);
    private readonly Slider _pause = Slider(5, 60, 30);
    private readonly Slider _eveningInterval = Slider(5, 60, 20);
    private readonly Slider _bed = Slider(15, 240, 120);
    private readonly Slider _emergency = Slider(0, 10, 5);
    private readonly CheckBox _eveningEnabled = new() { Content = UiKit.TextBlock(Localizer.T("Erinnerungen nach 23:00 anzeigen"), 13) };
    private readonly CheckBox _autostart = new() { Content = UiKit.TextBlock(Localizer.T("Mit Windows starten"), 13) };
    private readonly RadioButton _sleep = new() { Content = UiKit.TextBlock(Localizer.T("Ruhezustand"), 13), GroupName = "power", IsChecked = true };
    private readonly RadioButton _hibernate = new() { Content = UiKit.TextBlock(Localizer.T("Hibernate"), 13), GroupName = "power" };
    private readonly RadioButton _soft = new() { Content = UiKit.TextBlock(Localizer.T("Sanft herunterfahren"), 13), GroupName = "power" };
    private readonly RadioButton _hard = new() { Content = UiKit.TextBlock(Localizer.T("Hart herunterfahren"), 13), GroupName = "power" };
    private readonly TextBlock _error = UiKit.Caption("");
    private readonly Panel _overview = new();
    private readonly Panel _settings = new();
    private readonly Panel _historyPage = new();
    private readonly TextBlock _pageTitle = UiKit.TextBlock("Übersicht", 28, FontWeight.SemiBold);

    public DashboardWindow(RhythmCoordinator coordinator)
    {
        _coordinator = coordinator;
        Title = "VGT Rhythm Guard — Dashboard";
        Width = 1280;
        Height = 820;
        MinWidth = 1000;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = UiKit.AuroraGradient();
        _userName.Text = FormatUser(Environment.UserName);

        _language.ItemsSource = new[] { "Systemsprache", "Deutsch", "English", "Español", "Italiano", "Русский", "中文", "日本語" };
        _language.SelectedIndex = 0;

        var sidebar = BuildSidebar();
        var main = new Grid { Margin = new Thickness(18, 14, 22, 16) };
        main.ColumnDefinitions = new ColumnDefinitions("238,18,*");
        Grid.SetColumn(sidebar, 0);
        main.Children.Add(sidebar);
        var content = new Grid();
        Grid.SetColumn(content, 2);
        content.RowDefinitions = new RowDefinitions("Auto,*");
        content.Children.Add(BuildTopBar());
        var pages = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        Grid.SetRow(pages, 1);
        _overview.Children.Add(BuildOverview());
        _settings.Children.Add(BuildSettings());
        _historyPage.Children.Add(BuildHistory());
        pages.Children.Add(_overview);
        pages.Children.Add(_settings);
        pages.Children.Add(_historyPage);
        _settings.IsVisible = false;
        _historyPage.IsVisible = false;
        content.Children.Add(pages);
        main.Children.Add(content);
        Content = new Border { Child = main, Background = UiKit.AuroraGradient() };

        _coordinator.SnapshotChanged += (_, snapshot) => Dispatcher.UIThread.Post(() => UpdateStatus(snapshot));
        var timer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        timer.Tick += (_, _) => RefreshLists();
        timer.Start();
        RefreshLists();
        Opened += (_, _) => Localizer.TranslateTree(this);
        _ = UpdateStatusAsync();
    }

    private Control BuildSidebar()
    {
        var nav = new StackPanel { Spacing = 6, Margin = new Thickness(0, 24, 0, 0) };
        nav.Children.Add(Nav("Übersicht", true, () => ShowPage(0)));
        nav.Children.Add(Nav("Einstellungen", false, () => ShowPage(1)));
        nav.Children.Add(Nav("Verlauf", false, () => ShowPage(2)));

        var brand = UiKit.Horizontal(
            new Image { Source = Branding.Mark, Width = 42, Height = 42, Stretch = Stretch.Uniform },
            new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children = { UiKit.TextBlock("RhythmGuard", 17, FontWeight.SemiBold), UiKit.Caption("MENTAL RHYTHM") },
            });

        var userCard = UiKit.Horizontal(
            new Ellipse { Width = 34, Height = 34, Fill = UiKit.AccentGradient() },
            new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    _userName,
                    UiKit.Caption("Dein Rhythmus-System"),
                    UiKit.TextBlock("Powered by VisionGaiaTechnology", 10.5, FontWeight.Normal, UiKit.TextMuted),
                },
            });

        var stack = new StackPanel { Spacing = 0 };
        stack.Children.Add(brand);
        stack.Children.Add(nav);
        stack.Children.Add(new Panel { Height = 18 });
        stack.Children.Add(UiKit.Card(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                UiKit.Caption("AKTUELLER MODUS"),
                UiKit.TextBlock("Fokusphase", 14.5, FontWeight.SemiBold),
                _statusMode,
                _statusCountdown,
            },
        }, 16, 16));
        stack.Children.Add(new Panel { Height = 16 });
        stack.Children.Add(userCard);

        return UiKit.Card(stack, 18, 24);
    }

    private Button Nav(string text, bool active, Action click)
    {
        var button = UiKit.NavItem(Localizer.T(text), click);
        if (active)
        {
            button.Background = UiKit.Solid("#24FFFFFF");
        }

        return button;
    }

    private Control BuildTopBar()
    {
        var left = new StackPanel { Spacing = 4, Children = { _pageTitle, UiKit.Caption(DateTime.Now.ToString("dddd, d. MMMM yyyy")) } };
        var status = new Border
        {
            Background = UiKit.Solid("#2238D9A9"),
            BorderBrush = UiKit.Solid("#5538D9A9"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 7),
            Child = UiKit.TextBlock("FOKUS", 12, FontWeight.SemiBold, UiKit.Teal),
        };
        var pause = UiKit.Primary(Localizer.T("Pause jetzt"), () => _coordinator.PauseNow());
        var right = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
        right.Children.Add(status);
        right.Children.Add(pause);
        var grid = new Grid();
        grid.Children.Add(left);
        grid.Children.Add(right);
        return grid;
    }

    private Control BuildOverview()
    {
        var kpis = new Grid();
        kpis.ColumnDefinitions = new ColumnDefinitions("*,14,*,14,*,14,*");
        kpis.Children.Add(Kpi("NÄCHSTE PAUSE", _kpiNext, 0));
        kpis.Children.Add(Kpi("PAUSEN HEUTE", _kpiBreaks, 2));
        kpis.Children.Add(Kpi("ABENDMODUS", _kpiEvening, 4));
        kpis.Children.Add(Kpi("STREAK", _kpiStreak, 6));

        var middle = new Grid { Margin = new Thickness(0, 14, 0, 0) };
        middle.ColumnDefinitions = new ColumnDefinitions("2*,14,*");
        var chartCard = UiKit.Card(new StackPanel
        {
            Children =
            {
                UiKit.Horizontal(UiKit.TextBlock("Tagesrhythmus", 17, FontWeight.SemiBold), UiKit.Caption("07:00 — 23:00")),
                new Panel { Height = 8 },
                _chart,
            },
        });
        Grid.SetColumn(chartCard, 0);
        middle.Children.Add(chartCard);
        var ringCard = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children = { UiKit.TextBlock("Tagesziel", 17, FontWeight.SemiBold), _ring.CreateOverlay() },
        });
        Grid.SetColumn(ringCard, 2);
        middle.Children.Add(ringCard);

        var bottom = new Grid { Margin = new Thickness(0, 14, 0, 0) };
        bottom.ColumnDefinitions = new ColumnDefinitions("1.6*,14,*");
        var eventsCard = UiKit.Card(new StackPanel { Children = { UiKit.TextBlock("Letzte Ereignisse", 16, FontWeight.SemiBold), _events } });
        Grid.SetColumn(eventsCard, 0);
        bottom.Children.Add(eventsCard);
        var systemCard = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                UiKit.TextBlock("Systemstatus", 16, FontWeight.SemiBold),
                UiKit.TextBlock("Rhythmus-Engine · aktiv", 13, FontWeight.Normal, UiKit.TextSecondary),
                UiKit.TextBlock("Abendmodus · automatisch", 13, FontWeight.Normal, UiKit.TextSecondary),
                UiKit.TextBlock("Autostart · in Einstellungen", 13, FontWeight.Normal, UiKit.TextSecondary),
            },
        });
        Grid.SetColumn(systemCard, 2);
        bottom.Children.Add(systemCard);

        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
        stack.Children.Add(kpis);
        stack.Children.Add(middle);
        stack.Children.Add(bottom);
        return new ScrollViewer { Content = stack };
    }

    private Control Kpi(string title, TextBlock value, int column)
    {
        var card = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children = { UiKit.Caption(title), value },
        });
        Grid.SetColumn(card, column);
        return card;
    }

    private Control BuildSettings()
    {
        var rhythm = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                UiKit.TextBlock("Tagesrhythmus", 17, FontWeight.SemiBold),
                UiKit.Horizontal(UiKit.TextBlock("Aktives Fenster", 13), _dayStart, UiKit.TextBlock("–"), _dayEnd),
                UiKit.Label("Fokusphase (Minuten)"),
                _work,
                UiKit.Label("Pause (Minuten)"),
                _pause,
            },
        });

        var evening = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                UiKit.TextBlock("Abendmodus", 17, FontWeight.SemiBold),
                _eveningEnabled,
                UiKit.Label("Erinnerungs-Intervall (Minuten)"),
                _eveningInterval,
            },
        });

        var bed = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                UiKit.TextBlock("Bett-Modus", 17, FontWeight.SemiBold),
                UiKit.Label("Dauer bis Energie-Aktion (Minuten)"),
                _bed,
                UiKit.Label("Aktion danach"),
                new WrapPanel { Children = { _sleep, _hibernate, _soft, _hard } },
            },
        });

        var system = UiKit.Card(new StackPanel
        {
            Spacing = 8,
            Children =
            {
                UiKit.TextBlock("System", 17, FontWeight.SemiBold),
                _autostart,
                UiKit.Label("Notausstieg (ESC-Haltezeit in Sekunden, 0 = aus)"),
                _emergency,
                UiKit.Label("Sprache"),
                _language,
                _error,
                UiKit.Primary("Speichern", SaveSettings),
            },
        });

        var grid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        grid.ColumnDefinitions = new ColumnDefinitions("*,16,*");
        var left = new StackPanel { Spacing = 14, Children = { rhythm, evening } };
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);
        var right = new StackPanel { Spacing = 14, Children = { bed, system } };
        Grid.SetColumn(right, 2);
        grid.Children.Add(right);
        var scroll = new ScrollViewer { Content = grid };
        LoadSettings();
        return scroll;
    }

    private Control BuildHistory()
    {
        return new ScrollViewer
        {
            Content = UiKit.Card(new StackPanel { Spacing = 8, Children = { UiKit.TextBlock("Verlauf", 17, FontWeight.SemiBold), _history } }, 22),
        };
    }

    private void ShowPage(int index)
    {
        _overview.IsVisible = index == 0;
        _settings.IsVisible = index == 1;
        _historyPage.IsVisible = index == 2;
        _pageTitle.Text = index switch { 1 => "Einstellungen", 2 => "Verlauf", _ => "Übersicht" };
    }

    private async Task UpdateStatusAsync()
    {
        await Task.CompletedTask;
        UpdateStatus(_coordinator.CurrentSnapshot);
        RefreshLists();
    }

    private void UpdateStatus(RhythmSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        var mode = snapshot.Mode switch
        {
            RhythmMode.Focus => "FOKUS",
            RhythmMode.Break => "PAUSE",
            RhythmMode.Evening => "ABENDMODUS",
            RhythmMode.Bed => "BETT-MODUS",
            RhythmMode.BedExpired => "BEENDET",
            _ => "STATUS",
        };
        _statusMode.Text = mode;
        _statusCountdown.Text = Format(snapshot.Remaining);
        _kpiNext.Text = Format(snapshot.Remaining);
        var stats = _coordinator.GetStatistics();
        _kpiBreaks.Text = stats.TodayBreaksCompleted.ToString();
        _kpiStreak.Text = stats.StreakDays.ToString();
        _kpiEvening.Text = snapshot.Mode == RhythmMode.Evening ? "jetzt" : "in " + Format(TimeUntilEvening());
        _chart.Settings = _coordinator.Settings;
        var expected = Math.Max(1, (int)((_coordinator.Settings.DayEnd.ToTimeSpan() - _coordinator.Settings.DayStart.ToTimeSpan()).TotalMinutes / (_coordinator.Settings.WorkMinutes + _coordinator.Settings.BreakMinutes)));
        _ring.SetPercent(Math.Clamp((stats.TodayBreaksCompleted + 0.4) / expected - (stats.TodayBreaksAborted + stats.TodayEmergencyExits) * 0.08, 0, 1));
    }

    private TimeSpan TimeUntilEvening()
    {
        var evening = DateTime.Today + _coordinator.Settings.DayEnd.ToTimeSpan();
        if (_coordinator.CurrentSnapshot?.Mode == RhythmMode.Evening)
        {
            evening = DateTime.Today.AddDays(1) + _coordinator.Settings.DayEnd.ToTimeSpan();
        }

        return evening - DateTime.Now;
    }

    private void RefreshLists()
    {
        _events.Children.Clear();
        _history.Children.Clear();
        var recent = _coordinator.GetRecentEvents(6);
        foreach (var e in recent)
        {
            _events.Children.Add(UiKit.TextBlock($"{e.AtLocal:HH:mm} · {Describe(e)}", 12.5, FontWeight.Normal, UiKit.TextSecondary));
        }

        foreach (var e in _coordinator.GetRecentEvents(40))
        {
            _history.Children.Add(UiKit.TextBlock($"{e.AtLocal:yyyy-MM-dd HH:mm} · {Describe(e)}", 12.5, FontWeight.Normal, UiKit.TextSecondary));
        }
    }

    private static string Describe(RhythmEvent e) => e.Kind switch
    {
        RhythmEventKind.BreakCompleted => Localizer.T("Pause abgeschlossen"),
        RhythmEventKind.BreakStarted => Localizer.T("Pause gestartet"),
        RhythmEventKind.BreakAborted => Localizer.T("Pause abgebrochen"),
        RhythmEventKind.EveningReminderShown => Localizer.T("Abend-Erinnerung angezeigt"),
        RhythmEventKind.BedModeStarted => Localizer.T("Bett-Modus gestartet"),
        RhythmEventKind.BedModeCancelled => Localizer.T("Bett-Modus beendet"),
        RhythmEventKind.PowerActionExecuted => Localizer.T("Energie-Aktion ausgeführt"),
        RhythmEventKind.EmergencyExitUsed => Localizer.T("Notausstieg genutzt"),
        RhythmEventKind.SettingsChanged => Localizer.T("Einstellungen gespeichert"),
        RhythmEventKind.AppStarted => Localizer.T("RhythmGuard gestartet"),
        _ => e.Kind.ToString(),
    };

    private void LoadSettings()
    {
        var s = _coordinator.Settings;
        _dayStart.Text = s.DayStart.ToString("HH:mm");
        _dayEnd.Text = s.DayEnd.ToString("HH:mm");
        _work.Value = s.WorkMinutes;
        _pause.Value = s.BreakMinutes;
        _eveningInterval.Value = s.EveningReminderMinutes;
        _bed.Value = s.BedModeMinutes;
        _emergency.Value = s.EmergencyHoldSeconds;
        _eveningEnabled.IsChecked = s.EveningRemindersEnabled;
        _autostart.IsChecked = s.StartWithWindows;
        _sleep.IsChecked = s.BedModePowerAction == PowerAction.Sleep;
        _hibernate.IsChecked = s.BedModePowerAction == PowerAction.Hibernate;
        _soft.IsChecked = s.BedModePowerAction == PowerAction.ShutdownGraceful;
        _hard.IsChecked = s.BedModePowerAction == PowerAction.ShutdownHard;
        _language.SelectedIndex = s.UiLanguage switch { "de" => 1, "en" => 2, "es" => 3, "it" => 4, "ru" => 5, "zh" => 6, "ja" => 7, _ => 0 };
    }

    private void SaveSettings()
    {
        try
        {
            if (!TimeOnly.TryParseExact(_dayStart.Text?.Trim(), new[] { "H:mm", "HH:mm" }, out var start)
                || !TimeOnly.TryParseExact(_dayEnd.Text?.Trim(), new[] { "H:mm", "HH:mm" }, out var end))
            {
                _error.Text = "Zeitformat HH:mm erforderlich.";
                return;
            }

            var action = _hibernate.IsChecked == true ? PowerAction.Hibernate
                : _soft.IsChecked == true ? PowerAction.ShutdownGraceful
                : _hard.IsChecked == true ? PowerAction.ShutdownHard
                : PowerAction.Sleep;
            var language = _language.SelectedIndex switch { 1 => "de", 2 => "en", 3 => "es", 4 => "it", 5 => "ru", 6 => "zh", 7 => "ja", _ => "system" };

            var settings = _coordinator.Settings with
            {
                DayStart = start,
                DayEnd = end,
                WorkMinutes = (int)_work.Value,
                BreakMinutes = (int)_pause.Value,
                EveningReminderMinutes = (int)_eveningInterval.Value,
                EveningRemindersEnabled = _eveningEnabled.IsChecked == true,
                BedModeMinutes = (int)_bed.Value,
                BedModePowerAction = action,
                EmergencyHoldSeconds = (int)_emergency.Value,
                StartWithWindows = _autostart.IsChecked == true,
                UiLanguage = language,
            };

            _coordinator.ApplySettings(settings);
            Localizer.Initialize(language);
            _error.Text = "Gespeichert.";
        }
        catch (RhythmValidationException ex)
        {
            _error.Text = ex.Message;
        }
    }

    private static Slider Slider(double min, double max, double value) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        TickFrequency = Math.Max(1, (max - min) / 20),
        IsSnapToTickEnabled = true,
    };

    private static string Format(TimeSpan remaining)
    {
        var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        return clamped.TotalHours >= 1 ? clamped.ToString(@"h\:mm\:ss") : clamped.ToString(@"mm\:ss");
    }

    private static string FormatUser(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Profil";
        }

        var cleaned = name.Replace('.', ' ').Replace('_', ' ').Trim();
        return cleaned.Length == 0 ? "Profil" : char.ToUpper(cleaned[0]) + cleaned[1..];
    }
}
