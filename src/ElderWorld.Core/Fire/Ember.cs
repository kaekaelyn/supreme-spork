using ElderWorld.Core.Climate;

namespace ElderWorld.Core.Fire;

/// <summary>
/// A carried ember — the first technology in the game and one of the last ones you
/// stop using.
/// <para>
/// docs/02 §4: <i>"Volcanism gives us a beautiful onboarding affordance: before you
/// can make fire, you can fetch it. Fumaroles, hot ground, and geothermal vents let
/// a naked player carry an ember home on day one. Fire-by-friction comes later,
/// requires the right woods and dry tinder, and fails in rain — which makes the
/// ember-carrying skill remain relevant forever."</i>
/// </para>
/// <para>
/// The whole mechanic lives in the tension between the two controls. An ember needs
/// air to stay alight and insulation to stay hot, and the things that give it air
/// take away its insulation. Smother it and it cools; open it up and it burns out.
/// Carrying one for an hour is a genuine skill, and nothing in the game will explain
/// any of it.
/// </para>
/// <para>
/// <b>Lumped greybox model.</b> One reserve, one temperature, two controls. Not a
/// char-oxidation model, and does not need to be: what has to be right is the
/// handling, and the handling is right.
/// </para>
/// </summary>
public sealed class Ember
{
    /// <summary>The fixed integration step, in world seconds.</summary>
    public const double StepSeconds = 1.0;

    /// <summary>Below this temperature an ember can no longer light tinder, and is finished.</summary>
    public const double DeathTemperatureC = 250.0;

    /// <summary>Temperature at which an ember will reliably take a dry tinder bundle.</summary>
    public const double TinderIgnitionTemperatureC = 350.0;

    private const double MaximumCombustionWatts = 650.0;
    private const double ExposedConductanceWattsPerKelvin = 0.22;

    /// <summary>
    /// Below this air supply the coal suffocates and goes out.
    /// <para>
    /// Without it, sealing an ember away completely would be the optimal strategy —
    /// no air means slow burning, and perfect insulation means no heat loss, so a
    /// sealed ember would last for ever. Real ones smother. This is the other half of
    /// the tension the mechanic runs on.
    /// </para>
    /// </summary>
    private const double SuffocationAirSupply = 0.008;

    /// <summary>Creates an ember.</summary>
    /// <param name="reserveJoules">Chemical energy left in the coal, J.</param>
    /// <param name="temperatureC">Starting temperature, °C.</param>
    public Ember(double reserveJoules = 200_000.0, double temperatureC = 700.0)
    {
        RemainingJoules = Math.Max(0.0, reserveJoules);
        TemperatureC = temperatureC;
    }

    /// <summary>Chemical energy remaining, J. When this runs out, so does the ember.</summary>
    public double RemainingJoules { get; private set; }

    /// <summary>Current temperature, °C.</summary>
    public double TemperatureC { get; private set; }

    /// <summary>
    /// How much air reaches the coal, 0–1. A sealed container is near 0; an open
    /// palm in a breeze is 1. Around 0.03 is a bark tube packed with punk and moss,
    /// which is the answer, and which the player has to arrive at themselves.
    /// </summary>
    public double AirSupply { get; set; } = 0.05;

    /// <summary>
    /// How well the coal is wrapped, 0–1. Insulation keeps it hot; it also, in
    /// almost every real container, restricts <see cref="AirSupply"/>.
    /// </summary>
    public double Insulation { get; set; } = 0.9;

    /// <summary>True while the ember is still hot enough to be worth carrying.</summary>
    public bool IsAlive => RemainingJoules > 0.0 && TemperatureC >= DeathTemperatureC;

    /// <summary>True when this ember will light a dry tinder bundle.</summary>
    public bool CanIgniteTinder => IsAlive && TemperatureC >= TinderIgnitionTemperatureC;

    /// <summary>
    /// Sensible heat available to dump into a hearth bed, J. Modest — an ember alone
    /// rarely starts a fire, which is why you carry tinder too.
    /// </summary>
    public double HeatContentJoules => IsAlive ? Math.Min(RemainingJoules, 45_000.0) : 0.0;

    /// <summary>
    /// An ember lifted from hot volcanic ground: a fumarole margin, a vent, a
    /// still-warm ash bed. Free, on day one, to a player with no tools and no idea.
    /// </summary>
    public static Ember FromVolcanicVent() => new(reserveJoules: 260_000.0, temperatureC: 620.0);

    /// <summary>An ember lifted from an established fire, to carry to a new camp.</summary>
    public static Ember FromHearth(Hearth hearth)
    {
        ArgumentNullException.ThrowIfNull(hearth);
        if (hearth.State is FireState.Dead)
            return new Ember(reserveJoules: 0.0, temperatureC: 15.0);

        return new Ember(reserveJoules: 240_000.0, temperatureC: Math.Min(hearth.BedTemperatureC, 800.0));
    }

    /// <summary>Advances the ember by one fixed step.</summary>
    public void Step(EnvironmentSample environment, bool sheltered = false)
        => StepFor(environment, sheltered, StepSeconds);

    /// <summary>Advances the ember by a slab of world time, internally in fixed steps.</summary>
    public void Advance(EnvironmentSample environment, double worldSeconds, bool sheltered = false)
    {
        if (worldSeconds < 0.0)
            throw new ArgumentOutOfRangeException(nameof(worldSeconds), "Time does not run backwards.");

        int steps = (int)(worldSeconds / StepSeconds);
        for (int i = 0; i < steps; i++) StepFor(environment, sheltered, StepSeconds);

        double remainder = worldSeconds - steps * StepSeconds;
        if (remainder > 1e-9) StepFor(environment, sheltered, remainder);
    }

    private void StepFor(EnvironmentSample environment, bool sheltered, double dt)
    {
        if (RemainingJoules <= 0.0)
        {
            TemperatureC = environment.AirTemperatureC;
            return;
        }

        double airSupply = Math.Clamp(AirSupply, 0.0, 1.0);
        double insulation = Math.Clamp(Insulation, 0.0, 1.0);

        // Combustion is limited by the air it is allowed, and stops entirely without it.
        double combustionWatts = airSupply < SuffocationAirSupply
            ? 0.0
            : MaximumCombustionWatts * airSupply;

        double burnedJoules = Math.Min(RemainingJoules, combustionWatts * dt);
        RemainingJoules -= burnedJoules;
        double actualWatts = burnedJoules / dt;

        // Heat escapes past the wrapping, and wind reaches whatever is exposed.
        double windAtEmber = sheltered
            ? 0.0
            : environment.WindSpeedMetresPerSecond * environment.Site.WindExposure;

        double conductance = ExposedConductanceWattsPerKelvin
                             * (1.0 - 0.93 * insulation)
                             * (1.0 + 0.35 * windAtEmber);

        // Rain is what actually kills a carried ember, and it is why fire-by-friction
        // never makes this skill obsolete. A tight wrapping buys real time, so
        // walking home through a shower is a risk rather than an automatic loss.
        if (!sheltered && environment.Precipitation == PrecipitationKind.Rain)
        {
            conductance += environment.PrecipitationMmPerHour * 0.06 * (1.0 - 0.95 * insulation);
        }

        conductance = Math.Max(1e-4, conductance);

        // The coal settles quickly to the temperature its own output can sustain, so
        // solving the steady state directly is both simpler and better behaved than
        // integrating a very small thermal mass.
        double steadyStateC = environment.AirTemperatureC + actualWatts / conductance;

        // A short lag, so an ember does not respond instantly to being uncovered.
        const double ResponseTimeSeconds = 45.0;
        double blend = 1.0 - Math.Exp(-dt / ResponseTimeSeconds);
        TemperatureC += (steadyStateC - TemperatureC) * blend;

        if (RemainingJoules <= 0.0)
            TemperatureC = Math.Min(TemperatureC, environment.AirTemperatureC);
    }

    /// <summary>Kills the ember. Used when it is laid into a hearth, or dropped in a stream.</summary>
    public void Extinguish()
    {
        RemainingJoules = 0.0;
        TemperatureC = 0.0;
    }
}
