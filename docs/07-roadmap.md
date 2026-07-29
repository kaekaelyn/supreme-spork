# 07 — Roadmap, Scope & Risk

The ambition here is concentrated in **simulation and time**, not content volume. That's a good place for it, because simulation scales without an art team — but the 1:1 clock creates development problems that need solving early rather than discovered late.

---

## 1. Prove the thesis first: the Cold Open prototype

**Target: 6–10 weeks. One programmer, one artist, placeholder everything.**

Not a vertical slice. A **thesis test**, asking one question: *is being cold, naked, and unexplained in a conifer forest compelling for two hours?*

Scope:
- One 500 m × 500 m patch of conifer forest, greybox terrain
- The thermal model, complete and tuned
- Fire: carry an ember from a vent, feed it, shelter it, lose it
- Debris shelter construction
- Three animals: one small fish (catchable by hand), one small unafraid herbivore, one flock bird — **none of them named, described, or labelled**
- A real 23.4-hour day, running 1:1, with one full night
- **No UI whatsoever**

The night is the test. If two hours of real darkness with a fire is compelling rather than tedious, the whole time model works. If it isn't, we learn that for ten weeks of cost instead of two years.

## 2. The time problem in development

At 1:1, **nobody on the team can playtest a year.** This needs infrastructure from day one, not month eighteen:

- **A time-scale debug lever** (up to several thousand ×) available in dev builds and never in shipping ones. Everything must be correct at any rate — which is a genuine constraint on how systems are written, and much cheaper to enforce from the start than to retrofit.
- **Snapshot worlds.** Curated save states at "day 40 of the first winter," "year 2 spring," "year 5 established settlement," so anyone can load into any point in the arc immediately. These become the primary QA and design artefacts.
- **Headless ecology soak runs** simulating decades in seconds (see §6).
- **A long-running internal server at true 1:1**, started as early as possible and never reset, so at least one instance of the game is being experienced the way players will experience it. Start this during the prototype. By ship it will be several years old and it will teach us things nothing else can.

## 3. Vertical slice

**Target: 8–12 months after a green light on the prototype.**

- **One basin**, hand-authored, ~4 km², with the six biomes and the stratigraphic assemblages distributed spatially
- **A full year**, playable via snapshot states, with a survivable-but-brutal winter
- **Tech tiers 0–4**, ending at fired ceramics
- **12–15 species**, materialised and simulated, two-tier ecology core running and CI-tested
- **4-player co-op** over the Internet and LAN, dedicated always-on server binary
- **The Journal** — free writing, sketching, tallying — and the glyph system
- **Permadeath, arrivals, and the teaching mechanic**
- Naive-fauna fear propagation, demonstrably observable across a simulated year

## 4. Toward 1.0

| Phase | Adds |
|---|---|
| **Early Access** | The slice, hardened, plus 2–3 more basins |
| **EA year 1** | Tech tiers 5–8 to the ceiling, domestication, full roster (~35 species), Legacy world seeding, save-format stability guarantees |
| **EA year 2** | Multi-year events (volcanic winter), land management, 16-player servers, modding and server tooling, ornament and music depth |
| **1.0** | Polish, full ~50-species roster, the public dataset published and maintained |
| **Post-1.0** | **More of the same world, deeper** — additional basins in the same formation, more of the attested biota, better simulation. **No expansion to another formation or another time.** There is no *Microraptor* DLC. The game is one place |

## 5. Team shape

Minimum viable for the vertical slice:

- **2 gameplay/simulation programmers** (one owning the ecology core exclusively)
- **1 engine/graphics programmer** — feathers, vegetation, snow, weather, and the night-lighting problem, which is a bigger deal here than in most games
- **1 network/backend programmer** — the always-on server, persistence, and save-format stability are load-bearing, not plumbing
- **1 environment artist**, **1 creature artist/animator** — the creature artist is the highest-value hire in the project
- **1 designer** owning systems, tuning, and the "nothing is ever explained" discipline, which will be under constant pressure to erode
- **A paleontological advisor on retainer** — a working relationship, not a credit. Reviews every species entry. Budget from day one; it is the cheapest credibility available and it is the entire premise
- **A paleoartist consultant** for reconstruction review

## 6. Risks, ranked

**1. The ecosystem sim is a research problem, not an engineering task.** Predator-prey systems oscillate, collapse, and explode — and at 1:1 with always-on servers, a slow instability that takes two in-game years to manifest is a *catastrophic* bug that appears in players' worlds long after ship. Mitigation: build the core headless and engine-independent from day one, soak-test hundreds of in-game years in CI on every PR, and accept damping terms that aren't strictly ecologically pure. **Ship a stable sim over a correct one.**

**2. Save-format instability destroys multi-year worlds.** In a 1:1 game, a world is a years-long investment, and losing one to a patch is unforgivable in a way it simply isn't elsewhere. Mitigation: versioned saves, forward migration tested against real old worlds, and an explicit public commitment. Treat a broken save as a sev-1.

**3. The 1:1 clock loses players who wanted a survival game.** Mitigation: don't fight it. This is [not a general-release game](../README.md#audience) and the store page should say so in the first paragraph. Market the commitment as the feature, be honest that a year takes a year, and let the right people self-select. The wrong buyer is a refund and a bad review; the right buyer plays for three years.

**4. "Nothing is explained" reads as unfinished rather than deliberate.** Mitigation: the *body* has to communicate flawlessly — shivering, breath, clumsiness, exhaustion must be legible enough that no text is needed. Playtest specifically for "I didn't know what to do" versus "I knew what to do and couldn't." The first is a bug we must fix; the second is the game.

**5. Naturalistic animal density feels empty.** See [04 §7](04-bestiary-and-ecology.md#7-animal-density-and-pacing). Mitigation: density settings, deep tracking gameplay, and a dense small-fauna layer so the world is quiet rather than dead.

**6. Scientific criticism.** Unavoidable; the specialists are the loudest voices in this space. Mitigation: confidence tiers, public citations, an open dataset, a real advisor, and visible willingness to patch. **Being publicly correctable is a stronger position than being right.**

**7. Scope creep via the culture systems.** Journals, glyphs, music, ornament, and burial are all seductive and none of them keep a player alive. Mitigation: survival and ecology ship first; culture systems are the slice's *last* milestone.

**8. The engine choice being wrong.** Mitigation: the engine-independent ecology core means a port costs rendering and animation work, not simulation work.

## 7. Commercial notes

- **Early Access is right**, and the always-on 1:1 servers make the EA community genuinely load-bearing — they'll accumulate multi-year worlds that no internal testing can replicate.
- **Deliberately niche.** This is a game for people who want permanence, punishment, and no hand-holding. That audience is small, loyal, vocal, and underserved, and it sustains games like this for a decade. Price and scope accordingly; don't chase a mass audience the design actively repels.
- **The open dataset is the marketing.** A public, citable, versioned Yixian paleobiota dataset that scientists actually use is a permanent credibility asset no competitor can copy without doing the same work. It is also the entire educational contribution, now that [nothing is explained in-game](00-the-transplant.md#3-what-the-player-is-never-told) — and it's a *better* home for it, because a paleontologist can file an issue against a dataset and can't file one against a codex entry.
- **Differentiation is total.** There is no cold, strict, feathered, pre-angiosperm, real-time, permadeath survival game. Ark, The Isle, and Path of Titans are doing something else entirely; *Saurian* is Hell Creek and you play as a dinosaur. The nearest spiritual relative is Haven & Hearth, which is not a dinosaur game at all. The niche is empty.
