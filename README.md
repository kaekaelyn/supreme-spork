# The Down Age

**Working title.** A survival game set in the Yixian Formation of the Jehol Biota, western Liaoning, approximately 125.8–124.1 million years ago. Solo, co-op over the Internet, or LAN.

You wake up naked in a conifer forest. Nobody tells you when you are. Nobody tells you anything.

---

## The one-paragraph pitch

Every dinosaur survival game is a hot game. Tropical, lush, green, and full of scaly monsters that want to eat you. The actual Jehol Biota was nothing like that. It was a **cold volcanic highland** — mean annual temperature somewhere between 6 °C and 10 °C, possibly at two to four kilometres of elevation, with frozen winters, seasonal snow, and conifer forest. The dinosaurs were feathered because it was *cold*. There was no grass, no fruit, no grain, and effectively no flowers. The animals had never seen a primate and were not afraid of you. *The Down Age* is the survival game that takes all of that literally: a subalpine survival sim where your first enemy is hypothermia, your second is starvation-by-wrong-macronutrient, and the monsters mostly just want to be left alone until you teach them otherwise.

## What makes this different

1. **It's cold, and that's the whole game.** Hypothermia is the first boss. Winter is the recurring one. See [Survival Systems](docs/02-survival-systems.md).
2. **The Carbohydrate Problem.** No grasses, no cereals, no fruit, no tubers, five known species of flowering plant and one of them is a pond weed. Your calorie strategy has to be fat-and-protein, which means *rabbit starvation is a real failure state*. Ceramics — not agriculture — is the tech that opens up plant food, because you cannot detoxify cycad seeds without a pot to leach them in.
3. **Naive fauna.** Nothing here has ever seen a human. Early game is Eden. The ecology sim *learns*, species develop fear responses based on how you behave, and you are the one who makes the world hostile. There is no narrated story because this **is** the story, told entirely through systems.
4. **Honest epistemics as a mechanic.** Every species has a **confidence tier** drawn from the actual literature. Colour that we know from fossil melanosomes is rendered as known; colour we don't know is flagged as reconstruction. A **Speculation Level** setting (Strict / Standard / Rich) controls how much plausible-but-unpreserved detail the world shows you. See [The Science](docs/01-the-science.md).
5. **The paleontology is versioned data, not hardcoded art.** When a new paper drops, we patch the animal and the codex shows a changelog. The game gets *more correct* over time. See [`data/`](data/).
6. **Culture is built, not chosen.** No clan UI, no tech tree menu, no skill tree. Knowledge is a physical object in the world — ochre on rock, notches on bone. Players invent their own glyphs. Skills you merely practised die with you; skills you *taught* survive. See [Society & Multiplayer](docs/05-society-and-multiplayer.md).

## Documents

| Doc | What's in it |
|---|---|
| [01 — The Science](docs/01-the-science.md) | The paleontological bible. Climate, geology, flora, confidence tiers, the exclusion list. |
| [02 — Survival Systems](docs/02-survival-systems.md) | Thermal model, nutrition, water, disease, parasites, seasons, the day/night cycle. |
| [03 — Technology & Crafting](docs/03-technology-and-crafting.md) | The material culture tree, from bare hands to a heated longhouse. |
| [04 — Bestiary & Ecology](docs/04-bestiary-and-ecology.md) | Creature roster, AI design, the two-tier ecosystem simulation, domestication. |
| [05 — Society & Multiplayer](docs/05-society-and-multiplayer.md) | Solo vs. co-op, glyph writing, teaching, legacy, netcode-facing design. |
| [06 — Technical Architecture](docs/06-technical-architecture.md) | Engine recommendation, simulation layering, networking, world generation, modding. |
| [07 — Roadmap](docs/07-roadmap.md) | Vertical slice definition, milestones, team shape, risks. |
| [08 — Open Questions](docs/08-open-questions.md) | The decisions that are genuinely yours to make, with my recommendations. |
| [SOURCES.md](SOURCES.md) | Every scientific claim in these documents, with citations. |

## Design rules (non-negotiable)

These are the constraints that keep the project honest. Everything else is negotiable.

- **Never remove what the fossil record shows.** If a fossil preserves feathers, the animal has feathers. If melanosomes give us a colour, that is the colour.
- **Only add what wouldn't fossilise anyway.** Soft tissue, behaviour, vocalisation, sexual display, subtle colour, and social structure are open territory. Bone counts, body proportions, and integument type are not.
- **Plausibility beats coolness, every time.** If a feature would make a paleontologist wince, it does not ship.
- **No mythology, no magic, no time travel explanation.** The premise is never addressed in-game. It does not need to be.
- **When the science is uncertain, say so in the codex rather than picking a side silently.**

## Title candidates

*The Down Age* is my pick — it names the material culture (down feathers are the key insulation technology) and doubles as "an age of being brought low." Alternates: **Ashwinter**, **Sihetun**, **Barremian**, **Naive**, **One Hundred Twenty-Five**, **The Long Cold**.
