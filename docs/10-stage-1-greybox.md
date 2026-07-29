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
| Absolute minimum | **−17.6 °C** open, **−20.2 °C** in the forest | ~−20 °C |
| Absolute maximum | **23.4 °C** | ~24 °C |
| Months below freezing | **4.4** | 4–5 |

Sky radiant temperature is computed rather than approximated, because the gap is a first-order survival fact: a clear cold night's sky sits **26.9 K below air temperature**, an overcast one **3.2 K**. That difference is worth around 40 W to a standing person, which is the difference between a survivable night and a fatal one.

### The thermal model ([02 §1](02-survival-systems.md#1-thermal-model--the-primary-killer))

A two-node model in the Gagge lineage — a core and a skin shell, coupled by blood flow the body throttles to defend the core at the expense of the extremities. Every term is a real heat flow in watts.

Inputs, all of them from [02 §1](02-survival-systems.md#1-thermal-model--the-primary-killer): air temperature, wind, wet, insulation by zone, activity, fuel, shelter, ground contact, and radiant gain from fire and sun.

Four zones — head, core, hands, feet — because they fail separately. Hands and feet vasoconstrict almost completely and are what freezes; the head barely constricts at all, which is why an uncovered head keeps leaking heat long after the fingers have gone numb, and why a hat is the cheapest thermal technology in the game.

### Fire ([02 §4](02-survival-systems.md#4-fire))

The bed is a thermal mass. Combustion heats it, weather cools it, and everything else is read off its temperature.

Two behaviours worth calling out because neither is scripted:

- **Ventilation limits output.** Real fires are air-limited, not fuel-limited. Piling on ten times the kindling gives about five times the fire and mostly just makes it last longer.
- **Banking works, and falls out of one number.** Combustion below a fuel's ignition point still smoulders, and smouldering has no plume, so nearly all its output stays in the coals. The same ash blanket that starves a fire of oxygen also stops it radiating. So closing a fire down makes it dim, slow and *long* — a stone hearth with 8 kg of thick hardwood is still alive after a 14.7-hour night.

The fumarole ember-carry works: a well-wrapped ember lasts 3.7 h, a loosely wrapped one 1.3 h, one in an open hand 18 minutes. Heavy rain kills it; drizzle does not.

## 4. The documented reference scenarios

[06 §9](06-technical-architecture.md#9-testing-strategy) asks for exactly this, so here it is. All of these are assertions in `ThermalReferenceScenarioTests`, so a change to the model has to walk past them.

**The named reference — naked, standing, dry, well fed, 5 °C, 10 km/h wind, overcast:**

> shivering from ~11 h · **confusion (incapacitation) at ~13 h** · death at ~20 h

That is longer than intuition suggests and it is right. Still air at +5 °C is survivable for a long time: maximal shivering makes around 350 W against roughly 250 W of loss once the skin has cooled and the shell has closed down. What actually kills is *fuel* — shivering burns the glycogen reserve, and when that runs low shivering fades and the core falls away. The first-night killer in this game is winter, not a mild afternoon.

**A midwinter night, −17 °C, against a 14.7-hour night:**

| | Confusion | Death |
|---|---|---|
| Naked, open ground | 5.6 h | **8.2 h** |
| Naked, forest understory | 8.0 h | **11.4 h** |
| Naked, lying on bare ground | 6.7 h | 9.8 h |
| Naked, curled on a bough bed | 9.4 h | 13.3 h |
| Naked, sitting at a fire | 11.6 h | 16.9 h |
| Rawhide wrap at a fire | 26.1 h | survives |
| Full down and hide | — | survives, core 36.7 °C |
| **Soaked** down and hide | 7.4 h | 10.5 h |

Which is the shape the design asks for. A naked player in the open dies well before dawn. The forest buys hours without being told to. A fire and a shelter get you to morning and leave you miserable, which [03 §3](03-technology-and-crafting.md#tier-0--the-first-hour-naked) says is correct. Wet down is worse than no promise at all.

Cold water is a countdown in minutes: incapacitation at 53 minutes in autumn water, death at 3.3 h.

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

## 7. Three places the design documents disagree with themselves

Found while building this. None of them is serious, all of them are recorded rather than silently resolved.

**The engine.** [08 §12](08-decisions.md#12-engine--godot-4-revised) settles on Godot 4 with C#, and [09 §5](09-assets-and-production.md) notes that document supersedes the roadmap on *how* and *when*. But [06 §1](06-technical-architecture.md#1-engine) still recommends Unreal 5.4+, and [09 §2](09-assets-and-production.md#2-the-tool-stack)'s tool table still lists Unreal as the engine and leans on Megascans — the very thing [08 §12](08-decisions.md#12-engine--godot-4-revised) says stopped being free and therefore decided against Unreal. **Built to decision 12.** Worth a pass over 06 and 09 to bring them into line.

**The climate numbers cannot all be true at once.** [01 §2](01-the-science.md#2-climate--the-single-most-important-fact) gives a mean of 7 °C with extremes of −20 °C and +24 °C. The midpoint of those extremes is +2 °C, not +7 °C, so no symmetric model reaches all three. Resolved physically rather than by fudging: the distribution is cold-skewed, because snow cover plus long clear nights plus calm air build strong winter inversions while summer maxima are capped by evaporation and convection. That is genuinely how a high continental basin behaves, and it lands on all three figures. See `ClimateParameters.WinterDiurnalBoostC`.

**Midwinter nights are 14.7 hours, not "fifteen or sixteen."** [02 §6](02-survival-systems.md#the-long-night) says nights *"run to fifteen or sixteen in midwinter."* Computed honestly from the 42° N of [01 §1](01-the-science.md#1-when-and-where) and a 23.44° axial tilt, the winter solstice gives 8.71 h of daylight and therefore a 14.69 h night. Sixteen would need roughly 50° N. The model is left correct and the doc's figure is the one that is slightly out. The mean night of 11.70 h matches exactly.

## 8. The gate

> **Is the night compelling?** If two hours of real darkness with a fire is tedious, no amount of art fixes it. Do not proceed until this is a yes.
> — [09 §8](09-assets-and-production.md#stage-1--greybox-no-art-at-all)

Everything above is in service of being able to answer that honestly. The answer is not in this document, because it is not a thing that can be tested by assertion — it needs someone to sit in the dark for two hours and find out.

What Stage 1 can say is that the night is *real*: 14.7 hours of it in midwinter, genuinely dark when the moon is down, with a fire that throws about 300 W/m² at a metre and a half and has to be fed, and a body that is losing a measurable number of watts the whole time.
