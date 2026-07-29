# 08 — Decisions

Settled. Recorded with reasoning so future arguments start from why rather than from scratch. Supersedes the open-questions draft.

---

### 1. Scope — strict Yixian Formation. One time, one place. No fudging.

*Microraptor*, *Jeholornis*, *Sapeornis*, *Yixianornis*, *Anchiornis*, *Tianyulong*, *Epidexipteryx*, *Jeholopterus*, *Protopteryx* and *Eoconfuciusornis* do not appear in this game in any form, ever. No Jiufotang expansion. No "bonus" content from another formation. The goal is that it feels like you actually went back in time to a **specific** moment, and every borrowed animal is a small lie about that.

Stress-tested in [01 §7](01-the-science.md#7-the-exclusion-list): the 2024 reassessment constrains the Yixian's fossiliferous sequence to **under ~93,000 years**, so the whole assemblage genuinely *is* one snapshot. The unit differences (Lujiatun, Jianshangou, Dawangzhangzi, Jingangshan) are resolved **spatially**, as habitat variation across one basin, not as temporal layers.

### 2. Tech ceiling — what a 21st-century person could actually achieve. No metal.

Reframed from "stop before metal" to the sharper and better principle in [00 §2](00-the-transplant.md#2-the-tech-ceiling): the limit is **achievability, not knowledge**. A modern person knows smelting exists and still cannot run a furnace economy alone. There is no ore prospecting, no smelting, no forge, and no server toggle for one. Also out: the wheel as transport, glass, and plant-fibre textiles.

Reachable, because knowledge plus these materials genuinely gets you there: stone tools, cordage, fire, tanning, down insulation, ceramics and kilns, lime and mortar, soap, charcoal, the bow, timber construction, tuff excavation, selective breeding, preservation, writing, and a self-derived calendar.

### 3. Time — 1:1. The clock never stops.

A day is **23.4 real hours**; a year is **~374.6 days**, which is one real year; a lunar month is ~30 days; winter is four to five real months. Full model in [02 §6](02-survival-systems.md#6-time--the-11-world-clock).

Three things make it work rather than merely being hardcore:

- **Precession.** The 23.4-hour day drifts against real time by ~37 min/day, cycling a player's fixed play-slot through the entire day/night cycle every ~40 real days. The worst failure mode of real-time clocks is solved by an accurate detail, free.
- **The long night is content, not dead time.** Eleven hours by the fire is when handwork, teaching, writing, and music happen — historically exactly right, and it makes shelter the difference between the most productive part of the day and a slow death.
- **Logging off is how you skip time.** The server never skips, so shelter becomes *absence insurance* — what happens to your body, stores, and animals while you're gone is the real question base design answers.

Concession lever for groups that need it: a year-rate multiplier that leaves the day at 1:1, clearly labelled as costing astronomical coherence. **Default and intended: 1:1.**

### 4. Permadeath — on by default, server-configurable off.

Your character is gone; you return as [a new arrival](05-society-and-multiplayer.md#2-arrivals) with none of their practised skill. What survives is what you **wrote** and what you **taught** — and a written record transmits the concept, not the competence. Writing things down for whoever comes next is one of the very few things a modern transplant could definitely do from day one, which is why the whole social design rests on it.

Soft option: partial skill inheritance, off by default.

### 5. Where people come from — the world produces them, and never says why.

No breeding, no children, no NPCs. New players and returning dead **wake naked and alone somewhere in the world**, exactly as the first person did, at whatever hour and season it currently is. One rule answers reproduction, growth, and respawn without a word of explanation — and it generates the best social content in the game, because eventually a settlement finds a naked stranger at its edge in the snow and has to decide, twice, what it does about that.

**The generational texture comes from transmission loss, not from births.** The goal — knowledge handed down until the reasons are gone and only the practice remains — is achievable without a native generation, because myth is a rule that outlived its reason, and that is a system rather than a population. Teaching transmits technique without rationale; records degrade into ambiguity; copies of copies drift. Combined with permadeath and, at 1:1, genuine player turnover across real years, a settlement accumulates practices nobody can justify and documents nobody can fully read. See [05 §3a](05-society-and-multiplayer.md#3a-transmission-decay-and-how-myth-actually-forms).

**The acknowledged loss:** no character will ever be someone who never saw the 21st century. Everyone arrives with a modern mind, so the myth that forms is always modern people mythologising each other rather than a second generation mythologising the first. There is no version of this that keeps that texture without NPCs or reproduction, and both cost more than they return.

### 6. Information — none. Ever.

No names for anything. No codex, no bestiary, no field guide, no tooltips, no recipes, no tech tree, no date, no location, no framing device. You may reasonably conclude you are on another planet and the game will never correct you.

The in-game codex is replaced by [the Journal](03-technology-and-crafting.md#5-the-journal): a craftable, losable, burnable book you write and draw in yourself, in your own words, naming everything whatever you decide to name it. A settlement's journals are its science, its history, and possibly its scripture.

The scientific rigour is a constraint on the **world**, never a thing the world narrates. The educational value moves entirely outside the fiction, into the [open dataset](../data/) and published sources — which is a *better* home for it, because a paleontologist can file an issue against a dataset.

### 7. PvP — on, with intent matched to consequence.

Melee cannot harm a person without a deliberate, distinct commitment; you will never kill a friend by turning the wrong way. Projectiles hit whatever is in the line of fire. The environment — deadfalls, falling trees, fire, thin ice, gas hollows — kills anyone, and that is correct, because it produces tragedies rather than griefing and it teaches care.

With permadeath on, killing another player is permanent. In a settlement of six, that is the heaviest thing in the game.

### 8. Difficulty — very punishing. No curated spawn.

You wake wherever and whenever. If it is deep winter, you will probably die. A new server chooses its founding season; after that, arrivals get whatever season it is, with no easing and no grace period.

Settings adjust the **world's rules** — PvP, permadeath, time rate, animal density — and never its lethality. There is no easy mode, because the difficulty is not a dial on top of the design, it *is* the design.

### 9. Cover animal — *Sinosauropteryx*.

Ginger and white banded tail, bandit mask, countershaded, from melanosome evidence. The cover art is literally evidence-based, which is a marketing story in itself. *Yutyrannus* would sell better and would set exactly the wrong expectation.

### 10. Title — **The Elder World**.

### 11. Open dataset — yes in principle. **Licensing deferred.**

Working paleontologists will correct it for free, which is worth more than any consultant. It converts the project's biggest credibility risk into a community asset and — now that nothing is explained in-game — carries the entire educational contribution on its own.

**Superseded in part by §14:** publication and licensing are deferred until there is something worth publishing. The dataset is built to be publishable (citations, confidence tiers, versioning) so the option stays open at no cost.

---

## Production decisions

### 12. Engine — **Godot 4**, revised.

Cost is not the deciding factor, because **all three candidates are free for a non-commercial project**: Unreal is free with a 5% royalty only above $1M gross revenue (the $1,850/seat Unreal Subscription applies to *non-game* industries — archviz, film — not to us); Unity Personal is free under $200k revenue with the runtime fee cancelled in 2024; Godot is MIT with no threshold, royalty, or tier of any kind.

What decided it was **Megascans ceasing to be free**. Epic's free-for-Unreal arrangement ended at the close of 2024 and the Quixel-to-Fab migration completed in 2026; new assets are now individually priced or subscription-gated. Free access to photogrammetric rock, bark, moss, and snow was the single strongest argument for Unreal *for this project*, since it would have solved most of the environment art at no cost. Without it, Unreal's remaining edge is Nanite, Lumen, and PCG — real, but no longer decisive against its costs in learning curve, iteration speed, and hardware.

Against that, Godot offers: zero cost with no strings for a multi-year project, fast iteration, a small readable codebase you can modify yourself, and C# support that keeps the simulation testable. Critically, **the stylized art direction in [09 §1.3](09-assets-and-production.md#13-style-is-a-consistency-enforcement-mechanism-not-just-taste) is well within Godot's capability** — the photoreal path was the one that needed Unreal, and we already decided against photoreal for independent reasons.

**Known costs of this choice**, accepted with open eyes: no Nanite, so AI-generated meshes need decimation and LODs (automatable in Blender, but a real pipeline step); weaker terrain and foliage tooling, much of it community-maintained; a thinner 3D tutorial ecosystem; and more of the multiplayer layer built by hand.

**Fallback if Godot proves insufficient:** Unity 6 — free under $200k, the largest solo-dev tutorial ecosystem, pleasant C#, and the engine *The Long Dark* itself was built in, which is our stated visual reference.

### 13. Ecology core — same language as the engine, but isolated as a module.

Pragmatism wins for a solo project: one language, no FFI boundary, no cross-language debugging. With Godot that means **C#**.

**The one discipline to keep:** the ecology core lives in its own assembly with **no engine types in it** — no `Node`, no `Vector3` from the engine, no scene tree. Same language, zero engine coupling. This costs almost nothing to maintain and preserves both things the engine-independent design was for: headless soak tests that simulate centuries in seconds without launching the engine ([06 §9](06-technical-architecture.md#9-testing-strategy)), and the portability insurance if the engine choice is ever revisited.

### 14. Licensing — everything private for now.

No public dataset, no published code, no license file yet. Deferred until there is something worth publishing, which costs nothing now.

Two notes for whenever it is revisited: a repository with no license is all-rights-reserved by default, so nothing leaks by inaction; and the credibility benefit of the open dataset arrives only once it is public, so earlier publication buys earlier correction. Recommended when the time comes: **CC-BY-4.0** for the dataset, since attribution matches how researchers expect to cite data, and **MIT** for any tooling.

### Not a decision: writing our own engine.

Raised as a cost-saving measure, and it saves nothing — every candidate engine is already free. It would cost years of work that produces no game, on a project whose scarce resource is already the *art*, not the technology.

The honest middle path, and part of why Godot fits: **Godot is MIT-licensed and its source is small enough to read and modify.** You can change the engine where it doesn't do what you need, which is most of the appeal of writing one, without the multi-year detour of starting from nothing.
