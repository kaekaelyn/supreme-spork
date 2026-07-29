namespace ElderWorld.Core.Thermal;

/// <summary>
/// The stages of cold, in the order docs/02 §1 lists them.
/// <para>
/// <b>Every one of these is shown behaviourally and none of them is ever named.</b>
/// There is no status effect, no icon, no text. The body reports itself: you shiver,
/// your breath fogs, you drop things, the world stops making sense, and then you
/// start taking your clothes off. The player works out what is happening by having
/// it happen (docs/00 §3).
/// </para>
/// </summary>
public enum HypothermiaStage
{
    /// <summary>Core at or above 36 °C. Fine, or at least not yet not-fine.</summary>
    Normothermic = 0,

    /// <summary>Below 36 °C. Shivering, fogged breath, hunched posture.</summary>
    Shivering = 1,

    /// <summary>
    /// Below 35 °C, or hands cold enough on their own. Fine motor control degrades:
    /// crafting fails, items get dropped. The first stage with real mechanical teeth.
    /// </summary>
    ClumsyHands = 2,

    /// <summary>
    /// Below 33.5 °C. Judgement goes. The interface itself becomes unreliable —
    /// direction sense drifts, the world reads wrong.
    /// </summary>
    Confusion = 3,

    /// <summary>
    /// Below 31 °C. The character begins removing their own clothing.
    /// <para>
    /// A real terminal symptom — cold-paralysed blood vessels dilate and the victim
    /// feels a rush of heat — and docs/02 §1 calls it "the most horrifying possible
    /// mechanic". The character does it. The player cannot stop them.
    /// </para>
    /// </summary>
    ParadoxicalUndressing = 4,

    /// <summary>Below 30 °C. Unconscious. Whatever happens next happens without you.</summary>
    Unconscious = 5,

    /// <summary>Below 24 °C. Under permadeath, that character is gone.</summary>
    Dead = 6,
}

/// <summary>Core-temperature thresholds for <see cref="HypothermiaStage"/>, in °C.</summary>
public static class HypothermiaThresholds
{
    /// <summary>Shivering begins.</summary>
    public const double ShiveringC = 36.0;

    /// <summary>Fine motor control degrades. Clinical mild hypothermia.</summary>
    public const double ClumsyHandsC = 35.0;

    /// <summary>Judgement and orientation degrade.</summary>
    public const double ConfusionC = 33.5;

    /// <summary>Paradoxical undressing becomes possible.</summary>
    public const double ParadoxicalUndressingC = 31.0;

    /// <summary>Loss of consciousness.</summary>
    public const double UnconsciousC = 30.0;

    /// <summary>Death.</summary>
    public const double DeadC = 24.0;

    /// <summary>
    /// Hand skin temperature below which dexterity is impaired regardless of core
    /// temperature. You do not need to be hypothermic to lose your hands to the cold
    /// — you only need to have been holding a wet stone in the wind.
    /// </summary>
    public const double ImpairedHandSkinC = 15.0;

    /// <summary>Hand skin temperature at which fine manipulation is essentially gone.</summary>
    public const double UselessHandSkinC = 6.0;

    /// <summary>Classifies a core temperature.</summary>
    public static HypothermiaStage Classify(double coreTemperatureC) => coreTemperatureC switch
    {
        < DeadC => HypothermiaStage.Dead,
        < UnconsciousC => HypothermiaStage.Unconscious,
        < ParadoxicalUndressingC => HypothermiaStage.ParadoxicalUndressing,
        < ConfusionC => HypothermiaStage.Confusion,
        < ClumsyHandsC => HypothermiaStage.ClumsyHands,
        < ShiveringC => HypothermiaStage.Shivering,
        _ => HypothermiaStage.Normothermic,
    };
}
