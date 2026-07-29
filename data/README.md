# Paleobiota dataset

The biological source of truth for the game. **Nothing biological is hardcoded in the engine.** Codex entries, spawn tables, ecology parameters, and butchery tables are all generated from this directory at build time.

## Why it works this way

Three reasons, in order of importance:

1. **The science changes.** New melanosome studies, new specimens, and revised phylogenies land constantly in Jehol paleontology. When they do, a science patch should be a data PR — not an engine change. The in-game codex generates its changelog from this directory's git history, so players can see the game getting more correct.
2. **It's testable.** Ecology soak tests run headless against this data in CI. A species entry that destabilises the food web fails the build.
3. **It's correctable in public.** This dataset is intended to be open-licensed and published separately from the game, so working paleontologists can file issues and PRs against it. See [08 — Open Questions §11](../docs/08-open-questions.md).

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
| `note` | Factual context. May surface in the codex |

### Confidence

Any claim can carry a `confidence` field, or be nested in a block that does:

```yaml
coloration:
  confidence: A
  description: "Ginger and white banded tail, countershaded, bandit mask."
  source: vinther_2017_countershading
```

| Tier | Meaning |
|---|---|
| `A` | Preserved in the fossil record |
| `B` | Strongly inferred — phylogenetic bracketing or functional morphology, broad consensus |
| `C` | Plausible, and unpreservable by nature (vocalisation, subtle colour, sociality) |
| `D` | Speculative but defensible, and genuinely contested |

The player-facing **Speculation Level** setting filters on this: *Strict* expresses A–B, *Standard* A–C, *Rich* A–D. **No setting ever suppresses a tier A fact.** You cannot turn the feathers off.

### Sources

`source` keys resolve against [`../SOURCES.md`](../SOURCES.md). Every A- and B-tier claim must carry one. CI fails otherwise.

## Rules for contributors

1. **Yixian Formation only.** Check the [exclusion list](../docs/01-the-science.md#7-the-exclusion-list) before adding anything — *Microraptor*, *Anchiornis*, *Jeholornis*, and *Sapeornis* are the four most common mistakes, and *Yixianornis* is Jiufotang despite the name.
2. **Never downgrade a fossil fact to make an animal look better.** If it has feathers, it has feathers.
3. **Additions must be unpreservable to be permitted.** Behaviour, vocalisation, soft-tissue detail, and colour where unknown are open. Bone counts, proportions, and integument type are not.
4. **Tag your confidence honestly.** An unmarked guess is worse than a marked one.
5. **Flag geology that is plausible rather than attested** — obsidian and chalcedony availability are inferred from the formation's rhyolitic volcanism, not from a paper about the Yixian specifically, and the codex says so.

## Planned files

- `species.yaml` — the biota. **Present.**
- `materials.yaml` — stone, clay, fibre, pigment, fuel properties
- `nutrition.yaml` — calories, fat, protein, micronutrients, toxin loads per food item
- `climate.yaml` — seasonal temperature curves, precipitation, wind, snow accumulation by biome
- `schema/` — JSON Schema definitions used by the CI validator
