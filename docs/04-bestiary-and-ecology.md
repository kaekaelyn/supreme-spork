# 04 — Bestiary & Ecology

The full machine-readable roster lives in [`data/species.yaml`](../data/species.yaml). This document covers the design thinking: how the ecosystem simulates, how animals behave, and the two ideas that make this game's wildlife different from every other survival game.

---

## 1. Pillar: naive fauna

**Nothing in the Yixian has ever seen a primate.**

At world generation, every animal population has a **fear-of-human** value of approximately zero. A *Jeholosaurus* will let you walk up to it. A *Confuciusornis* will not leave its perch. A *Repenomamus* will investigate you with curiosity rather than caution. The first hours of the game are, genuinely, Eden — and the player will feel like they have found an exploit.

Then the simulation learns.

Fear is a **per-population, per-region, heritable trait** that increases when members of that population are killed, injured, or harassed by humans **and witnessed by survivors**. It decays slowly over generations if you leave them alone. It propagates through social species faster than solitary ones. It is inherited by offspring.

The consequences are enormous and entirely emergent:

- The valley you hunted out in year one is *still* skittish in year four.
- A newly-explored valley is naive again — which creates a real, non-arbitrary reason to expand, and a real cost to over-exploiting home range.
- Hunting **cleanly and unwitnessed** — from ambush, at distance, taking stragglers — keeps a population approachable. Hunting sloppily makes your own life permanently harder. The game never says this. You just notice, around year two, that everything runs from you now, and that you did that.
- In multiplayer, one careless player degrades the hunting for everyone, which generates *real* social norms about hunting conduct with zero mechanical enforcement.

This is the game's story, and it is told without a single line of narration. It is also, not incidentally, the actual story of *Homo sapiens* arriving anywhere.

## 2. Pillar: two-tier ecological simulation

The technical foundation for all of the above. Fully detailed in [06](06-technical-architecture.md), summarised here because it's an ecology decision as much as an engineering one.

- **Coarse tier.** The whole map is divided into ecological cells. Each cell holds *populations as numbers* — biomass, age structure, fear value, health, food availability — updated on a slow tick (seconds to minutes of real time). Predation, breeding, migration, starvation, and disease all run here, everywhere, always, whether or not a player is present.
- **Fine tier.** Near players, populations are **materialised** into actual agents with full AI, animation, and physics. When the player leaves, agents **de-materialise back into numbers**, carrying their state with them.

What this buys us:

- **You can genuinely deplete a species.** Over-hunt a valley and the numbers go down and stay down. There is no respawn timer. There is only reproduction, and reproduction needs a breeding population.
- **Predators respond.** Kill all the *Psittacosaurus* in your valley and the *Sinocalliopteryx* that ate them either starve, leave, or start looking at your settlement and your livestock as the remaining option. Predator pressure on your base becomes a *consequence of your own choices* instead of a spawn table.
- **Migration and seasonal movement** are free. Herds move to where the food is because the sim says so.
- **The world keeps living while you sleep**, and on a dedicated server, while everyone is offline.

## 3. Behaviour design rules

- **Most animals want nothing to do with you.** The default response to a novel large biped is investigation, then avoidance.
- **No aggro radius.** Animals have needs, senses, and states. A predator attacks because it is hungry and you look manageable, or because you are near its nest or kill.
- **Vocalisation is tier C and labelled as such.** We have no fossil evidence for dinosaur sounds. Our reconstructions should be based on phylogenetic bracketing — the syrinx is a late avian innovation, so most of these animals likely produced **closed-mouth vocalisations, booms and hisses** rather than birdsong. The codex says so.
- **Display behaviour is where tier C earns its keep.** *Confuciusornis* had elongated tail streamers present in only some specimens — near-certainly sexual display. We can build gorgeous, entirely defensible courtship behaviour on that.
- **Feathers do things.** Fluffing for insulation in cold, flattening in heat, raising in threat display, shaking off snow, preening, dust-bathing. A feathered animal that never behaves like a feathered animal is a wasted opportunity, and this stuff is cheap animation with enormous payoff.
- **Nesting and parental care.** Brooding posture in oviraptorosaurs is tier B and directly attested in relatives. Nests are a resource (eggs) with a consequence (angry parent, and a hit to the population number).

## 4. Roster highlights

Full list in [`data/species.yaml`](../data/species.yaml). These are the ones with the strongest design hooks.

### Predators and threats

**Yutyrannus huali** — ~9 m, ~1,400 kg, and covered in filamentous feathers up to 15–20 cm long. The largest feathered animal ever found. **This is not a mob.** One *Yutyrannus* in your region is a regional event with a name, a territory, and a set of habits you learn or die to. Never more than a handful on a map. It should be encountered maybe three times in an in-game year and each time should be terrifying.

**Sinocalliopteryx gigas** — ~2.4 m compsognathid. Preserved gut contents show it ate ***Confuciusornis*** **and** ***Sinornithosaurus*** — this is tier A, a directly fossilised food web. A serious, realistic threat to a lone human. The primary "you are being hunted" animal.

**Sinornithosaurus millenii** and **Zhenyuanlong suni** — feathered dromaeosaurs. Dangerous, not invincible. Pack coordination is **tier D** and should scale with the Speculation setting rather than being assumed.

**Dilong paradoxus** — ~1.6–2 m basal tyrannosauroid. Mid-tier threat, and a wonderful piece of context when the player later meets *Yutyrannus* and realises what it's related to.

**Repenomamus robustus / giganticus** — badger-sized carnivorous mammals, up to ~1 m, with **fossilised gut contents containing a juvenile** ***Psittacosaurus***. Not a threat to an adult human. A *massive* threat to your food stores, your eggs, your hatchlings, and your livestock. The best antagonist in the game precisely because it is small: it makes storage architecture matter.

***Pseudopulex magnus*** — the 23 mm flea. See [02 §5](02-survival-systems.md#5-disease-injury-and-parasites). Your most persistent enemy and it cannot be fought, only managed.

### Prey and resources

**Psittacosaurus lujiatunensis** — the workhorse herbivore. Countershaded for **closed forest**, dark chin and jugal bosses, disruptive patterning. Herding, alert, fast. The staple large-ish prey and the first domestication candidate.

**Jeholosaurus shangyuanensis** — small, fast bipedal ornithischian. Common small game. The second domestication candidate and probably the better one.

**Liaoceratops yanzigouensis** — small basal ceratopsian. Prey, and a lovely thing to just *watch*.

**Jinzhousaurus yangi** / **Bolong yixianensis** — large iguanodonts, ~7 m. Enormous calorie payoff, enormous danger, and the realistic source of **large-animal sinew** — the cordage bottleneck breaker. Bringing one down should be a group project or an elaborate trap.

**Dongbeititan dongi** — sauropod. Effectively unkillable and effectively terrain. Its presence changes the forest — trampled paths, browsed canopy, dung that supports an insect economy. Use it as a living landscape feature.

**Sinosauropteryx prima** — ~1 m, **ginger and white banded tail, bandit mask, countershaded for open habitat**. Our best-documented colour and therefore our poster animal. Put it on the box.

**Caudipteryx zoui** — ~1 m oviraptorosaur with a genuine tail fan and preserved gastroliths (so, herbivorous or omnivorous). Approachable, beautiful, and the best display-behaviour showcase in the roster.

**Confuciusornis sanctus** — abundant, beaked, sexually dimorphic with long paired tail streamers in some individuals. Known from **mass-mortality assemblages**, which suggests large flocks and gives us the single most spectacular set-piece in the game: a colony roost of thousands. Also a food source, a feather source, and a *down* source.

**Lycoptera davidi** — the small fish that is overwhelmingly the most abundant vertebrate in the whole formation, and an index fossil. **This is the player's protein floor.** Shoaling, seasonal, catchable in quantity with nets and weirs. Rendered for oil, this is what actually keeps you alive.

**Hyphalosaurus lingyuanensis** — ~1.5 m long-necked aquatic choristodere with 18+ neck vertebrae. Aquatic prey, and a genuinely alien-looking animal.

**Manchurochelys manchoukuoensis** — turtle. Food, and a shell, which is a container, which matters before ceramics.

**Protopsephurus** — a paddlefish. Large-bodied river fish. A real prize.

### The invertebrate economy

The Yixian saw the **largest insect diversification of the entire Mesozoic**. The classic index assemblage is **EEL** — *Ephemeropsis* (mayfly), *Eosestheria* (clam shrimp), *Lycoptera* (fish) — and all three are absurdly abundant.

- **Mass mayfly emergences** — a seasonal spectacle, a fish-feeding frenzy, and a genuine human food source (insect emergences were harvested by real people).
- ***Eosestheria*** clam shrimp — ephemeral pool blooms, edible, tedious, reliable.
- ***Coptoclava longipoda*** — a large predatory aquatic beetle larva. Unpleasant surprise in the shallows.
- **Long-proboscid scorpionflies** (*Mesopsyche*, *Vitimopsyche*) — Mesozoic gymnosperm pollinators. Pure ambience, and a detail that will make entomologists lose their minds with joy.
- **No honeybees.** No honey. Flag it in the codex, because the absence is itself interesting.

## 5. What the player can't do

- **You cannot tame anything by feeding it three berries.** There are no berries.
- **You cannot ride anything.** Nothing here is built for it, and nothing has been bred for it. A hard no.
- **There are no boss fights, no healthbars, no loot drops.** An animal you kill is a carcass with anatomy: hide, sinew, meat by cut, organs, fat, bone, feathers, gut. Butchery is a skill and a time investment, and you will not carry it all.

## 6. Domestication

The real long-game progression, replacing the standard "tame → mount → fight."

It is **multi-generational and it is slow**:

1. **Raid a nest.** Steal eggs or hatchlings. The parents object. The population number drops.
2. **Imprint.** Hatchlings raised from egg by a human imprint on humans. This is real bird biology and applies straightforwardly to non-avian dinosaurs.
3. **Raise.** They need food you can barely spare and shelter you'd rather use yourself, through a winter.
4. **Breed for docility.** Over generations, select for tractability, size, growth rate, or feather quality. Traits are heritable with real variance. This is *actual* animal husbandry.
5. **Payoff, eventually.** *Jeholosaurus* and *Psittacosaurus* as a meat, egg, and hide herd — a food supply that survives a bad hunting season. Not a mount. Not a weapon. **Food security**, which in this world is the only thing that matters.

**Repenomamus** deserves special mention: a badger-sized carnivorous mammal that raids your stores is *exactly* the profile of the animals humans actually domesticated first — commensal scavengers drawn to our rubbish. A tamed *Repenomamus* as a ratter and guard, arrived at over generations by tolerating the ones that were least aggressive, is both a lovely mechanic and a scientifically-literate joke about how cats happened.

## 7. Animal density and pacing

An honest warning: **a realistic Cretaceous ecosystem is emptier than players expect.** Real predator densities are low. Real herbivore encounters are occasional. If we simulate honestly, the map will feel sparse to anyone raised on Ark.

I think we hold the line, with two mitigations:
1. **Density is a server setting** with a clearly-labelled "Naturalistic" default and a "Populous" option that is honest about being a gameplay concession.
2. **We make sparseness good** by making every encounter matter, making tracking a deep skill, and making the invertebrate/fish/bird layer dense enough that the world never feels *dead* — just not full of large animals. Real forests are like this. They are not boring; they are quiet, and the quiet is what makes the *Yutyrannus* work.
