namespace ElderWorld.Core.Determinism;

/// <summary>
/// Seeded, stateless value noise. Every result is a pure function of (seed, position),
/// so it is bit-identical across runs, machines, save/load cycles, and debug time
/// scales.
/// <para>
/// Weather is built out of this rather than out of a random walk, and that choice is
/// load-bearing in two ways. A random walk would have to be stepped, so it would
/// drift apart between a 1× client and a 1000× soak test (docs/07 §2), and it would
/// have to be serialised, so weather would become part of the save format that
/// docs/06 §3 requires us never to break. A pure function of world time has neither
/// problem: there is nothing to step and nothing to store.
/// </para>
/// </summary>
public static class DeterministicNoise
{
    /// <summary>
    /// Integer hash — the finalisation mix from MurmurHash3. Cheap, and it
    /// avalanches well enough that adjacent seeds give unrelated sequences.
    /// </summary>
    public static uint Hash(uint value)
    {
        value ^= value >> 16;
        value *= 0x85EBCA6B;
        value ^= value >> 13;
        value *= 0xC2B2AE35;
        value ^= value >> 16;
        return value;
    }

    /// <summary>Hashes a lattice point and a seed to a uniform double in [0, 1).</summary>
    public static double HashTo01(uint seed, int x)
        => Hash(unchecked(Hash(seed) + (uint)x * 0x9E3779B9u)) / 4294967296.0;

    /// <summary>
    /// Smooth 1-D value noise in [0, 1), C¹-continuous across lattice points.
    /// </summary>
    public static double Value01(uint seed, double x)
    {
        double floor = Math.Floor(x);
        int lattice = (int)floor;
        double t = x - floor;

        double a = HashTo01(seed, lattice);
        double b = HashTo01(seed, lattice + 1);

        // Smoothstep, so the derivative is continuous and the weather has no kinks.
        double smooth = t * t * (3.0 - 2.0 * t);
        return a + (b - a) * smooth;
    }

    /// <summary>Smooth 1-D value noise in [-1, 1).</summary>
    public static double Signed(uint seed, double x) => Value01(seed, x) * 2.0 - 1.0;

    /// <summary>
    /// Summed octaves of <see cref="Signed"/>, normalised to roughly [-1, 1].
    /// Gives weather structure at several timescales at once — a slow synoptic
    /// pattern with faster gusts riding on it.
    /// </summary>
    public static double Fractal(uint seed, double x, int octaves = 3, double persistence = 0.5)
    {
        if (octaves < 1) throw new ArgumentOutOfRangeException(nameof(octaves), "Need at least one octave.");

        double sum = 0.0;
        double amplitude = 1.0;
        double total = 0.0;
        double frequency = 1.0;

        for (int i = 0; i < octaves; i++)
        {
            sum += Signed(unchecked(seed + (uint)i * 0x7F4A7C15u), x * frequency) * amplitude;
            total += amplitude;
            amplitude *= persistence;
            frequency *= 2.0;
        }

        return sum / total;
    }
}
