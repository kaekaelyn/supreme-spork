using ElderWorld.Core.Climate;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// The climate is the design decision the entire game hangs on (docs/01 §2), so the
/// envelope is pinned against the two isotope studies rather than against taste.
/// </summary>
public class ClimateTests
{
    private const uint Seed = 12345;

    private sealed record YearStats(double Mean, double Min, double Max);

    private static YearStats SampleAYear(SiteContext site, uint seed = Seed)
    {
        var climate = new ClimateModel(seed);
        double sum = 0.0, min = double.MaxValue, max = double.MinValue;
        int count = 0;

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 900.0)
        {
            double temperature = climate.Sample(t, site).AirTemperatureC;
            sum += temperature;
            min = Math.Min(min, temperature);
            max = Math.Max(max, temperature);
            count++;
        }

        return new YearStats(sum / count, min, max);
    }

    [Fact]
    public void MeanAnnualTemperatureMatchesTheDesignRuling()
    {
        // docs/01 §2 rules on ~7 °C, which sits inside both the clumped-isotope
        // (5.9 ± 1.7 °C) and oxygen-isotope (10 ± 4 °C) envelopes.
        YearStats stats = SampleAYear(SiteContext.OpenGround);
        Assert.InRange(stats.Mean, 6.0, 8.0);
    }

    [Fact]
    public void MeanAnnualTemperatureHoldsAcrossSeeds()
    {
        foreach (uint seed in new uint[] { 1, 7, 4242, 999_999 })
            Assert.InRange(SampleAYear(SiteContext.OpenGround, seed).Mean, 5.5, 8.5);
    }

    [Fact]
    public void WinterIsDeepAndSummerIsMild()
    {
        // docs/01 §2: "seasonally swinging from about −20 °C in deep winter to 24 °C
        // in high summer". Those two figures and the 7 °C mean cannot all come from a
        // symmetric model — their midpoint is +2 °C, not +7 °C — so the model is
        // cold-skewed by winter inversions, and lands near all three.
        YearStats stats = SampleAYear(SiteContext.OpenGround);

        Assert.InRange(stats.Min, -24.0, -14.0);
        Assert.InRange(stats.Max, 20.0, 28.0);
    }

    [Fact]
    public void ItFreezesForMonthsEveryYear()
    {
        // docs/02 §6: "Winter — ~4–5 real months below freezing."
        var climate = new ClimateModel(Seed);
        int freezingSamples = 0, total = 0;

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 3600.0)
        {
            if (climate.Sample(t, SiteContext.OpenGround).AirTemperatureC < 0.0) freezingSamples++;
            total++;
        }

        double monthsBelowFreezing = freezingSamples / (double)total * 12.0;
        Assert.InRange(monthsBelowFreezing, 3.0, 6.0);
    }

    [Fact]
    public void ClearNightsAreBrutallyColderThanOvercastOnes()
    {
        // docs/02 §1: "Clear nights are brutally colder than overcast ones because
        // radiative loss to a clear sky is real and models nicely." This is the single
        // most important environmental effect in the thermal model.
        var climate = new ClimateModel(Seed);
        double clearDepression = 0.0, overcastDepression = 0.0;

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 900.0)
        {
            EnvironmentSample sample = climate.Sample(t, SiteContext.OpenGround);
            if (sample.Sun.IsUp) continue;

            double depression = sample.AirTemperatureC - sample.SkyTemperatureC;
            if (sample.CloudCover < 0.15) clearDepression = Math.Max(clearDepression, depression);
            if (sample.CloudCover > 0.90) overcastDepression = Math.Max(overcastDepression, depression);
        }

        Assert.InRange(clearDepression, 18.0, 35.0);
        Assert.True(overcastDepression < 8.0,
            $"An overcast sky should radiate near air temperature, was {overcastDepression:F1} K below.");
    }

    [Fact]
    public void RidgesAreColderAndWindierThanTheBasinFloor()
    {
        // docs/02 §1's learnable map feature, and the lapse rate that produces it.
        var climate = new ClimateModel(Seed);
        double when = WorldClock.AtSeason(0.35, timeOfDay: 0.4);

        EnvironmentSample basin = climate.Sample(when, SiteContext.OpenGround);
        EnvironmentSample ridge = climate.Sample(when, SiteContext.ExposedRidge);

        // 1,000 m of ascent at 6.5 °C/km.
        Assert.Equal(basin.AirTemperatureC - 6.5, ridge.AirTemperatureC, 0);
        Assert.True(ridge.WindSpeedMetresPerSecond > basin.WindSpeedMetresPerSecond * 0.9);
    }

    [Fact]
    public void TheForestUnderstoryIsSheltered()
    {
        var climate = new ClimateModel(Seed);
        double when = WorldClock.AtSeason(0.35, timeOfDay: 0.4);

        EnvironmentSample open = climate.Sample(when, SiteContext.OpenGround);
        EnvironmentSample forest = climate.Sample(when, SiteContext.ForestUnderstory);

        Assert.True(forest.WindSpeedMetresPerSecond < open.WindSpeedMetresPerSecond * 0.4);
        Assert.True(forest.Site.SkyViewFactor < open.Site.SkyViewFactor);
    }

    [Fact]
    public void TheColdestPartOfTheYearIsNotTheSolstice()
    {
        // Real seasonal lag, and quietly a trap: a player dating the solstice by feel
        // rather than by watching the sun against the ridge will be three weeks out.
        var climate = new ClimateModel(Seed);

        double coldestPhase = 0.0, coldest = double.MaxValue;
        for (double phase = 0.0; phase < 1.0; phase += 0.001)
        {
            double mean = climate.SeasonalMeanC(phase);
            if (mean < coldest) { coldest = mean; coldestPhase = phase; }
        }

        Assert.True(coldestPhase > 0.02,
            $"The coldest day landed at phase {coldestPhase:F3}, on top of the solstice.");
        Assert.InRange(coldestPhase, 0.03, 0.09);
    }

    [Fact]
    public void TheDailyMinimumArrivesJustBeforeDawn()
    {
        // Which makes the last hour of an eleven-hour night the hardest one.
        var climate = new ClimateModel(Seed);
        double dayStart = WorldClock.AtSeason(0.03, timeOfDay: 0.0);

        double coldestTimeOfDay = 0.0, coldest = double.MaxValue;
        for (double offset = 0; offset < CretaceousCalendar.SecondsPerDay; offset += 300.0)
        {
            double at = dayStart + offset;
            double temperature = climate.Sample(at, SiteContext.OpenGround).AirTemperatureC;
            if (temperature < coldest)
            {
                coldest = temperature;
                double days = at / CretaceousCalendar.SecondsPerDay;
                coldestTimeOfDay = days - Math.Floor(days);
            }
        }

        // The last stretch of an eleven-hour night is the coldest part of it.
        double sunrise = SolarPosition.SunriseTimeOfDay(0.03);
        Assert.InRange(coldestTimeOfDay, sunrise - 0.12, sunrise + 0.06);
    }

    [Fact]
    public void SnowLiesThroughWinterAndIsGoneBySummer()
    {
        var climate = new ClimateModel(Seed);

        Assert.True(climate.Sample(WorldClock.AtSeason(0.02), SiteContext.OpenGround).SnowCoverFraction > 0.9);
        Assert.Equal(0.0, climate.Sample(WorldClock.AtSeason(0.5), SiteContext.OpenGround).SnowCoverFraction);
    }

    [Fact]
    public void LakeWaterLagsTheAirByMonths()
    {
        // The autumn trap: a pleasant afternoon over water that is still only a few
        // degrees, which is what makes falling in an emergency.
        var climate = new ClimateModel(Seed);

        EnvironmentSample lateSpring = climate.Sample(
            WorldClock.AtSeason(0.35, timeOfDay: 0.6), SiteContext.OpenGround);

        Assert.True(lateSpring.WaterTemperatureC < lateSpring.AirTemperatureC,
            "In spring the lake should still be colder than the air.");
        Assert.True(lateSpring.WaterTemperatureC >= 0.0, "Water cannot go below freezing.");
    }

    [Fact]
    public void PrecipitationFallsAsSnowWhenItIsColdEnough()
    {
        var climate = new ClimateModel(Seed);
        int rainBelowFreezing = 0, snowSeen = 0;

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 900.0)
        {
            EnvironmentSample sample = climate.Sample(t, SiteContext.OpenGround);
            if (sample.Precipitation == PrecipitationKind.Snow) snowSeen++;
            if (sample.Precipitation == PrecipitationKind.Rain && sample.AirTemperatureC < 0.0)
                rainBelowFreezing++;
        }

        Assert.True(snowSeen > 0, "It should snow.");
        Assert.Equal(0, rainBelowFreezing);
    }

    [Fact]
    public void MostOfTheTimeItIsNotRaining()
    {
        // A world where it always rains is a world where fire never works.
        var climate = new ClimateModel(Seed);
        int dry = 0, total = 0;

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 900.0)
        {
            if (climate.Sample(t, SiteContext.OpenGround).Precipitation == PrecipitationKind.None) dry++;
            total++;
        }

        Assert.InRange(dry / (double)total, 0.6, 0.98);
    }

    [Fact]
    public void EverySampleIsPhysicallyPlausible()
    {
        // A NaN or an absurd value anywhere here poisons the thermal model silently.
        var climate = new ClimateModel(Seed);

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 1800.0)
        {
            foreach (SiteContext site in new[]
                     { SiteContext.OpenGround, SiteContext.ForestUnderstory, SiteContext.ExposedRidge })
            {
                EnvironmentSample sample = climate.Sample(t, site);

                Assert.False(double.IsNaN(sample.AirTemperatureC));
                Assert.InRange(sample.AirTemperatureC, -45.0, 40.0);
                Assert.InRange(sample.RelativeHumidity, 0.0, 1.0);
                Assert.InRange(sample.CloudCover, 0.0, 1.0);
                Assert.InRange(sample.SnowCoverFraction, 0.0, 1.0);
                Assert.True(sample.WindSpeedMetresPerSecond >= 0.0);
                Assert.True(sample.SolarIrradianceWattsPerM2 >= 0.0);
                Assert.True(sample.SkyTemperatureC <= sample.AirTemperatureC + 0.5,
                    "The sky is never warmer than the air.");
                Assert.True(sample.IlluminanceLux >= 0.0);
            }
        }
    }

    [Fact]
    public void NightIsGenuinelyDarkAndDaylightIsNot()
    {
        var climate = new ClimateModel(Seed);

        EnvironmentSample noon = climate.Sample(
            WorldClock.AtSeason(0.5, timeOfDay: 0.5), SiteContext.OpenGround);
        Assert.True(noon.IlluminanceLux > 10_000.0, "Midsummer noon should be bright daylight.");
        Assert.False(noon.IsDark);

        // A moonless midwinter night in closed forest is the darkest the world gets.
        double darkest = double.MaxValue;
        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 900.0)
        {
            EnvironmentSample sample = climate.Sample(t, SiteContext.ForestUnderstory);
            if (!sample.Sun.IsUp) darkest = Math.Min(darkest, sample.IlluminanceLux);
        }

        Assert.True(darkest < 0.01, $"The darkest night was {darkest:F4} lux.");
    }

    [Fact]
    public void SnowMakesAMoonlitNightBrighter()
    {
        // docs/02 §6: "Snow has albedo. A snow-covered landscape under a gibbous moon
        // is genuinely navigable."
        var bare = new EnvironmentSample
        {
            SnowCoverFraction = 0.0,
            CloudCover = 0.0,
            Site = SiteContext.OpenGround,
            Moon = LunarPosition.Compute(0.0, timeOfDay: 0.0, lunarPhase: 0.5),
        };
        EnvironmentSample snowy = bare with { SnowCoverFraction = 1.0 };

        Assert.True(snowy.IlluminanceLux > bare.IlluminanceLux * 1.5);
    }

    [Fact]
    public void DewPointIsNeverAboveAirTemperature()
    {
        for (double temperature = -30.0; temperature <= 35.0; temperature += 2.5)
        {
            for (double humidity = 0.05; humidity <= 1.0; humidity += 0.05)
            {
                double dewPoint = ClimateModel.DewPointC(temperature, humidity);
                Assert.True(dewPoint <= temperature + 0.01,
                    $"Dew point {dewPoint:F2} exceeded air {temperature:F2} at RH {humidity:F2}.");
            }
        }
    }
}
