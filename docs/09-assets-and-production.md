# 09 — Assets & Production

A solo, AI-assisted production manual: what to make, with what, in what order.

**Context.** One person, no deadline, no commercial pressure, not for general release. That combination is unusually favourable — it means the correct strategy is *learn the pipeline properly and go slowly*, rather than the compromises a funded studio on a schedule would make. Scope is not capped; only the **starting slice** is.

---

## 1. Four principles that decide everything downstream

### 1.1 Accuracy lives in the silhouette. AI is bad at silhouette and good at surface.

Split the work along exactly that line. **You** control proportions, posture, and outline — from published skeletal reconstructions. **AI** handles surface detail, texture, colour, variation, and volume. Never invert this. A perfectly textured animal with wrong proportions is wrong; a crudely textured animal with correct proportions reads as real.

### 1.2 Never prompt with a taxon name.

The internet's dinosaur art is overwhelmingly inaccurate, so every generative model has a strong prior toward scaly, shrink-wrapped, tropical wrongness. Asking for "Sinosauropteryx" summons that prior directly.

**Prompt morphology instead.** Not *"Sinosauropteryx prima"* but:

> *a one-metre bipedal animal completely covered in shaggy ginger and white filamentous down, short two-fingered arms, long tail with rufous and white bands, dark stripe across the eyes, pale belly, standing in snow*

You are not asking it to recall a dinosaur. You are asking it to build an animal. The difference in output is dramatic. Better still, condition on a silhouette you provide (§4.2) so the model has no room to drift.

### 1.3 Style is a consistency-enforcement mechanism, not just taste.

Your assets will come from Megascans, three different generators, your own Blender work, and your phone camera. Every source has a different style. **A strong, uniform, slightly non-photoreal shading model unifies heterogeneous inputs at the renderer.** Photorealism does the opposite — it makes every mismatch visible.

Target: **The Long Dark's strategy**, not its exact look. Flat-ish shading, low-detail textures, heavy colour grading, atmosphere doing the work. Made by a small team, unmistakably not cartoonish, and it has aged better than its photoreal contemporaries.

Your setting hands you the strongest atmospheric toolkit in games for free: fog, snow, low sun at altitude, conifer silhouettes, and eleven-hour nights lit by a single fire. **Fog is the cheapest immersion in the medium** and you have a scientific justification for using it everywhere.

There is also a thematic option worth a serious style test: **naturalist illustration as the visual language** — the idiom of a field-guide plate. The game is about a person observing an unknown world and writing it down; rendering it that way is a conceptual match, not a budget dodge. And it happens to be something image models are excellent at.

### 1.4 Behaviour reads as quality more than models do.

An animal that fluffs against cold, shakes snow off, preens, sleeps curled with its snout under its arm, and flees when you're upwind will read as *alive* at modest fidelity. That's code, and code is where your AI assistance is strongest. Budget accordingly: **spend on animation and behaviour before you spend on polygons.**

---

## 2. The tool stack

Free-first, with the paid accelerators actually worth buying flagged. Prefer open-weight and offline tools where possible — this is a multi-year project and subscriptions bleed.

| Job | Tool | Notes |
|---|---|---|
| **Engine** | **Godot 4** | Settled in [08 §12](08-decisions.md#12-engine--godot-4-revised): free with no royalty tier at any revenue, small readable codebase, C# keeps the simulation testable. No Nanite, so meshes need manual decimation/LODs; no built-in PCG framework, so rules-based scattering (§5.4) is either a community addon or hand-rolled |
| **DCC hub** | **Blender** | Unavoidable and free. Blockout, retopo, rigging, cleanup, feather cards. This is the skill to learn first |
| **2D generation** | **ComfyUI** + Flux/SD locally | Free, offline, and critically supports **ControlNet / IP-Adapter** — reference conditioning is what beats the bad prior. Midjourney is prettier and far less controllable; use it for mood exploration only |
| **3D generation** | **Tripo** or **Meshy**; **Hunyuan3D** / **TRELLIS** locally | Genuinely good for props, rocks, tools, pottery. Treat creature output as a *base to rework*, never a finished asset |
| **Retopology** | Blender QuadriFlow (free) or **Quad Remesher** (~$100) | Quad Remesher is the single best small purchase in this list |
| **Trees** | **SpeedTree** (indie tier) or **The Grove** (Blender addon) | The Grove simulates botanical growth, which suits accurate morphologies. Trees are your hardest environment asset — see §5.2 |
| **Terrain** | **Gaea** (free tier) or in-engine sculpt | Basin macro-layout is hand-authored per [06 §4](06-technical-architecture.md#4-world-generation) |
| **Scans** | **Fab / Megascans** | No longer free (Quixel-to-Fab migration completed 2026; individually priced or subscription-gated) and not bundled with Godot regardless — budget for it explicitly. Basalt, andesite, bark, moss, dirt, snow all exist as real photogrammetry, which is still worth paying for over building from scratch |
| **Audio** | **Reaper** (cheap) + a field recorder + Freesound | See §6 |
| **Ecology core** | Plain C++ or Rust, no engine | Per [06 §2](06-technical-architecture.md#2-the-two-tier-simulation--the-core-technical-bet). Also the best-case target for AI-assisted coding |

**On engine reversibility:** the engine-independent ecology core means switching engines later costs you rendering and animation work, not simulation work. That was an architecture decision; it's now also your insurance policy for a multi-year solo project.

---

## 3. Phase 0 — Reference and style lock (do this before making anything)

This phase produces no game assets and it is the highest-leverage time you will spend.

### 3.1 Build the reference library

- **Skeletal reconstructions.** Scott Hartman's (skeletaldrawing.com) are the field standard; the papers in [SOURCES.md](../SOURCES.md) contain more. These define proportions and are non-negotiable inputs to every creature.
- **Living analogue photography — go to a botanical garden.** This is the cheap accuracy win nobody thinks of: **almost your entire flora is still alive.** *Ginkgo biloba* is literally the same lineage. Cycads, tree ferns (*Dicksonia*), horsetails (*Equisetum*), *Araucaria*, *Podocarpus*, and Wollemi pine are all living stand-ins for Yixian morphologies. One day with a camera gets you primary reference for most of the plant list. Shoot bark, needle arrangement, cone structure, understory, and light through canopy.
- **Ratite and archosaur footage.** Cassowary, emu, rhea, ostrich, secretary bird. Walking, running, turning, sitting, preening, fluffing, threat display. This is both your animation reference *and* your animation source (§4.6). Zoo trip with a tripod, plus documentary footage.
- **Cold conifer forest photography.** Fog, snow load on branches, low sun, blue-hour snow, firelight. Reference for grading and atmosphere.

### 3.2 Lock the style with one frame

Produce **one still image** that is the game you want — not concept art of a scene you'll never build, but the actual target look: a specific view of forest, in specific light, at the fidelity you intend to hit. Iterate on that single frame until it's right.

Everything afterwards is judged against it. Without it, a multi-year solo project drifts in style until nothing matches.

### 3.3 Learn Blender before you need it

Honest note: **the tools are not your critical path — your skills are.** The order that matters is blockout → retopo → UV → rigging → weight painting. A few focused months here saves years of fighting the pipeline. Everything else in this document assumes you can do those five things competently.

---

## 4. The creature pipeline

The hard one, done in full. Work through it **once, completely**, on a single animal, before touching a second.

### 4.1 Skeletal reference → proportions

Get the published skeletal. Set it up as a background reference in Blender at true scale. This is your accuracy ground truth and everything else conforms to it.

### 4.2 Blockout — the accuracy step, human-controlled

Build the correct silhouette in low-poly primitives matched to the skeletal. It is easier than it sounds: you are matching a 2D diagram with boxes and cylinders. **This is the step you never hand to a generator**, because this is where accuracy actually lives.

Watch for the classic errors the reference will save you from: shrink-wrapping (bone outlines visible through skin), missing soft tissue, under-muscled tails, and mistaking the feather envelope for the body — a feathered animal's *outline* is much fatter than its skeleton.

### 4.3 Surface — where AI takes over

Two routes, both keeping your silhouette:

- **Multi-view conditioning.** Render your blockout from 6–8 angles, run each through img2img with a morphology prompt (§1.2) at low denoise so the silhouette survives, then feed the resulting views to a multi-view-to-3D generator.
- **Sculpt directly** in Blender over the blockout, using AI-generated concept images purely as visual reference.

Either way, the mesh you get out is a **base**, not a deliverable.

### 4.4 Retopology and UVs

Required for anything that animates. Without Nanite, this step is fully load-bearing rather than partly optional — Godot needs sane deformation topology and reasonable poly counts on everything, not just hero animals. Quad Remesher, or manual for hero animals. Edge loops around joints, mouth, and eyes.

### 4.5 Feathers — the special problem

Generators will give you a lumpy approximation. Three approaches, in increasing cost:

1. **Sculpted feather masses + a shader that implies detail.** Cheapest, and with a stylized art direction it reads perfectly at gameplay distance. **This is what I recommend for solo.**
2. **Card/shell technique.** Feather clumps as alpha cards, placed with Blender's hair system, baked down. The standard game approach; good results, moderate work.
3. **Full curve-based grooming.** Beautiful, expensive, and a rabbit hole. Not until much later, if ever.

Whatever you choose, feathers must respond to **wet** (clumping and darkening) and **cold** (fluffing, which visibly changes silhouette). Those two states sell the climate in a single frame and are worth more than feather density.

### 4.6 Rigging and animation — your real wall

Rig with Blender **Rigify**, adapted to a bird-like biped. Then:

- **Video-to-motion from your ratite footage.** Video-to-motion is a standard pipeline feature now, and your setting makes it *scientifically correct* rather than a shortcut: these animals moved like large ground birds, so cassowary and emu gait is the right reference, free, and defensible. Nobody else's dinosaur game can say that.
- **Procedural locomotion in-engine.** Godot's `SkeletonIK3D` for foot placement plus `AnimationTree` blend spaces and state machines for motion blending and terrain/speed adaptation — the same category of system as Unreal's Control Rig, built by hand rather than out of the box. You author a small number of cycles and the tooling adapts them to terrain, speed, and slope. **For a solo dev this is the single biggest force multiplier in the document** — it turns "hundreds of animations" into "a few good ones plus systems" — though expect more of it to be your own code than a Control Rig equivalent would have needed.
- **Hand-author only the signature behaviours**: fluffing, snow-shake, preening, the tucked sleeping curl, threat display, feeding. A dozen short clips per animal, not hundreds.

### 4.7 Creature order

Build them in order of *ease*, not importance, so the pipeline matures on cheap subjects:

1. **A fish** (*Lycoptera*). Trivial rig, trivial animation, and it's the protein floor — your first creature should be the one you can't get wrong.
2. **A small ground herbivore** (*Jeholosaurus*). First real biped. Naive and approachable, so the player sees it up close — which is a good forcing function.
3. **A flock bird** (*Confuciusornis*). Introduces flight, flocking, and the roost set-piece.
4. **The hero** (*Sinosauropteryx*). Best-documented colour in the formation, and your cover animal. Do it fourth, when the pipeline is good.
5. **A small mammal raider** (*Repenomamus*). Fur instead of feathers; different shader path.
6. **A real threat** (*Sinocalliopteryx*). Complex combat and hunting behaviour.

*Yutyrannus* comes much later. It should be the best thing in the game and you should not attempt it until you can do it justice.

---

## 5. The environment pipeline

**Environment before creatures.** The forest is ~90% of the pixels on screen and nearly all of the immersion. A beautiful, correct, *empty* forest is already compelling — creatures are the last 10% of the feeling and 90% of the work.

### 5.1 Rocks, ground, and materials — nearly solved

Megascans has real photogrammetric basalt, andesite, volcanic rock, bark, moss, dirt, gravel, and snow. It is no longer free (§2's Scans row), and importing it into Godot is manual rather than the one-click Fab-to-Unreal path, but paying for it is still cheaper than building the same library from scratch. Your job is **curation, not creation**. See the rejection checklist (§7).

### 5.2 Trees — your hardest environment asset

There is no off-the-shelf accurate Cretaceous conifer. Build them in SpeedTree or The Grove from your botanical-garden reference, targeting *Araucaria*, *Podocarpus*, and Wollemi-pine morphologies rather than generic modern pine silhouettes. You need perhaps 5–8 tree species total, with variants. Budget real time here; the forest is the game.

### 5.3 Ground cover — the "no grass" problem

**Every foliage tool and asset pack assumes grass.** You have none. Your understory is ferns, horsetails, lycopods, moss, leaf litter, and bare volcanic soil. Expect to reject most stock foliage.

This is your fastest route to a world that looks genuinely alien without inventing anything, and it is worth building a small custom ground-cover set early.

### 5.4 Scattering and assembly

Author *rules* (conifer forest with fern understory, density by slope and moisture, clearings near water) rather than placing plants. Rules scale; hand placement does not, and you are one person. Godot has no built-in equivalent of Unreal's PCG framework — the options are a community scattering addon (audit for maintenance before committing) or a small hand-rolled tool driven by the same rule data the ecology core already uses for habitat. Given [decision 13](08-decisions.md#13-ecology-core--same-language-as-the-engine-but-isolated-as-a-module)'s C# core, a hand-rolled scatterer is a modest build and keeps the rules in one place.

### 5.5 Sky, weather, VFX

Procedural star field (**no real constellations** — see [01 §8](01-the-science.md#8-the-sky)), volumetric fog and clouds, snow accumulation, and firelight. Mostly engine features plus tuning rather than authored assets. High impact per hour spent.

---

## 6. Audio — your real graphics budget

For an asset-constrained project, **audio buys more immersion per hour than any visual work.** A boom in the fog that you never see is more frightening than any model you could commission.

**Creature vocalisations.** The extant phylogenetic bracket for dinosaurs is crocodilians and birds, so the correct sources are also the cheapest: **cassowary and ostrich booms, bittern, crocodilian rumbles, emu drumming.** Layer, pitch-shift, and process. And remember the syrinx is a late avian innovation — these animals **boomed, hissed, and rumbled; they did not sing.** A dawn chorus that is wrong in a way players cannot name is free differentiation and one of the strongest ideas available to you.

**Everything else** is field recording with a cheap recorder: wind in conifers, footsteps in snow at different depths, fire, water, stone on stone, knapping, breathing in cold air. Foley is the single most solo-friendly discipline in game development.

**Do a rough audio pass earlier than feels natural** — it transforms how the greybox feels and will change design decisions while changing them is still cheap.

---

## 7. The rejection checklist

Run every scanned, generated, or purchased asset against this. Pin it where you'll see it.

**Reject on sight:**
- **Grass.** Any grass. Anywhere. This is the most common contaminant by far
- **Flowers**, and any flowering plant that isn't *Archaefructus* or *Hyrcantha*
- **Broadleaf / deciduous angiosperm trees** — oak, maple, birch, willow, beech
- **Fruit, berries, nuts** — except conifer, ginkgo, and cycad seed structures
- **Butterflies and bees** — angiosperm-associated; use the long-proboscid scorpionflies instead
- **Reeds and cattails** — angiosperms. Use horsetails
- **Snakes** — they appear mid-Cretaceous, later than this
- **Any mammal larger than badger-sized**
- **Modern-looking birds** — no beaked songbirds, no waterfowl silhouettes
- **Deciduous broadleaf leaf litter** — your litter is needles, fern fronds, and cone scales

**Acceptable and underused:** mosses, liverworts, lichens, **fungi** (mushroom-forming fungi are known from Cretaceous amber — flag as tier B/C, and they're a lovely food and ambience item), and every kind of volcanic rock.

---

## 8. The order of work

The single most common solo failure is making art before the game is fun. Resist it.

### Stage 1 — Greybox, no art at all
The Cold Open prototype from [07 §1](07-roadmap.md#1-prove-the-thesis-first-the-cold-open-prototype), in pure untextured geometry. Thermal model, fire, one real 23.4-hour day, one full night, three placeholder-cube "animals." Rough audio pass.

**Gate: is the night compelling?** If two hours of real darkness with a fire is tedious, no amount of art fixes it. Do not proceed until this is a yes.

### Stage 2 — Systems depth, still greybox
Ecology core with two-tier materialisation. Nutrition. Fire and shelter. The Journal. 1:1 clock. Basic co-op. All of this is code, which is where your AI assistance is strongest — and none of it needs a single finished asset.

### Stage 3 — One of each, all the way through
Take **one** creature, **one** tree, **one** rock, **one** prop, and **one** building piece completely to final quality. This establishes the pipeline and exposes its problems at a fiftieth of the cost of discovering them at scale.

Then **stop and fix the pipeline** before making anything else.

### Stage 4 — The forest
Environment to shipping quality. Trees, ground cover, terrain, materials, scattering, weather, fog, snow, light. **Get the world to feel right while it's empty.** This is the stage where the game becomes itself.

### Stage 5 — Creatures, one at a time
In the order of §4.7. Each one gets full behaviour before the next one starts — a finished animal that acts alive beats three that stand around.

### Stage 6 — Structures and craftables
The building kit and the tool/pottery set. Heavy AI-generation territory; this is the easiest large asset batch in the project.

### Stage 7 — Full audio, VFX, and polish
Then expand — more creatures, more of the basin, the deeper systems from [07 §4](07-roadmap.md#4-toward-10). With no deadline, this stage never has to end.

---

## 9. Where money is actually worth spending

Even with no commercial pressure, a few small purchases return far more than their cost:

1. **Quad Remesher** (~$100) — saves hundreds of hours of retopo.
2. **A tree tool** — SpeedTree indie or The Grove. The forest is the game.
3. **A field recorder** — audio is your best immersion-per-hour and you cannot download the sound of *your* boots in *this* snow.
4. **A few months of a 3D generator subscription** during Stage 6, when you're batching props. Cancel between bursts.
5. **Optional but high-value: one commissioned hero creature** from a paleoart-literate 3D artist. Not for the asset — for the **calibration**. Having one animal you *know* is right, to measure your own against, is worth more than the model itself.

Free alternative to that last one: post work-in-progress to the paleoart community and ask for critique. They are exacting, generous with people who are visibly trying to get it right, and they will catch shrink-wrapping and proportion errors you cannot see yourself.
