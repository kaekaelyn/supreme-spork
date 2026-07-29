namespace ElderWorld.Core.Fire;

/// <summary>
/// A kind of fuel, with the physical properties that decide how it burns.
/// <para>
/// Everything here is drawn from what docs/03 §2 says is actually present in a
/// volcanic basin dominated by conifers. <b>The names are ours, not the game's</b> —
/// the player is never told what any of this is and will work out for themselves
/// that the resinous deadwood catches and the green stuff does not (docs/00 §3).
/// </para>
/// </summary>
public sealed record FuelSpecies
{
    /// <summary>Internal identifier. Never shown to the player.</summary>
    public required string Id { get; init; }

    /// <summary>Energy released per kg of bone-dry material, J/kg.</summary>
    public required double CalorificValueJPerKg { get; init; }

    /// <summary>Bulk density of the dry material, kg/m³. Decides how much surface a given mass presents.</summary>
    public required double DensityKgPerM3 { get; init; }

    /// <summary>
    /// Resin content, 0–1. Resinous wood lights from a cooler bed, burns hotter and
    /// smokes more. The conifer forest is generous with it, and it is also the glue,
    /// the sealant and the torch (docs/01 §4).
    /// </summary>
    public required double ResinFactor { get; init; }

    /// <summary>Bed temperature needed to ignite this fuel when bone dry, °C.</summary>
    public required double IgnitionTemperatureC { get; init; }

    /// <summary>
    /// Dry conifer deadwood. The staple: straight, abundant, resinous, and lying on
    /// the forest floor waiting to be picked up on the first afternoon.
    /// </summary>
    public static FuelSpecies ConiferDeadwood { get; } = new()
    {
        Id = "conifer_deadwood",
        CalorificValueJPerKg = 19.5e6,
        DensityKgPerM3 = 480.0,
        ResinFactor = 0.7,
        IgnitionTemperatureC = 300.0,
    };

    /// <summary>Conifer bark. Lights easily, burns fast and smokily, roofs a shelter.</summary>
    public static FuelSpecies ConiferBark { get; } = new()
    {
        Id = "conifer_bark",
        CalorificValueJPerKg = 18.0e6,
        DensityKgPerM3 = 320.0,
        ResinFactor = 0.5,
        IgnitionTemperatureC = 260.0,
    };

    /// <summary>Ginkgoalean and cycad wood. Denser, slower, less resin, holds a bed overnight.</summary>
    public static FuelSpecies HardWood { get; } = new()
    {
        Id = "hard_wood",
        CalorificValueJPerKg = 18.6e6,
        DensityKgPerM3 = 680.0,
        ResinFactor = 0.15,
        IgnitionTemperatureC = 380.0,
    };

    /// <summary>
    /// Tinder — bark bast, dry moss, punk wood, bracket fungus. Almost no energy,
    /// but it is what an ember can actually catch, which makes it the most valuable
    /// thing in a pocket.
    /// </summary>
    public static FuelSpecies Tinder { get; } = new()
    {
        Id = "tinder",
        CalorificValueJPerKg = 16.0e6,
        DensityKgPerM3 = 90.0,
        ResinFactor = 0.3,
        IgnitionTemperatureC = 180.0,
    };

    /// <summary>
    /// Coal. Attested — the Jianshangou unit carries coal seams (docs/03 §2). Hard
    /// to light, and then it burns for a very long time at high output, which makes
    /// it the answer to a winter night if you can find it and get it going.
    /// </summary>
    public static FuelSpecies Coal { get; } = new()
    {
        Id = "coal",
        CalorificValueJPerKg = 28.0e6,
        DensityKgPerM3 = 1350.0,
        ResinFactor = 0.0,
        IgnitionTemperatureC = 550.0,
    };

    /// <summary>Every fuel Stage 1 knows about.</summary>
    public static IReadOnlyList<FuelSpecies> All { get; } =
        [ConiferDeadwood, ConiferBark, HardWood, Tinder, Coal];
}
