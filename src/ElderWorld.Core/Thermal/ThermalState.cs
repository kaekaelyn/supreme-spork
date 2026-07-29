namespace ElderWorld.Core.Thermal;

/// <summary>
/// The body's thermal condition. Mutable, serialisable, and the thing that persists
/// while a player is logged off (docs/02 §6: <i>"Your character sleeps, or doesn't,
/// in whatever state you left them."</i>).
/// </summary>
public sealed class ThermalState
{
    /// <summary>
    /// Normal core temperature, °C.
    /// <para>
    /// This is the regulatory set point, not the 37.0 °C of common knowledge, and it
    /// must stay equal to the set point the model regulates against. Starting a body
    /// even a fraction above its own set point makes it vasodilate and sweat on the
    /// first step, which in a cold climate soaks its clothing before anything has
    /// happened to it.
    /// </para>
    /// </summary>
    public const double NormalCoreC = 36.8;

    /// <summary>Neutral mean skin temperature, °C.</summary>
    public const double NeutralSkinC = 33.7;

    /// <summary>Deep-body temperature, °C. What kills you.</summary>
    public double CoreTemperatureC { get; set; } = NormalCoreC;

    /// <summary>Area-weighted mean skin temperature, °C. What you feel.</summary>
    public double MeanSkinTemperatureC { get; set; } = NeutralSkinC;

    /// <summary>Per-zone skin temperature, °C, indexed by <see cref="BodyZone"/>.</summary>
    public double[] ZoneSkinTemperatureC { get; } = [NeutralSkinC, NeutralSkinC, NeutralSkinC, NeutralSkinC];

    /// <summary>
    /// Permanent frostbite damage per zone, 0–1.
    /// <para>
    /// docs/02 §1: <i>"Frostbite on extremities is permanent. Lose fingers, lose
    /// crafting speed and grip. Lose toes, lose sprint. This should be rare and
    /// avoidable, but it should be permanent, because permanent consequences are
    /// what make cold frightening instead of annoying."</i> Nothing in the game
    /// reduces this number.
    /// </para>
    /// </summary>
    public double[] FrostbiteDamage { get; } = [0.0, 0.0, 0.0, 0.0];

    /// <summary>Skin blood flow, L/(h·m²). Basal is 6.3; cold drives it toward 0.5.</summary>
    public double SkinBloodFlow { get; set; } = 6.3;

    /// <summary>
    /// Fraction of skin covered in sweat, 0–1. Baseline 0.06 is insensible
    /// perspiration, which never stops.
    /// </summary>
    public double SkinWettedness { get; set; } = 0.06;

    /// <summary>
    /// How much sweat has soaked into the clothing, 0–1.
    /// <para>
    /// This is the mechanism behind docs/02 §1's warning: <i>"Sprinting to stay warm
    /// and then stopping is how you die."</i> Work hard in a parka and you fill your
    /// own insulation with water; stop, and it is a wet garment in the wind. The
    /// trap is real, non-obvious, and entirely emergent from this one number.
    /// </para>
    /// </summary>
    public double ClothingSweatSaturation { get; set; }

    /// <summary>
    /// Readily mobilisable energy, kcal — glycogen and circulating substrate, not
    /// body fat.
    /// <para>
    /// Shivering runs on this, which is what makes docs/02 §1 true: <i>"You cannot
    /// stay warm while starving."</i> A full-scale nutrition model lands in Stage 2
    /// (docs/09 §8); this is the seam it plugs into.
    /// </para>
    /// </summary>
    public double EnergyReserveKcal { get; set; } = 1800.0;

    /// <summary>Metabolic heat produced last step, W. Read-only output.</summary>
    public double MetabolicHeatWatts { get; private set; }

    /// <summary>Shivering heat produced last step, W. Read-only output.</summary>
    public double ShiveringHeatWatts { get; private set; }

    /// <summary>Net heat balance last step, W. Negative means the body is losing.</summary>
    public double NetHeatBalanceWatts { get; private set; }

    /// <summary>Total world time this body has been simulated, seconds.</summary>
    public double ElapsedSeconds { get; private set; }

    /// <summary>The stage of hypothermia the core temperature currently sits in.</summary>
    public HypothermiaStage Stage => HypothermiaThresholds.Classify(CoreTemperatureC);

    /// <summary>True once the core has fallen past <see cref="HypothermiaThresholds.DeadC"/>.</summary>
    public bool IsDead => CoreTemperatureC < HypothermiaThresholds.DeadC;

    /// <summary>Skin temperature of a zone, °C.</summary>
    public double SkinTemperature(BodyZone zone) => ZoneSkinTemperatureC[(int)zone];

    /// <summary>Permanent frostbite damage in a zone, 0–1.</summary>
    public double Frostbite(BodyZone zone) => FrostbiteDamage[(int)zone];

    /// <summary>
    /// Manual dexterity, 0–1, where 1 is unimpaired.
    /// <para>
    /// Driven by hand skin temperature and by permanent frostbite, not by core
    /// temperature — you can lose your hands to the cold while your core is still
    /// fine, which is exactly how it works in the field. Crafting reads this and
    /// fails against it; nothing tells the player why their hands stopped obeying.
    /// </para>
    /// </summary>
    public double Dexterity
    {
        get
        {
            double handSkin = SkinTemperature(BodyZone.Hands);

            double cold = handSkin >= HypothermiaThresholds.ImpairedHandSkinC
                ? 1.0
                : Math.Clamp(
                    (handSkin - HypothermiaThresholds.UselessHandSkinC) /
                    (HypothermiaThresholds.ImpairedHandSkinC - HypothermiaThresholds.UselessHandSkinC),
                    0.0, 1.0);

            // Fingers you no longer have do not warm back up.
            double permanent = 1.0 - Frostbite(BodyZone.Hands);

            // Deep hypothermia takes the rest.
            double systemic = CoreTemperatureC >= HypothermiaThresholds.ClumsyHandsC
                ? 1.0
                : Math.Clamp((CoreTemperatureC - HypothermiaThresholds.ConfusionC) /
                             (HypothermiaThresholds.ClumsyHandsC - HypothermiaThresholds.ConfusionC), 0.0, 1.0);

            return Math.Clamp(cold * permanent * systemic, 0.0, 1.0);
        }
    }

    /// <summary>
    /// Shivering intensity, 0–1. The greybox drives camera shake, breath rate and
    /// involuntary vocalisation off this.
    /// </summary>
    public double ShiveringIntensity => Math.Clamp(ShiveringHeatWatts / 350.0, 0.0, 1.0);

    /// <summary>Called by <see cref="ThermalModel"/> after each step.</summary>
    internal void RecordStep(double metabolicWatts, double shiveringWatts, double netWatts, double stepSeconds)
    {
        MetabolicHeatWatts = metabolicWatts;
        ShiveringHeatWatts = shiveringWatts;
        NetHeatBalanceWatts = netWatts;
        ElapsedSeconds += stepSeconds;
    }

    /// <summary>A body that has just woken up: warm, dry, fed, and about to be none of those.</summary>
    public static ThermalState Fresh() => new();
}
