namespace VGT.RhythmGuard.Linux.Ui;

/// <summary>Abstraktion der UI-Schicht; der Coordinator kennt keine WPF-Fensterklassen.</summary>
internal interface IRhythmUi
{
    event EventHandler? EmergencyExitRequested;

    event EveningChoiceHandler? EveningChoiceReceived;

    void ShowBreak(BreakDisplay display);

    void UpdateBreak(BreakDisplay display);

    void HideBreak();

    void ShowEveningReminder(EveningReminderDisplay display);

    void HideEveningReminder();

    void ShowBedCountdown(BedCountdownDisplay display);

    void HideBedCountdown();

    void ShowBedWarning(BedWarningDisplay display);

    void UpdateBedWarning(BedWarningDisplay display);

    void HideBedWarning();

    void ShowShutdownWarning(ShutdownWarningDisplay display);

    void UpdateShutdownWarning(ShutdownWarningDisplay display);

    void HideShutdownWarning();
}
