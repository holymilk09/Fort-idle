# Reward report — Map 1 demo

Seed `1401`, `-demo` self-play. Times are game seconds from match start. Ledger figures are earned/spent. Night rows are the dawn (or resolve) snapshot: fire never hit 0, so every night cleared.

## Timeline

| t | Purchase |
| --- | --- |
| 14.0 | Wood wall |
| 44.6 | Turret |
| 92.2 | Hearty meal |
| 93.5 | Sharpen 1 |
| 95.0 | Melee 1 |
| 96.5 | Melee 2 |
| 144.3 | Hut |
| 160.3 | Spikes |
| 206.3 | Hearty meal |
| 207.6–210.6 | Sharpen 2–4 |
| 212.2 | Melee 3 |
| 214.3 | Hire 1 |
| 253.3 | Reinforced wall |
| 255.5 | Gate |
| 276.7 | Campfire Lv2 |
| 355.1 | Campfire Lv3 |
| 355.6 | Hearty meal |
| 356.5 | Backpack 1 |
| 357.0 | Broth 1 |
| 404.7 | Bone spikes (Lv2, hits brutes) |
| 512.5 | Hearty meal |
| 513.4 | Backpack 2 |
| 513.9–516.7 | Broth, armor 1, broth, armor 2, broth |
| 534.6 | Iron-bound gate |
| 582.2 | Watchtower |
| 650.0–652.9 | Research ranks 1–3 (after the Treant drop) |
| 658.9–673.8 | Broth 4–15 |
| 674.7 | Stack cap (coins) |
| 675.9 | Move speed (coins) |

First hero upgrade is melee 1 at **95.0s**. Melee 3 lands at 212.2s.

## Gaps

Gaps are seconds between consecutive `reward=` lines.

| Window | Median | Max |
| --- | --- | --- |
| First 180s (7 gaps) | 16.0s | 47.8s |
| Whole run (47 gaps) | 1.4s | 107.8s |

The 107.8s max is night 4 (bone spikes at 404.7s, next spend at 512.5s). Early session stays inside a minute. The whole-run median is short because broth, sharpen, and research chain while the hero stands on a pad.

## Supplies per day

| Day | Meat | Bone | Hide | Core | Sap | Iron | Coin | Wood | Stone |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 5/0 | 5/0 | 0/0 | 0/0 | 0/0 | 0/0 | 53/0 | 160/160 | 0/0 |
| 2 | 8/2 | 10/4 | 0/0 | 0/0 | 0/0 | 0/0 | 95/43 | 200/200 | 2/2 |
| 3 | 9/2 | 18/12 | 2/0 | 1/0 | 0/0 | 4/0 | 162/93 | 109/94 | 18/18 |
| 4 | 12/2 | 23/11 | 6/1 | 2/0 | 0/0 | 8/4 | 205/0 | 163/175 | 4/4 |
| 5 | 7/2 | 17/9 | 0/7 | 0/0 | 0/0 | 0/8 | 135/0 | 199/187 | 16/16 |

Day 5's ledger is the resolve instant, before the Treant fountain landed. After that: cores 3→8 and all 8 spent on research; sap 0→2 and still held; bones kept feeding broth; coins bought one stack tier and one speed tier.

## Nights

| Night | Wall HP | Gate HP | Fire HP | Outcome |
| --- | --- | --- | --- | --- |
| 1 | 0 / 300 | — | 1000 | Clear. Wood wall broke; the last shamblers died before the fire. Rebuilt at dawn. |
| 2 | 0 / 300 | — | 1000 | Clear. Same pattern on the repaired wood wall. |
| 3 | 336 / 600 | 0 / 500 | 1500 | Clear. Reinforced wall held. New gate was destroyed. |
| 4 | 48 / 600 | 48 / 500 | 2200 | Clear. Both structures nearly gone, fire full. |
| 5 | 120 / 600 | 0 / 1000 | 2200 | Clear, close. Iron gate was destroyed, reinforced wall left at 120, fire untouched. |

`FORTIDLE save=roundtrip ok` at resolve.

## Dead supplies

No supply is earned with nothing to spend it on.

Sap is the one stockpile this run did not spend. The Treant drops 2 sap, and the sink is turret Lv3 fire bolts. That pad also wants the day-5 iron upgrade (12 wood + 4 iron) before the sap dwell will take. This run put the iron into the campfire and the iron-bound gate, then raised the watchtower as night 5 started, so the upgrade pad never finished. Meat, bones, hide, iron, and cores all have spends in the log (meals, spikes and broth and sharpen, backpack and armor, wall/gate/fire, research).

## Adaptive nights — seed 1401, nights 1–10

Same seed, generated nights. Fort Power is `0.5 × DPS × 45 × sqrt(1 + fortHP / 5000)`. Night Threat is the map budget times a band of `(Power / expected) ^ 0.25`, clamped to 0.85–1.20. The band stays at 1.00 for nights 1–3. Coins are enemy HP / 20. Nights 1–5 keep the authored headcount (12 / 20 / 30+1 / 40+2 / Treant+25) and scale hit points when the band moves. Nights 6–10 use the composition formula on map-2 budgets (`×1.6`). No loss retry fired (`maxTries` 6 was unused). `FORTIDLE save=roundtrip ok`.

| Night | Power | Threat | Band | Ratio | Roster | Coins banked | Win | Dawn wall / gate / fire |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2273 | 720 | 1.00 | 0.32 | 12 | 33 | Clear | 48 / — / 1000 |
| 2 | 2879 | 1200 | 1.00 | 0.42 | 20 | 54 | Clear | 0 / — / 1000 |
| 3 | 3915 | 2400 | 1.00 | 0.61 | 30 + 1 brute | 117 | Clear | 264 / 0 / 1000 |
| 4 | 4344 | 3511 | 0.98 | 0.81 | 40 + 2 | 174 | Clear | 0 / 0 / 1000 |
| 5 | 4124 | 4347 | 0.97 | 1.05 | 25 + Treant 2898 HP | 75 | Clear, close | 0 / 0 / 1000 |
| 6 | 4827 | 1382 | 1.20 | 0.29 | 23 | 66 | Clear | 1100 / 802 / 1000 |
| 7 | 7682 | 2304 | 1.20 | 0.30 | 38 | 108 | Clear | 1100 / 1000 / 1000 |
| 8 | 7682 | 4250 | 1.11 | 0.55 | 60 + 1 brute | 180 | Clear | 1100 / 1000 / 1000 |
| 9 | 8052 | 5829 | 1.01 | 0.72 | 77 + 2 brutes | 261 | Clear | 1100 / 1800 / 1000 |
| 10 | 10100 | 7736 | 1.07 | 0.77 | 32 + 1 brute + boss 5160 HP | 126 | Clear | 1100 / 584 / 1500 |

Dawn columns are the next morning (night 10 is the resolve snapshot). Nights 1–2 still break the wood wall; the fire stays up. Night 5 is the tight one (ratio 1.05, reinforced wall already at 0 from night 4). Night 10’s stone wall stays full and the gate ends at 584. The on-screen cap of 150 is not reached (peak 77 shamblers).

Coins banked are what the clear line counted. HP / 20 still in the air is missing from that instant: night 5’s Treant (~145) lands on the next morning (day-6 ledger coins earned 211), and night 10’s boss (258) lands in the first status line after resolve (coins 1085 → 1343). Shambler nights sit within a few coins of HP / 20 (night 1 is 33 vs 36).

### Worker ROI

One worker, hired at t=691.6 for 200 coins (`200 × 1.5^level`, plus 3 planks per chop). Wood in the log is planks credited to that worker.

| After night | Wood | Coins spent | Wood / coins |
| --- | --- | --- | --- |
| 6 | 69 | 200 | 0.35 |
| 7 | 225 | 200 | 1.13 |
| 8 | 540 | 200 | 2.70 |
| 9 | 885 | 200 | 4.43 |

The hire pays back its coin cost in wood before night 8. The run stops at night 10, so there is no dawn line after the last fight; 4.43 is the last logged ratio.

### Supplies spent

Ledger spend across the ten days, then the fountain that lands after the banner:

| Supply | Spent during the ten days | Still held at clear | Later spend |
| --- | --- | --- | --- |
| Meat | Meals and cook | 102 | Trader `sell-meat` from t=1364.6 |
| Bones | Sharpen, spikes, broth | 21 | Broth until 0 after the banner |
| Hide | Armor and backpack (days 4 and 6) | 10 | Patches after the banner |
| Iron | Wall, gate, spikes, turret | 4 | Campfire Lv3 |
| Cores | Core focus ×4 (t=848–852) | 8, then 13 after the fountain | Research ranks 1–3 |
| Sap | Wick ×2 (t=845.6, 847.1) | 0, then 2 from the Treant | Turret Lv3 fire bolts at t=1290.5 |

No supply is earned with nothing to spend it on. The clear snapshot still holds meat, bones, hide, iron, and cores because the demo keeps playing on the result banner and spends the boss fountain there. The trader buys meat above the reserve of 4; bones and hide are sold only after meat.

## Staged construction — 2026-10-28

Seed 1401. `-stages` builds the wall, gate, watchtower, and turret from separate parts. Stage counts: wall 5, gate 5, watchtower 6, turret 5, spikes 3, campfire 3, hut 4. Upgrades use 4 stages (wall, gate, tower, turret, fire). A half-built hut restored from the save blob (`FORTIDLE stage-save ok shown=1`). The close camera sits on the wall for all five stages, then the gate, tower, and turret. Puffs are capped at 28 chips so four workers pouring at once do not spawn a puff per plank.

## Trees, ground, frame time — 2026-10-28

The clips were choppy because this machine renders with **llvmpipe** (software OpenGL, no GPU). Presented frames were about 110–150 ms, which is roughly 7–9 fps. That is the capture, not a phone. A mid-range phone with the mobile budget below is the 60 fps target; this VM cannot hit it.

`-perf` on the same 1280×720 window, seed 1401, four workers. Times are presented milliseconds. `allocAvg` is mono bytes per frame. Draw calls come from the renderer profiler.

| Window | | avg | p95 | p99 | worst | alloc | draws | batches | tris |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Quiet day | before | 130.76 | 135.80 | 136.11 | 136.11 | 3822 | 736 | 736 | 186855 |
| Quiet day | after | 107.75 | 110.77 | 114.05 | 114.05 | 2214 | 454 | 454 | 117079 |
| Night 5 (25 + boss) | before | 138.73 | 142.76 | 147.26 | 147.26 | 8988 | 1305 | 1305 | 438291 |
| Night 5 (25 + boss) | after | 112.51 | 115.81 | 117.09 | 117.09 | 7819 | 797 | 797 | 273173 |
| 150 shamblers | before | 149.14 | 176.07 | 176.64 | 176.64 | 8812 | 1130 | 1130 | 384559 |
| 150 shamblers | after | 121.62 | 143.69 | 147.91 | 147.91 | 7782 | 771 | 771 | 274483 |

Trees were lying down because the Kenney meshes are Z-up and `FortArt.Stand` cleared the importer rotation. A mesh whose renderer bounds are longer on Z than Y now gets −90° on X, then a random yaw and at most 2.5° of tilt. The first tree measured height 3.60 m against width 3.21 m. The same stand path covers the forest, the edge props, and the trees behind the locked map-2 gate.

The ground is one 512² albedo (grass noise, dirt paths, a soft forest edge) on a quad with URP Lit, plus instanced tufts and pebbles on a second unlit shader. No terrain detail system. Mobile quality in `FortIdleConfig` turns off HDR, sets MSAA to 1, render scale 0.85, one shadow cascade, and an 18 m shadow distance. The Android player module is not installed in this editor, so there is no Android build size.
