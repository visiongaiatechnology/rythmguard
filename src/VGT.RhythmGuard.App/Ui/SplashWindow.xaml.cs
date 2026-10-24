using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VGT.RhythmGuard.App.Localization;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>Marken-Ladescreen beim Start. Schließt sich nach abgeschlossener Initialisierung.</summary>
internal partial class SplashWindow : Window
{
    private bool _closing;

    public SplashWindow()
    {
        InitializeComponent();
        VersionText.Text = "v" + (typeof(SplashWindow).Assembly.GetName().Version?.ToString(3) ?? "1.2.0");
        Localizer.EnableAutoTranslation(this);

        var animation = new DoubleAnimation(-90, 280, TimeSpan.FromSeconds(1.5))
        {
            RepeatBehavior = RepeatBehavior.Forever,
        };
        ProgressTransform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    /// <summary>Stoppt die Animation für die Screenshot-QA.</summary>
    internal void PrepareForScreenshot()
    {
        ProgressTransform.BeginAnimation(TranslateTransform.XProperty, null);
        ProgressTransform.X = 95;
    }

    /// <summary>Blendet den Splash aus und schließt ihn nach der Animation.</summary>
    public void CloseSplash()
    {
        if (_closing)
        {
            return;
        }

        _closing = true;
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(240))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        fade.Completed += (_, _) =>
        {
            if (IsLoaded)
            {
                Close();
            }
        };
        BeginAnimation(OpacityProperty, fade);
    }
}
