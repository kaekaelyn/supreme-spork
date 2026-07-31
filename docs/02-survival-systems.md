# 02 — Survival Systems

Design principle throughout: **no needs bars on the HUD by default.** The body reports its own state. You shiver, your breath fogs, your hands get clumsy and drop things, your vision narrows, your character starts making involuntary noises. An optional numeric overlay exists in accessibility settings, but the intended experience is diegetic.

---

## 1. Thermal model — the primary killer

You wake up naked, at roughly 7 °C mean annual temperature, at altitude. **The clock starts immediately.** The most likely first-session death is hypothermia, not predation, and that is correct and intentional.

We model heat as a budget, not a bar:

**Core temperature** is driven by metabolic heat production minus losses to conduction, convection, radiation, and evaporation. Inputs that matter:

- **Air temperature** — diurnal swing is large at altitude. Clear nights are brutally colder than overcast ones because radiative loss to a clear sky is real and models nicely.
- **Wind** — the single most underrated killer. Windchill on the exposed ridges versus stillness in the forest understory is a legible, learnable map feature. Reading terrain for shelter is a skill.
- **Wet** — being wet multiplies conductive loss catastrophically. Falling in a lake in autumn should be a genuine emergency with a countdown measured in minutes. Rain without shelter is a slow version of the same.
- **Insulation** — clothing R-value by layer and coverage zone (head, core, hands, feet). **Down is the best insulator available**, and it is everywhere in this world, which is the material identity of the whole game.
- **Activity** — running generates heat but also sweat, and sweat in the cold is a trap. Sprinting to stay warm and then stopping is how you die. This is real, it's non-obvious, and learning it is a great player moment.
- **Fuel** — food in the stomach. Digestion is thermogenic. You cannot stay warm while starving.
- **Shelter thermal mass** — a stone-and-earth structure holds heat for hours after the fire is out. A brush lean-to does not. This makes the building progression *mean* something physically.

**Hypothermia stages**, all shown behaviourally: shivering → clumsy hands (crafting failures, dropped items) → confusion (HUD/UI degradation, unreliable compass sense) → paradoxical undressing (the character begins removing clothing, which is a real terminal symptom and the most horrifying possible mechanic) → death.

**Frostbite** on extremities is permanent. Lose fingers, lose crafting speed and grip. Lose toes, lose sprint. This should be rare and avoidable, but it should be permanent, because permanent consequences are what make cold *frightening* instead of annoying.

**Heat stress** exists in high summer but is a minor system. Don't over-invest.

## 2. Nutrition — the Carbohydrate Problem, mechanised

See [01 — The Science §4](01-the-science.md#4-the-flora--and-the-carbohydrate-problem) for why this world has no grain, fruit, or tubers.

We track **four** nutritional axes rather than one hunger bar:

| Axis | Runs out in | Failure |
|---|---|---|
| **Energy (kcal)** | Days | Weakness, then starvation |
| **Fat** | Weeks | **Protein poisoning** — nausea, weakness, diarrhoea, death despite a full stomach |
| **Protein** | Weeks | Muscle wasting, poor healing |
| **Micronutrients** | Months | Scurvy (vitamin C), beriberi (thiamine, from fern reliance), B6 deficiency (from ginkgo reliance) |

**Protein poisoning is the signature mechanic.** A player who successfully hunts every day and eats nothing but lean meat will die, slowly, with a full belly, and the game will not explain why. The fix — eat fat, eat organs, render marrow — is discoverable and historically exactly right. It reframes hunting: a lean winter *Jeholosaurus* is nearly worthless; an autumn one carrying fat is a windfall.

**Vitamin C** without fruit is the other elegant trap. Historical answers all exist here: **fresh organ meat, especially liver and adrenal tissue, eaten raw or lightly cooked**, and **conifer needle infusion** — spruce/pine needle tea is a genuine, well-documented antiscorbutic. So the counter to scurvy is *conifer tea*, in a conifer-dominated world. Perfect.

**Toxin load** is a fifth hidden axis:
- Cycad/bennettitalean seeds carry **cycasin**; leaching in changed water over days removes it. Insufficient leaching accumulates liver damage.
- Ginkgoalean seeds carry an antivitamin-B6 compound; safe in moderation, dangerous as a staple.
- Ferns and horsetails carry **thiaminase**; cooking destroys most of it, chronic raw intake causes beriberi.

None of this is explained anywhere. You will work out that the seeds are making you ill by getting ill, and the only place that knowledge can be stored is [your own written record](05-society-and-multiplayer.md#3-writing-two-layers).

### Seasonal food calendar

This is the spine of the game year.

| Season | Available | Strategy |
|---|---|---|
| **Spring** | Conifer cambium (sap-flow window), fiddleheads, horsetail shoots, fish spawning runs, eggs and nestlings | Recovery. The cambium window is a few weeks and missing it hurts |
| **Summer** | Fish, insects, small game, greens, first seeds | Surplus. Build, tan hides, dry meat |
| **Autumn** | **Fat animals**, mass seed drop (ginkgo, cycad, conifer), migrating birds | The only season that matters. Everything is preparation for or recovery from winter |
| **Winter** | Almost nothing. Ice fishing, cached food, lean game | Survive. Frozen lakes become walkable terrain and free refrigeration |

**Winter freezes your larder for free.** Meat cached outdoors keeps indefinitely once the frost sets. Spring thaw ruins it. That single interaction generates an entire strategic rhythm without any UI.

## 3. Water

Lakes, streams, snowmelt, and springs. But this is a **volcanic basin**, so:

- Some water is **alkaline or CO₂-charged** and boiling does not help. Mineral toxicity is a learn-the-map problem, not a process-it problem.
- **Ashfall fouls open water** for days to weeks after an eruption — a real, well-attested effect. Covered cisterns become valuable infrastructure.
- **Hot springs** are usable: cooking without fuel, warmth, washing, and hide processing. They are also the most contested resource on any multiplayer server, which makes them natural social flashpoints without any faction system.
- **In winter, water costs fuel.** Melting ice and snow burns wood. This is a genuine and rarely-modelled constraint that makes winter fuel logistics bite twice.

## 4. Fire

**Volcanism gives us a beautiful onboarding affordance:** before you can *make* fire, you can *fetch* it. Fumaroles, hot ground, and geothermal vents let a naked player carry an ember home on day one. Fire-by-friction comes later, requires the right woods and dry tinder, and fails in rain — which makes the ember-carrying skill remain relevant forever.

Fire needs: fuel (wood by species and dryness), airflow, shelter from rain and wind. Smoke needs to go somewhere — an unvented interior fire is another silent killer with no monster attached. Fire is also **light** and **predator deterrence** and **meat preservation** and **ceramics** and **warmth**. It's the hub of the whole tech graph and should feel like it.

## 5. Disease, injury, and parasites

The parasite system is where the setting gives us something no other survival game has.

***Pseudopulex magnus*** **is real.** A flea from the Yixian, nearly **23 mm long** excluding antennae — around ten times the size of a modern flea — with elongated, serrated piercing stylets. A related Jehol species preserves a distended abdomen from a blood meal estimated at ~15× what a modern flea takes. These animals fed on feathered and haired vertebrates.

They will also feed on you.

**Parasite load accumulates with sedentism.** The longer you occupy a site, the more bedding you accumulate, the more hides you store, the more animals you keep, the worse the infestation gets. Countermeasures are historically accurate and pleasing: smoke, freezing bedding outdoors in winter, moving camp seasonally, burning old bedding, and aromatic conifer boughs. This creates a *genuine strategic tension against permanent settlement* — the thing every other survival game pushes you toward unconditionally — and it does so from a real fossil.

Other health systems:

- **Wound infection.** Injuries introduce infection risk scaled by wound type and hygiene. Treatment is limited: heat, cleaning, and a small pharmacopoeia of genuinely-present plants (conifer resin is antimicrobial and was used this way; *Ephedra*-relatives are stimulants and bronchodilators — plausible, and worth including, but the effect should be discovered rather than signposted).
- **Broken bones.** Splint and immobilise. Weeks of reduced capability. In multiplayer, this makes you **dependent on other players**, which is the best possible way to generate social structure without a social mechanic.
- **Smoke inhalation, falls, drowning, cold-water shock, H₂S in hollows.** The environment should be more lethal than the animals, always.

## 6. Time — the 1:1 world clock

**The world clock runs in real time and never stops.** This is the game's most consequential decision after the climate, and everything else bends around it.

### The numbers

| Quantity | Value | Where it comes from |
|---|---|---|
| **Day** | **23.4 real hours** | Tidal deceleration of ~1.7 ms/century over 125 Myr |
| **Year** | **~374.6 days**, which is **one real year** | Orbital period is essentially unchanged; 8,766 h ÷ 23.4 h |
| **Lunar month** | **~30 days** | Moon ~0.3% closer, so a marginally shorter synodic month |
| **Winter** | **~4–5 real months** below freezing | 42° N paleolatitude at ~2,000 m |

A season lasts as long as a season lasts. A settlement that has stood for three real years has survived three real winters, and everyone who was there knows it.

### The precession gift

This is the detail that makes 1:1 viable, and it falls out of the science for free.

Because the Cretaceous day is 23.4 hours and the real day is 24, **in-game time-of-day drifts against real time by about 37 minutes per real day.** A player who only ever plays 8pm to midnight is *not* locked into perpetual darkness: their slice of the world's day slides steadily earlier, completing a full circuit through dawn, noon, dusk, and midnight roughly **every 40 real days.**

The single worst failure mode of a real-time clock — your schedule pinning you to one time of day forever — is solved by an accurate scientific detail, at no design cost. Do not round the day to 24 hours. The 0.6-hour difference is doing enormous work.

### The 374-day trap

**You know a year is 365 days. It is not.** It is about 374.6.

A player who builds a calendar on 365 will drift by ten days a year, misjudge the solstice, and be wrong about when winter starts — which, in this game, is a way to die. The only fix is to measure the year yourself, by watching where the sun rises against the ridge line across a full cycle. Nothing hints at this. It is the purest expression of [the Transplant pillar](00-the-transplant.md#1-knowledge-without-skill): your modern knowledge is not just insufficient, it is *actively wrong*, and you have to catch it.

This is also why a stone alignment on the ridge is a real piece of infrastructure rather than decoration.

### The long night

Nights average 11.7 real hours and run to about **14.7 hours in midwinter**. That is not a problem to be minimised; it is a design brief.

*(The 42° N paleolatitude of [01 §1](01-the-science.md#1-when-and-where) and a 23.44° axial tilt fix midwinter night length exactly, at 14.69 h — closer to fifteen hours than sixteen. An earlier draft of this document said "fifteen or sixteen"; the computed figure supersedes it. Sixteen-hour nights would need something closer to 50° N.)*

- **Night is when handwork happens.** Knapping, sewing, cordage, fletching, hide scraping, cooking, rendering, tending the fire, teaching, and writing are all *better done by firelight* — some of them exclusively so. This is historically exactly what people did all winter, and it means an eleven-hour night in a warm shelter is the most productive part of the day rather than dead time.
- **Night is the social season.** In co-op, night is when everyone is at the hearth. That is where teaching happens, where the record gets written, and where whatever your settlement believes gets decided.
- **Snow has albedo.** A snow-covered landscape under a gibbous moon is genuinely navigable. Moonless nights are the dark ones, which gives night a ~30-day rhythm and makes the lunar cycle worth tracking — another reason to build a calendar.
- **Without fire and shelter, night is simply survival**, in real time, for eleven hours. Early game this is exactly as brutal as it sounds. It should be.

### Sleep, absence, and the always-on server

**The server clock never skips.** There is no sleep-to-morning button, because with other players awake there is nothing to skip to.

So on a dedicated server, **logging off is how you skip time.** Your character sleeps, or doesn't, in whatever state you left them. This reframes the entire build progression: a shelter is not just warmth, it is **absence insurance**. What happens to your body, your stores, and your animals while you are not there is the real question that base design answers. Come back after four days and the season has moved, the fire is out, something has been at your cache, and the herd has gone somewhere else.

- **Solo** pauses the world on quit by default, with an option for a persistent clock for people who want it.
- **Dedicated servers** run continuously by default. This is a server setting, because some groups will want the world to pause when everyone is offline, and that should be their call.
- **Sleep in-world** is still a mechanic — it restores fatigue and burns time you were going to spend anyway — but it is 1:1, so it is something you do because you're tired, not to skip content.

### Starting, joining, and dying

- **A new server picks its founding date.** Start in spring if you want a chance, or in November if you want the other thing.
- **New arrivals get whatever season it currently is.** No easing, no grace period. Joining an established settlement in deep winter is a hard way to arrive, and how the settlement handles that is [their business](05-society-and-multiplayer.md#2-arrivals).
- **Death in February means restarting in February.** With permadeath on, this is savage, and it is the intended shape of the game.

### The honest caveat

1:1 asks for a real commitment, and some groups won't have it. The concession lever, if a server needs it, is a **year-rate multiplier that leaves the day at 1:1** — seasons pass 2× or 3× faster while a day is still 23.4 real hours. It costs astronomical coherence (the sun's declination changes faster than the day count justifies, and a carefully built calendar stops agreeing with the sky), and the setting should say so plainly. **The default, and the intended game, is 1:1.**

## 7. Snow and ice

- Snow **accumulates and persists** by depth, with real consequences: movement cost, structural load on roofs, and **tracking** — footprints in snow are an enormous gameplay layer, because you can track animals *and be tracked*, by animals and by people.
- Lakes freeze progressively: thin ice that breaks, then walkable ice, then thick ice you must chop through to fish. Frozen lakes are new traversal and free refrigeration. **Spring break-up is dangerous and dramatic**, and it ruins every cache you left out on the ice.

## 7. What we deliberately do not include

- **No stamina bar governing sprint.** Use a fatigue/sleep model instead; it interacts with cold and food and is more interesting.
- **No thirst bar ticking constantly.** Water is a logistics problem, not a nag.
- **No enemy waves, no raids, no scripted attacks on your base.** Predators have ecological reasons for approaching, or they don't come.
- **No "hardcore mode" toggle.** The difficulty settings that exist should adjust the *environment* (year length, winter severity, animal density), not add or remove systems.
