namespace ElderWorld.Core.Thermal;

/// <summary>
/// The four coverage zones docs/02 §1 names: <i>"clothing R-value by layer and
/// coverage zone (head, core, hands, feet)"</i>.
/// <para>
/// They are tracked separately because they fail separately. The core is what
/// kills you; the extremities are what you lose permanently and never get back.
/// </para>
/// </summary>
public enum BodyZone
{
    /// <summary>Scalp, face, neck. Barely vasoconstricts, so it leaks heat even when everything else has shut down.</summary>
    Head = 0,

    /// <summary>Torso, arms, legs. The bulk of the body and of the heat loss.</summary>
    Core = 1,

    /// <summary>Hands. Where dexterity is lost first — dropped items and failed crafts.</summary>
    Hands = 2,

    /// <summary>Feet. Where mobility is lost, and the commonest site of permanent frostbite.</summary>
    Feet = 3,
}

/// <summary>Per-zone physiology constants.</summary>
public static class BodyZones
{
    /// <summary>Every zone, in enum order. Iterate this rather than hard-coding indices.</summary>
    public static readonly BodyZone[] All = [BodyZone.Head, BodyZone.Core, BodyZone.Hands, BodyZone.Feet];

    /// <summary>Number of zones.</summary>
    public const int Count = 4;

    /// <summary>
    /// Fraction of total body surface area. Summing to 1.0 by construction, which a
    /// test asserts.
    /// </summary>
    public static double SurfaceAreaFraction(BodyZone zone) => zone switch
    {
        BodyZone.Head => 0.07,
        BodyZone.Core => 0.81,
        BodyZone.Hands => 0.05,
        BodyZone.Feet => 0.07,
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    /// <summary>
    /// How hard this zone shuts down its blood supply when the body is cold, 0–1.
    /// <para>
    /// This asymmetry is the whole reason zones are modelled at all. Hands and feet
    /// are sacrificed almost completely to defend the core — which is why they are
    /// what freezes — while the head hardly constricts at all, which is why an
    /// uncovered head keeps bleeding heat long after the fingers have gone numb.
    /// A hat is the cheapest thermal technology in the game and nothing will say so.
    /// </para>
    /// </summary>
    public static double VasoconstrictionSensitivity(BodyZone zone) => zone switch
    {
        BodyZone.Head => 0.15,
        BodyZone.Core => 0.45,
        BodyZone.Hands => 0.95,
        BodyZone.Feet => 0.90,
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };

    /// <summary>
    /// Baseline thermal conductance from core to this zone's skin, W/(m²·K) of zone
    /// area, at resting perfusion.
    /// </summary>
    public static double BasePerfusionConductance(BodyZone zone) => zone switch
    {
        BodyZone.Head => 22.0,
        BodyZone.Core => 12.0,
        BodyZone.Hands => 18.0,
        BodyZone.Feet => 14.0,
        _ => throw new ArgumentOutOfRangeException(nameof(zone)),
    };
}
