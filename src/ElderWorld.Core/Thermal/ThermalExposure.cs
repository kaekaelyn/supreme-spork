using ElderWorld.Core.Climate;

namespace ElderWorld.Core.Thermal;

/// <summary>
/// Everything acting on the body this step: the weather, what you are wearing, what
/// you are doing, and what is burning nearby.
/// </summary>
public readonly record struct ThermalExposure
{
    /// <summary>The weather and light at this place and moment.</summary>
    public required EnvironmentSample Environment { get; init; }

    /// <summary>What you are wearing.</summary>
    public Insulation Clothing { get; init; } = Insulation.Naked;

    /// <summary>Standing, sitting, lying, or curled.</summary>
    public Posture Posture { get; init; } = Posture.Standing;

    /// <summary>
    /// Activity level in met, above the posture's floor. 1 met ≈ 58.2 W/m².
    /// Walking is about 2.9, hard work about 4, running about 8.
    /// </summary>
    public double ActivityMet { get; init; } = 1.2;

    /// <summary>
    /// Radiant flux arriving from fire, W/m² on the facing side.
    /// Use <c>Hearth.IrradianceAt</c> to compute it from a fire and a distance.
    /// </summary>
    public double FireIrradianceWattsPerM2 { get; init; }

    /// <summary>
    /// Thermal resistance of whatever is between you and the ground, m²·K/W.
    /// Bare ground is 0. A thick bed of conifer boughs is around 0.5, which is the
    /// difference between waking up and not.
    /// </summary>
    public double BeddingResistance { get; init; }

    /// <summary>
    /// True when the body is in water.
    /// <para>
    /// docs/02 §1: <i>"Falling in a lake in autumn should be a genuine emergency
    /// with a countdown measured in minutes."</i> Water carries heat away about 25×
    /// faster than still air, and this flag is what makes that countdown real.
    /// </para>
    /// </summary>
    public bool Immersed { get; init; }

    /// <summary>Creates an exposure with the defaults above.</summary>
    public ThermalExposure() { }

    /// <summary>
    /// The naked, standing, fireless case: how the game begins, and the baseline
    /// every reference scenario measures against.
    /// </summary>
    public static ThermalExposure Naked(EnvironmentSample environment) => new()
    {
        Environment = environment,
        Clothing = Insulation.Naked,
        Posture = Posture.Standing,
        ActivityMet = Postures.BaseActivityMet(Posture.Standing),
    };
}
