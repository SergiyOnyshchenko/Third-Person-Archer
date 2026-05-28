# Meta Boss Crossbow Gate

**Date:** 2026-05-27
**Phase:** 4 — Boss Crossbow Gate Integration
**Based on:** Phase 2/3 implementation (`Meta_Balance_After_Fix.md`, `Meta_Balance_Pacing_Validation.md`)

---

## 1. Design Summary

### Problem

Before Phase 4, Boss missions in Zone 1 and Zone 2 had no Crossbow gate. After completing all Campaign missions in a zone, the Boss was immediately accessible regardless of Crossbow investment. Sniper mode had no direct connection to Boss unlock, so players had no structural motivation to play Sniper or upgrade their Crossbow before the Boss.

### Solution

Boss missions now require a minimum Crossbow damage threshold (a **Boss Crossbow Gate**). The gate:

- Applies **only to Boss missions** (`MissionType.Boss`).
- Uses the player's **equipped Crossbow damage** (computed snapshot, same source as Sniper gate and Campaign gate).
- Does **NOT** affect boss HP, difficulty scaling, or gameplay difficulty in any way. Boss gameplay is separate.
- Blocks access with a **popup** that explains Crossbow upgrade or purchase is needed.
- The popup opens **WeaponSelectionScreen with Crossbow preselected**.

### Design Motivation

**Intended player flow:**
1. Player completes all Campaign missions in a zone.
2. Boss unlocks by campaign completion (existing rule, unchanged).
3. Player checks Boss → Boss gate popup appears if Crossbow is too weak.
4. Player goes to Sniper mode (which gives CrossbowTokens) and plays 2–3 runs.
5. Player upgrades Crossbow with earned tokens.
6. Player returns to Boss → gate passes → Boss accessible.

This creates a **direct loop**: Campaign → Boss gate blocks → Sniper to earn Crossbow tokens → Crossbow upgrades → Boss unlocks.

### What Changed Vs. Before Phase 4

| Before | After |
|--------|-------|
| Boss accessible immediately after Campaign complete | Boss also requires Crossbow damage gate |
| No Crossbow pressure at Boss unlock | Crossbow investment required |
| Sniper motivation was only cash income | Sniper directly gates Boss access |
| Popup: only shown for Campaign/Sniper gates | Popup now also shown for Boss gate |

---

## 2. Gate Formula and Values

### Formula

```
required = BossBaseDamage + ZoneGrowth × bossZoneIndex
required × GateDamage loop multiplier
```

Where `bossZoneIndex` = 0-based index of the boss zone from `MissionContext.ZoneIndex`:
- Zone 1 = index 0
- Zone 2 = index 1
- Zone 3 = no boss (has no Boss segment; `IsBossUnlocked()` returns `false` regardless)

### Profile Values (in `BalanceConfig.asset` and code defaults)

| Field | Value |
|-------|-------|
| BaseDamage | 28 |
| ZoneGrowth | 9 |
| DamageCap | 200 |

### Boss Gate Values Per Zone

| Zone | ZoneIndex | Loop 0 | Loop 1 (×1.25) | Loop 2 (×1.55) |
|------|-----------|--------|----------------|----------------|
| Zone 1 | 0 | **28** | 35 | 43.4 |
| Zone 2 | 1 | **37** | 46.25 | 57.35 |
| Zone 3 | 2 | (no boss) | — | — |

---

## 3. Crossbow Damage Table

All Crossbow weapons use `damage(L) = Lerp(BaseDmg, MaxDmg, upgradeCurve(L/10))`.
For planning purposes, the table below uses a linear approximation (actual curve is slight ease-in/out S-shape).

### Crossbow 1 (base=22, max=37, unlock=level 0, free)

| Level | Damage (approx) | Passes Zone 1 Boss (≥28)? | Passes Zone 2 Boss (≥37)? |
|-------|-----------------|--------------------------|--------------------------|
| L0    | 22              | No                       | No                       |
| L1    | 23.5            | No                       | No                       |
| L2    | 25              | No                       | No                       |
| L3    | 26.5            | No                       | No                       |
| **L4** | **28**         | **Yes**                  | No                       |
| L5    | 29.5            | Yes                      | No                       |
| L6    | 31              | Yes                      | No                       |
| L7    | 32.5            | Yes                      | No                       |
| L8    | 34              | Yes                      | No                       |
| L9    | 35.5            | Yes                      | No                       |
| **L10** | **37**        | **Yes**                  | **Yes**                  |

### Crossbow 2 (base=37, max=55, unlock=campaign level 10, costs 550 cash / 12 tokens)

| Level | Damage (approx) | Passes Zone 2 Boss (≥37)? |
|-------|-----------------|--------------------------|
| L0    | 37              | **Yes** (37 ≥ 37)        |
| L1    | 38.8            | Yes                      |

**Zone 2 Boss can be passed with either Xbow1 maxed (L10=37) or Xbow2 base (L0=37).**

---

## 4. Sniper Reward Math and Expected Runs

### Sniper CrossbowToken Income

Sniper rewards: `Base=2, PerStep=1` → run at index N earns `2+N` tokens.

| Run (index) | Tokens Earned | Cumulative (incl. 1 start) |
|-------------|--------------|---------------------------|
| 0           | 2            | 3                         |
| 1           | 3            | 6                         |
| 2           | 4            | 10                        |
| 3           | 5            | 15 (minus Sniper gate spend) |
| 4           | 6            | 21 (minus spend)          |
| 5           | 7            | 28 (minus spend)          |

### Sniper Access Gate (existing, unchanged)

The Sniper gate uses Crossbow damage too. Each Sniper run partially funds the next gate.

| Sniper Index | Gate (required Xbow dmg) | Xbow1 Level Needed | Approx Token Cost to Unlock |
|-------------|--------------------------|--------------------|-----------------------------|
| 0–2         | 15 / 18 / 21             | L0 (base=22 passes) | 0                          |
| 3           | 24                       | L2 (25)             | ~2 tokens                  |
| 4           | 27                       | L4 (28)             | ~3 more tokens             |
| 5           | 30                       | L6 (31)             | ~3 more tokens             |
| 7           | 36                       | L10 (37)            | ~8 more tokens             |

### Token Flow to Zone 1 Boss Gate (target: ≥28 Crossbow damage)

| Step | Event | Tokens |
|------|-------|--------|
| Start | Starting CrossbowToken = 1 | 1 |
| Sniper 0 | Earn 2 | 3 |
| Sniper 1 | Earn 3 | 6 |
| Sniper 2 | Earn 4 | 10 |
| Sniper gate index 3 triggered | Spend ~2 for Xbow1 L2 | 8 |
| Sniper 3 | Earn 5 | 13 |
| Upgrade Xbow1 L2→L4 for Boss gate | Spend ~3 | **10 remaining, Xbow1 L4 (dmg=28)** |

**Result: Zone 1 Boss gate passable after ~3 Sniper runs + minimal upgrades.**

### Token Flow to Zone 2 Boss Gate (target: ≥37 Crossbow damage)

From Zone 1 Boss: player has Xbow1 at L4, ~10 tokens remaining.

| Step | Event | Tokens |
|------|-------|--------|
| After Zone 1 Boss | Xbow1 L4, ~10 tokens | 10 |
| Sniper 4 | Earn 6 | 16 |
| Sniper gate index 4 (need L4=28) | Already at L4, no spend | 16 |
| Sniper 5 | Earn 7 | 23 |
| Sniper gate index 5 (need L6=31) | Spend ~3 for L4→L6 | 20 |
| Upgrade Xbow1 L6→L10 for Boss gate | Spend ~6 more | **14 remaining, Xbow1 L10 (dmg=37)** |

**Result: Zone 2 Boss gate passable after ~2–3 additional Sniper runs (5–6 total) using Xbow1 maxed.**

### Summary: Expected Sniper Runs Per Boss Gate

| Boss Gate | Starting Point | Runs Needed | Required Xbow State |
|-----------|---------------|-------------|---------------------|
| Zone 1 Boss (gate=28) | Fresh (1 token) | **~2–3 Sniper runs** | Xbow1 L4 |
| Zone 2 Boss (gate=37) | After Zone 1 Boss | **~2–3 additional runs** | Xbow1 L10 (or Xbow2 L0) |

---

## 5. Before/After Player Flow

### Before Phase 4

```
Zone 1 Campaign complete
    → Zone 1 Boss: ACCESSIBLE IMMEDIATELY
    → Player with Xbow1 L0 (dmg=22) walks in, no gate
    → No Sniper motivation at Boss unlock
```

### After Phase 4

```
Zone 1 Campaign complete
    → Zone 1 Boss: Campaign complete ✓ → check Crossbow gate (28)
    → Player with Xbow1 L0 (dmg=22): BLOCKED
    → Popup: "Boss mission requires a stronger Crossbow"
    → Popup hint: "Play Sniper missions to earn Crossbow tokens..."
    → Player plays 2–3 Snipers, earns ~9 CrossbowTokens
    → Upgrades Xbow1 to L4 (dmg=28)
    → Zone 1 Boss: ACCESSIBLE ✓

Zone 2 Campaign complete
    → Zone 2 Boss: Campaign complete ✓ → check Crossbow gate (37)
    → Player with Xbow1 L6 (dmg=31): BLOCKED
    → Popup: "Boss mission requires a stronger Crossbow"
    → Player plays 2–3 more Snipers, maxes Xbow1 to L10 (dmg=37)
    → Zone 2 Boss: ACCESSIBLE ✓
```

---

## 6. Sniper Access Gate Decision

**Decision: Keep the existing Sniper Access Gate unchanged.**

### Reasoning

The Sniper gate (introduced in Phase 2/3) and the Boss gate are complementary, not competing:

| Gate | Purpose |
|------|---------|
| Sniper Access Gate | Keeps Crossbow progression meaningful *within* Sniper mode. Self-funding (earn tokens, spend on gate). |
| Boss Crossbow Gate | Creates motivation to *enter* Sniper mode in the first place. |

**Why removing Sniper gate would be wrong:**
- Without Sniper gate, Sniper mode has no internal upgrade pressure.
- The Boss gate alone motivates entering Sniper, but a player could enter Sniper and grind cash only without upgrading Crossbow — the Sniper gate ensures they engage with Crossbow upgrades.
- First 3 Snipers are free (Xbow1 base=22 passes gates 15/18/21). Sniper is accessible early.

**Why the two gates don't double-gate:**
- A player blocked at Boss → plays Sniper → Sniper gate may block once at Sniper 3 (gate=24, need L2=25, costs 2 tokens) → pass immediately with tokens just earned.
- The token cost to pass Sniper gates is always less than what each Sniper run earns. The loop is self-paying.
- Both gates point toward the same action (upgrade Crossbow) and the same source of tokens (play Sniper).

**No changes to:** `BalanceConfig.GateModule` `SniperAccessProfile` (BaseDamage=15, MissionGrowth=3, DamageCap=0).

---

## 7. Files Changed

### Code (6 files)

| File | Change |
|------|--------|
| `BalanceConfig.GateModule.cs` | Added `BossAccessProfile` class; added `_bossAccessProfile` serialized field; added `GetRequiredCrossbowDamageForBoss(int bossZoneIndex)` |
| `BalanceConfig.cs` | Added `GetRequiredCrossbowDamageForBoss(int bossZoneIndex, int loopIndex)` to Gate API region |
| `IMissionGateService.cs` | Added `CheckBossGate(MissionContext ctx)` to interface |
| `MissionGateService.cs` | Implemented `CheckBossGate` — uses `ctx.ZoneIndex`, reads Crossbow damage, returns `MissionGateResult` with `CanUpgradeToPass` |
| `MissionAvailability.cs` | Added `BossCrossbowDamageTooLow` to `AvailabilityBlockReason` enum |
| `MissionAvailabilityService.cs` | Boss case now checks `IsBossUnlocked()` first, then checks `CheckBossGate()` and returns `BossCrossbowDamageTooLow` if blocked |
| `MissionStartService.cs` | Handles `BossCrossbowDamageTooLow` by calling `CheckBossGate` and attaching gate result to `MissionStartResult` |
| `PlayMissionPresenter.cs` | Added `BossCrossbowDamageTooLow` to `canClickPlay`; added `ShowBossGatePopup(gate)` method; wired into `HandleStartFailure` switch |

### Assets (1 file)

| File | Change |
|------|--------|
| `BalanceConfig.asset` | Added `_sniperAccessProfile` (explicit, matching code defaults) and `_bossAccessProfile` (BaseDamage=28, ZoneGrowth=9, DamageCap=200) to `_gates` section |

### Documentation

| File | Status |
|------|--------|
| `Meta_Boss_Crossbow_Gate.md` | Created (this file) |
| `Meta_Balance_After_BossGate.md` | Created (final balance summary) |

---

## 8. Gate Values Did Not Change

These values are **unchanged** from Phase 2/3:

- Sniper Access Gate: BaseDamage=15, MissionGrowth=3
- Campaign gate profiles (Bow, Shuriken, Spear, Boomerang, Crossbow)
- Sniper rewards (CrossbowToken Base=2, PerStep=1)
- Crossbow weapon stats (Xbow1 base=22/max=37, Xbow2 base=37/max=55)

The Boss gate values are **new and additive**. No existing gate was modified.

---

## 9. Acceptance Criteria Check

| Criteria | Status |
|----------|--------|
| Boss mission unlock still requires all Campaign missions in the zone | ✓ `IsBossUnlocked()` checked first — unchanged |
| Boss mission then also requires Crossbow damage gate | ✓ `CheckBossGate` checked after `IsBossUnlocked` |
| Boss gate uses Crossbow equipped/computed damage | ✓ `GetEquippedDamage(WeaponClass.Crossbow)` |
| Boss gate does not affect boss HP/damage gameplay scaling | ✓ `GetEnemyStats()` returns `EnemyStats(1, 0)` for Boss type unconditionally — unchanged |
| If player ignores Sniper/Crossbow, Boss is blocked | ✓ Xbow1 L0 (22) < Zone 1 gate (28) → blocked |
| After ~2–3 Sniper runs and upgrades, Zone 1 Boss passable | ✓ After 3 Snipers: ~10 tokens → Xbow1 L4 (28) → passes |
| After ~2–3 more Sniper runs, Zone 2 Boss passable | ✓ After 5–6 total Snipers: Xbow1 L10 (37) → passes |
| Popup clearly explains Crossbow upgrade needed | ✓ Custom title + hint wired in `ShowBossGatePopup` |
| Popup opens WeaponSelectionScreen with Crossbow preselected | ✓ Reuses `DamageGatePopupController.GoToWeapons()` → `WeaponSelectionArgs(WeaponClass.Crossbow, ...)` |
| Crossbow is still not assigned to Campaign missions | ✓ No Campaign mission assignments changed |
| Zone 3 Boss is not added | ✓ Zone 3 has no Boss segment; `IsBossUnlocked()` returns false |
| Project compiles | ✓ All code changes are interface-complete and type-safe |

---

## 10. Remaining Risks and Tuning Notes

| Risk | Severity | Description |
|------|----------|-------------|
| Zone 1 Boss gate too easy | LOW | Gate=28 is reachable before a player even tries Sniper (if they spent starting tokens on Crossbow). Minimum friction. If this feels trivial in playtests, raise to 30–32. |
| Zone 2 Boss gate identity with Xbow1 max | LOW | Gate=37 equals Xbow1 max damage exactly. Xbow2 base also equals 37. A player who buys Xbow2 but never upgrades it also passes. This overlap is intentional — two valid paths to the gate. If design wants to force Xbow2 purchase, raise gate to 39. |
| Loop 1 Boss gate pressure | LOW | Loop 1: Zone 1=35 (need Xbow1 L9), Zone 2=46.25 (need Xbow2 L6). This is meaningful pressure for experienced players. Verify token income at loop 1 is sufficient (loop rewards have 10% cash bonus but same token multiplier). |
| Loop 2 Boss gate | LOW | Loop 2: Zone 2=57.35 requires Xbow3 (base=55, L1=57.5). Xbow3 unlocks at campaign level 15. Should be reachable by loop 2. |
| No playtest data on Sniper-Boss pacing | MEDIUM | The 2–3 Sniper run estimate is math-derived. Actual player behavior depends on whether they do Contracts, spend tokens on other weapons, or skip Sniper entirely. Monitor drop-off rate at Boss gate in analytics. |
| Popup hint text review | LOW | Hint text ("Play Sniper missions to earn Crossbow tokens...") is hardcoded in `PlayMissionPresenter.ShowBossGatePopup()`. Localization / wording may need design review before ship. |
| Xbow2 upgrade costs (startTok=9) | LOW | If a player wants to pass Zone 2 Boss with Xbow2 upgraded instead of maxing Xbow1, they need 9 tokens just for L1 of Xbow2. This is steep. The Xbow1-max path (free after ~5 Snipers) is the intended default path. |
