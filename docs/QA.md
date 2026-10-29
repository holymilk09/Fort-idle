# Fort Idle QA Checklist
Owner: Fort Idle QA. A build passes only on real Play Mode screenshots or recordings from Unity Engineer. Log results in qa/QA_LOG.md and send one line per build to Game Chief.

## 1. Visuals
- [ ] Trees/trunks vertical (low-camera shot; up-vector assertion green: angle(transform.up, Vector3.up) < 5°)
- [ ] No wrong-axis or tilted props; no characters stuck in bind pose
- [ ] Pad labels (WALL/GATE etc.) don't overlap each other or models
- [ ] HUD readable at phone res (coins, stack, night timer, dawn banner has no raw text)
- [ ] No z-fighting, missing materials or pink shaders

## 2. Night 1–10 demo pass
- [ ] Demo seed clears N1–10 with no stalls or retries; record the seed
- [ ] Difficulty curve sane: Night Threat to Fort Power ratio logged per night (flag <0.5 too easy, >1.2 wall)
- [ ] Core feel: stack speed, pad fill, staged builds visible
- [ ] Session hook: a ~10 min run ends wanting "one more night"

## 3. Save / load
- [ ] Mid-night quit and reload restores coins, buildings, partial build stage, night index
- [ ] Corrupt or missing save falls back cleanly (soft fail, no crash)

## 4. Perf counters (VM is llvmpipe at ~7–9 fps, so judge counters, not clip smoothness)
Capture at night 5 peak and night 10 peak:
- [ ] Draw calls (target ≤ 300 mobile; last known 797)
- [ ] Tris (target ≤ 150k; last known 273k)
- [ ] GC allocs per frame (target ~0 B steady state)
- [ ] Frame time p95 (report the soft-GPU ms; trend versus the previous build)

## 5. Fail cases
- [ ] Hero death / fort breach handled; no softlock at 0 coins

## Store checklist
Deferred to M3 (AAB + TestFlight).
