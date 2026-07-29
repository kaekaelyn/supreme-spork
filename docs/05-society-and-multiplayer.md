# 05 — Society & Multiplayer

The brief: *no traditional story, but plenty of room for roleplaying and building a new little human tribe or society, built and customised from the ground up.*

The design answer: **provide no culture, and provide excellent tools for making one.** Every system here is a blank instrument. The game ships zero lore, zero names, zero rituals, zero factions. It ships the *materials* from which players make those things, and it makes them mechanically load-bearing so they aren't merely decorative roleplay.

---

## 1. The three modes

| Mode | Design intent |
|---|---|
| **Solo** | You are alone. Genuinely, permanently alone. No NPC humans — introducing them would demand an explanation the premise refuses to give. The society layer becomes about **the record you leave for a successor who may never come.** This is melancholy and it is the best version of solo survival I can imagine for this setting |
| **Co-op (Internet)** | 2–8 typical, up to ~16. Dedicated server binary, or host-and-play. This is the game's heart |
| **LAN** | Same server binary, discoverable on the local network, fully offline-capable. First-class, not an afterthought |

### The Legacy feature (solo's secret weapon)

A solo world can be **seeded from another player's abandoned world file**. You arrive at somebody else's ruins: their collapsed longhouse, their middens, their glyphs on the rock face, their tally sticks, their graves, their half-finished stone alignment on the ridge. You cannot talk to them. You can only read what they left and try to work out what happened.

This is asynchronous multiplayer, it costs almost nothing to build on top of world serialisation we need anyway, and it is the most evocative feature in this entire document. It also creates a self-sustaining community economy of shared world files.

## 2. Writing: the glyph system

**Players invent their own writing.** This is the cornerstone of emergent culture.

- A **stamp/stroke set** of primitive marks — lines, arcs, dots, chevrons, hand stencils, hatching — that players combine into glyphs on a small grid.
- Glyphs are applied with **ochre**, **charcoal**, **incision**, or **relief carving** onto rock faces, posts, bark, hide, bone, and pottery.
- A glyph carries **no inherent meaning**. Meaning is a social convention the players establish and must teach each other.
- Glyphs can be **bound to functions** by the community: a trail marker that appears on your map only if you have been taught what it means; a territory post; a warning; a name.
- **Personal marks.** Every player designs a signature glyph. It appears on things they make. A well-made tool carries its maker's mark forever, and after that player is gone, a stranger finds a beautiful spear point with a mark they don't recognise.

Nothing in this system is authored by us. A server's writing system is entirely its own, is unintelligible to outsiders, and can be **lost**.

## 3. Teaching, skill, and death

- **Skills are practice-based and per-character.** You get better at knapping by knapping.
- **Teaching is a mechanic.** Perform a technique in proximity to another player who is watching, and they gain a large learning bonus toward it. Explicitly demonstrating is far faster than independent discovery.
- **Death is not a respawn.** Your character is gone. You return as a **new person** — no explanation, same as the first one — with none of your predecessor's practised skills.
- **What survives you** is what you recorded (glyphs, marked tools, the physical record) and what you taught (skills now living in other players' characters). 

This makes elders valuable, teaching load-bearing, and knowledge a genuine communal asset. It converts "the veteran player" from a mechanical advantage into a **social role**. And it means a server's culture is a real, fragile, accumulated thing that can actually be destroyed — by a bad winter, a fire, or a well-executed betrayal.

Solo players get the same rules, which is where it gets quietly brutal: everything you know dies with you unless you wrote it on a rock.

## 4. The Record

An automatically-maintained world chronicle, presented **diegetically as strata**.

Significant events — first fire, first pot fired, first winter survived, first death, first *Yutyrannus* sighting, the year of the ashfall, the founding of a hearth — are logged with their date in world-years and the people involved. It is viewable in-game as a layered cross-section, like a geological column of your own settlement's history.

It is also the game's **anti-griefing infrastructure**, because it makes actions attributable without any reputation UI. And it is a phenomenal share/export artefact: an auto-generated illustrated history of your server, suitable for posting.

## 5. Territory, property, and conflict

**No claim flags. No clan menus. No ownership toggles.**

- **A hearth defines a home.** A lit, maintained hearth marks a place as occupied. Hearths are visible at distance by smoke — which means your settlement's presence is *inherently* broadcast, and hiding requires giving up fire.
- **Property is possession plus social convention.** The game does not stop you taking things. The Record notes that you did.
- **PvP is server-configurable**, defaulting to **off** for public servers and **on** for private ones. Given the premise, I'd resist making PvP the centre of gravity — the environment is a better antagonist than other players, and the co-operative pressure of a shared winter is where the good stories are.
- **Interdependence is the real social engine.** Broken bones need a carer. Large game needs a group. A firing needs someone tending while you sleep. The systems generate need, and need generates society. Nothing needs to enforce it.

## 6. Roleplay affordances (all mechanically neutral, all optional)

Things players can make that the game gives shape to but no meaning:

- **Burial and graves.** A body can be interred, cairned, or marked. Graves persist and are recorded. Grave goods stay with the grave.
- **Ornament.** Feather work, beadwork from bone and stone, pigment body-marking, scarification, tattooing with ochre and needle, teeth and claw pendants. Deep customisation with real material costs. In a world where everything is feathered, **feather ornament is the natural aesthetic language** and we should invest heavily in it.
- **The calendar.** Solstice-marking alignments are *mechanically useful* (predicting winter's onset), which means observatories get built for practical reasons and then become ceremonial, which is exactly how it happened in reality.
- **Fire ceremony.** A hearth that has never gone out has a tracked continuous-burn duration. That number is meaningless mechanically. Players will care about it enormously.
- **Naming.** Places, animals (a specific *Yutyrannus* individual can be named, and the name propagates through the Record), tools, people.
- **Music.** Bone flutes are real, ancient, and buildable here. Percussion, whistles, drums from hide. Let players actually play them.

## 7. Multiplayer-facing technical constraints

Detailed in [06](06-technical-architecture.md), but the design-relevant summary:

- **Authoritative dedicated server.** The ecology sim must be server-side and single-source-of-truth.
- **The world persists while offline.** The coarse ecology tier keeps running on a dedicated server. Come back after a week and the season has changed and the herds have moved. This is a feature, and it needs to be a clearly-communicated server setting because some groups will hate it.
- **Proximity voice**, positional, with no global chat by default. In a game about teaching and shared knowledge, being unable to coordinate at distance is a *feature* — and it makes trail glyphs and signal fires genuinely necessary.
- **Small player counts, deep simulation.** We are not building a 100-player persistent shard. 16 players in a deeply simulated valley is far more interesting than 200 in a shallow one, and it's achievable.
