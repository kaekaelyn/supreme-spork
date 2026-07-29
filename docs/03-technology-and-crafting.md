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
| **Obsidian / volcanic glass** | The best cutting edge available | Requires rhyolitic volcanism, which the formation has. Rare, localised, worth travelling for. **Flag as plausible-not-attested in codex** |
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
Brain tanning (real, and the brain of an animal is famously almost exactly enough to tan its own hide — a great piece of trivia to leave in the codex). Sewing with **quill and bone needles** and sinew thread. **Down-stuffed garments and bedding — the single most valuable technology in the game.** Fish-skin waterproofing. Layered clothing with per-zone coverage.

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

### Tier 8 — Optional long tail
Bog iron / limonite from lake margins is geologically plausible; copper via malachite in hydrothermally altered volcanics is a stretch but arguable.

> **My recommendation: cap the tree below metal, at least at launch.** The endgame of this game should be a thriving multi-generational settlement that has learned to read its valley — not an anvil. Metal is the reflex answer and it's the least interesting direction available. Offer it as a server-configurable, clearly-labelled optional module for groups that want it, and make the *default* endgame social and ecological mastery. See [08](08-open-questions.md).

## 4. Knowledge as a physical object

The replacement for a tech tree UI:

- **You know what you have done.** Skills are per-character and practice-based.
- **Knowledge can be recorded** — ochre pigment on a rock face, notches on a tally stick, a carved bone plaque, arrangements of objects. These are *world objects*. They can be found by strangers, and they can burn.
- **Knowledge can be taught** — demonstrating a technique near another player transfers it (see [05](05-society-and-multiplayer.md)).
- **Knowledge dies.** If the only person who knew how to fire pottery dies and never recorded or taught it, that knowledge is gone from the world until someone rediscovers it.

In solo play this becomes quietly devastating: you are recording things for a successor who may never come. In multiplayer it makes elders genuinely valuable, and it makes a burned record hall a catastrophe with no combat attached to it.

## 5. The Codex

The in-game reference, and the game's actual soul.

- Fills in **as you observe**, not as you unlock. Watch a *Sinocalliopteryx* eat and the entry updates.
- Every entry shows its **confidence tier** and, for the curious, its **real citation**.
- Entries note **what we don't know** explicitly: *"Vocalisation unknown. No fossil evidence exists for the sounds this animal made. What you hear is our reconstruction."*
- Ships with a **changelog** so science patches are visible.

The codex is how this game earns a museum partnership, a classroom edition, and the goodwill of every paleontologist on the internet — which is, commercially, worth more than any amount of marketing spend.
