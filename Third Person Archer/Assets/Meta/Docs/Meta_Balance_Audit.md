# Meta Balance Audit

**Date:** 2026-05-27
**Scope:** `Assets/Meta` — gate system, weapon damage, upgrade economy, reward flow
**Problem statement:** Testers report not hitting damage gates. Goal of this audit is to find the mathematical root cause.

---

## Table of Contents

1. [Gate Curve Per Mission](#1-gate-curve-per-mission)
2. [Weapon Damage vs Gates](#2-weapon-damage-vs-gates)
3. [Upgrade Economy Math](#3-upgrade-economy-math)
4. [Reward Economy](#4-reward-economy)
5. [Player Scenario Simulations](#5-player-scenario-simulations)
6. [Gate Frequency Analysis](#6-gate-frequency-analysis)
7. [Runtime Correctness Findings](#7-runtime-correctness-findings)
8. [Problem List with Severity](#8-problem-list-with-severity)
9. [Root Cause Summary](#9-root-cause-summary)
10. [Recommendations](#10-recommendations)
11. [Designer Questions](#11-designer-questions)
12. [Data Sources](#12-data-sources)

---

## 1. Gate Curve Per Mission

### Formula (from `BalanceConfig.GateModule.cs` + `BalanceConfig.asset`)

```
value = BaseDamage + MissionGrowth * globalIndex
value *= max(0.01, AdditionalCurve.Evaluate(globalIndex))
requiredDamage = min(value, DamageCap)
```

`AdditionalCurve` has two keyframes: `(0, 0)` and `(1, 1)` with `PostInfinity = Clamp`.
- At `globalIndex = 0`: multiplier = `max(0.01, 0) = 0.01` → gate ≈ 0 (intentionally ungated)
- At `globalIndex >= 1`: multiplier = `1.0` (clamps at the second keyframe)

The curve effectively means: **mission 1 is completely ungated; all others use the linear formula directly.**

### Gate Profiles (from `BalanceConfig.asset`)

| Class      | BaseDamage | MissionGrowth | DamageCap |
|------------|-----------|---------------|-----------|
| Bow        | 10        | 2.5           | 92        |
| Crossbow   | 8         | 2.0           | 120       |
| Spear      | 20        | 3.0           | 120       |
| Shuriken   | 5         | 2.0           | 50        |
| Boomerang  | 12        | 2.0           | 100       |

### Computed Gate Values Per Campaign Mission

Campaign missions are assigned weapon classes: Zone 1 alternates Bow/Shuriken, Zone 2 adds Spear, Zone 3 adds Boomerang. **Crossbow is never assigned to any Campaign mission.**

| g | Zone | Mission | Class      | Gate Required |
|---|------|---------|------------|---------------|
| 0 | 1    | 1       | Bow        | ~0 (ungated)  |
| 1 | 1    | 2       | Shuriken   | 7             |
| 2 | 1    | 3       | Bow        | 15            |
| 3 | 1    | 4       | Shuriken   | 11            |
| 4 | 1    | 5       | Bow        | 20            |
| 5 | 1    | 6       | Shuriken   | 15            |
| 6 | 1    | 7       | Bow        | 25            |
| 7 | 2    | 1       | Spear      | **41**        |
| 8 | 2    | 2       | Shuriken   | 21            |
| 9 | 2    | 3       | Bow        | 32.5          |
|10 | 2    | 4       | Spear      | **50**        |
|11 | 2    | 5       | Shuriken   | **27**        |
|12 | 2    | 6       | Bow        | **40**        |
|13 | 2    | 7       | Spear      | **59**        |
|14 | 2    | 8       | Shuriken   | **33**        |
|15 | 3    | 1       | Boomerang  | **42**        |
|16 | 3    | 2       | Bow        | **50**        |
|17 | 3    | 3       | Shuriken   | **39**        |
|18 | 3    | 4       | Spear      | **74**        |
|19 | 3    | 5       | Boomerang  | **50**        |
|20 | 3    | 6       | Bow        | **60**        |
|21 | 3    | 7       | Shuriken   | **47**        |
|22 | 3    | 8       | Spear      | **86**        |

Bold = gate above starter weapon base damage (potential friction point).

### DamageCap Analysis

The cap values are never reached anywhere in the current 23-mission content:

| Class      | DamageCap | Highest Gate (g=22) | Gap  |
|------------|-----------|---------------------|------|
| Bow        | 92        | 65 (g=22)           | 27   |
| Crossbow   | 120       | 52 (g=22)           | 68   |
| Spear      | 120       | 86 (g=22)           | 34   |
| Shuriken   | 50        | 49 (g=22)           | 1    |
| Boomerang  | 100       | 56 (g=22)           | 44   |

DamageCap has zero effect on current content. Shuriken comes closest (cap=50 vs max gate=49).

---

## 2. Weapon Damage vs Gates

### Starter Weapon Damage (Level 0)

| Class      | Starter Weapon | Base Dmg | Max Dmg (L10) | First Gate | Ratio at L0 | First Real Block |
|------------|----------------|----------|---------------|------------|-------------|-----------------|
| Bow        | Bow WeaponDef 1| 35       | 55            | g2 = 15    | 2.33x       | g12 (40): needs L3 |
| Crossbow   | Xbow WeaponDef 1| 22      | 37            | N/A (no Campaign) | —    | Never gated in Campaign |
| Spear      | Spear WeaponDef 1| 180    | 260           | g7 = 41    | **4.39x**   | **Never blocked** |
| Shuriken   | Shuriken WeaponDef 1| 25  | 35            | g1 = 7     | 3.57x       | g11 (27): needs L2 |
| Boomerang  | Boom WeaponDef 1 | 80     | 100           | g15 = 42   | **1.90x**   | **Never blocked** |

### Class-by-Class Analysis

**Bow** — Moderate friction, only class that forces upgrade before end-game
- Zone 1 (g0-g6): passes all gates at L0 (max gate=25 vs 35 damage)
- Zone 2 g9 (32.5): passes at L0 barely (35 > 32.5)
- Zone 2 g12 (40): BLOCKED at L0. Needs L3 (cost: 162 cash, 3 tokens)
- Zone 3 g16 (50): BLOCKED even at L0 and L1. Needs L8 (cost: 512 cash, 8 tokens)
- Zone 3 g20 (60): BLOCKED even at max (L10=55). **Forces weapon purchase: Bow2 at 600 cash/14 tokens**
- Zone 3 g20 is the only Campaign gate that forces a weapon purchase for any class.

**Crossbow** — Zero Campaign gating
- Assigned only to Sniper and Boss missions.
- `MissionGateService` only checks gates for `MissionType.Campaign`.
- Crossbow is **never gate-checked** in the current content.

**Spear** — No friction at any point
- Starter damage (180) exceeds the highest Spear gate in the entire game (g22 = 86) by a factor of **2.09x**.
- All 5 Spear Campaign missions (g7, g10, g13, g18, g22) are passed at Spear1 base level.
- Upgrading Spear provides zero gating benefit — it is purely cosmetic for progression.

**Shuriken** — Light friction; cheapest to resolve
- g11 (27): blocked at L0 (25). Needs only 2 upgrades = 104 cash, 2 tokens.
- g14 (33): needs L8 = 512 cash, 8 tokens.
- g17 (39): Shuriken1 max (35) < 39. Forces Shuriken2 purchase (650 cash, 15 tokens) or Shuriken1 L10 alone is still insufficient.
- g21 (47): requires Shuriken2+ upgrades.
- Shuriken is the only class with consistent (if low-cost) friction throughout.

**Boomerang** — No friction at any point
- Starter damage (80) exceeds both Boomerang gates in the game (g15=42, g19=50) by factors of 1.9x and 1.6x.
- Boomerang is never blocked across the entire campaign.

---

## 3. Upgrade Economy Math

### Formulas

**Damage at level L:**
```
damage(L) = Lerp(BaseDmg, MaxDmg, L / maxUpgradeLevel)
           = BaseDmg + (MaxDmg - BaseDmg) * L / 10
```
Confirmed by `LoadoutSnapshot.asset`: Bow WeaponDef 1 at L3 shows damage=41 (expected: 35 + 2*3 = 41 ✓).

**Cost to upgrade from level L to L+1:**
```
cost(L) = Lerp(startCash, maxCash, priceCurve01.Evaluate(L / 10))
```
For a linear price curve: `cost(L) = startCash + (maxCash - startCash) * L / 10`

**Total cash to fully upgrade (L0 → L10):**
```
totalCash = sum(cost(L) for L in 0..9)
          = 10 * startCash + 4.5 * (maxCash - startCash)
```

**Total tokens to fully upgrade:**
```
totalTokens = 10 * startTok + 4.5 * (maxTok - startTok)
```

### Starter Weapon Full-Upgrade Costs

| Weapon          | Class     | Total Cash | Total Tokens | Dmg Gain | Cash per +1 Dmg | Tok per +1 Dmg |
|-----------------|-----------|------------|--------------|----------|-----------------|----------------|
| Bow WeaponDef 1 | Bow       | 680        | 14.5         | 20       | 34              | 0.73           |
| Spear WeaponDef 1 | Spear   | 680        | 14.5         | 80       | **8.5**         | **0.18**       |
| Shuriken WeaponDef 1 | Shuriken | 680   | 14.5         | 10       | 68              | 1.45           |
| Boomerang WeaponDef 1 | Boomerang | 680  | 14.5         | 20       | 34              | 0.73           |
| Xbow WeaponDef 1 | Crossbow  | 680        | 14.5         | 15       | 45              | 0.97           |

**All starter weapons cost the same to max (680 cash / 14.5 tokens), but deliver wildly different damage gains.**

Spear's base damage is already so far above any gate that upgrading it has no practical gating benefit. Despite this, Spear is the best value in the game in raw damage-per-cash.

### Damage Efficiency Imbalance

Cost per +1 damage (full upgrade, starter weapons):
- Spear: **8.5 cash/dmg** — 8x more efficient than Shuriken
- Bow: 34 cash/dmg
- Boomerang: 34 cash/dmg
- Crossbow: 45 cash/dmg
- Shuriken: **68 cash/dmg** — least efficient

This imbalance means players who happen to invest in Spear or Boomerang get far more upgrade value for the same spend, even though those weapons never block them.

### Starting Balance vs. Upgrade Costs

From `CurrencyStartingBalanceConfig.asset`:

| Currency       | Starting Amount | Bow1 Max Cost | Notes |
|----------------|-----------------|---------------|-------|
| Cash           | 100             | 680           | |
| BowToken       | 3               | 14.5          | |
| CrossbowToken  | 3               | 14.5          | |
| SpearToken     | 6               | 14.5          | |
| ShurikenToken  | 3               | 14.5          | |
| BoomerangToken | **10**          | 14.5          | Almost enough to max Boom1 immediately |

Starting BoomerangToken=10 means a new player has 69% of the tokens needed to max Boom1 before playing a single mission. Combined with Boomerang's base damage already clearing all Boomerang gates, there is effectively zero token friction for the Boomerang class.

---

## 4. Reward Economy

### Reward Formulas (from `BalanceConfig.RewardModule.cs`)

All rewards use `LinearProgression`: `reward = Base + PerStep * index`

Where `index` is the mission's global index (Campaign), or completion count for repeatable types.

### Campaign Reward Accumulation (Single Playthrough, No Upgrades)

| After Mission | Global Index | Cash Earned | Bow Tokens | Shuriken Tokens | Spear Tokens | Boom Tokens | Cumul. Cash |
|---------------|-------------|-------------|------------|-----------------|--------------|-------------|-------------|
| Z1 complete   | 6           | 160         | 32         | 24              | 0            | 0           | 910         |
| Z2 complete   | 14          | 240         | 52 (+20)   | 64 (+40)        | 38           | 0           | 2550        |
| Z3 complete   | 22          | 320         | 112 (+60)  | 132 (+68)       | 112 (+74)    | 72          | 4830        |

**Key observations:**
- After Zone 1 alone: 32 Bow tokens earned. Max Bow1 upgrade = 14.5 tokens. **Player can max Bow1 twice over from Zone 1 Campaign tokens alone.**
- After Zone 1: 24 Shuriken tokens. Max Shuriken1 = 14.5 tokens. **Player can max Shuriken1 with Zone 1 tokens alone.**
- By global index 6, token rewards (2+2*6=14) per mission exceed the per-token cost of most starter weapon levels.

### Token Reward Scaling vs. Weapon Token Costs

Token reward per Campaign mission at index N: `2 + 2*N`

| Mission Index | Token Reward | Cumul Tokens (same class) | Bow1 Max Cost | Shuriken1 Max Cost |
|---------------|-------------|--------------------------|---------------|-------------------|
| 0             | 2           | 2+3=5                    | 14.5          | 14.5              |
| 3             | 8           | 5+2+6+4+8=25             | 14.5          | 14.5              |
| 6             | 14          | 32                       | 14.5          | **paid off at g3** |
| 12            | 26          | 78                       | 14.5          | paid off at g3     |

Token income per mission grows linearly (`2+2*N`), but weapon token costs are fixed. By mission 7, a single mission rewards more tokens than two full starter weapon upgrades.

### Contracts and Boss Rewards

- **Contracts**: reward `AllTokens`, distributing to **all 5 classes simultaneously**. `5+5*N` tokens at index N means a single Contracts completion at index 1 gives 10 tokens to every class (50 total). This rapidly inflates the token economy.
- **Boss**: same `AllTokens` channel. Boss at index 0 gives 10 tokens to every class (50 total). First Boss completion effectively pays for a full starter weapon upgrade in every class at once.

---

## 5. Player Scenario Simulations

### Scenario A — Minimal Player (plays Campaign only, upgrades only when blocked)

| Event | Mission | Gate | Damage | Blocked? | Extra Spend |
|-------|---------|------|--------|----------|-------------|
| Start | — | — | Bow:35, Shuriken:25, Spear:180, Boom:80 | — | — |
| First block | Z2-C5 (g11) | Shuriken 27 | 25 | YES | 104 cash, 2 tokens → pass |
| Second block | Z2-C6 (g12) | Bow 40 | 35 | YES | 162 cash, 3 tokens → pass |
| Third block | Z2-C8 (g14) | Shuriken 33 | 27 | YES | ~408 cash, 6 tokens (to L8) → pass |
| Fourth block | Z3-C2 (g16) | Bow 50 | 35 | YES | ~350 cash, 5 tokens (to L8) → pass |
| Fifth block | Z3-C3 (g17) | Shuriken 39 | 33 | YES | 650 cash, 15 tokens (buy Shuriken2) → pass |
| Sixth block | Z3-C6 (g20) | Bow 60 | 51 | YES | 600 cash, 14 tokens (buy Bow2, Bow1 max=55<60) |
| Finish | Z3-C8 | Spear 86 | 180 | NO | Spear never blocked once |

**Total blocks for Scenario A: 6 blocks across 23 missions**
**Spear/Boomerang blocks: 0**
The player can complete all Spear and Boomerang missions with zero upgrades ever. Blocks occur exclusively in Bow and Shuriken missions.

### Scenario B — Efficient Player (upgrades proactively after each zone)

After Zone 1, uses ~200 cash and 4 tokens to take Bow to L3 and Shuriken to L3. All blocks from Scenario A are avoided. **Zero blocks across the entire campaign** with a small upfront investment that is trivially affordable from Zone 1 rewards alone.

### Scenario C — Grinder (maxes each weapon before advancing)

Maxes Bow1 (680 cash, 14.5 tokens) and Shuriken1 (680 cash, 14.5 tokens) before Zone 2. Has both weapons at max. **Zero blocks, ever.** The cost is affordable before Zone 2 starts from Zone 1 Campaign rewards alone (910 cash, 32 Bow tokens, 24 Shuriken tokens after Zone 1).

### Cross-Scenario Finding

All three scenarios confirm: **Spear and Boomerang never produce a block in any scenario**. The gate system produces meaningful friction only for Bow (mid-late game) and Shuriken (mid-game), and even that friction is resolved trivially cheaply. The intended "upgrade loop driver" does not function for 3 of the 5 weapon classes.

---

## 6. Gate Frequency Analysis

### Distribution of Blocks by Class

Out of 23 Campaign missions:
- **Bow gates that block a starter player:** g12, g16, g20 = **3 missions** (13%)
- **Shuriken gates that block a starter player:** g11, g14, g17, g21 = **4 missions** (17%)
- **Spear gates that block a starter player:** 0 missions (0%)
- **Boomerang gates that block a starter player:** 0 missions (0%)
- **Crossbow gates in Campaign:** 0 (class not assigned to Campaign)

### Block Density

| Zone | Missions | Blocks (Scenario A) | Block Rate |
|------|----------|--------------------|-----------:|
| Zone 1 | 7 | 0 | 0% |
| Zone 2 | 8 | 3 | 37.5% |
| Zone 3 | 8 | 3 | 37.5% |

Zone 1 is entirely friction-free. The first block appears at mission 11 (g11), more than halfway through Zone 2.

### Mission Spacing Between Blocks

Zone 2: blocks at g11, g12, g14 (clustered — two consecutive missions blocked)
Zone 3: blocks at g16, g17, g20 (near-consecutive in two cases)

**Clusters are worse than distributed blocks**: a player hitting two consecutive blocks (g11 Shuriken, g12 Bow) feels punished, not engaged. They must switch to the shop, upgrade two different weapons, then return.

### Gate Absence for Entire Classes

5 of the 5 weapon classes are present in the game. Only 2 (Bow, Shuriken) ever produce a gate block. The Spear and Boomerang gate profiles exist in config but have no practical effect because their starter weapon base damages were set too high relative to the growth curve.

---

## 7. Runtime Correctness Findings

### Gate Check — `MissionGateService`

The gate check runs correctly: `equippedDamage >= requiredDamage`.

- Gate damage is read from `LoadoutSnapshot.slot.Damage` (computed stats, not base stats).
- Snapshot is updated correctly by `LoadoutSnapshotUpdater` on equip and upgrade events.
- No off-by-one issues found. The formula in `BalanceConfig.GateModule.cs` matches the asset data.

**Issue:** `GateComfortFactor = 1.2` exists in `BalanceConfig` as an editor planning field. It is **not used at runtime** — the gate formula does not apply this factor. If the designer intended a 1.2x buffer on all gate values, it is silently ignored.

### Loop Multipliers — `MetaLoopProgressData`

Gate damage loop multipliers in `BalanceConfig.asset`:
- Loop 0: 1.0
- Loop 1: 1.0
- Loop 2: 1.0

All three loops apply the **same gate values**. Completing the game and looping does not increase difficulty. This is likely intentional but means repeat players never encounter harder gates.

### Weapon Requirement Service — `WeaponRequirementService`

- Loop 0: `mission.BaseWeaponClass` — correct per design.
- Loop N: `(baseIndex + N * rotationStep) % classCount` where `rotationStep = 1`.
- This rotates weapon class requirements on each loop, which could create unexpected gate mismatches (e.g., a Spear mission now requiring Bow) in loops 1+. No validation that the rotated class assignment produces sane gate values was found.

### `MissionGateService` — Campaign Only

Gate is only evaluated for `MissionType.Campaign`. Sniper and Boss missions with `fixedWeaponClass` are **not gate-checked**. A player with Crossbow damage below the Crossbow gate formula threshold can still play all Sniper/Boss missions. This is presumably intentional but makes Crossbow gating entirely decorative.

### `Save` Method Redundancy in `MissionSegmentData`

```csharp
private void Save()
{
    if (string.IsNullOrEmpty(_saveKey))
    {
        Debug.LogError(...);
        return;
    }
    // BUG: identical null check immediately after
    if (string.IsNullOrEmpty(_saveKey))
        return;
    ...
}
```

The second `if (string.IsNullOrEmpty(_saveKey))` check at line 146 is dead code — the first check already returns. No functional impact, but indicates copy-paste error.

### EconomyPacingModule — Editor Only, Not Enforced

`BalanceConfig.EconomyPacingModule.cs` contains design targets (e.g., `TargetUpgradesPerCampaignMission`, `TargetCampaignSessionsPerWeapon`) but these are planning parameters for an editor generator tool. They are **not enforced by any runtime system**. The actual balance is determined entirely by the hardcoded values in `BalanceConfig.asset`, which do not match the design targets.

---

## 8. Problem List with Severity

### P1 — CRITICAL: Spear and Boomerang starter weapons exceed all gates

**Spear1 base damage (180) is 2.09x the highest Spear gate in the entire game (g22 = 86).**
**Boomerang1 base damage (80) is 1.6x the highest Boomerang gate (g19 = 50).**

A player with Spear1 or Boomerang1 at Level 0 passes every single gate for those classes. There is zero incentive to upgrade these weapons for progression purposes. 5 of 23 Campaign missions (Spear: g7,g10,g13,g18,g22 and Boomerang: g15,g19) provide no gating friction whatsoever.

**Root cause:** Starter weapon base damages were set to values that match or exceed the weapon's gate cap, rather than starting below the mid-game gate curve.

### P2 — CRITICAL: Crossbow never assigned to Campaign missions

Crossbow has a complete gate profile (Base=8, Growth=2, Cap=120) but is never assigned to any Campaign mission. All Campaign missions use Bow, Shuriken, Spear, or Boomerang. Crossbow appears only in Sniper (`fixedWeaponClass=1`) and Boss missions, which are not gate-checked. The entire Crossbow gate system is dead code in the current content.

**Root cause:** Mission asset assignment mismatch — Crossbow gate profile exists but no Campaign mission uses it.

### P3 — CRITICAL: Token rewards outpace weapon upgrade costs from Zone 1

Campaign token rewards follow `2 + 2*N` per mission (class-specific). By the end of Zone 1 (7 missions), a player earns:
- 32 Bow tokens (vs max Bow1 upgrade cost of 14.5 tokens)
- 24 Shuriken tokens (vs max Shuriken1 upgrade cost of 14.5 tokens)

A player who never upgrades anything still accumulates enough tokens to max their starter weapons in 2-3 missions into Zone 2. By mid-game, token rewards per mission (2+2*14=30) are roughly **twice the full upgrade cost** of a starter weapon.

**Root cause:** `PerStep` value for `MissionToken` reward is too high (2 per mission) and scales upward rather than being fixed or slowly scaling.

### P4 — CRITICAL: GateComfortFactor not applied at runtime

`BalanceConfig` contains `GateComfortFactor = 1.2` (editor field). This value is **never used** in `BalanceConfig.GateModule.cs`. If this was intended to reduce gate requirements by 20% (i.e., `requiredDamage /= 1.2`) to give players headroom, it is silently ignored. If it was intended as a multiplier to increase gates, it is also silently ignored.

The actual runtime formula applies no comfort factor at all.

### P5 — MAJOR: Loop GateDamage multiplier = 1.0 for all loops

`BalanceConfig.asset` sets GateDamage multipliers to 1.0 for loops 0, 1, and 2. Players who replay the game (new loop) face identical gate requirements. There is no difficulty progression across loops.

**Note:** This may be intentional design. But it means repeat players are permanently past the upgrade-pressure phase.

### P6 — MAJOR: Upgrade costs not scaled to weapon class damage magnitude

All starter weapons share the same upgrade cost structure (startCash=50, maxCash=90, startTok=1, maxTok=2). But Spear1 gains 80 damage for that cost while Shuriken1 gains only 10 damage. Spear gives **8x more damage per cash** than Shuriken, yet Spear never needs upgrading for gates. The most economically efficient upgrade (Spear) is the most useless for gate progression. The least efficient upgrade (Shuriken) is the only one that matters.

### P7 — MAJOR: Boss rewards inflate all-class tokens simultaneously

First Boss completion gives `AllTokens` reward: `10 + 10*0 = 10` tokens to **every class** (50 total). This single event effectively pays for a full starter weapon upgrade across all 5 classes simultaneously. Players who complete the Zone 1 boss (requiring Crossbow which has no gate) immediately have enough tokens to max any starter weapon they choose.

### P8 — MINOR: AdditionalCurve "warmup" period has no effect

The `AdditionalCurve` keyframes at (0,0)→(1,1) are designed to provide a soft warmup, but since `globalIndex` is always an integer, the curve only ever evaluates at `x=0` (multiply by 0.01) or `x>=1` (multiply by 1.0). The interpolation between keyframes is never used. This is effectively a binary switch: mission 0 is ungated, all others use the full linear formula.

### P9 — MINOR: Dead code in `MissionSegmentData.Save()`

Duplicate `string.IsNullOrEmpty(_saveKey)` check at line 146. No functional impact.

---

## 9. Root Cause Summary

The testers' experience of "not hitting gates" has three compounding root causes:

**Root cause 1 — Starter weapon base damage set too high for Spear and Boomerang.**
The gate formula produces meaningful pressure only when the weapon's base damage is *below* the early-game gate curve. For Spear (180 base vs 41 first gate) and Boomerang (80 base vs 42 first gate), the base damage was set to match or exceed even the end-game gate, making the entire gate system irrelevant for those classes. 7 of 23 Campaign missions (30%) produce zero friction for any player.

**Root cause 2 — Token economy rewards outpace upgrade costs by a large margin.**
Even for the two classes that do produce gates (Bow and Shuriken), the friction is resolved trivially. After Zone 1, a player has earned 32 Bow tokens and 24 Shuriken tokens from Campaign rewards alone — more than enough to max both starter weapons (each costs 14.5 tokens). The cost to pass a gate is always affordable because token income grows faster than upgrade costs.

**Root cause 3 — Crossbow gate system is structurally disconnected from Campaign flow.**
Crossbow has a gate profile but zero Campaign missions. Sniper/Boss missions are not gate-checked. The Crossbow class weapon investment has no connection to progression gates at all.

These three issues together mean: 2 of 5 weapon classes (Spear, Boomerang) never block anyone; 1 of 5 (Crossbow) is structurally excluded from Campaign gating; and the 2 classes that do block (Bow, Shuriken) are unblocked cheaply within minutes of hitting the gate.

---

## 10. Recommendations

### R1 — Reduce Spear and Boomerang starter base damage [CRITICAL]

Target: starter base damage should fall *below* the first gate of its class.

Suggested targets (to create friction at early Zone 2 missions):
- Spear1 base: **30-40** (vs current 180). First Spear gate (g7) = 41. Player needs 1-2 upgrades.
- Boomerang1 base: **30-35** (vs current 80). First Boomerang gate (g15) = 42. Player needs upgrades.

Alternatively, increase the Spear and Boomerang gate `BaseDamage` and `MissionGrowth` values significantly. A Spear gate base of 140+ would start creating friction.

Both approaches require iterating on feel — the exact numbers should be validated in playtesting.

### R2 — Assign Crossbow to at least one Campaign mission per zone [CRITICAL]

Crossbow needs Campaign missions to make its gate (and the upgrade loop) meaningful. Replacing 1-2 Sniper-exclusive missions with Campaign Crossbow missions per zone, or replacing some of the repeated Bow/Shuriken missions in Zone 2-3, would bring Crossbow into the core upgrade loop.

### R3 — Reduce token reward PerStep [CRITICAL]

Current: `2 + 2*N` per Campaign mission. Suggested: `1 + 1*N` or even a fixed `2` per mission.

Contracts `AllTokens` reward (5+5N) is the largest driver of token inflation — reducing its PerStep from 5 to 1-2 would have a large effect. Boss `AllTokens` should be kept as a meaningful reward spike but may need its base reduced.

### R4 — Wire GateComfortFactor into the runtime formula, or remove it [CRITICAL]

Either apply it: `requiredDamage = calculatedGate / GateComfortFactor` (making gates 20% easier), or remove the field. Leaving a balance knob that does nothing is misleading to designers.

### R5 — Scale upgrade costs by weapon damage magnitude [MAJOR]

Spear and Boomerang should cost significantly more per level than Bow/Shuriken, reflecting their higher damage output. This prevents situations where the "free upgrade" (Spear is cheap + high damage) is also the one the player never needs.

Example: Spear upgrade token cost could be 3-5x higher than Shuriken's.

### R6 — Increase GateDamage loop multipliers [MAJOR]

Loop 1: 1.2x, Loop 2: 1.5x (or similar). This makes repeat runs meaningful and preserves upgrade pressure for experienced players. Current 1.0/1.0/1.0 means veteran players never engage the upgrade loop again.

### R7 — Redistribute gate blocks to avoid clusters [MINOR]

Consecutive blocks at g11+g12 (Shuriken+Bow in Zone 2) and g16+g17 (Bow+Shuriken in Zone 3) create frustrating back-to-back stops. Consider interleaving mission class assignments so no two consecutive Campaign missions require the same weapon class or both require upgrading simultaneously.

### R8 — Add at least one gate block in Zone 1 [MINOR]

Zone 1 is entirely frictionless — zero blocks for any player type. The first block doesn't appear until mission 11. Consider introducing a soft block at Zone 1 mission 5-6 (g4-g5) by either raising gate values or lowering starter damage, to teach the upgrade loop early.

---

## 11. Designer Questions

**DQ1.** Was Spear's base damage (180) intentionally set above all Spear gates? If so, what is the design intent for the Spear upgrade loop?

**DQ2.** Was Crossbow intentionally excluded from Campaign missions, or is this an oversight? If intentional, what is the intended upgrade driver for Crossbow investment?

**DQ3.** Is `GateComfortFactor = 1.2` meant to be applied at runtime? If yes, which direction — divide requiredDamage (easier gates) or multiply (harder gates)?

**DQ4.** The loop GateDamage multipliers are all 1.0 across 3 loops. Is this intentional, or were the loop values not yet set?

**DQ5.** What is the intended number of missions a player should be blocked per zone? The current design produces 0 blocks in Zone 1, 3 in Zone 2, 3 in Zone 3 (all in Bow/Shuriken only).

**DQ6.** Contract rewards distribute `AllTokens` (all 5 classes simultaneously). Was this intended to accelerate the token economy for all classes, or should contracts reward only a specific class?

**DQ7.** What is the intended "friction point" for first Spear mission (g7, gate=41)? The current starter (180) gives a 4.4x margin. Target margin: 1.0x (gate), 0.8x (slightly below gate, requiring 1 upgrade), or higher?

**DQ8.** Is the increasing token reward per mission (`2 + 2*N`) intentional — i.e., are late-game missions intended to produce large token surpluses — or is this a scaling oversight?

**DQ9.** The `EconomyPacingModule` in BalanceConfig contains fields like `TargetUpgradesPerCampaignMission`. Were these targets ever matched against the actual balance values? They are not enforced at runtime.

**DQ10.** Zone 3 has no Boss mission listed in the current content. Is this intentional (Zone 3 is unfinished), and how does this affect the loop completion trigger?

---

## 12. Data Sources

All calculations in this document are derived from the following files, read and verified directly:

| File | Key Data |
|------|----------|
| `Assets/Meta/Balance/Config/BalanceConfig.asset` | Gate profiles, reward progressions, loop multipliers, starting balances |
| `Assets/Meta/Balance/Config/BalanceConfig.GateModule.cs` | Gate formula implementation |
| `Assets/Meta/Balance/Config/BalanceConfig.RewardModule.cs` | Reward formula implementation |
| `Assets/Meta/Balance/Config/BalanceConfig.EconomyPacingModule.cs` | Editor-only pacing targets |
| `Assets/Meta/Weapons/Data/Wapons/*.asset` | All 26 weapon definitions (base/max damage, upgrade costs, unlock levels) |
| `Assets/Meta/Weapons/LoadoutSnapshot.asset` | Tester's current save state (Bow L3, all others L0) |
| `Assets/Meta/Weapons/Data/DefaultLoadout.asset` | Starting weapon assignments |
| `Assets/Meta/Economy/Data/CurrencyStartingBalanceConfig.asset` | Starting currency amounts |
| `Assets/Meta/Missions/Data/Zones/*/Missions/*.asset` | All 3 zones, all mission definitions (class assignments) |
| `Assets/Meta/Missions/Services/MissionGateService.cs` | Gate check logic |
| `Assets/Meta/Missions/Services/WeaponRequirementService.cs` | Weapon class requirement per loop |
| `Assets/Meta/Missions/Services/MissionCompletionService.cs` | Progress advancement logic |
| `Assets/Meta/Missions/Model/MissionSegmentData.cs` | Segment progress and save logic |
| `Assets/Meta/Weapons/Loadout/LoadoutSnapshotWeaponStatService.cs` | Gate reads equipped damage from snapshot |
| `Assets/Meta/Weapons/Loadout/LoadoutSnapshotUpdater.cs` | Snapshot update on equip/upgrade |
| `Assets/Meta/Economy/UpgradePriceProfile.cs` | Upgrade cost formula |

### Supporting Tables

Detailed computed tables are in `Assets/Meta/Docs/BalanceTables/`:

| File | Contents |
|------|----------|
| `campaign_gate_curve.csv` | Gate required vs starter damage for every Campaign mission |
| `weapon_damage_curve.csv` | Damage at each upgrade level for key weapons with costs |
| `upgrade_economy.csv` | Total upgrade costs, damage efficiency, gate unlock cost |
| `reward_curve.csv` | Cash and token rewards per mission across all types |
| `progression_simulation.csv` | Three player scenario simulations with block tracking |
