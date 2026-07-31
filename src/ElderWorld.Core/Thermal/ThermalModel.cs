using ElderWorld.Core.Climate;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Thermal;

/// <summary>
/// The thermal model. docs/02 §1 calls it "the primary killer" and asks for a
/// budget rather than a bar: <i>"Core temperature is driven by metabolic heat
/// production minus losses to conduction, convection, radiation, and evaporation."</i>
/// That is literally what this does — every term is a real heat flow in watts, and
/// core temperature is what falls out.
/// <para>
/// Structurally it is a two-node model in the Gagge lineage: a core and a skin
/// shell, coupled by blood flow that the body throttles to defend the core at the
/// expense of the extremities. That single mechanism produces most of the design's
/// stated behaviours for free — why the hands go first, why frostbite is a
/// peripheral injury, and why shivering stops before death rather than at it.
/// </para>
/// <para>
/// <b>Fixed-step by construction.</b> <see cref="Step"/> takes exactly one
/// <see cref="StepSeconds"/> of world time. Drive it through a
/// <see cref="FixedStepAccumulator"/> so results are identical at 1× and at 1000×,
/// per docs/07 §2.
/// </para>
/// </summary>
public sealed class ThermalModel
{
    /// <summary>The fixed integration step, in world seconds.</summary>
    public const double StepSeconds = 1.0;

    // ── Body constants ──────────────────────────────────────────────────────────
    // A modern, unconditioned, soft-footed adult (docs/00 §1).

    /// <summary>Body mass, kg.</summary>
    public const double BodyMassKg = 70.0;

    /// <summary>DuBois body surface area, m².</summary>
    public const double SurfaceAreaM2 = 1.8;

    /// <summary>Specific heat of body tissue, J/(kg·K).</summary>
    public const double TissueSpecificHeat = 3470.0;

    /// <summary>
    /// Fraction of body mass in the skin shell.
    /// <para>
    /// Held fixed rather than varying with blood flow as the full Gagge model does.
    /// A moving compartment boundary silently teleports stored heat between nodes
    /// unless it is carefully corrected, which shows up as temperature steps when
    /// vasoconstriction kicks in. Fixing the masses and letting the blood-flow
    /// conductance carry the whole regulatory effect is stable, and the conductance
    /// was doing most of the work anyway.
    /// </para>
    /// </summary>
    public const double SkinMassFraction = 0.10;

    /// <summary>Heat capacity of the core node, J/K.</summary>
    public const double CoreHeatCapacity = BodyMassKg * (1.0 - SkinMassFraction) * TissueSpecificHeat;

    /// <summary>Heat capacity of the skin node, J/K.</summary>
    public const double SkinHeatCapacity = BodyMassKg * SkinMassFraction * TissueSpecificHeat;

    /// <summary>One met, in W/m². The metabolic rate of a resting person.</summary>
    public const double MetToWattsPerM2 = 58.2;

    /// <summary>
    /// Ceiling on shivering thermogenesis, W. Peak human shivering is roughly four
    /// to five times basal and cannot be held for long.
    /// </summary>
    public const double MaxShiveringWatts = 350.0;

    private const double StefanBoltzmann = 5.670374419e-8;
    private const double SkinEmissivity = 0.97;
    private const double RadiatingAreaFactor = 0.72;
    private const double BasalSkinBloodFlow = 6.3;
    private const double MinSkinBloodFlow = 0.5;
    private const double MaxSkinBloodFlow = 90.0;
    private const double CoreSetPointC = 36.8;
    private const double SkinSetPointC = 33.7;
    private const double TissueFreezingPointC = -0.55;

    /// <summary>
    /// Floor on core-to-skin conductance while immersed, W/(m²·K). Roughly triple
    /// the vasoconstricted value in air.
    /// </summary>
    private const double ImmersedConductanceWattsPerM2Kelvin = 20.0;

    /// <summary>
    /// Frostbite accrued per kelvin of undercooling per second. Calibrated so that
    /// about half an hour of tissue held 10 °C below freezing costs the zone
    /// outright — which matches field frostbite times and keeps the injury, as the
    /// design asks, rare and avoidable but permanent.
    /// </summary>
    private const double FrostbiteRatePerKelvinSecond = 1.0 / (1800.0 * 10.0);

    /// <summary>
    /// Advances the body by exactly one <see cref="StepSeconds"/>.
    /// </summary>
    public void Step(ThermalState state, ThermalExposure exposure)
    {
        ArgumentNullException.ThrowIfNull(state);
        StepFor(state, exposure, StepSeconds);
    }

    /// <summary>
    /// Advances the body by a slab of world time, internally in fixed steps.
    /// Convenience for headless runs; the engine layer should use an accumulator so
    /// the remainder is carried between frames.
    /// </summary>
    public void Advance(ThermalState state, ThermalExposure exposure, double worldSeconds)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (worldSeconds < 0.0)
            throw new ArgumentOutOfRangeException(nameof(worldSeconds), "Time does not run backwards.");

        int steps = (int)(worldSeconds / StepSeconds);
        for (int i = 0; i < steps; i++)
            StepFor(state, exposure, StepSeconds);

        double remainder = worldSeconds - steps * StepSeconds;
        if (remainder > 1e-9)
            StepFor(state, exposure, remainder);
    }

    private static void StepFor(ThermalState state, ThermalExposure exposure, double dt)
    {
        EnvironmentSample environment = exposure.Environment;
        bool immersed = exposure.Immersed;

        // In water it is the water's temperature that matters, and lake water lags
        // the air by months — which is why autumn water is far colder than an autumn
        // afternoon feels, and why falling in is worse than it looks.
        double airTemperatureC = immersed ? environment.WaterTemperatureC : environment.AirTemperatureC;
        double windSpeed = immersed ? 0.0 : environment.WindSpeedMetresPerSecond;

        // ── Regulatory signals ──────────────────────────────────────────────────
        double coldCoreSignal = Math.Max(0.0, CoreSetPointC - state.CoreTemperatureC);
        double warmCoreSignal = Math.Max(0.0, state.CoreTemperatureC - CoreSetPointC);
        double coldSkinSignal = Math.Max(0.0, SkinSetPointC - state.MeanSkinTemperatureC);
        double warmSkinSignal = Math.Max(0.0, state.MeanSkinTemperatureC - SkinSetPointC);

        // Vasomotor control: dilate on a warm core, constrict on cold skin.
        double skinBloodFlow = Math.Clamp(
            (BasalSkinBloodFlow + 200.0 * warmCoreSignal) / (1.0 + 0.5 * coldSkinSignal),
            MinSkinBloodFlow, MaxSkinBloodFlow);
        state.SkinBloodFlow = skinBloodFlow;

        double constriction = Math.Clamp(
            (BasalSkinBloodFlow - skinBloodFlow) / (BasalSkinBloodFlow - MinSkinBloodFlow), 0.0, 1.0);

        // ── Heat production ─────────────────────────────────────────────────────
        double activityMet = Math.Max(exposure.ActivityMet, Postures.BaseActivityMet(exposure.Posture));
        double activityWatts = activityMet * MetToWattsPerM2 * SurfaceAreaM2;

        double shiveringWatts = ShiveringWatts(state, coldSkinSignal, coldCoreSignal);
        double metabolicWatts = activityWatts + shiveringWatts;

        // Shivering runs on stored substrate. Burn it.
        double kcalBurned = shiveringWatts * dt / 4184.0;
        state.EnergyReserveKcal = Math.Max(0.0, state.EnergyReserveKcal - kcalBurned);

        // ── Environmental coupling ──────────────────────────────────────────────
        double convectiveCoefficient = ConvectiveCoefficient(windSpeed, immersed);
        double exposedFraction = Postures.ExposedFraction(exposure.Posture);
        double meanRadiantTemperatureC = MeanRadiantTemperatureC(environment, exposedFraction, immersed);

        // One fixed-point pass to settle the radiative coefficient, which depends on
        // the surface temperature it is helping to determine.
        double radiativeCoefficient = RadiativeCoefficient(state.MeanSkinTemperatureC, meanRadiantTemperatureC);
        double operativeTemperatureC = OperativeTemperatureC(
            airTemperatureC, meanRadiantTemperatureC, convectiveCoefficient, radiativeCoefficient);
        radiativeCoefficient = RadiativeCoefficient(
            (state.MeanSkinTemperatureC + operativeTemperatureC) / 2.0, meanRadiantTemperatureC);
        operativeTemperatureC = OperativeTemperatureC(
            airTemperatureC, meanRadiantTemperatureC, convectiveCoefficient, radiativeCoefficient);

        Insulation clothing = EffectiveClothing(exposure, state, immersed);

        // ── Dry loss from the skin, summed over zones ───────────────────────────
        double dryLossWatts = 0.0;
        Span<double> zoneResistance = stackalloc double[BodyZones.Count];

        foreach (BodyZone zone in BodyZones.All)
        {
            double areaFraction = BodyZones.SurfaceAreaFraction(zone);
            double area = SurfaceAreaM2 * areaFraction * exposedFraction;

            double resistance = ZoneResistance(
                clothing, zone, windSpeed, convectiveCoefficient, radiativeCoefficient);
            zoneResistance[(int)zone] = resistance;

            dryLossWatts += area * (state.MeanSkinTemperatureC - operativeTemperatureC) / resistance;
        }

        // ── Evaporation ─────────────────────────────────────────────────────────
        (double wettedness, double regulatorySweating) = Wettedness(state, clothing, warmSkinSignal);
        state.SkinWettedness = wettedness;

        double evaporativeLossWatts = EvaporativeLossWatts(
            state, environment, clothing, convectiveCoefficient, exposedFraction, wettedness, immersed);

        // ── Respiration ─────────────────────────────────────────────────────────
        double metabolicPerM2 = metabolicWatts / SurfaceAreaM2;
        double ambientVapourKpa = environment.RelativeHumidity * SaturationVapourPressureKpa(airTemperatureC);
        double respiratoryDryWatts = 0.0014 * metabolicPerM2 * (34.0 - airTemperatureC) * SurfaceAreaM2;
        double respiratoryLatentWatts = 0.0173 * metabolicPerM2 * (5.87 - ambientVapourKpa) * SurfaceAreaM2;
        double respiratoryWatts = Math.Max(0.0, respiratoryDryWatts + respiratoryLatentWatts);

        // ── Conduction to ground ────────────────────────────────────────────────
        double groundLossWatts = GroundConductionWatts(state, exposure, clothing, immersed);

        // ── Radiant gains ───────────────────────────────────────────────────────
        double fireGainWatts = immersed
            ? 0.0
            : exposure.FireIrradianceWattsPerM2 * 0.30 * SurfaceAreaM2 * 0.95;

        double solarGainWatts = immersed
            ? 0.0
            : environment.SolarIrradianceWattsPerM2 * 0.25 * SurfaceAreaM2 * 0.70
              * environment.Site.SkyViewFactor;

        // ── Integrate the two nodes ─────────────────────────────────────────────
        double conductancePerM2 = 5.28 + 1.163 * skinBloodFlow;

        // Immersion defeats the body's main defence. Vasoconstriction works by
        // letting a still shell of tissue insulate the core, and water washing past
        // the limbs destroys that shell and short-circuits the countercurrent
        // exchange in the arms and legs. Whole-body conductance in cold water stays
        // high no matter how hard the body clamps down, which is the physiological
        // reason immersion is measured in minutes and air exposure in hours.
        if (immersed)
            conductancePerM2 = Math.Max(conductancePerM2, ImmersedConductanceWattsPerM2Kelvin);

        double bloodConductance = conductancePerM2 * SurfaceAreaM2;
        double bloodFlowWatts = bloodConductance * (state.CoreTemperatureC - state.MeanSkinTemperatureC);

        double coreNetWatts = metabolicWatts - respiratoryWatts - bloodFlowWatts;
        double skinNetWatts = bloodFlowWatts - dryLossWatts - evaporativeLossWatts
                              - groundLossWatts + fireGainWatts + solarGainWatts;

        state.CoreTemperatureC += coreNetWatts * dt / CoreHeatCapacity;
        state.MeanSkinTemperatureC += skinNetWatts * dt / SkinHeatCapacity;

        // Skin cannot fall below its surroundings or rise above the core.
        double skinFloor = Math.Min(operativeTemperatureC, state.CoreTemperatureC);
        state.MeanSkinTemperatureC = Math.Clamp(
            state.MeanSkinTemperatureC, skinFloor, state.CoreTemperatureC);

        UpdateZoneTemperatures(
            state, zoneResistance, operativeTemperatureC, constriction, exposedFraction, exposure);

        AccumulateFrostbite(state, dt);
        UpdateClothingSweat(state, environment, clothing, regulatorySweating, dt);

        double netWatts = metabolicWatts - respiratoryWatts - dryLossWatts
                          - evaporativeLossWatts - groundLossWatts + fireGainWatts + solarGainWatts;
        state.RecordStep(metabolicWatts, shiveringWatts, netWatts, dt);
    }

    /// <summary>
    /// Shivering thermogenesis, W.
    /// <para>
    /// Gagge's product of cold signals, then two ceilings that matter dramatically.
    /// It is limited by available substrate, so a starving body cannot defend itself
    /// — docs/02 §1's <i>"You cannot stay warm while starving."</i> And it fades out
    /// below about 32 °C, because in real moderate hypothermia shivering stops. That
    /// is the point where heat production collapses and the fall to death
    /// accelerates, and to the player it reads as the shaking suddenly stopping,
    /// which feels like relief and is the opposite.
    /// </para>
    /// </summary>
    private static double ShiveringWatts(ThermalState state, double coldSkinSignal, double coldCoreSignal)
    {
        if (coldSkinSignal <= 0.0 || coldCoreSignal <= 0.0) return 0.0;

        double demandWatts = 19.4 * coldSkinSignal * coldCoreSignal * SurfaceAreaM2;

        double fuelFactor = Math.Clamp(state.EnergyReserveKcal / 500.0, 0.0, 1.0);

        // Shivering fails as the core falls through the moderate range.
        double capacityFactor = Math.Clamp((state.CoreTemperatureC - 30.0) / 2.0, 0.0, 1.0);

        return Math.Min(demandWatts, MaxShiveringWatts) * fuelFactor * capacityFactor;
    }

    private static double ConvectiveCoefficient(double windSpeedMetresPerSecond, bool immersed)
    {
        // Water carries heat away roughly 25× faster than still air, which is what
        // turns a lake in autumn into a countdown measured in minutes.
        if (immersed) return 100.0;

        double forced = 8.3 * Math.Pow(Math.Max(0.0, windSpeedMetresPerSecond), 0.6);
        return Math.Max(3.1, forced);
    }

    private static double RadiativeCoefficient(double surfaceTemperatureC, double radiantTemperatureC)
    {
        double meanKelvin = (surfaceTemperatureC + radiantTemperatureC) / 2.0 + 273.15;
        return 4.0 * SkinEmissivity * StefanBoltzmann * RadiatingAreaFactor * Math.Pow(meanKelvin, 3.0);
    }

    /// <summary>
    /// Mean radiant temperature the body sees, °C — a fourth-power blend of the sky
    /// and the surrounding ground and vegetation.
    /// <para>
    /// This is where a clear night does its damage. Half of a standing person's
    /// radiant hemisphere is sky, and on a clear cold night that sky behaves like a
    /// surface 25 °C below air temperature. Put a roof over them and the same air
    /// becomes survivable, which is the physical reason a debris shelter is worth
    /// building on the first evening.
    /// </para>
    /// </summary>
    private static double MeanRadiantTemperatureC(
        EnvironmentSample environment, double exposedFraction, bool immersed)
    {
        if (immersed) return environment.AirTemperatureC;

        double skyWeight = 0.5 * environment.Site.SkyViewFactor * exposedFraction;
        skyWeight = Math.Clamp(skyWeight, 0.0, 1.0);

        double skyKelvin = environment.SkyTemperatureC + 273.15;
        double surroundKelvin = environment.AirTemperatureC + 273.15;

        double blended = skyWeight * Math.Pow(skyKelvin, 4.0)
                         + (1.0 - skyWeight) * Math.Pow(surroundKelvin, 4.0);

        return Math.Pow(blended, 0.25) - 273.15;
    }

    private static double OperativeTemperatureC(
        double airTemperatureC, double radiantTemperatureC,
        double convectiveCoefficient, double radiativeCoefficient)
        => (convectiveCoefficient * airTemperatureC + radiativeCoefficient * radiantTemperatureC)
           / (convectiveCoefficient + radiativeCoefficient);

    private static double ZoneResistance(
        Insulation clothing, BodyZone zone, double windSpeed,
        double convectiveCoefficient, double radiativeCoefficient)
    {
        double clothingResistance = clothing.EffectiveResistance(zone, windSpeed);
        double areaFactor = clothing.AreaFactor(zone);
        double surfaceResistance = 1.0 / (areaFactor * (convectiveCoefficient + radiativeCoefficient));
        return clothingResistance + surfaceResistance;
    }

    /// <summary>
    /// Folds sweat saturation and immersion into the clothing the body is actually
    /// wearing this step. Wet insulation is barely insulation.
    /// </summary>
    private static Insulation EffectiveClothing(ThermalExposure exposure, ThermalState state, bool immersed)
    {
        if (immersed) return exposure.Clothing.WithWetness(1.0);

        double wetness = Math.Max(exposure.Clothing.Wetness, state.ClothingSweatSaturation);
        return exposure.Clothing.WithWetness(wetness);
    }

    /// <summary>
    /// How much of the skin is wet, and how much of that is fresh sweat.
    /// <para>
    /// The two numbers have to be kept apart. Total wettedness drives evaporative
    /// cooling and includes water that soaked in from rain or from earlier sweat;
    /// only the <i>regulatory</i> component is new sweat that can soak the clothing
    /// further. Conflating them makes wet clothing register as active sweating, which
    /// soaks it further still — a feedback loop that silently converts one warm
    /// moment into permanently ruined insulation.
    /// </para>
    /// </summary>
    private static (double Total, double RegulatorySweating) Wettedness(
        ThermalState state, Insulation clothing, double warmSkinSignal)
    {
        const double InsensibleWettedness = 0.06;

        double meanBodyC = SkinMassFraction * state.MeanSkinTemperatureC
                           + (1.0 - SkinMassFraction) * state.CoreTemperatureC;
        double neutralBodyC = SkinMassFraction * SkinSetPointC + (1.0 - SkinMassFraction) * CoreSetPointC;
        double warmBodySignal = Math.Max(0.0, meanBodyC - neutralBodyC);

        double sweating = warmBodySignal > 0.0
            ? Math.Clamp(0.30 * warmBodySignal * Math.Exp(warmSkinSignal / 10.7), 0.0, 1.0)
            : 0.0;

        // Water already in the clothing keeps evaporating whether you sweat or not.
        double soaked = 0.6 * clothing.Wetness;

        double total = Math.Clamp(Math.Max(Math.Max(InsensibleWettedness, sweating), soaked), 0.0, 1.0);
        return (total, sweating);
    }

    private static double EvaporativeLossWatts(
        ThermalState state, EnvironmentSample environment, Insulation clothing,
        double convectiveCoefficient, double exposedFraction, double wettedness, bool immersed)
    {
        // Nothing evaporates underwater.
        if (immersed) return 0.0;

        double skinVapourKpa = SaturationVapourPressureKpa(state.MeanSkinTemperatureC);
        double ambientVapourKpa = environment.RelativeHumidity
                                  * SaturationVapourPressureKpa(environment.AirTemperatureC);
        double deficit = skinVapourKpa - ambientVapourKpa;
        if (deficit <= 0.0) return 0.0;

        // Lewis relation.
        double evaporativeCoefficient = 16.5 * convectiveCoefficient;

        double total = 0.0;
        foreach (BodyZone zone in BodyZones.All)
        {
            double area = SurfaceAreaM2 * BodyZones.SurfaceAreaFraction(zone) * exposedFraction;
            double resistance = clothing.EvaporativeResistance(zone)
                                + 1.0 / (clothing.AreaFactor(zone) * evaporativeCoefficient);
            total += area * wettedness * deficit / resistance;
        }

        return total;
    }

    private static double GroundConductionWatts(
        ThermalState state, ThermalExposure exposure, Insulation clothing, bool immersed)
    {
        if (immersed) return 0.0;

        double contactFraction = Postures.GroundContactFraction(exposure.Posture);
        double area = SurfaceAreaM2 * contactFraction;

        // Skin-to-soil contact resistance is tiny; bedding is what saves you.
        const double ContactResistance = 0.02;
        double resistance = ContactResistance
                            + Math.Max(0.0, exposure.BeddingResistance)
                            + clothing.EffectiveResistance(BodyZone.Core, 0.0);

        return area * (state.MeanSkinTemperatureC - exposure.Environment.GroundTemperatureC) / resistance;
    }

    /// <summary>
    /// Distributes the integrated mean skin temperature across the four zones.
    /// <para>
    /// Each zone settles at a quasi-static balance between the blood heat it is
    /// given and the heat it loses, then the whole set is shifted so its
    /// area-weighted mean matches the integrated value. That keeps the energy
    /// balance honest while still letting the hands sit twenty degrees below the
    /// torso, which is the fact the frostbite and dexterity systems are reading.
    /// </para>
    /// </summary>
    private static void UpdateZoneTemperatures(
        ThermalState state, ReadOnlySpan<double> zoneResistance, double operativeTemperatureC,
        double constriction, double exposedFraction, ThermalExposure exposure)
    {
        Span<double> quasiStatic = stackalloc double[BodyZones.Count];
        double weightedMean = 0.0;

        double fireBoost = exposure.FireIrradianceWattsPerM2 * 0.30 * 0.95;

        foreach (BodyZone zone in BodyZones.All)
        {
            int index = (int)zone;
            double areaFraction = BodyZones.SurfaceAreaFraction(zone);
            double area = SurfaceAreaM2 * areaFraction * exposedFraction;

            double perfusionFactor = 1.0 - BodyZones.VasoconstrictionSensitivity(zone) * constriction;
            perfusionFactor = Math.Clamp(perfusionFactor, 0.02, 1.0);
            double perfusionConductance = BodyZones.BasePerfusionConductance(zone) * perfusionFactor * area;

            double environmentConductance = area / zoneResistance[index];

            // Radiant gain from a fire raises the effective environment this zone sees.
            double localEnvironmentC = operativeTemperatureC
                                       + (environmentConductance > 0.0
                                           ? fireBoost * area / environmentConductance
                                           : 0.0);

            double temperature =
                (perfusionConductance * state.CoreTemperatureC + environmentConductance * localEnvironmentC)
                / (perfusionConductance + environmentConductance);

            quasiStatic[index] = temperature;
            weightedMean += temperature * areaFraction;
        }

        double correction = state.MeanSkinTemperatureC - weightedMean;

        foreach (BodyZone zone in BodyZones.All)
        {
            int index = (int)zone;
            double corrected = quasiStatic[index] + correction;
            state.ZoneSkinTemperatureC[index] = Math.Clamp(
                corrected,
                Math.Min(operativeTemperatureC, state.CoreTemperatureC),
                state.CoreTemperatureC);
        }
    }

    private static void AccumulateFrostbite(ThermalState state, double dt)
    {
        foreach (BodyZone zone in BodyZones.All)
        {
            int index = (int)zone;
            double temperature = state.ZoneSkinTemperatureC[index];
            if (temperature >= TissueFreezingPointC) continue;

            double undercooling = TissueFreezingPointC - temperature;
            state.FrostbiteDamage[index] = Math.Clamp(
                state.FrostbiteDamage[index] + undercooling * FrostbiteRatePerKelvinSecond * dt,
                0.0, 1.0);
        }
    }

    private static void UpdateClothingSweat(
        ThermalState state, EnvironmentSample environment, Insulation clothing,
        double regulatorySweating, double dt)
    {
        // Only fresh sweat soaks inward. Water already in the fabric must not count
        // as production, or the garment soaks itself.
        double production = Math.Max(0.0, regulatorySweating - 0.06);
        double soakRate = production * Math.Clamp(clothing.MeanClo / 2.0, 0.0, 1.0) / 900.0;

        // Drying needs dry air moving over it, and in a cold wet climate it is slow.
        double dryness = Math.Clamp(1.0 - environment.RelativeHumidity, 0.0, 1.0);
        double airflow = 1.0 + environment.WindSpeedMetresPerSecond;
        double dryRate = state.ClothingSweatSaturation * dryness * airflow / 7200.0;

        state.ClothingSweatSaturation = Math.Clamp(
            state.ClothingSweatSaturation + (soakRate - dryRate) * dt, 0.0, 1.0);
    }

    /// <summary>Saturation vapour pressure over water, kPa, by the Magnus formula.</summary>
    public static double SaturationVapourPressureKpa(double temperatureC)
        => 0.61094 * Math.Exp(17.625 * temperatureC / (temperatureC + 243.04));

    /// <summary>
    /// Advances clothing wetness for rain and drying, returning updated insulation.
    /// <para>
    /// Kept as a pure function on the side rather than folded into
    /// <see cref="Step"/> because clothing is owned by the equipment layer, not by
    /// the body. Sweat is the body's business and is tracked in
    /// <see cref="ThermalState.ClothingSweatSaturation"/>; rain is the world's.
    /// </para>
    /// </summary>
    public static Insulation UpdateClothingWetness(
        Insulation clothing, EnvironmentSample environment, bool sheltered, double worldSeconds)
    {
        double wetness = clothing.Wetness;

        if (!sheltered && environment.Precipitation == PrecipitationKind.Rain)
        {
            // A few minutes of steady rain is enough to matter; an hour soaks you through.
            double soakRate = Math.Clamp(environment.PrecipitationMmPerHour / 4.0, 0.0, 1.0) / 1800.0;
            wetness += soakRate * worldSeconds;
        }
        else
        {
            double dryness = Math.Clamp(1.0 - environment.RelativeHumidity, 0.0, 1.0);
            double airflow = 1.0 + environment.WindSpeedMetresPerSecond;
            // Below freezing, water in the fabric is ice and simply will not leave.
            double thaw = environment.AirTemperatureC > 0.0 ? 1.0 : 0.15;
            wetness -= wetness * dryness * airflow * thaw / 5400.0 * worldSeconds;
        }

        return clothing.WithWetness(Math.Clamp(wetness, 0.0, 1.0));
    }
}
