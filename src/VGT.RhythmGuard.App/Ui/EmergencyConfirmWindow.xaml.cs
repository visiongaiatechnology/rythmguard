using System.Windows;
using VGT.RhythmGuard.App.Localization;

namespace VGT.RhythmGuard.App.Ui;

/// <summary>Bewusste Hürde gegen versehentlichen Notausstieg.</summary>
internal partial class EmergencyConfirmWindow : Window
{
    public EmergencyConfirmWindow()
    {
        InitializeComponent();
        Localizer.EnableAutoTranslation(this);
        ConfirmInput.Focus();
    }

    private void OnConfirmTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => ConfirmButton.IsEnabled = string.Equals(ConfirmInput.Text.Trim(), "PAUSE", StringComparison.OrdinalIgnoreCase);

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (!ConfirmButton.IsEnabled)
        {
            return;
        }

        DialogResult = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => DialogResult = false;
}

