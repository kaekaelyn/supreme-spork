namespace ElderWorld.Core.Time;

/// <summary>
/// The authoritative world clock. Monotonic, never skips, never rewinds
/// (docs/06 §3: "The clock is authoritative and monotonic").
/// <para>
/// The world epoch (t = 0) is the <b>winter solstice of year 0</b>. A server picks
/// its founding date by starting the clock at a non-zero offset — start in spring
/// if you want a chance, or in November if you want the other thing (docs/02 §6).
/// </para>
/// </summary>
public sealed class WorldClock
{
    private double _totalSeconds;
    private double _timeScale = 1.0;

    /// <summary>
    /// The largest time scale a dev build may request. docs/07 §2 asks for "up to
    /// several thousand ×" so a year is reachable in development.
    /// </summary>
    public const double MaxDebugTimeScale = 10_000.0;

    /// <summary>
    /// Creates a clock, optionally founded partway through a year.
    /// </summary>
    /// <param name="foundingSeconds">
    /// World time at founding, seconds since winter solstice. Use
    /// <see cref="AtSeason"/> to express this readably.
    /// </param>
    public WorldClock(double foundingSeconds = 0.0)
    {
        if (double.IsNaN(foundingSeconds) || double.IsInfinity(foundingSeconds))
            throw new ArgumentOutOfRangeException(nameof(foundingSeconds), "Founding time must be finite.");
        if (foundingSeconds < 0.0)
            throw new ArgumentOutOfRangeException(nameof(foundingSeconds), "Founding time cannot precede the world epoch.");

        _totalSeconds = foundingSeconds;
    }

    /// <summary>
    /// A founding time expressed as a fraction of the year after the winter
    /// solstice: 0.0 = midwinter, 0.25 = spring, 0.5 = midsummer, 0.75 = autumn.
    /// </summary>
    /// <param name="yearPhase">Fraction of the year since the winter solstice, 0–1.</param>
    /// <param name="timeOfDay">Fraction of the day, solar noon = 0.5.</param>
    /// <remarks>
    /// The season is rounded down to a whole day before the time of day is added.
    /// A year is 374.615 days, so a raw fraction of it lands part-way through a day
    /// and would silently drag the requested time of day with it — asking for noon in
    /// midsummer would quietly give you early evening. The cost is that the season is
    /// accurate to the nearest day, which is far finer than any distinction the
    /// design draws.
    /// </remarks>
    public static double AtSeason(double yearPhase, double timeOfDay = 0.25)
    {
        double wholeDays = Math.Floor(yearPhase * CretaceousCalendar.DaysPerYear);
        return (wholeDays + timeOfDay) * CretaceousCalendar.SecondsPerDay;
    }

    /// <summary>Seconds elapsed since the world epoch. The single source of truth.</summary>
    public double TotalSeconds => _totalSeconds;

    /// <summary>
    /// Development-only time multiplier. <b>Must be 1.0 in any shipping build</b>
    /// (docs/07 §2). The engine layer is responsible for never exposing this
    /// outside a dev build; the core allows it so headless tests can drive years
    /// of world time in seconds.
    /// <para>
    /// Nothing in the simulation may read this. Every model integrates in fixed
    /// world-time steps precisely so that results are identical at any scale — see
    /// <see cref="FixedStepAccumulator"/>.
    /// </para>
    /// </summary>
    public double TimeScale
    {
        get => _timeScale;
        set
        {
            if (double.IsNaN(value) || value <= 0.0 || value > MaxDebugTimeScale)
                throw new ArgumentOutOfRangeException(
                    nameof(value), $"Time scale must be in (0, {MaxDebugTimeScale}].");
            _timeScale = value;
        }
    }

    /// <summary>
    /// Advances the clock by a real-time interval, scaled by <see cref="TimeScale"/>.
    /// Returns the amount of <i>world</i> time that passed, which is what every
    /// simulation model should be stepped with.
    /// </summary>
    public double Advance(double realSeconds)
    {
        if (realSeconds < 0.0)
            throw new ArgumentOutOfRangeException(nameof(realSeconds), "The clock does not run backwards.");

        double worldSeconds = realSeconds * _timeScale;
        _totalSeconds += worldSeconds;
        return worldSeconds;
    }

    /// <summary>Whole days elapsed since the world epoch.</summary>
    public long Day => (long)(_totalSeconds / CretaceousCalendar.SecondsPerDay);

    /// <summary>Whole years elapsed since the world epoch.</summary>
    public long Year => (long)(_totalSeconds / CretaceousCalendar.SecondsPerYear);

    /// <summary>
    /// Position within the day, 0.0–1.0. Solar noon is 0.5, so 0.0 is solar
    /// midnight. Not clock time — the player has no clock.
    /// </summary>
    public double TimeOfDay => Wrap01(_totalSeconds / CretaceousCalendar.SecondsPerDay);

    /// <summary>
    /// Position within the year, 0.0–1.0, measured from the winter solstice.
    /// 0.25 is the spring equinox, 0.5 the summer solstice, 0.75 the autumn equinox.
    /// </summary>
    public double YearPhase => Wrap01(_totalSeconds / CretaceousCalendar.SecondsPerYear);

    /// <summary>
    /// Position within the lunar month, 0.0–1.0, where 0.0 is new moon and 0.5 is
    /// full. The world epoch is defined to begin at new moon.
    /// </summary>
    public double LunarPhase => Wrap01(_totalSeconds / CretaceousCalendar.SecondsPerLunarMonth);

    /// <summary>Restores a clock from a saved world. Used only by persistence.</summary>
    public static WorldClock Restore(double totalSeconds) => new(totalSeconds);

    private static double Wrap01(double value)
    {
        double fraction = value - Math.Floor(value);
        // Guard the case where a very large value rounds to exactly 1.0.
        return fraction >= 1.0 ? 0.0 : fraction;
    }
}
