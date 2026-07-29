using ElderWorld.Core.Determinism;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Climate;

/// <summary>
/// The weather. A pure function of (seed, world time, site) — no internal state, so
/// it cannot desynchronise between a 1× client and a 1000× soak run, needs nothing
/// in the save file, and gives identical results on every machine.
/// <para>
/// Structure, coldest layer outward: a seasonal curve, a diurnal curve shaped by the
/// actual sunrise and sunset times, a slow synoptic term for cold snaps and thaws,
/// and an elevation lapse. Sky radiant temperature is computed properly rather than
/// approximated, because the gap between a clear sky and an overcast one is a
/// first-order survival fact in this game.
/// </para>
/// </summary>
public sealed class ClimateModel
{
    private readonly uint _seed;
    private readonly ClimateParameters _parameters;

    // Distinct noise streams. Offsets are arbitrary but fixed forever, because
    // changing one would change the weather of every existing save.
    private const uint SynopticStream = 0x00000001u;
    private const uint CloudStream = 0x5BF03A11u;
    private const uint WindStream = 0xA13C77E9u;
    private const uint HumidityStream = 0x2E9B4D07u;

    /// <summary>Creates a climate for a world seed.</summary>
    public ClimateModel(uint seed, ClimateParameters? parameters = null)
    {
        _seed = seed;
        _parameters = parameters ?? ClimateParameters.Yixian;
    }

    /// <summary>The parameters this climate was built with.</summary>
    public ClimateParameters Parameters => _parameters;

    /// <summary>Samples the environment at a moment and a place.</summary>
    public EnvironmentSample Sample(WorldClock clock, SiteContext site)
        => Sample(clock.TotalSeconds, site);

    /// <summary>Samples the environment at a world time and a place.</summary>
    public EnvironmentSample Sample(double worldSeconds, SiteContext site)
    {
        site = site.Normalised();

        double days = worldSeconds / CretaceousCalendar.SecondsPerDay;
        double yearPhase = Wrap01(worldSeconds / CretaceousCalendar.SecondsPerYear);
        double timeOfDay = Wrap01(days);
        double lunarPhase = Wrap01(worldSeconds / CretaceousCalendar.SecondsPerLunarMonth);

        SolarPosition sun = SolarPosition.Compute(yearPhase, timeOfDay);
        LunarPosition moon = LunarPosition.Compute(yearPhase, timeOfDay, lunarPhase);

        double synoptic = DeterministicNoise.Fractal(
            _seed ^ SynopticStream, days / _parameters.SynopticPeriodDays, octaves: 2);

        double cloudCover = CloudCoverAt(days, synoptic);
        double windSpeed = WindSpeedAt(days, synoptic, site);

        double airTemperature = AirTemperatureAt(yearPhase, timeOfDay, synoptic, cloudCover, windSpeed, site);
        double humidity = RelativeHumidityAt(days, cloudCover);

        (double precipitationRate, PrecipitationKind precipitationKind) =
            PrecipitationAt(cloudCover, humidity, airTemperature);

        double snowCover = SnowCoverAt(yearPhase, site);
        double solarIrradiance = sun.ClearSkyIrradiance(cloudCover, site.ElevationMetres);

        return new EnvironmentSample
        {
            WorldSeconds = worldSeconds,
            AirTemperatureC = airTemperature,
            RelativeHumidity = humidity,
            WindSpeedMetresPerSecond = windSpeed,
            CloudCover = cloudCover,
            PrecipitationMmPerHour = precipitationRate,
            Precipitation = precipitationKind,
            SkyTemperatureC = SkyTemperatureAt(airTemperature, humidity, cloudCover),
            GroundTemperatureC = GroundTemperatureAt(yearPhase, site, snowCover),
            WaterTemperatureC = WaterTemperatureAt(yearPhase, site),
            SnowCoverFraction = snowCover,
            SolarIrradianceWattsPerM2 = solarIrradiance,
            Sun = sun,
            Moon = moon,
            Site = site,
        };
    }

    /// <summary>
    /// Mean temperature of a given day of the year at the reference elevation, °C,
    /// before weather. Exposed because the fire, snow and ecology models all want
    /// the season rather than the moment.
    /// </summary>
    public double SeasonalMeanC(double yearPhase)
    {
        double phase = 2.0 * Math.PI * (yearPhase - _parameters.SeasonalLagFraction);
        return _parameters.AnnualMeanC - _parameters.SeasonalAmplitudeC * Math.Cos(phase);
    }

    /// <summary>How deep into winter we are, 0 at the warmest point of the year and 1 at the coldest.</summary>
    public double Winterness(double yearPhase)
    {
        double phase = 2.0 * Math.PI * (yearPhase - _parameters.SeasonalLagFraction);
        return (1.0 + Math.Cos(phase)) / 2.0;
    }

    private double AirTemperatureAt(
        double yearPhase, double timeOfDay, double synoptic,
        double cloudCover, double windSpeed, SiteContext site)
    {
        double seasonalMean = SeasonalMeanC(yearPhase);
        double winterness = Winterness(yearPhase);

        // Synoptic variability is strongly seasonal at midlatitudes: winter brings
        // cold-air outbreaks that swing the temperature far more than anything
        // summer produces. Modelling that asymmetry is what lets deep winter reach
        // −20 °C without high summer overshooting the +24 °C of docs/01 §2.
        double synopticOffset = synoptic * _parameters.SynopticAmplitudeC
                                * (_parameters.SynopticSummerFraction
                                   + (1.0 - _parameters.SynopticSummerFraction) * winterness);

        // Clear, calm air swings hardest: cloud traps outgoing longwave and wind
        // mixes the surface layer, and both flatten the daily range. This is the
        // mechanism that makes a still clear night the dangerous one.
        double clearness = 1.0 - 0.65 * cloudCover;
        double calmness = 1.0 / (1.0 + 0.25 * windSpeed);

        double halfRange =
            (_parameters.SummerDiurnalHalfRangeC + _parameters.WinterDiurnalBoostC * winterness)
            * clearness * calmness;

        double lapse = (site.ElevationMetres - _parameters.ReferenceElevationMetres)
                       / 1000.0 * _parameters.LapseRateCPerKm;

        return seasonalMean + synopticOffset + DiurnalOffsetC(yearPhase, timeOfDay, halfRange) - lapse;
    }

    /// <summary>
    /// The shape of a day's temperature, in °C either side of the daily mean.
    /// <para>
    /// A Parton–Logan curve: a sine from sunrise to a mid-afternoon peak, then
    /// exponential decay through the night. It is used in agrometeorology because it
    /// matches real records, and here it earns its place by making the minimum fall
    /// <i>just before sunrise</i> rather than at midnight. In a game whose central
    /// test is whether an eleven-hour night is compelling, the fact that the last
    /// hour before dawn is the coldest one is worth modelling properly.
    /// </para>
    /// </summary>
    private static double DiurnalOffsetC(double yearPhase, double timeOfDay, double halfRange)
    {
        double sunrise = SolarPosition.SunriseTimeOfDay(yearPhase);
        double sunset = SolarPosition.SunsetTimeOfDay(yearPhase);
        double dayLength = sunset - sunrise;
        double nightLength = 1.0 - dayLength;

        // Peak lags solar noon; expressed as a fraction of the daylit period.
        const double PeakLag = 0.15;
        const double NocturnalDecay = 2.2;

        if (timeOfDay >= sunrise && timeOfDay <= sunset)
        {
            double through = (timeOfDay - sunrise) / dayLength;
            // Stretching the sine puts its crest after solar noon instead of on it.
            double shaped = Math.Sin(Math.PI * through / (1.0 + 2.0 * PeakLag));
            return halfRange * (2.0 * shaped - 1.0);
        }

        // Night: decay from the temperature at sunset down to the pre-dawn minimum.
        double sinceSunset = timeOfDay > sunset
            ? timeOfDay - sunset
            : timeOfDay + 1.0 - sunset;
        double throughNight = sinceSunset / nightLength;

        double atSunset = halfRange * (2.0 * Math.Sin(Math.PI / (1.0 + 2.0 * PeakLag)) - 1.0);
        double decay = (Math.Exp(-NocturnalDecay * throughNight) - Math.Exp(-NocturnalDecay))
                       / (1.0 - Math.Exp(-NocturnalDecay));

        return -halfRange + (atSunset + halfRange) * decay;
    }

    private double CloudCoverAt(double days, double synoptic)
    {
        double fast = DeterministicNoise.Fractal(_seed ^ CloudStream, days / 1.6, octaves: 3);
        // Cloud tracks the synoptic pattern: low pressure brings both cloud and warmth.
        double combined = 0.55 * synoptic + 0.45 * fast;
        return Math.Clamp(0.5 + 0.65 * combined, 0.0, 1.0);
    }

    private double WindSpeedAt(double days, double synoptic, SiteContext site)
    {
        double gusting = DeterministicNoise.Fractal(_seed ^ WindStream, days / 0.35, octaves: 3);
        double freeStream = _parameters.MeanWindSpeedMetresPerSecond
                            * (1.0 + 0.55 * synoptic + 0.45 * gusting);

        // Terrain shelter never quite reaches zero — even deep forest breathes.
        double sheltered = freeStream * (0.05 + 0.95 * site.WindExposure);
        return Math.Max(0.1, sheltered);
    }

    private double RelativeHumidityAt(double days, double cloudCover)
    {
        double drift = DeterministicNoise.Fractal(_seed ^ HumidityStream, days / 2.4, octaves: 2);
        return Math.Clamp(0.55 + 0.30 * cloudCover + 0.12 * drift, 0.15, 1.0);
    }

    private (double RateMmPerHour, PrecipitationKind Kind) PrecipitationAt(
        double cloudCover, double humidity, double airTemperatureC)
    {
        // Precipitation needs both thick cloud and moisture. Below the threshold
        // nothing falls at all, which keeps most days dry.
        double potential = (cloudCover - 0.72) / 0.28 * (humidity - 0.60) / 0.40;
        if (potential <= 0.0) return (0.0, PrecipitationKind.None);

        double rate = Math.Clamp(potential, 0.0, 1.0) * 4.0;
        if (rate < 0.05) return (0.0, PrecipitationKind.None);

        PrecipitationKind kind = airTemperatureC > _parameters.RainSnowThresholdC
            ? PrecipitationKind.Rain
            : PrecipitationKind.Snow;

        return (rate, kind);
    }

    /// <summary>
    /// Effective radiant temperature of the sky, °C.
    /// <para>
    /// Clear-sky emissivity from the Berdahl–Fromberg relation on dew point, blended
    /// toward a black cloud deck as cover increases, then Stefan–Boltzmann inverted
    /// for a radiant temperature. On a clear −10 °C night this returns about −36 °C;
    /// overcast, about −11 °C. That 25 °C spread is worth roughly 40 W of radiative
    /// loss to a standing person, which is the difference between a survivable night
    /// and a fatal one.
    /// </para>
    /// </summary>
    private static double SkyTemperatureAt(double airTemperatureC, double humidity, double cloudCover)
    {
        double dewPointC = DewPointC(airTemperatureC, humidity);
        double clearEmissivity = Math.Clamp(0.741 + 0.0062 * dewPointC, 0.55, 1.0);

        // Cloud radiates at close to its own temperature, so overcast nearly closes
        // the gap to air temperature.
        double emissivity = clearEmissivity + (0.98 - clearEmissivity) * Math.Clamp(cloudCover, 0.0, 1.0);

        double airKelvin = airTemperatureC + 273.15;
        double skyKelvin = airKelvin * Math.Pow(emissivity, 0.25);
        return skyKelvin - 273.15;
    }

    /// <summary>Dew point from air temperature and relative humidity, by the Magnus formula.</summary>
    public static double DewPointC(double airTemperatureC, double relativeHumidity)
    {
        const double A = 17.625;
        const double B = 243.04;

        double rh = Math.Clamp(relativeHumidity, 0.01, 1.0);
        double gamma = Math.Log(rh) + A * airTemperatureC / (B + airTemperatureC);
        return B * gamma / (A - gamma);
    }

    /// <summary>
    /// Ground surface temperature, °C.
    /// <para>
    /// Soil carries weeks of thermal inertia, so this follows the seasonal curve
    /// shifted a month later and damped, and ignores the daily swing entirely. Under
    /// snow it is pinned near freezing, because snow is an excellent insulator and
    /// the soil beneath it sits at the latent-heat plateau — which is why a snow
    /// hollow is warmer to lie in than bare frozen ground, and why bare frozen
    /// ground in a clear snap will take everything you have.
    /// </para>
    /// </summary>
    private double GroundTemperatureAt(double yearPhase, SiteContext site, double snowCover)
    {
        const double LagFraction = 0.08; // About a month behind the air.
        const double Damping = 0.75;

        double lagged = SeasonalMeanC(Wrap01(yearPhase - LagFraction));
        double damped = _parameters.AnnualMeanC + (lagged - _parameters.AnnualMeanC) * Damping;

        double lapse = (site.ElevationMetres - _parameters.ReferenceElevationMetres)
                       / 1000.0 * _parameters.LapseRateCPerKm;
        double bare = damped - lapse;

        if (snowCover <= 0.0) return bare;

        // Under snow the soil sits at the latent-heat plateau near freezing rather
        // than tracking the air down.
        double insulated = Math.Max(bare, -1.5);
        return bare * (1.0 - snowCover) + insulated * snowCover;
    }

    /// <summary>
    /// Lake temperature, °C — heavily damped and lagged about two months behind the
    /// air, floored at freezing.
    /// <para>
    /// The lag is the whole point. On a mild autumn afternoon the air can be 12 °C
    /// while the lake is still 5 °C, and the player has no instrument for telling the
    /// difference until they are in it.
    /// </para>
    /// </summary>
    private double WaterTemperatureAt(double yearPhase, SiteContext site)
    {
        const double LagFraction = 0.16;
        const double Damping = 0.55;

        double lagged = SeasonalMeanC(Wrap01(yearPhase - LagFraction));
        double damped = _parameters.AnnualMeanC + (lagged - _parameters.AnnualMeanC) * Damping;

        double lapse = (site.ElevationMetres - _parameters.ReferenceElevationMetres)
                       / 1000.0 * _parameters.LapseRateCPerKm;

        return Math.Max(0.0, damped - lapse);
    }

    /// <summary>
    /// Fraction of ground under snow, 0–1.
    /// <para>
    /// <b>Stage 1 placeholder.</b> A smooth function of the seasonal curve, with melt
    /// lagging accumulation so spring keeps its snow a while. The real model in
    /// docs/02 §7 accumulates and persists snow by depth, with movement cost, roof
    /// loading and tracking, and it needs the state this deliberately does not carry.
    /// This exists so the greybox has ground albedo and the thermal model has a
    /// ground temperature that behaves.
    /// </para>
    /// </summary>
    private double SnowCoverAt(double yearPhase, SiteContext site)
    {
        double lapse = (site.ElevationMetres - _parameters.ReferenceElevationMetres)
                       / 1000.0 * _parameters.LapseRateCPerKm;

        // Snow lies once the season's mean is near freezing, and lingers past it.
        double meltLag = 0.03;
        double seasonal = SeasonalMeanC(Wrap01(yearPhase - meltLag)) - lapse;

        const double FullSnowBelowC = -1.0;
        const double NoSnowAboveC = 4.0;

        double t = (NoSnowAboveC - seasonal) / (NoSnowAboveC - FullSnowBelowC);
        return Math.Clamp(t, 0.0, 1.0);
    }

    private static double Wrap01(double value)
    {
        double fraction = value - Math.Floor(value);
        return fraction >= 1.0 ? 0.0 : fraction;
    }
}
