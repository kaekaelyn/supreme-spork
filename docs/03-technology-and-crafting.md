# 03 — Technology & Crafting

**There is no tech tree UI.** There is no research menu, no unlock notification, no skill points. Knowledge is acquired by doing, held by people, and recorded on objects. What follows is the *dependency graph the designers work from* — the player never sees it laid out.

---

## 1. The core principle: recipes are physical, not menu-driven

Crafting happens **in the world**, on surfaces, with tools, in stages. A hand axe is not "3 stone + 1 wood → axe." It is: find the right stone, strike it at the right angle, produce flakes, select one, haft it with pitch and sinew, let the pitch cure. Each stage is a real object that persists. You can leave a half-knapped core on the ground and come back to it in a week.

This is slower than menu crafting and that is the point. It also means **skill is embodied** — a player who has knapped a hundred cores is genuinely better at it, and can *demonstrate* it to another player, which is the foundation of the teaching system in [05](05-society-and-multiplayer.md).

## 2. Materials available in the Yixian

### Stone (from a volcanic basin)
Basalt, andesite, tuff, rhyolitic pyroclastics, and their weathering products are all directly attested in the formation's lithology.

| Material | Use | Notes |
|---|---|---|
| **Basalt / andesite** | Hammerstones, grinding slabs, hearth stones, construction | Abundant, tough, poor for flaking |
| **Tuff** | Carvable soft rock — **you can excavate dwellings into tuff cliffs** | Historically real (Cappadocia). A phenomenal, setting-specific building path |
| **Obsidian / volcanic glass** | The best cutting edge available | Requires rhyolitic volcanism, which the formation has. Rare, localised, worth travelling for. **Plausible-not-attested; flag in the public dataset** |
| **Chert / chalcedony / agate** | The workhorse flaking stone | Silicification of volcanics; regionally well known. Flag confidence |
| **Pumice** | Abrasive, float, insulation | Free from ashfall |
| **Ochre (iron oxides)** | Pigment, hide preservative | Volcanic weathering; essential for the glyph/record system |
| **Sulfur** | Fumarole deposits | Preservative, medicinal, later chemistry |
| **Lacustrine clay** | **Ceramics** | The keystone material. Abundant in lake margins |
| **Coal** | High-density fuel | Attested — the Jianshangou unit contains coal seams |

### Organic
Conifer timber (excellent, straight, abundant, resinous), conifer bark (roofing, containers, **bast fibre cordage**), conifer resin (pitch, glue, antimicrobial, sealant, torches), ginkgo and cycad wood, fern and horsetail fibre (weak), hides, **sinew** (the premium cordage), gut (containers, cordage, waterproofing), bone, antler-equivalents (there are none — no antlers exist yet, so **bone and tooth do all that work**), **feathers and down** (insulation, fletching, quill needles, ornament), eggshell (containers, lime), fish skin (waterproof, historically real), fish oil.

Note the absence: **no wool, no leather-from-cattle, no grass, no bast from flax or hemp, no bamboo, no reeds in the modern sense** (horsetails partly substitute), **no fruit-derived anything, no honey** (bees as we know them may not be present — flag; social bees are an angiosperm-era phenomenon).

## 3. The dependency graph

Written as tiers for the designers' benefit. In game these blur together and are discovered out of order.

### Tier 0 — The first hour, naked
Bare hands. Gather deadwood, sharp stone flakes off the ground, a fire ember from a fumarole. Build a debris shelter. **Goal: survive the first night.** A player who does everything right is still cold and miserable. That's correct.

### Tier 1 — Stone and cordage
Knapping. Hand axe, flake knife, scraper. Bark bast cordage — weak but sufficient. First hide from a small animal, scraped and rawhide-dried. **The bottleneck: sinew requires a large kill, and a large kill requires better tools than you have.** Resolving this deadlock (traps, cliffs, water, fire, cooperation) is the first real puzzle of the game, and in multiplayer the answer is *other people*.

### Tier 2 — Fire mastery and preservation
Friction fire. Controlled hearth. Smoking and drying racks. Rendering fat. **Bone marrow extraction** — solves protein poisoning. Cooking transforms toxin loads and calorie availability. Pitch glue from resin + charcoal + fat. Hafted tools.

### Tier 3 — Hide and textile
Brain tanning — and the brain of an animal is famously almost exactly enough to tan its own hide, which is the kind of thing a player discovers and writes down in disbelief. Sewing with **quill and bone needles** and sinew thread. **Down-stuffed garments and bedding — the single most valuable technology in the game.** Fish-skin waterproofing. Layered clothing with per-zone coverage.

At this point the player can survive a winter. This is the end of Act 1 and should take a serious player most of a first in-game year.

### Tier 4 — Ceramics (the keystone)
Clay preparation, volcanic ash temper, coil building, drying, pit firing, then kiln firing. Ceramics gives you:
- **Watertight vessels** → boiling → **leaching cycasin from cycad seeds** → the plant food economy opens
- Storage against damp and pests
- Rendering and oil storage at scale
- Later: crucibles, if the server allows metal

This is the true midgame gate and it is *historically the right one*. It should feel like an enormous accomplishment and it should be failure-prone — pots crack in firing, and losing a week's work to a bad firing is a real and instructive experience.

### Tier 5 — Construction
Post-and-beam conifer timber framing. Wattle-and-daub using fern and horsetail as binder in clay. Bark shingle roofing. **Sod and turf** — excellent insulation, historically the correct answer for a cold treeline environment. **Tuff excavation** for cliff dwellings. Stone-lined hearths with **thermal mass**. Smoke management: hearth → smoke hole → hood → true chimney. Sunken floors and earth berming for winter warmth (pit houses — exactly what subarctic peoples built, and exactly right here).

### Tier 6 — Projectiles and the hunt
Throwing spear → **atlatl** (the real revolution; a spear-thrower more than doubles effective range and energy) → traps and deadfalls → pit traps → snares → nets from bast and sinew → fish weirs → **bow**, late, because a bow needs good stave wood, quality cordage, glue, and fletching all at once.

**Melee combat against anything over 50 kg should be close to suicide.** The design intent is that you are a naked ape whose advantages are throwing, endurance, fire, traps, and cooperation. Every single one of those is historically what actually made humans dangerous.

### Tier 7 — Husbandry and land management
See [04 §6](04-bestiary-and-ecology.md#6-domestication). Plus **land management as the substitute for agriculture**: coppicing conifers for straight poles, clearing to encourage fern glades, transplanting horsetail beds to convenient sites, protecting productive ginkgo groves, and fire-managing scrub. You do not farm. You *garden the forest*, which is what pre-agricultural peoples actually did and which nearly no game models.

### Tier 8 — The last things a person can actually build

Not "late game unlocks." These are simply the outermost things reachable by [a modern mind with no infrastructure](00-the-transplant.md#2-the-tech-ceiling):

**Lime** — burnt from shell and lacustrine carbonate in a kiln you already built for pottery, giving plaster, mortar, and durable construction. **Soap** — rendered fat and wood-ash lye, which a modern person knows and a Neolithic one did not, and which feeds directly into the parasite and infection systems. **Charcoal** in quantity, for hotter fires and better ceramics. **The bow**, once stave wood, cordage, glue, and fletching all exist at once. **Cordage mechanics** — windlass, block, ramp, lever — enough to move timber and stone no group could otherwise lift. **Structured selective breeding** across generations.

And that is the ceiling.

> **There is no metal in this game.** No ore prospecting, no smelting, no forge, no server toggle. A modern person knows smelting exists; finding workable ore alone in this terrain and running a furnace economy is not something one person or one small settlement achieves, and pretending otherwise would be exactly the wheel this game refuses to reinvent. Also absent for the same reason: the wheel as transport, glass, and plant-fibre textiles — there is no flax, no hemp, no cotton, nothing to spin.
>
> The endgame is not a better tool. It is a settlement that has survived enough winters to have a memory, and has written down enough that its knowledge outlives anyone in it.

## 4. Knowledge as a physical object

The replacement for a tech tree UI, and the thing that makes [permadeath](05-society-and-multiplayer.md#4-permadeath-and-what-survives-you) mean something.

- **You know what you have done.** Skills are per-character and practice-based. They are not written down anywhere in the interface.
- **Knowledge can be recorded**, and you are literate, so you can record it *properly* — real sentences, real diagrams, real measurements, on rock faces, bark, hide, and later plaster. These are world objects. They can be found by strangers. They can burn.
- **A written record transmits the concept, not the skill.** This is the crucial rule. Reading your predecessor's notes on firing pottery tells you the clay mix, the drying time, and the three ways it goes wrong — an enormous learning-rate bonus and the difference between years of rediscovery and one careful season. It does not give you their hands. You still have to fire a hundred pots.
- **Knowledge can be taught** — demonstrating a technique in proximity to another player transfers it far faster than either practice or reading.
- **Knowledge dies.** If the only person who knew how to fire pottery dies without recording or teaching it, it is gone from the world until someone rediscovers it from nothing.

In solo play this is quietly devastating: you are writing for a successor who may never come, and who — if permadeath is on — will be you, with no idea what you knew. In multiplayer it makes elders genuinely valuable and makes a burned record a catastrophe with no combat attached to it.

## 5. The Journal

**There is no codex.** The game ships no reference, no bestiary, no field guide, and no names for anything. What it ships instead is a blank book and the means to fill it.

The Journal is a craftable, physical, losable object. In it you can:

- **Write freely** — your own words, your own language, in your own hand.
- **Sketch** — a simple drawing tool. Draw the animal. Draw the trap that worked. Draw the ridge line and where the sun rose against it.
- **Name things.** Every animal, plant, place, star, and season gets whatever name you give it, and that name is what appears on your own maps and marks. In multiplayer, whether the settlement adopts your name for the big striped one is a social question, not a mechanical one.
- **Tabulate.** Tally marks, day counts, measurements. This is how you catch the 374-day year.

A settlement's accumulated journals are its science — and its scripture, if that is the direction it goes.

**Journals are physical and mortal.** Ochre fades, bark rots, hide cracks and gets eaten, and fire does what fire does. Copying is the only insurance, it is slow deliberate work, and **it introduces errors** — a copy of a copy is not what was written. Degradation must produce *ambiguity rather than deletion*: a smudged number, a missing word, a diagram whose key is gone. Players arguing over what a dead person meant is the point; a blank page is just a punishment. The full reasoning is in [05 §3a](05-society-and-multiplayer.md#3a-transmission-decay-and-how-myth-actually-forms).

Because media differ in lifespan — bark dies with you, ochre on sheltered rock lasts generations, incision into stone is close to forever and costs days — **choosing what to carve in stone is choosing what your settlement thinks is eternal.** That is how monuments happen, with nobody telling anyone to build one. And somebody will eventually build a dry, stone-walled, fire-separated room to keep the journals in, and that will be a library, and nobody will have told them to do that either.

The scientific rigour of this game lives entirely in [the world itself](01-the-science.md) and in the [public dataset](../data/). It never speaks to the player. What the player gets is a pen.
