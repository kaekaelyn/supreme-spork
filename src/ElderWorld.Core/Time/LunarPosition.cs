namespace ElderWorld.Core.Time;

/// <summary>
/// Where the moon is and how much light it is throwing.
/// <para>
/// This matters more here than in most games. Nights average 11.7 hours and reach
/// 14.7 in midwinter, and docs/02 §6 makes the moon the difference between a
/// navigable night and a blind one: <i>"Snow has albedo. A snow-covered landscape
/// under a gibbous moon is genuinely navigable. Moonless nights are the dark ones,
/// which gives night a ~30-day rhythm and makes the lunar cycle worth tracking."</i>
/// That rhythm is a reason to build a calendar, so the phase has to be trackable by
/// eye and consistent over years.
/// </para>
/// <para>
/// <b>Approximation.</b> The moon is placed on the ecliptic, ignoring the 5.14°
/// inclination of its orbit and all the classical lunar inequalities. The phase
/// cycle, the rising-later-each-night behaviour and the seasonal swing in the
/// moon's altitude all come out right, which is everything the game reads from it.
/// A player cannot measure the difference with instruments they are able to build.
/// </para>
/// </summary>
public readonly struct LunarPosition
{
    /// <summary>Angle above the horizon, radians.</summary>
    public double AltitudeRadians { get; }

    /// <summary>Compass bearing, radians clockwise from north.</summary>
    public double AzimuthRadians { get; }

    /// <summary>Fraction of the disc lit, 0 at new moon and 1 at full.</summary>
    public double IlluminatedFraction { get; }

    private LunarPosition(double altitudeRadians, double azimuthRadians, double illuminatedFraction)
    {
        AltitudeRadians = altitudeRadians;
        AzimuthRadians = azimuthRadians;
        IlluminatedFraction = illuminatedFraction;
    }

    /// <summary>Altitude in degrees.</summary>
    public double AltitudeDegrees => AltitudeRadians * 180.0 / Math.PI;

    /// <summary>True when the moon is above the horizon.</summary>
    public bool IsUp => AltitudeRadians > 0.0;

    /// <summary>Computes the moon's position and phase for a moment in the world.</summary>
    /// <param name="yearPhase">Fraction of the year since the winter solstice, 0–1.</param>
    /// <param name="timeOfDay">Fraction of the day, solar noon = 0.5.</param>
    /// <param name="lunarPhase">Fraction of the synodic month, 0 = new, 0.5 = full.</param>
    /// <param name="latitudeDegrees">Observer latitude, degrees north.</param>
    public static LunarPosition Compute(
        double yearPhase,
        double timeOfDay,
        double lunarPhase,
        double latitudeDegrees = CretaceousCalendar.PaleolatitudeDegrees)
    {
        double latitude = latitudeDegrees * Math.PI / 180.0;
        double tilt = CretaceousCalendar.AxialTiltDegrees * Math.PI / 180.0;

        // The moon's ecliptic longitude leads the sun's by exactly the phase, which
        // is what the phase means.
        double sunLongitude = 2.0 * Math.PI * yearPhase - Math.PI / 2.0;
        double moonLongitude = sunLongitude + 2.0 * Math.PI * lunarPhase;

        double declination = Math.Asin(Math.Sin(tilt) * Math.Sin(moonLongitude));

        // Transit is delayed from solar noon by the phase: a new moon crosses the
        // meridian at midday, a full moon at midnight.
        double hourAngle = 2.0 * Math.PI * (timeOfDay - 0.5) - 2.0 * Math.PI * lunarPhase;

        SolarPosition horizon = SolarPosition.FromSphericals(latitude, declination, hourAngle);
        double illuminated = (1.0 - Math.Cos(2.0 * Math.PI * lunarPhase)) / 2.0;

        return new LunarPosition(horizon.AltitudeRadians, horizon.AzimuthRadians, illuminated);
    }

    /// <summary>
    /// Ground illuminance from the moon alone, in lux, before cloud or canopy.
    /// <para>
    /// A full moon at the zenith gives about 0.26 lux. Brightness is markedly
    /// non-linear in phase — the opposition surge means a half moon is nearer a
    /// tenth of a full moon than a half — which is why the exponent is here and why
    /// only the few nights around full are actually useful for travelling.
    /// </para>
    /// </summary>
    public double IlluminanceLux
    {
        get
        {
            if (AltitudeRadians <= 0.0) return 0.0;

            const double FullMoonZenithLux = 0.26;
            double phaseBrightness = Math.Pow(IlluminatedFraction, 2.4);

            // Long slant path through the atmosphere dims a low moon considerably.
            double extinction = Math.Pow(Math.Sin(AltitudeRadians), 0.5);

            return FullMoonZenithLux * phaseBrightness * extinction;
        }
    }
}
