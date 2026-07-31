using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// docs/06 §3: <i>"The clock is authoritative and monotonic. No skipping, no voting,
/// no acceleration."</i>
/// </summary>
public class WorldClockTests
{
    [Fact]
    public void AdvancesInRealTimeByDefault()
    {
        var clock = new WorldClock();
        double advanced = clock.Advance(3600.0);

        Assert.Equal(3600.0, advanced);
        Assert.Equal(3600.0, clock.TotalSeconds);
    }

    [Fact]
    public void RefusesToRunBackwards()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new WorldClock().Advance(-1.0));

    [Fact]
    public void RefusesAFoundingDateBeforeTheEpoch()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new WorldClock(-1.0));

    [Fact]
    public void SolarNoonIsMidday()
    {
        var clock = new WorldClock(WorldClock.AtSeason(0.0, timeOfDay: 0.5));
        Assert.Equal(0.5, clock.TimeOfDay, 6);
        Assert.True(SolarPosition.Compute(clock.YearPhase, clock.TimeOfDay).IsUp);
    }

    [Theory]
    [InlineData(0.0)]   // Midwinter — docs/02 §6's "if you want the other thing".
    [InlineData(0.25)]  // Spring — "if you want a chance".
    [InlineData(0.5)]
    [InlineData(0.75)]
    public void AServerCanBeFoundedInAnySeason(double season)
    {
        var clock = new WorldClock(WorldClock.AtSeason(season));

        // AtSeason snaps to a whole day, so the phase lands within a day of the ask.
        Assert.True(Math.Abs(clock.YearPhase - season) < 1.0 / CretaceousCalendar.DaysPerYear,
            $"Founding at season {season} gave phase {clock.YearPhase:F5}.");
    }

    [Fact]
    public void FoundingTimeOfDayIsNotPolluted()
    {
        // A year is not a whole number of days, so a raw fraction of one lands
        // part-way through a day. Asking for noon must give noon in every season.
        foreach (double season in new[] { 0.0, 0.17, 0.5, 0.83 })
        {
            var clock = new WorldClock(WorldClock.AtSeason(season, timeOfDay: 0.5));
            Assert.Equal(0.5, clock.TimeOfDay, 6);
        }
    }

    [Fact]
    public void TimeScaleMultipliesWorldTimeWithoutBreakingMonotonicity()
    {
        var clock = new WorldClock { TimeScale = 1000.0 };
        double advanced = clock.Advance(1.0);

        Assert.Equal(1000.0, advanced);
        Assert.Equal(1000.0, clock.TotalSeconds);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(WorldClock.MaxDebugTimeScale + 1.0)]
    public void TimeScaleIsBounded(double scale)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new WorldClock { TimeScale = scale });

    [Fact]
    public void DayAndYearCountsAdvanceTogether()
    {
        var clock = new WorldClock();
        clock.Advance(CretaceousCalendar.SecondsPerYear);

        Assert.Equal(1, clock.Year);
        Assert.Equal(374, clock.Day);
    }

    [Fact]
    public void PhasesStayInRangeAcrossManyYears()
    {
        // A dedicated server runs for real years without restarting, so the wrap has
        // to hold up a long way from the epoch.
        var clock = new WorldClock();
        clock.Advance(CretaceousCalendar.SecondsPerYear * 12.3456);

        Assert.InRange(clock.TimeOfDay, 0.0, 1.0);
        Assert.InRange(clock.YearPhase, 0.0, 1.0);
        Assert.InRange(clock.LunarPhase, 0.0, 1.0);
    }

    [Fact]
    public void RestoresExactlyFromASavedWorld()
    {
        // docs/06 §3 treats save-format stability as a first-class requirement, so
        // the clock must round-trip bit-for-bit.
        var original = new WorldClock();
        original.Advance(1234567.891);

        WorldClock restored = WorldClock.Restore(original.TotalSeconds);

        Assert.Equal(original.TotalSeconds, restored.TotalSeconds);
        Assert.Equal(original.YearPhase, restored.YearPhase);
    }

    [Fact]
    public void WorldTimeOfDayDriftsAgainstRealTimeByThirtySixMinutesADay()
    {
        // The precession gift, measured rather than asserted from the constant.
        // After one real day, the world clock has advanced past a whole world day.
        var clock = new WorldClock();
        clock.Advance(24.0 * 3600.0);

        double extraWorldTime = clock.TotalSeconds - CretaceousCalendar.SecondsPerDay;
        Assert.Equal(0.6 * 3600.0, extraWorldTime, 6);
    }
}
