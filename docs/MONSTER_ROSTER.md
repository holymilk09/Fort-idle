# Fort Idle — Monster Roster (art + spawn shopping list)

> Original names/designs only. Diablo archetypes inspired the *roles*, never Blizzard names or likenesses.  
> Forest/fort theme, Synty-style low-poly. Plugs into Night Threat (`PROGRESSION_ECONOMY.md` §2.4–2.5).  
> Soft-launch focus = maps 1–3 + endless reuse. Maps 4–5 bosses listed as P2 stubs.

## Evolution tiers (all combat monsters)

| Tier | Name | Visual | When (threat / HPx) |
|---|---|---|---|
| T1 | Base | Default silhouette | Spawn unlock |
| T2 | Veteran | Armor plates / color shift / eye glow | `HPx ≥ 2` (count-cap HP double) **or** Night Threat ≥ ~8k |
| T3 | Champion | Extra silhouette part + 1 elite affix baked in | `HPx ≥ 4` **or** elite roll (`min(25%, 2%×(n−5))` from N6) |

**Elite affixes (decals / VFX, any T2+):** Armored (iron bands), Swift (speed trails), Burning (ember aura), Shielded (bubble), Splitter (crack lines), Regen (green pulse), Vampiric (red mist). One affix per elite; counters a player choice per economy doc.

**HP baseline:** Shambler = 1.0× (60 HP). Coins ≈ HP ÷ 20.

---

## Roster table

| # | Name | Role | Counterplay | First | Map | HP / dmg vs Shambler | Drops | Evolutions |
|---|---|---|---|---|---|---|---|---|
| 1 | **Shambler** | Fodder | Walls + AoE | N1 | 1–E | 1× / 1× | Coins; meat ~30%; bones ~50% | T1 → **Ghoul** (T2, pale + torn) → **Wight** (T3, horned skull, Armored) |
| 2 | **Brute** | Wall-breaker | Spike Lv2+, hero burst, turrets | N3 | 1–E | 10× / 3× | Coins; iron; hide 2–3; 1 core | T1 bark-ogre → **Ironhide** (T2 bands) → **Warhide** (T3 shoulder plate + Shielded). **Not a knight.** |
| 3 | **Wild Boar** | Passive (day) | None — hunt for meat | Day | 1–2 | ~0.5× / 0 | Meat | No combat tiers |
| 4 | **Cinderling** | Swarm fodder (packs of 4–6) | Cleaves, Fire, spike lanes | N2 | 1–E | 0.6× / 0.8× | Coins; meat low | T1 ember-red imp → T2 scorched → T3 horned + Burning |
| 5 | **Embercaller** | Support (revives 2–3 dead Cinderlings once) | Focus-fire the caller first | N4 | 1–E | 2.5× / 1× | Coins; bones; 1 core ~20% | T1 staff → T2 hooded glow → T3 dual-horn + Regen |
| 6 | **Bonebow** | Ranged | Walls alone fail → need tower/turret | N3 | 1–E | 1.2× / 1.5× ranged | Coins; bones | T1 → T2 quiver armor → T3 dual-bow + Swift |
| 7 | **Gravebinder** | Support (raises 2 Shamblers from corpses) | Kill binder; Holy enchant | N6 / Map2 N1 | 2–E | 3× / 1× | Coins; bones; core ~30% | T1 → T2 bone crown → T3 floating ribs + Shielded |
| 8 | **Hornback** | Charger | Gate HP; slow (Ice); spikes | N4 | 1–E | 4× / 2.5× charge | Coins; hide 1; iron low | T1 goat-beast → T2 plated → T3 ram helm + Armored |
| 9 | **Carrion Flock** | Flyer swarm (3-unit cluster) | Archers / fire arrows; ignore ground spikes | N6 | 2–E | 0.4× each / 0.7× | Coins; meat; hide low | T1 crow → T2 ragged → T3 alpha + Swift |
| 10 | **Rotbloater** | Exploder (dies → AoE vs fort + units) | Kill at range; don’t melee-cluster | N7 / Blood N30 | 2–E | 2× / boom ~4× | Coins; meat; bones | T1 swollen → T2 green veins → T3 Splitter (2 mini-blasts) |
| 11 | **Dirtgrub** | Burrower (surfaces under walls) | Spikes on approach; repair pads | N8 / Map4 tease | 2–E | 3.5× / 2× to walls | Coins; stone dust; iron low | T1 worm → T2 plated segments → T3 armored maw |
| 12 | **Mistwisp** | Phase (ignores walls, hits campfire/hero) | Hero melee + turrets inside; Holy | N9 / Map5 tease | 3–E | 1.5× / 1.5× | Coins; core ~15% | T1 translucent → T2 blue glow → T3 crowned + Vampiric |
| 13 | **Ramfiend** | Siege (priority: walls/gate) | Reinforce walls; ballista focus | N10+ / Blood N50 | 3–E | 8× / 4× to structures | Coins; iron 2; hide; 1 core | T1 siege-beast → T2 iron skull → T3 battering horn + Armored |
| 14 | **Bog Leaper** | Wall-jumper | Towers + inner spikes; Ice slow | Map2 N1 | 2 | 1.8× / 1.2× | Coins; meat; reeds/clay tease | T1 → T2 moss → T3 winged membrane |
| 15 | **Bog Spitter** | Ranged (slows hero via mud spit) | Kill spitters; keep paths clear | Map2 N2 | 2 | 1.5× / 1.3× | Coins; bones; clay | T1 → T2 bloated sack → T3 twin-head spit |
| 16 | **Frost Pack** | Fast pack (wolves ×3–5) | Walls matter; Ice resists less — use Fire | Map3 N1 | 3 | 0.9× each / 1.2× | Coins; hide 1; meat | T1 → T2 frost mane → T3 alpha + Swift |

---

## Bosses

| # | Name | Map / night | HP vs Shambler | Phases (cheap) | Drops | Notes |
|---|---|---|---|---|---|---|
| B1 | **Treant** | Map1 N5 | 50× (3000) | Tank → summon Shamblers → enrage | 5 cores; Treant sap; big coins | **Done (not integrated)** — `art/monsters/treant_golem` |
| B2 | **Bog Hydra** | Map2 N5 | ~80× | 3 heads; kill heads to stop spit | 5 cores; bog essence; coins | GDD name kept; original design |
| B3 | **Frost Titan** | Map3 N5 | ~120× | Stomp → ice wall → shatter | 5 cores; frost essence | GDD |
| B4 | **Dune Maw** | Map4 / endless cycle | ~160× | Burrow under walls → emerge | 5 cores; sand essence | GDD Sand Wyrm role; Blood Moon rotation |

Endless: bosses rotate B1→B4 with +1 affix per cycle. Blood Moon adds: N30 Rotbloater wave, N40 **Stackthief** (P2 — steals carried planks; reuse Mistwisp rig + sack), N50 Ramfiend.

---

## Count

**16 combat/ambient monsters + 4 bosses** (Treant done). Soft-launch must-ship: #1–8, #14–15, B1–B2. P1: #9–13, #16, B3. P2: B4, Stackthief.

## Threat spend order (engine)

`boss → Ramfiend/Brute → elites → Hornback/Gravebinder → ranged → swarm → Shambler/Cinderling remainder` · Cap 150 · promote visual tier on HPx.
