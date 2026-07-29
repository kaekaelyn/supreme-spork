# 01 — The Science

This is the paleontological bible. Every other document defers to this one. Citations live in [SOURCES.md](../SOURCES.md).

---

## 1. When and where

**The Yixian Formation**, western Liaoning Province, northeastern China. Barremian to early Aptian, Early Cretaceous. Radiometric and Bayesian age modelling puts the whole formation at roughly **125.8–124.1 Ma**, and — this is remarkable — the entire fossiliferous sequence may span **less than about 93,000 years**. The Yixian is not a slow accumulation. It is a snapshot.

The Yixian is the middle of the three phases of the **Jehol Biota**. Below it sits the Huajiying/Dabeigou Formation; above it, the Jiufotang Formation (~120 Ma). This distinction matters enormously for our species list — see §7.

Paleolatitude was roughly **42° N**, comparable to modern Beijing, northern Spain, or Chicago.

### Members and beds

The formation is divided into units, and the ones with names you'll encounter are:

- **Lujiatun** — lowest. The famous **three-dimensional** preservation, animals buried in life posture. Long nicknamed "Chinese Dinosaur Pompeii," but see §3.
- **Jianshangou** — laminated shales, mudstones, tuffs, coal seams. Flattened, feather-preserving specimens. 230–420 m thick.
- **Dawangzhangzi** and **Jingangshan** — further lake-deposit horizons with exceptional preservation.

Different units preserve different assemblages. This is a **gift to level design**: it justifies distinct sub-biomes with genuinely different species mixes, without inventing anything.

## 2. Climate — the single most important fact

Two independent lines of evidence, both pointing cold:

- Oxygen isotope work on East Asian dinosaur remains gives mean air temperatures around **10 ± 4 °C** for the Yixian at ~42° N — "cool temperate," comparable to modern midlatitude conditions.
- Clumped-isotope analysis of paleosol carbonates at **Sihetun** gives a mean annual paleotemperature of **5.9 ± 1.7 °C** and a paleoelevation of **2.8–4.1 km**, implying a high-altitude habitat with **frozen winters**.

Supporting evidence: fossil insect groups present (Raphidioptera, Siberioperlidae stoneflies) are cold-adapted montane taxa implying alpine lakes and streams at 800 m or more. Fossil wood and palynology show conifer-dominated forest, with bisaccate conifer pollen exceeding 70% of the palynomorph assemblage. Lacustrine varves show **strong seasonality** — light carbonate laminae from warm-season photosynthetic precipitation alternating with dark siliciclastic laminae from cold-season terrestrial input. Rainfall alternated between semi-arid and mesic.

> **Design ruling.** We model the world at roughly **7 °C mean annual temperature**, seasonally swinging from about **−20 °C in deep winter to 24 °C in high summer**, at an elevation treated as **~2,000 m** in a basin floor rising to ~3,000 m on the volcanic ridges. This sits inside the plausible envelope of both studies, gives us snow, ice-over on lakes, and a genuine growing season. The uncertainty is disclosed in the published dataset and design notes — never in the game.
>
> **Implementation note.** These three figures cannot come from a symmetric seasonal curve: the midpoint of −20 °C and 24 °C is +2 °C, not the stated +7 °C mean. They are only mutually consistent under a **cold-skewed distribution** — deep winter dipping much further below the mean than high summer rises above it. That asymmetry is not a fudge; it is genuinely how a high continental basin behaves, driven by winter-specific mechanisms with no summer analogue (strong radiative inversions under clear, calm, snow-covered winter nights, versus summer maxima capped by evaporation and convection). Any implementation should be built cold-skewed from the start rather than tuned toward a symmetric model and then forced — see [10 §7](10-stage-1-greybox.md#7-three-places-the-design-documents-disagree-with-themselves) for the Stage 1 model that measures 7.0 °C / −17.6 °C / 23.4 °C against a full simulated year.

This is the design decision the entire game hangs on. It is also the single biggest departure from audience expectations, and we should lean into it hard in marketing. **Feathers are insulation. This is why.**

## 3. Volcanism — and a correction we should get right

The Yixian is a volcanic basin. Lithology is basalt, andesite, tuff, tuffaceous sandstone, rhyolitic pyroclastics, shale, mudstone, siltstone, and conglomerate, 225 to 4,000 m thick depending on where you measure. Small, low-energy lakes sat between frequent eruptions.

**The "Pompeii" story is out of date and we should not build the game on it.** The popular telling — animals flash-killed and buried by pyroclastic surges — has been substantially overturned. Work published in 2024 concludes that the Lujiatun animals were **not** killed by airborne volcanic ash in single catastrophic events, but buried by **multiple flood events carrying heavy volcaniclastic debris loads**, and that the Yixian as a whole represents "a brief snapshot of normal life and death," not a series of catastrophes.

> **Design ruling.** Volcanism is **ambient pressure**, not a scripted apocalypse. Ashfall fouls water and kills vegetation but fertilises soil. Lahars and debris flows follow drainages after heavy rain — *these* are the killers, and they are predictable if you learn to read the land. Rare limnic overturn events kill lake fish en masse: a protein bonanza and an omen. A "volcanic winter" year is a rare catastrophic event on a multi-year timer. Hot springs and fumaroles are usable infrastructure. Low hollows near fumaroles accumulate **H₂S and CO₂** — an invisible, silent, entirely real killer, and one of the best hazards in the game precisely because it has no monster attached.

## 4. The flora — and the Carbohydrate Problem

Forests dominated by **conifers**, with **ginkgoaleans**, **czekanowskialeans**, **cycads**, and **bennettitaleans**. Ground cover was **ferns, horsetails, lycopods, and mosses**. Represented groups: Bryophyta, Lycopodiales, Equisetales, Filicales, Pteridospermae, Cycadales, Bennettitales, Ginkgoales, Czekanowskiales, Coniferales, Gnetales (ephedroids), and — barely — Angiospermae.

**Flowering plants were essentially absent.** Around five species are known: three of *Archaefructus* (an aquatic herb), *Hyrcantha* (= *Sinocarpus*), and *Archaeamphora* — and *Archaeamphora* was shown in 2015 **not** to be a carnivorous angiosperm at all, and probably isn't an angiosperm. We exclude it.

Named genera to draw from:

| Group | Genera |
|---|---|
| Conifers | *Liaoningocladus*, *Podozamites*, *Elatides*, *Schizolepis*, *Pityostrobus*; Cheirolepidiaceae (*Classopollis* producers), Pinaceae, Taxodiaceae, Araucariaceae |
| Ginkgoales | *Ginkgoites*, *Baiera*, *Sphenobaiera* |
| Czekanowskiales | *Czekanowskia*, *Solenites*, *Phoenicopsis* |
| Bennettitales | *Ptilophyllum*, *Otozamites*, *Zamites* |
| Cycads | *Nilssonia* |
| Ferns | *Coniopteris*, *Onychiopsis*, *Ruffordia*, *Adiantopteris* |
| Horsetails | *Equisetites* |
| Lycopods | *Selaginellites* |
| Gnetaleans | *Chengia laxispicata*, *Liaoxia*, *Ephedrites* |
| Angiosperms | *Archaefructus* (3 spp.), *Hyrcantha* |

### What this means for the player, and why it's the best idea in this document

There is **no grass**. Grasses do not meaningfully exist yet. There is no grain, no bread, no thatch, no hay, no straw bedding, no woven grass basketry. There is no fruit, because fruit is an angiosperm invention and the angiosperms here amount to a pond weed. There are no legumes, no nuts as we understand them, no root vegetables.

Your carbohydrate options, in full:

| Source | Reality | Cost |
|---|---|---|
| **Conifer inner bark (cambium)** | Genuine historical famine food across the boreal world | Spring sap-flow window only; kills the tree if over-harvested |
| **Ginkgoalean seeds** | Edible, and a real food today | Contains an antivitamin-B6 compound; cumulative toxicity caps daily intake |
| **Cycad / bennettitalean seeds** | Edible **only after processing** | Cycasin. Requires repeated soaking and leaching — which requires **watertight vessels** |
| **Fern fiddleheads and rhizomes** | Real food, real risk | Thiaminase; must be cooked; chronic reliance causes B1 deficiency |
| **Horsetail shoots** | Marginal | Thiaminase, high silica; better as a *tool* than a food |
| ***Archaefructus*** | Aquatic, tiny | Trivial calories; a curiosity, not a crop |

The consequence: **you cannot eat your way out of this on plants alone.** The player is structurally pushed toward a hunter-fisher diet, and that opens a survival mechanic essentially no game has used — **protein poisoning**. Lean meat without fat causes rabbit starvation. You need *fat*: fish oil, bone marrow, mammal fat, the fat pads of birds. Autumn is when animals are fat. Winter is when they are not. This drives the entire annual cycle of play, and it is historically exactly how subarctic peoples lived.

And the elegant part: **ceramics unlocks the plant economy.** You cannot leach cycasin out of cycad seeds without a vessel you can soak and boil in. So the tech gate between "precarious forager" and "settled community" is *a fired clay pot* — which is both historically true and far more interesting than a crafting-bench upgrade.

### Also downstream of "no grass"

- **No thatch.** Roofing is conifer bark shingles, sod, hides, or carved tuff.
- **Cordage is the real bottleneck.** No flax, no hemp, no grass fibre. You have conifer bark bast, rawhide, gut, and **sinew** — which means your first genuine technology requires killing something big enough to have tendons. That's a beautiful early-game arc.
- **Horsetail is sandpaper.** *Equisetum* is silica-rich and was historically used as "scouring rush" for finishing wood and bone. Free, abundant, and a lovely detail.
- **Glue is conifer pitch.** Resin + charcoal + fat. Birch tar is unavailable (birches are a later invention), so pitch it is — and the conifer-dominated forest makes this the obvious, correct answer.

## 5. Colour — what we actually know

This is where the game earns its credibility. Melanosome analysis gives us real colour for a small number of Jehol animals, and we render those *as known*:

- ***Sinosauropteryx prima*** — **ginger/rufous and white banded tail**, **countershaded** (dark above, pale below), and a dark **"bandit mask"** stripe across the eyes. Critically, the *style* of its countershading indicates **open habitat** in direct sunlight, not closed forest.
- ***Psittacosaurus*** sp. — **countershaded** with pale underbelly and tail, more heavily pigmented chest, disruptive patterning, dark chin and dark jugal bosses. Its countershading pattern indicates a **closed, densely canopied forest** habitat.

Those two results together are a **gift to world design**: they are direct fossil evidence that the Yixian landscape was **heterogeneous** — open scrub and closed forest side by side — and they tell us *which animal belongs in which*. Our biome layout can be derived from camouflage studies. That is an absurdly good foundation and we should say so publicly.

For *Confuciusornis*, preliminary trace-metal (copper) work suggests mostly dark plumage on the torso with lighter or white feathers toward the wings; treat as **tier B**, and note that the related *Eoconfuciusornis* preserves hollow melanosomes indicating genuine **iridescence** — meaning iridescent structural colour was already present in Jehol birds and is defensible where evidence supports it.

Everything else gets a reconstruction flagged as such.

## 6. Confidence tiers

Every species, plant, and behaviour in the game carries a tier. **The player never sees it.** Tiers are an internal discipline for us and a public commitment in the [open dataset](../data/) — they govern what we are allowed to build, not what the game says about itself. The **Speculation Level** setting determines how much of tiers C and D the world expresses, and it is described in settings without any in-fiction framing.

| Tier | Meaning | Example |
|---|---|---|
| **A — Preserved** | Directly in the fossil record | *Sinosauropteryx* tail banding; *Yutyrannus* filamentous feathers; *Sinocalliopteryx* gut contents |
| **B — Strongly inferred** | Phylogenetic bracketing or functional morphology with broad consensus | Brooding posture in oviraptorosaurs; *Repenomamus* as a predator on juvenile dinosaurs |
| **C — Plausible** | Consistent with everything known, unpreservable by nature | Vocalisations, mating display behaviour, subtle colour, social group size |
| **D — Speculative** | Defensible but genuinely contested | Specific pack-hunting coordination; venom in *Zhangheotherium*'s tarsal spur |

**Speculation Level** in settings:
- **Strict** — tier A and B only. Animals we have no colour data for appear in muted, evidence-neutral plumage. Behaviour is minimal and naturalistic.
- **Standard** (default) — A through C. A fully fleshed-out living world where every addition is defensible.
- **Rich** — A through D. More display behaviour, more vivid speculative colour, more complex sociality. Still nothing invented from whole cloth.

Note what this setting does **not** do: it never *removes* tier A facts. You cannot turn the feathers off. That's the point.

## 7. The exclusion list

The single easiest way for this project to lose credibility is to include a famous "Jehol" animal that is from the wrong formation or the wrong period. Every one of these is commonly assumed to be Yixian and is not:

| Animal | Actually from | Age |
|---|---|---|
| ***Microraptor*** | Jiufotang Fm | ~120 Ma — too young |
| ***Jeholornis*** | Jiufotang Fm | ~120 Ma |
| ***Sapeornis*** | Jiufotang Fm | ~120 Ma |
| ***Yanornis***, ***Yixianornis*** | Jiufotang Fm | ~120 Ma — despite the name |
| ***Changyuraptor*** | Jiufotang Fm | ~120 Ma |
| ***Anchiornis*** | Tiaojishan Fm | Late Jurassic — ~35 Myr too early |
| ***Tianyulong*** | Tiaojishan Fm | Late Jurassic |
| ***Epidexipteryx*** | Daohugou beds | Middle/Late Jurassic |
| ***Jeholopterus*** | Daohugou beds | Middle/Late Jurassic |
| ***Protopteryx***, ***Eoconfuciusornis*** | Huajiying Fm | ~131 Ma — too old |
| ***Archaeamphora*** | Yixian, but not an angiosperm | Refuted 2015 |

> **Settled: strict Yixian. One time, one place. No fudging.** *Microraptor* does not appear in this game, in any form, ever — not as a mod-friendly stub, not as a "bonus," not in the launch trailer. Neither does anything else in the table above. There is no Jiufotang expansion planned, because a later formation would be a different world and this game is about *one*.

### Is the whole formation really "one time"?

Worth stress-testing, because the Yixian's nominal 125.8–124.1 Ma span is 1.7 Myr, and blending assemblages across that would itself be a kind of fudging.

The science lets us off. The 2024 PNAS reassessment constrains the fossiliferous sequence to **less than about 93,000 years** — geologically, a single instant — and characterises the formation as "a brief snapshot of normal life and death" rather than a series of catastrophes. On that reading, the Yixian assemblage genuinely *is* one moment, and drawing on all of it is not a compromise.

The remaining question is spatial, not temporal: Lujiatun, Jianshangou, Dawangzhangzi, and Jingangshan are different localities with somewhat different assemblages. **We resolve that spatially rather than by layering** — one basin, with the assemblages distributed across it as habitat variation. That is what the *Sinosauropteryx* (open habitat) and *Psittacosaurus* (closed forest) camouflage results already tell us to do, so the unit differences become the map's biome structure. One place, one time, coherent.

## 8. The sky

Worth getting right because it costs almost nothing and delights the exact audience we want.

- **The day is short.** Earth's rotation has been slowing at roughly 1.7 ms/century; at 125 Ma the day was about **23.4 hours**. Our day/night cycle should be scaled to a 23.4-hour day, and any clock the player builds should drift against it in a way that is discoverable.
- **The stars are wrong.** Over 125 million years, stellar proper motion completely scrambles the constellations. Not one modern constellation exists. The night sky must be **procedurally generated and unrecognisable** — no Orion, no Big Dipper. Getting this right is a flex that costs us one shader and a star catalogue we invent ourselves.
- **No pole star.** Precession and proper motion put the celestial pole somewhere unremarkable. Navigation by a fixed north star is unavailable; the player has to derive celestial north from arc motion, which is a genuinely satisfying skill to learn.
- **The Moon** is only ~0.3% closer than today. Negligible. Don't make it huge; that's exactly the sort of cheap "cool factor" the design rules forbid.
- **Higher atmospheric CO₂** in the Cretaceous supports a slightly hazier atmosphere and vivid volcanic-aerosol sunsets — free atmosphere-rendering justification.

The consequence: **the calendar is a technology**. Winter kills, so predicting winter matters, so solstice-marking matters, so a stone alignment on the ridge is *mechanically useful*, not decoration. Culture emerges because the environment demands it. This is the cleanest possible answer to "no story, but room for culture."

## 9. Standing rule: the paleontology is data

Every species lives in [`data/species.yaml`](../data/species.yaml) with its unit, size, integument, confidence tiers, and citations — **not** hardcoded in engine. When new work is published, we patch the dataset, bump its version, and the animal changes.

The changelog is **public and out-of-game** — in the open dataset's repository, where scientists can read and correct it — because nothing inside the fiction ever explains itself. A player who has been watching the same animal for two years may simply notice, after an update, that its plumage is different now. They will not be told why. If they care, the answer is a published citation, one click outside the game.

Nobody has shipped a game that gets more scientifically accurate after release. We should.
