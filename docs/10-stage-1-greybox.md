# 10 — Stage 1: the greybox

The Cold Open prototype from [07 §1](07-roadmap.md#1-prove-the-thesis-first-the-cold-open-prototype), built to the Stage 1 scope in [09 §8](09-assets-and-production.md#stage-1--greybox-no-art-at-all): **thermal model, fire, and a real 23.4-hour day, in pure untextured geometry.**

This document is the engineering record for that stage — what exists, what the numbers are, and what is deliberately absent. It is not player-facing and never will be; [nothing in the game explains itself](00-the-transplant.md#3-what-the-player-is-never-told).

---

## 1. Layout

```
ElderWorld.sln
├── src/ElderWorld.Core/          The simulation. Knows nothing about any engine.
├── tests/ElderWorld.Core.Tests/  113 tests. Run headless, no editor needed.
└── game/                         The Godot 4 project. Everything that knows what Godot is.
```

The split is [decision 13](08-decisions.md#13-ecology-core--same-language-as-the-engine-but-isolated-as-a-module), and it is the one architectural discipline worth defending: *"the ecology core lives in its own assembly with no engine types in it — no `Node`, no `Vector3` from the engine, no scene tree."*

`ArchitectureTests` fails the build if that is ever violated. It buys the two things the engine-independent design was for: soak tests that run centuries without launching an editor, and portability insurance if the engine choice is revisited.

**Nothing in `game/` makes a survival decision.** It reads the core and renders it.

## 2. Running it

```bash
dotnet test tests/ElderWorld.Core.Tests    # the simulation, headless, no engine
dotnet build ElderWorld.sln                # everything, including the Godot assembly
```

Open `game/` in Godot 4.5 or later and press play. If your editor version differs, change the `Godot.NET.Sdk` version in `game/ElderWorld.Game.csproj` to match; Godot usually offers to do it for you.

### Controls

Out-of-fiction, because the fiction explains nothing. There is no UI, no prompt and no crosshair.

| Key | |
|---|---|
| `W A S D` | Move |
| `Shift` | Run — which makes heat, and sweat, and sweat is a trap |
| `C` | Sit |
| `E` | Take an ember from hot ground · lay it in the hearth · add tinder |
| `Q` | Add a piece of deadwood to the hearth |
| `Esc` | Release the mouse |

Dev builds only, and never on screen — all console output:

| Key | |
|---|---|
| `1`–`5` | Time scale: 1×, 30×, 300×, 1500×, 6000× |
| `F1` | World state |
| `F2` | Body state |

The world starts in autumn, an hour or two before sunset, because the Stage 1 gate is about the night and the answer needs you to watch the light go.

## 3. What the models do

### The clock ([02 §6](02-survival-systems.md#6-time--the-11-world-clock))

A day is 23.4 real hours, a year is 374.615 days and takes exactly one real year, a lunar month is 30.14 days. The clock is monotonic and never skips.

The precession gift is real and measured: world time-of-day slides against real time by 36 minutes a real day, carrying a fixed play slot through the entire cycle in **39 real days**.

Sun and moon are computed from declination and hour angle at 42° N, not from a curve. Day length runs 8.7 h at the winter solstice to 14.7 h at the summer one; the year-round mean night is 11.70 h, exactly as [02 §6](02-survival-systems.md#the-long-night) says.

### Climate ([01 §2](01-the-science.md#2-climate--the-single-most-important-fact))

A pure function of `(seed, world time, site)` — no state, nothing to serialise, and identical at 1× and 6000×.

Measured over a full simulated year at the basin floor:

| | Model | Design |
|---|---|---|
| Mean annual | **7.0 °C** | ~7 °C |
| Absolute minimum (open ground) | **−17.6 °C** | ~−20 °C |
| Absolute maximum | **23.4 °C** | ~24 °C |
| Months below freezing | **4.4** | 4–5 |

Sky radiant temperature is computed rather than approximated, because the gap is a first-order survival fact: a clear cold night's sky sits **26.9 K below air temperature**, an overcast one **3.2 K**. That difference is worth around 40 W to a standing person, which is the difference between a survivable night and a fatal one.

**Canopy makes the air under it warmer at night, not colder — found as a bug, not designed in.** An earlier version of this model reduced wind under canopy (correctly — see `SiteContext.WindExposure`) but let that same reduction *amplify* the nighttime diurnal swing through the "calm air lets a radiative inversion build" mechanism above, with nothing counteracting it. The result was a forest that measured colder overnight than open ground at the same elevation — backwards from how real forest microclimates behave: canopy intercepts outgoing longwave and re-emits part of it downward, so forest floors are frost refugia and nearby clearings are frost pockets on the same clear, calm night (this is standard agricultural-meteorology microclimate science, not specific to this project). Fixed by giving `SiteContext.SkyViewFactor` — already used for a person's radiant environment — the same damping effect on the *ambient air*: closed canopy behaves like a site-fixed cloud layer it can never see past. At the coldest instant of a simulated year, open ground now reads **−18.3 °C** against **−15.2 °C** in the forest, the physically correct direction, and `CanopySuppressesNightCoolingRatherThanAmplifyingIt` in `ClimateTests` pins it so it cannot regress silently. This also strengthened the intended design fact in [02 §1](02-survival-systems.md#1-thermal-model--the-primary-killer) rather than weakening it: the forest now buys survival time for two independent reasons (less wind chill *and* warmer air) instead of one.

### The thermal model ([02 §1](02-survival-systems.md#1-thermal-model--the-primary-killer))

A two-node model in the Gagge lineage — a core and a skin shell, coupled by blood flow the body throttles to defend the core at the expense of the extremities. Every term is a real heat flow in watts.

Inputs, all of them from [02 §1](02-survival-systems.md#1-thermal-model--the-primary-killer): air temperature, wind, wet, insulation by zone, activity, fuel, shelter, ground contact, and radiant gain from fire and sun.

Four zones — head, core, hands, feet — because they fail separately. Hands and feet vasoconstrict almost completely and are what freezes; the head barely constricts at all, which is why an uncovered head keeps leaking heat long after the fingers have gone numb, and why a hat is the cheapest thermal technology in the game.

### Fire ([02 §4](02-survival-systems.md#4-fire))

The bed is a thermal mass. Combustion heats it, weather cools it, and everything else is read off its temperature.

Two behaviours worth calling out because neither is scripted:

- **Ventilation limits output.** Real fires are air-limited, not fuel-limited. Piling on ten times the kindling gives about five times the fire and mostly just makes it last longer.
- **Banking works, and falls out of one number.** Combustion below a fuel's ignition point still smoulders, and smouldering has no plume, so nearly all its output stays in the coals. The same ash blanket that starves a fire of oxygen also stops it radiating. So closing a fire down makes it dim, slow and *long* — a stone hearth with 8 kg of thick hardwood is still alive after a 14.7-hour night.

The fumarole ember-carry works: a well-wrapped ember lasts 3.7 h, a loosely wrapped one 0.9 h, one in an open hand 11 minutes. Heavy rain kills it; drizzle does not.

## 4. The documented reference scenarios

[06 §9](06-technical-architecture.md#9-testing-strategy) asks for exactly this, so here it is. All of these are assertions in `ThermalReferenceScenarioTests`, so a change to the model has to walk past them.

**The named reference — naked, standing, dry, well fed, 5 °C, 10 km/h wind, overcast:**

> shivering from ~11 h · **confusion (incapacitation) at ~13 h** · death at ~20 h

That is longer than intuition suggests and it is right. Still air at +5 °C is survivable for a long time: maximal shivering makes around 350 W against roughly 250 W of loss once the skin has cooled and the shell has closed down. What actually kills is *fuel* — shivering burns the glycogen reserve, and when that runs low shivering fades and the core falls away. The first-night killer in this game is winter, not a mild afternoon.

**The genuinely coldest instant a simulated year produces** (not an arbitrary fixed date — see the box below), open ground at **−18.3 °C**, forest at **−15.2 °C**, against a 14.7-hour night:

| | Confusion | Death |
|---|---|---|
| Naked, open ground | 6.4 h | **9.1 h** |
| Naked, forest understory | 10.5 h | **14.9 h** |
| Naked, lying on bare ground | 7.5 h | 11.0 h |
| Naked, curled on a bough bed | 12.2 h | 17.4 h |
| Naked, sitting at a fire, sheltered | — | survives to dawn |
| Full down and hide | — | survives, core 36.6 °C |
| **Soaked** down and hide | 7.7 h | 10.9 h |

Which is the shape the design asks for. A naked player in the open dies well before dawn. The forest buys hours without being told to, for two independent reasons now (§3's canopy fix). A fire and a shelter get you to morning and leave you cold — [03 §3](03-technology-and-crafting.md#tier-0--the-first-hour-naked) says a player who does everything right on night one is "still cold and miserable," and the model agrees: `AFireAndAShelterTurnALethalNightIntoAMiserableOne` requires survival to dawn *and* a core still measurably below normal when it arrives.

**Rawhide plus a tended fire is a genuine equilibrium, not a stopgap — but it needs the fire.** Tier 1's untailored hide wrap ([03 §3](03-technology-and-crafting.md#tier-1--stone-and-cordage)) at a 300 W/m² fire settles into a stable core (36.6 °C) that holds indefinitely — the same steady state full down and hide reaches with no fire at all. Take the fire away and the identical clothing holds that equilibrium only as long as the glycogen reserve funds the shivering it takes to hold it: reserve runs out around 15 hours, heat production collapses, and the core follows it down to death within another 5–9 hours. `RawhideNeedsATendedFireButDownAndHideDoesNot` in `ThermalReferenceScenarioTests` pins both halves of that, because the interesting design fact here is not "tier 1 is worse than tier 3" but *where* the two solutions actually differ: tier 3 is the one that doesn't need anyone awake feeding it.

Cold water is a countdown in minutes: incapacitation at 44 minutes in autumn water, death at 2.7 h.

> **On "the coldest instant."** Earlier drafts of this table used a fixed year-phase (11 days after the solstice) on the assumption that it represented "deep winter." It measured a comparatively mild −11.2 °C, not because the model is wrong but because a single fixed phase is a roll of the dice against `SynopticAmplitudeC`'s seed-dependent weather — for this seed the year's actual coldest moment lands two days *before* the solstice, not three weeks after it. `DeepWinterFixture` now searches a window around the solstice for the coldest sample instead of trusting one date to be representative, which is what a claim like "down and hide should not be lethal" needs to be tested against to mean anything.

## 5. Time-scale invariance

[07 §2](07-roadmap.md#2-the-time-problem-in-development) makes this a hard constraint, not a nicety: *"Everything must be correct at any rate."*

Every model integrates in fixed world-time steps through a `FixedStepAccumulator`, never against frame delta. `TimeScaleInvarianceTests` asserts that a body run for an hour at 1× and at 1000× agrees to within a single step, that 30 fps and 144 fps agree, and that a fire burns the same fuel whether time arrives in 1/60-second slivers or 60-second slabs.

If those tests ever fail, a bug found in a soak run will not reproduce at 1× and a balance pass done at 1× will be wrong for every player. They are the most important tests in the suite.

## 6. What is deliberately not here

Stage 1 scope only. None of this is a gap in the sense of being forgotten:

- **No creatures.** The three placeholder-cube animals of [07 §1](07-roadmap.md#1-prove-the-thesis-first-the-cold-open-prototype) are not in yet. The ecology core is Stage 2.
- **No nutrition.** `EnergyReserveKcal` exists and shivering burns it, which is the seam the four-axis model of [02 §2](02-survival-systems.md#2-nutrition--the-carbohydrate-problem-mechanised) plugs into. Protein poisoning, toxin load and the seasonal food calendar are Stage 2.
- **No crafting, no shelter construction, no clothing.** `Insulation` models all four zones and the down and hide sets are defined and tested, but nothing in the greybox lets you make them.
- **No snow accumulation.** Snow cover is a smooth function of the season, flagged in the code as a Stage 1 placeholder. The real model in [02 §7](02-survival-systems.md#7-snow-and-ice) accumulates by depth with movement cost, roof loading and tracking, and needs state this deliberately does not carry.
- **No stars.** [01 §8](01-the-science.md#8-the-sky) wants a procedurally generated, unrecognisable night sky with no modern constellations and no pole star. That is one shader and it is the first visual job of Stage 2. The night is currently dark and empty overhead.
- **No multiplayer, no persistence.** [06 §3](06-technical-architecture.md#3-networking)'s save format is Stage 2. The core is written to be serialisable — weather needs nothing stored at all — but nothing writes a file yet.
- **Fuel is unlimited.** Gathering is Stage 2; the greybox hands you deadwood so the night can be tested.
- **Placeholder audio.** Synthesised wind and fire, because Stage 1 ships no assets. [09 §6](09-assets-and-production.md#6-audio--your-real-graphics-budget) is right that this is worth doing early — it changes how the greybox feels — and equally right that the real thing is a field recorder.

## 7. Internal contradictions and a real bug, found and fixed

Found while building Stage 1, and resolved directly in the design documents and the code — not left as a standing trap for whoever reads them next, and not just noted and left broken. Recorded here so the reasoning survives even though the documents and the model now agree with themselves.

**The engine.** [08 §12](08-decisions.md#12-engine--godot-4-revised) settles on Godot 4 with C#, but [06 §1](06-technical-architecture.md#1-engine) still recommended Unreal 5.4+, and [09 §2](09-assets-and-production.md#2-the-tool-stack)'s tool table still listed Unreal as the engine and leaned on free Megascans access — the very thing decision 12 says stopped being true and therefore decided against Unreal. **Fixed:** 06 §1 now leads with Godot and keeps the Unreal analysis only as a clearly marked superseded reasoning trail; 09's tool table, its Nanite reference in §4.4, and its UE5 PCG mention in §5.4 are updated to Godot's actual constraints (no Nanite, no built-in PCG framework, Megascans no longer free).

**The climate numbers couldn't all be true at once.** [01 §2](01-the-science.md#2-climate--the-single-most-important-fact) gives a mean of 7 °C with extremes of −20 °C and +24 °C. The midpoint of those extremes is +2 °C, not +7 °C, so no symmetric model reaches all three. **Fixed:** 01 §2 now carries an implementation note explaining the resolution — a cold-skewed distribution, because snow cover plus long clear nights plus calm air build strong winter inversions with no summer analogue. Checked, not just asserted: a real analog at comparable latitude and elevation (Erzurum, Turkey — 39.9° N, 1,900 m, semi-arid continental) has an annual mean of 5.0–7.4 °C against a record low of −41 °C and record high of 36 °C, which is a considerably *more* aggressive cold skew than this model produces. So any future implementation should be built cold-skewed from the start rather than discovering the contradiction the way this one did. See `ClimateParameters.WinterDiurnalBoostC` for where that asymmetry lives in code.

**A real bug: the forest was modelled colder at night than open ground.** Not a documentation mismatch — the climate model's `AirTemperatureAt` used a site's wind exposure to modulate the nighttime diurnal swing (calm air lets a radiative inversion build harder, which is real and correct on open ground) but never touched `SiteContext.SkyViewFactor`. Since forest reduces wind, forest got *more* calm-air amplification of the nighttime dip with nothing counteracting it — backwards from how real forest microclimates behave. Canopy intercepts outgoing longwave and re-emits part of it downward, which is why forest floors are frost refugia and adjacent clearings are frost pockets on the same clear, calm night; this is standard agricultural-meteorology microclimate science, not a project-specific claim. The symptom was visible in the very first calibration run — forest read 2.7 °C colder than open ground at the identical instant — and got reported in an earlier draft of this document without the direction being questioned. **Fixed:** canopy now damps the diurnal swing the same way cloud cover does, using the same `SkyViewFactor` the thermal model already used for a person's radiant environment. Open ground is untouched (`SkyViewFactor = 1.0` there, so the damping term is a no-op); forest at the same instant now reads warmer, not colder, and `CanopySuppressesNightCoolingRatherThanAmplifyingIt` in `ClimateTests` pins the direction. This also strengthens the forest-shelter design claim in [02 §1](02-survival-systems.md#1-thermal-model--the-primary-killer) rather than undermining it — see §3 above for the corrected figures.

**The "documented reference scenario" numbers weren't testing what they claimed to.** Several tests in `ThermalReferenceScenarioTests` sampled a fixed year-phase (11 days after the solstice) intending it to represent "deep winter," and this document reported numbers computed from a *different*, uncommitted probe that had actually searched for the coldest night of the year. The fixed-phase tests were correct — they just tested a milder night (−11.2 °C) than the −17 to −18 °C this document claimed, because `SynopticAmplitudeC`'s seed-dependent weather can put the year's actual coldest instant anywhere across several weeks around the solstice — for this seed, two days *before* it. One test comment separately asserted "should not be lethal at −15 °C" for a scenario that was never actually run at −15 °C. **Fixed:** `DeepWinterFixture` now searches a window around the solstice for the coldest sample rather than trusting one fixed date, all `ThermalReferenceScenarioTests`/`FireTests`/`EmberTests` deep-winter scenarios use it, and §4 above reports the numbers those tests actually produce. One consequence worth keeping: "down and hide should not be lethal" is now verified against the year's genuine worst case, not an arbitrary mild sample of one — a stronger claim than the one that shipped first.

**A published number with no test behind it.** This document's original "rawhide wrap at a fire: 26.1 h confusion, survives" row came from a one-off probe script, not from anything committed — there was no assertion anywhere backing that specific figure. Investigating it turned up a more interesting and more accurate finding than the number it replaced: rawhide plus a *tended* fire reaches a genuine steady-state equilibrium (indefinitely sustainable, not merely delayed failure), while the same clothing without the fire holds that equilibrium only as long as glycogen funds the shivering, then fails within hours of running out. **Fixed:** added `RawhideNeedsATendedFireButDownAndHideDoesNot`, which asserts both halves, and §4 above reports the finding instead of a single unverified number.

## 8. The gate

> **Is the night compelling?** If two hours of real darkness with a fire is tedious, no amount of art fixes it. Do not proceed until this is a yes.
> — [09 §8](09-assets-and-production.md#stage-1--greybox-no-art-at-all)

Everything above is in service of being able to answer that honestly. The answer is not in this document, because it is not a thing that can be tested by assertion — it needs someone to sit in the dark for two hours and find out.

What Stage 1 can say is that the night is *real*: 14.7 hours of it in midwinter, genuinely dark when the moon is down, with a fire that throws about 300 W/m² at a metre and a half and has to be fed, and a body that is losing a measurable number of watts the whole time.
