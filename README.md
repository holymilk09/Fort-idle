# Fort Idle (code-only snapshot)

Code-only mirror of private Origin `matt-fb/FortIdle` @ `60ac4a3db087497a192eb6ce532a4fd220ae4366`.  
Engine: **Unity 6000.3.26f1** / URP. Soft-launch target: iOS + Android.

**Start here:** [`CLAUDE.md`](CLAUDE.md) (on-ramp). Design pack under [`docs/`](docs/).

## Open without paid art

Paid Synty / Tripo meshes and most `Resources/Art` FBX/PNG were **omitted**. Open Map1 anyway — gameplay uses **`BuiltinPrimitives`** when meshes are missing (pink mats / cubes / capsules expected). Assign nothing; `FortIdleBootstrap` on the `FortIdle` GO already references `FortIdleConfig.asset`.

```
# Linux player self-play / QA (after a player build)
./FortIdle.x86_64 -demo          # DemoDriver + seed 1401
./FortIdle.x86_64 -perf          # PerfHarness + FrameMeter
./FortIdle.x86_64 -elements      # ElementSheet catalog (no MatchDirector)
./FortIdle.x86_64 -stages        # StageShowcase
```

## Verified notes

Checked against export sources on 2026-10-29 (Asia/Shanghai):

- **`-perf`**: `PerfHarness.Requested()` exact-matches argv `-perf` (CLAUDE previously said “not verified” — corrected).
- **`-demo` / `-elements` / `-stages`**: same `GetCommandLineArgs()` pattern in `DemoDriver` / `ElementSheet` / `StageShowcase`. Bootstrap also seeds `Random.InitState(1401)` for `-demo`/`-stages`/`-perf`.
- **Map1 boot**: sole scene in `EditorBuildSettings`; GO `FortIdle` → `FortIdleBootstrap` with `config` = `FortIdleConfig.asset` (guid `01090d4eed4ea307988b4fa5d14c772d`), `URPLit` + `URPLitEmission`.
- **Not verified here:** live Play Mode / store builds; Mac/Windows packaging beyond shared argv parsing; cloud GPU fps (llvmpipe notes still apply).

## What’s excluded

See `../mirror_exclude.txt` (or the exclude list shipped with this zip). No Unity license files, no `.env`, no paid art packs.

Public GitHub (when pushed): https://github.com/holymilk09/Fort-idle — may lag this snapshot SHA.
