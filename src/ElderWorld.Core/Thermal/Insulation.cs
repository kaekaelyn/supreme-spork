namespace ElderWorld.Core.Thermal;

/// <summary>
/// What you are wearing, per zone, in clo.
/// <para>
/// 1 clo = 0.155 m²·K/W, defined as what a resting person needs to stay comfortable
/// at 21 °C — roughly a business suit. You start the game at 0.00.
/// </para>
/// <para>
/// docs/02 §1: <i>"Down is the best insulator available, and it is everywhere in
/// this world, which is the material identity of the whole game."</i> The two
/// modifiers below are why down alone is not enough: an unshelled down garment is
/// wind-permeable, and wet down is worth almost nothing. Getting from
/// <see cref="Naked"/> to <see cref="DownAndHide"/> is most of the first year.
/// </para>
/// </summary>
public readonly record struct Insulation
{
    /// <summary>Insulation over the head, clo.</summary>
    public double HeadClo { get; init; }

    /// <summary>Insulation over torso, arms and legs, clo.</summary>
    public double CoreClo { get; init; }

    /// <summary>Insulation over the hands, clo.</summary>
    public double HandsClo { get; init; }

    /// <summary>Insulation over the feet, clo.</summary>
    public double FeetClo { get; init; }

    /// <summary>
    /// How readily wind blows through, 0 = windproof to 1 = open weave.
    /// <para>
    /// Raw down and loose fibre are near 1: they trap air beautifully until air
    /// moves through them, at which point they stop working. A hide or fish-skin
    /// shell over the top is what fixes it, and that is a separate technology from
    /// the down itself.
    /// </para>
    /// </summary>
    public double WindPermeability { get; init; }

    /// <summary>
    /// How wet the clothing is, 0–1.
    /// <para>
    /// docs/02 §1: <i>"being wet multiplies conductive loss catastrophically."</i>
    /// Soaked insulation retains well under a fifth of its dry value, and then goes
    /// on evaporating into you. This single number is the difference between rain
    /// being weather and rain being an emergency.
    /// </para>
    /// </summary>
    public double Wetness { get; init; }

    /// <summary>How you wake up. Naked, at 7 °C, at altitude, with the clock already running.</summary>
    public static Insulation Naked { get; } = new()
    {
        HeadClo = 0.0,
        CoreClo = 0.0,
        HandsClo = 0.0,
        FeetClo = 0.0,
        WindPermeability = 1.0,
        Wetness = 0.0,
    };

    /// <summary>
    /// A first attempt: untailored rawhide over the core, nothing on the extremities.
    /// Tier 1 work, and it is not enough for a winter night.
    /// </summary>
    public static Insulation RawhideWrap { get; } = new()
    {
        HeadClo = 0.0,
        CoreClo = 1.1,
        HandsClo = 0.0,
        FeetClo = 0.3,
        WindPermeability = 0.35,
        Wetness = 0.0,
    };

    /// <summary>
    /// The tier 3 target: down-stuffed, sewn, hide-shelled, covering every zone.
    /// docs/03 §3 calls this "the single most valuable technology in the game".
    /// </summary>
    public static Insulation DownAndHide { get; } = new()
    {
        HeadClo = 2.5,
        CoreClo = 4.0,
        HandsClo = 2.0,
        FeetClo = 2.5,
        WindPermeability = 0.10,
        Wetness = 0.0,
    };

    /// <summary>Insulation over a given zone, clo.</summary>
    public double CloFor(BodyZone zone) => zone switch
    {
        BodyZone.Head => HeadClo,
        BodyZone.Core => CoreClo,
        BodyZone.Hands => HandsClo,
        BodyZone.Feet => FeetClo,
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    /// <summary>Area-weighted mean insulation across the whole body, clo.</summary>
    public double MeanClo
    {
        get
        {
            double total = 0.0;
            foreach (BodyZone zone in BodyZones.All)
                total += CloFor(zone) * BodyZones.SurfaceAreaFraction(zone);
            return total;
        }
    }

    /// <summary>
    /// Effective thermal resistance of a zone's clothing, m²·K/W, after wind has
    /// blown through it and water has filled it.
    /// </summary>
    public double EffectiveResistance(BodyZone zone, double windSpeedMetresPerSecond)
    {
        double clo = CloFor(zone);
        if (clo <= 0.0) return 0.0;

        const double CloToSi = 0.155;
        double dry = clo * CloToSi;

        // Wind strips a permeable garment's trapped air. A windproof shell keeps
        // nearly all of it; raw down keeps little.
        double permeability = Math.Clamp(WindPermeability, 0.0, 1.0);
        double windPenalty = Math.Exp(-0.25 * permeability * Math.Max(0.0, windSpeedMetresPerSecond));

        // Water conducts about 25× better than still air, and it displaces the air.
        double wetPenalty = 1.0 - 0.85 * Math.Clamp(Wetness, 0.0, 1.0);

        return dry * windPenalty * wetPenalty;
    }

    /// <summary>
    /// Clothing area factor: bundled clothing has more outer surface than bare skin,
    /// so it presents more area to convect and radiate from. ISO 9920's linear fit.
    /// </summary>
    public double AreaFactor(BodyZone zone) => 1.0 + 0.31 * CloFor(zone);

    /// <summary>
    /// Evaporative resistance of a zone's clothing, m²·kPa/W. Sweat has to get out
    /// through the same layers that keep heat in.
    /// </summary>
    public double EvaporativeResistance(BodyZone zone)
    {
        // Re ≈ 0.155·clo / (Lewis ratio × permeability index). Collapsed to a constant.
        const double PerCloResistance = 0.016;
        return CloFor(zone) * PerCloResistance;
    }

    /// <summary>Returns a copy at a given wetness.</summary>
    public Insulation WithWetness(double wetness) => this with { Wetness = Math.Clamp(wetness, 0.0, 1.0) };
}
