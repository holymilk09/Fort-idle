#!/usr/bin/env python3
"""Headless mirror of ThreatMath.BaseBudget / KindOf / Build (band + valve).

Defaults are copied by hand from Assets/FortIdle/Scripts/FortIdleConfig.cs
(ProgressionTuning, EndlessTuning, PrestigeTuning). The progression block is
not overridden in FortIdleConfig.asset, so the .cs defaults are live.

Power curve (there is no live game here):
  doc  = Power Score column of docs/PROGRESSION_ECONOMY.md section 4 (nights 1-30),
         then PS30 * (n/30)^2.6, which matches the doc's ~8.6%/night endless slope
         at night 30 and slows after it (power grows polynomially, threat exponentially).
  live = seed-1401 dusk Fort Power from REWARD_REPORT.md (nights 1-10 only).

Usage: python3 Tools/sim/threat_sim.py [--nights 40] [--old] [--embers N]
  --old     prints the pre-retune continuation (no carry floor, map-5 cycling).
  --embers  scales the doc power curve by the Ember DPS bonus (soft-capped).
"""
import argparse
import math

# ProgressionTuning
BUDGET = [720, 1200, 2400, 3600, 4500]
MAP_GROWTH = 1.6
BOSS_EVERY = 5
RUBBER_ALPHA, RUBBER_MIN, RUBBER_MAX, RUBBER_OFF_UNTIL = 0.25, 0.85, 1.2, 3
LOSS_VALVE = 0.8
TARGET_NORMAL, TARGET_BOSS = 0.75, 0.95

# EndlessTuning
AUTHORED_NIGHTS = 25
ENDLESS_GROWTH = 1.10
BLOOD_EVERY = 10
BLOOD_MULT, BLOOD_LOOT = 1.5, 2.0
REST_MULT = 0.7
TARGET_BLOOD, TARGET_REST = 1.0, 0.55
CARRY_FLOOR, CARRY_STEP = 0.6, 1.08
DECAY_START, DECAY_SPAN = 41, 10
DECAY = [0.5, 0.25, 0.1]

# PrestigeTuning
EMBER_K, EMBER_DIV = 10.0, 10000.0
EMBER_DPS, EMBER_SOFT_CAP = 0.03, 3.0

DOC_PS = [3529, 4960, 6204, 6965, 8304, 5545, 6701, 7914, 9478, 10655,
          8833, 9972, 11698, 12864, 14056, 12847, 14123, 15863, 16757, 18856,
          18379, 20837, 22352, 24797, 29079, 30867, 32611, 36870, 39336, 42887]
LIVE_PS = [2273, 2879, 3915, 4344, 4124, 4827, 7682, 7682, 8052, 10100]


def doc_power(n):
    if n <= len(DOC_PS):
        return float(DOC_PS[n - 1])
    return DOC_PS[-1] * (n / 30.0) ** 2.6


def ember_mult(claimed):
    raw = claimed * EMBER_DPS
    if EMBER_SOFT_CAP > 0:
        raw = raw / (1.0 + raw / EMBER_SOFT_CAP)
    return 1.0 + raw


def kind_of(n):
    if n > AUTHORED_NIGHTS and BLOOD_EVERY > 0:
        past = n - AUTHORED_NIGHTS
        if past % BLOOD_EVERY == 0:
            return "blood"
        if past > 1 and (past - 1) % BLOOD_EVERY == 0:
            return "rest"
    return "boss" if n % BOSS_EVERY == 0 else "normal"


def target(kind):
    return {"normal": TARGET_NORMAL, "boss": TARGET_BOSS, "blood": TARGET_BLOOD, "rest": TARGET_REST}[kind]


def base_budget(n, old=False):
    index = max(0, (n - 1) % 5)
    if not old and n > AUTHORED_NIGHTS:
        maps = max(1, AUTHORED_NIGHTS // 5)
        top = BUDGET[4] * MAP_GROWTH ** (maps - 1)
        b = top * ENDLESS_GROWTH ** (n - AUTHORED_NIGHTS)
        k = kind_of(n)
        if k == "blood":
            b *= BLOOD_MULT
        elif k == "rest":
            b *= REST_MULT
        return b
    m = min(5, (n - 1) // 5 + 1)
    b = BUDGET[index] * MAP_GROWTH ** (m - 1)
    if not old and m > 1 and CARRY_FLOOR > 0:
        prev_boss = BUDGET[4] * MAP_GROWTH ** (m - 2)
        b = max(b, CARRY_FLOOR * prev_boss * CARRY_STEP ** index)
    return b


def plan(n, power, old=False, streak=0):
    if old:
        kind = "boss" if n % BOSS_EVERY == 0 else "normal"
    else:
        kind = kind_of(n)
    b = base_budget(n, old)
    expected = b / target(kind)
    band = 1.0
    if n > RUBBER_OFF_UNTIL and expected > 1:
        band = min(RUBBER_MAX, max(RUBBER_MIN, (power / expected) ** RUBBER_ALPHA))
    valve = LOSS_VALVE if streak >= 2 else 1.0
    threat = b * band * valve
    return kind, b, band, threat, threat / power


def coin_scale(n):
    s = 1.0
    if n >= DECAY_START:
        step = (n - DECAY_START) // DECAY_SPAN
        s = DECAY[min(step, len(DECAY) - 1)]
    if kind_of(n) == "blood":
        s *= BLOOD_LOOT
    return s


def embers(lifetime):
    return int(math.floor(EMBER_K * math.sqrt(lifetime / EMBER_DIV)))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--nights", type=int, default=40)
    ap.add_argument("--old", action="store_true")
    ap.add_argument("--embers", type=int, default=0)
    args = ap.parse_args()
    boost = ember_mult(args.embers)
    print("| Night | Kind | Base | PS(doc) | Band | Threat | Ratio(doc) | PS(live) | Ratio(live) | Coin x | Embers |")
    print("|---|---|---|---|---|---|---|---|---|---|---|")
    lifetime = 0.0
    stall = 0
    for n in range(1, args.nights + 1):
        ps = doc_power(n) * boost
        kind, b, band, threat, ratio = plan(n, ps, args.old)
        live = ""
        live_ratio = ""
        if n <= len(LIVE_PS) and args.embers == 0:
            _, _, _, _, lr = plan(n, LIVE_PS[n - 1], args.old)
            live = str(LIVE_PS[n - 1])
            live_ratio = "%.2f" % lr
        if stall == 0 and ratio > 1.2:
            stall = n
        if stall == 0:
            lifetime += threat
        print("| %d | %s | %d | %d | %.2f | %d | %.2f | %s | %s | %.2f | %d |" % (
            n, kind, round(b), round(ps), band, round(threat), ratio, live, live_ratio, coin_scale(n), embers(lifetime)))
    print()
    if stall:
        print("Run stalls at night %d (ratio > 1.2). Rebuild there: %d Embers (lifetime threat %d)." % (stall, embers(lifetime), round(lifetime)))
    else:
        print("No stall in range. Embers_total: %d (lifetime threat %d)." % (embers(lifetime), round(lifetime)))


if __name__ == "__main__":
    main()
