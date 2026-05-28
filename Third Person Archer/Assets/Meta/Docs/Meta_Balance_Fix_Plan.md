# Meta Balance Fix Plan

**Date:** 2026-05-27
**Based on:** `Meta_Balance_Audit.md` + designer answers (DQ1–DQ10)
**Scope:** All of `Assets/Meta` — gate formulas, weapon stats, rewards, economy, loop scaling, Sniper progression

---

## 1. What Is Being Changed and Why

### 1.1 Gate Profiles — Bow and Shuriken (tune higher)

**Problem:** Bow gates start at Base=10 which is too low relative to Bow1's base damage of 35. The first meaningful Bow gate only appears at g12 (zone 2 mid). Zone 1 is entirely friction-free.

**Fix:** Raise Bow gate: `Base=25, Growth=2, Cap=150`

| Mission (g) | Old Gate | New Gate | Bow1 Base | Status |
|-------------|----------|----------|-----------|--------|
| g2          | 15       | 29       | 35        | Pass   |
| g4          | 20       | 33       | 35        | Pass   |
| g6          | 25       | **37**   | 35        | **Block → Teaching gate** |
| g9          | 32.5     | 43       | 35        | Block (need L4) |
| g12         | 40       | 49       | 35        | Block (need L7) |
| g16         | 50       | **57**   | 55 (max)  | **Forces Bow2 purchase** |
| g20         | 60       | 65       | 55 (max)  | Bow2 needed |

**Expected effect:** Player hits their first teaching gate at Zone 1's last Bow mission (g6) instead of not hitting any gate until Zone 2.

---

### 1.2 Gate Profiles — Shuriken (tune higher)

**Problem:** Shuriken gate starts at Base=5, which is lower than Shuriken1's base damage of 25 for the first ~7 missions.

**Fix:** Raise Shuriken gate: `Base=10, Growth=2, Cap=120`

| Mission (g) | Old Gate | New Gate | Shuriken1 Base | Status |
|-------------|----------|----------|----------------|--------|
| g1          | 7        | 12       | 25             | Pass   |
| g3          | 11       | 16       | 25             | Pass   |
| g5          | 15       | 20       | 25             | Pass   |
| g8          | 21       | **26**   | 25             | **Block (need L1)** |
| g11         | 27       | 32       | 25             | Block (need L7) |
| g14         | 33       | **38**   | 35 (max)       | **Forces Shuriken2** |
| g17         | 39       | 44       | 35 (max)       | Shuriken2 needed |
| g21         | 47       | 52       | 35 (max)       | Shuriken2 L6 or Shuriken3 |

Old DamageCap=50 was too restrictive and would have capped gates even in loop 2. New Cap=120 allows loop scaling to function.

---

### 1.3 Gate Profiles — Spear (scale to new weapon stats)

**Problem:** Spear1 base damage (180) was 4.4x the first Spear gate (41). Players never needed upgrades for any Spear mission. Spear gates must be tuned around Spear's higher absolute damage.

**Fix:** Lower Spear1 base damage AND raise Spear gate growth together.

New weapon: `Spear1 base=65, max=110`
New gate: `Base=20, Growth=5, Cap=300`

| Mission (g) | New Gate | Spear1 Status |
|-------------|----------|---------------|
| g7          | 55       | **Pass at base (65)** — first mission free |
| g10         | 70       | Block (need L1 = 70.5) |
| g13         | 85       | Block (need L4 = 87) |
| g18         | 110      | Block (need max L10 = 110) |
| g22         | **130**  | **Forces Spear2 purchase** |

Spear1 max (110) is still 2x Bow1 max (55), maintaining Spear as the highest-damage class. Spear2 new base=110, max=190 handles late Zone 3 and all loops.

**Old DamageCap=120** would have been hit by loop 1 (130*1.25=162.5). New Cap=300 gives room for loop scaling.

---

### 1.4 Gate Profiles — Boomerang (create friction)

**Problem:** Boomerang1 base (80) exceeded both Boomerang gates in the game (42, 50). Zero friction ever.

**Fix:** Lower Boomerang1 base AND raise gate growth.

New weapon: `Boomerang1 base=55, max=80`
New gate: `Base=15, Growth=2.5, Cap=150`

| Mission (g) | New Gate | Boomerang1 Status |
|-------------|----------|-------------------|
| g15         | 52.5     | **Pass at base (55)** — first mission free |
| g19         | 62.5     | **Block (need L3 = 64)** |

With only 2 Boomerang Campaign missions, friction is modest but meaningful. Player must upgrade ~3 levels before the second Boomerang mission.

---

### 1.5 Spear Weapon Stats — Scale Down Entire Tree

**Problem (DQ1):** Spear should remain highest damage class but starter must not exceed Spear gates.

**Fix:** Reduce all Spear weapon base/max damage proportionally.

| Weapon  | Old Base | New Base | Old Max | New Max |
|---------|----------|----------|---------|---------|
| Spear1  | 180      | **65**   | 260     | **110** |
| Spear2  | 260      | **110**  | 380     | **190** |
| Spear3  | 380      | **190**  | 560     | **300** |

Spear2 unlocks at campaign level 23 (when player first needs it for g22 Zone 3 last Spear mission). Spear3 unlocks at level 35 (loop progression).

Upgrade costs for Spear1 raised slightly to reflect higher-tier weapon:
- Old: startCash=50, maxCash=90, startTok=1, maxTok=2
- New: startCash=70, maxCash=130, startTok=2, maxTok=5

---

### 1.6 Boomerang Weapon Stats — Scale Down

| Weapon     | Old Base | New Base | Old Max | New Max |
|------------|----------|----------|---------|---------|
| Boomerang1 | 80       | **55**   | 100     | **80**  |
| Boomerang2 | 100      | **80**   | 150     | **130** |

Higher-tier Boomerangs adjusted to maintain smooth upgrade ladder. Purchase costs and other stats unchanged.

---

### 1.7 Campaign Rewards — Reduce Token Scaling (DQ8)

**Problem:** Campaign token rewards scaled as `2 + 2*N` per mission. By Zone 1 end (g6), a player had earned 32 Bow tokens — enough to max Bow1 twice over. Contracts became irrelevant for token income.

**Fix:**
- Old: `Base=2, PerStep=2` → New: `Base=1, PerStep=0` (flat 1 token per mission)
- Old cash: `Base=100, PerStep=10` → New: `Base=70, PerStep=7`

**Effect:**
- Zone 1 Bow missions (g0,g2,g4,g6): earn 4 Bow tokens
- Zone 1+2 Bow missions total: 6 Bow tokens
- Bow1 max upgrade: 14.5 tokens total → player must do Contracts to max it
- Zone 1 provides just enough for the Zone 1 teaching gate (1 token for L1) and Zone 2 mid-gates (L4 = 4 tokens), with 1-2 Contracts runs needed for later upgrades

---

### 1.8 Contracts Rewards — Reduce Flooding (DQ6)

**Problem:** Contracts gave `5 + 5*N` AllTokens per run. First run alone gave 5 tokens to every class simultaneously (25 total). Very quickly made upgrade costs trivial.

**Fix:**
- AllTokens: `Base=3, PerStep=1` (was 5+5)
- Cash: `Base=40, PerStep=15` (was 50+50)

**Effect:** First Contracts run gives 3 tokens to each class. 3 Contracts runs = 9-15 tokens per class, which helps meaningfully but doesn't instantly max anything.

---

### 1.9 Sniper Rewards — Adjust to Be Better Than Contracts for Cash (DQ2)

- Cash: `Base=150, PerStep=50` (was 200+100)
- CrossbowToken: `Base=2, PerStep=1` (was 3+2)

Sniper remains the best single-run cash mode (first run = 150 vs Contracts 40). CrossbowToken income funds the new Sniper access gate.

---

### 1.10 Boss Rewards — Reduce AllTokens Flood

**Problem:** First Boss gave 10 tokens to every class (50 total), which immediately erased upgrade pressure for all weapon classes simultaneously.

**Fix:**
- Cash: `Base=300, PerStep=100` (was 400+100)
- AllTokens: `Base=3, PerStep=2` (was 10+10)

First Boss now gives 3 tokens to each class (15 total). This is still a clear reward spike but does not single-handedly fund weapon tree progression.

---

### 1.11 Starting Tokens — Reduce to 1 Per Class

**Problem:** Starting BoomerangToken=10 meant a player started with 69% of the tokens needed to max Boomerang1 before playing a single mission. SpearToken=6 was similarly inflated.

**Fix:** All weapon token starting amounts set to 1.

| Currency      | Old Start | New Start |
|---------------|-----------|-----------|
| Cash          | 100       | 100 (unchanged) |
| BowToken      | 3         | **1** |
| CrossbowToken | 3         | **1** |
| SpearToken    | 6         | **1** |
| ShurikenToken | 3         | **1** |
| BoomerangToken| 10        | **1** |

---

### 1.12 Loop GateDamage Multipliers — Implement Escalation (DQ4)

**Problem:** All 3 loops had `GateDamage=1.0`. Replaying added zero gate pressure.

**Fix:**

| Loop | Old GateDamage | New GateDamage | Old CampaignDifficulty | New CampaignDifficulty | New Reward Mult (Cash) |
|------|----------------|----------------|------------------------|------------------------|------------------------|
| 0    | 1.0            | 1.0            | 1.0                    | 1.0                    | 1.0 |
| 1    | 1.0            | **1.25**       | 1.2                    | **1.3**                | 1.1 (all modes) |
| 2    | 1.0            | **1.55**       | 1.35                   | **1.5**                | 1.2–1.25 |

**Loop 1 effect:** Spear gate at g22 becomes 130*1.25=162.5 (needs Spear2 L7).
**Loop 2 effect:** Spear gate at g22 becomes 130*1.55=201.5 (needs Spear3 purchase + minor upgrade).

DamageCap raised on all classes (especially Spear: 120→300) to ensure caps never interfere with loop scaling.

---

### 1.13 Remove GateComfortFactor (DQ3)

**Problem:** `_gateComfortFactor = 1.2f` was serialized in GateModule and exposed via `GetGateComfortFactor()` on BalanceConfig, but was **never used in any runtime gate calculation**. Misleading dead code.

**Fix:** Remove the field, property, and public accessor entirely from all CS files. The orphaned value in the .asset file will be ignored by Unity.

---

### 1.14 Remove EconomyPacingModule (DQ9)

**Problem:** The `EconomyPacingModule` held design targets (`TargetUpgradesPerCampaignMission`, etc.) and was exposed via `BalanceConfig.EconomyPacing`. It had no callers found in the runtime codebase — purely editor planning data that was never enforced.

**Fix:** Remove the field from `BalanceConfig.cs` and replace the class file with a stub to avoid meta file issues. The orphaned `_economyPacing` section in the .asset will be ignored by Unity.

---

### 1.15 Sniper Access Gate — New System (DQ2)

**Design intent:** Crossbow has a complete upgrade tree (7 weapons) and token economy but is intentionally excluded from Campaign missions. Sniper mode should be the vehicle for Crossbow progression. A Crossbow damage gate for Sniper access makes Crossbow investment meaningful without changing one-shot sniper gameplay.

**Implementation:**

A new `SniperAccessProfile` is added to `BalanceConfig.GateModule`:
- `BaseDamage = 15` (Crossbow1 base = 22, passes first 3 Snipers at L0)
- `MissionGrowth = 3` (gate increases by 3 per Sniper completion index)
- `DamageCap = 0` (uncapped, but Crossbow1 max=37 keeps it finite for early game)

Gate per Sniper index (SniperCompletedIndex):
| Index | Required Xbow Dmg | Crossbow1 Status |
|-------|------------------|-----------------|
| 0     | 15               | Pass (base=22)  |
| 1     | 18               | Pass            |
| 2     | 21               | Pass (barely)   |
| 3     | 24               | Block (need L2=25) |
| 5     | 30               | Block (need L6=31) |
| 7     | 36               | Block (need L10=37) |
| 8     | 39               | Block (need Crossbow2) |

Sniper token rewards (CrossbowToken, `Base=2, PerStep=1`) self-fund this progression. First 3 Snipers earn 9 Crossbow tokens (L0-L2 costs ~3 tokens), enough to upgrade ahead of the gate.

**New gate check:**
- `IMissionGateService.CheckSniperGate(MissionContext ctx, int sniperCompletedIndex)` returns `MissionGateResult`
- `MissionAvailabilityService` checks this gate for `MissionType.Sniper`, returns `SniperDamageTooLow` if blocked
- Enemy HP is unchanged — `SniperHpFromGate=0.01` means HP is always ~1 regardless of gate value, preserving one-shot feel

---

## 2. Risk Assessment

| Change | Risk | Mitigation |
|--------|------|------------|
| Spear base 180→65 | HIGH — large stat change; existing saves have Spear-equipped players | LoadoutSnapshotUpdater recalculates on init; gate service uses live computed stats |
| Boomerang base 80→55 | MEDIUM — affects existing tester save state | Same — auto-recalculates |
| Sniper gate new system | MEDIUM — new code path, new enum value, new constructor parameter | Compile-tested; all paths have fallback returns |
| GateComfortFactor removal | LOW — only used in generator helpers, not gameplay | Verified: no callers in Meta codebase |
| EconomyPacingModule removal | LOW — no runtime callers found | Stub file prevents .meta issues; asset orphan data is silently ignored |
| Starting tokens reduced | MEDIUM — affects existing tester saves only if starting amounts are applied fresh | Starting amounts only apply on new game / first login |
| Loop multiplier changes | LOW — doesn't affect loop 0 (first run); loop index is persistent |  |

---

## 3. Before/After Progression Summary

### Campaign-Only Minimal Player

| Zone | Before (Old) | After (New) |
|------|-------------|-------------|
| Zone 1 | 0 blocks, 0 upgrades needed | **1 block at g6 (teaching gate, Bow L1)** |
| Zone 2 | 3 blocks (Shuriken/Bow), all trivially cheap | **4-5 blocks** (Shuriken L1@g8, Bow L4@g9, Shuriken L7@g11, Bow L7@g12, Shuriken→Shuriken2@g14) |
| Zone 3 | 3 blocks, Spear/Boom never blocked | **7-8 blocks** (Bow2 needed, Shuriken2 needed, Spear L2+ needed, Boom L3 needed) |
| Spear missions | NEVER blocked (180>86 max gate) | **Blocked from g10 onward** |
| Boomerang missions | NEVER blocked (80>50 max gate) | **Blocked at g19** |

### Economic Flow

| | Before | After |
|--|--------|-------|
| Campaign token income | 32 BowTok after Zone 1 | **4 BowTok** after Zone 1 |
| Contracts relevance | Near zero — Campaign flooded tokens | **Primary token source** |
| Sniper relevance | Good cash but Crossbow meaningless | **Good cash + Crossbow progression** |
| Loop 2 gate pressure | Identical to loop 0 | **55% harder gates** |

---

## 4. Files Changed

### Code Files
- `BalanceConfig.GateModule.cs` — remove GateComfortFactor, add SniperAccessProfile
- `BalanceConfig.cs` — remove EconomyPacing field/property, remove GetGateComfortFactor, add GetRequiredCrossbowDamageForSniper
- `BalanceConfig.EconomyPacingModule.cs` — replaced with stub partial class
- `MissionAvailability.cs` — add `SniperDamageTooLow` enum value
- `IMissionGateService.cs` — add `CheckSniperGate` method
- `MissionGateService.cs` — implement `CheckSniperGate`
- `MissionAvailabilityService.cs` — inject MetaLoopProgressData, add Sniper gate check
- `MainMenuRuntime.cs` — pass _loopProgressData to MissionAvailabilityService constructor

### Asset Files
- `BalanceConfig.asset` — gate profiles, rewards, loop multipliers
- `Spear WeaponDef 1.asset` — base 180→65, max 260→110
- `Spear WeaponDef 2.asset` — base 260→110, max 380→190
- `Spear WeaponDef 3.asset` — base 380→190, max 560→300
- `Boomerang WeaponDef 1.asset` — base 80→55, max 100→80
- `Boomerang WeaponDef 2.asset` — base 100→80, max 150→130
- `CurrencyStartingBalanceConfig.asset` — all tokens to 1

### Documentation
- `Meta_Balance_Fix_Plan.md` (this file)
- `Meta_Balance_After_Fix.md` — post-implementation analysis

---

## 5. Manual Unity Inspector Steps Required

1. **Open Unity** — let Unity recompile CS files. Verify no compile errors.
2. **Verify LoadoutSnapshot** — Open `Assets/Meta/Weapons/LoadoutSnapshot.asset`. The damage values should auto-refresh on next Play (via `LoadoutSnapshotUpdater.RefreshAll()`). If not refreshed, enter Play mode once with the main menu scene to trigger refresh.
3. **Check BalanceConfig Inspector** — Verify new gate profiles show correct values. `SniperAccessProfile` should appear as a new sub-section.
4. **Check Weapon Inspector** — Verify Spear1 damage shows base=65, max=110 in Inspector.
5. **Test play gate popup** — Enter Play mode and verify that Bow at Zone 1 g6 shows the damage gate popup (current=35, required=37).
6. **Test Sniper gate** — Play 3 Snipers, verify 4th shows blocked if Crossbow1 is at L0. Verify Crossbow L2 unblocks it.
7. **Save the scene** — Unity may not auto-save .asset edits. Save all modified assets via File > Save.
