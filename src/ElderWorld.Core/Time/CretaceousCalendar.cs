namespace ElderWorld.Core.Time;

/// <summary>
/// The astronomy of the world, as constants. Derived in docs/01 §8 and docs/02 §6.
/// <para>
/// None of this is ever shown to the player (docs/00 §3). The player derives the
/// year length by watching where the sun rises against the ridge, or dies of being
/// wrong about it.
/// </para>
/// </summary>
public static class CretaceousCalendar
{
    /// <summary>
    /// Length of a day, in real hours. Earth's rotation has been decelerating at
    /// roughly 1.7 ms/century; over 125 Myr that gives a day of about 23.4 hours.
    /// <para>
    /// Do not round this to 24. The 0.6-hour difference is what produces
    /// <see cref="PrecessionCycleRealDays"/>, which is the single detail that makes
    /// a 1:1 clock survivable for a player with a fixed play schedule.
    /// </para>
    /// </summary>
    public const double HoursPerDay = 23.4;

    /// <summary>Length of a day in seconds: 84,240.</summary>
    public const double SecondsPerDay = HoursPerDay * 3600.0;

    /// <summary>
    /// Hours in a year. The orbital period is essentially unchanged over 125 Myr,
    /// so this is the Julian year: 365.25 × 24.
    /// </summary>
    public const double HoursPerYear = 8766.0;

    /// <summary>
    /// Days in a year: 8766 ÷ 23.4 ≈ <b>374.615</b>.
    /// <para>
    /// The player "knows" a year is 365 days. It is not. A calendar built on 365
    /// drifts nearly ten days a year, misjudges the solstice, and is a way to die.
    /// This is the purest expression of the Transplant pillar (docs/00 §1) and
    /// nothing in the game may ever hint at it.
    /// </para>
    /// </summary>
    public const double DaysPerYear = HoursPerYear / HoursPerDay;

    /// <summary>Seconds in a year. Exactly one real year, which is the point.</summary>
    public const double SecondsPerYear = HoursPerYear * 3600.0;

    /// <summary>
    /// Synodic (new moon to new moon) month, in hours.
    /// <para>
    /// The Moon was ~0.3% closer at 125 Ma. Orbital period scales as a^1.5, so the
    /// sidereal month shortens from 655.72 h to 655.72 × 0.997^1.5 ≈ 652.77 h.
    /// Converting sidereal to synodic against an unchanged 8766 h year:
    /// 1/T_syn = 1/652.77 − 1/8766 gives T_syn ≈ 705.3 h.
    /// </para>
    /// </summary>
    public const double SynodicMonthHours = 705.29;

    /// <summary>
    /// Days in a lunar month: ≈ <b>30.14</b>. Longer in days than the modern 29.53
    /// even though it is shorter in hours, because the days themselves are shorter.
    /// This is the rhythm that decides which nights are navigable (docs/02 §6).
    /// </summary>
    public const double DaysPerLunarMonth = SynodicMonthHours / HoursPerDay;

    /// <summary>Seconds in a lunar month.</summary>
    public const double SecondsPerLunarMonth = SynodicMonthHours * 3600.0;

    /// <summary>
    /// Paleolatitude of the basin, degrees north. Roughly modern Beijing or Chicago
    /// (docs/01 §1). Drives day length, solar altitude, and therefore the whole
    /// seasonal temperature swing.
    /// </summary>
    public const double PaleolatitudeDegrees = 42.0;

    /// <summary>
    /// Axial tilt, degrees.
    /// <para>
    /// <b>Assumption, not evidence.</b> Cretaceous obliquity is not directly
    /// constrained by anything in docs/01, and long-term obliquity varies only a
    /// degree or two, so we use the modern value. Flagged here because everything
    /// seasonal hangs off it. That it is numerically near 23.4 is a coincidence and
    /// has nothing to do with <see cref="HoursPerDay"/>.
    /// </para>
    /// </summary>
    public const double AxialTiltDegrees = 23.44;

    /// <summary>
    /// How far the world's time-of-day slides against the real clock each real day,
    /// in real hours: 24 − 23.4 = 0.6 (36 minutes).
    /// </summary>
    public const double PrecessionDriftRealHoursPerDay = 24.0 - HoursPerDay;

    /// <summary>
    /// Real days for a fixed real-world play slot to travel the entire cycle of
    /// world time-of-day and return: 23.4 ÷ 0.6 = <b>39</b>.
    /// <para>
    /// So somebody who only ever plays 8pm to midnight is not locked in darkness
    /// forever. Their slice slides through dusk, midnight, dawn and noon and back
    /// in about 39 real days. The worst failure mode of a real-time clock, solved
    /// by an accurate detail at no design cost.
    /// </para>
    /// </summary>
    public const double PrecessionCycleRealDays = HoursPerDay / PrecessionDriftRealHoursPerDay;
}
