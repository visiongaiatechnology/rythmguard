using System.Globalization;
using System.Windows;
using VGT.RhythmGuard.App.Localization;
using System.Windows.Media.Animation;
using VGT.RhythmGuard.App.Infrastructure;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>Nicht blockierende Abend-Erinnerung mit klaren Optionen.</summary>
internal partial class EveningReminderWindow : Window
{
    private bool _closedByOwner;

    public EveningReminderWindow()
    {
        InitializeComponent();
        Localizer.EnableAutoTranslation(this);
    }

    public event Action<EveningChoice>? ChoiceMade;

    public void Apply(EveningReminderDisplay display)
    {
        TimeCaption.Text = "ABENDMODUS · " + display.ShownAt.ToString("HH:mm", CultureInfo.InvariantCulture);
        BedHintText.Text = string.Create(
            CultureInfo.InvariantCulture,
            $"Bett-Modus: {display.BedModeMinutes} Minuten anschauen, danach greift die Energie-Aktion.");
        StartBedButton.Content = "Bett-Modus starten (" + display.BedModeMinutes + " Min)";

        Opacity = 0;
        var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(240))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        BeginAnimation(OpacityProperty, animation);
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
        if (!_closedByOwner)
        {
            // Schließen über Window-Manager ist immer ein Dismiss.
            ChoiceMade?.Invoke(EveningChoice.Dismiss);
        }

        base.OnClosing(e);
    }

    private void OnStartBedClick(object sender, RoutedEventArgs e)
    {
        _closedByOwner = true;
        ChoiceMade?.Invoke(EveningChoice.StartBedMode);
        Close();
    }

    private void OnDismissClick(object sender, RoutedEventArgs e)
    {
        _closedByOwner = true;
        ChoiceMade?.Invoke(EveningChoice.Dismiss);
        Close();
    }
}

