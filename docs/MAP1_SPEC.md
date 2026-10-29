# Fort Idle — Map 1 Spec (derived from storyboards)

**Sourced benchmarks** (Supersonic, https://supersonic.com/?p=82671): idle arcade avg CPI ≈ **$0.50**; avg D0 playtime **1200–1800s**; core features = constantly moving character, stacking, unlock/expand/explore, upgrades; "no resource left behind" (wood stays needed all game); camera should tease locked areas; RV/IAP for speed, hiring, capacity.
**Everything numeric below is an ESTIMATE** (our tuning starting points). Validate by frame-stepping 60fps recordings of Pro Builder / Burger Please / My Perfect Hotel — not yet done.

## Player
- Move speed: 5.0 m/s base (est.); upgrades +10%/level to 7.5 m/s. Joystick, no accel lag (reach full speed ≤0.1s).
- Stack capacity: 20 → 40 → 60 → 90 → 120 (5 upgrade levels; est.). Ads show 120 as "absurd" endgame.
- Stack visual: 1 plank = 0.12m height; sway via spring; cap visual at 60 rendered planks + scale trick beyond (pooled).
- Chop: 1 tree hit/0.4s, yields 3 planks; stumps respawn 8s.

## Pads (pour)
- Pour rate: 12 planks/s base (est.), accelerating to 25/s after 1s held on pad. 120 planks ≈ 6s (ad shows 3s → ad build uses 40/s cosmetic).
- Pour only when standing on pad; partial progress saved.

| Build | Cost (planks) | Effect |
|---|---|---|
| Wall segment | 40 | HP 300 |
| Gate | 60 | HP 500, workers pass |
| Watchtower | 100 | +30% vision, archer slot |
| Turret 1 | 80 | DPS below |
| Worker hut | 150 | unlocks hiring |
| Upgrade pad (bag) | 50 → ×1.6/level | +stack capacity |

## Day/Night
- Day 90s (night 1 day: 60s), warning at 20s ("00:20 until night"), night 45s (est.).
- Night 1 wave: 12 monsters (60 HP, 1.5 m/s); night 2: 20; night 3: 30 + 1 brute (600 HP); boss night 5 (3000 HP).
- Turret: 25 dmg × 2 shots/s = **50 DPS**, range 12m; kills night-1 wave in ~15s if aimed (est.).
- Loot: 5 coins/monster; coins fund upgrades (wood builds, coins upgrade → two-resource loop).

## Workers
- Hire at hut: 200 coins, then ×1.5 each (max 4 on map 1).
- Worker: 3.5 m/s, capacity 15, auto-loop tree→nearest unfinished pad. Ad #4 timelapse uses 4 workers.

## Session pacing target
Map 1 completes in ~12–15 min (fits the 1200–1800s D0 benchmark with map 2 tease visible at camera edge).

## 30s Playable Ad flow
> Measured reference (2026-10-12, ffmpeg scene-detect + 4fps frame step on official Google Play store trailers, 360p, in refs/): end card / logo appears at **27.5s of 29.8s** (Burger Please, https://youtu.be/w9oWc_Q-its), **~28.4s of 30.0s** (My Perfect Hotel, https://youtu.be/6rcVjMYFwg8; scene cut 28.40s), Pro Builder 3D shows an in-game offer popup at **~28.75s of 29.4s** and has no end card (https://youtu.be/48Lv-8qhetk). So all 3 put the brand/CTA card in the last ~1.5–2.5s of a 30s clip. These are store trailers, not playable ads, so the 23s persistent-CTA timing below stays **est.**; the full-screen CTA at about 30s matches the trailers.
| Time | Beat |
|---|---|
| 0–3s | Hand-pointer tutorial on joystick; hero next to trees; "Stack wood!" |
| 3–10s | Player chops; stack capped at 40 (fast yields ×2) |
| 10–15s | Arrow to WALL pad (cost 40); pour 40/s; wall rises |
| 15–17s | Timer "00:10 until night"; arrow to TURRET pad (cost 30) |
| 17–23s | Gather + pour turret; completes |
| 23s / turret first shot | **Install button appears** (persistent CTA), slow-mo horde wave of 10 |
| 23–30s | Loot fountain; end card "Can you survive the night?"; full-screen CTA at 30s or on idle 5s |
Fallback: if player idles 4s at any point, auto-play continues so turret fires by ≤25s. Budget: <5MB, MRAID 3.0, Unity → Luna/playable export.

## Tech
Unity URP, Cinemachine 3 (Follow + Impulse), DOTween (OutBack builds, punch), ObjectPool<T> for planks/enemies/coins, Synty POLYGON packs with custom hero/monster silhouettes.
