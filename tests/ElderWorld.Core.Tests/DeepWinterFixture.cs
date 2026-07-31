using ElderWorld.Core.Climate;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// Finds a genuinely cold winter night for a given climate, rather than trusting a
/// single fixed year-phase to be cold.
/// <para>
/// The smooth seasonal minimum sits at a fixed phase (docs/01 §2's implementation
/// note puts it about 20 days after the solstice), but <see cref="ClimateParameters.SynopticAmplitudeC"/>
/// is large enough relative to how flat the seasonal curve is near its minimum that
/// the coldest <i>instant</i> of a given year can land anywhere across several weeks
/// either side of the solstice, seed-dependent — for seed 2024 it lands two days
/// <b>before</b> the solstice, not after it. A single fixed phase (this fixture
/// replaced an earlier one hardcoded to 0.03, eleven days after the solstice) will
/// sometimes land on a comparatively mild night purely by chance, which understates
/// how cold a real winter night in this world gets and understates what a survival
/// claim like "down and hide should not be lethal" is actually being tested against.
/// </para>
/// <para>
/// Searching a window and taking the coldest sample is the only way to test what a
/// winter night actually does, deterministically and reproducibly per seed.
/// </para>
/// </summary>
internal static class DeepWinterFixture
{
    private const double WindowDays = 25.0;

    /// <summary>
    /// World seconds of the coldest sample within <see cref="WindowDays"/> either
    /// side of a winter solstice, at open ground.
    /// </summary>
    /// <param name="solsticeYear">
    /// Which of the climate's (endless, identical) yearly solstices to search around.
    /// Any value gives an equally valid winter; a fixed default keeps every caller's
    /// result reproducible without them having to agree on one explicitly.
    /// </param>
    public static double ColdestNightSeconds(ClimateModel climate, int solsticeYear = 5)
    {
        double solstice = solsticeYear * CretaceousCalendar.SecondsPerYear;
        double window = WindowDays * CretaceousCalendar.SecondsPerDay;

        double coldest = double.MaxValue;
        double coldestSeconds = solstice;

        for (double t = solstice - window; t <= solstice + window; t += 600.0)
        {
            double temperature = climate.Sample(t, SiteContext.OpenGround).AirTemperatureC;
            if (temperature < coldest)
            {
                coldest = temperature;
                coldestSeconds = t;
            }
        }

        return coldestSeconds;
    }
}
