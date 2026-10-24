using System.Globalization;
using System.Windows;
using VGT.RhythmGuard.App.Localization;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>Sanfte Warnung vor dem Ende des Bett-Modus (nicht blockierend).</summary>
internal partial class BedWarningWindow : Window
{
    public BedWarningWindow()
    {
        InitializeComponent();
        Localizer.EnableAutoTranslation(this);
    }

    public void Apply(BedWarningDisplay display)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(display.Remaining.TotalMinutes));
        TitleText.Text = Localizer.Format("Bett-Modus endet in {0} Minuten", minutes);
        BodyText.Text = "Bitte speichere deine Arbeit. Danach greift die Energie-Aktion.";
        ActionText.Text = Localizer.Format("Danach: {0}", Describe(display.Action));
    }

    public void PositionTopCenter()
    {
        var workArea = SystemParameters.WorkArea;
        UpdateLayout();
        Left = workArea.Left + (workArea.Width - ActualWidth) / 2d;
        Top = workArea.Top + 32;
    }

    public void CloseWithoutNotification() => Close();

    private static string Describe(PowerAction action) => action switch
    {
        PowerAction.Sleep => "Ruhezustand",
        PowerAction.Hibernate => "Hibernate",
        PowerAction.ShutdownGraceful => "Herunterfahren",
        PowerAction.ShutdownHard => "hartes Herunterfahren",
        _ => "Energie-Aktion",
    };
}

