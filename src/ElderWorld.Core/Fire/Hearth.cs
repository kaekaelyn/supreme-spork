using ElderWorld.Core.Climate;

namespace ElderWorld.Core.Fire;

/// <summary>What a fire is currently doing. Derived from the bed, never set directly.</summary>
public enum FireState
{
    /// <summary>Cold. Nothing here but ash.</summary>
    Dead = 0,

    /// <summary>A glowing coal with no flame. Alive, barely, and revivable with tinder and breath.</summary>
    Ember = 1,

    /// <summary>Caught and flaming, but fragile — wind or rain will still take it.</summary>
    Kindling = 2,

    /// <summary>Burning properly.</summary>
    Flame = 3,

    /// <summary>Hot bed, deep coals, robust against weather. What you want before dark.</summary>
    Established = 4,

    /// <summary>Fuel exhausted but the bed still hot. Feed it and it comes back.</summary>
    Embers = 5,
}

/// <summary>
/// A fire. docs/02 §4 makes it the hub of the entire tech graph — <i>"Fire is also
/// light and predator deterrence and meat preservation and ceramics and warmth. It's
/// the hub of the whole tech graph and should feel like it."</i>
/// <para>
/// The bed is modelled as a thermal mass. Combustion heats it, weather cools it, and
/// everything else — whether new fuel catches, how much light and heat come off it,
/// whether it survives the night — is read off its temperature. That is what lets a
/// stone-lined hearth genuinely hold heat for hours after the fuel is gone, and what
/// makes a fire something you tend rather than a switch you flip.
/// </para>
/// </summary>
public sealed class Hearth
{
    /// <summary>The fixed integration step, in world seconds.</summary>
    public const double StepSeconds = 1.0;

    private const double DeadBelowC = 60.0;
    private const double EmberFloorC = 150.0;
    private const double KindlingAboveC = 350.0;
    private const double FlameAboveC = 500.0;
    private const double EstablishedAboveC = 700.0;

    // Where the chemical energy goes. A flaming fire radiates roughly a third of its
    // output and loses most of the rest up the plume, so only a little stays in the
    // bed. Smouldering has no plume worth the name, so nearly all of its much smaller
    // output stays in the coals — which is the entire physics of banking a fire for
    // the night, and the reason a banked fire survives while a flaming one burns out.
    private const double RadiantFraction = 0.30;
    private const double FlamingBedRetainedFraction = 0.20;
    private const double SmoulderingBedRetainedFraction = 0.70;

    /// <summary>Below this bed temperature nothing burns at all, °C.</summary>
    private const double SmoulderFloorC = 250.0;

    /// <summary>Peak smouldering rate as a fraction of the flaming rate.</summary>
    private const double SmoulderIntensityCeiling = 0.22;

    private const double BurnFluxKgPerM2Second = 0.0045;
    private const double WaterEvaporationJPerKg = 2.6e6;
    private const double StefanBoltzmann = 5.670374419e-8;

    /// <summary>
    /// Ceiling on heat release per m² of bed, W, before the airflow factor.
    /// <para>
    /// Real fires are ventilation-limited, not fuel-limited. Without this, a double
    /// armful of dry twigs presents so much surface that the model produces an 80 kW
    /// bonfire in a hearth the size of a dinner plate. Capping release by bed area is
    /// what makes a small fire small however much kindling is piled on it.
    /// </para>
    /// </summary>
    private const double MaxHeatReleasePerBedAreaW = 100_000.0;

    private readonly List<FuelPiece> _fuel = [];

    /// <summary>Creates a fire on a given hearth construction.</summary>
    /// <param name="thermalMassJPerK">
    /// Heat capacity of the bed and any stones around it, J/K. A bare scrape in the
    /// dirt is about 4,000; a stone-lined hearth 40,000 or more, and holds embers
    /// through a night because of it.
    /// </param>
    /// <param name="airflow">
    /// How well the lay breathes, 0–1. A smothered heap is 0.2; a properly built
    /// lay with a reflector is 1.0.
    /// </param>
    /// <param name="rainShelter">How much of the rain is kept off, 0–1.</param>
    public Hearth(double thermalMassJPerK = 6000.0, double airflow = 0.7, double rainShelter = 0.0)
    {
        if (thermalMassJPerK <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(thermalMassJPerK), "A hearth must have thermal mass.");

        ThermalMassJPerK = thermalMassJPerK;
        Airflow = Math.Clamp(airflow, 0.0, 1.0);
        RainShelter = Math.Clamp(rainShelter, 0.0, 1.0);
    }

    /// <summary>Heat capacity of the bed and hearth stones, J/K.</summary>
    public double ThermalMassJPerK { get; }

    /// <summary>How well the fire lay breathes, 0–1.</summary>
    public double Airflow { get; set; }

    /// <summary>How much rain is kept off the fire, 0–1.</summary>
    public double RainShelter { get; set; }

    /// <summary>Temperature of the fuel bed, °C. The state variable everything else reads.</summary>
    public double BedTemperatureC { get; private set; } = 15.0;

    /// <summary>Radiant power leaving the fire, W. This is what warms a person sitting by it.</summary>
    public double RadiantPowerWatts { get; private set; }

    /// <summary>Total chemical power being released, W.</summary>
    public double OutputWatts { get; private set; }

    /// <summary>
    /// Smoke production, arbitrary units.
    /// <para>
    /// Tracked from Stage 1 because docs/02 §4 flags an unvented interior fire as
    /// "another silent killer with no monster attached", and because docs/05 §6 makes
    /// smoke the thing that broadcasts your settlement for miles. Wet and resinous
    /// fuel smoke hardest.
    /// </para>
    /// </summary>
    public double SmokeRate { get; private set; }

    /// <summary>Fuel currently on the fire.</summary>
    public IReadOnlyList<FuelPiece> Fuel => _fuel;

    /// <summary>Total dry mass of fuel remaining, kg.</summary>
    public double RemainingDryMassKg
    {
        get
        {
            double total = 0.0;
            foreach (FuelPiece piece in _fuel) total += piece.DryMassKg;
            return total;
        }
    }

    /// <summary>Area of the burning bed, m². Grows with the fuel loaded onto it.</summary>
    public double BedAreaM2 => Math.Clamp(0.05 + 0.04 * RemainingDryMassKg, 0.05, 1.2);

    /// <summary>What the fire is currently doing.</summary>
    public FireState State
    {
        get
        {
            if (BedTemperatureC < DeadBelowC) return FireState.Dead;

            bool hasFuel = RemainingDryMassKg > 1e-4;
            if (!hasFuel)
                return BedTemperatureC >= EmberFloorC ? FireState.Embers : FireState.Dead;

            if (BedTemperatureC >= EstablishedAboveC) return FireState.Established;
            if (BedTemperatureC >= FlameAboveC) return FireState.Flame;
            if (BedTemperatureC >= KindlingAboveC) return FireState.Kindling;
            return FireState.Ember;
        }
    }

    /// <summary>True while the fire is producing useful heat.</summary>
    public bool IsBurning => State is FireState.Kindling or FireState.Flame or FireState.Established;

    /// <summary>True while there is anything left to revive.</summary>
    public bool IsAlive => State != FireState.Dead;

    /// <summary>Adds a piece of fuel.</summary>
    public void AddFuel(FuelPiece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);
        _fuel.Add(piece);
    }

    /// <summary>
    /// Lays an ember into the bed, which is how a naked player on day one gets fire
    /// at all.
    /// <para>
    /// docs/02 §4: <i>"Volcanism gives us a beautiful onboarding affordance: before
    /// you can make fire, you can fetch it."</i> The fumaroles and hot ground are
    /// there from the first hour; fire-by-friction is a later technology that fails
    /// in the rain, which is what keeps ember-carrying worth knowing forever.
    /// </para>
    /// </summary>
    public void LayEmber(Ember ember)
    {
        ArgumentNullException.ThrowIfNull(ember);
        if (!ember.IsAlive) return;

        // The ember dumps its heat into the bed. On a cold stone hearth that may not
        // be enough on its own, which is why you bring tinder as well.
        BedTemperatureC += ember.HeatContentJoules / ThermalMassJPerK;
        ember.Extinguish();
    }

    /// <summary>Directly sets the bed temperature. For world loading and for hot volcanic ground.</summary>
    public void SetBedTemperature(double temperatureC) => BedTemperatureC = temperatureC;

    /// <summary>Advances the fire by one fixed step.</summary>
    public void Step(EnvironmentSample environment) => StepFor(environment, StepSeconds);

    /// <summary>Advances the fire by a slab of world time, internally in fixed steps.</summary>
    public void Advance(EnvironmentSample environment, double worldSeconds)
    {
        if (worldSeconds < 0.0)
            throw new ArgumentOutOfRangeException(nameof(worldSeconds), "Time does not run backwards.");

        int steps = (int)(worldSeconds / StepSeconds);
        for (int i = 0; i < steps; i++) StepFor(environment, StepSeconds);

        double remainder = worldSeconds - steps * StepSeconds;
        if (remainder > 1e-9) StepFor(environment, remainder);
    }

    private void StepFor(EnvironmentSample environment, double dt)
    {
        double windAtFire = environment.WindSpeedMetresPerSecond * environment.Site.WindExposure;

        // Wind feeds a fire oxygen, up to the point where it starts stripping heat
        // off the bed faster than combustion replaces it. Both effects are here, and
        // which one wins depends on how hot the bed already is — so a gale kills a
        // young fire and merely roars an established one.
        double airflowFactor = Math.Clamp(0.08 + 0.90 * Airflow + 0.35 * windAtFire, 0.06, 2.5);

        (double releasedWatts, bool flaming) = BurnFuel(airflowFactor, dt);

        OutputWatts = releasedWatts;
        SmokeRate = ComputeSmokeRate(releasedWatts);

        double retainedFraction = flaming ? FlamingBedRetainedFraction : SmoulderingBedRetainedFraction;
        double bedGainWatts = releasedWatts * retainedFraction;
        double bedLossWatts = BedLossWatts(environment, windAtFire);

        BedTemperatureC += (bedGainWatts - bedLossWatts) * dt / ThermalMassJPerK;
        BedTemperatureC = Math.Max(environment.AirTemperatureC, BedTemperatureC);

        RadiantPowerWatts = ComputeRadiantPower(releasedWatts, environment);

        DryNearbyFuel(environment, dt);
        _fuel.RemoveAll(piece => piece.IsSpent);
    }

    /// <summary>
    /// Burns whatever is hot enough to burn, returning the chemical power released
    /// and whether the fire is flaming rather than merely smouldering.
    /// </summary>
    private (double ReleasedWatts, bool Flaming) BurnFuel(double airflowFactor, double dt)
    {
        if (_fuel.Count == 0) return (0.0, false);

        // Demand first, ventilation limit second: work out what the fuel would burn
        // if it had all the air it wanted, then scale the whole bed back to what the
        // available air can actually support.
        double demandKgPerSecond = 0.0;
        bool anyFlaming = false;

        Span<double> pieceRates = _fuel.Count <= 64
            ? stackalloc double[_fuel.Count]
            : new double[_fuel.Count];

        for (int i = 0; i < _fuel.Count; i++)
        {
            FuelPiece piece = _fuel[i];
            pieceRates[i] = 0.0;
            if (piece.IsSpent) continue;

            double intensity = CombustionIntensity(BedTemperatureC, piece.EffectiveIgnitionTemperatureC);
            if (intensity <= 0.0) continue;

            if (BedTemperatureC >= piece.EffectiveIgnitionTemperatureC) anyFlaming = true;

            // Water in the wood suppresses combustion far more than it costs in energy.
            double moistureFactor = Math.Pow(1.0 / (1.0 + piece.MoistureFraction), 2.0);

            double rate = piece.ExposedAreaM2 * BurnFluxKgPerM2Second
                          * airflowFactor * intensity * moistureFactor;

            pieceRates[i] = rate;
            demandKgPerSecond += rate;
        }

        if (demandKgPerSecond <= 0.0) return (0.0, false);

        double releasedJoules = 0.0;
        double ventilationCapWatts = MaxHeatReleasePerBedAreaW * BedAreaM2 * airflowFactor;

        // Convert the cap into a mass throttle using the bed's mean energy density.
        double meanCalorific = MeanCalorificValueJPerKg();
        double throttle = 1.0;
        if (meanCalorific > 0.0)
        {
            double cappedKgPerSecond = ventilationCapWatts / meanCalorific;
            if (demandKgPerSecond > cappedKgPerSecond)
                throttle = cappedKgPerSecond / demandKgPerSecond;
        }

        for (int i = 0; i < _fuel.Count; i++)
        {
            if (pieceRates[i] <= 0.0) continue;

            FuelPiece piece = _fuel[i];
            double consumed = piece.Consume(pieceRates[i] * throttle * dt);
            if (consumed <= 0.0) continue;

            double gross = consumed * piece.Species.CalorificValueJPerKg;
            double evaporationCost = consumed * piece.MoistureFraction * WaterEvaporationJPerKg;
            releasedJoules += Math.Max(0.0, gross - evaporationCost);
        }

        return (releasedJoules / dt, anyFlaming);
    }

    /// <summary>
    /// How hard a piece burns at the current bed temperature, as a multiple of its
    /// nominal flaming rate.
    /// <para>
    /// Three regimes. Below <see cref="SmoulderFloorC"/> nothing happens. Between
    /// there and the piece's ignition point it smoulders — glowing combustion of
    /// char, slow and flameless, which is what a banked fire is doing all night.
    /// Above ignition it flames, and the rate climbs with bed temperature.
    /// </para>
    /// </summary>
    private static double CombustionIntensity(double bedTemperatureC, double ignitionTemperatureC)
    {
        if (bedTemperatureC < SmoulderFloorC) return 0.0;

        if (bedTemperatureC < ignitionTemperatureC)
        {
            double through = (bedTemperatureC - SmoulderFloorC)
                             / Math.Max(1.0, ignitionTemperatureC - SmoulderFloorC);
            return SmoulderIntensityCeiling * Math.Clamp(through, 0.0, 1.0);
        }

        double above = (bedTemperatureC - ignitionTemperatureC) / 250.0;
        return SmoulderIntensityCeiling + Math.Clamp(above, 0.0, 1.4);
    }

    private double MeanCalorificValueJPerKg()
    {
        double weighted = 0.0;
        double mass = 0.0;
        foreach (FuelPiece piece in _fuel)
        {
            if (piece.IsSpent) continue;
            weighted += piece.Species.CalorificValueJPerKg * piece.DryMassKg;
            mass += piece.DryMassKg;
        }
        return mass > 0.0 ? weighted / mass : 0.0;
    }

    /// <summary>
    /// How fast the bed sheds the heat it is holding, W.
    /// <para>
    /// Both loss terms scale with <see cref="Airflow"/>, and that coupling is the
    /// point. Banking a fire means burying it — heaping ash and coals over it and
    /// closing it down — and the same blanket that starves it of oxygen also stops
    /// it radiating and convecting. An open lay is bright and hot and gone by
    /// midnight; a buried one is barely alive and still there at dawn. Neither
    /// behaviour is scripted; both fall out of one number the player sets by how
    /// they build the fire.
    /// </para>
    /// </summary>
    private double BedLossWatts(EnvironmentSample environment, double windAtFire)
    {
        double bedKelvin = BedTemperatureC + 273.15;
        double airKelvin = environment.AirTemperatureC + 273.15;
        double area = BedAreaM2;

        double emissivity = 0.02 + 0.13 * Airflow;
        double radiative = emissivity * StefanBoltzmann * area
                           * (Math.Pow(bedKelvin, 4.0) - Math.Pow(airKelvin, 4.0));

        double convectiveCoefficient = (0.8 + 11.0 * Airflow) * (1.0 + 0.6 * windAtFire);
        double convective = convectiveCoefficient * area * (BedTemperatureC - environment.AirTemperatureC);

        double rain = 0.0;
        if (environment.Precipitation != PrecipitationKind.None)
        {
            // Rain landing on the bed boils off and takes the latent heat with it.
            // One mm of rain over one m² is one litre, so one kg — no conversion
            // factor beyond the hour-to-second one.
            double exposedFraction = 1.0 - RainShelter;
            double waterKgPerSecond = environment.PrecipitationMmPerHour * area * exposedFraction / 3600.0;
            rain = waterKgPerSecond * WaterEvaporationJPerKg;
        }

        return Math.Max(0.0, radiative + convective + rain);
    }

    private double ComputeRadiantPower(double releasedWatts, EnvironmentSample environment)
    {
        double flameRadiant = releasedWatts * RadiantFraction;

        // Even with no flame, a hot bed of coals radiates — which is what a banked
        // fire is for, and why embers are worth keeping alive through a night.
        double bedKelvin = BedTemperatureC + 273.15;
        double airKelvin = environment.AirTemperatureC + 273.15;
        double emissivity = 0.02 + 0.13 * Airflow;
        double emberRadiant = emissivity * StefanBoltzmann * BedAreaM2
                              * (Math.Pow(bedKelvin, 4.0) - Math.Pow(airKelvin, 4.0));

        return Math.Max(0.0, Math.Max(flameRadiant, emberRadiant));
    }

    private double ComputeSmokeRate(double releasedWatts)
    {
        if (releasedWatts <= 0.0 && BedTemperatureC < EmberFloorC) return 0.0;

        double wetness = 0.0;
        double totalMass = 0.0;
        foreach (FuelPiece piece in _fuel)
        {
            wetness += piece.MoistureFraction * piece.DryMassKg;
            totalMass += piece.DryMassKg;
        }
        double meanMoisture = totalMass > 0.0 ? wetness / totalMass : 0.0;

        // Smouldering smokes far worse than clean flame.
        double smoulderFactor = BedTemperatureC < FlameAboveC ? 2.5 : 1.0;
        return (releasedWatts / 1000.0) * (1.0 + 2.0 * meanMoisture) * smoulderFactor;
    }

    /// <summary>
    /// Fuel stacked by a burning fire dries out, and fuel left in the rain soaks.
    /// Drying tomorrow's wood by tonight's fire is a real and entirely undocumented
    /// technique the player either works out or does not.
    /// </summary>
    private void DryNearbyFuel(EnvironmentSample environment, double dt)
    {
        bool raining = environment.Precipitation == PrecipitationKind.Rain && RainShelter < 0.5;

        foreach (FuelPiece piece in _fuel)
        {
            if (piece.IsSpent) continue;

            if (raining)
            {
                piece.AdjustMoisture(environment.PrecipitationMmPerHour * 2.0e-5 * dt);
            }
            else if (BedTemperatureC > 200.0)
            {
                double dryingRate = (BedTemperatureC - 200.0) / 200.0 * 1.2e-5;
                piece.AdjustMoisture(-dryingRate * dt);
            }
        }
    }

    /// <summary>
    /// Radiant flux arriving at a given distance, W/m².
    /// <para>
    /// A ground fire radiates into roughly a hemisphere, so flux falls as 1/(2πd²).
    /// Feed this into <c>ThermalExposure.FireIrradianceWattsPerM2</c>.
    /// </para>
    /// <para>
    /// Only the side facing the fire receives it, which the thermal model accounts
    /// for and which is why the correct thing to do on a cold night is to keep
    /// turning around. Nobody will tell the player that either.
    /// </para>
    /// </summary>
    public double IrradianceAt(double distanceMetres)
    {
        // Clamped so that standing implausibly close does not divide by zero. At
        // this range you would be burning anyway.
        double distance = Math.Max(0.35, distanceMetres);
        return RadiantPowerWatts / (2.0 * Math.PI * distance * distance);
    }

    /// <summary>
    /// Illuminance at a distance, lux, for the greybox lighting.
    /// <para>
    /// A campfire is worth roughly 1–2 lux at a couple of metres. That is very
    /// little light, and it is the whole reason an eleven-hour night is a design
    /// brief rather than a gap between days.
    /// </para>
    /// </summary>
    public double IlluminanceAt(double distanceMetres)
    {
        double distance = Math.Max(0.35, distanceMetres);
        // Luminous efficacy of a wood flame is poor: about 0.6 lm/W of radiant power.
        double luminousFlux = RadiantPowerWatts * 0.6;
        return luminousFlux / (2.0 * Math.PI * distance * distance);
    }
}
