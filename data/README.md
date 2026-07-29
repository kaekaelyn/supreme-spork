# Paleobiota dataset

The biological source of truth for the game. **Nothing biological is hardcoded in the engine.** Spawn tables, ecology parameters, model bindings, and butchery tables are all generated from this directory at build time.

Note what is *not* generated from it: player-facing text. [Nothing in the game is named, labelled, or described](../docs/00-the-transplant.md#3-what-the-player-is-never-told). This dataset shapes the world; it never speaks to the player.

## Why it works this way

Three reasons, in order of importance:

1. **The science changes.** New melanosome studies, new specimens, and revised phylogenies land constantly in Jehol paleontology. When they do, a science patch should be a data PR — not an engine change. The changelog is generated from this directory's git history and published **outside** the game, since the game explains nothing about itself.
2. **It's testable.** Ecology soak tests run headless against this data in CI. A species entry that destabilises the food web fails the build.
3. **It's correctable in public.** This dataset is intended to be open-licensed and published separately from the game, so working paleontologists can file issues and PRs against it. See [08 — Decisions §11](../docs/08-decisions.md).

## Schema

Every organism entry supports:

| Field | Meaning |
|---|---|
| `genus`, `species` | Binomial. `species: null` where only the genus is established |
| `unit` | Stratigraphic unit(s) within the Yixian Formation. **Anything outside the Yixian does not belong in this file** |
| `length_m` / `wingspan_m` / `length_mm` / `mass_kg` | Physical dimensions |
| `diet` | Feeding guild; drives the ecology core's trophic links |
| `habitat` | `aquatic`, `semi_aquatic`, `lacustrine`, terrestrial (default) |
| `integument` | `feathers`, `filamentous_feathers`, `pennaceous_feathers`, `pycnofibres`, `fur`, `scales`, `scales_and_quills` |
| `abundance` | Relative population weighting for the coarse ecology tier |
| `game_role` | Tags consumed by spawn/encounter design |
| `design_note` | Guidance for designers. Not player-facing |
| `note` | Factual context for the team and the public dataset. Never player-facing |

### Confidence

Any claim can carry a `confidence` field, or be nested in a block that does:

```yaml
coloration:
  confidence: A
  description: "Ginger and white banded tail, countershaded, bandit mask."
  source: smithwick_2017_sinosauropteryx
```

| Tier | Meaning |
|---|---|
| `A` | Preserved in the fossil record |
| `B` | Strongly inferred — phylogenetic bracketing or functional morphology, broad consensus |
| `C` | Plausible, and unpreservable by nature (vocalisation, subtle colour, sociality) |
| `D` | Speculative but defensible, and genuinely contested |

Tiers are never shown to the player — they are an internal discipline and a public commitment in this dataset. The **Speculation Level** setting filters on them: *Strict* expresses A–B, *Standard* A–C, *Rich* A–D. **No setting ever suppresses a tier A fact.** You cannot turn the feathers off.

### Sources

`source` keys resolve against [`../SOURCES.md`](../SOURCES.md). Every A- and B-tier claim must carry one. CI fails otherwise.

## Rules for contributors

1. **Yixian Formation only.** Check the [exclusion list](../docs/01-the-science.md#7-the-exclusion-list) before adding anything — *Microraptor*, *Anchiornis*, *Jeholornis*, and *Sapeornis* are the four most common mistakes, and *Yixianornis* is Jiufotang despite the name. This rule has no exceptions and no expansion pack.
2. **Never downgrade a fossil fact to make an animal look better.** If it has feathers, it has feathers.
3. **Additions must be unpreservable to be permitted.** Behaviour, vocalisation, soft-tissue detail, and colour where unknown are open. Bone counts, proportions, and integument type are not.
4. **Tag your confidence honestly.** An unmarked guess is worse than a marked one.
5. **Flag geology that is plausible rather than attested** — obsidian and chalcedony availability are inferred from the formation's rhyolitic volcanism, not from a Yixian-specific paper. Flag it here, where it can be read and challenged.

## Planned files

- `species.yaml` — the biota. **Present.**
- `materials.yaml` — stone, clay, fibre, pigment, fuel properties
- `nutrition.yaml` — calories, fat, protein, micronutrients, toxin loads per food item
- `climate.yaml` — seasonal temperature curves, precipitation, wind, snow accumulation by biome
- `schema/` — JSON Schema definitions used by the CI validator
