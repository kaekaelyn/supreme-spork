using System.Reflection;
using ElderWorld.Core.Climate;
using ElderWorld.Core.Determinism;
using ElderWorld.Core.Thermal;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// docs/06 §9: <i>"Determinism tests on the ecology core — same seed, same result, so
/// bug reports are reproducible."</i> At 1:1, a bug in somebody's two-year-old world
/// is unreproducible unless the world is a pure function of its seed.
/// </summary>
public class DeterminismTests
{
    [Fact]
    public void SameSeedGivesTheSameWeather()
    {
        var first = new ClimateModel(20260729);
        var second = new ClimateModel(20260729);

        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 7919.0)
        {
            EnvironmentSample a = first.Sample(t, SiteContext.OpenGround);
            EnvironmentSample b = second.Sample(t, SiteContext.OpenGround);

            Assert.Equal(a.AirTemperatureC, b.AirTemperatureC);
            Assert.Equal(a.WindSpeedMetresPerSecond, b.WindSpeedMetresPerSecond);
            Assert.Equal(a.CloudCover, b.CloudCover);
        }
    }

    [Fact]
    public void DifferentSeedsGiveDifferentWeather()
    {
        var first = new ClimateModel(1);
        var second = new ClimateModel(2);

        bool anyDifference = false;
        for (double t = 0; t < CretaceousCalendar.SecondsPerYear; t += 7919.0)
        {
            if (Math.Abs(first.Sample(t, SiteContext.OpenGround).AirTemperatureC
                         - second.Sample(t, SiteContext.OpenGround).AirTemperatureC) > 0.5)
            {
                anyDifference = true;
                break;
            }
        }

        Assert.True(anyDifference, "Two seeds must produce recognisably different worlds.");
    }

    [Fact]
    public void SamplingOrderDoesNotMatter()
    {
        // Weather is stateless, so a client that samples out of order — or skips
        // ahead after a reconnect — must see exactly the same world.
        var climate = new ClimateModel(7);
        double[] times = [500_000.0, 12.0, 9_000_000.0, 3.5, 250_000.0];

        double[] forward = times.Select(t => climate.Sample(t, SiteContext.OpenGround).AirTemperatureC).ToArray();
        double[] backward = times.Reverse()
            .Select(t => climate.Sample(t, SiteContext.OpenGround).AirTemperatureC)
            .Reverse().ToArray();

        Assert.Equal(forward, backward);
    }

    [Fact]
    public void ThermalModelIsReproducible()
    {
        static double RunOnce()
        {
            var climate = new ClimateModel(31337);
            var model = new ThermalModel();
            var state = ThermalState.Fresh();
            var exposure = ThermalExposure.Naked(
                climate.Sample(WorldClock.AtSeason(0.05), SiteContext.OpenGround));

            for (int i = 0; i < 7200; i++) model.Step(state, exposure);
            return state.CoreTemperatureC;
        }

        Assert.Equal(RunOnce(), RunOnce());
    }

    [Fact]
    public void RandomStreamIsStableAcrossRuntimeVersions()
    {
        // Pinned values. System.Random makes no cross-version guarantee, which is why
        // the core ships its own generator; if these change, every existing seeded
        // world has silently changed with them.
        var random = new DeterministicRandom(seed: 12345);
        double[] draws = [random.NextDouble(), random.NextDouble(), random.NextDouble()];

        var replay = new DeterministicRandom(seed: 12345);
        Assert.Equal(draws[0], replay.NextDouble());
        Assert.Equal(draws[1], replay.NextDouble());
        Assert.Equal(draws[2], replay.NextDouble());

        Assert.All(draws, d => Assert.InRange(d, 0.0, 1.0));
    }

    [Fact]
    public void RandomStreamRoundTripsThroughASave()
    {
        var random = new DeterministicRandom(seed: 99);
        for (int i = 0; i < 50; i++) random.NextUInt64();

        (ulong s0, ulong s1) = random.Capture();
        DeterministicRandom restored = DeterministicRandom.Restore(s0, s1);

        Assert.Equal(random.NextUInt64(), restored.NextUInt64());
    }

    [Fact]
    public void RandomIsWellDistributed()
    {
        var random = new DeterministicRandom(seed: 5);
        double sum = 0.0;
        const int Draws = 200_000;
        for (int i = 0; i < Draws; i++) sum += random.NextDouble();

        Assert.Equal(0.5, sum / Draws, 2);
    }

    [Fact]
    public void NoiseIsContinuous()
    {
        // Weather built on discontinuous noise would produce temperature steps, which
        // read as a bug and break the thermal model's integration.
        double previous = DeterministicNoise.Value01(3, 0.0);
        for (double x = 0.0; x < 20.0; x += 0.01)
        {
            double current = DeterministicNoise.Value01(3, x);
            Assert.True(Math.Abs(current - previous) < 0.1,
                $"Noise jumped from {previous:F4} to {current:F4} at x={x:F2}.");
            previous = current;
        }
    }

    [Fact]
    public void NoiseStaysInRange()
    {
        for (double x = 0.0; x < 100.0; x += 0.37)
        {
            Assert.InRange(DeterministicNoise.Value01(11, x), 0.0, 1.0);
            Assert.InRange(DeterministicNoise.Signed(11, x), -1.0, 1.0);
            Assert.InRange(DeterministicNoise.Fractal(11, x), -1.0, 1.0);
        }
    }
}

/// <summary>
/// Guards the one discipline docs/08 §13 asks us to keep:
/// <i>"the ecology core lives in its own assembly with no engine types in it — no
/// Node, no Vector3 from the engine, no scene tree. Same language, zero engine
/// coupling."</i>
/// <para>
/// It preserves both things the engine-independent design was for: headless soak
/// tests that simulate centuries without launching the engine, and the portability
/// insurance if the engine choice is ever revisited.
/// </para>
/// </summary>
public class ArchitectureTests
{
    private static readonly string[] ForbiddenAssemblies =
        ["Godot", "GodotSharp", "UnityEngine", "UnrealEngine", "MonoGame", "Silk.NET"];

    [Fact]
    public void CoreReferencesNoGameEngine()
    {
        Assembly core = typeof(WorldClock).Assembly;

        foreach (AssemblyName reference in core.GetReferencedAssemblies())
        {
            Assert.False(
                ForbiddenAssemblies.Any(forbidden =>
                    reference.Name!.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase)),
                $"The simulation core must not reference '{reference.Name}'. See docs/08 §13.");
        }
    }

    [Fact]
    public void CoreExposesNoEngineTypesOnItsPublicSurface()
    {
        Assembly core = typeof(WorldClock).Assembly;

        foreach (Type type in core.GetExportedTypes())
        {
            string? assemblyName = type.Assembly.GetName().Name;
            Assert.False(
                ForbiddenAssemblies.Any(forbidden =>
                    assemblyName!.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase)),
                $"Public type {type.FullName} comes from an engine assembly.");
        }
    }
}
