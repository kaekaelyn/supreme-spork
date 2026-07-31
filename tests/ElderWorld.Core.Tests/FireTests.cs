using ElderWorld.Core.Climate;
using ElderWorld.Core.Fire;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// docs/02 §4. Fire is the hub of the whole tech graph, and the behaviours asserted
/// here are the ones the player is expected to discover without ever being told.
/// </summary>
public class FireTests
{
    // Uses the genuinely coldest instant near a solstice, not a fixed phase — see
    // DeepWinterFixture. A fire that "carries the whole night" has to be tested
    // against the worst a night actually gets, not an arbitrarily mild sample of one.
    private static EnvironmentSample WinterNight(SiteContext? site = null)
    {
        var climate = new ClimateModel(2024);
        return climate.Sample(DeepWinterFixture.ColdestNightSeconds(climate), site ?? SiteContext.ForestUnderstory);
    }

    private static Hearth OpenCampfire()
    {
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.8);
        hearth.SetBedTemperature(600.0);
        for (int i = 0; i < 6; i++)
            hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.15, 0.04));
        return hearth;
    }

    [Fact]
    public void AGoodFireReachesEstablishedAndThrowsUsefulHeat()
    {
        Hearth hearth = OpenCampfire();
        hearth.Advance(WinterNight(), 1800.0);

        Assert.Equal(FireState.Established, hearth.State);
        Assert.InRange(hearth.OutputWatts, 5_000.0, 30_000.0);

        // A campfire at 1.5 m should be a real heat source but not a furnace.
        Assert.InRange(hearth.IrradianceAt(1.5), 150.0, 800.0);
    }

    [Fact]
    public void SixKilosOfConiferBurnsForAFewHoursNotMinutes()
    {
        Hearth hearth = OpenCampfire();
        EnvironmentSample night = WinterNight();

        double hours = 0.0;
        while (hearth.RemainingDryMassKg > 0.05 && hours < 12.0)
        {
            hearth.Advance(night, 600.0);
            hours += 600.0 / 3600.0;
        }

        Assert.InRange(hours, 1.5, 5.0);
    }

    /// <summary>
    /// Builds a fire of a single fuel thickness. Same mass, species and dryness every
    /// time — only the thickness changes.
    /// </summary>
    private static Hearth HearthOfThickness(double thicknessMetres, double bedTemperatureC)
    {
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.8);
        hearth.SetBedTemperature(bedTemperatureC);
        hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 2.0, 0.15, thicknessMetres));
        return hearth;
    }

    [Fact]
    public void OnlyThinFuelWillCatchFromAWeakBed()
    {
        // Twigs present an order of magnitude more surface per kilo than a log does,
        // so a marginal bed can light them and cannot light it. This is why you build
        // a fire small and feed it upward, and the game never says so.
        Hearth twigs = HearthOfThickness(0.005, bedTemperatureC: 320.0);
        Hearth logs = HearthOfThickness(0.15, bedTemperatureC: 320.0);

        EnvironmentSample night = WinterNight();
        twigs.Advance(night, 120.0);
        logs.Advance(night, 120.0);

        Assert.True(twigs.OutputWatts > logs.OutputWatts * 4.0,
            $"Twigs ({twigs.OutputWatts / 1000:F1} kW) should far outburn logs " +
            $"({logs.OutputWatts / 1000:F1} kW) on a cool bed.");
    }

    [Fact]
    public void ThickFuelLastsFarLongerThanThinFuel()
    {
        // The other half of the same fact, and the reason you switch to heavy wood
        // before dark rather than after.
        static double BurnHours(double thicknessMetres)
        {
            Hearth hearth = HearthOfThickness(thicknessMetres, bedTemperatureC: 650.0);
            EnvironmentSample night = WinterNight();

            double hours = 0.0;
            while (hearth.RemainingDryMassKg > 0.05 && hours < 24.0)
            {
                hearth.Advance(night, 300.0);
                hours += 300.0 / 3600.0;
            }
            return hours;
        }

        Assert.True(BurnHours(0.15) > BurnHours(0.005) * 2.0,
            "A log should outlast the same mass of twigs several times over.");
    }

    [Fact]
    public void PilingOnFuelGivesDiminishingReturns()
    {
        // Real fires are ventilation-limited rather than fuel-limited. A heap of
        // kindling exposes something like fifty times the surface of a small handful,
        // but it cannot burn what it cannot get air to — so output tracks the fire's
        // footprint, not its fuel load, and the extra fuel simply lasts longer.
        static (double Output, double Mass) Burn(int pieces)
        {
            var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.8);
            hearth.SetBedTemperature(700.0);
            for (int i = 0; i < pieces; i++)
                hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 0.5, 0.15, 0.008));

            double mass = hearth.RemainingDryMassKg;
            hearth.Advance(WinterNight(), 60.0);
            return (hearth.OutputWatts, mass);
        }

        (double smallOutput, double smallMass) = Burn(2);
        (double hugeOutput, double hugeMass) = Burn(20);

        double massRatio = hugeMass / smallMass;
        double outputRatio = hugeOutput / smallOutput;

        Assert.True(outputRatio < massRatio * 0.75,
            $"Ten times the fuel gave {outputRatio:F1}× the output; ventilation should hold it well " +
            "below proportional.");

        // Watts per kilo on the fire must fall, not stay flat.
        Assert.True(hugeOutput / hugeMass < smallOutput / smallMass * 0.75,
            "Specific output should drop as the pile grows.");
    }

    [Fact]
    public void WetFuelWillNotCatchFromAWeakBed()
    {
        // The rain problem, and the reason a dry tinder store is worth a shelter.
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.7);
        hearth.SetBedTemperature(320.0);
        for (int i = 0; i < 4; i++)
            hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.85, 0.04));

        hearth.Advance(WinterNight(), 1800.0);

        Assert.True(hearth.RemainingDryMassKg > 3.5,
            $"Soaked wood should barely burn; {4.0 - hearth.RemainingDryMassKg:F2} kg went.");
        Assert.NotEqual(FireState.Established, hearth.State);
    }

    [Fact]
    public void DryFuelCatchesFromTheSameBedThatWetFuelRefuses()
    {
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.7);
        hearth.SetBedTemperature(320.0);
        for (int i = 0; i < 4; i++)
            hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.12, 0.04));

        hearth.Advance(WinterNight(), 1800.0);

        Assert.True(hearth.BedTemperatureC > 500.0,
            $"Seasoned wood should build the bed up; it reached {hearth.BedTemperatureC:F0} °C.");
    }

    [Fact]
    public void RainOnAnUnshelteredFireStealsRealHeat()
    {
        EnvironmentSample dry = WinterNight() with { AirTemperatureC = 4.0 };
        EnvironmentSample wet = dry with
        {
            Precipitation = PrecipitationKind.Rain,
            PrecipitationMmPerHour = 12.0,
        };

        static double BedAfter(EnvironmentSample environment, double rainShelter)
        {
            var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.7, rainShelter: rainShelter);
            hearth.SetBedTemperature(620.0);
            for (int i = 0; i < 3; i++)
                hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.2, 0.04));
            hearth.Advance(environment, 1200.0);
            return hearth.BedTemperatureC;
        }

        double sheltered = BedAfter(wet, rainShelter: 0.95);
        double exposed = BedAfter(wet, rainShelter: 0.0);

        Assert.True(sheltered > exposed + 20.0,
            $"A roof over the fire should matter: sheltered {sheltered:F0} °C vs exposed {exposed:F0} °C.");
    }

    [Fact]
    public void ABankedStoneHearthCarriesTheWholeNight()
    {
        // docs/02 §6 makes the long night the productive part of the day, which only
        // works if a fire can be made to last one. Banking — heaping the fire down so
        // it barely breathes — is the technique, and it emerges from one number.
        var hearth = new Hearth(thermalMassJPerK: 40_000.0, airflow: 0.12);
        hearth.SetBedTemperature(700.0);
        for (int i = 0; i < 8; i++)
            hearth.AddFuel(new FuelPiece(FuelSpecies.HardWood, 1.0, 0.18, 0.10));

        var climate = new ClimateModel(2024);
        double coldestSeconds = DeepWinterFixture.ColdestNightSeconds(climate);
        double nightHours = SolarPosition.NightLengthHours(WorldClock.Restore(coldestSeconds).YearPhase);
        hearth.Advance(climate.Sample(coldestSeconds, SiteContext.ForestUnderstory), nightHours * 3600.0);

        Assert.True(hearth.IsAlive,
            $"A banked fire should still be alive after {nightHours:F1} h; bed was {hearth.BedTemperatureC:F0} °C.");
        Assert.True(hearth.BedTemperatureC > 200.0);
    }

    [Fact]
    public void AnOpenLayBurnsOutFasterThanABankedOne()
    {
        static double RemainingAfterNight(double airflow)
        {
            var hearth = new Hearth(thermalMassJPerK: 40_000.0, airflow: airflow);
            hearth.SetBedTemperature(700.0);
            for (int i = 0; i < 8; i++)
                hearth.AddFuel(new FuelPiece(FuelSpecies.HardWood, 1.0, 0.18, 0.10));
            hearth.Advance(WinterNight(), 8.0 * 3600.0);
            return hearth.RemainingDryMassKg;
        }

        Assert.True(RemainingAfterNight(0.12) > RemainingAfterNight(0.9),
            "Closing a fire down should make its fuel last longer.");
    }

    [Fact]
    public void AFireWithNoFuelLeftBecomesEmbersAndThenDies()
    {
        var hearth = new Hearth(thermalMassJPerK: 4000.0, airflow: 0.8);
        hearth.SetBedTemperature(700.0);

        EnvironmentSample night = WinterNight();
        hearth.Advance(night, 60.0);
        Assert.Equal(FireState.Embers, hearth.State);

        hearth.Advance(night, 6.0 * 3600.0);
        Assert.Equal(FireState.Dead, hearth.State);
    }

    [Fact]
    public void StoneThermalMassHoldsEmbersLongerThanABareScrape()
    {
        // The physical argument for a stone-lined hearth being worth the work.
        static double BedAfter(double thermalMass)
        {
            var hearth = new Hearth(thermalMass, airflow: 0.3);
            hearth.SetBedTemperature(700.0);
            hearth.Advance(WinterNight(), 2.0 * 3600.0);
            return hearth.BedTemperatureC;
        }

        Assert.True(BedAfter(40_000.0) > BedAfter(4_000.0) + 50.0,
            "Stone should still be warm when a scrape in the dirt has gone cold.");
    }

    [Fact]
    public void CoalOutlastsWoodOnceItIsLit()
    {
        // Attested: the Jianshangou unit carries coal seams (docs/03 §2). Hard to
        // light, and then it does not stop.
        Assert.True(FuelSpecies.Coal.IgnitionTemperatureC > FuelSpecies.ConiferDeadwood.IgnitionTemperatureC);
        Assert.True(FuelSpecies.Coal.CalorificValueJPerKg > FuelSpecies.ConiferDeadwood.CalorificValueJPerKg);
        Assert.True(FuelSpecies.Coal.DensityKgPerM3 > FuelSpecies.HardWood.DensityKgPerM3);
    }

    [Fact]
    public void ResinousConiferLightsFromACoolerBedThanHardwood()
    {
        var conifer = new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.15, 0.03);
        var hardwood = new FuelPiece(FuelSpecies.HardWood, 1.0, 0.15, 0.03);

        Assert.True(conifer.EffectiveIgnitionTemperatureC < hardwood.EffectiveIgnitionTemperatureC);
    }

    [Fact]
    public void WetFuelNeedsAMuchHotterBed()
    {
        var dry = new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.12, 0.04);
        var soaked = new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.9, 0.04);

        Assert.True(soaked.EffectiveIgnitionTemperatureC > dry.EffectiveIgnitionTemperatureC * 1.5);
    }

    [Fact]
    public void FuelStackedByAHotFireDriesOut()
    {
        // Drying tomorrow's wood at tonight's fire. Real, undocumented, and the sort
        // of thing a player writes in a journal the first time they notice it.
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.7);
        hearth.SetBedTemperature(700.0);

        var damp = new FuelPiece(FuelSpecies.ConiferDeadwood, 3.0, 0.45, 0.12);
        hearth.AddFuel(damp);
        double before = damp.MoistureFraction;

        hearth.Advance(WinterNight(), 3600.0);

        Assert.True(damp.MoistureFraction < before, "Fuel by a hot fire should lose water.");
    }

    [Fact]
    public void SmokeRisesWithWetAndSmoulderingFuel()
    {
        static double SmokeFrom(double moisture)
        {
            var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.7);
            hearth.SetBedTemperature(600.0);
            for (int i = 0; i < 3; i++)
                hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, moisture, 0.04));
            hearth.Advance(WinterNight(), 300.0);
            return hearth.SmokeRate / Math.Max(1.0, hearth.OutputWatts / 1000.0);
        }

        Assert.True(SmokeFrom(0.6) > SmokeFrom(0.12),
            "Wet fuel should smoke harder per kilowatt than seasoned fuel.");
    }

    [Fact]
    public void IrradianceFallsOffWithDistance()
    {
        Hearth hearth = OpenCampfire();
        hearth.Advance(WinterNight(), 600.0);

        double near = hearth.IrradianceAt(1.0);
        double far = hearth.IrradianceAt(4.0);

        // Hemispherical spreading: sixteen times the area at four times the range.
        Assert.Equal(16.0, near / far, 1);
    }

    [Fact]
    public void ACampfireIsAVeryPoorLamp()
    {
        // docs/02 §6 treats the long night as a design brief, not a gap. That only
        // holds if firelight is genuinely dim — a small pool of light and no more.
        Hearth hearth = OpenCampfire();
        hearth.Advance(WinterNight(), 600.0);

        Assert.True(hearth.IlluminanceAt(8.0) < 20.0,
            "Firelight should not usefully reach across a clearing.");
    }
}

/// <summary>
/// The fumarole ember-carry of docs/02 §4 — the first technology in the game, and the
/// one thing a naked player can do on day one.
/// </summary>
public class EmberTests
{
    // Uses the genuinely coldest instant near a solstice, not a fixed phase — see
    // DeepWinterFixture. A fire that "carries the whole night" has to be tested
    // against the worst a night actually gets, not an arbitrarily mild sample of one.
    private static EnvironmentSample WinterNight(SiteContext? site = null)
    {
        var climate = new ClimateModel(2024);
        return climate.Sample(DeepWinterFixture.ColdestNightSeconds(climate), site ?? SiteContext.ForestUnderstory);
    }

    private static double LifetimeHours(Ember ember, EnvironmentSample environment, bool sheltered)
    {
        double seconds = 0.0;
        while (ember.IsAlive && seconds < 24.0 * 3600.0)
        {
            ember.Step(environment, sheltered);
            seconds += Ember.StepSeconds;
        }
        return seconds / 3600.0;
    }

    [Fact]
    public void AWellWrappedEmberSurvivesLongEnoughToWalkHome()
    {
        var ember = Ember.FromVolcanicVent();
        ember.Insulation = 0.9;
        ember.AirSupply = 0.03;

        double hours = LifetimeHours(ember, WinterNight(), sheltered: true);

        Assert.InRange(hours, 2.0, 8.0);
    }

    [Fact]
    public void AnExposedEmberDiesWithinTheHour()
    {
        var ember = Ember.FromVolcanicVent();
        ember.Insulation = 0.05;
        ember.AirSupply = 0.6;

        double hours = LifetimeHours(ember, WinterNight(SiteContext.OpenGround), sheltered: false);

        Assert.True(hours < 1.0, $"An ember carried in an open hand lasted {hours:F2} h.");
    }

    [Fact]
    public void SmotheringAnEmberAndOpeningItUpBothKillIt()
    {
        // The tension the whole mechanic lives in: it needs air to stay alight and
        // insulation to stay hot, and every real container trades one for the other.
        var smothered = new Ember();
        smothered.Insulation = 1.0;
        smothered.AirSupply = 0.0;

        var flared = new Ember();
        flared.Insulation = 0.0;
        flared.AirSupply = 1.0;

        var wellHandled = new Ember();
        wellHandled.Insulation = 0.9;
        wellHandled.AirSupply = 0.03;

        EnvironmentSample night = WinterNight();
        double smotheredHours = LifetimeHours(smothered, night, sheltered: true);
        double flaredHours = LifetimeHours(flared, night, sheltered: true);
        double goodHours = LifetimeHours(wellHandled, night, sheltered: true);

        Assert.True(goodHours > flaredHours, "Burning it open should waste it.");
        Assert.True(goodHours > smotheredHours, "Sealing it completely should choke it.");
    }

    [Fact]
    public void HeavyRainKillsACarriedEmberButLightRainDoesNot()
    {
        // docs/02 §4: fire-by-friction "fails in rain — which makes the ember-carrying
        // skill remain relevant forever." Carrying one through weather is a risk, not
        // an automatic loss.
        EnvironmentSample night = WinterNight(SiteContext.OpenGround) with { AirTemperatureC = 4.0 };

        EnvironmentSample drizzle = night with
        { Precipitation = PrecipitationKind.Rain, PrecipitationMmPerHour = 1.0 };
        EnvironmentSample downpour = night with
        { Precipitation = PrecipitationKind.Rain, PrecipitationMmPerHour = 15.0 };

        static Ember Wrapped()
        {
            var ember = Ember.FromVolcanicVent();
            ember.Insulation = 0.9;
            ember.AirSupply = 0.03;
            return ember;
        }

        double inDrizzle = LifetimeHours(Wrapped(), drizzle, sheltered: false);
        double inDownpour = LifetimeHours(Wrapped(), downpour, sheltered: false);

        Assert.True(inDrizzle > 1.0, $"A wrapped ember should survive drizzle; lasted {inDrizzle:F2} h.");
        Assert.True(inDownpour < 0.5, $"A downpour should kill it; lasted {inDownpour:F2} h.");
    }

    [Fact]
    public void AnEmberLaidIntoAHearthGivesUpItsHeat()
    {
        var hearth = new Hearth(thermalMassJPerK: 3000.0, airflow: 0.8);
        double before = hearth.BedTemperatureC;

        var ember = Ember.FromVolcanicVent();
        hearth.LayEmber(ember);

        Assert.True(hearth.BedTemperatureC > before, "The bed should take the ember's heat.");
        Assert.False(ember.IsAlive, "The ember is spent once it is laid in.");
    }

    [Fact]
    public void AnEmberAloneRarelyStartsAFireWithoutTinder()
    {
        // Which is why you carry tinder as well, and why tinder is worth a pocket.
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.8);
        hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.15, 0.04));
        hearth.LayEmber(Ember.FromVolcanicVent());

        Assert.True(hearth.BedTemperatureC < FuelSpecies.ConiferDeadwood.IgnitionTemperatureC,
            "An ember dropped onto sticks should not light them on its own.");
    }

    [Fact]
    public void TinderIsWhatAnEmberCanActuallyLight()
    {
        Assert.True(FuelSpecies.Tinder.IgnitionTemperatureC < Ember.TinderIgnitionTemperatureC);
        Assert.True(FuelSpecies.Tinder.IgnitionTemperatureC < FuelSpecies.ConiferDeadwood.IgnitionTemperatureC);
    }

    [Fact]
    public void AFreshVentEmberCanLightTinder()
    {
        var ember = Ember.FromVolcanicVent();
        ember.Insulation = 0.9;
        ember.AirSupply = 0.05;
        ember.Step(WinterNight(), sheltered: true);

        Assert.True(ember.CanIgniteTinder,
            $"A fresh vent ember was only {ember.TemperatureC:F0} °C.");
    }

    [Fact]
    public void AnEmberCanBeTakenFromAnEstablishedFireToFoundANewCamp()
    {
        var hearth = new Hearth(thermalMassJPerK: 6000.0, airflow: 0.8);
        hearth.SetBedTemperature(750.0);
        for (int i = 0; i < 3; i++)
            hearth.AddFuel(new FuelPiece(FuelSpecies.ConiferDeadwood, 1.0, 0.15, 0.04));
        hearth.Advance(WinterNight(), 300.0);

        Ember carried = Ember.FromHearth(hearth);
        Assert.True(carried.IsAlive);
        Assert.True(carried.CanIgniteTinder);
    }

    [Fact]
    public void ADeadFireYieldsNothingToCarry()
    {
        var cold = new Hearth();
        Assert.Equal(FireState.Dead, cold.State);
        Assert.False(Ember.FromHearth(cold).IsAlive);
    }
}
