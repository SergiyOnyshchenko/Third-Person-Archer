# Meta Balance Pacing Validation

**Date:** 2026-05-27
**Phase:** 3 — Pacing Validation + UX Follow-up
**Based on:** `Meta_Balance_After_Fix.md` + Phase 2 implementation

---

## 1. Zone 2 Gate Spacing Analysis

Zone 2 spans global mission indices g7–g14 (8 missions across Spear/Shuriken/Bow cycling).

| g  | Class     | Gate  | Weapon State Before Mission | Status    | Notes |
|----|-----------|-------|-----------------------------|-----------|-------|
| 7  | Spear     | 55    | Spear1 base=65              | **FREE**  | First Spear mission, no upgrade needed |
| 8  | Shuriken  | 26    | Shuriken1 base=25           | BLOCK     | Cheap: L1 (50 cash, 1 tok) |
| 9  | Bow       | 43    | Bow1 L1=37 (from g6)        | BLOCK     | Moderate: need L4 (3 more levels, ~170 cash, 3 tok) |
| 10 | Spear     | 70    | Spear1 base=65              | BLOCK     | Cheap: L1 (70 cash, **1 tok** after Phase 3 fix) |
| 11 | Shuriken  | 32    | Shuriken1 L1=26             | BLOCK     | Heavy: need L7 (6 more levels, ~240 cash, 6 tok) |
| 12 | Bow       | 49    | Bow1 L4=43                  | BLOCK     | Moderate: need L7 (3 more levels, ~170 cash, 3 tok) |
| 13 | Spear     | 85    | Spear1 L1=70.5              | BLOCK     | Moderate: need L4 (**~5 tok** after Phase 3 fix, was ~10) |
| 14 | Shuriken  | 38    | Shuriken1 max=35            | **FORCED PURCHASE** | Shuriken2: 650 cash, 15 tok |

**Zone 2 block count:** 7 out of 8 missions require player action.

---

## 2. Consecutive Block Analysis

### Cluster 1 — Zone 2 Entry (g8/g9/g10)

Three consecutive missions across three different weapon classes:

```
g8  Shuriken L1  → 1 token      (trivial)
g9  Bow L4       → 3 tokens     (moderate)
g10 Spear L1     → 1 token      (trivial after Phase 3 fix)
```

**Assessment: ACCEPTABLE.** The g8 and g10 blocks are teaching-grade (1 token each). g9 is moderate but expected as Zone 2 "entry tax." The class alternation is intentional — Zone 2 introduces Spear and reinforces all three classes. Each block is affordable from Zone 1 income + 1 Contracts run.

### Cluster 2 — Zone 2 Mid (g11/g12/g13)

Three consecutive missions across three different weapon classes, all requiring meaningful upgrades:

```
g11 Shuriken L7  → 6 tokens     (expensive)
g12 Bow L7       → 3 tokens     (moderate)
g13 Spear L4     → 5 tokens     (moderate after Phase 3 fix, was ~10)
```

**Assessment: MODERATE CONCERN.** These three missions require different class tokens simultaneously. A player must have ~14 tokens across 3 classes before advancing. This is the most likely friction point for minimal players. However:
- By g11, the player has completed 11 missions earning tokens in each class.
- 1–2 Contracts runs before g11 provides ~6–8 tokens per class — enough to clear all three blocks.
- The Spear token cost fix (Phase 3) reduces g13 from ~10 tokens to ~5 tokens, meaningfully cutting the cluster cost.

**Verdict:** No gate formula change needed. The fix is already applied (Spear token cost). If playtest confirms frustration at this cluster, consider one non-upgrade mission (e.g., a free-pass Spear or Bow mission inserted between g12 and g14) to give the player a reward beat before g13.

### Cluster 3 — Zone 2 Exit (g14)

Single forced purchase. Follows g13 Spear block directly.

**Assessment: HIGH STAKES but correct design.** Shuriken2 purchase (650 cash, 15 tok) is a major investment. However, it comes after 14 missions of income accumulation and 2–3 expected Contracts runs. Players should have enough cash by g14. Tokens may be tight — player needs 15 Shuriken tokens, and Campaign gives only 7 Shuriken tokens through g14. Requires ~2 Contracts runs for Shuriken tokens.

---

## 3. Gate Formula Assessment — No Changes Made

Softening individual Zone 2 gates via the continuous linear formula (`Base + Growth × globalIndex`) is not feasible without breaking calibrated checkpoints:

| Concern | Why formula can't be changed |
|---------|------------------------------|
| Soften g9 Bow (43→37) | Requires lowering Bow base or growth → breaks g6 teaching gate (must stay >35) |
| Soften g11 Shuriken (32→28) | Requires raising Shuriken1 base or lowering growth → removes g8 teaching gate |
| Soften g13 Spear | Already fixed via **Spear token cost reduction** (not gate value) |

**Conclusion:** Gate values remain unchanged. The Spear token fix addresses the heaviest Zone 2 friction. Gate formula is preserved.

---

## 4. Estimated Contracts/Sniper Runs Required

### Before g14 (Shuriken2 forced purchase)

| Resource Needed | Campaign Income (through g13) | Gap | Required Grind |
|-----------------|------------------------------|-----|----------------|
| Shuriken tokens (15) | 7 from Campaign | -8 | **~2 Contracts runs** (gives 3+4=7 tok, need 3rd run) |
| Cash for Shuriken2 (650) | ~1617 cumulative | surplus | No grind needed for cash |

**Verdict:** 2–3 Contracts runs needed before g14, primarily for Shuriken tokens.

### Before g16 (Bow2 forced purchase)

| Resource Needed | Available after g15 | Gap | Required Grind |
|-----------------|--------------------|----|----------------|
| Bow tokens (14) | 8 from Campaign g16 | -6 | **~1–2 Contracts runs** |
| Cash for Bow2 (600) | 1960 cumulative | surplus | No cash grind needed |

**Verdict:** 1–2 Contracts runs for Bow tokens. Cash is fine.

### Before g22 (Spear2 forced purchase)

| Resource Needed | Available after g21 | Gap | Required Grind |
|-----------------|--------------------|----|----------------|
| Spear tokens (25 for purchase + upgrades) | ~9 from Campaign + Spear1 upgrade spend | significant gap | **3–4 Contracts runs** |
| Cash for Spear2 (1000) | 3381 cumulative | surplus | No cash grind needed |

**Verdict:** Spear2 is the most expensive token gate. 3–4 Contracts runs needed, spread across Zone 3. This is the intended "major progression milestone."

### Summary

| Gate | Min Contracts Runs | Notes |
|------|-------------------|-------|
| g14 (Shuriken2) | 2–3 | Primarily for Shuriken tokens |
| g16 (Bow2) | 1–2 | Primarily for Bow tokens |
| g22 (Spear2) | 3–4 | Spread across Zone 3 |
| Total Zone 1–3 | **6–9 Contracts runs** | Consistent with ~1–2 runs per zone |

---

## 5. Spear1 Upgrade Token Cost — Recommendation Implemented

**Problem:** Spear1 `startTok=2 / maxTok=5` gave a total-to-max of ~35 tokens. With 4–6 Spear tokens from Campaign before g13, the player needed 3+ Contracts runs just for Spear, on top of Shuriken and Bow token needs.

**Fix applied:** `startTok=2 → 1`, `maxTok=5 → 3`

**Effect:**

| Metric | Before | After |
|--------|--------|-------|
| Spear1 L1 token cost | 2 | **1** |
| Spear1 L4 token cost (cumulative) | ~10 | **~5** |
| Spear1 L10 token cost (total) | ~35 | **~20** |
| g10 block cost | 70 cash, 2 tok | 70 cash, **1 tok** |
| g13 block cost | ~200 cash, ~10 tok | ~200 cash, **~5 tok** |

Spear remains the most expensive weapon to upgrade (Spear max ~20 tok vs Bow/Shuriken/Boom ~14.5 tok), preserving meaningful differentiation without excessive token drain.

---

## 6. Sniper Gate UX — Implementation Summary

**Before Phase 3:** `MissionStartService` returned `SniperDamageTooLow` with gate data, but no popup appeared. The Play button was also not clickable in the Sniper-blocked state, making the block invisible to the player.

**After Phase 3:**

### What was wired

| Change | File |
|--------|------|
| `SniperDamageTooLow` added to `canClickPlay` condition | `PlayMissionPresenter.cs` |
| `ShowSniperGatePopup()` added, called on `SniperDamageTooLow` | `PlayMissionPresenter.cs` |
| Sniper popup uses existing `DamageGatePopup` screen | (no new screen needed) |

### Popup content for Sniper gate

| Field | Text |
|-------|------|
| Title | "Sniper access requires a stronger Crossbow" |
| Current damage | Player's equipped Crossbow damage |
| Required damage | Gate requirement for next Sniper mission |
| Hint (can upgrade) | "Upgrade your Crossbow to unlock this Sniper mission." |
| Hint (max tier hit) | "Your Crossbow is fully upgraded. Buy a stronger Crossbow to continue." |
| Button | Opens WeaponSelectionScreen with Crossbow preselected |

### UX Flow
1. Player taps Sniper mission button
2. Play button is enabled (player can tap it)
3. On tap: `SniperDamageTooLow` detected → `ShowSniperGatePopup()` called
4. Popup shows Crossbow damage gap + contextual hint
5. "Upgrade" button navigates to WeaponSelectionScreen → Crossbow tab preselected

---

## 7. Forced Purchase Popup Behavior

**Before Phase 3:** `DamageGatePopupController` always showed "Upgrade your {weapon}, or buy a new one." regardless of whether upgrading was possible. At forced-purchase gates (g14, g16, g21, g22), the hint was misleading — the current weapon cannot be upgraded to pass.

**After Phase 3:**

### What changed

| Change | File |
|--------|------|
| `CanUpgradeToPass` added to `MissionGateResult` | `MissionGateResult.cs` |
| `GetEquippedMaxDamage()` added to `ILoadoutWeaponStatService` | `ILoadoutWeaponStatService.cs`, `LoadoutSnapshotWeaponStatService.cs` |
| Gate checks compute `canUpgrade = maxDamage >= required` | `MissionGateService.cs` |
| `CanUpgradeToPass`, `CustomTitle`, `CustomHint` added to `DamageGatePopupArgs` | `DamageGatePopupArgs.cs` |
| `_forcedPurchaseHintFormat` serialized field added | `DamageGatePopupController.cs` |
| Controller selects hint based on `CanUpgradeToPass` | `DamageGatePopupController.cs` |

### Hint text selection logic

```
if CustomHint set     → use CustomHint
else if !CanUpgrade   → "Your {weapon} is fully upgraded. Buy a stronger weapon of the same class."
else                  → "Upgrade your {weapon}, or buy a new one."
```

### Affected forced-purchase gates

| Gate | Weapon | CanUpgradeToPass | Hint shown |
|------|--------|-----------------|-----------|
| g14  | Shuriken1 max=35 < required=38 | **false** | "Your shuriken is fully upgraded. Buy a stronger weapon." |
| g16  | Bow1 max=55 < required=57 | **false** | "Your bow is fully upgraded. Buy a stronger weapon." |
| g21  | Shuriken2 max=50 < required=52 | **false** | "Your shuriken is fully upgraded. Buy a stronger weapon." |
| g22  | Spear1 max=110 < required=130 | **false** | "Your spear is fully upgraded. Buy a stronger weapon." |
| All upgrade gates | current < required but max ≥ required | **true** | "Upgrade your {weapon}, or buy a new one." |

---

## 8. Files Changed in Phase 3

### Code (6 files)

| File | Change |
|------|--------|
| `ILoadoutWeaponStatService.cs` | Added `GetEquippedMaxDamage(WeaponClass)` to interface |
| `LoadoutSnapshotWeaponStatService.cs` | Implemented `GetEquippedMaxDamage` via `slot.Weapon.MaxStats.Damage` |
| `MissionGateResult.cs` | Added `CanUpgradeToPass` bool (default `true`) |
| `MissionGateService.cs` | Both gate checks now compute and pass `canUpgrade` |
| `DamageGatePopupArgs.cs` | Added `CanUpgradeToPass`, `CustomTitle`, `CustomHint` |
| `DamageGatePopupController.cs` | Added `_forcedPurchaseHintFormat` field; selects hint contextually |
| `PlayMissionPresenter.cs` | Wired `SniperDamageTooLow` to `canClickPlay` + `ShowSniperGatePopup()` |

### Assets (1 file)

| File | Change |
|------|--------|
| `Spear WeaponDef 1.asset` | `_startUpgradeTokens: 2 → 1`, `_maxUpgradeTokens: 5 → 3` |

---

## 9. Remaining Tuning Risks

| Risk | Severity | Notes |
|------|----------|-------|
| g11+g12+g13 cluster | MEDIUM | Three consecutive blocks in different classes still requires pre-planning. If playtests show >30% drop-off at Zone 2 mid, consider inserting a reward-only event or softening g11 Shuriken gate by 1 level (L6 instead of L7). |
| Shuriken2 token cost at g14 | MEDIUM | 15 tokens required. Zone 2 Campaign gives 7. Player needs ~3 Contracts runs specifically for Shuriken tokens before g14. Monitor if this feels too grindy vs. cash-only pressure. |
| Spear2 token cost at g22 | MEDIUM | Still the biggest single gate (25 tokens for purchase + upgrades needed). 3–4 Contracts runs spread over Zone 3. The reduced Spear1 upgrade cost (Phase 3) eases earlier gates but not the Spear2 purchase requirement. |
| `_forcedPurchaseHintFormat` Inspector default | LOW | The serialized default string in `DamageGatePopupController` reads "Your {0} is fully upgraded. Buy a stronger weapon of the same class." The `{0}` placeholder is filled with the weapon class name (e.g., "bow"). Verify the final wording is correct in the Unity Inspector and adjust if needed. |
| `WeaponDef.MaxStats.Damage` vs computed max | LOW | `GetEquippedMaxDamage` returns `WeaponDef.MaxStats.Damage`, which is the raw design-time max. If any runtime modifier inflates max damage (elemental bonuses, loadout buffs), the computed max could exceed this value, making `canUpgrade=false` incorrect. Verify no such runtime modifiers exist for Campaign damage checks. |
| Sniper Crossbow2 access (index 8+) | LOW | Sniper index 8 requires Crossbow2 (base=37, gate=39). Only ~2 Sniper missions remain in Zone 3 by that point. The `CanUpgradeToPass=false` popup will now correctly say "buy a new Crossbow" rather than "upgrade." |
| Loop 2 gate pressure still untested | LOW | Loop 2 gates are 55% harder. All upgrade/purchase analysis above applies to loop 0 only. Loop 2 forced-purchase gates (e.g., Bow g16 = 71.25 > Bow2 max=80... actually Bow2 max=80 > 71.25, so still upgradeable in loop 1; Bow g16 in loop 2 = 88.35 > Bow2 max=80 → forces Bow3) will now correctly show "buy a stronger weapon" popup. |
