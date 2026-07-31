using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// The astronomy. These numbers are load-bearing for the entire design, so they are
/// pinned here: docs/02 §6 says explicitly <i>"Do not round the day to 24 hours. The
/// 0.6-hour difference is doing enormous work."</i>
/// </summary>
public class CalendarTests
{
    [Fact]
    public void DayIsTwentyThreePointFourHours()
    {
        Assert.Equal(23.4, CretaceousCalendar.HoursPerDay);
        Assert.Equal(84_240.0, CretaceousCalendar.SecondsPerDay);
    }

    [Fact]
    public void YearIsAboutThreeHundredAndSeventyFiveDays()
    {
        // The 374-day trap of docs/02 §6. A player who assumes 365 drifts almost ten
        // days a year and misjudges the solstice.
        Assert.InRange(CretaceousCalendar.DaysPerYear, 374.5, 374.7);
        Assert.NotEqual(365.0, CretaceousCalendar.DaysPerYear, 0);
    }

    [Fact]
    public void OneWorldYearIsOneRealYear()
    {
        // The whole 1:1 premise. A year takes a year.
        double realYearSeconds = 365.25 * 24 * 3600;
        Assert.Equal(CretaceousCalendar.SecondsPerYear, realYearSeconds, 6);
    }

    [Fact]
    public void LunarMonthIsAboutThirtyDays()
    {
        // Longer in days than the modern 29.53 despite being shorter in hours,
        // because the days themselves are shorter.
        Assert.InRange(CretaceousCalendar.DaysPerLunarMonth, 29.5, 30.5);
        Assert.True(CretaceousCalendar.SynodicMonthHours < 708.7,
            "A moon 0.3% closer must have a shorter synodic month in hours than today's.");
    }

    [Fact]
    public void PrecessionCarriesAFixedPlaySlotThroughTheWholeDay()
    {
        // docs/08 §3 calls this one of the three things that make 1:1 work rather
        // than merely being hardcore. Without it a player with a fixed evening slot
        // would be locked in darkness forever.
        Assert.Equal(0.6, CretaceousCalendar.PrecessionDriftRealHoursPerDay, 10);
        Assert.InRange(CretaceousCalendar.PrecessionCycleRealDays, 38.0, 40.0);
    }

    [Fact]
    public void MeanNightIsElevenPointSevenHours()
    {
        // docs/02 §6: "Nights average 11.7 real hours". Exactly half a 23.4-hour day,
        // which is what a year-long average must give at any latitude.
        double total = 0.0;
        const int Samples = 4000;
        for (int i = 0; i < Samples; i++)
            total += SolarPosition.NightLengthHours(i / (double)Samples);

        Assert.Equal(11.7, total / Samples, 2);
    }

    [Fact]
    public void MidwinterNightIsAboutFifteenHours()
    {
        // docs/02 §6 says nights "run to fifteen or sixteen in midwinter". Computed
        // honestly from 42° N and a 23.44° tilt, the winter solstice gives 14.69 h,
        // which rounds to the lower end of that claim. Recorded here rather than
        // fudged — see the note in the Stage 1 README.
        double night = SolarPosition.NightLengthHours(yearPhase: 0.0);
        Assert.InRange(night, 14.5, 15.0);
    }

    [Fact]
    public void EquinoxesSplitTheDayEvenly()
    {
        Assert.Equal(0.5, SolarPosition.DaylightFraction(0.25), 3);
        Assert.Equal(0.5, SolarPosition.DaylightFraction(0.75), 3);
    }

    [Fact]
    public void SunIsHighestAtMidsummerNoonAndLowestAtMidwinterNoon()
    {
        double midsummer = SolarPosition.Compute(yearPhase: 0.5, timeOfDay: 0.5).AltitudeDegrees;
        double midwinter = SolarPosition.Compute(yearPhase: 0.0, timeOfDay: 0.5).AltitudeDegrees;

        // At 42° N: 90 − 42 + 23.44 = 71.4°, and 90 − 42 − 23.44 = 24.6°.
        Assert.Equal(71.44, midsummer, 1);
        Assert.Equal(24.56, midwinter, 1);
    }

    [Fact]
    public void SunRisesInTheEastAndSetsInTheWest()
    {
        // Morning: east of the meridian. Afternoon: west. The player has no compass,
        // so this is the only direction reference in the game and it must be right.
        double morning = SolarPosition.Compute(yearPhase: 0.25, timeOfDay: 0.30).AzimuthDegrees;
        double afternoon = SolarPosition.Compute(yearPhase: 0.25, timeOfDay: 0.70).AzimuthDegrees;

        Assert.InRange(morning, 45.0, 135.0);
        Assert.InRange(afternoon, 225.0, 315.0);
    }

    [Fact]
    public void MoonIsFullAtMidPhaseAndNewAtZero()
    {
        double newMoon = LunarPosition.Compute(0.3, 0.5, lunarPhase: 0.0).IlluminatedFraction;
        double fullMoon = LunarPosition.Compute(0.3, 0.5, lunarPhase: 0.5).IlluminatedFraction;

        Assert.Equal(0.0, newMoon, 6);
        Assert.Equal(1.0, fullMoon, 6);
    }

    [Fact]
    public void FullMoonRidesTheNightAndNewMoonRidesTheDay()
    {
        // The ~30-day rhythm that decides which nights are navigable (docs/02 §6).
        // A full moon must be up at midnight; a new moon must not.
        LunarPosition fullAtMidnight = LunarPosition.Compute(0.25, timeOfDay: 0.0, lunarPhase: 0.5);
        LunarPosition newAtMidnight = LunarPosition.Compute(0.25, timeOfDay: 0.0, lunarPhase: 0.0);

        Assert.True(fullAtMidnight.IsUp, "A full moon should be up at solar midnight.");
        Assert.False(newAtMidnight.IsUp, "A new moon should be down at solar midnight.");
        Assert.True(fullAtMidnight.IlluminanceLux > 0.1, "A full moon should light the ground.");
        Assert.Equal(0.0, newAtMidnight.IlluminanceLux);
    }

    [Fact]
    public void HalfMoonIsFarDimmerThanHalfOfAFullMoon()
    {
        // The opposition surge. Only the few nights around full are worth travelling on.
        double full = LunarPosition.Compute(0.25, 0.0, 0.50).IlluminanceLux;
        double half = LunarPosition.Compute(0.25, 0.0, 0.25).IlluminanceLux;

        Assert.True(half < full * 0.3,
            $"A half moon ({half:F4} lux) should be well under a third of a full moon ({full:F4} lux).");
    }
}
