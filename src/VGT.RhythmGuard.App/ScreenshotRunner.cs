using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VGT.RhythmGuard.App.Infrastructure;
using VGT.RhythmGuard.App.Ui;
using VGT.RhythmGuard.Core;
using Size = System.Windows.Size;

namespace VGT.RhythmGuard.App;

/// <summary>
/// Erzeugt echte Render-Screenshots aller zentralen Oberflächen ohne Enforcement.
/// Wird ausschließlich über `--screenshot &lt;verzeichnis&gt;` aufgerufen.
/// </summary>
internal static class ScreenshotRunner
{
    public static void CaptureAll(string directory, AppLogger logger)
    {
        Directory.CreateDirectory(directory);
        try
        {
            CaptureBreakOverlay(Path.Combine(directory, "01-break-overlay.png"));
            CaptureShutdownOverlay(Path.Combine(directory, "02-shutdown-warning.png"));
            CaptureEveningReminder(Path.Combine(directory, "03-evening-reminder.png"));
            CaptureBedCountdown(Path.Combine(directory, "04-bed-countdown.png"));
            CaptureBedWarning(Path.Combine(directory, "05-bed-warning.png"));
            CaptureDashboard(Path.Combine(directory, "06-dashboard-overview.png"), DashboardPage.Overview);
            CaptureDashboard(Path.Combine(directory, "07-dashboard-settings.png"), DashboardPage.Settings);
            CaptureDashboard(Path.Combine(directory, "08-dashboard-history.png"), DashboardPage.History);
            CaptureSplash(Path.Combine(directory, "09-splash.png"));
            logger.Info("Screenshots erzeugt: " + directory);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.Runtime.InteropServices.ExternalException)
        {
            logger.Error("Screenshot-Erzeugung fehlgeschlagen.", ex);
        }
    }

    private static void CaptureDashboard(string path, DashboardPage page)
    {
        const int width = 1440;
        const int height = 900;
        var window = new SettingsWindow();
        window.ShowPageForScreenshot(page);

        var root = (FrameworkElement)window.Content;
        root.Width = width;
        root.Height = height;
        root.Measure(new Size(width, height));
        root.Arrange(new Rect(0, 0, width, height));
        root.UpdateLayout();

        window.RedrawForScreenshot();
        root.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(root);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void CaptureBreakOverlay(string path)
    {
        var screen = System.Windows.Forms.Screen.PrimaryScreen ?? System.Windows.Forms.Screen.AllScreens[0];
        var window = new FullscreenOverlayWindow(OverlayKind.Break, screen, DpiHelper.PrimaryScale, captureDesktop: false);
        window.ApplyBreak(new BreakDisplay(
            Remaining: TimeSpan.FromMinutes(29) + TimeSpan.FromSeconds(12),
            EndsAt: DateTime.Now.AddMinutes(29),
            Progress: 0.08,
            EmergencyExitEnabled: true,
            EmergencyHoldSeconds: 5,
            IsManual: false));

        CaptureElement((FrameworkElement)window.Content, 1600, 900, path);
    }

    private static void CaptureShutdownOverlay(string path)
    {
        var screen = System.Windows.Forms.Screen.PrimaryScreen ?? System.Windows.Forms.Screen.AllScreens[0];
        var window = new FullscreenOverlayWindow(OverlayKind.ShutdownWarning, screen, DpiHelper.PrimaryScale, captureDesktop: false);
        window.ApplyShutdown(new ShutdownWarningDisplay(47, PowerAction.ShutdownGraceful));
        CaptureElement((FrameworkElement)window.Content, 1600, 900, path);
    }

    private static void CaptureEveningReminder(string path)
    {
        var window = new EveningReminderWindow();
        window.Apply(new EveningReminderDisplay(DateTime.Today.AddHours(23).AddMinutes(2), 120));
        CaptureAutoSized(window, path);
    }

    private static void CaptureBedCountdown(string path)
    {
        var window = new BedCountdownWindow();
        window.Apply(new BedCountdownDisplay(TimeSpan.FromMinutes(118), PowerAction.Sleep));
        CaptureAutoSized(window, path);
    }

    private static void CaptureBedWarning(string path)
    {
        var window = new BedWarningWindow();
        window.Apply(new BedWarningDisplay(TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(37), PowerAction.Sleep));
        CaptureAutoSized(window, path);
    }

    private static void CaptureSplash(string path)
    {
        var splash = new SplashWindow();
        splash.PrepareForScreenshot();
        const int width = 600;
        const int height = 380;
        var root = (FrameworkElement)splash.Content;
        CaptureElement(root, width, height, path);
    }

    private static void CaptureAutoSized(Window window, string path)
    {
        var root = (FrameworkElement)window.Content;
        root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = root.DesiredSize;
        root.Arrange(new Rect(size));
        root.UpdateLayout();
        CaptureElement(root, Math.Max(1, (int)Math.Ceiling(size.Width)), Math.Max(1, (int)Math.Ceiling(size.Height)), path);
    }

    private static void CaptureElement(FrameworkElement element, int width, int height, string path)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
