using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Linux.Ui;

internal enum EveningChoice
{
    StartBedMode = 0,
    Dismiss = 1,
}

internal sealed record BreakDisplay(
    TimeSpan Remaining,
    DateTime EndsAt,
    double Progress,
    bool EmergencyExitEnabled,
    int EmergencyHoldSeconds,
    bool IsManual);

internal sealed record EveningReminderDisplay(DateTime ShownAt, int BedModeMinutes);

internal sealed record BedCountdownDisplay(TimeSpan Remaining, PowerAction Action);

internal sealed record BedWarningDisplay(TimeSpan Remaining, PowerAction Action);

internal sealed record ShutdownWarningDisplay(int Seconds, PowerAction Action);

internal delegate void EveningChoiceHandler(EveningChoice choice);
