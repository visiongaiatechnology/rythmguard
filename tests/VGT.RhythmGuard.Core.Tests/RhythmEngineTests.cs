using Xunit;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Core.Tests;

public sealed class RhythmEngineTests
{
    private static readonly RhythmSettings Default = RhythmSettings.Default;

    private static DateTime At(int day, int hour, int minute, int second = 0)
        => new(2026, 10, day, hour, minute, second, DateTimeKind.Unspecified);

    [Fact]
    public void DayStart_IsFocusWithTwoHourPhase()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 7, 0), Default);

        Assert.Equal(RhythmMode.Focus, snapshot.Mode);
        Assert.Equal(At(24, 7, 0), snapshot.PhaseStart);
        Assert.Equal(At(24, 9, 0), snapshot.PhaseEnd);
        Assert.Equal(TimeSpan.FromHours(2), snapshot.Remaining);
        Assert.Equal(1, snapshot.CycleNumber);
        Assert.Equal(At(24, 9, 0), snapshot.NextBreakStart);
        Assert.Equal(0d, snapshot.PhaseProgress);
    }

    [Fact]
    public void LastMinuteOfFocus_RemainsOneMinute()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 8, 59), Default);

        Assert.Equal(RhythmMode.Focus, snapshot.Mode);
        Assert.Equal(TimeSpan.FromMinutes(1), snapshot.Remaining);
    }

    [Fact]
    public void BreakStartsAtTwoHourBoundary()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 9, 0), Default);

        Assert.Equal(RhythmMode.Break, snapshot.Mode);
        Assert.Equal(At(24, 9, 0), snapshot.PhaseStart);
        Assert.Equal(At(24, 9, 30), snapshot.PhaseEnd);
        Assert.Equal(TimeSpan.FromMinutes(30), snapshot.Remaining);
        Assert.Equal(At(24, 11, 30), snapshot.NextBreakStart);
    }

    [Fact]
    public void BreakEndsIntoNextFocus()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 9, 30), Default);

        Assert.Equal(RhythmMode.Focus, snapshot.Mode);
        Assert.Equal(At(24, 9, 30), snapshot.PhaseStart);
        Assert.Equal(At(24, 11, 30), snapshot.PhaseEnd);
        Assert.Equal(2, snapshot.CycleNumber);
    }

    [Fact]
    public void LateBreakSlot_At2145_IsBreak()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 21, 45), Default);

        Assert.Equal(RhythmMode.Break, snapshot.Mode);
        Assert.Equal(At(24, 21, 30), snapshot.PhaseStart);
        Assert.Equal(At(24, 22, 0), snapshot.PhaseEnd);
        Assert.Equal(TimeSpan.FromMinutes(15), snapshot.Remaining);
    }

    [Fact]
    public void TrailingFocus_IsTruncatedAtDayEnd()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 22, 0), Default);

        Assert.Equal(RhythmMode.Focus, snapshot.Mode);
        Assert.Equal(At(24, 22, 0), snapshot.PhaseStart);
        Assert.Equal(At(24, 23, 0), snapshot.PhaseEnd);
        Assert.Equal(TimeSpan.FromHours(1), snapshot.Remaining);
        Assert.Null(snapshot.NextBreakStart);
    }

    [Fact]
    public void AtDayEnd_IsEveningMode()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 23, 0), Default);

        Assert.Equal(RhythmMode.Evening, snapshot.Mode);
        Assert.Equal(At(24, 23, 0), snapshot.PhaseStart);
        Assert.Equal(At(25, 7, 0), snapshot.PhaseEnd);
        Assert.Equal(At(24, 23, 20), snapshot.NextEveningReminder);
        Assert.Equal(At(24, 23, 0), snapshot.DueEveningReminder);
    }

    [Fact]
    public void DueEveningReminder_TracksReachedGridPoints()
    {
        Assert.Equal(At(24, 23, 0), RhythmEngine.Evaluate(At(24, 23, 0, 0), Default).DueEveningReminder);
        Assert.Equal(At(24, 23, 0), RhythmEngine.Evaluate(At(24, 23, 19, 59), Default).DueEveningReminder);
        Assert.Equal(At(24, 23, 20), RhythmEngine.Evaluate(At(24, 23, 20, 0), Default).DueEveningReminder);
        Assert.Equal(At(24, 23, 20), RhythmEngine.Evaluate(At(24, 23, 39, 59), Default).DueEveningReminder);
        Assert.Equal(At(24, 23, 40), RhythmEngine.Evaluate(At(24, 23, 40, 0), Default).DueEveningReminder);
    }

    [Fact]
    public void DueEveningReminder_IsNeverInTheFuture()
    {
        var start = At(24, 23, 0);
        for (var minute = 0; minute < 120; minute++)
        {
            var now = start.AddMinutes(minute).AddSeconds(0.4);
            var snapshot = RhythmEngine.Evaluate(now, Default);

            Assert.NotNull(snapshot.DueEveningReminder);
            Assert.True(snapshot.DueEveningReminder <= now);
        }
    }

    [Fact]
    public void DueEveningReminder_ContinuesThroughEarlyMorning()
    {
        Assert.Equal(At(25, 6, 40), RhythmEngine.Evaluate(At(25, 6, 41), Default).DueEveningReminder);
        Assert.Equal(At(25, 6, 40), RhythmEngine.Evaluate(At(25, 6, 59), Default).DueEveningReminder);
        Assert.Null(RhythmEngine.Evaluate(At(25, 7, 0), Default).DueEveningReminder);
    }

    [Fact]
    public void NextEveningReminder_IsStrictlyInTheFuture_Regression()
    {
        // Dokumentiert den Ursprungsfehler: der "nächste" Slot kann nie mit "jetzt"
        // verglichen werden, weil er beim Überschreiten sofort zum übernächsten springt.
        var justBefore = RhythmEngine.Evaluate(At(24, 23, 19, 59), Default);
        Assert.Equal(At(24, 23, 20), justBefore.NextEveningReminder);

        var justAfter = RhythmEngine.Evaluate(At(24, 23, 20, 0).AddSeconds(0.4), Default);
        Assert.Equal(At(24, 23, 40), justAfter.NextEveningReminder);
    }

    [Fact]
    public void DueEveningReminder_IsNullWhenRemindersAreDisabled()
    {
        var settings = Default with { EveningRemindersEnabled = false };
        var snapshot = RhythmEngine.Evaluate(At(24, 23, 5), settings);

        Assert.Equal(RhythmMode.Evening, snapshot.Mode);
        Assert.Null(snapshot.NextEveningReminder);
        Assert.Null(snapshot.DueEveningReminder);
    }

    [Fact]
    public void EveningReminder_TickLoop_ShowsAtEveningStartAndEvery20Minutes()
    {
        var shown = new List<DateTime>();
        DateTime? lastShown = null;
        var tick = At(24, 22, 59, 50);

        while (tick <= At(24, 23, 45, 0))
        {
            var snapshot = RhythmEngine.Evaluate(tick, Default);
            if (snapshot.Mode == RhythmMode.Evening
                && snapshot.DueEveningReminder is { } due
                && lastShown != due)
            {
                lastShown = due;
                shown.Add(tick);
            }

            tick = tick.AddSeconds(1);
        }

        Assert.Equal(3, shown.Count);
        Assert.Equal(At(24, 23, 0, 0), shown[0]);
        Assert.Equal(TimeSpan.FromMinutes(20), shown[1] - shown[0]);
        Assert.Equal(TimeSpan.FromMinutes(20), shown[2] - shown[1]);
    }

    [Fact]
    public void NextEveningReminder_OldCoordinatorCondition_NeverTriggers()
    {
        // Regression: die alte UI-Bedingung "jetzt >= NextEveningReminder" konnte nie
        // wahr werden, weil der nächste Slot immer strikt in der Zukunft liegt.
        var triggered = 0;
        for (var tick = At(24, 23, 0, 0); tick <= At(24, 23, 59, 0); tick = tick.AddSeconds(1))
        {
            var snapshot = RhythmEngine.Evaluate(tick, Default);
            if (snapshot.NextEveningReminder is { } due && tick >= due)
            {
                triggered++;
            }
        }

        Assert.Equal(0, triggered);
    }

    [Fact]
    public void EveningReminders_AreAlignedToInterval()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 23, 1), Default);

        Assert.Equal(RhythmMode.Evening, snapshot.Mode);
        Assert.Equal(At(24, 23, 20), snapshot.NextEveningReminder);
    }

    [Fact]
    public void EarlyMorning_IsStillEveningWithReminders()
    {
        var snapshot = RhythmEngine.Evaluate(At(25, 6, 1), Default);

        Assert.Equal(RhythmMode.Evening, snapshot.Mode);
        Assert.Equal(At(25, 7, 0), snapshot.PhaseEnd);
        Assert.Equal(At(25, 6, 20), snapshot.NextEveningReminder);
    }

    [Fact]
    public void LastEveningReminder_BeforeDayStart_IsNotScheduledPastBoundary()
    {
        var snapshot = RhythmEngine.Evaluate(At(25, 6, 40), Default);

        Assert.Equal(RhythmMode.Evening, snapshot.Mode);
        Assert.Null(snapshot.NextEveningReminder);
    }

    [Fact]
    public void AfterMidnight_ReminderGridContinues()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 23, 59), Default);

        Assert.Equal(RhythmMode.Evening, snapshot.Mode);
        Assert.Equal(At(25, 0, 0), snapshot.NextEveningReminder);
    }

    [Fact]
    public void BedMode_OverridesSchedule()
    {
        var bed = new BedSession
        {
            StartedAtLocal = At(24, 20, 0),
            Duration = TimeSpan.FromMinutes(120),
            Action = PowerAction.Sleep,
        };

        var snapshot = RhythmEngine.Evaluate(At(24, 21, 0), Default, bed);

        Assert.Equal(RhythmMode.Bed, snapshot.Mode);
        Assert.Equal(TimeSpan.FromHours(1), snapshot.Remaining);
        Assert.Null(snapshot.PendingPowerAction);
        Assert.NotNull(snapshot.BedSession);
    }

    [Fact]
    public void ExpiredBedMode_RequestsPowerAction()
    {
        var bed = new BedSession
        {
            StartedAtLocal = At(24, 20, 0),
            Duration = TimeSpan.FromMinutes(120),
            Action = PowerAction.Hibernate,
        };

        var snapshot = RhythmEngine.Evaluate(At(24, 22, 0), Default, bed);

        Assert.Equal(RhythmMode.BedExpired, snapshot.Mode);
        Assert.Equal(TimeSpan.Zero, snapshot.Remaining);
        Assert.Equal(PowerAction.Hibernate, snapshot.PendingPowerAction);
    }

    [Fact]
    public void CustomSettings_UseShorterCycle()
    {
        var custom = Default with { WorkMinutes = 90, BreakMinutes = 15 };

        var focus = RhythmEngine.Evaluate(At(24, 7, 0), custom);
        Assert.Equal(At(24, 8, 30), focus.PhaseEnd);

        var pause = RhythmEngine.Evaluate(At(24, 8, 30), custom);
        Assert.Equal(RhythmMode.Break, pause.Mode);
        Assert.Equal(At(24, 8, 45), pause.PhaseEnd);
    }

    [Fact]
    public void InvalidSettings_ThrowFromEvaluate()
    {
        var invalid = Default with { WorkMinutes = 10 };

        Assert.Throws<RhythmValidationException>(() => RhythmEngine.Evaluate(At(24, 12, 0), invalid));
    }

    [Fact]
    public void Progress_IsBoundedBetweenZeroAndOne()
    {
        var snapshot = RhythmEngine.Evaluate(At(24, 10, 15), Default);

        Assert.InRange(snapshot.PhaseProgress, 0d, 1d);
    }
}
