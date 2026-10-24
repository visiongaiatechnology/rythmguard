using System.Globalization;
using System.Windows;
using VGT.RhythmGuard.App.Localization;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>Kleine, nicht aktivierende Statusanzeige für den laufenden Bett-Modus.</summary>
internal partial class BedCountdownWindow : Window
{
    private bool _closedByOwner;

    public BedCountdownWindow()
    {
        InitializeComponent();
        Localizer.EnableAutoTranslation(this);
    }

    public void Apply(BedCountdownDisplay display)
    {
        CountdownText.Text = FormatCountdown(display.Remaining);
        ActionText.Text = "danach " + Localizer.T(Describe(display.Action));
    }

    public void PositionBottomRight()
    {
        var workArea = SystemParameters.WorkArea;
        UpdateLayout();
        Left = workArea.Right - ActualWidth - 28;
        Top = workArea.Bottom - ActualHeight - 28;
    }

    public void CloseWithoutNotification()
    {
        _closedByOwner = true;
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _ = _closedByOwner;
        base.OnClosing(e);
    }

    private static string FormatCountdown(TimeSpan remaining)
    {
        var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        return clamped.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }

    private static string Describe(PowerAction action) => action switch
    {
        PowerAction.Sleep => "Ruhezustand",
        PowerAction.Hibernate => "Hibernate",
        PowerAction.ShutdownGraceful => "Herunterfahren",
        PowerAction.ShutdownHard => "hartes Herunterfahren",
        _ => "Energie-Aktion",
    };
}

