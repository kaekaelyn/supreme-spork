namespace ElderWorld.Core.Time;

/// <summary>
/// Turns a variable slab of world time into a whole number of fixed-size steps,
/// carrying the remainder.
/// <para>
/// This is the mechanism behind the hard requirement in docs/07 §2: <i>"Everything
/// must be correct at any rate — which is a genuine constraint on how systems are
/// written, and much cheaper to enforce from the start than to retrofit."</i>
/// </para>
/// <para>
/// Integrating a model directly against frame delta makes its results depend on
/// frame rate and on the debug time scale, so a bug found at 1000× would not
/// reproduce at 1× and a tuning pass at 1× would be wrong at 1000×. Fixed steps
/// plus a carried remainder make the result a function of elapsed world time only.
/// Any model that consumes time must go through one of these.
/// </para>
/// </summary>
public sealed class FixedStepAccumulator
{
    private double _pending;

    /// <summary>
    /// Steps per pump are capped so that a huge debug slab, or a machine that
    /// stalled, cannot lock the process in a single update. Time beyond the cap is
    /// discarded rather than queued, which keeps the sim responsive at the cost of
    /// running slower than requested — the honest tradeoff, and it is reported
    /// through <see cref="LastPumpWasClamped"/>.
    /// </summary>
    public const int MaxStepsPerPump = 20_000;

    /// <summary>Creates an accumulator with a fixed step size in world seconds.</summary>
    public FixedStepAccumulator(double stepSeconds)
    {
        if (double.IsNaN(stepSeconds) || stepSeconds <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(stepSeconds), "Step size must be positive.");
        StepSeconds = stepSeconds;
    }

    /// <summary>The fixed step size, in world seconds.</summary>
    public double StepSeconds { get; }

    /// <summary>World time not yet consumed by a whole step.</summary>
    public double Pending => _pending;

    /// <summary>True if the most recent pump hit <see cref="MaxStepsPerPump"/> and dropped time.</summary>
    public bool LastPumpWasClamped { get; private set; }

    /// <summary>
    /// Adds world time and returns how many fixed steps are now due. Call the model
    /// once per step with <see cref="StepSeconds"/>.
    /// </summary>
    public int Pump(double worldSeconds)
    {
        if (worldSeconds < 0.0)
            throw new ArgumentOutOfRangeException(nameof(worldSeconds), "Time does not run backwards.");

        _pending += worldSeconds;
        long steps = (long)(_pending / StepSeconds);

        LastPumpWasClamped = steps > MaxStepsPerPump;
        if (LastPumpWasClamped)
        {
            _pending = 0.0;
            return MaxStepsPerPump;
        }

        _pending -= steps * StepSeconds;
        return (int)steps;
    }

    /// <summary>Clears the carried remainder. Used when a world is loaded.</summary>
    public void Reset() => _pending = 0.0;
}
