# Fort Idle — Art Manifest (soft-launch checklist)

> Shopping list for **3D Art** + **Unity Engineer**. No spend / no accounts from this lane.  
> Tripos = Tripo Pro (Edwin). Synty = packs Edwin buys. CC0 = Kenney/Quaternius on disk or fetch.  
> Tris = LOD0 from `ART_STYLE.md`. Status: `done` · `done-not-integrated` · `in-progress` · `todo`.  
> Staged builds: `{Build}_T{tier}_{Group}_{##}` per ART_STYLE §9.

## Suggested batch order (QA-gated)

| Batch | Contents | Gate |
|---|---|---|
| **A** | Hero Viking Lv1, Shambler T1, Brute T1 (new look), Treant integrate, Wall/Gate T1–T3 parts, trees/rocks/ground, wood+coin pickups | Map-1 vertical slice looks like the game |
| **B** | Watchtower / Turret / Spikes / Campfire / Hut staged parts; workers ×2; supply drops (meat, bones, hide, core, sap) | Full map-1 build set readable |
| **C** | Soldier / Musketeer / Knight Lv1; weapon set (1 mesh each type); hit/death/build VFX | Allies visible in night |
| **D** | Cinderling + Embercaller; Bonebow; Hornback; Rotbloater; elite affix VFX | New map-1/2 threat roles |
| **E** | Ally Lv2–3 skins; hero Iron/Lord; weapon tiers 2–3; enchantment VFX (8) | Element sheet in game |
| **F** | Carrion Flock, Dirtgrub, Mistwisp, Ramfiend, Bog Leaper/Spitter, Frost Pack; Bog Hydra | Maps 2–3 soft launch |
| **G** | Trader + research/forge; map-2 gate; UI icons atlas; Frost Titan; polish LODs | Soft-launch complete |
| **H (P2)** | Dune Maw, Stackthief, crystal fort tier, map 4–5 env | Post soft launch |

---

## Monsters (every tier)

| Asset | Source | Tris | Anims | Pri | Status |
|---|---|---|---|---|---|
| Shambler T1 / Ghoul T2 / Wight T3 | Tripo → Blender LODs | 1.2–2k | Idle Walk Attack Hit Die | P0 | todo |
| Brute T1 / Ironhide T2 / Warhide T3 | Tripo bark-ogre (**not knight**) | 3–5k | Idle Walk Attack Hit Die Roar | P0 | todo |
| Cinderling T1–T3 | Tripo small imp pack | 0.8–1.2k | Idle Run Attack Die | P1 | todo |
| Embercaller T1–T3 | Tripo + staff prop | 1.5–2.5k | Idle Walk Cast Revive Die | P1 | todo |
| Bonebow T1–T3 | Tripo skeleton archer | 1.2–2k | Idle Walk Shoot Hit Die | P1 | todo |
| Gravebinder T1–T3 | Tripo raiser | 2–3k | Idle Walk Cast Raise Die | P1 | todo |
| Hornback T1–T3 | Tripo goat-beast charger | 2.5–4k | Idle Run Charge Hit Die | P1 | todo |
| Carrion Flock T1–T3 | Tripo bird ×1, instance ×3 | 0.6–1k | Fly Dive Die | P1 | todo |
| Rotbloater T1–T3 | Tripo bloater | 1.5–2.5k | Idle Walk Explode Die | P1 | todo |
| Dirtgrub T1–T3 | Tripo burrower | 1.5–3k | Burrow Emerge Attack Die | P1 | todo |
| Mistwisp T1–T3 | Tripo ghost (alpha mat) | 1–2k | Float Phase Attack Die | P1 | todo |
| Ramfiend T1–T3 | Tripo siege beast | 4–6k | Idle Walk Smash Hit Die | P1 | todo |
| Bog Leaper T1–T3 | Tripo | 1.5–2.5k | Idle Hop Jump Attack Die | P1 | todo |
| Bog Spitter T1–T3 | Tripo | 1.5–2.5k | Idle Walk Spit Die | P1 | todo |
| Frost Pack T1–T3 | Tripo wolf | 1–2k | Idle Run Bite Die | P1 | todo |
| Wild Boar | Quaternius / Tripo | 0.8–1.5k | Idle Walk Flee Die | P0 | todo |
| Elite affix decals/VFX (7) | Blender kitbash + particles | — | Loop auras | P1 | todo |

## Bosses

| Asset | Source | Tris | Anims | Pri | Status |
|---|---|---|---|---|---|
| Treant | Tripo→mobile LODs exist | 4k (prod OK; style guide 8–12k optional) | Idle Stomp Attack Summon Hit Die | P0 | **done-not-integrated** |
| Bog Hydra | Tripo 3-head | 10–14k | Idle Spit Bite Enrage Die | P1 | todo |
| Frost Titan | Tripo | 10–14k | Idle Stomp IceWall Shatter Die | P1 | todo |
| Dune Maw | Tripo | 10–14k | Burrow Emerge Bite Die | P2 | todo |

## Hero & workers

| Asset | Source | Tris | Anims | Pri | Status |
|---|---|---|---|---|---|
| Hero Viking Leather (Lv1) | Tripo + Synty Vikings proportions | 2.5–4k | Idle Walk Run Carry Chop Attack Hit Die | P0 | todo |
| Hero Iron / Viking Lord (Lv2–3) | Gear/material swaps on same rig | same | same | P1 | todo |
| Worker Peasant / Builder / Master (×4) | Synty Fantasy Kingdom / Quaternius | 1.5–2.5k | Idle Walk Carry Chop Deposit | P0 | todo |

## Allied units (Lv1–3 each)

| Asset | Source | Tris | Anims | Pri | Status |
|---|---|---|---|---|---|
| Soldier: Militia → Iron Spearman → Royal Guard | Synty Knights / Fantasy Kingdom | 1.5–2.5k | Idle Walk Attack Hit Die | P0–P1 | todo |
| Musketeer: Trapper → Musketeer → Elite | Synty + firearm prop | 1.5–2.5k | Idle Walk Aim Shoot Hit Die | P1 | todo |
| Knight: Squire → Knight → Paladin | Synty Knights (**ally only**) | 2–3k | Idle Walk Attack Hit Die | P1 | todo |

## Weapons (6 types × readable tiers)

| Asset | Source | Tris | Notes | Pri | Status |
|---|---|---|---|---|---|
| Sword / Axe / Spear / Bow / Firearm / Shield | Synty Fantasy Kingdom + Vikings; Tripo fill | ≤400 each | Wood→Iron→Rare→Royal readable at phone; attach sockets on hero/allies | P1 | todo |

## Enchantment VFX (8)

| VFX | Look | Pri | Status |
|---|---|---|---|
| Fire, Ice, Lightning, Holy, Shadow, Nature, Arcane, Blood | Weapon trail + hit decal + brief aura; Synty-style particles / custom URP | P1 | todo |

## Walls, gate, defenses (staged groups)

| Asset | Groups / tiers | Source | Tris | Pri | Status |
|---|---|---|---|---|---|
| Wall T1–T3 | 5: Post Rail Plank Brace Cap (+Band T2) | Batch A kitbash `art/batch_a/walls/` (Synty polish later) | 172–288 | P0 | **done-not-integrated** |
| Gate T1–T3 | 5: Frame L/R, Lintel, Door, Hardware, Spike/Portcullis | Batch A kitbash `art/batch_a/gates/` | 284–512 | P0 | **done-not-integrated** |
| Watchtower T1–T3 | 6: Leg Brace Platform Railing Ladder Roof | Synty Fantasy Kingdom | 1.5–3k | P0 | todo |
| Turret / ballista T1–T3 | 5: Base Mount Beam Arms Ammo | Synty siege / Tripo detail | 1–2.5k | P0 | todo |
| Spike trap T1–T3 | 3: Bed Spike Tip (wood→bone→iron mats) | Synty / Kenney Survival fence | 200–500 | P0 | todo |
| Campfire T1–T3 | 3: Ring Fuel Flame (+`VFX_Fire`) | Kenney Nature → Synty polish | 300–800 | P0 | sourced-cc0 ring |

## Buildings

| Asset | Groups | Source | Tris | Pri | Status |
|---|---|---|---|---|---|
| Hut (worker) | 4: Floor Wall Door Roof | Synty Fantasy Kingdom | 2–4k | P0 | todo |
| Trader stall | 3–4 modular | Synty / Kenney Fantasy Town | 1.5–3k | P1 | todo |
| Research / Workbench / Forge | 3–4 | Synty Adventure props | 1–2.5k | P1 | todo |
| Map-2 gate (locked tease) | 1 set + lock VFX | Synty gate variant | 0.8–1.5k | P1 | todo |
| Upgrade pads (bag/speed) | 4: Base Pillar IconMount GlowRing | Tripo simple / custom | 200–600 | P0 | todo |

## Supplies / pickups

| Asset | Source | Tris | Pri | Status |
|---|---|---|---|---|---|
| Wood plank + carried stack | Kenney Survival wood / custom instance | ≤80 + stack instances | P0 | sourced-cc0 ref |
| Coin | Custom / Kenney | ≤80 | P0 | todo |
| Stone, Iron chunk | Custom atlas props | ≤80 | P0 | todo |
| Meat, Bones, Hide | Tripo small props | ≤120 | P0 | todo |
| Monster core (emissive), Treant sap | Tripo / custom emissive | ≤120 | P0 | todo |
| Carried-stack visuals (log / glowing plank skins) | Instance meshes | — | P1 | todo |

## Environment

| Asset | Source | Tris | Pri | Status |
|---|---|---|---|---|---|
| Trees (pine/oak) + stump + log | Batch A `art/batch_a/environment/` (Kenney recolor) | 196–292 | P0 | **done-not-integrated** |
| Rocks / stone nodes | Batch A env rocks S/M/L | 80 | P0 | **done-not-integrated** |
| Ground grass/dirt + path | Batch A ground/path tiles + GROUND_PLAN.md | 2 | P0 | **done-not-integrated** |
| Forest edge / night lane eyes | Batch A Env_ForestEdge_01/02 (eyes VFX still todo) | 560 | P0 | **done-not-integrated** (eyes todo) |
| Locked map-2 swamp tease (edge) | Palette swap + gate | — | P1 | todo |

## UI icons

| Asset | Source | Size | Pri | Status |
|---|---|---|---|---|---|
| Supply icons (wood coin stone iron meat bones hide core sap) | Custom flat | 128–256 atlas | P0 | todo |
| Unit icons (hero worker soldier musketeer knight) | Custom / Synty INTERFACE later | 128–256 | P1 | todo |
| Pad / build icons | Custom | 128–256 | P0 | todo |

## Core VFX

| VFX | Source | Pri | Status |
|---|---|---|---|
| Hit sparks / blood-sap | Custom URP particles | P0 | todo |
| Death puff / dissolve | Custom | P0 | todo |
| Build-stage land puff + wood chips | Custom (matches staged builds) | P0 | todo |
| Level-up / fort complete pop | Custom | P0 | todo |
| Loot fountain magnet | Custom | P0 | todo |
| Night warning / Blood Moon sky | Volume + particles | P1 | todo |

---

## Notes for producers

1. **Integrate Treant first** in Batch A — already on disk as `Enemy_Treant_LOD*`.  
2. Prefer **one shared atlas material** per pack family (ART_STYLE §4).  
3. Brute must **never** read as the ally Knight.  
4. Synty Fantasy Kingdom is the highest-leverage pack for Batch A–B buildings (Edwin purchase). Until then, Kenney CC0 refs unblock grey→readable.  
5. Generator of record = **Tripo Pro** (Meshy dropped — ignore older Meshy lines in ART_STYLE §11).
