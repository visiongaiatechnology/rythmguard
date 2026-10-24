using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using VGT.RhythmGuard.App.Localization;
using VGT.RhythmGuard.Core;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Path = System.Windows.Shapes.Path;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;
using RadioButton = System.Windows.Controls.RadioButton;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;
using TextBox = System.Windows.Controls.TextBox;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;

namespace VGT.RhythmGuard.App;

internal enum DashboardPage
{
    Overview = 0,
    Settings = 1,
    History = 2,
}

/// <summary>Aurora-Glass-Dashboard: Übersicht, Einstellungen und Verlauf in einer Shell.</summary>
internal partial class SettingsWindow : Window
{
    private readonly RhythmCoordinator? _coordinator;
    private readonly bool _previewMode;
    private RhythmSettings _settings;
    private DashboardPage _currentPage = DashboardPage.Overview;

    public SettingsWindow(RhythmCoordinator coordinator)
    {
        _coordinator = coordinator;
        _settings = coordinator.Settings;
        InitializeComponent();

        DateCaption.Text = DateTime.Now.ToString("dddd, d. MMMM yyyy", CultureInfo.GetCultureInfo("de-DE"));
        VersionText.Text = "v" + (typeof(SettingsWindow).Assembly.GetName().Version?.ToString(3) ?? "1.2.0");
        UserNameText.Text = FormatUserName(Environment.UserName);
        Localizer.EnableAutoTranslation(this);
        LoadSettings(coordinator.Settings);
        AutostartCheck.IsChecked = coordinator.IsAutostartEnabled;
        UpdateStatus(coordinator.CurrentSnapshot);
        UpdateStatistics();
        RefreshEvents();
        RefreshHistory();

        coordinator.SnapshotChanged += OnSnapshotChanged;
        coordinator.SettingsChanged += OnCoordinatorSettingsChanged;
        Loaded += (_, _) =>
        {
            RedrawVisuals();
            RefreshEvents();
        };
        WindowRoot.SizeChanged += (_, _) => RedrawVisuals();
        Closed += (_, _) =>
        {
            coordinator.SnapshotChanged -= OnSnapshotChanged;
            coordinator.SettingsChanged -= OnCoordinatorSettingsChanged;
        };
    }

    /// <summary>Vorschau-Konstruktor für Screenshot-QA (keine Persistenz, kein Enforcement).</summary>
    public SettingsWindow()
    {
        _previewMode = true;
        _settings = RhythmSettings.Default;
        InitializeComponent();

        DateCaption.Text = DateTime.Now.ToString("dddd, d. MMMM yyyy", CultureInfo.GetCultureInfo("de-DE"));
        VersionText.Text = "v" + (typeof(SettingsWindow).Assembly.GetName().Version?.ToString(3) ?? "1.2.0");
        UserNameText.Text = FormatUserName(Environment.UserName);
        Localizer.EnableAutoTranslation(this);
        LoadSettings(_settings);
        AutostartCheck.IsChecked = true;
        UpdatePreviewData();
        Loaded += (_, _) => RedrawVisuals();
        WindowRoot.SizeChanged += (_, _) => RedrawVisuals();
    }

    /// <summary>Schaltet die Vorschau-Seite für Screenshot-QA um.</summary>
    internal void ShowPageForScreenshot(DashboardPage page)
    {
        switch (page)
        {
            case DashboardPage.Settings:
                NavSettings.IsChecked = true;
                break;
            case DashboardPage.History:
                NavHistory.IsChecked = true;
                break;
            default:
                NavOverview.IsChecked = true;
                break;
        }
    }

    /// <summary>Erzwingt Chart-/Ring-Rendering ohne sichtbares Fenster (QA).</summary>
    internal void RedrawForScreenshot()
    {
        RedrawVisuals();
        RefreshEvents();
        RefreshHistory();
    }

    private void OnNavChecked(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized || sender is not RadioButton { Tag: string tag })
        {
            return;
        }

        var page = tag switch
        {
            "settings" => DashboardPage.Settings,
            "history" => DashboardPage.History,
            _ => DashboardPage.Overview,
        };

        ShowPage(page);
    }

    private void ShowPage(DashboardPage page)
    {
        _currentPage = page;
        PageOverview.Visibility = page == DashboardPage.Overview ? Visibility.Visible : Visibility.Collapsed;
        PageSettings.Visibility = page == DashboardPage.Settings ? Visibility.Visible : Visibility.Collapsed;
        PageHistory.Visibility = page == DashboardPage.History ? Visibility.Visible : Visibility.Collapsed;
        PageTitleText.Text = page switch
        {
            DashboardPage.Settings => "Einstellungen",
            DashboardPage.History => "Verlauf",
            _ => "Übersicht",
        };

        if (page == DashboardPage.Overview)
        {
            RedrawVisuals();
        }
    }

    private void OnSnapshotChanged(object? sender, RhythmSnapshot snapshot) => UpdateStatus(snapshot);

    private void OnCoordinatorSettingsChanged(object? sender, EventArgs e)
    {
        if (_coordinator is null)
        {
            return;
        }

        _settings = _coordinator.Settings;
        LoadSettings(_settings);
        UpdateStatistics();
        RefreshEvents();
        RefreshHistory();
    }

    private void LoadSettings(RhythmSettings settings)
    {
        DayStartBox.Text = settings.DayStart.ToString("HH:mm", CultureInfo.InvariantCulture);
        DayEndBox.Text = settings.DayEnd.ToString("HH:mm", CultureInfo.InvariantCulture);
        WorkSlider.Value = settings.WorkMinutes;
        BreakSlider.Value = settings.BreakMinutes;
        EveningSlider.Value = settings.EveningReminderMinutes;
        EveningEnabledCheck.IsChecked = settings.EveningRemindersEnabled;
        BedSlider.Value = settings.BedModeMinutes;
        EmergencySlider.Value = settings.EmergencyHoldSeconds;
        AutostartCheck.IsChecked = settings.StartWithWindows;

        ActionSleep.IsChecked = settings.BedModePowerAction == PowerAction.Sleep;
        ActionHibernate.IsChecked = settings.BedModePowerAction == PowerAction.Hibernate;
        ActionShutdownSoft.IsChecked = settings.BedModePowerAction == PowerAction.ShutdownGraceful;
        ActionShutdownHard.IsChecked = settings.BedModePowerAction == PowerAction.ShutdownHard;

        UpdateValueLabels();
        UpdateEmergencyHint();
        SelectLanguage(settings.UiLanguage);
    }

    private void SelectLanguage(string? code)
    {
        var normalized = string.IsNullOrWhiteSpace(code) ? "system" : code;
        foreach (var item in LanguageCombo.Items)
        {
            if (item is ComboBoxItem { Tag: string tag } && string.Equals(tag, normalized, StringComparison.OrdinalIgnoreCase))
            {
                LanguageCombo.SelectedItem = item;
                return;
            }
        }

        LanguageCombo.SelectedIndex = 0;
    }

    private string GetSelectedLanguage()
        => LanguageCombo.SelectedItem is ComboBoxItem { Tag: string tag } ? tag : "system";

    private static string FormatUserName(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return "Profil";
        }

        var cleaned = userName.Replace('.', ' ').Replace('_', ' ').Trim();
        return cleaned.Length == 0 ? "Profil" : char.ToUpper(cleaned[0], CultureInfo.CurrentCulture) + cleaned[1..];
    }

    private void OnBrandNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // Kein Standardbrowser verfügbar: Link bleibt ohne Aktion.
        }
    }

    private void UpdateStatus(RhythmSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        TopStatusText.Text = snapshot.Mode switch
        {
            RhythmMode.Focus => "FOKUS",
            RhythmMode.Break => "PAUSE",
            RhythmMode.Evening => "ABENDMODUS",
            RhythmMode.Bed => "BETT-MODUS",
            RhythmMode.BedExpired => "BEENDET",
            _ => "STATUS",
        };
        SidebarModeText.Text = snapshot.Mode switch
        {
            RhythmMode.Focus => "Fokusphase",
            RhythmMode.Break => "Erzwungene Pause",
            RhythmMode.Evening => "Abendmodus",
            RhythmMode.Bed => "Bett-Modus",
            RhythmMode.BedExpired => "Bett-Modus beendet",
            _ => "Rhythmus",
        };
        SidebarCountdownText.Text = snapshot.Mode switch
        {
            RhythmMode.Focus => "Nächste Pause: " + FormatRemaining(snapshot.Remaining),
            RhythmMode.Break => "Pause endet in " + FormatRemaining(snapshot.Remaining),
            RhythmMode.Evening => "Tag beginnt in " + FormatRemaining(snapshot.Remaining),
            RhythmMode.Bed => "Ruhemodus in " + FormatRemaining(snapshot.Remaining),
            _ => "Energie-Aktion läuft",
        };
        KpiNextBreakText.Text = FormatRemaining(snapshot.Remaining);
        KpiNextBreakCaption.Text = snapshot.Mode switch
        {
            RhythmMode.Focus => "danach " + BreakValueText.Text + " Ruhe",
            RhythmMode.Break => "Bildschirm bleibt aus",
            RhythmMode.Evening => "Tag beginnt um " + DayStartBox.Text,
            RhythmMode.Bed => "danach Energie-Aktion",
            _ => "Energie-Aktion läuft",
        };
        KpiEveningText.Text = snapshot.Mode == RhythmMode.Evening ? "jetzt" : "in " + FormatHoursMinutes(TimeUntilEvening(snapshot));
        KpiEveningCaption.Text = EveningEnabledCheck.IsChecked == true
            ? "Erinnerungen alle " + EveningValueText.Text
            : "Erinnerungen deaktiviert";

        _ = snapshot;

        TopStatusPill.Background = new SolidColorBrush(snapshot.Mode switch
        {
            RhythmMode.Break => Color.FromArgb(34, 76, 201, 240),
            RhythmMode.Evening => Color.FromArgb(34, 201, 167, 255),
            RhythmMode.Bed or RhythmMode.BedExpired => Color.FromArgb(34, 247, 37, 133),
            _ => Color.FromArgb(34, 79, 227, 193),
        });
        TopStatusDot.Fill = new SolidColorBrush(snapshot.Mode switch
        {
            RhythmMode.Break => Color.FromArgb(255, 76, 201, 240),
            RhythmMode.Evening => Color.FromArgb(255, 201, 167, 255),
            RhythmMode.Bed or RhythmMode.BedExpired => Color.FromArgb(255, 247, 37, 133),
            _ => Color.FromArgb(255, 79, 227, 193),
        });

        StatusEveningDot.Fill = new SolidColorBrush(snapshot.IsEvening ? Color.FromArgb(255, 201, 167, 255) : Color.FromArgb(255, 132, 125, 171));
        StatusEveningText.Text = snapshot.IsEvening ? "aktiv" : "inaktiv";
        StatusBedDot.Fill = new SolidColorBrush(snapshot.Mode is RhythmMode.Bed or RhythmMode.BedExpired ? Color.FromArgb(255, 247, 37, 133) : Color.FromArgb(255, 132, 125, 171));
        StatusBedText.Text = snapshot.Mode is RhythmMode.Bed or RhythmMode.BedExpired ? "aktiv" : "inaktiv";

        RedrawVisuals();
        UpdateProgressBars(snapshot);
    }

    private void UpdateProgressBars(RhythmSnapshot snapshot)
    {
        var width = WindowRoot.ActualWidth > 300 ? WindowRoot.ActualWidth - 700 : 600;
        var kpiWidth = width / 4.0 - 70;
        KpiBreakProgressFill.Width = Math.Max(0, kpiWidth * Math.Clamp(snapshot.PhaseProgress, 0d, 1d));
        SidebarProgressFill.Width = Math.Max(0, 170 * Math.Clamp(snapshot.PhaseProgress, 0d, 1d));
    }

    private void UpdateStatistics()
    {
        RhythmStatistics stats;
        if (_coordinator is null)
        {
            stats = new RhythmStatistics
            {
                TodayBreaksCompleted = 3,
                WeekBreaksCompleted = 11,
                TodayBreaksAborted = 0,
                TodayEmergencyExits = 0,
                TodayEveningReminders = 2,
                StreakDays = 4,
            };
        }
        else
        {
            stats = _coordinator.GetStatistics();
        }

        KpiBreaksTodayText.Text = stats.TodayBreaksCompleted.ToString(CultureInfo.InvariantCulture);
        KpiBreaksTodayCaption.Text = "diese Woche: " + stats.WeekBreaksCompleted.ToString(CultureInfo.InvariantCulture);
        KpiStreakText.Text = stats.StreakDays.ToString(CultureInfo.InvariantCulture);
        KpiStreakCaption.Text = stats.StreakDays == 1 ? "Tag mit Pausen" : "Tage mit Pausen";
        MetricBreaksText.Text = stats.TodayBreaksCompleted + " / " + ExpectedBreaks();
        MetricAbortedText.Text = (stats.TodayBreaksAborted + stats.TodayEmergencyExits).ToString(CultureInfo.InvariantCulture);
        MetricRemindersText.Text = stats.TodayEveningReminders.ToString(CultureInfo.InvariantCulture);
    }

    private int ExpectedBreaks()
    {
        var dayMinutes = _settings.DayEnd.ToTimeSpan() - _settings.DayStart.ToTimeSpan();
        var cycle = _settings.WorkMinutes + _settings.BreakMinutes;
        return Math.Max(1, (int)(dayMinutes.TotalMinutes / cycle));
    }

    private void UpdatePreviewData()
    {
        var sampleSnapshot = RhythmEngine.Evaluate(DateTime.Today.AddHours(10), _settings);
        UpdateStatus(sampleSnapshot);
        UpdateStatistics();

        var now = DateTime.Now;
        var events = new List<RhythmEvent>
        {
            RhythmEvent.Create(RhythmEventKind.BreakCompleted, now.AddMinutes(-12)),
            RhythmEvent.Create(RhythmEventKind.BreakStarted, now.AddMinutes(-44)),
            RhythmEvent.Create(RhythmEventKind.BreakCompleted, now.AddHours(-2)),
            RhythmEvent.Create(RhythmEventKind.EveningReminderShown, now.AddHours(-3)),
            RhythmEvent.Create(RhythmEventKind.BedModeStarted, now.AddHours(-4)),
            RhythmEvent.Create(RhythmEventKind.BreakCompleted, now.AddHours(-5)),
        };
        FillEventList(EventsList, events, compact: true);
        FillEventList(HistoryList, events.Concat(new[]
        {
            RhythmEvent.Create(RhythmEventKind.SettingsChanged, now.AddDays(-1).AddHours(9)),
            RhythmEvent.Create(RhythmEventKind.BreakCompleted, now.AddDays(-1).AddHours(11)),
            RhythmEvent.Create(RhythmEventKind.AppStarted, now.AddDays(-1).AddHours(7)),
        }).ToList(), compact: false);

        KpiNextBreakText.Text = "01:24:10";
        KpiNextBreakCaption.Text = "danach 30 Min Ruhe";
        KpiBreaksTodayText.Text = "3";
        KpiBreaksTodayCaption.Text = "diese Woche: 11";
        KpiEveningText.Text = "in 6h 22m";
        KpiEveningCaption.Text = "Erinnerungen alle 20 Min";
        KpiStreakText.Text = "4";
        KpiStreakCaption.Text = "Tage mit Pausen";
    }

    private void RefreshEvents()
    {
        if (_coordinator is null)
        {
            return;
        }

        FillEventList(EventsList, _coordinator.GetRecentEvents(6), compact: true);
    }

    private void RefreshHistory()
    {
        if (_coordinator is null)
        {
            return;
        }

        FillEventList(HistoryList, _coordinator.GetRecentEvents(40), compact: false);
    }

    private void FillEventList(Panel target, IReadOnlyList<RhythmEvent> events, bool compact)
    {
        target.Children.Clear();
        if (events.Count == 0)
        {
            target.Children.Add(new TextBlock
            {
                Text = "Noch keine Ereignisse – dein Rhythmus beginnt mit der ersten Pause.",
                Style = (Style)FindResource("Text.Caption"),
                Margin = new Thickness(0, 6, 0, 0),
            });
            return;
        }

        foreach (var rhythmEvent in events)
        {
            var (glyph, color, text) = Describe(rhythmEvent);
            var row = new Grid { Margin = new Thickness(0, compact ? 5 : 7, 0, compact ? 5 : 7) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var circle = new Border
            {
                Width = compact ? 26 : 30,
                Height = compact ? 26 : 30,
                CornerRadius = new CornerRadius(9),
                Background = new SolidColorBrush(Color.FromArgb(30, color.R, color.G, color.B)),
                Child = new TextBlock
                {
                    Text = glyph,
                    FontSize = compact ? 12 : 13,
                    Foreground = new SolidColorBrush(color),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            Grid.SetColumn(circle, 0);
            row.Children.Add(circle);

            var label = new TextBlock
            {
                Text = text,
                Foreground = (Brush)FindResource("Brush.TextSecondary"),
                FontSize = compact ? 12.5 : 13.5,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            Grid.SetColumn(label, 1);
            row.Children.Add(label);

            var time = new TextBlock
            {
                Text = rhythmEvent.AtLocal.ToString("HH:mm", CultureInfo.InvariantCulture),
                Style = (Style)FindResource("Text.Caption"),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(time, 2);
            row.Children.Add(time);

            target.Children.Add(row);
        }
    }

    private static (string Glyph, Color Color, string Text) Describe(RhythmEvent rhythmEvent) => rhythmEvent.Kind switch
    {
        RhythmEventKind.BreakCompleted => ("✓", Color.FromRgb(79, 227, 193), "Pause abgeschlossen"),
        RhythmEventKind.BreakStarted => ("◷", Color.FromRgb(76, 201, 240), "Pause gestartet"),
        RhythmEventKind.BreakAborted => ("↺", Color.FromRgb(255, 122, 162), "Pause abgebrochen"),
        RhythmEventKind.EveningReminderShown => ("☾", Color.FromRgb(201, 167, 255), "Abend-Erinnerung angezeigt"),
        RhythmEventKind.BedModeStarted => ("⌂", Color.FromRgb(247, 37, 133), "Bett-Modus gestartet"),
        RhythmEventKind.BedModeCancelled => ("↺", Color.FromRgb(255, 122, 162), "Bett-Modus beendet"),
        RhythmEventKind.PowerActionExecuted => ("⏻", Color.FromRgb(255, 122, 162), "Energie-Aktion ausgeführt"),
        RhythmEventKind.EmergencyExitUsed => ("!", Color.FromRgb(255, 122, 162), "Notausstieg genutzt"),
        RhythmEventKind.SettingsChanged => ("⚙", Color.FromRgb(201, 167, 255), "Einstellungen gespeichert"),
        RhythmEventKind.AppStarted => ("◦", Color.FromRgb(185, 179, 220), "RythmGuard gestartet"),
        _ => ("·", Color.FromRgb(132, 125, 171), "Ereignis"),
    };

    private void RedrawVisuals()
    {
        DrawChart();
        DrawRing();
    }

    private void DrawChart()
    {
        var canvas = RhythmChartCanvas;
        var width = canvas.ActualWidth;
        var height = canvas.ActualHeight;
        if (width < 120 || height < 80)
        {
            return;
        }

        canvas.Children.Clear();

        var dayStart = _settings.DayStart;
        var dayEnd = _settings.DayEnd;
        var totalMinutes = (dayEnd.ToTimeSpan() - dayStart.ToTimeSpan()).TotalMinutes;
        if (totalMinutes <= 0)
        {
            return;
        }

        var paddingTop = 18d;
        var usable = height - paddingTop - 8d;
        var work = _settings.WorkMinutes;
        var cycle = _settings.WorkMinutes + _settings.BreakMinutes;

        double Intensity(double minutesOfDay)
        {
            var inCycle = minutesOfDay % cycle;
            return inCycle < work ? 0.82 + 0.18 * Math.Sin(minutesOfDay / 55.0) : 0.1 + 0.05 * Math.Sin(minutesOfDay / 30.0);
        }

        var points = new List<Point>();
        const int samples = 240;
        for (var index = 0; index <= samples; index++)
        {
            var minutes = totalMinutes * index / samples;
            var x = width * index / samples;
            var y = paddingTop + (1 - Math.Clamp(Intensity(minutes), 0, 1)) * usable;
            points.Add(new Point(x, y));
        }

        // Pausen-Bänder hinter der Kurve
        var cycleStart = 0d;
        while (cycleStart + work < totalMinutes)
        {
            var bandStart = width * (cycleStart + work) / totalMinutes;
            var bandEnd = width * Math.Min(totalMinutes, cycleStart + cycle) / totalMinutes;
            var band = new Rectangle
            {
                Width = Math.Max(2, bandEnd - bandStart),
                Height = height,
                Fill = new SolidColorBrush(Color.FromArgb(26, 79, 227, 193)),
                RadiusX = 6,
                RadiusY = 6,
            };
            Canvas.SetLeft(band, bandStart);
            Canvas.SetTop(band, 0);
            canvas.Children.Add(band);
            cycleStart += cycle;
        }

        // Fläche unter der Kurve
        var area = new StreamGeometry();
        using (var context = area.Open())
        {
            context.BeginFigure(new Point(0, height), true, true);
            foreach (var point in points)
            {
                context.LineTo(point, true, false);
            }

            context.LineTo(new Point(width, height), true, false);
        }

        area.Freeze();
        var areaPath = new Path
        {
            Data = area,
            Fill = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromArgb(96, 124, 92, 255), 0),
                    new(Color.FromArgb(26, 76, 201, 240), 0.7),
                    new(Color.FromArgb(0, 76, 201, 240), 1),
                },
                new Point(0, 0),
                new Point(0, 1)),
        };
        canvas.Children.Add(areaPath);

        // Linie
        var line = new Polyline
        {
            Points = new PointCollection(points),
            Stroke = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromRgb(124, 92, 255), 0),
                    new(Color.FromRgb(76, 201, 240), 0.6),
                    new(Color.FromRgb(79, 227, 193), 1),
                },
                new Point(0, 0),
                new Point(1, 0)),
            StrokeThickness = 2.6,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
        };
        canvas.Children.Add(line);

        // Jetzt-Marker
        var now = DateTime.Now.TimeOfDay;
        var nowMinutes = (now - dayStart.ToTimeSpan()).TotalMinutes;
        if (nowMinutes >= 0 && nowMinutes <= totalMinutes)
        {
            var x = width * nowMinutes / totalMinutes;
            var marker = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = paddingTop - 6,
                Y2 = height,
                Stroke = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
                StrokeThickness = 1.4,
                StrokeDashArray = new DoubleCollection { 3, 4 },
            };
            canvas.Children.Add(marker);

            var y = paddingTop + (1 - Math.Clamp(Intensity(nowMinutes), 0, 1)) * usable;
            var dot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Color.FromRgb(244, 242, 255)),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(124, 92, 255),
                    BlurRadius = 18,
                    ShadowDepth = 0,
                    Opacity = 0.95,
                },
            };
            Canvas.SetLeft(dot, x - 5);
            Canvas.SetTop(dot, y - 5);
            canvas.Children.Add(dot);

            var label = new TextBlock
            {
                Text = "JETZT",
                FontFamily = (FontFamily)FindResource("Font.Text"),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("Brush.TextMuted"),
            };
            Canvas.SetLeft(label, Math.Min(width - 44, x + 8));
            Canvas.SetTop(label, paddingTop - 14);
            canvas.Children.Add(label);
        }

        ChartAxisText1.Text = dayStart.ToString("HH:mm", CultureInfo.InvariantCulture);
        ChartAxisText2.Text = (dayStart.ToTimeSpan() + TimeSpan.FromMinutes(totalMinutes / 3d)).ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        ChartAxisText3.Text = (dayStart.ToTimeSpan() + TimeSpan.FromMinutes(totalMinutes / 3d * 2)).ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        ChartAxisText4.Text = dayEnd.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    private void DrawRing()
    {
        var canvas = RingCanvas;
        canvas.Children.Clear();

        const double size = 196;
        const double center = size / 2;
        const double radius = 80;
        var percent = CalculateDailyScore();

        var background = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
            StrokeThickness = 14,
        };
        Canvas.SetLeft(background, center - radius);
        Canvas.SetTop(background, center - radius);
        canvas.Children.Add(background);

        RingPercentText.Text = Math.Round(percent * 100) + "%";

        if (percent <= 0.01)
        {
            return;
        }

        var angle = percent * 360d;
        var startPoint = new Point(center, center - radius);
        var radians = (angle - 90) * Math.PI / 180d;
        var endPoint = new Point(center + radius * Math.Cos(radians), center + radius * Math.Sin(radians));

        var figure = new PathFigure { StartPoint = startPoint, IsClosed = false };
        figure.Segments.Add(new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            IsLargeArc = angle > 180,
            SweepDirection = SweepDirection.Clockwise,
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        var arc = new Path
        {
            Data = geometry,
            Stroke = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromRgb(124, 92, 255), 0),
                    new(Color.FromRgb(76, 201, 240), 0.6),
                    new(Color.FromRgb(79, 227, 193), 1),
                },
                new Point(0, 0),
                new Point(1, 1)),
            StrokeThickness = 14,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
        };
        canvas.Children.Add(arc);
    }

    private double CalculateDailyScore()
    {
        var stats = _coordinator?.GetStatistics();
        var completed = stats?.TodayBreaksCompleted ?? 3;
        var aborted = (stats?.TodayBreaksAborted ?? 0) + (stats?.TodayEmergencyExits ?? 0);
        var expected = Math.Max(1, ExpectedBreaks());
        var score = (completed + 0.4) / expected - aborted * 0.08;
        return Math.Clamp(score, 0d, 1d);
    }

    private void UpdateValueLabels()
    {
        WorkValueText.Text = FormatMinutes(WorkSlider.Value);
        BreakValueText.Text = FormatMinutes(BreakSlider.Value);
        EveningValueText.Text = FormatMinutes(EveningSlider.Value);
        BedValueText.Text = FormatMinutes(BedSlider.Value);
        EmergencyValueText.Text = EmergencySlider.Value <= 0
            ? "aus"
            : string.Create(CultureInfo.InvariantCulture, $"{EmergencySlider.Value:0} Sek");
        UpdateEmergencyHint();
    }

    private void UpdateEmergencyHint()
    {
        EmergencyHint.Text = EmergencySlider.Value <= 0
            ? "0 = kein Notausstieg (Hardcore): Eine laufende Pause kann nicht abgebrochen werden."
            : "Notausstieg ist verfügbar: ESC so lange halten, dann PAUSE eintippen.";
    }

    private static string FormatMinutes(double minutes)
        => string.Create(CultureInfo.InvariantCulture, $"{minutes:0} Min");

    private static string FormatRemaining(TimeSpan remaining)
    {
        var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        return clamped.TotalHours >= 1
            ? clamped.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : clamped.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    private static string FormatHoursMinutes(TimeSpan span)
    {
        var clamped = span < TimeSpan.Zero ? TimeSpan.Zero : span;
        return string.Create(CultureInfo.InvariantCulture, $"{(int)clamped.TotalHours}h {clamped.Minutes:00}m");
    }

    private TimeSpan TimeUntilEvening(RhythmSnapshot snapshot)
    {
        var evening = DateTime.Today + _settings.DayEnd.ToTimeSpan();
        var reference = snapshot.Mode == RhythmMode.Evening ? DateTime.Today.AddDays(1) + _settings.DayEnd.ToTimeSpan() : evening;
        return reference - DateTime.Now;
    }

    private void OnSliderChanged(object sender, RoutedEventArgs e)
    {
        // Während InitializeComponent feuern Slider/CheckBoxen, bevor alle Labels existieren.
        if (WorkValueText is null || BreakValueText is null || EveningValueText is null
            || BedValueText is null || EmergencyValueText is null || EmergencyHint is null)
        {
            return;
        }

        UpdateValueLabels();
        if (_coordinator is not null && _currentPage == DashboardPage.Overview && IsLoaded)
        {
            UpdateStatistics();
            RedrawVisuals();
        }
    }

    private void OnPauseNowClick(object sender, RoutedEventArgs e)
    {
        _coordinator?.PauseNow();
        UpdateStatus(_coordinator?.CurrentSnapshot);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (_previewMode || _coordinator is null)
        {
            return;
        }

        try
        {
            _settings = BuildSettings();
            _coordinator.ApplySettings(_settings);
            Localizer.Initialize(_settings.UiLanguage);
            Localizer.TranslateWindow(this);
            AutostartCheck.IsChecked = _coordinator.IsAutostartEnabled;
            UpdateStatistics();
            RefreshEvents();
            RefreshHistory();
            RedrawVisuals();
            ErrorText.Foreground = (Brush)FindResource("Brush.Mint");
            ErrorText.Text = "Gespeichert.";
        }
        catch (RhythmValidationException ex)
        {
            ErrorText.Foreground = (Brush)FindResource("Brush.Danger");
            ErrorText.Text = ex.Message;
        }
        catch (RhythmStorageException ex)
        {
            ErrorText.Foreground = (Brush)FindResource("Brush.Danger");
            ErrorText.Text = "Speichern fehlgeschlagen: " + ex.Message;
        }
    }

    private RhythmSettings BuildSettings()
    {
        if (!TimeOnly.TryParseExact(DayStartBox.Text.Trim(), new[] { "H:mm", "HH:mm" }, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dayStart))
        {
            throw new RhythmValidationException("Startzeit muss im Format HH:mm angegeben werden.");
        }

        if (!TimeOnly.TryParseExact(DayEndBox.Text.Trim(), new[] { "H:mm", "HH:mm" }, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dayEnd))
        {
            throw new RhythmValidationException("Endzeit muss im Format HH:mm angegeben werden.");
        }

        var powerAction = ActionHibernate.IsChecked == true
            ? PowerAction.Hibernate
            : ActionShutdownSoft.IsChecked == true
                ? PowerAction.ShutdownGraceful
                : ActionShutdownHard.IsChecked == true
                    ? PowerAction.ShutdownHard
                    : PowerAction.Sleep;

        return _settings with
        {
            DayStart = dayStart,
            DayEnd = dayEnd,
            WorkMinutes = (int)WorkSlider.Value,
            BreakMinutes = (int)BreakSlider.Value,
            EveningReminderMinutes = (int)EveningSlider.Value,
            EveningRemindersEnabled = EveningEnabledCheck.IsChecked == true,
            BedModeMinutes = (int)BedSlider.Value,
            BedModePowerAction = powerAction,
            EmergencyHoldSeconds = (int)EmergencySlider.Value,
            StartWithWindows = AutostartCheck.IsChecked == true,
            UiLanguage = GetSelectedLanguage(),
        };
    }

    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        StartNativeWindowDrag();
        e.Handled = true;
    }

    /// <summary>
    /// Fallback: Ziehen aus dem oberen 100-px-Bereich funktioniert überall außer auf
    /// interaktiven Controls. Das macht das randlose Fenster auch dann beweglich, wenn
    /// ein Klick ein Kind-Element trifft, das das Bubble-Event nicht weiterreicht.
    /// </summary>
    private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        var position = e.GetPosition(this);
        if (position.Y > 100 || IsInteractiveSource(e.OriginalSource as DependencyObject))
        {
            return;
        }

        StartNativeWindowDrag();
        e.Handled = true;
    }

    private void StartNativeWindowDrag()
    {
        // Nativer Fenster-Drag: funktioniert auch bei elevated gestarteten,
        // randlosen WPF-Fenstern zuverlässiger als DragMove().
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        _ = ReleaseCapture();
        _ = SendMessage(handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
    }

    private static bool IsInteractiveSource(DependencyObject? source)
    {
        for (var current = source; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is Button or TextBox or CheckBox or Slider or RadioButton or ScrollBar or ScrollViewer)
            {
                return true;
            }

            if (current is FrameworkElement { Name: "WindowRoot" })
            {
                break;
            }
        }

        return false;
    }

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HTCAPTION = 0x2;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam);

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
