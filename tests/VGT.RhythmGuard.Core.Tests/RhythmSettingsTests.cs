using Xunit;
using VGT.RhythmGuard.Core;

namespace VGT.RhythmGuard.Core.Tests;

public sealed class RhythmSettingsTests
{
    [Fact]
    public void DefaultSettings_AreValid()
    {
        var settings = RhythmSettings.Default.Validate();

        Assert.Equal(new TimeOnly(7, 0), settings.DayStart);
        Assert.Equal(new TimeOnly(23, 0), settings.DayEnd);
        Assert.Equal(120, settings.WorkMinutes);
        Assert.Equal(30, settings.BreakMinutes);
        Assert.Equal(20, settings.EveningReminderMinutes);
        Assert.Equal(120, settings.BedModeMinutes);
        Assert.Equal(PowerAction.Sleep, settings.BedModePowerAction);
    }

    [Fact]
    public void DayStartAfterDayEnd_Throws()
    {
        var settings = RhythmSettings.Default with { DayStart = new TimeOnly(23, 0), DayEnd = new TimeOnly(7, 0) };

        Assert.Throws<RhythmValidationException>(() => settings.Validate());
    }

    [Theory]
    [InlineData(29, 30)]
    [InlineData(481, 30)]
    [InlineData(120, 4)]
    [InlineData(120, 121)]
    public void WorkOrBreakOutOfRange_Throws(int workMinutes, int breakMinutes)
    {
        var settings = RhythmSettings.Default with { WorkMinutes = workMinutes, BreakMinutes = breakMinutes };

        Assert.Throws<RhythmValidationException>(() => settings.Validate());
    }

    [Theory]
    [InlineData(4)]
    [InlineData(121)]
    public void EveningReminderOutOfRange_Throws(int minutes)
    {
        var settings = RhythmSettings.Default with { EveningReminderMinutes = minutes };

        Assert.Throws<RhythmValidationException>(() => settings.Validate());
    }

    [Fact]
    public void UnknownLanguage_Throws()
    {
        var settings = RhythmSettings.Default with { UiLanguage = "fr" };

        Assert.Throws<RhythmValidationException>(() => settings.Validate());
    }

    [Theory]
    [InlineData(14)]
    [InlineData(481)]
    public void BedModeOutOfRange_Throws(int minutes)
    {
        var settings = RhythmSettings.Default with { BedModeMinutes = minutes };

        Assert.Throws<RhythmValidationException>(() => settings.Validate());
    }
}
