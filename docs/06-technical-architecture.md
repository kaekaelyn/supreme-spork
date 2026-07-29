# 06 — Technical Architecture

Opinionated recommendations, with reasoning. Every one of these is arguable and I've tried to say where.

---

## 1. Engine

**Settled: Godot 4, C#.** See [decision 12](08-decisions.md#12-engine--godot-4-revised) for the reasoning and its date — this section originally recommended Unreal and is kept below only as the superseded reasoning trail. Nothing past this point should be read as current guidance; treat every "Unreal" and "Nanite/Lumen" reference in this document and in [09](09-assets-and-production.md) as historical unless a passage explicitly says otherwise.

**Critical architectural rule regardless of engine: the ecology simulation must not live in the engine.** See §2.

### Superseded reasoning (Unreal, pre-decision-12)

The original recommendation was Unreal Engine 5.4+, on the strength of Nanite for high-polygon vegetation, Lumen for under-canopy light, mature dedicated-server tooling, and — the argument decision 12 records as having actually decided it — free Megascans access, which "would have solved most of the environment art at no cost." That stopped being true when Epic's free-for-Unreal Megascans arrangement ended at the close of 2024 (the Quixel-to-Fab migration completed in 2026). Without it, Nanite/Lumen/PCG were real advantages but no longer decisive against Unreal's cost in learning curve, iteration speed, and hardware — especially since the [stylised, non-photoreal art direction](09-assets-and-production.md#13-style-is-a-consistency-enforcement-mechanism-not-just-taste) never needed Nanite or Lumen the way a photoreal pipeline would have.

Known costs of the Godot choice, accepted with open eyes: no Nanite, so AI-generated meshes need decimation and LODs (automatable in Blender, but a real pipeline step); weaker terrain and foliage tooling, much of it community-maintained; a thinner 3D tutorial ecosystem; and more of the multiplayer layer built by hand. Fallback if Godot proves insufficient: Unity 6 — free under $200k, the largest solo-dev tutorial ecosystem, and the engine *The Long Dark* itself was built in.

## 2. The two-tier simulation — the core technical bet

This is the architecture the game lives or dies on.

```
┌─────────────────────────────────────────────────────────────┐
│  ECOLOGY CORE  (engine-independent, plain C++/Rust library)  │
│                                                              │
│   • Hex or square cells covering the whole map               │
│   • Per cell: species populations, age structure, biomass,   │
│     fear-of-human value, forage availability, water, snow    │
│   • Slow tick (1 Hz or slower). Runs everywhere, always.     │
│   • Predation, breeding, starvation, migration, disease      │
│   • Deterministic, seeded, fully serialisable, headless      │
│   • Unit-testable and runnable WITHOUT the engine            │
└──────────────────────────┬───────────────────────────────────┘
                           │  materialise / dematerialise
┌──────────────────────────▼───────────────────────────────────┐
│  AGENT LAYER  (in-engine, near players only)                 │
│   • Individual animals with AI, animation, physics           │
│   • Spawned FROM cell populations; despawn writes state BACK │
│   • Full sensory model: sight cones, scent (wind-borne),     │
│     hearing. Individual memory and fear.                     │
└──────────────────────────────────────────────────────────────┘
```

**Why the core must be engine-independent:** it lets us run the ecology headless for testing and balancing. We can simulate a hundred in-game years in seconds, in CI, and assert that populations don't collapse or explode. An ecosystem simulation you can only test by playing the game is an ecosystem simulation that will never be balanced. This is the highest-leverage engineering decision in the project.

**Materialisation rules:**
- Cells within a radius of any player materialise. Radius scales with terrain visibility.
- Materialisation is **stochastic but conserving** — spawn positions are plausible for the species and terrain, and the count is drawn from the cell population. Walking back and forth across a boundary must not duplicate animals.
- De-materialisation writes back health, fear, age, and position so an animal you wounded is still wounded when you meet it again.
- **Named individuals** (a specific *Yutyrannus*, a domesticated herd) are exempt: they persist as full entities regardless of player proximity, at low simulation fidelity.

## 3. Networking

- **Authoritative dedicated server.** The ecology core runs server-side only. Non-negotiable — client-side ecology means desync and cheating.
- **Server binary ships alongside the game**, headless, Linux and Windows, so LAN and self-hosting are trivial. Also supports listen-server ("host and play") for the friend-group case where nobody wants to run infrastructure.
- **Replication budget.** With ≤16 players, per-animal replication is affordable. Prioritise by relevance: animals near a player at full rate, distant ones at reduced rate or not at all.
- **Client-side prediction** for player movement only. Animals are server-authoritative with interpolation — a slight lag on an animal is far better than a rubber-banding predator.
- **World persistence.** Full world state (terrain deformation, structures, ecology cells, journals, inscriptions, glyph definitions, graves) serialises to a single portable directory. **This must be a clean, documented, copyable format** because the Legacy seed in [05](05-society-and-multiplayer.md#the-legacy-seed) depends on world files being shareable — including the player-authored text and imagery in them.

### The clock is infrastructure, not a feature

The [1:1 always-on world clock](02-survival-systems.md#6-time--the-11-world-clock) has architectural consequences that have to be designed for from the start, not retrofitted:

- **The server runs 24/7 and simulates continuously.** The ecology core must be cheap enough to run indefinitely with zero players connected — which the coarse tier already is, and which is another argument for keeping it engine-independent and headless. A world nobody is logged into should cost almost nothing.
- **Self-hosting must be genuinely easy**, because at 1:1 a world is a multi-year commitment and people will run these on a home box or a cheap VPS for a very long time. Small footprint, clean upgrades, no data loss across patches. **Save-format stability is a first-class requirement**, not a nice-to-have — breaking a two-year-old world is unforgivable in a game built on permanence.
- **Backups and migration are player-facing features.** Ship them.
- **The clock is authoritative and monotonic.** No skipping, no voting, no acceleration. Solo pauses on quit (default); dedicated servers do not.
- **Long-absence reconciliation.** A player returning after weeks needs the world to have moved coherently — seasons, ecology, structural decay, food spoilage, fire out, animals dispersed. All of it falls out of the coarse tier running continuously, which is the payoff for building it properly.

## 4. World generation

**Not fully procedural.** A hand-authored **paleogeographic basin layout** — lake positions, volcanic centres, ridge lines, drainage, the tuff cliffs — with procedural detail generation inside it. Reasons: the real Yixian is a specific and knowable landscape, procedural generation cannot be trusted to produce a plausible volcanic lake basin, and hand-authoring the macro-layout lets us place the biome heterogeneity that the *Sinosauropteryx* and *Psittacosaurus* camouflage studies tell us existed.

Biome layout derived from the science:

| Biome | Basis | Signature species |
|---|---|---|
| **Closed conifer forest** | Palynology: >70% bisaccate conifer pollen | *Psittacosaurus*, *Sinocalliopteryx* |
| **Open scrub / floodplain** | *Sinosauropteryx* countershading indicates open habitat | *Sinosauropteryx*, iguanodonts |
| **Lake margin & wetland** | The lacustrine deposits themselves | *Hyphalosaurus*, *Archaefructus*, waterfowl-analogue birds, fish |
| **Volcanic slopes & ash fields** | Formation lithology | Sparse; obsidian, sulfur, fumaroles, hot springs |
| **Tuff badlands** | Pyroclastic deposits | Carvable dwellings; low productivity |
| **Subalpine / treeline** | 2.8–4.1 km paleoelevation estimates | Marginal. Cold, exposed, and where you go for stone |

**Seed-locked but hand-curated:** ship a small number of authored basin seeds rather than infinite procedural worlds. This is a game about learning *one* valley deeply.

## 5. Vegetation and the "no grass" problem

A practical warning: **every off-the-shelf environment art pipeline assumes grass.** Grass cards are the default ground cover in every foliage tool, every asset pack, every terrain shader. We have none. Our ground cover is **ferns, horsetails, lycopods, moss, leaf litter, and bare volcanic soil.**

This is an art-direction risk *and* an enormous opportunity — it is the fastest way for a screenshot to look genuinely alien without inventing anything. Budget for a **custom ground-cover pipeline** early, and audit every asset for accidental angiosperms. (Expect to reject a lot of otherwise-good foliage assets. Broadleaf trees, flowers, grasses, and fruit are all anachronisms here.)

## 6. Feathers

The technical signature of the game and worth real R&D time.

- **Feathers are the primary surface treatment on nearly every vertebrate.** Card-based feather shells with per-feather motion at close range, LOD-ing to shell/fin rendering at distance.
- **Feathers must respond to state**: wind, wet (clumping and darkening — enormously important for realism), cold (fluffing, which visibly changes silhouette), threat display, and damage.
- Wet-feather and snow-accumulation shaders are high-value: they sell the climate in a single frame.

## 7. Data pipeline

The versioned-paleontology principle from [01 §9](01-the-science.md#9-standing-rule-the-paleontology-is-data) needs real infrastructure:

- All species, plants, and their properties live in [`data/`](../data/) as YAML, validated against a schema in CI.
- The build pipeline generates spawn tables, ecology parameters, model/material bindings, and butchery tables **from that data**. It generates nothing player-facing in text form, because [nothing in the game is named or described](00-the-transplant.md#3-what-the-player-is-never-told).
- Each entry carries `confidence`, `sources`, and `revision`. A science patch is a data PR, and the changelog is generated from git history — published **outside** the game, in the open dataset repository.
- **The dataset should be public and open-licensed**, separately from the game. Paleontologists will correct it for free, which is worth more than a consultant, and it turns our biggest credibility risk into a community asset.

## 8. Modding

Given the audience, mod support is close to mandatory and cheap if planned:
- Species data is already external data, so modders can add taxa trivially.
- Somebody will add *Microraptor* within a week of launch. Let them, and never ship it ourselves, in any form — not as a stub, not as an "official" pack, not in a trailer. [One time, one place](01-the-science.md#7-the-exclusion-list). What we ship is what defines us; what the workshop does is the workshop's business.
- The likelier and more valuable mods are **tooling**: journal export, map rendering, glyph fonts, and server-management utilities. Design the save format so those are possible without reverse-engineering.

## 9. Testing strategy

- **Headless ecology soak tests in CI.** Run 100 in-game years across every biome seed; assert no population collapses, no exponential blooms, no NaN, and stable predator/prey ratios. This should run on every PR touching ecology or species data.
- **Nutrition/thermal model unit tests** with reference scenarios: "naked at 5 °C in 10 km/h wind" should produce a documented, reviewable time-to-incapacitation.
- **Determinism tests** on the ecology core — same seed, same result, so bug reports are reproducible.
