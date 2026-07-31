using ElderWorld.Core.Climate;
using ElderWorld.Core.Fire;
using ElderWorld.Core.Thermal;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// docs/07 §2 makes this a hard constraint rather than a nicety:
/// <i>"A time-scale debug lever (up to several thousand ×) available in dev builds and
/// never in shipping ones. Everything must be correct at any rate — which is a genuine
/// constraint on how systems are written, and much cheaper to enforce from the start
/// than to retrofit."</i>
/// <para>
/// If these fail, a bug found during a 1000× soak run will not reproduce at 1×, and a
/// balance pass done at 1× will be wrong for everyone who plays the game. They are the
/// most important tests in the suite.
/// </para>
/// </summary>
public class TimeScaleInvarianceTests
{
    private const double OneHourOfWorldTime = 3600.0;

    private static EnvironmentSample ColdNight() => new ClimateModel(4242)
        .Sample(WorldClock.AtSeason(0.03, timeOfDay: 0.05), SiteContext.ForestUnderstory);

    /// <summary>
    /// Runs a body for a fixed span of world time, delivered at a given time scale
    /// through the accumulator, exactly as the engine layer does each frame.
    /// </summary>
    private static ThermalState RunAtTimeScale(double timeScale, double worldSeconds)
    {
        var clock = new WorldClock { TimeScale = timeScale };
        var accumulator = new FixedStepAccumulator(ThermalModel.StepSeconds);
        var model = new ThermalModel();
        var state = ThermalState.Fresh();
        var exposure = ThermalExposure.Naked(ColdNight());

        const double FrameSeconds = 1.0 / 60.0;
        double target = worldSeconds;

        while (clock.TotalSeconds < target)
        {
            double advanced = clock.Advance(FrameSeconds);
            int steps = accumulator.Pump(advanced);
            for (int i = 0; i < steps; i++)
                model.Step(state, exposure);
        }

        return state;
    }

    [Theory]
    [InlineData(10.0)]
    [InlineData(100.0)]
    [InlineData(1000.0)]
    public void ThermalModelGivesTheSameAnswerAtAnyTimeScale(double timeScale)
    {
        ThermalState atRealTime = RunAtTimeScale(1.0, OneHourOfWorldTime);
        ThermalState accelerated = RunAtTimeScale(timeScale, OneHourOfWorldTime);

        // A frame boundary can land one fixed step either side of the target, so a
        // single step's worth of drift is expected — about 0.006 °C of core here.
        // Anything larger means a model is integrating against frame time somewhere.
        const double OneStepOfDrift = 0.05;

        Assert.True(Math.Abs(atRealTime.CoreTemperatureC - accelerated.CoreTemperatureC) < OneStepOfDrift,
            $"Core differed by {Math.Abs(atRealTime.CoreTemperatureC - accelerated.CoreTemperatureC):F4} °C " +
            $"between 1× and {timeScale}×.");
        Assert.True(Math.Abs(atRealTime.MeanSkinTemperatureC - accelerated.MeanSkinTemperatureC) < OneStepOfDrift);
        Assert.True(Math.Abs(atRealTime.SkinTemperature(BodyZone.Hands)
                             - accelerated.SkinTemperature(BodyZone.Hands)) < OneStepOfDrift);
    }

    [Fact]
    public void FrameRateDoesNotChangeTheOutcome()
    {
        // The same world time delivered as 30 fps frames and as 144 fps frames.
        // A player on a slower machine must not get a different body.
        static ThermalState RunAtFrameRate(double fps)
        {
            var accumulator = new FixedStepAccumulator(ThermalModel.StepSeconds);
            var model = new ThermalModel();
            var state = ThermalState.Fresh();
            var exposure = ThermalExposure.Naked(ColdNight());

            double elapsed = 0.0;
            while (elapsed < OneHourOfWorldTime)
            {
                elapsed += 1.0 / fps;
                int steps = accumulator.Pump(1.0 / fps);
                for (int i = 0; i < steps; i++) model.Step(state, exposure);
            }

            return state;
        }

        Assert.Equal(RunAtFrameRate(30.0).CoreTemperatureC, RunAtFrameRate(144.0).CoreTemperatureC, 2);
    }

    [Fact]
    public void WeatherIsIdenticalWhateverTheTimeScale()
    {
        // Weather is a pure function of world time rather than a stepped random walk,
        // so this holds by construction. Asserted anyway, because the day somebody
        // makes it stateful the whole soak-testing strategy quietly breaks.
        var climate = new ClimateModel(99);

        var slow = new WorldClock();
        var fast = new WorldClock { TimeScale = 5000.0 };

        for (int i = 0; i < 500; i++) slow.Advance(10.0);
        fast.Advance(1.0);

        Assert.Equal(slow.TotalSeconds, fast.TotalSeconds, 6);
        Assert.Equal(
            climate.Sample(slow.TotalSeconds, SiteContext.OpenGround).AirTemperatureC,
            climate.Sample(fast.TotalSeconds, SiteContext.OpenGround).AirTemperatureC, 9);
    }

    [Fact]
    public void FireBurnsTheSameAmountOfFuelAtAnyTimeScale()
    {
        static double RemainingFuelAfter(double sliceSeconds)
        {
            var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.8);
            hearth.SetBedTemperature(650.0);
            for (int i = 0; i < 4; i++)
                hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.15, 0.04));

            EnvironmentSample night = ColdNight();
            double elapsed = 0.0;
            while (elapsed < OneHourOfWorldTime)
            {
                hearth.Advance(night, sliceSeconds);
                elapsed += sliceSeconds;
            }

            return hearth.RemainingDryMassKg;
        }

        // One hour delivered as 1/60 s slivers, as 1 s ticks, and as 60 s slabs.
        double fine = RemainingFuelAfter(1.0 / 60.0);
        double normal = RemainingFuelAfter(1.0);
        double coarse = RemainingFuelAfter(60.0);

        Assert.Equal(fine, normal, 2);
        Assert.Equal(normal, coarse, 2);
    }

    [Fact]
    public void AccumulatorCarriesItsRemainderRatherThanLosingIt()
    {
        // Time dropped on the floor each frame would make the world run slow, and at
        // 1:1 over real years that error compounds without bound.
        var accumulator = new FixedStepAccumulator(1.0);

        int total = 0;
        for (int i = 0; i < 600; i++) total += accumulator.Pump(1.0 / 60.0);

        Assert.Equal(10, total);
        Assert.True(accumulator.Pending < 1e-9, "Ten seconds of world time should leave no remainder.");
    }

    [Fact]
    public void AccumulatorClampsAnAbsurdSlabRatherThanHanging()
    {
        var accumulator = new FixedStepAccumulator(1.0);
        int steps = accumulator.Pump(1_000_000.0);

        Assert.Equal(FixedStepAccumulator.MaxStepsPerPump, steps);
        Assert.True(accumulator.LastPumpWasClamped);
    }
}
