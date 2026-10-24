using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using VGT.RhythmGuard.Core;
using VGT.RhythmGuard.Linux.Localization;

namespace VGT.RhythmGuard.Linux.Ui;

/// <summary>Abend-Erinnerung (nicht blockierend, unten rechts).</summary>
internal sealed class EveningReminderWindow : Window
{
    private readonly TextBlock _time;
    private readonly TextBlock _hint;
    private readonly Button _startButton;

    public EveningReminderWindow(EveningReminderDisplay display)
    {
        Title = "Abendmodus";
        Width = 440;
        Height = 280;
        CanResize = false;
        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
        Topmost = true;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        WindowStartupLocation = WindowStartupLocation.Manual;

        _time = UiKit.Caption($"ABENDMODUS · {display.ShownAt:HH:mm}");
        _hint = UiKit.Caption(Localizer.Format("Bett-Modus: {0} Minuten anschauen, danach greift die Energie-Aktion.", display.BedModeMinutes));
        _startButton = UiKit.Primary($"Bett-Modus starten ({display.BedModeMinutes} Min)", () =>
        {
            ChoiceMade?.Invoke(EveningChoice.StartBedMode);
            Close();
        });

        var close = UiKit.Secondary(Localizer.T("Später"), () =>
        {
            ChoiceMade?.Invoke(EveningChoice.Dismiss);
            Close();
        });

        Content = new Border
        {
            Background = UiKit.Solid("#F0150F33"),
            BorderBrush = UiKit.Solid("#30FFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(26),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    _time,
                    UiKit.TextBlock("Zeit, loszulassen.", 24, FontWeight.SemiBold),
                    UiKit.TextBlock(Localizer.T("Du hast für heute genug gearbeitet. Ab jetzt darf der Tag ruhig enden."), 13.5, FontWeight.Normal, UiKit.TextSecondary),
                    UiKit.Inset(_hint),
                    UiKit.Horizontal(_startButton, close),
                },
            },
        };

        Opened += (_, _) => PositionBottomRight();
    }

    public event Action<EveningChoice>? ChoiceMade;

    private void PositionBottomRight()
    {
        var area = Screens.Primary?.WorkingArea;
        if (area is { } rect)
        {
            Position = new PixelPoint(rect.X + rect.Width - (int)Width - 28, rect.Y + rect.Height - (int)Height - 28);
        }
    }
}

/// <summary>Bett-Modus: kleine Countdown-Pille und Warnfenster.</summary>
internal sealed class BedCountdownWindow : Window
{
    private readonly TextBlock _countdown;
    private readonly TextBlock _action;

    public BedCountdownWindow(BedCountdownDisplay display)
    {
        Title = "Bett-Modus";
        Width = 330;
        Height = 84;
        CanResize = false;
        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
        Topmost = true;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        WindowStartupLocation = WindowStartupLocation.Manual;

        _countdown = UiKit.TextBlock("", 24, FontWeight.SemiBold);
        _action = UiKit.Caption("");
        Apply(display);

        Content = new Border
        {
            Background = UiKit.Solid("#F0150F33"),
            BorderBrush = UiKit.Solid("#30FFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(18, 12),
            Child = UiKit.Horizontal(
                new Ellipse { Width = 34, Height = 34, Fill = UiKit.AccentGradient() },
                new StackPanel
                {
                    Spacing = 2,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { UiKit.Caption("BETT-MODUS"), _action },
                },
                _countdown),
        };

        Opened += (_, _) => PositionBottomRight();
    }

    public void Apply(BedCountdownDisplay display)
    {
        _countdown.Text = Format(display.Remaining);
        _action.Text = "danach " + Localizer.T(Describe(display.Action));
    }

    private void PositionBottomRight()
    {
        var area = Screens.Primary?.WorkingArea;
        if (area is { } rect)
        {
            Position = new PixelPoint(rect.X + rect.Width - (int)Width - 28, rect.Y + rect.Height - (int)Height - 28);
        }
    }

    private static string Format(TimeSpan remaining)
        => (remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining).ToString(@"hh\:mm\:ss");

    internal static string Describe(PowerAction action) => action switch
    {
        PowerAction.Sleep => "Ruhezustand",
        PowerAction.Hibernate => "Hibernate",
        PowerAction.ShutdownGraceful => "Herunterfahren",
        PowerAction.ShutdownHard => "hartes Herunterfahren",
        _ => "Energie-Aktion",
    };
}

internal sealed class BedWarningWindow : Window
{
    private readonly TextBlock _title;
    private readonly TextBlock _action;

    public BedWarningWindow(BedWarningDisplay display)
    {
        Title = "Bett-Modus endet";
        Width = 500;
        Height = 120;
        CanResize = false;
        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
        Topmost = true;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        WindowStartupLocation = WindowStartupLocation.Manual;

        _title = UiKit.TextBlock("", 14, FontWeight.SemiBold);
        _action = UiKit.Caption("");
        Apply(display);

        Content = new Border
        {
            Background = UiKit.Solid("#F0150F33"),
            BorderBrush = UiKit.Solid("#30FFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(22, 16),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    _title,
                    UiKit.TextBlock("Bitte speichere deine Arbeit. Danach greift die Energie-Aktion.", 13, FontWeight.Normal, UiKit.TextSecondary),
                    _action,
                },
            },
        };

        Opened += (_, _) =>
        {
            var area = Screens.Primary?.WorkingArea;
            if (area is { } rect)
            {
                Position = new PixelPoint(rect.X + (rect.Width - (int)Width) / 2, rect.Y + 32);
            }
        };
    }

    public void Apply(BedWarningDisplay display)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(display.Remaining.TotalMinutes));
        _title.Text = Localizer.Format("Bett-Modus endet in {0} Minuten", minutes);
        _action.Text = Localizer.Format("Danach: {0}", Localizer.T(BedCountdownWindow.Describe(display.Action)));
    }
}
