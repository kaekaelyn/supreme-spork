namespace ElderWorld.Core.Time;

/// <summary>
/// Where the sun is, and how hard it is shining. Pure function of world time and
/// latitude — no state, so it is automatically deterministic and identical at any
/// debug time scale.
/// <para>
/// The player uses this without being told: the sun rising against a fixed point on
/// the ridge is the only instrument available for measuring the 374.615-day year
/// (docs/02 §6). A stone alignment on the ridge is real infrastructure, which is
/// why this needs to be right rather than merely pretty.
/// </para>
/// </summary>
public readonly struct SolarPosition
{
    /// <summary>Angle above the horizon, radians. Negative when the sun is down.</summary>
    public double AltitudeRadians { get; }

    /// <summary>Compass bearing, radians clockwise from north.</summary>
    public double AzimuthRadians { get; }

    private SolarPosition(double altitudeRadians, double azimuthRadians)
    {
        AltitudeRadians = altitudeRadians;
        AzimuthRadians = azimuthRadians;
    }

    /// <summary>Altitude in degrees.</summary>
    public double AltitudeDegrees => AltitudeRadians * 180.0 / Math.PI;

    /// <summary>Azimuth in degrees clockwise from north.</summary>
    public double AzimuthDegrees => AzimuthRadians * 180.0 / Math.PI;

    /// <summary>True when the sun's centre is above the geometric horizon.</summary>
    public bool IsUp => AltitudeRadians > 0.0;

    /// <summary>
    /// Civil twilight or better — enough light to work outside without fire.
    /// The sun is within 6° below the horizon.
    /// </summary>
    public bool IsCivilTwilightOrBrighter => AltitudeRadians > -6.0 * Math.PI / 180.0;

    /// <summary>
    /// Solar declination in radians for a point in the year, on a circular-orbit
    /// approximation.
    /// <para>
    /// Orbital eccentricity is ignored: the Cretaceous value is not constrained by
    /// anything in docs/01, and on a circular orbit the seasons are symmetric, which
    /// is the conservative choice. It costs a small asymmetry between the halves of
    /// the year that no player could detect without instruments they cannot build.
    /// </para>
    /// </summary>
    /// <param name="yearPhase">Fraction of the year since the winter solstice, 0–1.</param>
    public static double DeclinationRadians(double yearPhase)
    {
        double tilt = CretaceousCalendar.AxialTiltDegrees * Math.PI / 180.0;
        // Ecliptic longitude measured from the vernal equinox. Year phase runs from
        // the winter solstice, which sits a quarter-turn earlier.
        double eclipticLongitude = 2.0 * Math.PI * yearPhase - Math.PI / 2.0;
        return Math.Asin(Math.Sin(tilt) * Math.Sin(eclipticLongitude));
    }

    /// <summary>
    /// Fraction of the day between sunrise and sunset, 0–1.
    /// <para>
    /// At 42° N this runs from about 0.372 at the winter solstice to 0.628 at the
    /// summer solstice. Multiplied by the 23.4-hour day that is 8.7 hours of
    /// daylight in midwinter, so a <b>14.7-hour night</b>; the year-round mean is
    /// exactly half a day, 11.7 hours (docs/02 §6).
    /// </para>
    /// </summary>
    public static double DaylightFraction(double yearPhase, double latitudeDegrees = CretaceousCalendar.PaleolatitudeDegrees)
    {
        double latitude = latitudeDegrees * Math.PI / 180.0;
        double declination = DeclinationRadians(yearPhase);
        double cosHourAngle = -Math.Tan(latitude) * Math.Tan(declination);

        if (cosHourAngle <= -1.0) return 1.0; // Midnight sun — not reachable at 42° N.
        if (cosHourAngle >= 1.0) return 0.0;  // Polar night — likewise.

        return Math.Acos(cosHourAngle) / Math.PI;
    }

    /// <summary>Time of day at sunrise, as a fraction of the day (solar noon = 0.5).</summary>
    public static double SunriseTimeOfDay(double yearPhase, double latitudeDegrees = CretaceousCalendar.PaleolatitudeDegrees)
        => 0.5 - DaylightFraction(yearPhase, latitudeDegrees) / 2.0;

    /// <summary>Time of day at sunset, as a fraction of the day.</summary>
    public static double SunsetTimeOfDay(double yearPhase, double latitudeDegrees = CretaceousCalendar.PaleolatitudeDegrees)
        => 0.5 + DaylightFraction(yearPhase, latitudeDegrees) / 2.0;

    /// <summary>Length of the night in real hours.</summary>
    public static double NightLengthHours(double yearPhase, double latitudeDegrees = CretaceousCalendar.PaleolatitudeDegrees)
        => (1.0 - DaylightFraction(yearPhase, latitudeDegrees)) * CretaceousCalendar.HoursPerDay;

    /// <summary>Computes the sun's position for a moment in the world.</summary>
    public static SolarPosition Compute(
        double yearPhase,
        double timeOfDay,
        double latitudeDegrees = CretaceousCalendar.PaleolatitudeDegrees)
    {
        double latitude = latitudeDegrees * Math.PI / 180.0;
        double declination = DeclinationRadians(yearPhase);
        double hourAngle = 2.0 * Math.PI * (timeOfDay - 0.5);

        return FromSphericals(latitude, declination, hourAngle);
    }

    /// <summary>
    /// Shared horizon-coordinate conversion, so the moon uses exactly the same
    /// geometry as the sun.
    /// </summary>
    internal static SolarPosition FromSphericals(double latitude, double declination, double hourAngle)
    {
        double sinAltitude =
            Math.Sin(latitude) * Math.Sin(declination) +
            Math.Cos(latitude) * Math.Cos(declination) * Math.Cos(hourAngle);
        sinAltitude = Math.Clamp(sinAltitude, -1.0, 1.0);
        double altitude = Math.Asin(sinAltitude);

        double cosAzimuth =
            (Math.Sin(declination) - sinAltitude * Math.Sin(latitude)) /
            (Math.Cos(altitude) * Math.Cos(latitude));
        cosAzimuth = Math.Clamp(cosAzimuth, -1.0, 1.0);
        double azimuth = Math.Acos(cosAzimuth);

        // Before noon the sun is east of the meridian; after, west.
        if (Math.Sin(hourAngle) > 0.0)
            azimuth = 2.0 * Math.PI - azimuth;

        return new SolarPosition(altitude, azimuth);
    }

    /// <summary>
    /// Clear-sky global irradiance on a horizontal surface, W/m².
    /// <para>
    /// Kasten–Young air mass with a bulk atmospheric transmittance, raised for the
    /// basin's ~2 km elevation, plus a crude diffuse term. This feeds the thermal
    /// model's solar gain and the climate model's diurnal swing. It is a
    /// greybox-grade approximation and is not trying to be a radiative transfer code.
    /// </para>
    /// </summary>
    /// <param name="cloudCover">0 = clear, 1 = overcast.</param>
    /// <param name="elevationMetres">Height above sea level.</param>
    public double ClearSkyIrradiance(double cloudCover = 0.0, double elevationMetres = 2000.0)
    {
        if (AltitudeRadians <= 0.0) return 0.0;

        const double SolarConstant = 1361.0;
        double altitudeDegrees = AltitudeDegrees;

        // Kasten & Young (1989). Behaves near the horizon where 1/sin blows up.
        double airMass = 1.0 / (Math.Sin(AltitudeRadians)
            + 0.50572 * Math.Pow(altitudeDegrees + 6.07995, -1.6364));

        // Thinner air at altitude: roughly 12% less mass per km.
        double pressureRatio = Math.Exp(-elevationMetres / 8500.0);
        double transmittance = Math.Pow(0.7, Math.Pow(airMass * pressureRatio, 0.678));

        double direct = SolarConstant * transmittance * Math.Sin(AltitudeRadians);
        double diffuse = 0.10 * SolarConstant * Math.Sin(AltitudeRadians);

        // Overcast still passes a fair amount of diffuse light — it is grey, not black.
        double cloudFactor = 1.0 - 0.75 * Math.Clamp(cloudCover, 0.0, 1.0);
        return (direct + diffuse) * cloudFactor;
    }
}
