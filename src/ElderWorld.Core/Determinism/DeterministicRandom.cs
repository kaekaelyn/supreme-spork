namespace ElderWorld.Core.Determinism;

/// <summary>
/// A small, fast, seeded PRNG with explicit state, for the places that genuinely
/// need a stream of draws rather than a function of time — which fuel piece caught,
/// where a spark landed.
/// <para>
/// Deliberately <b>not</b> <see cref="System.Random"/>: its algorithm is not
/// guaranteed stable across .NET versions, and docs/06 §9 requires that the same
/// seed reproduce the same world so a bug report is reproducible. This is xorshift128+,
/// pinned here and therefore stable forever.
/// </para>
/// <para>
/// Anything that can be expressed as a pure function of world time should use
/// <see cref="DeterministicNoise"/> instead — it needs no serialisation and cannot
/// desynchronise.
/// </para>
/// </summary>
public sealed class DeterministicRandom
{
    private ulong _s0;
    private ulong _s1;

    /// <summary>Creates a generator from a seed. Equal seeds give equal sequences.</summary>
    public DeterministicRandom(ulong seed)
    {
        // SplitMix64 the seed so that even seed 0 or 1 starts well mixed.
        _s0 = SplitMix64(ref seed);
        _s1 = SplitMix64(ref seed);
        if (_s0 == 0 && _s1 == 0) _s1 = 0x9E3779B97F4A7C15UL; // xorshift cannot escape all-zero
    }

    private DeterministicRandom(ulong s0, ulong s1)
    {
        _s0 = s0;
        _s1 = s1;
    }

    private static ulong SplitMix64(ref ulong state)
    {
        unchecked
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>Next raw 64-bit value.</summary>
    public ulong NextUInt64()
    {
        unchecked
        {
            ulong s1 = _s0;
            ulong s0 = _s1;
            ulong result = s0 + s1;
            _s0 = s0;
            s1 ^= s1 << 23;
            _s1 = s1 ^ s0 ^ (s1 >> 18) ^ (s0 >> 5);
            return result;
        }
    }

    /// <summary>Uniform double in [0, 1).</summary>
    public double NextDouble() => (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);

    /// <summary>Uniform double in [min, max).</summary>
    public double NextDouble(double min, double max) => min + NextDouble() * (max - min);

    /// <summary>True with the given probability.</summary>
    public bool Chance(double probability) => NextDouble() < probability;

    /// <summary>Uniform integer in [min, max).</summary>
    public int NextInt(int min, int max)
    {
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "Range must be non-empty.");
        return min + (int)(NextDouble() * (max - min));
    }

    /// <summary>Captures the generator's state so a world can be saved mid-stream.</summary>
    public (ulong S0, ulong S1) Capture() => (_s0, _s1);

    /// <summary>Rebuilds a generator from captured state.</summary>
    public static DeterministicRandom Restore(ulong s0, ulong s1) => new(s0, s1);
}
