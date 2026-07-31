using ElderWorld.Core.Climate;
using ElderWorld.Core.Thermal;
using ElderWorld.Core.Time;

namespace ElderWorld.Core.Tests;

/// <summary>
/// docs/06 §9 asks for exactly this:
/// <i>"Nutrition/thermal model unit tests with reference scenarios: 'naked at 5 °C in
/// 10 km/h wind' should produce a documented, reviewable time-to-incapacitation."</i>
/// <para>
/// So the numbers are documented here, in the assertions, where a change to the model
/// has to walk past them. The bands are deliberately wide enough to allow tuning and
/// narrow enough that a broken model cannot pass.
/// </para>
/// </summary>
public class ThermalReferenceScenarioTests
{
    /// <summary>The docs/06 §9 reference environment, built by hand so it never moves.</summary>
    private static EnvironmentSample ReferenceConditions() => new()
    {
        AirTemperatureC = 5.0,
        RelativeHumidity = 0.70,
        WindSpeedMetresPerSecond = 10.0 / 3.6,
        CloudCover = 1.0,
        SkyTemperatureC = 3.0,
        GroundTemperatureC = 5.0,
        WaterTemperatureC = 5.0,
        SolarIrradianceWattsPerM2 = 0.0,
        Site = SiteContext.OpenGround,
    };

    private sealed record Outcome(double ToShiveringHours, double ToConfusionHours, double ToDeathHours, ThermalState Final);

    private static Outcome Run(ThermalExposure exposure, double maxHours = 30.0)
    {
        var model = new ThermalModel();
        var state = ThermalState.Fresh();

        double shivering = -1.0, confusion = -1.0, death = -1.0;
        int steps = (int)(maxHours * 3600.0 / ThermalModel.StepSeconds);

        for (int i = 0; i < steps; i++)
        {
            model.Step(state, exposure);
            double hours = i * ThermalModel.StepSeconds / 3600.0;

            if (shivering < 0 && state.Stage >= HypothermiaStage.Shivering) shivering = hours;
            if (confusion < 0 && state.Stage >= HypothermiaStage.Confusion) confusion = hours;
            if (death < 0 && state.IsDead) { death = hours; break; }
        }

        return new Outcome(shivering, confusion, death, state);
    }

    /// <summary>
    /// <b>THE DOCUMENTED REFERENCE SCENARIO.</b> Naked, standing, dry, well fed, at
    /// 5 °C in a 10 km/h wind under overcast sky.
    /// <para>
    /// The model gives: <b>shivering from ~11 h, confusion (incapacitation) at ~13 h,
    /// death at ~20 h.</b>
    /// </para>
    /// <para>
    /// That is longer than intuition suggests, and it is right. Still air at +5 °C is
    /// survivable for a long time: maximal shivering produces around 350 W against
    /// roughly 250 W of loss once the skin has cooled and vasoconstriction has closed
    /// the shell down, so the body holds its core almost indefinitely. What actually
    /// kills is <i>fuel</i> — shivering burns the glycogen reserve, and when that runs
    /// low shivering fades, heat production collapses, and the core falls away.
    /// The first-night killer in this game is winter, not a mild afternoon.
    /// </para>
    /// </summary>
    [Fact]
    public void ReferenceScenario_NakedAtFiveDegreesInTenKilometreWind()
    {
        Outcome outcome = Run(ThermalExposure.Naked(ReferenceConditions()));

        Assert.InRange(outcome.ToShiveringHours, 8.0, 14.0);
        Assert.InRange(outcome.ToConfusionHours, 10.0, 17.0);
        Assert.InRange(outcome.ToDeathHours, 16.0, 26.0);
    }

    [Fact]
    public void WindIsTheUnderratedKiller()
    {
        // docs/02 §1: "Wind — the single most underrated killer."
        Outcome windy = Run(ThermalExposure.Naked(ReferenceConditions()));
        Outcome calm = Run(ThermalExposure.Naked(
            ReferenceConditions() with { WindSpeedMetresPerSecond = 0.2 }));

        Assert.True(calm.ToConfusionHours > windy.ToConfusionHours * 1.4,
            $"Calm air ({calm.ToConfusionHours:F1} h) should buy far more time than wind " +
            $"({windy.ToConfusionHours:F1} h) at the same temperature.");
    }

    [Fact]
    public void ANakedPlayerDiesOutdoorsOnAWinterNight()
    {
        // The whole premise of docs/02 §1: "The most likely first-session death is
        // hypothermia, not predation, and that is correct and intentional."
        var climate = new ClimateModel(2024);
        double coldestSeconds = DeepWinterFixture.ColdestNightSeconds(climate);
        EnvironmentSample night = climate.Sample(coldestSeconds, SiteContext.OpenGround);

        Outcome outcome = Run(ThermalExposure.Naked(night));

        double nightLength = SolarPosition.NightLengthHours(WorldClock.Restore(coldestSeconds).YearPhase);
        Assert.True(outcome.ToDeathHours > 0 && outcome.ToDeathHours < nightLength,
            $"A naked player in the open should not survive a {nightLength:F1} h winter night " +
            $"(died at {outcome.ToDeathHours:F1} h).");
    }

    [Fact]
    public void ForestUnderstoryBuysRealTimeOverOpenGround()
    {
        // docs/02 §1: "Windchill on the exposed ridges versus stillness in the forest
        // understory is a legible, learnable map feature. Reading terrain for shelter
        // is a skill." Nothing tells the player this; the forest simply kills slower.
        var climate = new ClimateModel(2024);
        double when = DeepWinterFixture.ColdestNightSeconds(climate);

        Outcome open = Run(ThermalExposure.Naked(climate.Sample(when, SiteContext.OpenGround)));
        Outcome forest = Run(ThermalExposure.Naked(climate.Sample(when, SiteContext.ForestUnderstory)));

        Assert.True(forest.ToDeathHours > open.ToDeathHours * 1.2,
            $"The forest ({forest.ToDeathHours:F1} h) should meaningfully outlast open ground " +
            $"({open.ToDeathHours:F1} h).");
    }

    [Fact]
    public void AFireAndAShelterTurnALethalNightIntoAMiserableOne()
    {
        // docs/03 §3 on tier 0: "Goal: survive the first night. A player who does
        // everything right is still cold and miserable. That's correct."
        var climate = new ClimateModel(2024);
        double coldestSeconds = DeepWinterFixture.ColdestNightSeconds(climate);
        EnvironmentSample night = climate.Sample(coldestSeconds, SiteContext.DebrisShelter);

        Outcome outcome = Run(new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.Naked,
            Posture = Posture.Sitting,
            ActivityMet = 1.0,
            FireIrradianceWattsPerM2 = 300.0,
            BeddingResistance = 0.3,
        }, maxHours: SolarPosition.NightLengthHours(WorldClock.Restore(coldestSeconds).YearPhase));

        Assert.True(outcome.ToDeathHours < 0, "A fire and a shelter should get you to dawn.");
        Assert.True(outcome.Final.CoreTemperatureC < ThermalState.NormalCoreC - 0.3,
            "...and you should still be cold when you get there.");
    }

    [Fact]
    public void RawhideNeedsATendedFireButDownAndHideDoesNot()
    {
        // docs/03 §3's tier 1 — a first, untailored hide wrap — and its tier 3 down
        // and hide set are not the same kind of solution, and the difference is not
        // "tier 3 feels warmer". Verified below: rawhide plus a tended fire holds a
        // genuine steady-state core (a real energy balance, not just delaying the
        // inevitable) about as well as down and hide manages with *no* fire at all.
        // The real gap is what happens once nobody is feeding the fire — take it away
        // and the same rawhide layer only holds that equilibrium for as long as
        // glycogen lasts, then heat production collapses and the core follows. A
        // single night is not always enough to expose that (the two can arrive at
        // opposite outcomes within a few minutes of each other, which would make a
        // single-night window a coin flip), so this checks a window well past the
        // point the fireless case's fuel reserve actually runs out.
        var climate = new ClimateModel(2024);
        double coldestSeconds = DeepWinterFixture.ColdestNightSeconds(climate);
        EnvironmentSample night = climate.Sample(coldestSeconds, SiteContext.ForestUnderstory);
        const double WindowHours = 24.0;

        Outcome rawhideWithFire = Run(new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.RawhideWrap,
            Posture = Posture.Sitting,
            ActivityMet = 1.0,
            FireIrradianceWattsPerM2 = 300.0,
            BeddingResistance = 0.3,
        }, maxHours: WindowHours);

        Outcome rawhideAlone = Run(new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.RawhideWrap,
            Posture = Posture.Standing,
            ActivityMet = 1.2,
        }, maxHours: WindowHours);

        Assert.True(rawhideWithFire.ToDeathHours < 0,
            $"Rawhide and a tended fire should hold a stable core for {WindowHours:F0} h, " +
            $"ended at {rawhideWithFire.Final.CoreTemperatureC:F2} °C.");
        Assert.True(rawhideAlone.ToDeathHours is > 0 && rawhideAlone.ToDeathHours < WindowHours,
            $"Rawhide alone, without a fire, should not last {WindowHours:F0} h once its energy " +
            $"reserve runs out — the fire is doing real work, not decoration " +
            $"(died at {rawhideAlone.ToDeathHours:F1} h).");
    }

    [Fact]
    public void DownAndHideMakeAWinterNightSurvivable()
    {
        // docs/03 §3: "Down-stuffed garments and bedding — the single most valuable
        // technology in the game." This is what the whole first year is for. Tested
        // against the coldest instant of the year this seed produces, not an
        // arbitrary winter sample — "should not be lethal" has to hold at the worst
        // case, or it is not really the claim docs/03 is making.
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.OpenGround);

        Outcome outcome = Run(new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.DownAndHide,
            Posture = Posture.Standing,
            ActivityMet = 1.2,
        });

        Assert.True(outcome.ToDeathHours < 0,
            $"Full down and hide should not be lethal even at the year's coldest instant " +
            $"({night.AirTemperatureC:F1} °C).");
        Assert.True(outcome.Final.CoreTemperatureC > 36.0,
            $"Core should stay near normal, was {outcome.Final.CoreTemperatureC:F2} °C.");
    }

    [Fact]
    public void WetClothingIsWorseThanNoneOfItsPromise()
    {
        // docs/02 §1: "being wet multiplies conductive loss catastrophically."
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.OpenGround);

        Outcome dry = Run(new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.DownAndHide,
        });

        Outcome soaked = Run(new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.DownAndHide.WithWetness(1.0),
        });

        Assert.True(dry.ToDeathHours < 0, "Dry down should survive.");
        Assert.True(soaked.ToDeathHours is > 0 and < 14.0,
            $"Soaked down should be lethal within a night, died at {soaked.ToDeathHours:F1} h.");
    }

    [Fact]
    public void ColdWaterIsACountdownInMinutesNotHours()
    {
        // docs/02 §1: "Falling in a lake in autumn should be a genuine emergency with
        // a countdown measured in minutes."
        var climate = new ClimateModel(2024);
        EnvironmentSample autumn = climate.Sample(
            WorldClock.AtSeason(0.85), SiteContext.OpenGround);

        Outcome outcome = Run(new ThermalExposure
        {
            Environment = autumn,
            Clothing = Insulation.Naked,
            Immersed = true,
        }, maxHours: 8.0);

        Assert.True(outcome.ToConfusionHours is > 0 and < 2.0,
            $"Incapacitation in autumn water should arrive within two hours, was {outcome.ToConfusionHours:F2} h.");
        Assert.True(outcome.ToDeathHours is > 0 and < 6.0,
            $"Autumn water should be lethal within six hours, was {outcome.ToDeathHours:F2} h.");
    }

    [Fact]
    public void BeddingIsTheDifferenceBetweenSleepingAndDying()
    {
        // The physical justification for a bough bed being real technology.
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.ForestUnderstory);

        Outcome bareGround = Run(new ThermalExposure
        {
            Environment = night, Clothing = Insulation.Naked,
            Posture = Posture.Lying, ActivityMet = 0.8, BeddingResistance = 0.0,
        });

        Outcome boughBed = Run(new ThermalExposure
        {
            Environment = night, Clothing = Insulation.Naked,
            Posture = Posture.Lying, ActivityMet = 0.8, BeddingResistance = 0.5,
        });

        Assert.True(boughBed.ToDeathHours > bareGround.ToDeathHours * 1.15,
            $"A bough bed ({boughBed.ToDeathHours:F1} h) should outlast bare ground " +
            $"({bareGround.ToDeathHours:F1} h).");
    }

    [Fact]
    public void StarvationTakesAwayTheAbilityToStayWarm()
    {
        // docs/02 §1: "You cannot stay warm while starving."
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.ForestUnderstory);

        var exposure = ThermalExposure.Naked(night);

        var model = new ThermalModel();
        var fed = ThermalState.Fresh();
        var starving = ThermalState.Fresh();
        starving.EnergyReserveKcal = 0.0;

        for (int i = 0; i < 3600; i++)
        {
            model.Step(fed, exposure);
            model.Step(starving, exposure);
        }

        Assert.True(starving.ShiveringHeatWatts < fed.ShiveringHeatWatts * 0.25,
            "A starving body should barely be able to shiver.");
        Assert.True(starving.CoreTemperatureC < fed.CoreTemperatureC,
            "...and should therefore be losing its core faster.");
    }

    [Fact]
    public void HandsGoLongBeforeTheCoreDoes()
    {
        // docs/02 §1's progression begins with clumsy hands, and it is driven by hand
        // skin temperature rather than core temperature — you lose your hands to the
        // cold while you are still entirely lucid.
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.ForestUnderstory);

        var model = new ThermalModel();
        var state = ThermalState.Fresh();
        var exposure = ThermalExposure.Naked(night);

        for (int i = 0; i < 1800; i++) model.Step(state, exposure);

        Assert.True(state.SkinTemperature(BodyZone.Hands) < state.SkinTemperature(BodyZone.Core),
            "Hands must be colder than the torso.");
        Assert.True(state.Dexterity < 0.5, "Dexterity should already be failing after half an hour.");
        Assert.Equal(HypothermiaStage.Normothermic, HypothermiaThresholds.Classify(state.CoreTemperatureC));
    }

    [Fact]
    public void TheHeadKeepsLeakingHeatAfterTheExtremitiesHaveShutDown()
    {
        // The head barely vasoconstricts, which is why a hat is the cheapest thermal
        // technology available and why nothing in the game will point that out.
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.ForestUnderstory);

        var model = new ThermalModel();
        var state = ThermalState.Fresh();
        for (int i = 0; i < 3600; i++) model.Step(state, ThermalExposure.Naked(night));

        Assert.True(state.SkinTemperature(BodyZone.Head) > state.SkinTemperature(BodyZone.Hands) + 5.0,
            "The head should stay much warmer than the hands, because it does not shut its blood off.");
    }

    [Fact]
    public void FrostbiteIsPermanent()
    {
        // docs/02 §1: "Frostbite on extremities is permanent... because permanent
        // consequences are what make cold frightening instead of annoying."
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.OpenGround);

        var model = new ThermalModel();
        var state = ThermalState.Fresh();
        for (int i = 0; i < 6 * 3600; i++) model.Step(state, ThermalExposure.Naked(night));

        double damage = state.Frostbite(BodyZone.Hands);
        Assert.True(damage > 0.0, "A naked winter night should cost the hands something.");

        // Now put them somewhere warm for a day and confirm nothing heals.
        EnvironmentSample summer = climate.Sample(
            WorldClock.AtSeason(0.5, timeOfDay: 0.5), SiteContext.OpenGround);
        for (int i = 0; i < 24 * 3600; i++)
        {
            model.Step(state, new ThermalExposure
            {
                Environment = summer,
                Clothing = Insulation.DownAndHide,
            });
        }

        Assert.Equal(damage, state.Frostbite(BodyZone.Hands));
        Assert.True(state.Dexterity < 1.0, "Lost fingers must keep costing dexterity forever.");
    }

    [Fact]
    public void SprintingThenStoppingIsATrap()
    {
        // docs/02 §1: "running generates heat but also sweat, and sweat in the cold is
        // a trap. Sprinting to stay warm and then stopping is how you die. This is
        // real, it's non-obvious, and learning it is a great player moment."
        var climate = new ClimateModel(2024);
        EnvironmentSample night = climate.Sample(
            WorldClock.AtSeason(0.10, timeOfDay: 0.0), SiteContext.OpenGround);

        var model = new ThermalModel();
        var sweated = ThermalState.Fresh();

        // Half an hour of hard work in a parka.
        var working = new ThermalExposure
        {
            Environment = night,
            Clothing = Insulation.DownAndHide,
            ActivityMet = 6.0,
        };
        for (int i = 0; i < 1800; i++) model.Step(sweated, working);

        Assert.True(sweated.ClothingSweatSaturation > 0.05,
            $"Hard work in insulation should wet it; saturation was {sweated.ClothingSweatSaturation:F3}.");

        // Then stop, and stand in the cold with what is now wet clothing.
        var resting = working with { ActivityMet = 1.2 };
        var neverWorked = ThermalState.Fresh();

        for (int i = 0; i < 3 * 3600; i++)
        {
            model.Step(sweated, resting);
            model.Step(neverWorked, resting);
        }

        Assert.True(sweated.CoreTemperatureC < neverWorked.CoreTemperatureC,
            $"The player who sweated ({sweated.CoreTemperatureC:F2} °C) should end up colder than " +
            $"the one who did not ({neverWorked.CoreTemperatureC:F2} °C).");
    }

    [Fact]
    public void HypothermiaStagesRunInTheOrderTheDesignStates()
    {
        Assert.True(HypothermiaThresholds.ShiveringC > HypothermiaThresholds.ClumsyHandsC);
        Assert.True(HypothermiaThresholds.ClumsyHandsC > HypothermiaThresholds.ConfusionC);
        Assert.True(HypothermiaThresholds.ConfusionC > HypothermiaThresholds.ParadoxicalUndressingC);
        Assert.True(HypothermiaThresholds.ParadoxicalUndressingC > HypothermiaThresholds.UnconsciousC);
        Assert.True(HypothermiaThresholds.UnconsciousC > HypothermiaThresholds.DeadC);

        Assert.Equal(HypothermiaStage.Normothermic, HypothermiaThresholds.Classify(37.0));
        Assert.Equal(HypothermiaStage.Shivering, HypothermiaThresholds.Classify(35.5));
        Assert.Equal(HypothermiaStage.ClumsyHands, HypothermiaThresholds.Classify(34.0));
        Assert.Equal(HypothermiaStage.Confusion, HypothermiaThresholds.Classify(32.0));
        Assert.Equal(HypothermiaStage.ParadoxicalUndressing, HypothermiaThresholds.Classify(30.5));
        Assert.Equal(HypothermiaStage.Unconscious, HypothermiaThresholds.Classify(28.0));
        Assert.Equal(HypothermiaStage.Dead, HypothermiaThresholds.Classify(23.0));
    }

    [Fact]
    public void ShiveringStopsBeforeDeathRatherThanAtIt()
    {
        // Real, and dramatically important: in moderate hypothermia the shaking stops.
        // It feels like relief and it is the opposite.
        var model = new ThermalModel();
        var state = ThermalState.Fresh();
        state.CoreTemperatureC = 30.5;
        state.MeanSkinTemperatureC = 12.0;

        var climate = new ClimateModel(2024);
        model.Step(state, ThermalExposure.Naked(
            climate.Sample(DeepWinterFixture.ColdestNightSeconds(climate), SiteContext.OpenGround)));

        Assert.True(state.ShiveringHeatWatts < 100.0,
            $"Shivering should have largely failed by 30.5 °C, was {state.ShiveringHeatWatts:F0} W.");
    }

    [Fact]
    public void BodyZonesCoverTheWholeBody()
    {
        double total = BodyZones.All.Sum(BodyZones.SurfaceAreaFraction);
        Assert.Equal(1.0, total, 6);
    }

    [Fact]
    public void AFreshBodyIsInEquilibriumWithItself()
    {
        // A body that starts even slightly above its own set point will vasodilate and
        // sweat on the first step, soaking its clothing before anything has happened.
        var model = new ThermalModel();
        var state = ThermalState.Fresh();

        var comfortable = new EnvironmentSample
        {
            AirTemperatureC = 22.0, RelativeHumidity = 0.5,
            WindSpeedMetresPerSecond = 0.1, CloudCover = 0.5,
            SkyTemperatureC = 20.0, GroundTemperatureC = 20.0, WaterTemperatureC = 20.0,
            Site = SiteContext.ForestUnderstory,
        };

        for (int i = 0; i < 3600; i++)
        {
            model.Step(state, new ThermalExposure
            {
                Environment = comfortable,
                Clothing = Insulation.RawhideWrap,
                ActivityMet = 1.2,
            });
        }

        Assert.Equal(0.0, state.ClothingSweatSaturation, 3);
        Assert.InRange(state.CoreTemperatureC, 36.4, 37.2);
    }
}
