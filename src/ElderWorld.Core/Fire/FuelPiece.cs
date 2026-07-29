namespace ElderWorld.Core.Fire;

/// <summary>
/// One physical piece of fuel on the fire. Pieces are tracked individually because
/// docs/03 §1 insists crafting is physical rather than menu-driven: a fire is a pile
/// of specific objects, and a wrist-thick wet log behaves nothing like a handful of
/// dry twigs even at identical mass.
/// </summary>
public sealed class FuelPiece
{
    /// <summary>Creates a piece of fuel.</summary>
    /// <param name="species">What it is.</param>
    /// <param name="dryMassKg">Mass of the dry material, kg, excluding its water.</param>
    /// <param name="moistureFraction">
    /// Water as a fraction of dry mass. Seasoned deadwood is around 0.15;
    /// freshly-fallen wood 0.35; green or rained-on wood 0.6 and upward.
    /// </param>
    /// <param name="thicknessMetres">Characteristic thickness. Twigs 0.005, wrist-thick 0.04, logs 0.15.</param>
    public FuelPiece(FuelSpecies species, double dryMassKg, double moistureFraction, double thicknessMetres)
    {
        ArgumentNullException.ThrowIfNull(species);
        if (dryMassKg <= 0.0) throw new ArgumentOutOfRangeException(nameof(dryMassKg), "Fuel must have mass.");
        if (thicknessMetres <= 0.0) throw new ArgumentOutOfRangeException(nameof(thicknessMetres), "Fuel must have thickness.");

        Species = species;
        DryMassKg = dryMassKg;
        MoistureFraction = Math.Clamp(moistureFraction, 0.0, 2.0);
        ThicknessMetres = thicknessMetres;
    }

    /// <summary>What this piece is made of.</summary>
    public FuelSpecies Species { get; }

    /// <summary>Remaining dry mass, kg.</summary>
    public double DryMassKg { get; private set; }

    /// <summary>Water content as a fraction of dry mass.</summary>
    public double MoistureFraction { get; private set; }

    /// <summary>Characteristic thickness, m.</summary>
    public double ThicknessMetres { get; }

    /// <summary>True once the piece has been consumed.</summary>
    public bool IsSpent => DryMassKg <= 1e-6;

    /// <summary>
    /// Surface area available to burn, m².
    /// <para>
    /// Treating a piece as a cylinder gives area ≈ 4·volume/thickness, so a given
    /// mass split into twigs presents an order of magnitude more surface than the
    /// same mass as a log. That one relation is why you cannot start a fire with
    /// logs and cannot keep one alive overnight with twigs, without either fact
    /// being stated anywhere.
    /// </para>
    /// </summary>
    public double ExposedAreaM2 => 4.0 * (DryMassKg / Species.DensityKgPerM3) / ThicknessMetres;

    /// <summary>
    /// Bed temperature this piece needs before it will catch, °C. Water in the wood
    /// raises it steeply, which is the whole difficulty of a fire in the rain.
    /// </summary>
    public double EffectiveIgnitionTemperatureC =>
        Species.IgnitionTemperatureC * (1.0 + 1.6 * MoistureFraction)
        * (1.0 - 0.25 * Species.ResinFactor);

    /// <summary>Burns dry mass, taking its water with it. Returns the mass actually consumed.</summary>
    internal double Consume(double dryMassKg)
    {
        double consumed = Math.Min(DryMassKg, Math.Max(0.0, dryMassKg));
        DryMassKg -= consumed;
        return consumed;
    }

    /// <summary>Drives water out of the piece as it sits by the fire, or soaks it in the rain.</summary>
    internal void AdjustMoisture(double delta)
        => MoistureFraction = Math.Clamp(MoistureFraction + delta, 0.0, 2.0);
}
