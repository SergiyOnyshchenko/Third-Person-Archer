# Enemy Stats Scaling Audit

**Date:** 2026-05-27
**Phase:** 1 — Audit Only (no code or balance changes)
**Auditor:** Claude Code

---

## 1. Overview

Enemy HP and damage are assigned at runtime in the gameplay scene by `EnemyBalancedStatsApplier`, a MonoBehaviour that reads from `GameplayRuntime.Instance` (singleton) and calls `BalanceConfig.GetEnemyStats(...)`.

The formula is **gate-driven**: enemy HP is not set to an absolute number. Instead it is derived from the **required weapon damage for the current mission** (the same gate damage value used by meta access gates). This ties enemy difficulty directly to expected player power at that mission.

The formula is:
```
HP     = round( gateDamage × HpFromGate    × difficultyScalar × typeHpMultiplier    × loopDifficultyMultiplier )
Damage = round( gateDamage × DamageFromGate × difficultyScalar × typeDamageMultiplier × loopDifficultyMultiplier )
```

**Critical observation:** Enemy damage is currently **hardcoded to 5** in `EnemyBalancedStatsApplier`. The computed `stats.Damage` from the formula is calculated but never applied — it is commented out. Only `SetHealth(stats.MaxHp)` uses the formula.

---

## 2. Entry Points

### Primary: `EnemyBalancedStatsApplier`
**File:** `Assets/Scripts/Enemy/EnemyBalancedStatsApplier.cs`

- MonoBehaviour, `[DisallowMultipleComponent]`
- Serialized field: `EnemyArchetypeTag _archetypeTag` (can be auto-resolved from sibling component)
- Resolves `IEnemyStatsReceiver` via `GetComponentInChildren<IEnemyStatsReceiver>(true)`
- **`Awake()`:** Resolves references only.
- **`Start()`:** Calls `_receiver.SetDamage(5)` (hardcoded), then `Apply()`.
- **`Apply()`:** Reads `GameplayRuntime.Instance`, calls `balance.GetEnemyStats(...)`, calls `_receiver.SetHealth(stats.MaxHp)` and `_receiver.SetDamage(5)` (hardcoded again). `stats.Damage` is computed but commented out.
- `Apply()` is `public` but is not called by any other class — only from `Start()`.

### Secondary (legacy): `EnemyStatsApplier`
**File:** `Assets/Scripts/Enemy/EnemyStatsApplier.cs`

- MonoBehaviour, `[DisallowMultipleComponent]`
- Reads from a **static `EnemyStatsConfig` ScriptableObject** — not formula-driven
- Also applies in `Start()` via `receiver.SetHealth(config.Health)` and `receiver.SetDamage(config.Damage)`
- This is the **old system** that predates `EnemyBalancedStatsApplier`

### Boss: `BossMaxHealthApplier`
**File:** `Assets/Scripts/Enemy/BossMaxHealthApplier.cs`

- MonoBehaviour, applies boss HP with a 0.25s `DOVirtual.DelayedCall`
- Does NOT use `EnemyBalancedStatsApplier` or `BalanceConfig.GetEnemyStats`
- Sets `MaxHealth = player.Damage.Value × 10` (player's current weapon damage × 10)
- Completely independent of the scaling formula

### Receiver: `EnemyStatsReceiverSystem`
**File:** `Assets/Scripts/Enemy/EnemyStatsReceiverSystem.cs`

- `Actor.System` implementing `IEnemyStatsReceiver` and `IActorIniter`
- Receives and applies stats to `MaxHealth` and `Damage` Actor Properties
- Resolved via `ActorController` in `InitActor()`

---

## 3. Data Sources

| Source | Role |
|---|---|
| `EnemyArchetypeTag` | Tags an enemy with `EnemyArchetype` enum (Melee/RangedPassive/RangedActive) |
| `BalanceConfig.EnemyModule` | All difficulty scalars, HP/damage from gate factors, archetype multipliers |
| `BalanceConfig.GateModule` | Per-weapon-class gate profiles (BaseDamage, MissionGrowth, DamageCap, AdditionalCurve) |
| `BalanceConfig.LoopModule` | Loop multipliers for difficulty and gate damage |
| `MissionContext` | Selected type, balance mode, globalCampaignIndex, requiredWeaponClass, loopIndex |
| `MetaLoopProgressData` | `ContractsCompletedIndex`, `SniperCompletedIndex`, `CurrentLoopIndex` |
| `GameplayRuntime` | Singleton providing Balance, Context, ContractsCompletedIndex, SniperCompletedIndex |
| `EnemyStatsConfig` | Static HP/Damage asset (old system, `EnemyStatsApplier` only) |

**Player weapon/loadout damage is NOT used** in the standard enemy scaling formula.
Exception: Boss (`BossMaxHealthApplier`) scales directly off `player.Damage.Value`.

---

## 4. EnemyArchetype Multipliers

Defined in `BalanceConfig.EnemyModule._enemyTypeProfiles` (type: `EnemyTypeProfile[]`).
Values from `BalanceConfig.asset`:

| Archetype | Enum Value | HpMultiplier | DamageMultiplier | Notes |
|---|---|---|---|---|
| `Melee` | 1 | **1.00** | 1.30 | Highest HP — tank role |
| `RangedPassive` | 2 | **0.75** | 0.75 | Medium HP, low damage |
| `RangedActive` | 3 | **0.50** | 2.00 | Lowest HP, glass cannon |

HP ordering: Melee > RangedPassive > RangedActive. This matches the intended design.

DamageMultiplier is **currently irrelevant** — enemy damage is hardcoded to 5 regardless.

If `GetEnemyProfile(archetype)` returns `null` (e.g., archetype = `None`), both multipliers default to 1.0.

---

## 5. Mission Type Scaling

### Campaign

**Formula:**
```
gateDamage   = (BaseDamage + MissionGrowth × globalIndex) × AdditionalCurve(globalIndex)
               capped by DamageCap, multiplied by loopGateDamageMultiplier
diffScalar   = campaignDifficultyCurve(globalIndex) × loop.CampaignDifficulty
HP = round( gateDamage × 4.0 × diffScalar × typeHpMultiplier )
```

**Indexes used:** `ctx.GlobalCampaignIndex` (0-based, spans all zones: Zone 1 = 0-6, Zone 2 = 7-14, Zone 3 = 15-22).

**Campaign difficulty curve** (`_campaignDifficultyCurve`, Animation Curve, ClampForever):
- index 0  → ~1.00
- index ~2.75 → ~1.064
- index ~20 → ~3.975
- index >20 → clamped at ~3.975

This is a smooth curve, slow at the start, accelerating in the mid-game.

**Loop multipliers for Campaign:**
| Loop | CampaignDifficulty | GateDamage |
|---|---|---|
| 0 | 1.00 | 1.00 |
| 1 | 1.30 | 1.25 |
| 2 | 1.50 | 1.55 |

**AdditionalCurve note:** All weapon gate profiles share the same AdditionalCurve shape: 0 at index 0, 1 at index 1, clamped at 1 thereafter. This means at `globalIndex=0` (very first campaign mission), the curve evaluates to 0 → clamped to 0.01 → effective gate damage is only 1% of the normal value. This makes the first campaign mission have nearly negligible enemy HP (warmup mission).

**Bow gate profile** (WeaponClass=0, BaseDamage=25, Growth=2, Cap=150):
- index=0: gateDamage = 25 × 0.01 = 0.25
- index=1: 27
- index=5: 35
- index=10: 45
- index=20: 65

---

### Contracts

**Formula:**
```
gateDamage      = same as Campaign, using globalCampaignIndex of the randomly selected campaign source mission
contractsDiff   = 0.5 + 0.05 × contractsCompletedIndex
HP = round( gateDamage × 1.0 × contractsDiff × typeHpMultiplier )
```

**Source:** `ContractPoolService.PickRandomEligibleCampaignMission()` picks a random already-completed Campaign mission. The `globalCampaignIndex` of that selected mission becomes the `gateIndex` for the gate damage calculation. There is no override for this — contracts are anchored to real campaign mission levels.

**Auto-leveling:** The difficulty scalar grows linearly with `ContractsCompletedIndex` (+0.05 per completion, base 0.5). This is **true auto-leveling** — it is not bounded.

**Weapon class difficulty multiplier** (`_weaponClassDifficultyMultipliers`): all values are currently 1.0. No per-class tweak active.

**Comparison to Campaign at same globalIndex, loop 0:**
- Campaign: `gateDamage × 4.0 × diffCurve(index)` — at index=7: ~`39 × 4.0 × 1.65 ≈ 257`
- Contracts (N=0): `39 × 1.0 × 0.5 = 20`
- Contracts (N=20): `39 × 1.0 × 1.5 = 59`
- Contracts (N=60): `39 × 1.0 × 3.5 = 137`

Contracts are much easier than Campaign for the same mission level, unless `ContractsCompletedIndex` grows very large.

---

### Sniper

**Formula:**
```
gateDamage      = Crossbow gate damage at the resolved gateIndex (see note below)
sniperDiff      = 0.1 + 0.05 × sniperCompletedIndex
HP = round( gateDamage × 0.01 × sniperDiff × typeHpMultiplier )
```

**SniperHpFromGate = 0.01** combined with **sniperDifficulty base = 0.1** means the effective multiplier starts at `0.01 × 0.1 = 0.001`. For any reasonable gateDamage (even up to 50), the result is:
- HP = round(50 × 0.001 × 1.0) = round(0.05) = 0 → clamped to **1**

Even after many completions (N=50 → sniperDiff=2.6), the result stays 1:
- HP = round(50 × 0.01 × 2.6 × 1.0) = round(1.3) = 1

**Sniper enemies effectively always have HP=1**, achieving one-shot feel through the formula design, not a special code path. No separate Sniper-specific code branch beyond the formula parameters.

**gateIndex for Sniper:** There is a path discrepancy:
- Via `MissionContextService` (non-LaunchRequest): `GlobalCampaignIndex=-1`, `CompanyLevel=-1` → `gateIndex=0` (very low gate damage)
- Via `LaunchRequest` (actual gameplay launch): `CompanyLevel = progress.GetCompanyLevel()` → `gateIndex = companyLevel - 1` (scales with player progress)

Since HP rounds to 1 regardless, this discrepancy has no gameplay impact currently. But it would matter if SniperHpFromGate were increased.

---

### Boss

**In `BalanceConfig.GetEnemyStats()`:**
```csharp
if (ctx.SelectedType == MissionType.Boss)
    return new EnemyModule.EnemyStats(1, 0);
```
Boss immediately returns `HP=1, Damage=0` — completely bypasses the scaling formula.

**Boss HP is set by `BossMaxHealthApplier`:**
- Runs 0.25s after Start (DOTween delay)
- Finds the `Player` in scene via `FindObjectOfType<Player>()`
- Sets `Boss.MaxHealth = player.Damage.Value × 10`
- This means Boss HP tracks player weapon damage directly — it is fully player-power-scaled

Boss stats do NOT depend on `BalanceConfig.EnemyModule`, `EnemyArchetype` multipliers, globalCampaignIndex, loop difficulty, or any BalanceConfig formula. The meta Boss Crossbow Gate is an **access gate only** and has no connection to this runtime HP setting.

---

## 6. Actual Formulas

### Gate Damage (base for all formulas)
```
gateDamage = (BaseDamage[wc] + MissionGrowth[wc] × gateIndex)
             × max(0.01, AdditionalCurve[wc].Evaluate(gateIndex))
             × loopMultipliers.GateDamage
```
Capped at `DamageCap[wc]` if DamageCap > 0.

### Gate profiles by WeaponClass (loop 0, ignoring AdditionalCurve beyond index 0)
| WeaponClass | BaseDamage | Growth | Cap |
|---|---|---|---|
| Bow (0) | 25 | 2.0 | 150 |
| Crossbow (1) | 8 | 2.0 | 120 |
| Spear (2) | 20 | 5.0 | 300 |
| Shuriken (3) | 10 | 2.0 | 120 |
| Boomerang (4) | 15 | 2.5 | 150 |

### Difficulty Scalars
```
Campaign:  diffCurve(globalIndex) × loop.CampaignDifficulty
Contracts: (0.5 + 0.05 × contractsN) × loop.ContractDifficulty
Sniper:    (0.1 + 0.05 × sniperN)    × loop.SniperDifficulty
Boss:      1.0 (excluded)
```

### HP/Damage from Gate Factors
| MissionType | HpFromGate | DamageFromGate |
|---|---|---|
| Campaign | 4.00 | 0.15 |
| Contracts | 1.00 | 0.12 |
| Sniper | 0.01 | 0.06 |

### Final Formulas
```
HP     = max(1, round( gateDamage × HpFromGate[type] × diffScalar × typeHpMult × mpHpMult ))
Damage = max(0, round( gateDamage × DmgFromGate[type] × diffScalar × typeDmgMult × mpDmgMult ))
       → currently overridden to hardcoded 5 in EnemyBalancedStatsApplier
```

---

## 7. Example Calculations

Campaign difficulty curve values used below are approximate interpolations from the 3 known anchor points:

| globalIndex | diffCurve approx |
|---|---|
| 0 | 1.00 |
| 1 | 1.01 |
| 5 | 1.20 |
| 7 | 1.50 |
| 10 | 2.00 |
| 15 | 2.80 |
| 20 | 3.97 |
| 22 | 3.97 (clamped) |

---

### Campaign — Early (globalIndex=1, Bow, Melee, loop=0)
```
gateDamage = (25 + 2×1) × 1.0 = 27
diffScalar = ~1.01 × 1.0 = 1.01
HP   = round(27 × 4.0 × 1.01 × 1.0) = round(109) = 109
Dmg  = hardcoded 5
```

### Campaign — Mid (globalIndex=7, Bow, Melee, loop=0)
```
gateDamage = (25 + 2×7) × 1.0 = 39
diffScalar = ~1.50 × 1.0 = 1.50
HP   = round(39 × 4.0 × 1.50 × 1.0) = round(234) = 234
Dmg  = hardcoded 5
```

### Campaign — Late (globalIndex=20, Bow, Melee, loop=0)
```
gateDamage = (25 + 2×20) × 1.0 = 65
diffScalar = ~3.97 × 1.0 = 3.97
HP   = round(65 × 4.0 × 3.97 × 1.0) = round(1032) = 1032
Dmg  = hardcoded 5
```

### Campaign — Late (globalIndex=20, Bow, RangedActive, loop=0)
```
gateDamage = 65
diffScalar = 3.97
HP   = round(65 × 4.0 × 3.97 × 0.5) = round(516) = 516
Dmg  = hardcoded 5
```

### Campaign — Late, Loop 2 (globalIndex=20, Bow, Melee)
```
gateDamage = 65 × 1.55 = 100.75
diffScalar = 3.97 × 1.50 = 5.96
HP   = round(100.75 × 4.0 × 5.96 × 1.0) = round(2402) = 2402
Dmg  = hardcoded 5
```

### Contracts — Early (contractsN=0, source campaign index=5, Bow, Melee, loop=0)
```
gateDamage = (25 + 2×5) × 1.0 = 35
contractsDiff = 0.5 + 0.05×0 = 0.50
HP   = round(35 × 1.0 × 0.50 × 1.0) = round(17.5) = 18
Dmg  = hardcoded 5
```

### Contracts — After 20 completions (contractsN=20, source index=10, Bow, Melee, loop=0)
```
gateDamage = (25 + 2×10) × 1.0 = 45
contractsDiff = 0.5 + 0.05×20 = 1.50
HP   = round(45 × 1.0 × 1.50 × 1.0) = round(67.5) = 68
Dmg  = hardcoded 5
```

### Sniper (any sniperN, Crossbow, Melee, loop=0)
```
gateDamage = any reasonable value, e.g. 26 (Crossbow at index=9)
sniperDiff = 0.1 + 0.05×N (e.g. N=0: 0.1, N=50: 2.6)
HP (N=0)  = round(26 × 0.01 × 0.1 × 1.0) = round(0.026) = 0 → clamped to 1
HP (N=50) = round(26 × 0.01 × 2.6 × 1.0) = round(0.68)  = 1
HP (N=100)= round(26 × 0.01 × 5.1 × 1.0) = round(1.33)  = 1
Dmg = hardcoded 5
```
All Sniper enemies have HP=1 under current configuration.

### Boss (special case bypass)
```
GetEnemyStats() → returns EnemyStats(HP=1, Dmg=0) immediately
EnemyBalancedStatsApplier sets MaxHealth=1, Damage=5

BossMaxHealthApplier (0.25s delay):
  HP = player.Damage.Value × 10
  e.g. player has Bow at 50 damage → Boss HP = 500
```

---

## 8. Runtime Flow

```
[Meta UI] Player selects mission
    → MissionStartService.TryStartSelected()
    → Gate check passes
    → MissionLaunchRequest.Set(mode, globalIndex, weaponClass, loopIndex, ...)
    → Scene loaded (gameplay scene)

[Gameplay scene Awake]
    → GameplayRuntime.Awake()
        → BuildContext() reads MissionLaunchRequest → creates MissionContext
        → Context.SelectedType, BalanceMode, GlobalCampaignIndex, RequiredWeaponClass set

[Enemy GameObject Start — order not guaranteed]
    → EnemyBalancedStatsApplier.Start()
        → _receiver.SetDamage(5)   ← hardcoded, unconditional
        → Apply()
            → GameplayRuntime.Instance  (must exist)
            → rt.Context, rt.Balance    (must be valid)
            → balance.GetEnemyStats(ctx, archetype, contractsN, sniperN, isMP)
                → early-exit if Boss → returns (1, 0)
                → resolves effectiveType = ctx.BalanceMode
                → resolves gateIndex from ctx.GlobalCampaignIndex or ctx.CompanyLevel
                → gateDamage = GetRequiredDamageForCampaign(requiredWeaponClass, gateIndex, loop)
                → diffScalar = GetDifficultyScalar(effectiveType, progressionIndex, weaponClass, loop)
                → typeHp, typeDmg from archetype profile
                → HP = round(gateDamage × HpFactor × diff × typeHp)
                → Dmg = round(...) [NOT USED]
            → _receiver.SetHealth(stats.MaxHp)
            → _receiver.SetDamage(5)   ← hardcoded again

[Boss only, 0.25s later]
    → BossMaxHealthApplier.ApplyMaxHealth()
        → player.Damage.Value × 10 → boss.MaxHealth.SetValue(health)
        → overrides any previous MaxHealth setting
```

---

## 9. Potential Problems / Questions

### P1 — Enemy Damage Hardcoded to 5 (Critical)
**Evidence:** `EnemyBalancedStatsApplier.cs` lines 20 and 49: `_receiver.SetDamage(5)`. Line 50: `//_receiver.SetDamage(stats.Damage)` is commented out. Stats.Damage is computed (including archetype damage multiplier, gate scaling, difficulty) but never applied.
**Affected files:** `EnemyBalancedStatsApplier.cs`
**Severity: Critical**
All enemies deal exactly 5 damage regardless of archetype, mission level, or loop. The damage formula, DamageMultipliers, and ContractDamageFromGate / SniperDamageFromGate factors are all dead code. RangedActive's glass-cannon DamageMultiplier=2.0 has no effect. Enemy difficulty from damage side is completely flat.
**Question:** Was damage intentionally hardcoded during development as a placeholder? Is enemy damage important for player challenge, or is it purely cosmetic at this stage?

---

### P2 — Two Competing Appliers Can Coexist (Major)
**Evidence:** `EnemyStatsApplier` and `EnemyBalancedStatsApplier` are both `[DisallowMultipleComponent]` individually but there is no guard against having both on the same enemy prefab. Both apply stats in `Start()`. Whichever runs last wins. If `EnemyStatsApplier` is present on a prefab, it may apply static config values that override or get overridden by the formula.
**Affected files:** `EnemyStatsApplier.cs`, `EnemyBalancedStatsApplier.cs`
**Severity: Major**
Need to audit each enemy prefab in the project to confirm which applier they use. If any enemy still has both, stats will be non-deterministic (depends on MonoBehaviour Start() execution order).

---

### P3 — Contracts Difficulty Is Unbounded (Major)
**Evidence:** `_contractsDifficulty.Base=0.5, PerStep=0.05` — no cap defined in `LinearProgression` itself. After 70 completions, `contractsDiff = 0.5 + 3.5 = 4.0`, which matches or exceeds late-game Campaign difficulty scalar at the same gate level. The gate damage for Contracts is also anchored to the source campaign mission's level, which increases as the player progresses.
**Affected files:** `BalanceConfig.EnemyModule.cs`, `BalanceConfig.asset`
**Severity: Major**
Contracts should feel like grind mode (easier than Campaign). At very high completions they could become as hard as or harder than Campaign. There is no ceiling.
**Question:** Should contracts difficulty be capped? At what point should contracts stop getting harder?

---

### P4 — Sniper gateIndex Discrepancy Between Launch Paths (Minor)
**Evidence:** When Sniper is launched via `MissionContextService` (non-LaunchRequest), `CompanyLevel=-1` → `gateIndex=0`. When launched via `MissionLaunchRequest`, `companyLevel = progress.GetCompanyLevel()` → `gateIndex = companyLevel - 1`. This means the base `gateDamage` used in the Sniper HP formula differs between paths.
**Affected files:** `BalanceConfig.cs`, `GameplayRuntime.cs`
**Severity: Minor**
Currently irrelevant because `SniperHpFromGate=0.01` ensures HP always rounds to 1. Would become a real issue if SniperHpFromGate were ever raised.

---

### P5 — AdditionalCurve Makes First Campaign Mission (index=0) Trivially Easy (Minor)
**Evidence:** All `WeaponGateProfile.AdditionalCurve` assets have keyframes at (0,0) and (1,1) with ClampForever mode. At `globalIndex=0`, the curve evaluates to 0 → clamped to 0.01 → gate damage reduced to 1% of normal. For Bow: `25 × 0.01 = 0.25 gateDamage` → Melee HP = round(0.25 × 4.0 × 1.0 × 1.0) = **1 HP** for the first mission.
**Affected files:** `BalanceConfig.asset`
**Severity: Minor** (probably intentional warmup mechanic, but worth confirming)
**Question:** Is the first campaign mission (globalIndex=0) intended to be a nearly-zero-resistance tutorial? Should enemies at index=0 have more than 1 HP?

---

### P6 — BossMaxHealthApplier Uses FindObjectOfType (Minor)
**Evidence:** `BossMaxHealthApplier.cs` line 19: `FindObjectOfType<Player>()` — expensive, unreliable in multi-actor scenes.
**Affected files:** `BossMaxHealthApplier.cs`
**Severity: Minor** (out of scope for this audit per instructions — noting only)

---

### P7 — EnemyStatsConfig vs BalanceConfig Duplication (Minor)
**Evidence:** `EnemyStatsConfig` ScriptableObject (static Health/Damage fields) is used by the old `EnemyStatsApplier`. The new `BalanceConfig.EnemyModule` provides formula-driven stats. Both systems exist in the codebase.
**Affected files:** `EnemyStatsConfig.cs`, `EnemyStatsApplier.cs`
**Severity: Minor**
If old `EnemyStatsApplier` prefabs remain in gameplay scenes, they bypass the formula entirely and use static values. Needs prefab audit.

---

### P8 — RangedActive HP Multiplier May Be Too Low at High Campaign Levels (Minor)
**Evidence:** `typeHpMultiplier = 0.5` for RangedActive. At globalIndex=20, loop=0, Bow mission: Melee HP ≈ 1032, RangedActive HP ≈ 516. These are still meaningful health pools. However, if DamageMultiplier=2.0 is ever activated, RangedActive would deal ~2x more damage than Melee while having half the HP — an extreme glass-cannon at high levels.
**Severity: Minor** (no current impact while damage is hardcoded to 5)

---

## 10. Designer Questions

1. **Enemy damage:** Should enemy damage scale with mission level and loop? Is damage hardcoded to 5 intentional, or was it a temporary debugging measure? If scaling damage matters for difficulty, line 50 in `EnemyBalancedStatsApplier.cs` needs to be uncommented.

2. **Melee shots-to-kill target:** How many arrows (with a mid-game bow, say 40 damage) should it take to kill a Melee enemy in early/mid/late Campaign?
   - Early (index≈1): HP≈110 → ~3 arrows at 40 dmg
   - Mid (index≈7): HP≈234 → ~6 arrows
   - Late (index≈20): HP≈1032 → ~26 arrows
   Are these values in the right ballpark?

3. **RangedActive:** Should RangedActive always die faster than Melee? The current setup (0.5× HP) ensures this. Is the ratio (2:1 HP difference) correct?

4. **Contracts difficulty ceiling:** Should Contracts ever become as hard as Campaign? Or should there be a cap on `contractsDifficulty`? If yes — what is the maximum acceptable difficulty scalar for Contracts?

5. **Contracts scaling base:** Contracts currently start at `diffScalar=0.5` (50% of gate damage × 1.0). This makes early contracts significantly easier than Campaign. Is this the right feel, or should Contracts start closer to Campaign difficulty?

6. **Sniper one-shot guarantee:** Currently all Sniper enemies have HP=1. If a player has 30 Crossbow damage, they one-shot everything. Is this intentional and should it remain? Or should Sniper enemies have slightly more HP (e.g. HP=5) to allow for bodyshot vs headshot differentiation?

7. **Headshot/bodyshot differentiation:** Is there a damage multiplier system for headshots? Should Sniper bodyshots NOT kill in one shot (HP > 1) to reward headshots?

8. **Loop enemy scaling:** Loop 1 and Loop 2 apply `CampaignDifficulty` multipliers (1.3× and 1.5×) on top of the normal difficulty curve. Is this the intended loop progression? Contracts and Sniper loop difficulty multipliers are currently 1.0 for all loops — should they scale in later loops?

9. **Boss HP:** `BossMaxHealthApplier` sets boss HP = `player.Damage × 10`. This means boss HP automatically adjusts to player power. Is this the intended mechanic? Should Boss HP ever be fixed/predictable, or is this dynamic scaling intentional?

10. **First mission warmup:** At `globalIndex=0`, all enemies have HP≈1 due to the AdditionalCurve effect. Is this an intentional zero-resistance tutorial, or is it a configuration mistake?

---

## 11. Files Reviewed

| File | Notes |
|---|---|
| `Assets/Scripts/Enemy/EnemyBalancedStatsApplier.cs` | Primary applier — reads formula, applies HP |
| `Assets/Scripts/Enemy/EnemyStatsApplier.cs` | Legacy applier — reads static EnemyStatsConfig |
| `Assets/Scripts/Enemy/EnemyStatsConfig.cs` | Static HP/Damage ScriptableObject definition |
| `Assets/Scripts/Enemy/EnemyArchetypeTag.cs` | Tags an enemy with Melee/RangedPassive/RangedActive |
| `Assets/Scripts/Enemy/EnemyStatsReceiverSystem.cs` | Actor.System that writes HP/Damage to Actor properties |
| `Assets/Scripts/Enemy/IEnemyStatsReceiver.cs` | Interface: SetHealth, SetDamage, SetCoinsDrop, SetManaDrop |
| `Assets/Scripts/Enemy/BossMaxHealthApplier.cs` | Boss HP override: player.Damage × 10, bypasses formula |
| `Assets/Meta/Balance/EnemyArchetype.cs` | Enum: None, Melee, RangedPassive, RangedActive |
| `Assets/Meta/Balance/Config/BalanceConfig.cs` | GetEnemyStats() formula, GetDifficultyScalar(), gate API |
| `Assets/Meta/Balance/Config/BalanceConfig.EnemyModule.cs` | EnemyTypeProfile, difficulty scalars, HpFromGate factors |
| `Assets/Meta/Balance/Config/BalanceConfig.GateModule.cs` | WeaponGateProfile per weapon class, sniper/boss access profiles |
| `Assets/Meta/Balance/Config/BalanceConfig.LoopModule.cs` | LoopMultipliers: CampaignDifficulty, ContractDifficulty, GateDamage |
| `Assets/Meta/Balance/Config/BalanceConfig.Progressions.cs` | LinearProgression helper (base + perStep × index) |
| `Assets/Meta/Balance/Config/BalanceConfig.asset` | All actual numerical values |
| `Assets/Meta/Balance/MetaLoopProgressData.cs` | Persisted grind indices: ContractsCompletedIndex, SniperCompletedIndex |
| `Assets/Meta/Missions/Controllers/GameplayRuntime.cs` | Singleton: provides Balance, Context, grind indices |
| `Assets/Meta/Missions/MissionContext.cs` | Context DTO: type, globalIndex, weaponClass, balanceMode, etc. |
| `Assets/Meta/Missions/Services/MissionContextService.cs` | Builds MissionContext from progress + catalog |
| `Assets/Meta/Missions/Launch/ContractPoolService.cs` | Picks random eligible campaign mission for Contracts |
| `Assets/Meta/Weapons/Catalog/WeaponEnums.cs` | WeaponClass enum: Bow=0, Crossbow=1, Spear=2, Shuriken=3, Boomerang=4 |
