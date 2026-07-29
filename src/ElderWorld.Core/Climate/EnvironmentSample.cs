using ElderWorld.Core.Time;

namespace ElderWorld.Core.Climate;

/// <summary>What kind of precipitation is falling.</summary>
public enum PrecipitationKind
{
    /// <summary>Nothing falling.</summary>
    None,

    /// <summary>Liquid. The most dangerous weather in the game, because wet plus cold kills.</summary>
    Rain,

    /// <summary>Frozen. Colder, and far safer, than rain at 2 °C.</summary>
    Snow,
}

/// <summary>
/// The complete state of the world's physical environment at one place and moment.
/// Everything the thermal model and the fire model need to read.
/// </summary>
public readonly record struct EnvironmentSample
{
    /// <summary>World time this was sampled at, seconds since the epoch.</summary>
    public double WorldSeconds { get; init; }

    /// <summary>Air temperature, °C.</summary>
    public double AirTemperatureC { get; init; }

    /// <summary>Relative humidity, 0–1.</summary>
    public double RelativeHumidity { get; init; }

    /// <summary>Wind speed at the site, m/s, after terrain exposure.</summary>
    public double WindSpeedMetresPerSecond { get; init; }

    /// <summary>Cloud cover, 0 = clear to 1 = overcast.</summary>
    public double CloudCover { get; init; }

    /// <summary>Precipitation rate, mm/hour water-equivalent.</summary>
    public double PrecipitationMmPerHour { get; init; }

    /// <summary>Rain, snow, or nothing.</summary>
    public PrecipitationKind Precipitation { get; init; }

    /// <summary>
    /// Effective radiant temperature of the sky, °C.
    /// <para>
    /// On a clear cold night this runs 20–30 °C below air temperature, and that
    /// difference is what makes docs/02 §1's claim true: <i>"Clear nights are
    /// brutally colder than overcast ones because radiative loss to a clear sky is
    /// real and models nicely."</i> It is not a fudge factor — it is where the heat
    /// actually goes.
    /// </para>
    /// </summary>
    public double SkyTemperatureC { get; init; }

    /// <summary>
    /// Ground surface temperature, °C. Lags the air by weeks because soil has
    /// thermal mass. Matters the moment you sit or lie down without a bough bed.
    /// </summary>
    public double GroundTemperatureC { get; init; }

    /// <summary>
    /// Lake and stream temperature, °C.
    /// <para>
    /// Water has enormous thermal mass, so this lags the air by roughly two months
    /// and is heavily damped. That lag is the trap: an autumn afternoon can be
    /// pleasant while the lake is still only a few degrees, which is exactly the
    /// emergency docs/02 §1 describes. Floored at 0 °C, below which the lake is ice.
    /// </para>
    /// </summary>
    public double WaterTemperatureC { get; init; }

    /// <summary>Fraction of ground under snow, 0–1.</summary>
    public double SnowCoverFraction { get; init; }

    /// <summary>Global horizontal solar irradiance, W/m².</summary>
    public double SolarIrradianceWattsPerM2 { get; init; }

    /// <summary>Where the sun is.</summary>
    public SolarPosition Sun { get; init; }

    /// <summary>Where the moon is, and how much of it is lit.</summary>
    public LunarPosition Moon { get; init; }

    /// <summary>The site these values were sampled for.</summary>
    public SiteContext Site { get; init; }

    /// <summary>
    /// Ground-level illuminance in lux, from whichever of sun, moon and stars are
    /// available, reduced by cloud and canopy.
    /// <para>
    /// The greybox uses this to decide how dark the night actually is. Snow raises
    /// it through albedo, which is what makes a moonlit winter landscape navigable
    /// and a moonless one not (docs/02 §6).
    /// </para>
    /// </summary>
    public double IlluminanceLux
    {
        get
        {
            // Rough daylight conversion: ~110 lux per W/m² of global irradiance.
            double sunlight = SolarIrradianceWattsPerM2 * 110.0;

            double cloudTransmission = 1.0 - 0.80 * CloudCover;
            double moonlight = Moon.IlluminanceLux * cloudTransmission;

            // Starlight and airglow, the floor below which a night never goes.
            const double Starlight = 0.002;
            double skyGlow = (moonlight + Starlight * cloudTransmission);

            // Fresh snow throws back most of what lands on it. This is why a
            // midwinter night can be brighter than an autumn one.
            double albedoGain = 1.0 + 0.7 * SnowCoverFraction;

            double canopy = Math.Clamp(Site.SkyViewFactor, 0.02, 1.0);

            return sunlight * canopy + skyGlow * albedoGain * canopy;
        }
    }

    /// <summary>True when there is not enough light to work outside without fire.</summary>
    public bool IsDark => IlluminanceLux < 1.0;
}
