# Enemy Health Clamp — Implementation Notes

**Date:** 2026-05-27
**Phase:** 3 — Implementation
**Builds on:** `Enemy_Stats_Scaling_Audit.md`, `Enemy_Health_Clamp_Audit_And_Plan.md`

---

## Files Changed

| File | Change |
|---|---|
| `Assets/Meta/Balance/Config/BalanceConfig.EnemyModule.cs` | Added `WeaponClassHealthProfile` class, `_weaponHealthProfiles` field, `GetMaxShotsToKill()`, `ShouldApplyHealthClamp()` |
| `Assets/Meta/Balance/Config/BalanceConfig.cs` | Added clamp block after HP calculation in `GetEnemyStats()` |
| `Assets/Meta/Balance/Config/BalanceConfig.asset` | Added `_weaponHealthProfiles` array with 4 entries (Bow, Spear, Shuriken, Boomerang) |

**Unchanged:** `EnemyBalancedStatsApplier.cs`, `BossMaxHealthApplier.cs`, all Contracts/Sniper logic, all gate formulas.

---

## Final Formula

```
// Step 1 — existing progression HP (unchanged)
progressedHp = max(1, round( gateDamage × HpFromGate × diffScalar × typeHpMult × loopDiffMult ))

// Step 2 — Campaign health clamp (new)
if ShouldApplyHealthClamp(weaponClass, effectiveType):
    maxShots    = GetMaxShotsToKill(weaponClass, archetype)
    if maxShots > 0:
        maxAllowedHp = max(1, round( gateDamage × maxShots ))
        hp = min(progressedHp, maxAllowedHp)

// gateDamage already includes the loop GateDamage multiplier,
// so maxAllowedHp scales up in later loops → shots-to-kill stays constant per loop.
```

---

## Final Shots-To-Kill Table

| WeaponClass | Melee max | RangedPassive max | RangedActive max | Campaign | Contracts |
|---|---|---|---|---|---|
| **Bow** | 4 | 3 | 2 | Yes | No |
| **Crossbow** | — | — | — | No (not a Campaign weapon) | No |
| **Spear** | 2 | 1 | 1 | Yes | No |
| **Shuriken** | 5 | 3 | 3 | Yes | No |
| **Boomerang** | 3 | 2 | 1 | Yes | No |

`0` in `GetMaxShotsToKill()` means "no profile found → no clamp applied".

---

## Before / After Examples (loop 0, gateDamage = gate formula result)

### Bow, Melee

| globalIndex | gateDamage | diffScalar | progressedHP | maxAllowedHP (×4) | finalHP | shots (at gate dmg) |
|---|---|---|---|---|---|---|
| 1 | 27 | ~1.01 | 109 | 108 | **108** | 4 |
| 7 | 39 | ~1.50 | 234 | 156 | **156** | 4 |
| 10 | 45 | ~2.00 | 360 | 180 | **180** | 4 |
| 20 | 65 | ~3.97 | 1032 | 260 | **260** | 4 |

Clamp caps shots at **4** regardless of how far campaign progresses.

### Spear, Melee

| globalIndex | gateDamage | progressedHP | maxAllowedHP (×2) | finalHP | shots |
|---|---|---|---|---|---|
| 5 | 45 | 216 | 90 | **90** | 2 |
| 10 | 70 | 560 | 140 | **140** | 2 |
| 20 | 120 | 1906 | 240 | **240** | 2 |

Spear Melee always dies in **≤2** shots for a barely-gated player.

### Shuriken, Melee

| globalIndex | gateDamage | progressedHP | maxAllowedHP (×5) | finalHP | shots |
|---|---|---|---|---|---|
| 5 | 20 | 96 | 100 | **96** | 4.8 → 5 |
| 10 | 30 | 240 | 150 | **150** | 5 |
| 20 | 50 | 794 | 250 | **250** | 5 |

Shuriken Melee caps at **5** shots. Early campaign may be slightly under the cap (natural progression).

### Boomerang, Melee

| globalIndex | gateDamage | progressedHP | maxAllowedHP (×3) | finalHP | shots |
|---|---|---|---|---|---|
| 5 | 27.5 | 132 | 82.5 → 83 | **83** | 3 |
| 10 | 40 | 320 | 120 | **120** | 3 |
| 20 | 65 | 1033 | 195 | **195** | 3 |

Boomerang Melee always **≤3** shots.

### RangedActive comparison (Bow, late campaign index=20)

| | Before clamp | After clamp |
|---|---|---|
| progressedHP | 516 | 516 |
| maxAllowedHP | — | 65 × 2 = 130 |
| finalHP | **516** | **130** |
| shots at gate dmg | ~8 | **2** |

### Loop 2 scaling check (Bow, Melee, index=20)

| | Value |
|---|---|
| gateDamage (with loop GateDamage × 1.55) | 65 × 1.55 = **100.75** |
| progressedHP | 100.75 × 4.0 × (3.97 × 1.5) = ~2402 |
| maxAllowedHP | 100.75 × 4 = **403** |
| Player weapon must deal ≥100.75 to pass gate | ~100 dmg |
| shots | 403 / 100.75 ≈ **4** |

Loop scaling works correctly. Gate pressure increases (player needs stronger weapon) but shots-to-kill stays at 4.

---

## Contracts / Sniper / Boss — Confirmed Unaffected

### Contracts
`ShouldApplyHealthClamp(wc, MissionType.Contracts)` returns `p.ApplyToContracts` which is `false` for all profiles. Clamp block is skipped entirely. Contracts HP formula unchanged.

### Sniper
`effectiveType = MissionType.Sniper`. `ShouldApplyHealthClamp(wc, MissionType.Sniper)` returns `false` (neither Campaign nor Contracts check matches). Sniper HP remains HP=1 as before. One-shot gameplay confirmed intact.

### Boss
`GetEnemyStats()` returns `new EnemyModule.EnemyStats(1, 0)` at line 139 before the clamp block is ever reached. Boss is fully bypassed. `BossMaxHealthApplier` continues to set HP = player.Damage × 10.

### Enemy damage
`_receiver.SetDamage(5)` in `EnemyBalancedStatsApplier` is unchanged. Hardcoded to 5 as before.

---

## Test Checklist

- [ ] **Bow Melee, Campaign index=10:** HP should be `45 × 4 = 180` (clamped from ~360)
- [ ] **Bow Melee, Campaign index=20:** HP should be `65 × 4 = 260` (clamped from ~1032)
- [ ] **Spear Melee, Campaign index=10:** HP should be `70 × 2 = 140` (clamped from ~560)
- [ ] **Spear Melee, Campaign index=20:** HP should be `120 × 2 = 240` (clamped from ~1906)
- [ ] **Shuriken Melee, Campaign index=10:** HP should be `30 × 5 = 150` (clamped from ~240)
- [ ] **Boomerang Melee, Campaign index=10:** HP should be `40 × 3 = 120` (clamped from ~320)
- [ ] **Bow RangedActive, Campaign index=20:** HP should be `65 × 2 = 130` (clamped from ~516)
- [ ] **Spear RangedPassive, Campaign index=10:** HP should be `70 × 1 = 70` (clamped from ~420)
- [ ] **Contracts (any weapon):** HP should be unchanged from pre-clamp formula
- [ ] **Sniper:** HP = 1, one-shot confirmed
- [ ] **Boss:** HP = 1 at formula exit, BossMaxHealthApplier overrides to player.Damage × 10
- [ ] **Campaign index=1 (early), Bow Melee:** HP should be `27 × 4 = 108`, not 109 (small clamp)
- [ ] **Campaign index=0 (warmup):** gateDamage ≈ 0.25, maxAllowedHp = round(0.25 × 4) = 1 → HP stays 1 (AdditionalCurve effect, not broken by clamp)
- [ ] **No profile for Crossbow in Campaign:** `ShouldApplyHealthClamp(Crossbow, Campaign)` returns false — no error
- [ ] **Project compiles** without errors

---

## Manual Unity Inspector Steps

After opening the project in Unity Editor:

1. Open `Assets/Meta/Balance/Config/BalanceConfig.asset` in the Inspector.
2. Expand `_enemies` → scroll to **Campaign Health Clamp** header.
3. Confirm `_weaponHealthProfiles` shows 4 entries: Bow, Spear, Shuriken, Boomerang.
4. Verify values match the table above.
5. All `ApplyToCampaign = true`, all `ApplyToContracts = false`.

If the array appears empty (Unity didn't deserialize from YAML):
- The field name `_weaponHealthProfiles` in the asset must match exactly the C# field name. Verify spelling.
- If needed, manually add the 4 entries via the Inspector `+` button and set values from the table above.

---

## Remaining Risks

### R1 — Enemy damage still hardcoded to 5
Enemy damage is not affected by this clamp. All enemies deal exactly 5 damage. Archetype damage multipliers (RangedActive = 2.0×) are computed but not applied. This is a known deferred decision.

### R2 — Overleveled player kills very fast
If a player has e.g. 200 Bow damage and the gate is 65, enemies have HP=260. Player kills in 260/200 = 1.3 → 2 shots. Enemies die very fast. This is intentional — meta investment rewards faster kills. No fix needed.

### R3 — Shuriken RangedActive max=3 may still feel heavy for rapid fire
Shuriken fires very fast. 3 shots at 0.25s reload = 0.75s to kill a RangedActive. This may feel slow relative to the fire rate. If testing reveals it feels grindy, reduce `MaxShotsVsRangedActive` for Shuriken to 2.

### R4 — AdditionalCurve warmup at index=0 interacts with clamp
At globalIndex=0: gateDamage ≈ 0.25 (AdditionalCurve = 0.01 clamp). maxAllowedHp = round(0.25 × maxShots) → rounds to 0 → `Mathf.Max(1, 0) = 1`. All enemies at the warmup mission have HP=1 regardless of archetype. This was true before the clamp too — no regression.

### R5 — No profile for a weapon class used in Campaign
If a Campaign mission requires a weapon class that has no `WeaponClassHealthProfile` entry, `ShouldApplyHealthClamp()` returns `false` — no clamp is applied, and enemies use the uncapped progression formula. This is the safe default. If new weapon classes are added to Campaign in the future, add a profile entry.
