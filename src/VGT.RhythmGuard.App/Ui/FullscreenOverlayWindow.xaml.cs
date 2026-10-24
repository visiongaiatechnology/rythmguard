using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using VGT.RhythmGuard.App.Localization;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VGT.RhythmGuard.Core;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace VGT.RhythmGuard.App.Ui;

internal enum OverlayKind
{
    Break = 0,
    ShutdownWarning = 1,
}

/// <summary>Vollbild-Overlay für erzwungene Pausen und Shutdown-Warnung (pro Monitor).</summary>
internal partial class FullscreenOverlayWindow : Window
{
    private static readonly string[] Suggestions =
    {
        "Augen schließen und zehnmal tief durchatmen.",
        "Ein Glas Wasser holen und langsam trinken.",
        "Kurz aufstehen, Schultern kreisen, Fenster öffnen.",
        "Fünf Minuten nach draußen, ohne Telefon.",
        "Zehn ruhige Atemzüge: 4 ein, 6 aus.",
        "Kurz dehnen: Nacken, Rücken, Beine.",
    };

    private static readonly TimeSpan BodyBreakDelay = TimeSpan.FromSeconds(10);

    private readonly OverlayKind _kind;
    private readonly DispatcherTimer _emergencyTimer;
    private readonly Stopwatch _emergencyWatch = new();
    private int _emergencyHoldSeconds;
    private bool _emergencyEnabled;
    private bool _closedByOwner;
    private bool _confirmOpen;
    private string? _lastSuggestion;

    public FullscreenOverlayWindow(OverlayKind kind, System.Windows.Forms.Screen screen, double scale, bool captureDesktop = true)
    {
        _kind = kind;
        InitializeComponent();
        Localizer.EnableAutoTranslation(this);

        var bounds = screen.Bounds;
        Left = bounds.Left / scale;
        Top = bounds.Top / scale;
        Width = bounds.Width / scale;
        Height = bounds.Height / scale;

        BackdropImage.Source = captureDesktop ? CaptureScreen(bounds) : null;

        _emergencyTimer = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromMilliseconds(60),
        };
        _emergencyTimer.Tick += (_, _) => UpdateEmergencyHold();

        PreviewKeyDown += OnPreviewKeyDown;
        PreviewKeyUp += OnPreviewKeyUp;
    }

    public event EventHandler? EmergencyExitRequested;

    public void ActivateWindow()
    {
        Activate();
        Focus();
        Keyboard.Focus(this);
    }

    public void ApplyBreak(BreakDisplay display)
    {
        if (_kind != OverlayKind.Break)
        {
            return;
        }

        _emergencyEnabled = display.EmergencyExitEnabled && display.EmergencyHoldSeconds > 0;
        _emergencyHoldSeconds = display.EmergencyHoldSeconds;

        ModeCaption.Text = Localizer.T(display.IsManual ? "DEINE PAUSE" : "ERZWUNGENE PAUSE");
        TitleText.Text = Localizer.T(display.Remaining <= BodyBreakDelay ? "Jetzt ist Pause." : "Zeit für dich.");
        CountdownText.Text = FormatCountdown(display.Remaining);
        SubtitleText.Text = display.IsManual
            ? Localizer.T("Du hast die Pause selbst gestartet. Bleib dabei.")
            : Localizer.T("Der Bildschirm bleibt aus. Steh auf, atme, trink Wasser.");
        SuggestionPanel.Visibility = Visibility.Visible;
        HintText.Visibility = Visibility.Visible;
        HintText.Text = "VGT RythmGuard · Protect Your Rhythm. Protect Yourself.";
        EmergencyPanel.Visibility = _emergencyEnabled ? Visibility.Visible : Visibility.Collapsed;

        ProgressFill.Width = Math.Max(0, (ProgressTrack.ActualWidth > 0 ? ProgressTrack.ActualWidth : 520) * Math.Clamp(display.Progress, 0d, 1d));

        if (display.Remaining <= BodyBreakDelay && _lastSuggestion != "Aufstehen. Der PC kann warten.")
        {
            _lastSuggestion = "Aufstehen. Der PC kann warten.";
            SuggestionText.Text = Localizer.T(_lastSuggestion);
            return;
        }

        var minuteBucket = (int)Math.Floor(display.Remaining.TotalMinutes);
        var suggestion = Suggestions[Math.Abs(minuteBucket) % Suggestions.Length];
        if (suggestion != _lastSuggestion)
        {
            _lastSuggestion = suggestion;
            SuggestionText.Text = Localizer.T(suggestion);
        }
    }

    public void ApplyShutdown(ShutdownWarningDisplay display)
    {
        if (_kind != OverlayKind.ShutdownWarning)
        {
            return;
        }

        ModeCaption.Text = Localizer.T("SICHERHEITSMODUS");
        TitleText.Text = Localizer.T("Der PC wird gleich heruntergefahren.");
        CountdownText.Text = Math.Max(0, display.Seconds).ToString("00", CultureInfo.InvariantCulture) + " s";
        SubtitleText.Text = Localizer.T("Bitte speichere deine Arbeit. Danach fährt der Rechner herunter.");
        SuggestionPanel.Visibility = Visibility.Collapsed;
        EmergencyPanel.Visibility = Visibility.Collapsed;
        HintText.Text = "VGT RythmGuard · Bett-Modus ist abgelaufen.";
        ProgressFill.Width = Math.Max(0, (ProgressTrack.ActualWidth > 0 ? ProgressTrack.ActualWidth : 520) * (1d - Math.Clamp(display.Seconds / 60d, 0d, 1d)));
    }

    public void CloseWithoutNotification()
    {
        _closedByOwner = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_closedByOwner && _kind == OverlayKind.Break)
        {
            e.Cancel = true;
            return;
        }

        _emergencyTimer.Stop();
        base.OnClosing(e);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !_emergencyEnabled || _confirmOpen)
        {
            return;
        }

        e.Handled = true;
        if (!_emergencyWatch.IsRunning)
        {
            _emergencyWatch.Restart();
            _emergencyTimer.Start();
        }
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !_emergencyEnabled)
        {
            return;
        }

        e.Handled = true;
        ResetEmergencyHold();
    }

    private void UpdateEmergencyHold()
    {
        if (_emergencyHoldSeconds <= 0)
        {
            _emergencyTimer.Stop();
            return;
        }

        var ratio = Math.Clamp(_emergencyWatch.Elapsed.TotalSeconds / _emergencyHoldSeconds, 0d, 1d);
        EmergencyProgressFill.Width = Math.Max(0, 96 * ratio);
        if (ratio < 1d)
        {
            return;
        }

        ResetEmergencyHold();
        _confirmOpen = true;
        try
        {
            var confirm = new EmergencyConfirmWindow { Owner = this };
            var accepted = confirm.ShowDialog() == true;
            if (accepted)
            {
                EmergencyExitRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        finally
        {
            _confirmOpen = false;
        }
    }

    private void ResetEmergencyHold()
    {
        _emergencyWatch.Reset();
        _emergencyTimer.Stop();
        EmergencyProgressFill.Width = 0;
    }

    private static string FormatCountdown(TimeSpan remaining)
    {
        var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        return clamped.TotalHours >= 1
            ? clamped.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : clamped.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    private static BitmapSource? CaptureScreen(System.Drawing.Rectangle bounds)
    {
        try
        {
            using var bitmap = new System.Drawing.Bitmap(
                Math.Max(1, bounds.Width),
                Math.Max(1, bounds.Height),
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bitmap.Size);
            }

            var handle = bitmap.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    handle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                _ = DeleteObject(handle);
            }
        }
        catch (Exception ex) when (ex is Win32Exception or ArgumentException or ExternalException)
        {
            return null;
        }
    }

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr handle);
}

