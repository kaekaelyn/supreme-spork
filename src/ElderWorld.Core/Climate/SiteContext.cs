namespace ElderWorld.Core.Climate;

/// <summary>
/// Where you are standing, as far as the weather is concerned.
/// <para>
/// This is the structure behind one of the design's stated skills: <i>"Windchill on
/// the exposed ridges versus stillness in the forest understory is a legible,
/// learnable map feature. Reading terrain for shelter is a skill."</i> (docs/02 §1).
/// The same air temperature is survivable in a hollow and lethal on a ridge, and
/// nothing ever tells the player that.
/// </para>
/// </summary>
public readonly record struct SiteContext
{
    /// <summary>
    /// Height above sea level. The basin floor sits at ~2,000 m rising to ~3,000 m
    /// on the volcanic ridges (docs/01 §2), so elevation alone is worth about 6.5 °C
    /// across the map.
    /// </summary>
    public double ElevationMetres { get; init; }

    /// <summary>
    /// How much of the free-stream wind reaches you, 0–1. Deep forest understory is
    /// around 0.15; an exposed ridge is 1.0.
    /// </summary>
    public double WindExposure { get; init; }

    /// <summary>
    /// How much of the sky hemisphere you can see, 0–1. Open ground is 1.0, closed
    /// canopy about 0.25, inside a shelter near 0.
    /// <para>
    /// This is the single most underrated number in the model. It governs radiative
    /// loss to the sky, and on a clear winter night the sky behaves like a surface
    /// 25 °C colder than the air. Standing in the open and standing under a conifer
    /// are genuinely different survival propositions, which is why a debris shelter
    /// with a roof is worth building even when it does nothing for the wind.
    /// </para>
    /// </summary>
    public double SkyViewFactor { get; init; }

    /// <summary>Open, flat, exposed ground on the basin floor. The default place to wake up.</summary>
    public static SiteContext OpenGround { get; } = new()
    {
        ElevationMetres = 2000.0,
        WindExposure = 1.0,
        SkyViewFactor = 1.0,
    };

    /// <summary>Closed conifer forest: most of the wind gone, most of the sky gone.</summary>
    public static SiteContext ForestUnderstory { get; } = new()
    {
        ElevationMetres = 2000.0,
        WindExposure = 0.15,
        SkyViewFactor = 0.25,
    };

    /// <summary>An exposed volcanic ridge at 3,000 m. Where you go for stone, and where you die.</summary>
    public static SiteContext ExposedRidge { get; } = new()
    {
        ElevationMetres = 3000.0,
        WindExposure = 1.0,
        SkyViewFactor = 1.0,
    };

    /// <summary>Inside a debris shelter: no wind, almost no sky.</summary>
    public static SiteContext DebrisShelter { get; } = new()
    {
        ElevationMetres = 2000.0,
        WindExposure = 0.05,
        SkyViewFactor = 0.05,
    };

    /// <summary>Validates and clamps the fields into their legal ranges.</summary>
    public SiteContext Normalised() => this with
    {
        WindExposure = Math.Clamp(WindExposure, 0.0, 1.0),
        SkyViewFactor = Math.Clamp(SkyViewFactor, 0.0, 1.0),
    };
}
