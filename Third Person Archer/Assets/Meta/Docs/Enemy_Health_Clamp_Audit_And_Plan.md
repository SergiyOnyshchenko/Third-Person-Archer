# Enemy Health Clamp — Audit & Plan

**Date:** 2026-05-27
**Phase:** 2 — Audit + Design Plan (no code or asset changes)
**Builds on:** `Enemy_Stats_Scaling_Audit.md` (Phase 1)

---

## 1. Current System Summary

### Where HP is computed
`BalanceConfig.GetEnemyStats()` (`Assets/Meta/Balance/Config/BalanceConfig.cs`)

Formula:
```
HP = max(1, round( gateDamage × CampaignHpFromGate × diffScalar × typeHpMult × loopDiffMult ))
```

- `gateDamage` = required weapon damage for this mission (`GetRequiredDamageForCampaign(weaponClass, gateIndex, loop)`)
- `CampaignHpFromGate` = 4.0 (constant from `BalanceConfig.EnemyModule`)
- `diffScalar` = `campaignDifficultyCurve(globalCampaignIndex)` (AnimationCurve, 1.0→~4.0 over 20 indices)
- `typeHpMult` = archetype multiplier (Melee=1.0, RangedPassive=0.75, RangedActive=0.5)
- `loopDiffMult` = per-loop campaign difficulty multiplier (loop0=1.0, loop1=1.3, loop2=1.5)

### Where HP is applied
`EnemyBalancedStatsApplier.Apply()` (`Assets/Scripts/Enemy/EnemyBalancedStatsApplier.cs`)
Calls `_receiver.SetHealth(stats.MaxHp)` in `Start()`.

### What data is read
- `GameplayRuntime.Instance.Balance` → `BalanceConfig`
- `GameplayRuntime.Instance.Context` → `MissionContext` (type, globalCampaignIndex, requiredWeaponClass, loopIndex)
- `GameplayRuntime.ContractsCompletedIndex`, `SniperCompletedIndex` from `MetaLoopProgressData`
- `EnemyArchetypeTag` on the enemy GameObject

### Equipped weapon damage usage
**Currently NOT used** in the enemy HP formula. HP is derived from gate damage (minimum required player damage), not from what the player actually has equipped.

`LoadoutSnapshot` / `ILoadoutWeaponStatService` provide `GetEquippedDamage(WeaponClass)` but this is wired into gate checking (meta layer), not into runtime HP scaling.

---

## 2. Current Campaign HP Problem

### The core issue

The formula uses `HpFromGate = 4.0` × `diffScalar`. Since `diffScalar` is unbounded (1.0 → ~4.0), and the formula is gate-based:

> **shots-to-kill for a player who just barely passes the gate** = `4.0 × diffScalar × typeHpMult`

This is independent of weapon class. The only difference between Bow and Spear in the current system is the gate damage value — not the shots-to-kill. Both a barely-gated Bow player and a barely-gated Spear player need exactly `4 × diffScalar` shots to kill a Melee enemy.

This is the root problem: **shots-to-kill grows linearly with the difficulty curve, with no upper bound.**

### Campaign difficulty curve (approximate)
| globalIndex | diffScalar |
|---|---|
| 1 | ~1.01 |
| 5 | ~1.20 |
| 7 | ~1.50 |
| 10 | ~2.00 |
| 15 | ~2.80 |
| 20 | ~3.97 |
| 22 | ~3.97 (clamped) |

### Gate damage values (loop 0, AdditionalCurve=1.0 for index≥1)
| globalIndex | Bow gate | Spear gate | Shuriken gate | Boomerang gate |
|---|---|---|---|---|
| 1 | 27 | 25 | 12 | 17.5 |
| 5 | 35 | 45 | 20 | 27.5 |
| 7 | 39 | 55 | 24 | 32.5 |
| 10 | 45 | 70 | 30 | 40 |
| 15 | 55 | 95 | 40 | 52.5 |
| 20 | 65 | 120 | 50 | 65 |

### Computed Melee HP by weapon class (loop 0)
`HP = gateDamage × 4.0 × diffScalar × 1.0`

| globalIndex | diffScalar | Bow Melee HP | Spear Melee HP | Shuriken Melee HP | Boomerang Melee HP |
|---|---|---|---|---|---|
| 1 | 1.01 | **109** | **101** | **48** | **71** |
| 5 | 1.20 | **168** | **216** | **96** | **132** |
| 7 | 1.50 | **234** | **330** | **144** | **195** |
| 10 | 2.00 | **360** | **560** | **240** | **320** |
| 15 | 2.80 | **616** | **1064** | **448** | **588** |
| 20 | 3.97 | **1032** | **1906** | **794** | **1033** |

### Shots-to-kill for barely-gated player (weapon damage = gate damage)
`shots = HP / gateDamage = 4.0 × diffScalar × typeHpMult`

| globalIndex | diffScalar | Melee shots | RangedPassive shots | RangedActive shots |
|---|---|---|---|---|
| 1 | 1.01 | **4** | **3** | **2** |
| 5 | 1.20 | **4.8** | **3.6** | **2.4** |
| 7 | 1.50 | **6** | **4.5** | **3** |
| 10 | 2.00 | **8** | **6** | **4** |
| 15 | 2.80 | **11.2** | **8.4** | **5.6** |
| 20 | 3.97 | **15.9** | **11.9** | **7.9** |

### Loop 1 & 2 multipliers
With loopDiffMult applied on top:
- Loop 1 (×1.3): index=20 Melee shots = 15.9 × 1.3 = **~21**
- Loop 2 (×1.5): index=20 Melee shots = 15.9 × 1.5 = **~24**

### Conclusion
Late Campaign (index 15-20) requires 11-16+ shots against Melee for a player who just passed the gate. Loop 2 pushes this to 24. This is a **bullet sponge problem** and the reason for the clamp system.

The meta Campaign gates ensure the player *can* deal enough damage, but not that they *won't need too many shots*. These are different properties.

---

## 3. Shots-To-Kill Design Table

**Principle:**
- `Melee` sets the ceiling (highest HP) — these are the values that define "how tough is a tough enemy"
- `RangedPassive` should be meaningfully easier than Melee
- `RangedActive` should die faster than both — glass cannon feel

**Notes on weapon role:**
- `Bow` — standard all-rounder, forgiving fight rhythm (multiple shots acceptable)
- `Spear` — powerful, slow-firing, should kill fast or gameplay feels punishing
- `Shuriken` — rapid fire, low per-shot damage, designed for many small hits
- `Boomerang` — high damage, mid-tier speed, should feel noticeably more powerful than Bow
- `Crossbow` — NOT used in Campaign; used in Sniper (one-shot) and Sniper access gate

### Proposed shots-to-kill table

| WeaponClass | Gameplay Role | Rec. vs Melee | **Max vs Melee** | Rec. vs RangedPassive | **Max vs RangedPassive** | Rec. vs RangedActive | **Max vs RangedActive** | Notes |
|---|---|---|---|---|---|---|---|---|
| **Bow** | Balanced, mid-damage | 3–4 | **6** | 2–3 | **4** | 1–2 | **3** | Standard fight rhythm |
| **Crossbow** | Sniper/Boss only | N/A | **N/A** | N/A | **N/A** | N/A | **N/A** | Not in Campaign |
| **Spear** | Power shot, slow reload | 1–2 | **2** | 1 | **2** | 1 | **1** | Every shot should feel decisive |
| **Shuriken** | Rapid fire, low per-shot | 3–4 | **5** | 2–3 | **4** | 1–2 | **2** | High ammo compensates for many shots |
| **Boomerang** | High damage, mid-speed | 2–3 | **4** | 1–2 | **3** | 1 | **2** | More powerful than Bow, less than Spear |

> **These values are proposals.** See Section 13 (Designer Questions) for open decisions.

### Rationale

**Spear max=2 (Melee):**
Spear has highest base damage (65-190+). A player using Spear should feel powerful. Two decisive shots keeps the weapon's identity intact. If enemies survive 8+ Spear hits, the weapon loses its punch.

**Shuriken max=5 (Melee):**
Shuriken trades low per-shot damage for high fire rate and ammo count. More shots are expected and feel natural due to the rapid auto-fire gameplay rhythm.

**Bow max=6 (Melee):**
Standard balanced experience. 6 shots allows a meaningful fight without becoming a slog.

**Boomerang max=4 (Melee):**
Boomerang base damage (55-130) is significantly higher than Bow (35-80). Sits between Bow and Spear in feel. 4 shots maintains its "stronger than Bow" identity.

---

## 4. Proposed Campaign Health Clamp Formula

### Design principle
Campaign HP still grows via the progression formula. The clamp only cuts the ceiling. It does NOT replace the formula — it adds a hard upper limit based on expected gameplay feel.

### Formula
```
// Step 1: compute normal progression HP
progressedHP = max(1, round( gateDamage × HpFromGate × diffScalar × typeHpMult × loopDiffMult ))

// Step 2: compute max allowed HP based on shots-to-kill target
maxAllowedHP = max(1, round( gateDamage × MaxShotsToKill[weaponClass][archetype] ))

// Step 3: apply clamp
finalHP = min(progressedHP, maxAllowedHP)
```

`MaxShotsToKill[weaponClass][archetype]` comes from a designer-configured profile in `BalanceConfig.EnemyModule` (see Section 9).

### When does the clamp activate?

The clamp activates when `progressedHP > maxAllowedHP`:
```
HpFromGate × diffScalar × typeHpMult × loopDiffMult > MaxShotsToKill[wc][arch]
```

For Bow (HpFromGate=4.0), Melee (typeHpMult=1.0), loop 0 (loopDiffMult=1.0):
- MaxShots=6: activates when `diffScalar > 1.5` → approximately **globalIndex ≥ 8**
- Below index 8: normal formula applies (early campaign grows naturally)
- From index 8 onwards: HP is clamped (late campaign stays manageable)

### Does the clamp kill meta progression feel?

No. The clamp is applied to HP, not to the gate. The gate still grows. The player still must upgrade their weapon to pass new missions. What changes is that enemies don't become bullet sponges — the player still feels progression through gate requirements, economy, and zone variety, not through enemy HP creep.

> "If you barely passed the gate, enemies still take at most `MaxShots` hits to kill. If you overleveled, they die in fewer shots — that's fine, it rewards meta investment."

### Hard clamp vs soft clamp

| Approach | Description | Pros | Cons |
|---|---|---|---|
| **Hard clamp (recommended)** | `finalHP = min(progressedHP, maxAllowedHP)` | Simple, predictable, designer-tunable | Does not guarantee "exactly maxShots" — overleveled player finishes faster |
| Soft clamp (lerp) | Blend between progression HP and maxAllowedHP | Smoother transition | More complex, harder to reason about |
| Recommended target | Pull HP toward `recommendedShots × gateDamage`, cap at `maxShots` | Two-tier tuning | Double the parameters to maintain |
| Per-archetype multiplier only | Adjust typeHpMult down | Easy to implement | Doesn't solve the diffScalar growth problem |

**Recommendation: Hard clamp with `min(progressedHP, maxAllowedHP)`.**

A second optional field `RecommendedShotsToKill` could be added to the profile for documentation/editor reference only, without affecting the runtime formula.

---

## 5. Campaign Is Not Contracts

### Key distinction

| Property | Campaign | Contracts |
|---|---|---|
| HP source | Gate-based formula + diffScalar | Gate-based formula + contractsDiff |
| diffScalar nature | Grows with campaign index (fixed curve) | Grows with completions count (linear) |
| HP clamp | **Yes — add shots-to-kill cap** | Not needed (lower HP to start with) |
| Player power assumption | Player just passed gate (minimum viable) | Player is grinding, likely overleveled |

### Why Campaign needs a clamp but Contracts may not

Campaign: progression pushes enemies from diffScalar=1 to diffScalar=4. A barely-gated player hits this problem. Enemy HP must be capped.

Contracts at N=0: HP = `gateDamage × 1.0 × 0.5 × typeHpMult`. The `HpFromGate=1.0` and `contractsDiff=0.5` gives half the HP of early Campaign for the same gateDamage. Even at N=20 (diff=1.5), Contracts HP < Campaign early HP at the same gateDamage. The clamp problem doesn't arise in Contracts until N is very large.

### How to maintain the distinction in code

The clamp formula checks `effectiveType == MissionType.Campaign` before applying.
The `WeaponClassHealthProfile` can have a separate flag: `ApplyToCampaign = true`, `ApplyToContracts = false`.

This ensures:
- Campaign gets capped HP at `gateDamage × maxShots`
- Contracts continue using the auto-leveling formula without intervention

---

## 6. Contracts Scaling

### Current state
- HP = `gateDamage × 1.0 × (0.5 + 0.05×N) × typeHpMult`
- Source mission: random completed Campaign mission → `gateIndex = mission.GlobalCampaignIndex`
- Auto-leveling is unbounded (no cap on N)

### Does it need a clamp?

At low N (0-10): Contracts HP is roughly 50-100% of gate damage → ~1-2 shots for barely-gated player. Very easy. Intentional grind-mode feel.

At N=20: `contractsDiff = 1.5`. HP = `gateDamage × 1.5 × typeHpMult`. Shots = 1.5 for barely-gated player. Still very manageable.

At N=60: `contractsDiff = 3.5`. Shots = 3.5. Getting closer to Campaign feel.

At N=100: `contractsDiff = 5.5`. Shots = 5.5. Starting to approach Campaign-level difficulty at the same gate.

**Recommendation: Do not add shots-to-kill clamp to Contracts right now.** The lower `ContractHpFromGate=1.0` (vs Campaign's 4.0) provides built-in safety. However, the unbounded linear growth at very high N (50+) should be monitored. A separate `MaxContractsDifficulty` cap (e.g., cap at diff=3.0) in `BalanceConfig.EnemyModule._contractsDifficulty` would address the P3 issue from Phase 1 audit without touching HP clamps.

If Contracts ever gets a shots-to-kill profile in the future, use looser values (e.g., Bow max=10 vs Campaign max=6) to preserve the "easy grind mode" character.

---

## 7. Sniper Scaling

### Current state
- `SniperHpFromGate = 0.01`
- `SniperBase = 0.1`, `SniperPerStep = 0.05`
- Combined multiplier at N=0: `0.01 × 0.1 = 0.001`
- HP = `gateDamage × 0.001 × typeHpMult`
- For any gateDamage up to ~1000: HP always rounds to **1**

### One-shot status: CONFIRMED

The current system reliably produces HP=1 for all Sniper enemies. Even after 100 completions (diff=5.1):
- HP = round(`50 × 0.01 × 5.1 × 1.0`) = round(2.55) = **3**

At N=100 with RangedActive:
- HP = round(`50 × 0.01 × 5.1 × 0.5`) = round(1.275) = **1**

So even at extreme grind, Sniper enemies stay at HP 1-3, which a Crossbow shot (30-80 damage) kills in one hit.

### Sniper health clamp interaction

The proposed Campaign health clamp is scoped to `MissionType.Campaign` only. Sniper uses `MissionType.Sniper` as `effectiveType`. Therefore **the clamp will NOT affect Sniper enemies.** No special handling needed.

### Recommendation
Keep current Sniper HP formula unchanged. Add a comment or assertion that `SniperHpFromGate` should stay ≤ 0.05 to preserve one-shot gameplay. If a designer accidentally raises `SniperHpFromGate` to 1.0, enemies would gain HP=100+ and break the Sniper mode.

**Optional safety:** Add `[Max(0.1f)]` tooltip warning on `SniperHpFromGate` in `BalanceConfig.EnemyModule`.

---

## 8. Boss Scaling

### Boss bypass location
In `BalanceConfig.GetEnemyStats()`:
```csharp
if (ctx.SelectedType == MissionType.Boss)
    return new EnemyModule.EnemyStats(1, 0);
```
Returns `HP=1, Damage=0` immediately — the entire formula is skipped.

### Why Boss must not use Campaign health clamp
Boss HP is controlled by `BossMaxHealthApplier` (separate MonoBehaviour), not by `BalanceConfig.GetEnemyStats`. It sets `HP = player.Damage × 10` via a 0.25s delayed call, overriding whatever the formula set. The Campaign health clamp in `GetEnemyStats()` would only affect the temporary HP=1 value (which gets immediately replaced by `BossMaxHealthApplier`), so it would have zero effect anyway.

### Crossbow Boss Gate
The Crossbow Boss Gate is a **meta access gate** — it gates *entry* to the Boss mission, not Boss HP. Boss HP = player damage × 10. These are independent systems. The health clamp plan does not need to reference or affect Boss scaling in any way.

**Conclusion:** Boss is confirmed fully isolated from the enemy health clamp system.

---

## 9. Data / Code Design Plan

### Recommended approach: Add `WeaponClassHealthProfile[]` to `BalanceConfig.EnemyModule`

This is the minimal change that achieves full designer control:
- Visible in Inspector (Unity serialized array)
- Tunable per weapon class and per archetype
- Centralized in `BalanceConfig` (consistent with existing pattern)
- No changes to `EnemyBalancedStatsApplier`
- Campaign-only flag allows future opt-in for Contracts

### New data structure (design only — do not implement yet)

Add inside `BalanceConfig.EnemyModule`:
```csharp
[Serializable]
public sealed class WeaponClassHealthProfile
{
    [Tooltip("Weapon class this profile applies to.")]
    public WeaponClass WeaponClass;

    [Header("Campaign Health Clamp (shots-to-kill max)")]
    [Tooltip("Max shots a barely-gated player should need to kill a Melee enemy.")]
    [Min(1)] public int MaxShotsVsMelee = 6;

    [Tooltip("Max shots a barely-gated player should need to kill a RangedPassive enemy.")]
    [Min(1)] public int MaxShotsVsRangedPassive = 4;

    [Tooltip("Max shots a barely-gated player should need to kill a RangedActive enemy.")]
    [Min(1)] public int MaxShotsVsRangedActive = 2;

    [Tooltip("Apply this clamp to Campaign missions.")]
    public bool ApplyToCampaign = true;

    [Tooltip("Apply this clamp to Contracts missions. Usually false.")]
    public bool ApplyToContracts = false;
}

[Header("Per-Weapon-Class Health Clamps")]
[Tooltip("Shots-to-kill caps per weapon class. Prevents Campaign enemies from becoming bullet sponges.")]
[SerializeField] private WeaponClassHealthProfile[] _weaponHealthProfiles;
```

New method in `EnemyModule`:
```csharp
public int GetMaxShotsToKill(WeaponClass wc, EnemyArchetype archetype)
{
    // find profile for wc
    // return matching archetype field
    // return int.MaxValue if no profile found (no clamp)
}

public bool ShouldApplyHealthClamp(WeaponClass wc, MissionType missionType)
{
    // return true if profile exists and flag for missionType is enabled
}
```

### Where to apply the clamp in `BalanceConfig.GetEnemyStats()`

After the current HP calculation line, add:
```csharp
// After: int hp = max(1, round(gateDamage × hpFromGate × difficulty × typeHp × mpHp))

if (_enemies.ShouldApplyHealthClamp(ctx.RequiredWeaponClass, effectiveType))
{
    int maxShots = _enemies.GetMaxShotsToKill(ctx.RequiredWeaponClass, archetype);
    int maxAllowedHp = Mathf.Max(1, Mathf.RoundToInt(gateDamage * maxShots));
    hp = Mathf.Min(hp, maxAllowedHp);
}
```

### Why this approach is preferred over alternatives

| Option | Verdict |
|---|---|
| A. `WeaponClassHealthProfile[]` in `BalanceConfig.EnemyModule` | **Recommended** — all data in one place, Inspector-visible, no applier changes |
| B. New `HealthClampModule` in `BalanceConfig` | Over-engineered for this feature |
| C. Clamp logic in `EnemyBalancedStatsApplier` | Splits logic between scene object and ScriptableObject — harder to tune |
| D. Separate `EnemyHealthScalingService` | Too many layers for a simple min() operation |
| E. Hardcoded per-class values in code | Not designer-friendly |

---

## 10. Runtime Flow Plan

### Current flow
```
Mission selected → [meta gate check] → MissionLaunchRequest.Set()
→ Scene loaded → GameplayRuntime.Awake() → MissionContext built
→ EnemyBalancedStatsApplier.Start()
   → GetEnemyStats(ctx, archetype, contractsN, sniperN, isMP)
      → gateDamage = gate formula
      → HP = gateDamage × 4.0 × diffScalar × typeHpMult × loopDiff
   → _receiver.SetHealth(HP)
```

### Proposed future flow
```
Mission selected → [meta gate check] → MissionLaunchRequest.Set()
→ Scene loaded → GameplayRuntime.Awake() → MissionContext built
→ EnemyBalancedStatsApplier.Start()
   → GetEnemyStats(ctx, archetype, contractsN, sniperN, isMP)
      → gateDamage = gate formula
      → progressedHP = gateDamage × 4.0 × diffScalar × typeHpMult × loopDiff    ← UNCHANGED
      → [NEW] if Campaign and health profile exists for weaponClass:
            maxAllowedHP = gateDamage × MaxShotsToKill[wc][archetype]
            finalHP = min(progressedHP, maxAllowedHP)
         else:
            finalHP = progressedHP
      → HP = max(1, finalHP)
   → _receiver.SetHealth(HP)
```

### What stays unchanged
- `EnemyBalancedStatsApplier` MonoBehaviour — no changes needed
- Gate formula — unchanged
- Archetype HP multipliers — unchanged (still control relative ratios between archetypes)
- Contracts and Sniper paths — not affected by clamp (flagged per profile)
- Boss bypass — unchanged

### What changes
- `BalanceConfig.EnemyModule` — new `WeaponClassHealthProfile[]` field + 2 new methods
- `BalanceConfig.GetEnemyStats()` — ~5 new lines after HP calculation
- `BalanceConfig.asset` — new profile data for Bow, Spear, Shuriken, Boomerang

### Player equipped damage
Player equipped damage (`LoadoutSnapshot.GetEquippedDamage(wc)`) is **not needed** for the clamp formula. The clamp is expressed as `gateDamage × maxShots`, not `playerDamage × maxShots`. This avoids requiring a new data source in GameplayRuntime and keeps the formula self-consistent.

If in the future you want "if player is overleveled, reduce enemy HP proportionally" — that would require `LoadoutSnapshot` in `GameplayRuntime`. That's a different feature (auto-leveling for Campaign) and should be a separate design decision.

---

## 11. Example Before/After Calculations

### Example 1: Campaign, Bow, Melee, mid-game (globalIndex=7)

**Setup:**
- Gate damage: Bow at index=7 = `25 + 2×7 = 39`
- Expected player weapon: Bow 1 at ~level 7 (interpolated damage ≈ 42) or Bow 1 max (55)
- diffScalar ≈ 1.50
- typeHpMult = 1.0 (Melee)

| | Before clamp | After clamp |
|---|---|---|
| progressedHP | 234 | 234 |
| maxAllowedHP | — | 39 × 6 = **234** |
| finalHP | **234** | **234** |
| shots (at player dmg=42) | 5.6 → 6 shots | 5.6 → 6 shots |
| **Change** | — | No change (at threshold) |

> At index=7, the formula naturally produces exactly 6 shots. Clamp activates from index 8+.

---

### Example 2: Campaign, Bow, Melee, late game (globalIndex=20)

**Setup:**
- Gate damage: 65 (Bow at index=20)
- Expected player weapon: Bow 2 at level 3-4 ≈ 65-70 damage (needed to pass gate)
- diffScalar ≈ 3.97
- typeHpMult = 1.0

| | Before clamp | After clamp |
|---|---|---|
| progressedHP | 1032 | 1032 |
| maxAllowedHP | — | 65 × 6 = **390** |
| finalHP | **1032** | **390** |
| shots (at player dmg=65) | 15.9 → **16 shots** | 6 shots |
| **Change** | Bullet sponge | Manageable |

> Clamp reduces late-game Melee HP by **62%**. Saves ~10 shots.

---

### Example 3: Campaign, Spear, Melee, mid-game (globalIndex=10)

**Setup:**
- Gate damage: 70 (Spear at index=10)
- Expected player weapon: Spear 1 at max (110 damage, passes gate)
- diffScalar ≈ 2.00
- typeHpMult = 1.0

| | Before clamp | After clamp |
|---|---|---|
| progressedHP | 560 | 560 |
| maxAllowedHP | — | 70 × 2 = **140** |
| finalHP | **560** | **140** |
| shots (at player dmg=70, gate-min) | 8 shots | **2 shots** |
| shots (at player dmg=110, Spear 1 max) | 5.1 → 6 shots | 1.3 → **2 shots** |
| **Change** | Too many | Decisive, powerful feel |

---

### Example 4: Campaign, Shuriken, RangedActive, late game (globalIndex=20)

**Setup:**
- Gate damage: 50 (Shuriken at index=20)
- Expected player weapon: Shuriken 2 max (50 damage, just meets gate)
- diffScalar ≈ 3.97
- typeHpMult = 0.5 (RangedActive)

| | Before clamp | After clamp |
|---|---|---|
| progressedHP | `50 × 4.0 × 3.97 × 0.5 = 397` | 397 |
| maxAllowedHP | — | 50 × 2 = **100** |
| finalHP | **397** | **100** |
| shots (at player dmg=50) | 7.9 → **8 shots** | **2 shots** |
| **Change** | RangedActive too tanky | Fragile, glass-cannon feel restored |

---

### Example 5: Contracts comparison (same gate, index=10, Bow, Melee)

**Setup:**
- Gate damage: 45 (Bow at index=10), contractsN=10
- contractsDiff = 0.5 + 0.05×10 = 1.0

| | Contracts HP | Campaign HP (clamped) |
|---|---|---|
| HP | `45 × 1.0 × 1.0 × 1.0 = 45` | 390 |
| shots (player dmg=45) | 1 shot | 6 shots |
| **Feel** | Very fast, grind-friendly | Meaningful fight |

> Contracts enemies are ~8.7× easier than Campaign at the same gate level. This is intentional and should remain.

---

### Example 6: Sniper, one-shot confirmation

**Setup:**
- SniperHpFromGate = 0.01, SniperBase = 0.1
- Any Crossbow damage (e.g., 37 — Xbow1 maxed)
- gateDamage ≈ 26 (Crossbow gate at typical index)

| | HP | Shots at 37 dmg |
|---|---|---|
| sniperN=0 | round(26 × 0.01 × 0.1 × 1.0) = round(0.026) = 0 → clamped to **1** | 1 |
| sniperN=50 | round(26 × 0.01 × 2.6 × 1.0) = round(0.68) = **1** | 1 |
| **Confirmed one-shot** | | |

> Clamp not needed. HP formula already achieves one-shot feel.

---

## 12. Potential Problems / Risks

### R1 — Overleveled player still kills fast, clamp may feel too easy in early loop
**Scenario:** Player grinds tokens and maxes Spear 2 (max=190) before entering mid-campaign. Gate at index=10 requires 70. Player has 190. Enemy HP = min(560, 140) = 140. Shots for overleveled player: 140/190 = 0.74 → 1 shot.

**Assessment:** This is acceptable and desirable. Meta investment (upgrading beyond gate requirement) should reward faster kills. The clamp prevents *bullet sponge for underpowered player*, not "enemy kills too fast for overpowered player". That's not a problem — it's a reward.

**Risk:** If enemies feel trivially easy even for average players, HpFromGate should be raised, not the clamp reduced.

---

### R2 — Archetype HP multipliers conflict with per-archetype clamp
**Scenario:** Melee clamp=2 shots. RangedActive clamp=1 shot. Archetype typeHpMult=0.5 for RangedActive. Both the multiplier AND the clamp push RangedActive HP down. At low diffScalar, the archetype multiplier may already achieve < 1 shot → HP could round to 1 even without clamp.

**Assessment:** Minor. `max(1, ...)` ensures minimum HP=1. The clamp applies after archetype multiplier is already baked in. This is correct behavior — RangedActive should always die first.

**Note:** The `MaxShotsVsRangedActive` values in the profile should be set with the awareness that typeHpMult=0.5 is already applied. So "1 shot max" means HP = gateDamage × 1. If typeHpMult gives HP = gateDamage × 0.5 naturally, the clamp at 1 is redundant but harmless.

---

### R3 — Loop scaling becomes invisible due to clamp
**Scenario:** Loop 1 applies CampaignDifficulty × 1.3 and GateDamage × 1.25. If the clamp is `gateDamage × maxShots`, and gateDamage scales with loop, then `maxAllowedHP` also scales with loop (since gateDamage includes loop multiplier). The player's weapon must pass a higher gate (weapon damage scales with loop), so shots-to-kill stays the same.

**Example:** Loop 0, index=20, Bow, Melee: HP=390, shots=6. Loop 2, index=20: gateDamage=65×1.55=100.75, maxAllowedHP=100.75×6=605. Player weapon must be ~100 damage to pass gate. Shots = 605/100 = 6. Same shots-to-kill.

**Assessment:** This is exactly correct. The clamp preserves shots-to-kill consistency across loops. Loops get harder via gate pressure (need stronger weapon), not via bullet sponge. This is the intended design.

---

### R4 — Minimum HP floor not defined
**Scenario:** A very early campaign mission (index=1) with a powerful weapon gate might have naturally low HP (e.g., HP=48 for Shuriken RangedActive at index=1). After clamp: min(48, Shuriken_gate(1) × 2) = min(48, 12×2) = min(48, 24) = 24. That's already low, no issue.

**Larger concern:** At index=0 (AdditionalCurve=0.01), enemy HP rounds to 1 regardless of clamp. This is an existing behavior from the warmup curve.

**Assessment:** No minimum HP floor is needed for now. `max(1, ...)` in the formula handles the edge case. If a minimum HP floor becomes desirable (e.g., HP never below 10), it can be added as `MinimumCampaignHp` in `BalanceConfig.EnemyModule`.

---

### R5 — Multi-hit, rapid fire, and fire-rate not accounted for
**Scenario:** Shuriken has high fire rate and ammo. "5 shots" for Shuriken might feel different from "5 shots" for Bow because Shuriken fires faster and has 60 ammo. Shuriken might empty 5 shots in 1 second, while Bow takes 5 seconds.

**Assessment:** The shots-to-kill design table accounts for this intuitively (Shuriken gets higher max shots). However, if Shuriken gets a multishot upgrade, shots-to-kill would drop even further. The current formula does not account for projectile count per shot — it treats all shots as single-hit.

**Recommendation:** When designing `MaxShotsVsMelee` for Shuriken, consider that "5 shots" with 0.25s reload = 1.25 second TTK. For Spear with "2 shots" and 1.5s reload = 3 seconds TTK. Shuriken kills faster in real time despite more shots. This may be intentional and is acceptable.

---

### R6 — Damage hardcode (5) masks the effectiveness of the health clamp analysis
The current enemy damage is hardcoded to 5. All shots-to-kill analysis assumes **player damage → enemy HP**, not the reverse. The clamp only affects HP (how fast enemies die), not the flat 5 damage enemies deal. These are independent systems. When enemy damage is eventually unfrozen, it should use a separate scaling model.

---

### R7 — Late Spear campaign missions may require Spear 2 (gating issue, not clamp issue)
Spear gate at index=20 is 120. Spear 1 max=110 is below this gate. This means Spear players need Spear 2 (unlock level 23) to access index≥19 missions. This is a meta progression gate issue (separate from HP clamp) but is worth noting: the HP clamp at `gateDamage × 2` for Spear means that when gateDamage=120, `maxAllowedHP=240`. If player is using Spear 2 base (110) which barely doesn't pass the gate, the system already protects them from needing more than 2 shots once they do pass.

---

## 13. Designer Questions

Please answer these to finalize the `WeaponClassHealthProfile` values before implementation:

1. **Bow max shots vs Melee:** Is 6 shots the right ceiling for Bow, or should it be lower (5) to feel more responsive? Does 6 shots feel like an engaging fight or start to feel grindy?

2. **Spear: always 2 shots max vs Melee?** Is "2 shots" a strict ceiling, or is 3 acceptable for a "strong Melee" enemy? Should Spear ever take 1 shot on RangedActive, or can it take 2?

3. **Shuriken: 4 or 5 max shots?** Given Shuriken's fast fire rate, does 5 feel natural (rapid fire rhythm) or should 4 be the ceiling to keep fights shorter?

4. **Boomerang: 4 max vs Melee.** Is Boomerang positioned closer to Bow (6 shots) or Spear (2 shots)? Current proposal is 4 as a midpoint. Should it be 3 (closer to Spear) or 5 (closer to Bow)?

5. **RangedPassive shots:** Should RangedPassive always require strictly fewer shots than Melee, or can they sometimes be equal? (Current proposal: RangedPassive max ≈ 66-75% of Melee max.)

6. **RangedActive shots:** Should RangedActive always die in 1-2 shots regardless of weapon class? Or is it OK for Shuriken RangedActive to take 3-4 shots given the rapid fire rhythm?

7. **Clamp in Contracts:** Should Contracts ever use the same shots-to-kill profiles, or should Contracts remain uncapped as an easy grind mode? (Current proposal: Contracts uncapped, Campaign capped.)

8. **Headshots:** Does the game have headshot multipliers? If headshots deal 2× damage, then "max 6 shots" means a headshot-only player kills in 3 shots. Is this factored into your shots-to-kill targets, or should targets assume bodyshots?

9. **Loop shots-to-kill:** In loop 1 and loop 2, enemies deal more gate pressure (need stronger weapon) but shots-to-kill stays the same with the proposed clamp. Is this correct? Or should loops intentionally increase shots-to-kill (making enemies tankier, not just harder to access)?

10. **Minimum HP floor:** Should enemies ever have a guaranteed minimum HP (e.g., HP ≥ 5) to prevent one-shot kills at any difficulty level except Sniper? Or is one-shot Campaign enemies acceptable when the player is highly overleveled?

---

## 14. Implementation Plan

**Do not implement yet.** This is the sequence to follow after designer sign-off on shot values (Section 13).

### Step 1 — Add data structure to `BalanceConfig.EnemyModule`
**File:** `Assets/Meta/Balance/Config/BalanceConfig.EnemyModule.cs`
- Add `WeaponClassHealthProfile` inner class (see Section 9)
- Add `WeaponClassHealthProfile[] _weaponHealthProfiles` serialized field
- Add `GetMaxShotsToKill(WeaponClass wc, EnemyArchetype archetype)` method
- Add `ShouldApplyHealthClamp(WeaponClass wc, MissionType missionType)` method (returns true if profile found and `ApplyToCampaign`/`ApplyToContracts` flag set)

### Step 2 — Apply clamp in `BalanceConfig.GetEnemyStats()`
**File:** `Assets/Meta/Balance/Config/BalanceConfig.cs`
- After the current `hp = max(1, round(...))` line, add ~5 lines:
  ```
  if (shouldApplyClamp)
      hp = max(1, min(hp, round(gateDamage × maxShots)))
  ```
- Clamp only when `effectiveType == Campaign` (or per-profile flag)
- Does NOT affect Contracts, Sniper, or Boss paths

### Step 3 — Configure `BalanceConfig.asset`
Add initial `_weaponHealthProfiles` entries for 4 Campaign weapon classes:
- Bow: MaxShotsVsMelee=6, MaxShotsVsRangedPassive=4, MaxShotsVsRangedActive=3
- Spear: MaxShotsVsMelee=2, MaxShotsVsRangedPassive=2, MaxShotsVsRangedActive=1
- Shuriken: MaxShotsVsMelee=5, MaxShotsVsRangedPassive=4, MaxShotsVsRangedActive=2
- Boomerang: MaxShotsVsMelee=4, MaxShotsVsRangedPassive=3, MaxShotsVsRangedActive=2
- Crossbow: (optional) MaxShotsVsMelee=1, MaxShotsVsRangedPassive=1, MaxShotsVsRangedActive=1, ApplyToCampaign=false

### Step 4 — Testing checklist
- [ ] Verify Melee HP at campaign index=7 equals exactly `Bow_gate(7) × 6 = 234` (at threshold)
- [ ] Verify Melee HP at campaign index=20 equals `Bow_gate(20) × 6 = 390` (clamped)
- [ ] Verify Spear Melee HP at index=10 equals `Spear_gate(10) × 2 = 140` (clamped)
- [ ] Verify Contracts HP at same mission is NOT clamped (returns uncapped value)
- [ ] Verify Sniper HP remains 1 (no regression)
- [ ] Verify Boss enemies: `GetEnemyStats()` early-exits and returns HP=1 (clamp never reached)
- [ ] Verify all 3 archetypes (Melee, RangedPassive, RangedActive) use correct per-archetype maxShots
- [ ] Verify a missing `WeaponClassHealthProfile` (no entry for that class) returns unclamped HP (safe default)
- [ ] Verify loop 1/2: `gateDamage` includes loop multiplier → `maxAllowedHP` also scales → shots-to-kill stays constant per loop

### Step 5 — Debug/editor helper suggestions
- Add a `[ContextMenu("Compute Campaign HP Table")]` on `BalanceConfig` that prints a summary table (index, weapon class, archetype, progressedHP, clampedHP, shots) to the Console
- Add a tooltip warning on `SniperHpFromGate` in `BalanceConfig.EnemyModule`: "Keep this ≤ 0.05 to preserve one-shot Sniper gameplay"
- Consider a custom Inspector drawer for `WeaponClassHealthProfile[]` that shows "shots vs Melee / RangedPassive / RangedActive" as a compact table

---

## 15. Files Reviewed

| File | Notes |
|---|---|
| `Assets/Meta/Docs/Enemy_Stats_Scaling_Audit.md` | Phase 1 audit baseline |
| `Assets/Scripts/Enemy/EnemyBalancedStatsApplier.cs` | Primary applier, entry point |
| `Assets/Scripts/Enemy/BossMaxHealthApplier.cs` | Boss bypass, player damage × 10 |
| `Assets/Meta/Balance/Config/BalanceConfig.cs` | `GetEnemyStats()` formula, clamp insertion point |
| `Assets/Meta/Balance/Config/BalanceConfig.EnemyModule.cs` | HpFromGate factors, archetype profiles, new profile will go here |
| `Assets/Meta/Balance/Config/BalanceConfig.GateModule.cs` | Per-class gate profiles (BaseDamage, Growth, Cap) |
| `Assets/Meta/Balance/Config/BalanceConfig.LoopModule.cs` | Loop difficulty and gate multipliers |
| `Assets/Meta/Balance/Config/BalanceConfig.asset` | All actual values |
| `Assets/Meta/Missions/Controllers/GameplayRuntime.cs` | Runtime singleton, data flow |
| `Assets/Meta/Missions/MissionContext.cs` | Context DTO |
| `Assets/Meta/Missions/Launch/MissionLaunchRequest.cs` | Launch data — does not need changes |
| `Assets/Meta/Weapons/Catalog/WeaponEnums.cs` | WeaponClass enum |
| `Assets/Meta/Weapons/Defs/WeaponDef.cs` | Weapon base/max stats structure |
| `Assets/Meta/Weapons/Loadout/LoadoutSnapshot.cs` | Player equipped weapon data source (not used in clamp formula) |
| `Assets/Meta/Weapons/Loadout/LoadoutSnapshotWeaponStatService.cs` | `GetEquippedDamage()` — available but not needed for gate-based clamp |
| `Assets/Meta/Missions/Services/Interfaces/ILoadoutWeaponStatService.cs` | Interface for equipped damage |
| `Assets/Meta/Missions/Services/WeaponRequirementService.cs` | Weapon class assignment per mission/loop |
| `Assets/Meta/Missions/Launch/ContractPoolService.cs` | Contracts source mission picker |
| **Weapon assets read for damage values:** | |
| `Assets/Meta/Weapons/Data/Wapons/Bow WeaponDef 1.asset` | base=35, max=55 |
| `Assets/Meta/Weapons/Data/Wapons/Bow WeaponDef 2.asset` | base=55, max=80, unlock=12 |
| `Assets/Meta/Weapons/Data/Wapons/Spear WeaponDef 1.asset` | base=65, max=110 |
| `Assets/Meta/Weapons/Data/Wapons/Spear WeaponDef 2.asset` | base=110, max=190, unlock=23 |
| `Assets/Meta/Weapons/Data/Wapons/Spear WeaponDef 3.asset` | base=190, max=300, unlock=35 |
| `Assets/Meta/Weapons/Data/Wapons/Shuriken WeaponDef 1.asset` | base=25, max=35 |
| `Assets/Meta/Weapons/Data/Wapons/Shuriken WeaponDef 2.asset` | base=35, max=50, unlock=13 |
| `Assets/Meta/Weapons/Data/Wapons/Boomerang WeaponDef 1.asset` | base=55, max=80 |
| `Assets/Meta/Weapons/Data/Wapons/Boomerang WeaponDef 2.asset` | base=80, max=130, unlock=28 |
