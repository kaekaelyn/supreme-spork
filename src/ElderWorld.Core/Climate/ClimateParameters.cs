namespace ElderWorld.Core.Climate;

/// <summary>
/// The tunable constants of the Yixian climate, in one place, with their derivation.
/// Everything here traces to docs/01 §2, which is the paleontological bible and the
/// document every other one defers to.
/// </summary>
public sealed record ClimateParameters
{
    /// <summary>
    /// Mean annual air temperature at the basin floor, °C.
    /// <para>
    /// Two independent studies bracket this: clumped-isotope work on Sihetun
    /// paleosol carbonates gives 5.9 ± 1.7 °C, and oxygen-isotope work on East Asian
    /// dinosaur remains gives 10 ± 4 °C. docs/01 §2 rules on ~7 °C, inside both
    /// envelopes.
    /// </para>
    /// </summary>
    /// <para>
    /// Set slightly above the 7 °C target because the realised mean comes out below
    /// the seasonal curve's own mean: nights are long and their cooling limb is a
    /// decaying exponential, so a year spends more time near its minima than near
    /// its maxima. A test measures the realised annual mean rather than trusting
    /// this number.
    /// </para>
    public double AnnualMeanC { get; init; } = 7.6;

    /// <summary>
    /// Half the annual swing in <i>daily mean</i> temperature, °C. Gives a midwinter
    /// daily mean near −4 °C and a midsummer one near +19 °C.
    /// </summary>
    public double SeasonalAmplitudeC { get; init; } = 13.0;

    /// <summary>
    /// How far the coldest part of the year lags the winter solstice, as a fraction
    /// of the year. About 20 days.
    /// <para>
    /// Real, and quietly important: it means the coldest day is <b>not</b> the
    /// solstice. A player deriving a calendar by feel rather than by watching the
    /// sun against the ridge will place the solstice three weeks late and be wrong
    /// about when to expect spring.
    /// </para>
    /// </summary>
    public double SeasonalLagFraction { get; init; } = 0.055;

    /// <summary>
    /// Half the clear-sky diurnal range in high summer, °C.
    /// </summary>
    public double SummerDiurnalHalfRangeC { get; init; } = 5.5;

    /// <summary>
    /// Extra diurnal half-range in deep winter, °C, on top of the summer figure.
    /// <para>
    /// Winter nights swing further than summer ones here, which is the opposite of
    /// the naive expectation and is genuinely how a high continental basin behaves:
    /// snow cover plus long clear nights plus calm air build strong surface
    /// inversions, while summer maxima are capped by evaporation and convection.
    /// <b>It is also what reconciles docs/01 §2's three numbers</b> — a mean of 7 °C
    /// with extremes of −20 °C and +24 °C is impossible for any symmetric model,
    /// since the midpoint of those extremes is +2 °C. A cold-skewed distribution
    /// produced by winter inversions gives all three at once.
    /// </para>
    /// </summary>
    public double WinterDiurnalBoostC { get; init; } = 7.0;

    /// <summary>
    /// Amplitude of the slow synoptic term in deep winter, °C — the passing weather
    /// systems that give cold snaps and thaws. This is what carries a clear midwinter
    /// night down to the −20 °C the design calls for.
    /// </summary>
    public double SynopticAmplitudeC { get; init; } = 7.0;

    /// <summary>
    /// What fraction of <see cref="SynopticAmplitudeC"/> survives into high summer.
    /// <para>
    /// Midlatitude synoptic variability is markedly weaker in summer — the jet is
    /// further poleward and there are no cold-air outbreaks to import. Without this
    /// asymmetry, any winter deep enough to reach −20 °C drags summer up past 30 °C.
    /// </para>
    /// </summary>
    public double SynopticSummerFraction { get; init; } = 0.40;

    /// <summary>Period of the synoptic term, in world days.</summary>
    public double SynopticPeriodDays { get; init; } = 4.0;

    /// <summary>Reference elevation for <see cref="AnnualMeanC"/>: the basin floor.</summary>
    public double ReferenceElevationMetres { get; init; } = 2000.0;

    /// <summary>
    /// Environmental lapse rate, °C per km of ascent. The standard 6.5 °C/km, which
    /// makes the 3,000 m ridges 6.5 °C colder than the basin floor before wind.
    /// </summary>
    public double LapseRateCPerKm { get; init; } = 6.5;

    /// <summary>Mean free-stream wind speed, m/s, before terrain exposure.</summary>
    public double MeanWindSpeedMetresPerSecond { get; init; } = 3.2;

    /// <summary>Above this air temperature precipitation falls as rain, °C.</summary>
    public double RainSnowThresholdC { get; init; } = 1.0;

    /// <summary>The settled defaults of docs/01 §2.</summary>
    public static ClimateParameters Yixian { get; } = new();
}
