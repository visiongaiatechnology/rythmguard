using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VGT.RhythmGuard.App.Infrastructure;
using VGT.RhythmGuard.Core;
using DrawingPoint = System.Drawing.Point;

namespace VGT.RhythmGuard.App;

/// <summary>Tray-Präsenz mit Live-Status und Schnellaktionen.</summary>
internal sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly RhythmCoordinator _coordinator;
    private readonly Action _openSettings;
    private readonly Action _exitApplication;
    private readonly ToolStripMenuItem _statusItem;
    private readonly Icon _icon;
    private bool _disposed;

    public TrayService(RhythmCoordinator coordinator, Action openSettings, Action exitApplication)
    {
        _coordinator = coordinator;
        _openSettings = openSettings;
        _exitApplication = exitApplication;

        _icon = TrayIconFactory.Create();

        _statusItem = new ToolStripMenuItem("Status wird geladen …") { Enabled = false };

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Einstellungen öffnen", null, (_, _) => _openSettings()));
        menu.Items.Add(new ToolStripMenuItem("Pause jetzt starten", null, (_, _) => _coordinator.PauseNow()));
        menu.Items.Add(new ToolStripMenuItem("Bett-Modus starten", null, (_, _) => _coordinator.StartBedMode()));
        menu.Items.Add(new ToolStripSeparator());
        var autostartItem = new ToolStripMenuItem("Mit Windows starten")
        {
            CheckOnClick = true,
            Checked = _coordinator.Settings.StartWithWindows,
        };
        autostartItem.Click += (_, _) => _coordinator.ApplySettings(_coordinator.Settings with { StartWithWindows = autostartItem.Checked });
        menu.Items.Add(autostartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Beenden", null, (_, _) => _exitApplication()));

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "VGT RythmGuard",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _notifyIcon.DoubleClick += (_, _) => _openSettings();

        _coordinator.SnapshotChanged += OnSnapshotChanged;
        _coordinator.SettingsChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (_notifyIcon.ContextMenuStrip?.Items[5] is ToolStripMenuItem autostartItem)
        {
            autostartItem.Checked = _coordinator.Settings.StartWithWindows;
        }
    }

    private void OnSnapshotChanged(object? sender, RhythmSnapshot snapshot)
    {
        try
        {
            if (_disposed)
            {
                return;
            }

            var status = DescribeStatus(snapshot);
            _statusItem.Text = status;
            _notifyIcon.Text = Truncate(status, 63);
        }
        catch (InvalidOperationException)
        {
            // Tray wurde bereits entsorgt.
        }
    }

    private static string DescribeStatus(RhythmSnapshot snapshot)
    {
        var remaining = FormatRemaining(snapshot.Remaining);
        return snapshot.Mode switch
        {
            RhythmMode.Focus => $"Fokus · Pause in {remaining}",
            RhythmMode.Break => $"Pause · noch {remaining}",
            RhythmMode.Evening => $"Abendmodus · Tag beginnt in {remaining}",
            RhythmMode.Bed => $"Bett-Modus · {remaining} bis {DescribePowerAction(snapshot.BedSession?.Action ?? PowerAction.Sleep)}",
            RhythmMode.BedExpired => "Bett-Modus beendet · Energie-Aktion läuft",
            _ => "VGT RythmGuard",
        };
    }

    private static string DescribePowerAction(PowerAction action) => action switch
    {
        PowerAction.Sleep => "Ruhezustand",
        PowerAction.Hibernate => "Ruhezustand (Hibernate)",
        PowerAction.ShutdownGraceful => "Herunterfahren",
        PowerAction.ShutdownHard => "hartes Herunterfahren",
        _ => "Energie-Aktion",
    };

    private static string FormatRemaining(TimeSpan remaining)
    {
        var totalMinutes = Math.Max(0, (int)Math.Ceiling(remaining.TotalMinutes));
        return totalMinutes >= 60
            ? string.Create(CultureInfo.InvariantCulture, $"{totalMinutes / 60}h {totalMinutes % 60:00}m")
            : string.Create(CultureInfo.InvariantCulture, $"{totalMinutes}m");
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..(max - 1)] + "…";

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _coordinator.SnapshotChanged -= OnSnapshotChanged;
        _coordinator.SettingsChanged -= OnSettingsChanged;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon.Dispose();
    }
}

/// <summary>Liefert das Tray-Icon aus dem in die EXE eingebetteten Marken-Icon.</summary>
internal static class TrayIconFactory
{
    public static Icon Create()
    {
        try
        {
            var executablePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(executablePath))
            {
                var extracted = Icon.ExtractAssociatedIcon(executablePath);
                if (extracted is not null)
                {
                    return (Icon)extracted.Clone();
                }
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException)
        {
            // Fallback: programmatisch erzeugtes Icon.
        }

        return CreateFallback();
    }

    private static Icon CreateFallback()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using var background = new SolidBrush(Color.FromArgb(255, 21, 16, 51));
            graphics.FillEllipse(background, 1, 1, 30, 30);

            using var ring = new Pen(Color.FromArgb(255, 76, 201, 240), 2.4f);
            graphics.DrawArc(ring, 5, 5, 22, 22, 40, 280);

            using var gold = new SolidBrush(Color.FromArgb(255, 201, 167, 255));
            graphics.FillEllipse(gold, 13, 13, 6, 6);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            _ = DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}
