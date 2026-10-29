# Fort Idle — Handoff
Lead: Game Chief. Source of truth: Origin https://cursor.com/codebase/matt-fb/FortIdle (PRIVATE).
Docs: GDD.md, MAP1_SPEC.md, STORYBOARDS.md. Status: PROGRESS.md.
- main = 6999f7f (hire ladder + nights 2–5 + element sheet). Launch new agents on https://cursor.com/codebase/matt-fb/FortIdle.
- Unity license: needs [REDACTED_SECRET_NAME] secret for [REDACTED_UNITY_ACCOUNT].
- Teardown Analyst: competitor numbers still est.; optional later.
- Ad Creative: waiting on better capture after nights/monsters.

## Update 2026-10-27
- Unity Personal license active on the cloud agent; main builds and runs -demo/-elements.
- main d75a952: defenses + free CC0 art pass. Next proposed: animations, hero melee, save/load.

- 2026-10-27: ON HOLD per Matt. Next milestone (animations, hero melee, save/load) not started. Do not start without his go.
- 2026-10-27 15:24: Gameplay-reward milestone in flight on bc-1cec9ef7 (see PROGRESS). Art: AI image-to-3D approved for later, after gameplay proven. Allies/weapons/enchantments sheet = future milestone.
- 2026-10-27 16:32 CST: Matt approved queuing adaptive-enemy + economy core milestone (Fort Power vs Night Threat, generated nights, threat-scaled coins, shared price curve, 2+ uses per supply, Trader, worker buff) on bc-1cec9ef7 after current milestone. Endless/Rebuild deferred.
- 2026-10-27 17:16 CST: Adaptive nights + economy core merged to main b07dd77 (PR #5; main fast-forwarded, PR may show open). Demo seed 1401 cleared N1-10 no retries; N5 ratio 1.05 close; N6-7 too easy (ratio ~0.3); worker ROI 4.43 by N9; no dead supplies; save roundtrip ok. Clip fort-idle-threat.mp4, sheet v5. Gaps: endless/Blood Moon/elites/Rebuild deferred, boss phases, greybox new items, dawn banner raw text. Agent idle.
- 2026-10-28 10:08 CST: Matt asked for staged build animations (cost split into ~5 stages, e.g. every 20 of 100, each stage matching that exact structure). Queued to bc-1cec9ef7 from main b07dd77. Endless mode/Rebuild not yet approved.
- 2026-10-28 10:38 CST: Staged construction done. main 190573d (ff from b07dd77), PR #6. Stages: wall 5, gate 5, tower 6, turret 5, spikes 3, campfire 3, hut 4, upgrades 4. Save keeps partial stage. Clip sent. Endless/Rebuild still awaiting Matt's go.
- 2026-10-28 17:00 CST: Matt flagged tilted trees, polygon ground, laggy clip. Queued to bc-1cec9ef7: tree upright fix, cheap free ground upgrade, frame-time measurement plus mobile perf pass (pooling, batching, instancing, mobile URP). Endless still on hold.
- 2026-10-28 17:21 CST: Trees upright, ground textured, mobile perf pass. main 33d6182 (ff from 190573d), PR #7. Clip choppiness = llvmpipe recording (~7–9 fps on VM), not phone. Night 5 after: avg 112 ms on soft GPU; draws 1305→797, tris 438k→273k. Android module not installed. Endless still on hold.
- 2026-10-28 17:50 CST: Matt says trees still lying down in night 5 clip (33d6182 fix not visible). Queued re-fix to bc-1cec9ef7 with up-vector assertion + low-camera screenshot verification, plus pad label overlap fix. Genie 4 assessed: research preview, no engine export; concept/ad previz only, not for the build.
- 2026-10-28 17:52 CST: Matt: stay on Unity (Unreal rejected for mobile idle). Free look pass (lighting/post/free packs) offered and HELD by Matt: gameplay first. Tree fix still running.

## Team restructure 2026-10-28 18:05 CST (Matt-approved via Edwin)
- Game Chief: roadmap, milestones, Matt's decisions only. Does NOT drive cloud agents.
- Unity Engineer (b79c6923): sole owner of repo matt-fb/FortIdle + cloud agents. Inherits bc-1cec9ef7 (tree up-vector re-fix + pad label overlap in flight; main 33d6182).
- Fort Idle 3D Art (1d71cf85): ART_STYLE.md, CREDITS.md, source Synty/Meshy/Kenney/Quaternius + Blender. NO in-game integration until Matt lifts the art hold.
- Fort Idle QA (810e6c15): gates every build on real Play Mode shots/recordings. Known: VM is llvmpipe (~7–9 fps capture), so judge perf by draws/tris/allocs, not clip smoothness.
- Ad Creative: parked until new art. Release bot ~2 weeks before soft launch.
- Milestones: M0 foundation, M1 genre look, M2 rewarding loop, M3 AAB + TestFlight, M4 soft launch (Play closed test, then App Store), M5 polish. Target 4–6 weeks.
- Matt holds: look pass/art integration (gameplay first); endless mode + Rebuild awaiting his go. Meshy + Synty purchases handled by Edwin. MacBook unity-mcp not connected yet, so cloud agents only.
- 2026-10-28 21:50 CST: Status to Matt. main build 6b92d10 (PR #10) QA PASS: trees upright 47/47, labels clear, hero idle/walk anim, boss bar clear. Perf 626 draws/222k tris (target <=300/<=150k, art hold). Art: ART_STYLE.md, Tripo Pro pipeline, Treant golem 1.95M->4k tris, not integrated (art hold). Synty rec ~$55 via Edwin.
- 2026-10-28 22:42 CST: Matt LIFTED ART HOLD. Order: finish art for everything (enemies with evolving tiers, Diablo-inspired roster; allies/units; walls/defenses; buildings; supplies), integrate with look pass, THEN endless mode + Rebuild. Executor writing ART_MANIFEST.md + MONSTER_ROSTER.md.
- 2026-10-28 22:45 CST: MONSTER_ROSTER.md (16 monsters + 4 bosses, T1-T3, elite affixes) and ART_MANIFEST.md (batches A-H) written. Unity Engineer: look pass + Treant import running; tier hooks next. 3D Art: rig Treant, then hero/Shambler/Brute, walls, env.
- 2026-10-28 22:53 CST: Matt: back up to GitHub holymilk09/Fort-idle (PUBLIC, his choice), CODE ONLY, no paid art. Sent to Unity Engineer with secret-scan, art/secret excludes, auto-mirror after QA-passed builds.
- 2026-10-29 05:57 CST: Batch A (PR #15, ca339fc) all fixes pass except boss framing. Decision: Treant scaled to ~4.5 m (2.5x hero) + 15-20% cam pullback near boss; boss <=35% of frame. Perf 133 draws/40k tris. GitHub mirror: Matt's OK (t49 22:52) relayed to Unity Engineer.
- 2026-10-29 08:26 CST: Matt approved public code-only GitHub push ("Approve"). Relayed to Unity Engineer.
- 2026-10-29 08:30 CST: Matt rule (via Edwin) — no new cloud agents or follow-up runs without Matt's explicit OK (usage 34→71%). Running PR #1 video agent + mirror export may finish. Treant fix on PR #15 HELD pending Matt OK.
- 2026-10-29 11:41 CST: Matt directly asked to back up game to GitHub holymilk09/Fort-idle (cloud credits out, wants Claude to review). Sent Unity Engineer: box-side push, no cloud agent, code-only + gitleaks.
