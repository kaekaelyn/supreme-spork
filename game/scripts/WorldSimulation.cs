using ElderWorld.Core.Climate;
using ElderWorld.Core.Time;
using Godot;

namespace ElderWorld.Game;

/// <summary>
/// The bridge between the engine and the simulation core. Owns the world clock and
/// the climate, advances the clock once per frame, and lets everything else sample
/// the world.
/// <para>
/// This is the <b>only</b> node permitted to advance time. Everything else reads
/// <see cref="LastWorldDelta"/> and steps its own model through a fixed-step
/// accumulator, so the whole simulation stays frame-rate and time-scale independent
/// (docs/07 §2).
/// </para>
/// </summary>
public partial class WorldSimulation : Node
{
    /// <summary>
    /// Where the world starts.
    /// <para>
    /// Autumn, an hour or two before sunset, because the Stage 1 gate in docs/09 §8
    /// is <i>"is the night compelling?"</i> and the answer needs the player to watch
    /// the light go rather than to spawn into darkness. docs/08 §8 is clear that a
    /// real server picks any season it likes, and that a winter start will probably
    /// kill you.
    /// </para>
    /// </summary>
    [Export] public double FoundingSeason { get; set; } = 0.80;

    /// <summary>Time of day at founding. 0.5 is solar noon; sunset in autumn is near 0.73.</summary>
    [Export] public double FoundingTimeOfDay { get; set; } = 0.66;

    /// <summary>World seed. The same seed always gives the same weather (docs/06 §9).</summary>
    [Export] public int WorldSeed { get; set; } = 20260729;

    /// <summary>The authoritative clock.</summary>
    public WorldClock Clock { get; private set; } = null!;

    /// <summary>The weather.</summary>
    public ClimateModel Climate { get; private set; } = null!;

    /// <summary>World seconds that passed in the frame just processed.</summary>
    public double LastWorldDelta { get; private set; }

    public override void _Ready()
    {
        // Runs before every other node's _Process, so the clock is always current by
        // the time anything reads it.
        ProcessPriority = -1000;

        Clock = new WorldClock(WorldClock.AtSeason(FoundingSeason, FoundingTimeOfDay));
        Climate = new ClimateModel((uint)WorldSeed);
    }

    public override void _Process(double delta)
        => LastWorldDelta = Clock.Advance(delta);

    /// <summary>Samples the environment at a place, right now.</summary>
    public EnvironmentSample SampleAt(SiteContext site) => Climate.Sample(Clock, site);
}
