# Fort Idle — handoff for Claude (public mirror)

Public repo: **https://github.com/holymilk09/Fort-idle** (code-only mirror of private Origin `matt-fb/FortIdle`).  
Owner: Matt. Soft-launch target: **iOS App Store + Google Play**. Engine: **Unity 6000.3.26f1**, **URP** (mobile renderer).

This file is the on-ramp. Design detail lives under `docs/` (same set as this mirror's companion pack). Do not expect paid Synty / Tripo meshes in the public tree.

---

## 1) What the game is

**Fort Idle** is a mobile idle/arcade fort builder: chop wood → stack planks → pour into glowing build pads → fort rises before night → defend the campfire against waves → dawn upgrades → repeat. Pitch: *"The ad you saw — for real."*

- **Map 1** = Monster Forest tutorial (walls, gate, tower, turret, hut, workers, nights 1–5 + Treant boss).
- Later biomes (design): Swamp, Frozen Peaks, Desert Ruins, Crystal Wastes.
- Monetization / soft-launch KPIs: see `docs/GDD.md`. Numbers marked *(est.)* are tuning starts, not live telemetry.

Milestones (team): M0 foundation → M1 genre look → M2 rewarding loop → **M3 AAB + TestFlight** → M4 soft launch → M5 polish. Target ~4–6 weeks to soft launch from late Oct 2026.

---

## 2) How to open and run

| Item | Value | Confidence |
|---|---|---|
| Unity version | **6000.3.26f1** (Unity 6 LTS) | Verified in `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | URP; `Assets/Settings/Mobile_RPAsset.asset` + Mobile_Renderer | Verified from repo file list |
| Main scene | `Assets/FortIdle/Scenes/Map1.unity` | Verified |
| Tuning | **Single source:** `Assets/FortIdle/Config/FortIdleConfig.asset` (+ `FortIdleConfig.cs`) | Verified |
| Self-play | Linux/player flag **`-demo`** (drives `DemoDriver`) | Verified |
| Perf capture | **`-perf`** → `PerfHarness.Requested()` scans `GetCommandLineArgs()` for exact `-perf`; also seeds RNG 1401 and attaches `FrameMeter` when `-demo`/`-perf`/`-qashot` | Verified in source |
| Elements grid | **`-elements`** → `ElementSheet.Requested()`; Bootstrap early-outs before world build | Verified in source |
| License | Unity Personal (or Plus) required to enter Play Mode / build | Account details are out-of-band — never commit licenses |

**Map1 bootstrap (verified):** `EditorBuildSettings` lists only `Assets/FortIdle/Scenes/Map1.unity`. Scene GO `FortIdle` has `FortIdle.FortIdleBootstrap` with `config` → `Assets/FortIdle/Config/FortIdleConfig.asset` (guid `01090d4eed…`), plus `litMaterial`/`emissionMaterial` (`URPLit` / `URPLitEmission`). `Awake` assigns `FortWorld.Config`, errors if config null, and for `-elements` builds sun/camera then returns without MatchDirector. Flags `-demo` / `-stages` / `-perf` (or `config.demo.runByDefault`) call `Random.InitState(1401)`.

**Also verified argv flags:** `-stages` (StageShowcase), `-look` (LookReel), `-treeshot` (TreeShots), `-qashot` (QaShot), `-poseshot` (PoseShot). `PoseCheck` lives in `PoseWatch.cs`.

**Still unknown here:** Mac/Windows player packaging beyond shared `GetCommandLineArgs()` parsing; whether Android Build Support is installed on a given machine (historically **not** on the cloud VM).

Cloud capture GPUs are often **llvmpipe (~7–9 fps)**. Judge performance by **draw calls / tris / GC allocs / p95**, not clip smoothness.

---

## 3) Repo map (from Origin code-only tree)

```
Assets/FortIdle/
  Config/FortIdleConfig.asset     # all gameplay numbers
  Scenes/Map1.unity
  Scripts/                        # gameplay (see below)
  Editor/ProjectSetup.cs, FortArtImport.cs
  Materials/URPLit*.mat
  Shaders/FortScatter.shader
  Resources/Art/                  # OFTEN ABSENT in public mirror (paid/CC0 meshes excluded)
  Art/Licenses/                   # may be excluded; see CREDITS.md
Assets/Settings/                  # URP mobile + PC assets
Packages/manifest.json
ProjectSettings/                  # ProjectVersion.txt → confirm 6000.3.26f1
CREDITS.md, PROGRESS.md, REWARD_REPORT.md, README.md, CLAUDE.md; design docs under `docs/` (ART_STYLE.md, GDD.md, …)
```

**Important scripts (C#):**

| Area | Scripts |
|---|---|
| Bootstrap / world | `FortIdleBootstrap`, `FortWorld`, `MatchDirector`, `FortProgress` |
| Config / math | `FortIdleConfig`, `FortMath`, `ThreatMath` |
| Hero / input | `HeroController`, `VirtualJoystick`, `PlankStack` |
| Build pads / staged builds | `BuildPad`, `StagedBuild`, `StageKit`, `AssignPad` |
| Night / combat | `Monster`, `Turret`, `FireCore`, `Bolt`, `Boar` |
| Economy | `CoinPad`, `CoinPickup`, `Stockpile`, `TraderStall`, `WorkerAgent` |
| Art / look | `FortArt`, `FortLook`, `FortGround`, `FortAnim` |
| Save | `FortSave` |
| Demo / QA / perf | `DemoDriver`, `ElementSheet`, `QaShot`, `PoseWatch`, `TreeCheck`, `PerfHarness`, `FrameMeter`, `TriReport` |
| Fallbacks | `BuiltinPrimitives` (procedural meshes when FBX missing) |

Staged build stage counts (live): wall 5, gate 5, tower 6, turret 5, spikes 3, campfire 3, hut 4, upgrades 4. Part naming: `{Build}_T{tier}_{Group}_{##}` (0.5 m snap).

---

## 4) Current state (as of 2026-10-29 Asia/Shanghai)

| Checkpoint | Status |
|---|---|
| Last **QA PASS** (gameplay) | **`6b92d10`** (PR #10): trees upright 47/47, pad labels clear, hero idle/walk/carry, boss bar clear. Perf then: **626 draws / 222k tris** (above budget; pre–Batch A art). |
| **Batch A** art integrate | **PR #15**, tip **`ca339fc`**: most checks PASS; **FAIL = Treant boss framing** (7.7 m boss fills ~70% of frame). Perf on that build: **~133 draws / ~40k tris** (under budget). |
| Treant fix | Spec decided: scale **~4.5 m (~2.5× hero)**, **`bossScale` + `bossCamPullback`** in FortIdleConfig, boss ≤**35%** of frame. **Not merged** — held (cloud credits / Matt OK for agent runs). Intermediate tip `cf4682b` had 4.5 m scale but camera/death still rejected. |
| Adaptive nights + economy | On main earlier (`b07dd77` lineage): Fort Power vs Night Threat, seed ~1401 clears N1–10; N6–7 too easy. |
| Endless / Rebuild | **Designed** in `PROGRESSION_ECONOMY.md`, **not built**. |
| Public GitHub push | Approved by Matt (code-only). May lag Origin; confirm SHA on clone. |

Evidence folders for agents (not in public git): per-commit under a private `qa/<sha>/` layout. Log: `docs/QA_LOG.md`.

---

## 5) Missing art (expect pink / placeholders)

Public mirror policy: **code only — no paid Synty packs, no Tripo outputs.** CC0 Kenney/Quaternius FBX under `Assets/FortIdle/Resources/Art/` were also stripped from the mirror export in progress (see exclude list). You will likely see:

- Missing meshes / **pink materials** / bind-pose or primitive stand-ins.
- Runtime fallback: **`BuiltinPrimitives`** + simple `URPLit` materials.
- Batch A deliverables exist privately (`art/batch_a/`, `build/*.tar.gz`) but **must not** be committed to the public repo.

Placeholders / paths (private art lane — **not** in public tree):

- Treant: `art/monsters/treant_golem/` → engine name pattern `Enemy_Treant_LOD*`
- Walls/gates/env kitbash: `art/batch_a/walls/`, `gates/`, `environment/`
- Atlas ref: `art/batch_a/textures/FortIdle_Atlas_512.png`

If something is pink: do not buy assets into the public repo. Either restore CC0-only meshes with licenses in `CREDITS.md`, or keep primitives until a private Origin sync.

---

## 6) Open work (priority order)

1. **Treant readability fix** — `bossScale` ≈ 4.5 m; `bossCamPullback` ~15–20% when near boss; boss ≤35% frame; death clip grounded; fade must not hide hero/horde. Re-QA with Play Mode shots.
2. **Finish Batch A sign-off** — wall/gate T3 frames, treant-die evidence, GC counter consistency.
3. **Batches B→H** per `docs/ART_MANIFEST.md` (defenses/hut/workers → allies → more monsters → tiers → maps 2–3 → soft-launch polish → P2).
4. **Monster tier + elite affix hooks** — T1/T2/T3 + affixes from `docs/MONSTER_ROSTER.md` wired to Night Threat / HPx (`PROGRESSION_ECONOMY.md` §2.4–2.5).
5. **Endless mode + Rebuild (Embers prestige)** — after art order Matt set.
6. **Map 2 opening nights** — N6–7 currently too easy (threat ratio ~0.3); retune.
7. **M3** — Android module + **AAB**, then TestFlight path.

Do **not** spend money, publish store builds, or add paid art without Matt's explicit OK.

---

## 7) Rules (non-negotiable)

**Mobile perf budgets (ship bar):** ≤**300** draw calls, ≤**150k** tris at night peak; **pooling** (`FortPool` / `ObjectPool`); **~0 B GC / frame** steady state; SRP Batcher + GPU instancing; prefer 1 shadow cascade, short shadow distance, no HDR, render scale ~0.85 when needed.

**Tuning:** all knobs in **`FortIdleConfig`** — no magic numbers scattered in new systems.

**Git hygiene:** never commit paid art, `.ulf` licenses, `.env`, API keys, passwords, or Unity account secrets. Scan before any public push.

**QA:** a build **passes only** on real Play Mode screenshots/recordings (see `docs/QA.md`). Clip choppiness on software GL is not a FAIL by itself.

**Trees:** Kenney meshes are often Z-up — need **−90° X** (or equivalent). Up-vector assertion: angle(transform.up, Vector3.up) < 5°.

**Brute ≠ Knight.** Knight is an ally silhouette; Brute is bark-ogre.

---

## 8) Docs in `docs/` (this pack)

| File | Covers |
|---|---|
| `GDD.md` | Pitch, loops, maps, economy, monetization, MVP plan |
| `MAP1_SPEC.md` | Map 1 numbers, pads, day/night, playable-ad beat sheet |
| `PROGRESSION_ECONOMY.md` | Endless, adaptive threat, multi-supply economy, Rebuild (design; mostly unbuilt) |
| `MONSTER_ROSTER.md` | 16 monsters + 4 bosses, T1–T3, elite affixes |
| `ART_MANIFEST.md` | Batches A–H shopping list + status |
| `ART_STYLE.md` | Palette, scale, tris, modular staged-build naming, sources |
| `QA.md` | Pass checklist |
| `QA_LOG.md` | Dated PASS/FAIL lines |
| `HANDOFF.md` / `PROGRESS.md` | Team timeline (scrubbed in public pack) |

Also useful if present in the Unity repo root: `CREDITS.md`, `REWARD_REPORT.md`, `UI_COMPONENTS.md` (UI design; may not be mirrored yet).

---

## Quick start checklist for Claude

1. Clone `holymilk09/Fort-idle`, open with **Unity 6000.3.26f1**.
2. Open `Map1`, confirm `FortIdleConfig` is assigned on bootstrap.
3. Enter Play Mode; optionally build a Linux player and run with `-demo`.
4. Expect missing art — use counters / logs, not beauty, for first triage.
5. Read `docs/QA_LOG.md` before proposing refactors; prefer fixing Treant framing + config knobs over architecture churn.
6. Keep PRs small, evidence-backed, and free of paid assets/secrets.

*Generated for public handoff 2026-10-29 (Asia/Shanghai). Private Origin remains source of truth until the mirror SHA matches.*
