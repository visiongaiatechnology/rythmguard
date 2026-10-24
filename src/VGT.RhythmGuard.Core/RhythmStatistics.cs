namespace VGT.RhythmGuard.Core;

/// <summary>Aggregierte Kennzahlen aus der Ereignis-Historie.</summary>
public sealed record RhythmStatistics
{
    public int TodayBreaksCompleted { get; init; }

    public int TodayBreaksAborted { get; init; }

    public int TodayEveningReminders { get; init; }

    public int TodayEmergencyExits { get; init; }

    public int TodayBedSessions { get; init; }

    public int WeekBreaksCompleted { get; init; }

    public int WeekBreaksAborted { get; init; }

    public int TotalBreaksCompleted { get; init; }

    public DateTime? LastBreakCompletedAt { get; init; }

    /// <summary>Anzahl aufeinanderfolgender Tage (bis heute/gistern) mit mindestens einer fertigen Pause.</summary>
    public int StreakDays { get; init; }
}

/// <summary>Reine, testbare Statistik-Berechnung über Ereignissen.</summary>
public static class StatisticsCalculator
{
    public static RhythmStatistics Calculate(IEnumerable<RhythmEvent> events, DateTime localNow)
    {
        ArgumentNullException.ThrowIfNull(events);

        var materialized = events.ToList();
        var today = localNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 0));
        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            weekStart = today.AddDays(-6);
        }

        var completedByDay = materialized
            .Where(e => e.Kind == RhythmEventKind.BreakCompleted)
            .GroupBy(e => e.AtLocal.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var todayEvents = materialized.Where(e => e.AtLocal.Date == today).ToList();
        var weekEvents = materialized.Where(e => e.AtLocal.Date >= weekStart && e.AtLocal.Date <= today).ToList();

        return new RhythmStatistics
        {
            TodayBreaksCompleted = todayEvents.Count(e => e.Kind == RhythmEventKind.BreakCompleted),
            TodayBreaksAborted = todayEvents.Count(e => e.Kind == RhythmEventKind.BreakAborted),
            TodayEveningReminders = todayEvents.Count(e => e.Kind == RhythmEventKind.EveningReminderShown),
            TodayEmergencyExits = todayEvents.Count(e => e.Kind == RhythmEventKind.EmergencyExitUsed),
            TodayBedSessions = todayEvents.Count(e => e.Kind == RhythmEventKind.BedModeStarted),
            WeekBreaksCompleted = weekEvents.Count(e => e.Kind == RhythmEventKind.BreakCompleted),
            WeekBreaksAborted = weekEvents.Count(e => e.Kind == RhythmEventKind.BreakAborted),
            TotalBreaksCompleted = materialized.Count(e => e.Kind == RhythmEventKind.BreakCompleted),
            LastBreakCompletedAt = materialized
                .Where(e => e.Kind == RhythmEventKind.BreakCompleted)
                .Select(e => (DateTime?)e.AtLocal)
                .Max(),
            StreakDays = CalculateStreak(completedByDay, today),
        };
    }

    private static int CalculateStreak(Dictionary<DateTime, int> completedByDay, DateTime today)
    {
        if (completedByDay.Count == 0)
        {
            return 0;
        }

        var cursor = completedByDay.ContainsKey(today) ? today : today.AddDays(-1);
        if (!completedByDay.ContainsKey(cursor))
        {
            return 0;
        }

        var streak = 0;
        while (completedByDay.ContainsKey(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
