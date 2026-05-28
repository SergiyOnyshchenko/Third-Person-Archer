# Meta Balance After Boss Gate (Phase 4)

**Date:** 2026-05-27
**Status:** Implemented
**Based on:** Phase 2/3 (`Meta_Balance_After_Fix.md`) + Phase 4 (`Meta_Boss_Crossbow_Gate.md`)

---

## 1. What Is New in Phase 4

Phase 4 adds a **Boss Crossbow Gate** on top of the existing Phase 2/3 balance. No existing gate formula, weapon stat, or reward value was changed. The Boss gate is purely additive.

---

## 2. Full Gate Summary (After Phase 4)

### Campaign Gates (per global mission index g)

| g  | Zone | Class     | Gate  | Forced Purchase? |
|----|------|-----------|-------|-----------------|
| 0  | 1    | Bow       | ~0    | — (ungated)     |
| 1  | 1    | Shuriken  | 12    | —               |
| 2  | 1    | Bow       | 29    | —               |
| 3  | 1    | Shuriken  | 16    | —               |
| 4  | 1    | Bow       | 33    | —               |
| 5  | 1    | Shuriken  | 20    | —               |
| **6** | **1** | **Bow** | **37** | — (teaching gate; Bow1 L1) |
| 7  | 2    | Spear     | 55    | — (Spear1 base=65 passes) |
| **8** | **2** | **Shuriken** | **26** | — (Shuriken1 L1) |
| **9** | **2** | **Bow** | **43** | — (Bow1 L4) |
| **10** | **2** | **Spear** | **70** | — (Spear1 L1) |
| **11** | **2** | **Shuriken** | **32** | — (Shuriken1 L7) |
| **12** | **2** | **Bow** | **49** | — (Bow1 L7) |
| **13** | **2** | **Spear** | **85** | — (Spear1 L4) |
| **14** | **2** | **Shuriken** | **38** | **Shuriken2 purchase** |
| 15 | 3    | Boomerang | 52.5  | — (Boom1 base=55 passes) |
| **16** | **3** | **Bow** | **57** | **Bow2 purchase** |
| **17** | **3** | **Shuriken** | **44** | — (Shuriken2 needed) |
| **18** | **3** | **Spear** | **110** | — (Spear1 max=110) |
| **19** | **3** | **Boomerang** | **62.5** | — (Boom1 L3=64) |
| **20** | **3** | **Bow** | **65** | — (Bow2 needed) |
| **21** | **3** | **Shuriken** | **52** | **Shuriken3 purchase** |
| **22** | **3** | **Spear** | **130** | **Spear2 purchase** |

### Sniper Access Gate (unchanged from Phase 2/3)

Formula: `15 + 3 × sniperCompletedIndex`

| Sniper Index | Required Xbow Dmg | Xbow1 Level Needed |
|-------------|------------------|---------------------|
| 0–2         | 15 / 18 / 21     | L0 (passes at base=22) |
| 3           | 24               | L2                  |
| 5           | 30               | L6                  |
| 7           | 36               | L10                 |
| 8           | 39               | **Xbow2 needed**    |

### Boss Crossbow Gate (new in Phase 4)

Formula: `28 + 9 × bossZoneIndex`

| Boss | ZoneIndex | Loop 0 | Loop 1 (×1.25) | Loop 2 (×1.55) | What Passes |
|------|-----------|--------|----------------|----------------|-------------|
| Zone 1 Boss | 0 | **28** | 35 | 43.4 | Xbow1 L4 / Loop 1: Xbow1 L9 / Loop 2: Xbow2 L4 |
| Zone 2 Boss | 1 | **37** | 46.25 | 57.35 | Xbow1 max / Loop 1: Xbow2 L6 / Loop 2: Xbow3 L1 |
| Zone 3 | 2 | (no boss) | — | — | N/A |

---

## 3. Reward Tables (unchanged from Phase 2/3)

| Mode      | Cash           | Token                   |
|-----------|----------------|-------------------------|
| Campaign  | 70 + 7N        | 1 flat (class-specific) |
| Contracts | 40 + 15N       | 3 + 1N (all classes)    |
| Sniper    | 150 + 50N      | 2 + 1N (Crossbow)       |
| Boss      | 300 + 100N     | 3 + 2N (all classes)    |

---

## 4. Weapon Stats (unchanged from Phase 2/3)

| Weapon     | Base | Max |
|------------|------|-----|
| Bow1       | 35   | 55  |
| Spear1     | 65   | 110 |
| Shuriken1  | 25   | 35  |
| Boom1      | 55   | 80  |
| Xbow1      | 22   | 37  |
| Xbow2      | 37   | 55  |
| Xbow3      | 55   | 80  |

---

## 5. Starting Balances (unchanged from Phase 2/3)

| Currency       | Starting Amount |
|----------------|-----------------|
| Cash           | 100             |
| BowToken       | 1               |
| CrossbowToken  | 1               |
| SpearToken     | 1               |
| ShurikenToken  | 1               |
| BoomerangToken | 1               |

---

## 6. Complete Player Progression Flow (After Phase 4)

```
New Player
│
├── Zone 1 Campaign (g0–g6, 7 missions)
│   ├── g0: Free pass
│   ├── g6: TEACHING GATE — Bow L1 needed (50 cash, 1 BowTok)
│   └── Earns ~637 cash, 4 BowTok, 3 ShurikenTok
│
├── Zone 1 Sniper (optional, now motivated by Boss gate)
│   ├── Run 0–2: Free (Xbow1 base=22 passes)
│   └── Run 3+: Buy Xbow1 upgrades from CrossbowToken earned
│
├── Zone 1 Boss (unlocks after Campaign complete)
│   ├── CHECK: Crossbow damage ≥ 28?
│   ├── Xbow1 L0 (22) → BLOCKED → popup → go play Sniper
│   ├── After ~3 Sniper runs + L4 upgrade → dmg=28
│   └── Zone 1 Boss: PLAYABLE ✓ → earns 300 cash + 3 all-class tokens
│
├── Zone 2 Campaign (g7–g14, 8 missions)
│   ├── g7: Spear free pass
│   ├── g8–g13: 6 blocks across Shuriken/Bow/Spear
│   ├── g14: FORCED Shuriken2 purchase (650 cash, 15 ShurikenTok)
│   └── Earns ~1148 cash total in Zone 2
│
├── Zone 2 Sniper (runs 3–5, motivated by Zone 2 Boss)
│   └── Earns ~5+6+7=18 more CrossbowTokens; funds Xbow1 max
│
├── Zone 2 Boss (unlocks after Zone 2 Campaign complete)
│   ├── CHECK: Crossbow damage ≥ 37?
│   ├── Xbow1 L6 (31) → BLOCKED → popup → go play more Sniper
│   ├── After ~2 more Snipers + upgrades → Xbow1 L10 (37)
│   └── Zone 2 Boss: PLAYABLE ✓ → earns 400 cash + 5 all-class tokens
│
├── Zone 3 Campaign (g15–g22, 8 missions)
│   ├── g15: Boomerang free pass
│   ├── g16: FORCED Bow2 purchase (600 cash, 14 BowTok)
│   ├── g21: FORCED Shuriken3 purchase
│   ├── g22: FORCED Spear2 purchase (1000 cash, 25 SpearTok)
│   └── Earns ~1596 cash total in Zone 3
│
└── Zone 3 has no Boss → loop back or end
```

---

## 7. Loop Scaling Summary

| Loop | GateDamage Mult | Zone 1 Boss Gate | Zone 2 Boss Gate | Campaign Gate (g22) |
|------|-----------------|------------------|------------------|---------------------|
| 0    | 1.0             | 28               | 37               | Spear: 130          |
| 1    | 1.25            | 35               | 46.25            | Spear: 162.5        |
| 2    | 1.55            | 43.4             | 57.35            | Spear: 201.5        |

---

## 8. Gate System Code Map

| Gate Type | Check Method | Block Reason | Popup Trigger |
|-----------|-------------|--------------|---------------|
| Campaign | `CheckCampaignGate` | `CampaignDamageTooLow` | `ShowDamageGatePopup` |
| Sniper | `CheckSniperGate` | `SniperDamageTooLow` | `ShowSniperGatePopup` |
| Boss | `CheckBossGate` | `BossCrossbowDamageTooLow` | `ShowBossGatePopup` |

All three use `DamageGatePopup` screen with `DamageGatePopupArgs`. Boss and Sniper gates use `customTitle` and `customHint` overrides. All open `WeaponSelectionScreen` with the required weapon class preselected.

---

## 9. Remaining Tuning Risks

| Risk | Severity |
|------|----------|
| Zone 1 Boss gate (28) may feel trivial if player coincidentally upgraded Crossbow | LOW |
| Zone 2 Boss gate (37) = Xbow1 max: some players will not notice they're at max and assume they need to buy Xbow2 | LOW |
| No telemetry yet on Sniper–Boss pacing | MEDIUM |
| Loop 2 Boss Zone 2 gate (57.35) forces Xbow3 purchase (cost: 700 cash / 17 tokens) | LOW |
| Hint text in `ShowBossGatePopup` is hardcoded English — needs localization review before ship | LOW |
