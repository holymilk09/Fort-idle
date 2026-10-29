# Fort Idle — Art Style Guide (v1)

> Target look: polished commercial mobile idle (peers: Burger Please / Burger Idle, Pro Builder 3D).  
> Base: Synty POLYGON stylized low-poly. Custom hero/monsters/props via Tripo Pro (fiji bot account) matching that style. CC0 Kenney/Quaternius fills gaps.  
> **Rule:** art hold lifted per Game Chief 2026-10-28 — hand assets to Unity Engineer; this lane does not edit game code. Commercial-use license only. No spend beyond approved Tripo use (Edwin handles Synty; other spend needs Matt OK via Game Chief).

---

## 1. Palette (hex)

| Role | Hex | Notes |
|---|---|---|
| Ground (grass) | `#5A8A45` | Map 1 clearing default |
| Dirt / path | `#8B6B45` | Pad rings, trails, stump soil |
| Foliage mid | `#3D8B4A` | Tree canopies, bushes |
| Foliage dark | `#2A5E32` | Treeline depth, night silhouette |
| Wood T1 (fresh) | `#C48A4A` | Palisade, early builds |
| Wood T2 (aged) | `#8B5A2B` | Reinforced wood, hut beams |
| Stone T1 | `#9AA3A8` | Stone wall / ring |
| Stone T2 | `#6B7378` | Darker stone, cliffs |
| Metal / iron band | `#5C6B78` | T2 iron bands, bolts |
| Iron dark | `#3A4550` | T3 iron / portcullis |
| UI accent | `#FFCC33` | Buttons, coin, bag pad |
| Pad glow | `#4DE8FF` | Build pads, pour juice |
| Danger / HP warn | `#E84B3C` | Fail, threat, damage |
| Enemy / night fog | `#6B3D7A` | Shambler tint, dusk grade |
| Night ambient | `#1A1F3A` | Sky / ambient after dusk |
| Fire warm | `#FF8A2B` | Campfire core, night readable |

Muted environment, saturated interactables (pads, resources, fire). Crystal tier later: `#7BE0FF` / `#B48CFF` accents only.

---

## 2. Unit scale

| Thing | Size |
|---|---|
| World unit | **1 Unity unit = 1 meter** |
| Hero height | **1.7 m** (chunky silhouette; head ~1.75–1.8 with helmet) |
| Worker height | **1.55 m** |
| Shambler | **1.6 m** (hunched read ~1.4) |
| Brute | **2.4 m** (wide, heavy — **not** a knight silhouette) |
| Treant (boss) | **6–8 m** torso; canopy to ~10–12 m |
| Plank (stack) | **0.12 m** height per plank (MAP1_SPEC); visual cap 60 meshes |
| Wall segment footprint | **~2.0 × 0.5 m** (snaps on 0.5 m grid) |
| Gate footprint | **~3.0 × 1.0 m** |
| Watchtower footprint | **~2.5 × 2.5 m** |
| Turret footprint | **~2.0 × 2.0 m** |
| Spike trap | **~2.0 × 1.0 m** |
| Campfire core | **~2.0 m** diameter clear |
| Hut | **~4.0 × 4.0 m** |
| Pad glow radius | Match footprint + 0.25 m rim |

Snapping grid: **0.5 m** horizontal, **0.25 m** vertical for modular parts. Slot pivots at footprint center, Y=0 ground.

---

## 3. Proportions (chunky stylized)

- Hero: big head/hands (~1.15× real), short neck, thick limbs; axe/sword readable at phone distance.
- Buildings: slightly oversized posts and rails vs real carpentry (toy fort, not CAD).
- Props (planks, coins, meat): oversized pickup scale so they read in stack and loot fountain.
- Enemies: exaggerated silhouette — Shambler limp arms; Brute **ogre/bark-plated beast** (knight is ally); Treant trunk legs + glowing eyes.
- Avoid skinny poles, thin walls, or realistic armor folds.

---

## 4. Poly & texture budgets (mobile URP)

Scene targets (GDD): ≤150 draw calls, ≤300k tris, 60 fps mid-tier phones.

| Asset class | Tris (LOD0) | LOD1 | LOD2 / impostor | Notes |
|---|---|---|---|---|
| Hero | 2.5–4k | 1.2k | 400 | One skinned mesh preferred |
| Worker / ally | 1.5–2.5k | 800 | 300 | Shared skeleton where possible |
| Shambler | 1.2–2k | 600 | 250 | Heavy instancing |
| Brute | 3–5k | 1.5k | 500 | Few on screen |
| Treant | 8–12k | 4k | 1.5k | Boss only |
| Wall / gate segment | 300–800 | 150 | 60 | Modular parts summed |
| Watchtower (full) | 1.5–3k | 800 | 300 | |
| Turret / ballista | 1–2.5k | 700 | 250 | |
| Hut | 2–4k | 1k | 400 | |
| Tree | 200–600 | 100 | billboard | Pool + GPU instance |
| Rock / stump / prop | 80–300 | 40 | — | Shared atlas |
| Coin / drop | ≤80 | — | — | Instance |

**Textures**
- Prefer **Synty-style shared gradient / color atlas** (e.g. 512 or 1024 atlas), **single material** per pack family.
- Vertex color OK where it cuts a texture sample.
- Hero/unique monsters: ≤2×512 albedo (+optional 256 emissive). No 2K on mobile props.
- UI icons: 128–256 atlas, point filter or light AA.
- Avoid unique materials per prop — batching > pretty.

**LODs:** screen relative; trees/rocks LOD1 by ~15 m; characters LOD1 by ~20 m.

---

## 5. Texture / material approach

- Flat-ish stylized shading: soft specular, high roughness on wood/stone.
- Shared atlas UVs; trim sheets for wood planks / iron bands.
- Emissive: pads (`#4DE8FF`), fire, Treant eyes, night lane eyes — keep bloom-friendly but clamped.
- One URP Lit (or Simple Lit on low) material template; variants via color / atlas offset.

---

## 6. URP lighting look

| Light | Day | Night |
|---|---|---|
| Key (Directional) | Warm white `#FFF4DD`, intensity ~1.1–1.4, ~45° elev | Cool `#8899FF`, intensity ~0.08–0.15 |
| Ambient / sky | Soft blue-green | `#1A1F3A` dominant |
| Campfire | Soft fill | Point / spot `#FF8A2B`, hero/readability |
| Fog | Light distance haze | Purple-blue height fog, denser |

**Outline:** optional **thin inverted-hull or URP Renderer Feature outline** on hero + interactables only; off by default on low-end. Prefer silhouette via value contrast first.

**Camera (GDD):** Cinemachine follow ~**50° pitch**, **~40 mm**, drop to ~30° with tall stack; pinch-zoom limited. Art must read from that angle (no detail only on tops).

---

## 7. Post-processing (URP Volume) — mobile cost notes

| Effect | Day | Night / fire lit | Mobile note |
|---|---|---|---|
| Bloom | Soft, threshold high; pads/fire only | Stronger on fire / Treant eyes | **Cheap if thresholded**; avoid full-screen glow spam |
| Color grading | Mild warm lift, slight contrast | Cool shadows, warm mid from fire (“gray→color” when fire lit) | **Lut 16³ or volume overrides**; one global + night override |
| Vignette | Subtle | Slightly stronger | Cheap |
| Ambient Occlusion | Off or low SSAO on high only | Off on mid/low | **Expensive** — default **off** for MVP |
| Depth of Field | Off (gameplay) | Off; optional boss cam | Skip on mobile |
| Motion blur | Off | Off | Skip |
| Film grain | Off | Very light optional | Prefer off |
| Shadows | Soft 1 cascade mid; hard 1 cascade low | Same | Keep cascade count ≤2 |

Night “color burst” when fire is healthy: lift saturation + warm midtones via volume blend, not a second scene.

---

## 8. FBX export / naming

- **Axis:** Blender → Unity: forward **-Z**, up **+Y**; apply scale **1.0** (meters). Export **Apply Transform**.
- **Scale:** hero 1.7 m tall in Blender = 1.7 units in Unity.
- **Pivots:** characters at feet center; buildings at footprint center ground; modular parts at their local snap point (see §9).
- **Naming:** `Category_Variant_Part_##` — e.g. `Wall_T1_Post_01`, `Hero_Viking_Mesh`, `Enemy_Shambler_LOD1`.
- **Animations:** clip names `Idle`, `Walk`, `Run`, `Carry`, `Chop`, `Attack`, `Hit`, `Die`, `Victory` (humanoid Mecanim where possible).
- **Materials:** strip to atlas material names `MI_FortIdle_Atlas`, `MI_FortIdle_Emissive`.

---

## 9. MODULAR PART RULES (staged builds)

Staged construction (live on main): wall **5**, gate **5**, tower **6**, turret **5**, spikes **3**, campfire **3**, hut **4**, upgrades **4**.  
Each stage adds or swaps **named part groups**. Tiers (T1 wood → T2 reinforced → T3 stone/iron) **keep the same group names and pivots** so meshes swap 1:1.

### Convention

- Name: `{Build}_T{tier}_{Group}_{index:02}`  
  Example: `Wall_T1_Post_01`, `Wall_T2_Post_01` (same pivot/bounds).
- **Pivot:** group local origin at snap socket; parent empty `{Build}_Root` at slot center.
- **Grid:** 0.5 m; part extents should not fight neighbors when sockets align.
- **Stage visibility:** stages enable groups in order; unfinished groups hidden or shown as ghost (engine-side). Art delivers all groups per tier.
- **No orphan geometry** outside a named group.

### Part groups per build

#### Wall (5 stages) — groups
1. `Post` (corner/interval posts)  
2. `Rail` (horizontal rails)  
3. `Plank` (infill boards)  
4. `Brace` (diagonal braces)  
5. `Cap` (top caps / spikes optional)  
**Tiers:** T1 wood palisade → T2 wood + `Band` (iron band submesh on Post/Plank) → T3 stone (`Post`/`Plank` stone meshes, same names).

#### Gate (5 stages)
1. `Frame_L` / `Frame_R`  
2. `Lintel`  
3. `Door_L` / `Door_R` (or single `Door`)  
4. `Hardware` (hinges, bar)  
5. `Spike` / `Portcullis` (T3)  
T2 adds iron-bound materials on Door/Hardware; T3 swaps Door→Portcullis mesh keeping pivots.

#### Watchtower (6 stages)
1. `Leg` (×4)  
2. `Brace`  
3. `Platform`  
4. `Railing`  
5. `Ladder` / `Access`  
6. `Roof` (T2+; T1 open)  
Archer mannequin optional separate `Occupant` (not a stage).

#### Turret / ballista (5 stages)
1. `Base`  
2. `Post` / `Mount`  
3. `Beam` / `Stock`  
4. `Bow` / `Arms`  
5. `Ammo` / `Crank` (cosmetic)

#### Spike trap (3 stages)
1. `Bed` / `Frame`  
2. `Spike`  
3. `Brace` / `Tip` (bone/iron material swap by tier)

#### Campfire core (3 stages)
1. `Ring` (stones/logs)  
2. `Fuel` (logs)  
3. `Flame` (mesh + VFX socket `VFX_Fire`)  
T2 stone ring; T3 larger bonfire — same group names.

#### Hut (4 stages)
1. `Floor` / `Foundation`  
2. `Wall`  
3. `Door` / `Frame`  
4. `Roof`

#### Upgrade pads (bag / speed) (4 stages)
1. `Base`  
2. `Pillar`  
3. `IconMount`  
4. `GlowRing` (emissive)

Engine may map stage index → subset of groups; art must not rename groups between tiers.

---

## 10. Animation list per rig

| Rig | Clips (required MVP) |
|---|---|
| Hero | Idle, Walk, Run, Carry (loop), Chop, Attack (melee), Hit, Die |
| Worker | Idle, Walk, Carry, Chop (short), Deposit |
| Shambler | Idle, Walk, Attack, Hit, Die |
| Brute | Idle, Walk, Attack (heavy), Hit, Die, Roar (optional) |
| Treant | Idle, Walk/Stomp, Attack, Summon, Hit, Die (long) |
| Ally units (later) | Idle, Walk, Attack, Hit, Die |

Carry: upper-body override or dedicated FullBody Carry; stack is separate instanced meshes.

---

## 11. Character & building source shortlist

Generator of record: **Tripo Pro** (Meshy dropped 2026-10-28). Default path: Tripo generate → free GLB export → free Blender optimize/LOD/bake (`art/TRIPO_PIPELINE.md`). Tripo credits only when needed (e.g. Retopo for rigged hero).

| Subject | Preferred source | Fallback | Notes |
|---|---|---|---|
| Hero (Viking Leather→Iron→Lord) | **Tripo Pro** custom in Synty proportions | Synty Vikings / Adventure peasant-warrior | Skins = material/gear swaps |
| Workers | Synty Fantasy Kingdom peasants | Quaternius RPG chars / Kenney blocky | 4 per map |
| Soldier / Musketeer / Knight (allies) | Synty Knights + Fantasy Kingdom | Quaternius knight (ally only) | Knight ≠ Brute |
| Shambler | **Tripo Pro** zombie shambler | Quaternius / Kenney zombie-like | Hunched, torn, not armored |
| Brute | **Tripo Pro** — **beast/ogre/bark-hide**; must **not** read as knight | Custom Blender blockout | GDD: knight is now ally |
| Treant | **Tripo Pro** boss treant (done: `art/monsters/treant_golem/`) | Quaternius nature + custom | Glowing eyes, weak-spot socket |
| Walls/gate/tower/hut | Synty **Fantasy Kingdom** modular | Kenney Castle / Fantasy Town CC0 | Retarget to modular groups |
| Nature / trees / rocks | Synty **Nature** | Kenney Nature Kit CC0 (in `art/source`) | |
| Campfire / survival props | Synty Vikings / Adventure | Kenney Survival Kit CC0 | |
| Spikes / fortifications | Synty Fantasy Kingdom battleground | Kenney Survival fence-fortified | |
| UI icons | Custom flat + Synty INTERFACE later | Kenney UI packs CC0 | |

---

## 12. Out of scope (this lane)

- No Unity scene/prefab integration until Matt lifts art hold.  
- No purchases from this agent (Edwin owns Synty spend; Tripo credits used sparingly).  
- No ads creatives until new art lands.
