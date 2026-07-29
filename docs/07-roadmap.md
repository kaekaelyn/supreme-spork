# 07 — Roadmap, Scope & Risk

Honest assessment: this is an ambitious project, and the ambition is concentrated in **simulation**, not content. That's a good place for it to be, because simulation scales without an art team.

---

## 1. Prove the thesis first: the Cold Open prototype

**Target: 6–10 weeks. One programmer, one artist, placeholder everything.**

Not a vertical slice. A **thesis test**. One question: *is being cold and naked in a conifer forest, with no instructions, compelling for 45 minutes?*

Scope:
- One 500 m × 500 m patch of conifer forest, greybox terrain
- The thermal model, complete and tuned
- Fire: fetch an ember from a vent, feed it, shelter it
- Debris shelter construction
- Three species: *Lycoptera* (fish, catchable by hand), *Jeholosaurus* (naive, approachable, edible), *Confuciusornis* (ambient, beautiful)
- Day/night at 23.4 h, one weather system, rain
- **No UI whatsoever**

If the first 45 minutes aren't gripping with zero content, more content won't fix it. If they are, everything else is execution.

## 2. Vertical slice

**Target: 8–12 months from a green light on the prototype.**

- **One valley**, hand-authored, ~4 km², with all six biomes represented
- **A full year cycle** including a survivable-but-brutal winter
- **Tech tiers 0–4**, ending at fired ceramics
- **12–15 species**, materialised and simulated, with the two-tier ecology core running and CI-tested
- **4-player co-op** over the Internet and LAN, dedicated server binary
- **The Codex**, with real citations, and the Speculation Level setting working
- **Glyph writing** and the teaching mechanic
- Naive-fauna fear propagation, demonstrably observable across the year

This is a publishable Early Access build.

## 3. Toward 1.0

| Phase | Adds |
|---|---|
| **EA launch** | The vertical slice, hardened, plus 2–3 more valleys |
| **EA year 1** | Tech tiers 5–7, domestication, the Record, full roster (~35 species), Legacy world sharing |
| **EA year 2** | Multi-year events (volcanic winter), land management, 16-player servers, modding SDK, music/ornament depth |
| **1.0** | Polish, the full ~50-species roster, education/museum edition |
| **Post-1.0** | The **Jiufotang** data pack — the same basin ~5 million years later, with *Microraptor*, *Jeholornis*, and *Sapeornis*. A sequel's worth of content shipped as data |

## 4. Team shape

Minimum viable for the vertical slice, roughly:

- **2 gameplay/simulation programmers** (one owning the ecology core exclusively)
- **1 engine/graphics programmer** (feathers, vegetation, snow, weather)
- **1 network programmer** (part-time until the slice)
- **1 environment artist** and **1 creature artist/animator** — the creature artist is the highest-value hire in the project
- **1 designer** (systems, tuning, and owner of the "no UI" discipline)
- **A paleontological advisor on retainer.** Not a courtesy credit — a working relationship with someone who reviews every species entry. Budget for this from day one; it is the cheapest credibility we will ever buy, and it is the entire premise of the game
- **A paleoartist consultant** for reconstruction review

## 5. Risks, ranked

**1. The ecosystem sim is a research problem, not an engineering task.** Predator-prey systems oscillate, collapse, and explode. Mitigation: build the core headless and engine-independent from day one (§[06.2](06-technical-architecture.md#2-the-two-tier-simulation--the-core-technical-bet)), soak-test in CI, and accept damping terms that aren't strictly ecologically pure. Ship a stable sim over a correct one.

**2. "No UI, no tutorial" is a wall for most players.** Mitigation: diegetic feedback has to be *outstanding* — the body must communicate clearly enough that no text is needed. Playtest with non-genre players early and often. The optional accessibility overlay is not a failure.

**3. Naturalistic animal density feels empty.** See [04 §7](04-bestiary-and-ecology.md#7-animal-density-and-pacing). Mitigation: density settings, deep tracking gameplay, and a dense small-fauna layer so the world is quiet rather than dead.

**4. Audience expectation mismatch.** People will buy this expecting Ark with feathers, then discover a cold subalpine hiking simulator where a flea is more dangerous than a tyrannosaur. Mitigation: *market the cold*. Lead every trailer with snow and breath-fog. Make the first screenshot a naked human shivering in a snowy conifer forest under an unrecognisable sky. Set expectations honestly and the right players will find it.

**5. Scientific criticism.** Unavoidable, and the specialists are the loudest voices in this space. Mitigation: the confidence-tier system, public citations, an open dataset, a real advisor, and a visible willingness to patch. **Being publicly correctable is a stronger position than being right.**

**6. Scope creep via the culture systems.** Glyphs, the Record, teaching, music, and ornament are all seductive and none of them keep a player alive. Mitigation: survival and ecology ship first. Culture systems are the vertical slice's *last* milestone, not its first.

**7. The engine choice being wrong.** Mitigation: the engine-independent ecology core means a port costs us rendering and animation work, not simulation work. That's the insurance policy.

## 6. Commercial notes

- **Early Access is the right model.** Survival games are validated by long-tail community play, and the ecology sim genuinely benefits from thousands of hours of unexpected player behaviour.
- **The education market is real and underserved.** A "Museum Edition" — Strict speculation, codex-forward, no death, free-roam — is a small amount of extra work for access to schools, museums, and institutional licensing. It also generates enormous goodwill.
- **The open dataset is marketing.** A public, citable, versioned Yixian paleobiota dataset that scientists actually use is a permanent credibility asset that no competitor can copy without doing the same work.
- **Differentiation is total.** There is no cold, scientifically-strict, feathered, pre-angiosperm survival game. The nearest neighbours (Ark, The Isle, Path of Titans, Saurian) are all doing something else — and *Saurian*, the closest in spirit, is Hell Creek and plays as a dinosaur rather than a human. The niche is genuinely empty.
