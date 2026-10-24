namespace VGT.RhythmGuard.Core;

/// <summary>
/// Deterministische, zustandslose Rhythmus-Engine.
/// Sämtliche Zeitberechnungen laufen auf lokaler Wanduhr-Zeit (Kind=Unspecified),
/// damit Tagesfenster und Abendraster unabhängig von Zeitzonen-Details stabil bleiben.
/// </summary>
public static class RhythmEngine
{
    /// <summary>
    /// Berechnet den Zustand zu einem lokalen Zeitpunkt. Reine Funktion ohne Seiteneffekte.
    /// </summary>
    /// <exception cref="RhythmValidationException">Konfiguration ist ungültig.</exception>
    public static RhythmSnapshot Evaluate(DateTime localNow, RhythmSettings settings, BedSession? bedSession = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var valid = settings.Validate();

        if (bedSession is not null)
        {
            return EvaluateBed(localNow, valid, bedSession);
        }

        var dayStart = localNow.Date + valid.DayStart.ToTimeSpan();
        var dayEnd = localNow.Date + valid.DayEnd.ToTimeSpan();

        if (localNow < dayStart || localNow >= dayEnd)
        {
            return EvaluateEvening(localNow, valid, dayStart, dayEnd);
        }

        return EvaluateDay(localNow, valid, dayStart, dayEnd);
    }

    private static RhythmSnapshot EvaluateBed(DateTime localNow, RhythmSettings settings, BedSession bed)
    {
        var end = bed.EndsAtLocal;
        var windowStart = localNow.Date + settings.DayStart.ToTimeSpan();
        var windowEnd = localNow.Date + settings.DayEnd.ToTimeSpan();

        if (localNow < end)
        {
            var length = end - bed.StartedAtLocal;
            return new RhythmSnapshot
            {
                Mode = RhythmMode.Bed,
                PhaseStart = bed.StartedAtLocal,
                PhaseEnd = end,
                Remaining = end - localNow,
                PhaseProgress = Progress(bed.StartedAtLocal, end, localNow),
                NextBreakStart = null,
                NextEveningReminder = null,
                CycleNumber = null,
                BedSession = bed,
                PendingPowerAction = null,
                DayStart = windowStart,
                DayEnd = windowEnd,
            };
        }

        return new RhythmSnapshot
        {
            Mode = RhythmMode.BedExpired,
            PhaseStart = bed.StartedAtLocal,
            PhaseEnd = end,
            Remaining = TimeSpan.Zero,
            PhaseProgress = 1d,
            NextBreakStart = null,
            NextEveningReminder = null,
            CycleNumber = null,
            BedSession = bed,
            PendingPowerAction = bed.Action,
            DayStart = windowStart,
            DayEnd = windowEnd,
        };
    }

    private static RhythmSnapshot EvaluateEvening(DateTime localNow, RhythmSettings settings, DateTime dayStart, DateTime dayEnd)
    {
        DateTime eveningStart;
        DateTime nextDayStart;

        if (localNow < dayStart)
        {
            eveningStart = dayStart.AddDays(-1).Add(settings.DayEnd.ToTimeSpan() - settings.DayStart.ToTimeSpan()).Add(settings.DayStart.ToTimeSpan());
            eveningStart = localNow.Date.AddDays(-1) + settings.DayEnd.ToTimeSpan();
            nextDayStart = dayStart;
        }
        else
        {
            eveningStart = dayEnd;
            nextDayStart = localNow.Date.AddDays(1) + settings.DayStart.ToTimeSpan();
        }

        DateTime? nextReminder = null;
        if (settings.EveningRemindersEnabled)
        {
            nextReminder = FindNextEveningReminder(localNow, eveningStart, nextDayStart, settings.EveningReminderMinutes);
        }

        var windowStart = nextDayStart.Date + settings.DayStart.ToTimeSpan();
        var windowEnd = nextDayStart.Date + settings.DayEnd.ToTimeSpan();

        return new RhythmSnapshot
        {
            Mode = RhythmMode.Evening,
            PhaseStart = eveningStart,
            PhaseEnd = nextDayStart,
            Remaining = nextDayStart - localNow,
            PhaseProgress = Progress(eveningStart, nextDayStart, localNow),
            NextBreakStart = null,
            NextEveningReminder = nextReminder,
            CycleNumber = null,
            BedSession = null,
            PendingPowerAction = null,
            DayStart = windowStart,
            DayEnd = windowEnd,
        };
    }

    private static DateTime? FindNextEveningReminder(DateTime localNow, DateTime eveningStart, DateTime nextDayStart, int intervalMinutes)
    {
        var interval = TimeSpan.FromMinutes(intervalMinutes);
        if (localNow < eveningStart)
        {
            return eveningStart;
        }

        var elapsed = localNow - eveningStart;
        var steps = (long)(elapsed.Ticks / interval.Ticks) + 1; // strikt nach localNow
        var candidate = eveningStart + TimeSpan.FromTicks(steps * interval.Ticks);
        return candidate < nextDayStart ? candidate : null;
    }

    private static RhythmSnapshot EvaluateDay(DateTime localNow, RhythmSettings settings, DateTime dayStart, DateTime dayEnd)
    {
        var workLength = TimeSpan.FromMinutes(settings.WorkMinutes);
        var cycleLength = TimeSpan.FromMinutes(settings.WorkMinutes + settings.BreakMinutes);

        var elapsed = localNow - dayStart;
        var cycleIndex = elapsed.Ticks / cycleLength.Ticks;
        var cycleStart = dayStart + TimeSpan.FromTicks(cycleIndex * cycleLength.Ticks);
        var workEnd = cycleStart + workLength;
        var cycleNumber = checked((int)cycleIndex + 1);

        if (localNow < workEnd)
        {
            var phaseEnd = workEnd < dayEnd ? workEnd : dayEnd;
            DateTime? nextBreak = workEnd < dayEnd ? workEnd : null;

            return new RhythmSnapshot
            {
                Mode = RhythmMode.Focus,
                PhaseStart = cycleStart,
                PhaseEnd = phaseEnd,
                Remaining = phaseEnd - localNow,
                PhaseProgress = Progress(cycleStart, phaseEnd, localNow),
                NextBreakStart = nextBreak,
                NextEveningReminder = null,
                CycleNumber = cycleNumber,
                BedSession = null,
                PendingPowerAction = null,
                DayStart = dayStart,
                DayEnd = dayEnd,
            };
        }

        var breakEnd = cycleStart + cycleLength;
        var breakPhaseEnd = breakEnd < dayEnd ? breakEnd : dayEnd;
        var followingWorkEnd = breakPhaseEnd + workLength;
        DateTime? nextBreakAfterBreak = followingWorkEnd < dayEnd ? followingWorkEnd : null;

        return new RhythmSnapshot
        {
            Mode = RhythmMode.Break,
            PhaseStart = workEnd,
            PhaseEnd = breakPhaseEnd,
            Remaining = breakPhaseEnd - localNow,
            PhaseProgress = Progress(workEnd, breakPhaseEnd, localNow),
            NextBreakStart = nextBreakAfterBreak,
            NextEveningReminder = null,
            CycleNumber = cycleNumber,
            BedSession = null,
            PendingPowerAction = null,
            DayStart = dayStart,
            DayEnd = dayEnd,
        };
    }

    private static double Progress(DateTime start, DateTime end, DateTime now)
    {
        var total = (end - start).Ticks;
        if (total <= 0)
        {
            return 0d;
        }

        var done = (now - start).Ticks;
        var value = (double)done / total;
        return Math.Clamp(value, 0d, 1d);
    }
}
