# Meta Balance After Fix

**Date:** 2026-05-27
**Status:** Implemented
**Based on:** `Meta_Balance_Fix_Plan.md`

---

## 1. Before/After Summary

### Gate Profiles

| Class      | Base (before→after) | Growth (before→after) | Cap (before→after) |
|------------|--------------------|-----------------------|--------------------|
| Bow        | 10 → **25**        | 2.5 → **2**           | 92 → **150**       |
| Crossbow   | 8 (unchanged)      | 2 (unchanged)         | 120 (unchanged)    |
| Spear      | 20 (unchanged)     | 3 → **5**             | 120 → **300**      |
| Shuriken   | 5 → **10**         | 2 (unchanged)         | 50 → **120**       |
| Boomerang  | 12 → **15**        | 2 → **2.5**           | 100 → **150**      |

Caps were raised above maximum loop-2 gate values so they never interfere with loop scaling.

### Weapon Stats (Damage Only)

| Weapon          | Base (before→after) | Max (before→after) |
|-----------------|--------------------|--------------------|
| Spear1          | 180 → **65**       | 260 → **110**      |
| Spear2          | 260 → **110**      | 380 → **190**      |
| Spear3          | 380 → **190**      | 560 → **300**      |
| Boomerang1      | 80 → **55**        | 100 → **80**       |
| Boomerang2      | 100 → **80**       | 150 → **130**      |
| All others      | unchanged          | unchanged          |

Spear remains the highest damage class: Spear1 base=65 vs Bow1 base=35 (1.9x ratio). Spear1 max=110 vs Bow1 max=55 (2.0x ratio).

### Rewards

| Mode      | Before (Cash)    | After (Cash)     | Before (Token)   | After (Token)    |
|-----------|-----------------|-----------------|-----------------|-----------------|
| Campaign  | 100+10N         | **70+7N**        | 2+2N (class)    | **1 flat**       |
| Contracts | 50+50N          | **40+15N**       | 5+5N (all)      | **3+1N (all)**   |
| Sniper    | 200+100N        | **150+50N**      | 3+2N (xbow)     | **2+1N (xbow)**  |
| Boss      | 400+100N        | **300+100N**     | 10+10N (all)    | **3+2N (all)**   |

### Starting Tokens

| Currency       | Before | After |
|----------------|--------|-------|
| BowToken       | 3      | **1** |
| CrossbowToken  | 3      | **1** |
| SpearToken     | 6      | **1** |
| ShurikenToken  | 3      | **1** |
| BoomerangToken | 10     | **1** |

### Loop Multipliers

| Loop | GateDamage (before→after) | CampaignDifficulty | CampaignCash | ContractCash |
|------|--------------------------|-------------------|--------------|--------------|
| 0    | 1.0 (unchanged)          | 1.0               | 1.0          | 1.0          |
| 1    | 1.0 → **1.25**           | 1.2 → **1.3**     | 1.0 → **1.1** | 1.0 → **1.1** |
| 2    | 1.0 → **1.55**           | 1.35 → **1.5**    | 1.0 → **1.2** | 1.0 → **1.25** |

### Dead Code Removed

- `_gateComfortFactor` field and `GateComfortFactor` property removed from `GateModule`
- `GetGateComfortFactor()` removed from `BalanceConfig`
- `EconomyPacingModule` class removed, `_economyPacing` field and `EconomyPacing` property removed from `BalanceConfig`
- `_economyPacing` block removed from `BalanceConfig.asset`

### New Systems Added

- `SniperAccessProfile` in `BalanceConfig.GateModule` — Crossbow damage gate for Sniper access
- `IMissionGateService.CheckSniperGate()` — Sniper gate check
- `SniperDamageTooLow` in `AvailabilityBlockReason` enum
- `MissionAvailabilityService` now checks Sniper gate (requires `MetaLoopProgressData` injection)
- `MissionStartService` returns Sniper gate result on failure for UI popup use

---

## 2. Gate Table Per Campaign Mission (After Fix)

| g  | Zone | Class     | New Gate | Starter Weapon | Starter Dmg | Status         | What Blocks |
|----|------|-----------|----------|----------------|-------------|----------------|-------------|
| 0  | 1    | Bow       | ~0       | Bow1           | 35          | PASS (ungated) | — |
| 1  | 1    | Shuriken  | 12       | Shuriken1      | 25          | PASS           | — |
| 2  | 1    | Bow       | 29       | Bow1           | 35          | PASS           | — |
| 3  | 1    | Shuriken  | 16       | Shuriken1      | 25          | PASS           | — |
| 4  | 1    | Bow       | 33       | Bow1           | 35          | PASS           | — |
| 5  | 1    | Shuriken  | 20       | Shuriken1      | 25          | PASS           | — |
| **6** | **1** | **Bow** | **37** | **Bow1**     | **35**      | **BLOCK**      | **Teaching gate: +1 upgrade** |
| 7  | 2    | Spear     | 55       | Spear1         | 65          | PASS           | First Spear mission free |
| **8** | **2** | **Shuriken** | **26** | **Shuriken1** | **25** | **BLOCK**  | **+1 upgrade (cheap)** |
| 9  | 2    | Bow       | 43       | Bow1 L1        | 37          | BLOCK          | Need Bow1 L4 total |
| **10** | **2** | **Spear** | **70** | **Spear1** | **65**    | **BLOCK**      | **+1 upgrade** |
| 11 | 2    | Shuriken  | 32       | Shuriken1 L1   | 26          | BLOCK          | Need L7 total |
| 12 | 2    | Bow       | 49       | Bow1 L4        | 43          | BLOCK          | Need L7 total |
| **13** | **2** | **Spear** | **85** | **Spear1** | **70.5** | **BLOCK**     | **Need L4** |
| **14** | **2** | **Shuriken** | **38** | **Shuriken1** | **32** | **BLOCK** | **Shuriken1 max=35 < 38. Forced purchase!** |
| 15 | 3    | Boomerang | 52.5     | Boom1          | 55          | PASS           | First Boom mission free |
| **16** | **3** | **Bow** | **57** | **Bow1 max**  | **55**      | **BLOCK**      | **Bow1 max=55 < 57. Forced Bow2!** |
| 17 | 3    | Shuriken  | 44       | Shuriken2      | 35          | BLOCK          | Need Shuriken2 L6 |
| **18** | **3** | **Spear** | **110** | **Spear1** | **varies** | **BLOCK**    | **Need Spear1 max=110** |
| **19** | **3** | **Boomerang** | **62.5** | **Boom1** | **55** | **BLOCK** | **Need Boom1 L3=64** |
| 20 | 3    | Bow       | 65       | Bow2           | 55+         | BLOCK          | Need Bow2 L4 |
| **21** | **3** | **Shuriken** | **52** | **Shuriken2** | **50** | **BLOCK** | **Shuriken2 max=50 < 52. Forced Shuriken3!** |
| **22** | **3** | **Spear** | **130** | **Spear1** | **110**   | **BLOCK**      | **Spear1 max=110 < 130. Forced Spear2!** |

**Bold = requires player action.**

Forced purchases: Shuriken2 (g14), Bow2 (g16), Shuriken3 (g21), Spear2 (g22).

---

## 3. Weapon Damage vs Gates (After Fix)

| Class     | Starter Base | Starter Max | First Gate Passed | First Gate Blocked | Gate Forcing Purchase |
|-----------|-------------|-------------|------------------|--------------------|-----------------------|
| Bow       | 35          | 55          | g2=29            | g6=37 (L1)         | g16=57 (Bow2)         |
| Crossbow  | 22          | 37          | Sniper 0-2       | Sniper 3=24 (L2)   | Sniper 8=39 (Xbow2)   |
| Spear     | 65          | 110         | g7=55 (free)     | g10=70 (L1)        | g22=130 (Spear2)      |
| Shuriken  | 25          | 35          | g1=12            | g8=26 (L1)         | g14=38 (Shuriken2)    |
| Boomerang | 55          | 80          | g15=52.5 (free)  | g19=62.5 (L3)      | g21=52 via Shuriken3  |

No class auto-passes all gates with base weapon. All classes require at least 1 meaningful upgrade.

---

## 4. Upgrade Cost Table (After Fix)

### To Pass First Block (cheapest viable upgrade path)

| Class     | First Block | Cost to Unblock | Cash | Tokens |
|-----------|-------------|-----------------|------|--------|
| Bow       | g6=37       | Bow1 L1         | 50   | 1      |
| Shuriken  | g8=26       | Shuriken1 L1    | 50   | 1      |
| Spear     | g10=70      | Spear1 L1       | 70   | 2      |
| Boomerang | g19=62.5    | Boom1 L3        | 162  | 3      |
| Crossbow  | Sniper 3=24 | Xbow1 L2        | 104  | 2      |

All first-block costs are affordable from a few missions of income. Teaching gates are designed to be quick to pass.

### Total to Max Starter Weapons (L0→L10)

| Weapon     | Cash  | Tokens |
|------------|-------|--------|
| Bow1       | 825   | 14.5   |
| Spear1     | 1125  | 34.5   |
| Shuriken1  | 680   | 14.5   |
| Boom1      | 680   | 14.5   |
| Xbow1      | 680   | 14.5   |

Spear1 costs 1.65x more than other starters in tokens, reflecting higher damage magnitude.

---

## 5. Reward Curve Table (After Fix)

### Campaign Income (Minimal Player, No Upgrades)

| After      | Cumul Cash | Bow Tok | Shuriken Tok | Spear Tok | Boom Tok |
|------------|-----------|---------|--------------|-----------|----------|
| Zone 1 end | 637 + 100 start = 737 | 4+1=5 | 3+1=4 | 0+1=1 | 0+1=1 |
| Zone 2 end | 1785 (missions) + 100 start = 1885 | 7 | 7 | 4 | 1 |
| Zone 3 end | 3381 + 100 start = 3481 | 9 | 9 | 6 | 3 |

### Zone 1 Token Analysis (Key Data Point)

**Before fix:** Zone 1 gave 32 Bow tokens → player could max Bow1 twice over
**After fix:** Zone 1 gives 5 Bow tokens → covers teaching gate (1 tok) + Zone 2 early gates (4 tok). To advance further, player must do Contracts.

### Contracts Value (After Fix)

| Run | Cash | Tokens per Class |
|-----|------|-----------------|
| 0   | 40   | 3               |
| 1   | 55   | 4               |
| 3   | 85   | 6               |
| 5   | 115  | 8               |

3 Contracts runs: ~195 cash, 18 tokens per class (3+4+5). Enough to upgrade Bow1 to L4-L5 range.

### Sniper Value (After Fix)

| Run | Cash | XbowToken |
|-----|------|-----------|
| 0   | 150  | 2         |
| 2   | 250  | 4         |
| 5   | 400  | 7         |

Sniper is clearly better than Contracts for cash. The 9 Crossbow tokens from first 4 Sniper runs exactly fund the upgrades to pass Sniper gates 3-6.

---

## 6. Scenario Simulations (After Fix)

### Scenario A — Minimal Player (Campaign only, upgrades only when blocked)

| Zone | Blocks | Actions Required | Grind Needed? |
|------|--------|-----------------|---------------|
| Zone 1 | **1** (g6, teaching gate) | Bow1 L1 (50 cash, 1 tok) | No |
| Zone 2 | **5** (g8, g9, g10, g11+g12, g13, g14) | Multiple upgrades; Shuriken2 purchase forced | **1-2 Contracts runs for tokens** |
| Zone 3 | **6** (g16, g17, g18, g19, g20, g21, g22) | Bow2 purchase, Shuriken3 purchase, Spear2 purchase | **3-5 Contracts/Sniper runs** |
| **Total blocks** | **~12** | — | **Yes — multiple grind runs** |

### Scenario B — Efficient Player (upgrades proactively)

| Zone | Strategy | Blocks | Grind Needed? |
|------|----------|--------|---------------|
| Zone 1 | After g6 teaching gate, upgrades Bow1 to L4 | 1 (taught) | No |
| Zone 2 | Proactively upgrades each class when tokens arrive | 1-2 | 1 Contracts for tokens |
| Zone 3 | Still needs weapon purchases (Bow2, Shuriken2, Spear2) | 2-3 purchase forced | 2-3 Contracts/Sniper runs |

### Scenario C — Grinder (maxes weapons before advancing)

- After Zone 1: tries to max Bow1 (825 cash, 14.5 tok) and Shuriken1 (680 cash, 14.5 tok)
- Zone 1 income: 737 cash, 5 BowTok, 4 ShurikenTok
- **Cannot max both starters from Zone 1 income alone — must do 2-4 Contracts runs**
- This is the intended behavior: Contracts is now a required grind source even for grinders

---

## 7. Expected First Gate Per Zone

| Zone | First Gate | Class    | Gate Value | Required Action |
|------|-----------|----------|------------|-----------------|
| Zone 1 end | g6 | Bow | 37 | **Bow1 L1** — 50 cash, 1 token |
| Zone 2 entry | g8 | Shuriken | 26 | **Shuriken1 L1** — 50 cash, 1 token |
| Zone 2 mid | g10 | Spear | 70 | **Spear1 L1** — 70 cash, 2 tokens |
| Zone 2 end | g14 | Shuriken | 38 | **Buy Shuriken2** — 650 cash, 15 tokens |
| Zone 3 entry | g16 | Bow | 57 | **Buy Bow2** — 600 cash, 14 tokens |
| Zone 3 mid | g18 | Spear | 110 | **Spear1 max** — 1125 cash total, 34.5 tokens |
| Zone 3 end | g22 | Spear | 130 | **Buy Spear2** — 1000 cash, 25 tokens |

---

## 8. Expected Grind Runs Needed for Key Gates

| Gate | Class | What Player Needs | Est. Contracts Runs | Est. Sniper Runs |
|------|-------|------------------|--------------------|--------------------|
| g6 teaching (Zone 1) | Bow | 1 Bow token above Zone 1 income | 0 | 0 |
| g9 (Zone 2) | Bow | Bow1 L4 total = 4 tokens | ~1 Contracts | 0 |
| g11 (Zone 2) | Shuriken | Shuriken1 L7 = 7 tokens | ~1 Contracts | 0 |
| Shuriken2 purchase (g14) | Shuriken | 650 cash + 15 tokens | ~2 Contracts | ~2 Sniper |
| Bow2 purchase (g16) | Bow | 600 cash + 14 tokens | ~2 Contracts | ~2 Sniper |
| Spear2 purchase (g22) | Spear | 1000 cash + 25 tokens | ~3 Contracts | ~3 Sniper |

The Loop 1 gates are 25% harder — all the above gates require ~1 extra upgrade level, pushing grind by an additional 1-2 Contracts runs per major upgrade.

---

## 9. Sniper Access Gate (New System)

| Sniper Index | Required Xbow Dmg | Xbow1 Level Needed | Xbow Tokens Earned Before |
|-------------|------------------|---------------------|--------------------------|
| 0           | 15               | L0 (base=22)        | 1 (start)                |
| 1           | 18               | L0                  | 3                        |
| 2           | 21               | L0 (barely)         | 6                        |
| 3           | 24               | L2 (25)             | 10                       |
| 4           | 27               | L4 (28)             | 15                       |
| 5           | 30               | L6 (31)             | 21                       |
| 6           | 33               | L8 (34)             | 28                       |
| 7           | 36               | L10 (37)            | 36                       |
| 8           | 39               | Crossbow2 needed    | 45                       |

The Sniper reward loop is self-funding: each Sniper run earns `2+sniperIndex` Crossbow tokens, which fund the upgrade needed for the next gate. First 3 Snipers are free (passes at base). The player faces their first Sniper gate at the 4th Sniper mission, by which point they've earned ~10 CrossbowTokens — enough to upgrade to L2.

---

## 10. Loop 2/3 Pressure

### Key gates × loop multipliers:

| Gate          | Loop 0 | Loop 1 (×1.25) | Loop 2 (×1.55) |
|---------------|--------|----------------|----------------|
| Bow g16       | 57     | 71.25          | 88.35          |
| Bow g20       | 65     | 81.25          | 100.75         |
| Spear g13     | 85     | 106.25         | 131.75         |
| Spear g22     | 130    | 162.5          | 201.5          |
| Shuriken g21  | 52     | 65             | 80.6           |

- **Loop 1:** Spear g22=162.5 requires Spear2 L7. Bow g20=81.25 requires Bow2 max.
- **Loop 2:** Spear g22=201.5 requires Spear3 (purchase + 2 upgrades). Bow g20=100.75 requires Bow3.

Loop 2 forces weapon tier upgrades across all classes.

---

## 11. Remaining Risks and Follow-up Tuning Notes

### Active Risks

| Risk | Severity | Description |
|------|----------|-------------|
| Spear token cost | MEDIUM | Spear1 now costs 34.5 tokens to max (startTok=2, maxTok=5). Starting with 1 SpearTok and only 4-6 SpearTok from Campaign before Spear gates appear, player may need multiple Contracts runs just for Spear tokens. Monitor in playtesting. |
| Zone 2 block density | MEDIUM | 5 blocks in Zone 2 (g8, g9/g10, g11/g12, g13, g14) is high. If blocks cluster (g11+g12 are consecutive Shuriken+Bow), it feels punishing. Consider tweaking Zone 2 mission order if feedback is "too many blocks in a row." |
| Shuriken2 purchase pressure | MEDIUM | Gate g14 forces Shuriken2 (650 cash + 15 tokens). Player may not have enough from Campaign alone. Should be fine with 1 Contracts + 1 Sniper run before g14. Verify in playtesting. |
| Existing tester saves | LOW | Existing saves with Spear/Boomerang weapons will auto-recalculate via LoadoutSnapshotUpdater on next init. Old gate saves are fine. |
| SniperAccessProfile defaults | LOW | Unity will serialize default values (BaseDamage=15, MissionGrowth=3) on first load. No manual Inspector action needed, but verify in Inspector after first Unity load. |
| Crossbow2 access (Sniper 8+) | LOW | Sniper index 8 needs Crossbow2 (base 37<39). Only 2 Sniper missions remain in Zone 3. By that point, player likely has Crossbow2 from previous campaign level unlock. But verify unlock timing. |

### Recommended Follow-up

1. **Playtest Zone 2 pacing** — If consecutive blocks at g11 (Shuriken) + g12 (Bow) feel bad, consider swapping one to a non-consecutive slot.
2. **Calibrate Contracts run frequency** — The intended grind pace is 1-2 Contracts per Campaign zone transition. Add telemetry tracking to verify.
3. **Sniper gate popup** — `MissionStartService` returns the `SniperGateResult` on failure, but no dedicated popup is wired in `PlayMissionPresenter`. A Sniper-specific gate popup (showing required Crossbow damage) should be added for proper UX.
4. **Loop 2 reward multipliers** — At loop 2, cash rewards increase by 20-25%. Verify that increased cash doesn't trivialize weapon purchases in loop 2. If so, increase weapon purchase prices in loop 2 using the `EconomyCost` loop multiplier (already defined in `LoopMultipliers`, currently unused).
5. **Boomerang upgrades** — With only 2 Boomerang Campaign missions, Boomerang upgrade pressure is low. If Boomerang feels underserved, consider adding one more Boomerang mission to Zone 2 or early Zone 3.
6. **Spear1 upgrade cost tuning** — startTok=2 / maxTok=5 for Spear1 is a significant increase from the original 1/2. If testers find Spear upgrades too expensive token-wise, lower to startTok=1/maxTok=3.
