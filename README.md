# The Elder World

A survival game set in the Yixian Formation of the Jehol Biota, western Liaoning, approximately 125 million years ago. Solo, co-op over the Internet, or LAN.

You wake up naked in a cold conifer forest. Nobody tells you when you are. Nobody tells you anything, and nobody ever will.

---

## The premise

You are a person from the 21st century. You have a modern mind and modern knowledge — germ theory, the concept of a lever, what a bow is, how selective breeding works, that boiling water makes it safe. You have never made any of it with your hands.

**The game is the gap between knowing and doing.** It does not need to teach you anything, because you already know. It only has to let you fail at it.

You may not know where you are. You may not know *when* you are. Nothing in the game will tell you, and it is entirely reasonable to conclude you are on another planet. What this place is, what these animals are, what any of it means — that is yours to decide, name, record, and pass on.

## The five pillars

**1. It is cold, and that is the whole game.** Not a jungle. Mean annual temperature between about 6 °C and 10 °C, possibly at two to four kilometres of elevation, with frozen winters, deep snow, and conifer forest. The dinosaurs are feathered because it is *cold*. Hypothermia is the first thing that kills you. Winter is the thing that kills you every year after.

**2. Time is real.** The world clock runs 1:1 and never stops. A day is 23.4 real hours — the actual Cretaceous day. A year is 374 of them, and it takes a real year. Seasons last as long as seasons last. See [Time](docs/02-survival-systems.md#6-time--the-11-world-clock).

**3. The Carbohydrate Problem.** No grasses, no cereals, no fruit, no tubers. Five known species of flowering plant, one of which is a pond weed. Your calories have to come from fat and protein, which makes protein poisoning a real way to die with a full stomach. Fired pottery — not agriculture — is what opens plant food, because you cannot leach cycasin out of a cycad seed without a vessel.

**4. Nothing here has ever seen a human.** Every population starts unafraid. You can walk up to things. Then the ecology learns: fear rises where kills are witnessed, is inherited, and decays only across generations. Around year two you notice that everything runs from you now, and that you did that. There is no story because this *is* the story, told entirely through systems.

**5. Meaning is player-made.** No names, no field guide, no codex, no tutorial, no lore. You name what you find. You write your own record. Your settlement's beliefs, superstitions, rituals, myths, and science are invented at the table and written on rock. The game supplies a world and a pen.

## Design rules (non-negotiable)

- **Never remove what the fossil record shows.** Feathers where there are feathers. Melanosome colour where we have it.
- **Only add what wouldn't fossilise anyway.** Behaviour, vocalisation, soft tissue, sociality, colour where unknown. Never bone counts, proportions, or integument type.
- **Plausibility beats coolness, every time.**
- **One time, one place. No fudging.** Strict Yixian Formation. Nothing from Jiufotang, nothing from Tiaojishan, nothing from Daohugou — see the [exclusion list](docs/01-the-science.md#7-the-exclusion-list).
- **Nothing is named or explained to the player, ever.** Scientific accuracy is a constraint on the *world*, not a thing the world tells you about itself.
- **The tech ceiling is what a modern person could actually achieve**, not what they know exists. No metal.
- **The game does not get easier.** Settings change the world's rules, never its lethality.

## Documents

| Doc | What's in it |
|---|---|
| [00 — The Transplant](docs/00-the-transplant.md) | Who you are, what you know, what you can't do, and where the tech ceiling sits |
| [01 — The Science](docs/01-the-science.md) | The paleontological bible. Climate, geology, flora, confidence tiers, exclusions |
| [02 — Survival Systems](docs/02-survival-systems.md) | Thermal model, nutrition, water, fire, parasites, and the 1:1 world clock |
| [03 — Technology & Crafting](docs/03-technology-and-crafting.md) | Material culture from bare hands to the tech ceiling. The Journal |
| [04 — Bestiary & Ecology](docs/04-bestiary-and-ecology.md) | Roster, naive fauna, two-tier ecology sim, domestication |
| [05 — Society & Multiplayer](docs/05-society-and-multiplayer.md) | Modes, arrivals, writing and glyphs, teaching, permadeath, PvP |
| [06 — Technical Architecture](docs/06-technical-architecture.md) | Engine, ecology core, always-on server, world generation, data pipeline |
| [07 — Roadmap](docs/07-roadmap.md) | Prototype and vertical slice scope, team, risks |
| [08 — Decisions](docs/08-decisions.md) | Settled design decisions and their reasoning |
| [09 — Assets & Production](docs/09-assets-and-production.md) | The solo/AI-assisted production manual: pipeline, tools, order of work |
| [10 — Stage 1: the greybox](docs/10-stage-1-greybox.md) | What is built, the reference scenario numbers, and what is deliberately absent |
| [SOURCES.md](SOURCES.md) | Every scientific claim, cited, with verification status |
| [data/](data/) | The paleobiota as versioned, open, machine-readable data |

## Building it

```bash
dotnet test tests/ElderWorld.Core.Tests    # the simulation, headless — no engine needed
dotnet build ElderWorld.sln                # everything, including the Godot assembly
```

Then open `game/` in Godot 4.5+ and press play. Controls and the current state of the
prototype are in [10 — Stage 1](docs/10-stage-1-greybox.md).

The simulation lives in `src/ElderWorld.Core`, which contains no engine types at all —
[decision 13](docs/08-decisions.md#13-ecology-core--same-language-as-the-engine-but-isolated-as-a-module),
enforced by a test. Everything in `game/` reads that core and renders it; nothing there
makes a survival decision.

## Audience

This is not a general-release game. It is punishing, slow, permanent, and unexplained, and it is built for people who want that. The reference points are Haven & Hearth and Project Zomboid's relationship with time, not Ark.
