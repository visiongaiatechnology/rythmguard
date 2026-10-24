using Xunit;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Core.Tests;

public sealed class StatisticsCalculatorTests
{
    private static RhythmEvent E(RhythmEventKind kind, DateTime at) => RhythmEvent.Create(kind, at);

    [Fact]
    public void EmptyHistory_ReturnsZeros()
    {
        var stats = StatisticsCalculator.Calculate(Array.Empty<RhythmEvent>(), new DateTime(2026, 10, 24, 18, 0, 0));

        Assert.Equal(0, stats.TodayBreaksCompleted);
        Assert.Equal(0, stats.TotalBreaksCompleted);
        Assert.Equal(0, stats.StreakDays);
        Assert.Null(stats.LastBreakCompletedAt);
    }

    [Fact]
    public void CountsTodayAndWeekSeparately()
    {
        var today = new DateTime(2026, 10, 24, 18, 0, 0); // Samstag
        var events = new[]
        {
            E(RhythmEventKind.BreakCompleted, today.AddHours(-2)),
            E(RhythmEventKind.BreakCompleted, today.AddHours(-4)),
            E(RhythmEventKind.BreakAborted, today.AddHours(-5)),
            E(RhythmEventKind.BreakCompleted, today.AddDays(-1)),
            E(RhythmEventKind.BreakCompleted, today.AddDays(-2)),
            E(RhythmEventKind.BreakCompleted, today.AddDays(-10)),
        };

        var stats = StatisticsCalculator.Calculate(events, today);

        Assert.Equal(2, stats.TodayBreaksCompleted);
        Assert.Equal(1, stats.TodayBreaksAborted);
        Assert.Equal(5, stats.TotalBreaksCompleted);
        // Woche beginnt Montag (19.10.): 24., 23., 22. => 4
        Assert.Equal(4, stats.WeekBreaksCompleted);
    }

    [Fact]
    public void Streak_CountsConsecutiveDaysIncludingToday()
    {
        var today = new DateTime(2026, 10, 24, 18, 0, 0);
        var events = new[]
        {
            E(RhythmEventKind.BreakCompleted, today),
            E(RhythmEventKind.BreakCompleted, today.AddDays(-1)),
            E(RhythmEventKind.BreakCompleted, today.AddDays(-2)),
        };

        var stats = StatisticsCalculator.Calculate(events, today);

        Assert.Equal(3, stats.StreakDays);
    }

    [Fact]
    public void Streak_StopsAtMissingDay()
    {
        var today = new DateTime(2026, 10, 24, 18, 0, 0);
        var events = new[]
        {
            E(RhythmEventKind.BreakCompleted, today),
            E(RhythmEventKind.BreakCompleted, today.AddDays(-2)),
        };

        var stats = StatisticsCalculator.Calculate(events, today);

        Assert.Equal(1, stats.StreakDays);
    }

    [Fact]
    public void EveningAndBedMetrics_AreCounted()
    {
        var today = new DateTime(2026, 10, 24, 23, 30, 0);
        var events = new[]
        {
            E(RhythmEventKind.EveningReminderShown, today.AddMinutes(-10)),
            E(RhythmEventKind.EveningReminderShown, today.AddMinutes(-30)),
            E(RhythmEventKind.BedModeStarted, today.AddMinutes(-5)),
            E(RhythmEventKind.EmergencyExitUsed, today.AddMinutes(-20)),
        };

        var stats = StatisticsCalculator.Calculate(events, today);

        Assert.Equal(2, stats.TodayEveningReminders);
        Assert.Equal(1, stats.TodayBedSessions);
        Assert.Equal(1, stats.TodayEmergencyExits);
    }
}
