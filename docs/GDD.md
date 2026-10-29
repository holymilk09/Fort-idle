# Fort Idle — Game Design Document (v0.1)

> Numbers marked *(est.)* are our tuning estimates. Sourced: Supersonic (supersonic.com/?p=82671): idle arcade avg CPI ≈ $0.50, avg D0 playtime 1200–1800s, core = moving character + stacking + unlock/expand + upgrades, "no resource left behind". Map 1 numbers reused from MAP1_SPEC.md.

## 1. Pitch
**"The ad you saw — for real."** A lone survivor stacks a ridiculous tower of wood on their back, pours it into glowing build pads, and turns a forest clearing into a fortress before night falls. Every ad (stack, fail, night timer, timelapse, wrong choice, boss) is an actual moment in the game. Polished 3D, a dynamic camera, real survival tension in a genre known for flat visuals.

## 2. Loops
**Core 30s loop:** chop trees (3 planks/0.4s hit) → stack grows (cap 20→120) → run to pad → pour (12→25 planks/s) → structure rises (juice + hit-stop) → next pad lights up.
**Day/night loop (~2.5 min):** Day 90s (first day 60s) of building → "00:20 until night" warning → night 45s defending (hero melee + turrets) → loot fountain → dawn upgrades.
**Session loop (8–15 min, est.):** 3–5 day/night cycles → complete a zone tier or a boss night → spend coins on hero/research → collect offline earnings → map 2 tease visible at the camera edge.
**Long-term loop:** fully fortify map → boss → unlock next biome; old resources stay needed ("no resource left behind": wood is still used in every biome).

## 3. Meta progression
| System | Details *(est.)* |
|---|---|
| Hero upgrades (coins) | Speed 5→7.5 m/s (+10%/lvl), stack cap 20/40/60/90/120 (beyond 120 via research), chop power, melee damage |
| Workers | Hut 150 planks; 200 coins, ×1.5 each, 4 per map (more via research). 3.5 m/s, carry 15, auto tree→pad. Worker roles later: carrier, repairer, gunner |
| Fort tiers | T1 wood → T2 stone → T3 iron → T4 crystal; each tier ×2.5 HP, new visual, needs that biome's resource + wood |
| Research (Workbench) | Persistent tree: pour speed, carry cap, turret DPS, offline cap, worker count; costs coins + gems + biome cores; real-time timers 1m–8h |
| Hero gear/skins | Cosmetic + small stat perks (≤5%); stack skins (logs, glowing planks) |

## 4. Maps / biomes
| # | Biome | New resource | Monsters | Boss | Mechanic twist |
|---|---|---|---|---|---|
| 1 | Monster Forest | Wood | Shamblers (60 HP, 1.5 m/s), Brute (600 HP) | Treant (3000 HP, night 5) | Tutorial; walls, gate, tower, turret |
| 2 | Swamp | Reeds/clay (bricks) | Leapers jump walls, spitters | Bog Hydra (3 heads) | Mud slows hero; build bridges |
| 3 | Frozen Peaks | Stone + ice | Yetis, frost wolves (fast packs) | Frost Titan | Cold meter: keep fires fueled with wood |
| 4 | Desert Ruins | Iron ore | Scarabs (swarm 100+), burrowers | Sand Wyrm (emerges under walls) | Day heat; night is long |
| 5 | Crystal Wastes | Crystals (power) | Wraiths ignore walls, golems | Void King | Power grid: turrets need crystal pylons |

## 5. Night waves & bosses
- Waves (map 1): N1 12, N2 20, N3 30 + brute, N4 40 + 2 brutes, N5 boss + 25 adds *(est.)*. Later maps ×1.5 per biome.
- Spawn from 2–3 treeline lanes, telegraphed by glowing eyes at dusk; lanes rotate so walls need full coverage (WALL vs TURRET choice matters → Ad #5).
- Turret 1: 50 DPS, 12 m. Archer tower: 30 DPS, 18 m, shoots over walls. Hero melee: 40 DPS cleave.
- Bosses: 3 phases (tank → summon adds → enrage), weak spot hit-stop, slow-mo kill cam, cinematic treeline smash entrance (Ad #6).
- Juice: hit-stop 60–120 ms, damage numbers, loot fountain + magnet.

## 6. Fail state
- Fire (core campfire, 1000 HP) destroyed = night lost (the Ad #2 screen crack). Soft fail: you keep all gathered resources and 50% of night loot; structures repair at dawn for planks. Never lose permanent progress.
- Retry options: free retry, or rewarded ad "Second Wind" (revive fire at 50% + 10s turret overdrive), max 1 per night.
- Difficulty valve: after 2 losses, next night −20% monsters *(est.)*.
- Stat hook: "Only X% survive night 1" uses real telemetry once live.

## 7. Economy
| Currency | Type | Source | Sink |
|---|---|---|---|
| Wood/stone/ore/crystal | Map resources (soft, per-map) | Chopping, workers | Pads, fort tiers |
| Coins | Soft | Monster loot, offline earnings, chests | Hero upgrades, workers, research |
| Gems | Hard | IAP, battle pass, achievements, small daily drip (~10/day est.) | Timer skips, skins, extra chests, revive (alt to RV) |
| Biome cores | Progression | Bosses | Research tier gates |
Faucet target: hero upgrade every ~2 min in the first session, ~5–8 min by day 3 *(est.)*.

## 8. Monetization
- **Rewarded video** (main early revenue): 2× night loot, Second Wind revive, instant worker for 3 min, double stack cap for 60s, offline earnings ×3, free chest every 4h. Cap ~12 RV/day *(est.)*.
- **Interstitials:** none before night 3; then max 1 per 3 min, never during nights.
- **IAP:** Remove ads ($4.99), Starter pack ($1.99: hero skin + 300 gems + worker), gem packs ($0.99–$49.99), Builder's Bundle (permanent +1 worker/map, $9.99).
- **Battle pass (Season, 30 days):** free + premium ($7.99) tracks; skins, stack skins, gems, research boosts.
- **Rule:** no paywall in maps 1–2; everything purchasable is also earnable; gear stat perks capped at 5%.

## 9. Retention
- Offline idle earnings: workers keep producing coins (cap 2h, research to 8h); "Welcome back" pop-up with RV ×3.
- Daily: login calendar (7-day, day 7 skin), 3 daily quests, daily "Blood Moon" challenge night (harder, better loot).
- Weekly events: Horde Rush (endless waves leaderboard), Build Race (fill pads fastest), seasonal themes (Halloween pumpkin fort).
- Notifications: research done, offline cap full, event start (opt-in).
- Social later: leaderboards for "highest stack" (Ad #1 hook), clan fort raids (post-MVP).

## 10. Art direction
- **Look:** Synty POLYGON base (Nature/Fantasy/Survival packs) + custom hero and monster silhouettes, bespoke pads/VFX, URP lighting with real day→dusk→night color grading (gray-out/color-burst used in ads and in game when the fire is lit).
- **Camera (key differentiator):** instead of the fixed tilted top-down, a Cinemachine 3rd-person follow at ~50° pitch, 40mm, that dynamically lowers to ~30° when running with a tall stack (so the stack reads), pulls wide when entering a pad zone, cranes up for structure reveals, and orbits for boss/slow-mo moments. Players can pinch-zoom within limits. Camera always teases locked areas.
- **Readability:** muted environment, saturated interactables (pads glow, resources bright), clear silhouettes at phone size.
- **UI:** Clash-level polish bar: chunky buttons with press-squash, big counters, minimal HUD at night.

## 11. Tech
- Unity 6 LTS, URP (mobile renderer), C#.
- Cinemachine 3 (follow, impulse shake, blends), DOTween (builds OutBack, punch, counters), `UnityEngine.Pool.ObjectPool<T>` for planks, enemies, projectiles, coins, VFX.
- Stack: render ≤60 plank meshes via GPU instancing, then compress visually beyond; physics-free spring sway.
- Enemies: simple steering + flow field per lane (no NavMesh agents en masse); up to 150 enemies on screen with instancing / ECS-lite update loop.
- Perf targets: 60 fps on iPhone 12 / Galaxy A54-class, 30 fps floor on 3 GB Android; ≤150 draw calls, ≤300k tris, app size ≤150 MB with addressables for biomes 3–5.
- Save: local JSON + cloud sync (Unity Cloud Save or PlayFab); offline earnings via server time check.
- Analytics: Firebase/GameAnalytics; ads mediation: AppLovin MAX or Unity LevelPlay; playable via Luna.

## 12. MVP plan (12 weeks, 1–3 people, est.)
| Weeks | Milestone | Exit criteria |
|---|---|---|
| 1–2 | Prototype core loop | Gray-box: joystick, chop, stack, pour, wall rises; feels good at 60fps |
| 3–4 | Night combat | Waves, turret, hero melee, fire fail state, loot fountain |
| 5–6 | Map 1 complete | All pads, workers, 5 nights + Treant boss, Synty art pass 1 |
| 7 | Meta | Hero upgrades, research v1, offline earnings, save/load |
| 8 | Monetization + analytics | RV placements, IAP starter/remove-ads, events tracked |
| 9 | Map 2 (Swamp) | New resource + leapers + Hydra |
| 10 | Juice & camera polish, ads | Ads #1 and #3 captured from real gameplay; playable ad (30s flow from MAP1_SPEC) |
| 11 | CPI test | Run ad creatives on Meta/TikTok for marketability (target CPI ≤ $0.50, sourced genre avg) |
| 12 | Soft launch (e.g., PH/CA/AU, est.) | Live ops dashboard, first balance patch |

**Soft launch KPI targets *(est. unless noted)*:**
| KPI | Target |
|---|---|
| CPI (US) | ≤ $0.50 (Supersonic genre avg, sourced); tier-1 hybrid-casual may run higher |
| D0 playtime | 1200–1800s (sourced genre avg) |
| D1 retention | ≥ 40% *(est.)* |
| D7 retention | ≥ 12% *(est.)* |
| Night 1 survival | 70–85% *(est.)* |
| RV per DAU | ≥ 4 *(est.)* |
| Crash-free sessions | ≥ 99.5% |

## 13. How the ads map to the game
| Ad | Ad moment | Where it's real in the game |
|---|---|---|
| 1 The Stack | Absurd stack pours into pad, wall/gate/tower rise | Core loop; stack cap upgrades to 120+; "highest stack" leaderboard; stack skins |
| 2 The Fail | Slow gathering, monsters break in, "Only 2% survive night 1" | Real fail state (fire destroyed); Second Wind revive; survival % from live telemetry (don't claim 2% unless true) |
| 3 Night Falls | "00:20 until night" rush, turret finished at 00:01, slow-mo shred | Day timer warning at 20s; turret pads; slow-mo kill cam on wave clear; loot fountain |
| 4 Zero to Fortress | Timelapse, workers looping, gray → color when fire lit | Worker system; fort tiers; in-game "replay timelapse" share feature; fire lighting color grade |
| 5 Wrong Choice | WALL vs TURRET pads, archers shoot over wall | Branching pad choices each day; archer enemies (maps 1–2) punish wall-only builds |
| 6 The Boss | Giant breaks treeline, upgraded hero melts it, next zone revealed | Night 5 boss per biome (Treant first); boss kill reveals next biome via camera pull-out |

## Monster drops & extra supplies (added 2026-10-27, Matt ask; design only, not built)
| Supply | Source | Use |
|---|---|---|
| Meat | Shamblers (~30% drop), wild boars in the forest by day | Cook at the campfire: hero heals; a "Hearty meal" at dawn gives workers +25% speed next day |
| Bones | Shamblers (~50% drop) | Bone spikes: spike trap upgrade (more damage, hits brutes) |
| Hide | Brutes (guaranteed 2–3) | Bigger backpack: stack cap upgrade tier; leather armor for hero HP |
| Monster cores (glowing) | Brutes (1), Treant (5) | Research/workbench; unlock map 2 gate |
| Treant sap | Treant boss | Premium turret ammo (fire bolts) |
All amounts go in FortIdleConfig. Drops fountain out like coins and auto-collect when the hero walks near.

## Defense level-ups, map 1 (proposed 2026-10-27, Matt ask; design only, not built)
| Structure | Lv1 | Lv2 | Lv3 |
|---|---|---|---|
| Wall | Wood palisade, 300 HP (wood) | Reinforced, iron bands, ~600 HP (wood+stone) | Stone wall, 1100 HP, monsters stagger on hit (stone+iron) |
| Gate | Wood gate, 500 HP | Iron-bound, 1000 HP, auto-closes at night | Portcullis, 1800 HP, spikes damage attackers |
| Watchtower | +30% vision, 1 archer, 30 DPS, 18 m | 2 archers, 60 DPS, roof (archers safe) | Fire arrows: burn damage, 22 m, spots boss weak points |
| Turret (ballista) | 50 DPS, 12 m | 80 DPS, 15 m, bolts pierce 2 enemies | 120 DPS, 18 m, fire bolts with Treant sap (splash) |
| Spike trap | Wood spikes, hits shamblers, slows | Bone spikes (bones), hits brutes too | Iron spikes, bleed damage, re-arms faster |
| Campfire (core) | 1000 HP | Stone ring, 1500 HP, cooks meat | Bonfire, 2200 HP, warmth heals nearby hero/workers |
Each level: new model + build pad glow; costs and stats live in FortIdleConfig. Fixes queued: towers/turret become attack targets; spikes work on brutes from Lv2.

## Map 1 monsters & drops, compiled (2026-10-27; drops beyond coins/iron are proposed)
| Monster | HP | Appears | Drops |
|---|---|---|---|
| Shambler (zombie) | 60 | Every night (N1 12, N2 20, N3 30, N4 40) | Coins; meat ~30%; bones ~50% |
| Brute (knight) | 600 | N3 (1), N4 (2) | Coins; iron; hide 2-3; 1 monster core |
| Treant (boss) | 3000 | N5 + 25 shamblers | Big coin fountain; 5 monster cores; Treant sap |
| Wild boar (day, not hostile) | low | Forest by day | Meat |
Non-monster supplies: wood (trees), stone (rocks after N1).

## Element sheet from Matt, 2026-10-27 15:21 CST (reference image: /home/box/agent-data/agents/44b721a4-6eb3-4771-8bca-c658d6edac39/attachments/22de6a5af5b204d804b85e66c4a9c09727e9cfebffdc87863b34154b94ff34d7.png)
Allies (Lv1/Lv2/Lv3): Hero Viking (Leather/Iron/Viking Lord); Workers up to 4 (Peasant/Builder/Master Builder); Soldier spearman, frontline (Militia/Iron Spearman/Royal Guard); Musketeer, ranged behind walls (Trapper/Musketeer/Elite Musketeer); Knight, elite melee (Squire/Knight/Paladin).
Weapons (hero + units): Swords (Wooden, Iron, Knight's Longsword, Royal Blade); Axes (Wood, Iron, Battle, Runic); Spears (Wood, Iron, Halberd, Royal); Bows (Short, Hunting, Recurve, Elven); Firearms (Hand Cannon, Musket, Repeating Musket, Arcane Rifle); Shields (Wood, Iron, Knight, Holy Bulwark).
Enchantments: Fire (burn, vs hordes), Ice (slow, vs fast), Lightning (chain, vs groups), Holy (vs undead, stun), Shadow (life steal, vs high HP), Nature (DoT, synergy w/ Treant sap), Arcane (armor reduce), Blood (bleed stacks, vs brutes/boss).
Open questions: Knight is now an ally, so the brute needs a new look; recruitable fighters are a new system (where they come from, cost); possibly more sheets coming.
