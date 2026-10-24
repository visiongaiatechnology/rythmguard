using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using VGT.RhythmGuard.Core;
using VGT.RhythmGuard.Linux.Localization;

namespace VGT.RhythmGuard.Linux.Ui;

internal enum OverlayKind
{
    Break = 0,
    ShutdownWarning = 1,
}

/// <summary>Vollbild-Overlay (Avalonia) für Pausen und Shutdown-Warnungen. Pro Monitor eine Instanz.</summary>
internal sealed class OverlayWindow : Window
{
    private readonly OverlayKind _kind;
    private readonly TextBlock _caption;
    private readonly TextBlock _title;
    private readonly TextBlock _countdown;
    private readonly TextBlock _subtitle;
    private readonly TextBlock _suggestion;
    private readonly Border _progress;
    private readonly ProgressBar _emergencyBar;
    private readonly int _holdSeconds;
    private DispatcherTimer? _holdTimer;
    private DateTime _holdStarted;
    private bool _emergencyEnabled;

    public OverlayWindow(OverlayKind kind, int holdSeconds)
    {
        _kind = kind;
        _holdSeconds = holdSeconds;

        SystemDecorations = SystemDecorations.None;
        CanResize = false;
        Topmost = true;
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Background = Brushes.Black;

        _caption = UiKit.Caption("ERZWUNGENE PAUSE");
        _caption.HorizontalAlignment = HorizontalAlignment.Center;
        _title = UiKit.TextBlock("Zeit für dich.", 32, FontWeight.SemiBold);
        _title.HorizontalAlignment = HorizontalAlignment.Center;
        _countdown = UiKit.TextBlock("30:00", 68, FontWeight.Light);
        _countdown.HorizontalAlignment = HorizontalAlignment.Center;
        _subtitle = UiKit.TextBlock(string.Empty, 16, FontWeight.Normal, UiKit.TextSecondary);
        _subtitle.TextAlignment = TextAlignment.Center;
        _subtitle.MaxWidth = 460;
        _suggestion = UiKit.TextBlock(string.Empty, 13.5, FontWeight.Normal, UiKit.TextSecondary);

        _progress = new Border
        {
            Height = 4,
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = UiKit.AccentGradient(),
            CornerRadius = new CornerRadius(2),
        };

        _emergencyBar = new ProgressBar { Height = 4, Width = 120, Minimum = 0, Maximum = 1, Value = 0 };

        if (kind == OverlayKind.Break)
        {
            BuildBreakContent();
        }
        else
        {
            BuildShutdownContent();
        }

        Opened += (_, _) => WindowState = WindowState.FullScreen;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    public event EventHandler? EmergencyExitRequested;

    private void BuildBreakContent()
    {
        var stack = new StackPanel { Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        stack.Children.Add(LogoImage(42));
        stack.Children.Add(_caption);
        stack.Children.Add(_title);
        stack.Children.Add(_countdown);
        stack.Children.Add(_subtitle);
        stack.Children.Add(new Border
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0),
            Background = UiKit.Solid("#0DFFFFFF"),
            BorderBrush = UiKit.Solid("#1AFFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12, 8),
            Child = UiKit.Horizontal(new Ellipse { Width = 7, Height = 7, Fill = UiKit.Solid(UiKit.Gold), VerticalAlignment = VerticalAlignment.Center }, _suggestion),
        });
        stack.Children.Add(new Border
        {
            Height = 4,
            Width = 520,
            Margin = new Thickness(0, 16, 0, 0),
            Background = UiKit.Solid("#1FFFFFFF"),
            CornerRadius = new CornerRadius(2),
            Child = _progress,
        });
        stack.Children.Add(UiKit.Caption(Localizer.T("Notausstieg: ESC 5 Sekunden halten")));
        stack.Children.Add(_emergencyBar);
        stack.Children.Add(UiKit.Caption("VGT Rhythm Guard · Powered by VisionGaiaTechnology"));

        Content = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#1A1246"), 0),
                    new GradientStop(Color.Parse("#101A3A"), 0.6),
                    new GradientStop(Color.Parse("#1F1038"), 1),
                },
            },
            Child = new Border
            {
                Background = UiKit.Solid("#12FFFFFF"),
                BorderBrush = UiKit.Solid("#30FFFFFF"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(26),
                Padding = new Thickness(54, 44),
                MaxWidth = 640,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = stack,
            },
        };
    }

    private void BuildShutdownContent()
    {
        var stack = new StackPanel { Spacing = 12, VerticalAlignment = VerticalAlignment.Center };
        stack.Children.Add(LogoImage(40));
        stack.Children.Add(_caption);
        stack.Children.Add(_title);
        stack.Children.Add(_countdown);
        stack.Children.Add(_subtitle);
        stack.Children.Add(new Border
        {
            Height = 4,
            Width = 520,
            Margin = new Thickness(0, 18, 0, 0),
            Background = UiKit.Solid("#1FFFFFFF"),
            CornerRadius = new CornerRadius(2),
            Child = _progress,
        });

        Content = new Border
        {
            Background = UiKit.AuroraGradient(),
            Child = new Border
            {
                Background = UiKit.Solid("#E6140F2E"),
                BorderBrush = UiKit.Solid("#44FF7AA2"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(26),
                Padding = new Thickness(54, 44),
                MaxWidth = 640,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = stack,
            },
        };
    }

    public void ActivateWindow()
    {
        Show();
        Activate();
        Focus();
    }

    public void ApplyBreak(BreakDisplay display)
    {
        _emergencyEnabled = display.EmergencyExitEnabled && display.EmergencyHoldSeconds > 0;
        _caption.Text = Localizer.T(display.IsManual ? "DEINE PAUSE" : "ERZWUNGENE PAUSE");
        _title.Text = Localizer.T(display.Remaining <= TimeSpan.FromSeconds(10) ? "Jetzt ist Pause." : "Zeit für dich.");
        _countdown.Text = Format(display.Remaining);
        _subtitle.Text = Localizer.T(display.IsManual
            ? "Du hast die Pause selbst gestartet. Bleib dabei."
            : "Der Bildschirm bleibt aus. Steh auf, atme, trink Wasser.");
        _suggestion.Text = Localizer.T(display.Remaining.TotalMinutes < 10
            ? "Aufstehen. Der PC kann warten."
            : "Augen schließen und zehnmal tief durchatmen.");
        _progress.Width = 520 * Math.Clamp(display.Progress, 0d, 1d);
        _emergencyBar.IsVisible = _emergencyEnabled;
    }

    public void ApplyShutdown(ShutdownWarningDisplay display)
    {
        _caption.Text = Localizer.T("SICHERHEITSMODUS");
        _title.Text = Localizer.T("Der PC wird gleich heruntergefahren.");
        _countdown.Text = Math.Max(0, display.Seconds).ToString("00") + " s";
        _subtitle.Text = Localizer.T("Bitte speichere deine Arbeit. Danach fährt der Rechner herunter.");
        _progress.Width = 520 * (1d - Math.Clamp(display.Seconds / 60d, 0d, 1d));
        _emergencyBar.IsVisible = false;
    }

    public void CloseWithoutNotification() => Close();

    private static string Format(TimeSpan remaining)
    {
        var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        return clamped.TotalHours >= 1 ? clamped.ToString(@"h\:mm\:ss") : clamped.ToString(@"mm\:ss");
    }

    private static Image LogoImage(double height) => new()
    {
        Source = Branding.Wordmark,
        Height = height,
        Stretch = Stretch.Uniform,
        HorizontalAlignment = HorizontalAlignment.Center,
    };

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !_emergencyEnabled || _kind != OverlayKind.Break)
        {
            return;
        }

        e.Handled = true;
        if (_holdTimer is not null)
        {
            return;
        }

        _holdStarted = DateTime.UtcNow;
        _holdTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(60) };
        _holdTimer.Tick += (_, _) =>
        {
            var ratio = Math.Clamp((DateTime.UtcNow - _holdStarted).TotalSeconds / Math.Max(1, _holdSeconds), 0d, 1d);
            _emergencyBar.Value = ratio;
            if (ratio < 1d)
            {
                return;
            }

            StopHold();
            _ = ConfirmEmergencyAsync();
        };
        _holdTimer.Start();
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            StopHold();
        }
    }

    private void StopHold()
    {
        _holdTimer?.Stop();
        _holdTimer = null;
        _emergencyBar.Value = 0;
    }

    private async Task ConfirmEmergencyAsync()
    {
        var confirm = new EmergencyConfirmWindow();
        var accepted = await confirm.ShowDialog<bool>(this);
        if (accepted)
        {
            EmergencyExitRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

internal sealed class EmergencyConfirmWindow : Window
{
    public EmergencyConfirmWindow()
    {
        Title = "Notausstieg bestätigen";
        Width = 520;
        Height = 320;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SystemDecorations = SystemDecorations.BorderOnly;
        CanResize = false;
        Topmost = true;
        Background = UiKit.Solid("#150F33");

        var input = new TextBox { Watermark = "PAUSE", Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        var ok = UiKit.Primary("Notausstieg bestätigen", () =>
        {
            if (string.Equals(input.Text?.Trim(), "PAUSE", StringComparison.OrdinalIgnoreCase))
            {
                Close(true);
            }
        });
        var cancel = UiKit.Secondary("Weitermachen", () => Close(false));

        Content = UiKit.Card(new StackPanel
        {
            Spacing = 12,
            Children =
            {
                UiKit.Caption("NOTAUSSTIEG"),
                UiKit.TextBlock("Pause wirklich abbrechen?", 24, FontWeight.SemiBold),
                UiKit.TextBlock("Der Notausstieg ist für echte Ausnahmen gedacht.", 13.5, FontWeight.Normal, UiKit.TextSecondary),
                UiKit.Caption(Localizer.T("Tippe PAUSE zur Bestätigung:")),
                input,
                UiKit.Horizontal(cancel, ok),
            },
        }, 28);
    }
}

/// <summary>Geladene Marken-Assets (Bitmap einmalig).</summary>
internal static class Branding
{
    public static readonly Avalonia.Media.Imaging.Bitmap Mark = Load("logo-mark.png");
    public static readonly Avalonia.Media.Imaging.Bitmap Wordmark = Load("logo-wordmark.png");

    private static Avalonia.Media.Imaging.Bitmap Load(string name)
        => new(AssetLoader.Open(new Uri($"avares://VGT.RhythmGuard/Assets/{name}")));
}
