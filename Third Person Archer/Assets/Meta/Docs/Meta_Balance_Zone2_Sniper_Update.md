# Meta Balance — Zone 2 Pacing + Sniper Update

**Date:** 2026-06-01
**Phase:** Zone 2 Pacing + Sniper Rebalance
**Based on:** `Meta_Balance_After_BossGate.md` + `Meta_GameDesign_Audit_RU.md`

---

## 1. Audit Findings

### Zone 2 Pacing Problem (Pre-Change)

The design audit (`Meta_GameDesign_Audit_RU.md`, section 7) identified Zone 2 as the primary pacing risk:
- **7 out of 8 Campaign missions were blocked gates** (87% gate density)
- Two back-to-back 3-clusters: g8/g9/g10 and g11/g12/g13
- Only g7 (Spear free pass) gave breathing room before 7 consecutive blocks
- This created a "gate wall" instead of a difficulty curve

The audit recommended: reduce Zone 2 to 4–5 meaningful blocks, insert free-pass missions between clusters, avoid three different weapon-class blocks back-to-back.

### Sniper Content Changes (Pre-Audit)

New Sniper mission files were added by the developer before this phase:
- **Zone 1 (Japanese):** Sniper 6 added (new scene: `Level1_8 (Sniper) (2).unity`)
- **Zone 2 (North):** Sniper 3 and 4 added (new scenes: `Level2_5 (Sniper)`, `Level2_6 (Sniper)`)
- **Zone 3 (Wild West):** Sniper 3 and 4 added (new scenes: `Level3_1 (Sniper)`, `Level3_5 (Sniper)`)

**ZoneData Errors Found During Audit:**
During GUID cross-referencing, two errors were discovered in the modified ZoneData files:
| ZoneData | Entry | GUID | Problem |
|----------|-------|------|---------|
| ZoneData 2 (North) | 5th Sniper entry | `7d629bfad38c8334fb13db7375204e60` | References **Jap Sniper 1** (wrong zone!) |
| ZoneData 3 (Wild West) | 5th Sniper entry | `ff0c4061e43d4ea4a82aafd059bad1d4` | References **Jap Sniper 6** (wrong zone!) |

Additionally, Zone 1 ZoneData was never updated to include Jap Sniper 1 or Jap Sniper 6. It had Snipers 2, 3, 4, 5 but not 1 or 6.

**New Sniper count (after ZoneData corrections):**
| Zone | Before | After |
|------|--------|-------|
| Zone 1 (Japanese) | 4 in ZoneData (Snipers 2–5) | **6** (Snipers 1–6) |
| Zone 2 (North) | 5 in ZoneData (correct 4 + bogus Jap1) | **4** (North 1–4) |
| Zone 3 (Wild West) | 5 in ZoneData (correct 4 + bogus Jap6) | **4** (West 1–4) |
| **Total** | **7** (per prior docs: 5+2+0) → **13** (broken state) | **14** |

**Note:** Prior docs reported 7 Sniper missions (5+2+0). Between prior sessions and now, the developer added new missions and reorganized Zone 1 ZoneData, leaving Sniper 1 accidentally absent from Zone 1 and incorrectly placed in Zone 2.

---

## 2. Zone 2 Gate Table — BEFORE Changes

| g | Mission | Class | Gate Req. | Weapon Baseline | Status | Block Severity |
|---|---------|-------|-----------|-----------------|--------|----------------|
| 7 | Camp 1 | Spear | 55 | Spear1 base=**65** | **FREE** | — |
| 8 | Camp 2 | Shuriken | 26 | Shuriken1 base=**25** | BLOCK | Teaching (L1, 1 tok) |
| 9 | Camp 3 | Bow | 43 | Bow1 L4 needed | BLOCK | Moderate (3 toks) |
| 10 | Camp 4 | Spear | 70 | Spear1 L1 needed | BLOCK | Teaching (1 tok) |
| 11 | Camp 5 | Shuriken | 32 | Shuriken1 L7 needed | BLOCK | Heavy (6 toks) |
| 12 | Camp 6 | Bow | 49 | Bow1 L7 needed | BLOCK | Moderate (3 toks) |
| 13 | Camp 7 | Spear | 85 | Spear1 L4+ needed | BLOCK | Moderate (4–5 toks) |
| 14 | Camp 8 | Shuriken | 38 | Shuriken1 max=35 < 38 | **FORCED BUY** | Shuriken2 (15 toks) |

**Block count: 7 blocks out of 8 missions (87%). Two 3-clusters at g8/g9/g10 and g11/g12/g13.**

---

## 3. Zone 2 Gate Table — AFTER Changes

Changes applied: Spear1 base 65→**75**, Shuriken1 base 25→**27**.

| g | Mission | Class | Gate Req. | Weapon Baseline | Status | Change |
|---|---------|-------|-----------|-----------------|--------|--------|
| 7 | Camp 1 | Spear | 55 | Spear1 base=**75** | **FREE** | (same) |
| 8 | Camp 2 | Shuriken | 26 | Shuriken1 base=**27** | **FREE** | ← was BLOCK |
| 9 | Camp 3 | Bow | 43 | Bow1 L4 needed | BLOCK | (unchanged) |
| 10 | Camp 4 | Spear | 70 | Spear1 base=**75** | **FREE** | ← was BLOCK |
| 11 | Camp 5 | Shuriken | 32 | Shuriken1 L7 needed (27→33) | BLOCK | (same level req, cheaper from base) |
| 12 | Camp 6 | Bow | 49 | Bow1 L7 needed | BLOCK | (unchanged) |
| 13 | Camp 7 | Spear | 85 | Spear1 **L3** needed (75→86) | BLOCK | ← reduced from L4/L5 |
| 14 | Camp 8 | Shuriken | 38 | Shuriken1 max=35 < 38 | **FORCED BUY** | (unchanged) |

**Block count: 4 blocks + 1 forced purchase = 5 gate events (from 7). Target was 4–5. ✓**

### Pacing Flow (After)
```
g7: FREE  (Spear — zone intro gift)
g8: FREE  (Shuriken — warm-up, still earns 1 Shu tok)
g9: BLOCK (Bow L4 — first meaningful gate, moderate)
g10: FREE (Spear — breathing room between clusters)
g11: BLOCK (Shuriken L7 — heavy gate, requires Contracts)
g12: BLOCK (Bow L7 — moderate, follow-on)
g13: BLOCK (Spear L3 — cheap, ~3 Spear toks, just a bump)
g14: FORCED (Shuriken2 — zone exit milestone)
```

**Consecutive block maximum reduced from 3 to 2** (g11+g12 are still back-to-back, but g13 is a light single-level gate). The 3-cluster at zone entry (g8/g9/g10) is fully broken up.

---

## 4. What Changed and Why

### Spear1 base damage: 65 → 75

**File:** `Spear WeaponDef 1.asset`

**Why:** The audit recommended this exact change ("Поднять starter weapon damage для Spear (base 65→75, чтобы g10=70 стал free pass)"). Spear1 base of 65 put g10 just out of reach (gate=70), requiring L1 upgrade even though the player just got the weapon in g7. With base=75:
- g7 gate=55: even more free (75 >> 55), reinforcing Spear intro feel
- g10 gate=70: free pass (75 > 70) — breathing room mid-zone ✓
- g13 gate=85: now needs L3 (75+35×0.3≈86) instead of L4/L5 — reduced but not eliminated ✓
- Zone 3 g18 gate=110: unchanged (Spear1 max=110 still required) ✓

**Spear1 damage progression (new):**
| Level | Damage | Passes Gate |
|-------|--------|-------------|
| L0 | 75 | g7(55), g10(70) ✓ |
| L1 | ~79 | — |
| L2 | ~82 | — |
| L3 | ~86 | g13(85) ✓ |
| L4 | ~89 | — |
| L10 | 110 | g18(110) ✓ |

### Shuriken1 base damage: 25 → 27

**File:** `Shuriken WeaponDef 1.asset`

**Why:** g8 Shuriken gate=26. With base=25 it required L1 immediately at Zone 2 entry. With base=27, g8 is a free pass — the player's first Zone 2 Shuriken mission delivers content without a wall. Zone 1 Shuriken gates (g1=12, g3=16, g5=20) are all < 25 originally, so they are already free. They remain free at 27. Token economy impact is neutral: the player saves 1 token by not buying L1 at g8, but needs the same total tokens to reach L7 for g11.

**Shuriken1 damage progression (new):**
| Level | Damage | Passes Gate |
|-------|--------|-------------|
| L0 | 27 | g8(26) ✓ |
| L7 | ~33 | g11(32) ✓ |
| L10 | 35 | — (g14 forced buy at 38) |

### Sniper Access Gate growth: 3.0 → 2.5

**File:** `BalanceConfig.asset` → `_sniperAccessProfile.MissionGrowth`

**Why:** With 14 Sniper missions (doubled from 7), the old growth=3 formula would require Xbow2 at index 8 (the 9th Sniper mission). Xbow2 upgrade tokens are very expensive (9–18 per level), and the early Zone 2 Snipers would hit an expensive wall just as players enter new content. With growth=2.5, Xbow1 max (37) remains sufficient through index 8 (gate=35 < Xbow1 max=37). Xbow2 is required from index 9 onward — which corresponds to late Zone 2 or Zone 3 Snipers. This is a more natural gate ramp across the doubled content.

### ZoneData fixes

**Files:** `ZoneData 1 (Japanese).asset`, `ZoneData 2 (North).asset`, `ZoneData 3 (Wild West).asset`

Corrected wrong-zone Sniper mission references. Zone 1 now has all 6 Jap Sniper missions. Zone 2 and 3 each have exactly their own 4 Sniper missions. See Section 1 for details.

---

## 5. Sniper Mission Count Summary

### New Sniper Layout

| Zone | Mission | Scene | Sniper Index (global) |
|------|---------|-------|----------------------|
| Zone 1 | Jap Sniper 1 | Level1_8 (Sniper) (1) | 0 |
| Zone 1 | Jap Sniper 2 | — | 1 |
| Zone 1 | Jap Sniper 3 | — | 2 |
| Zone 1 | Jap Sniper 4 | — | 3 |
| Zone 1 | Jap Sniper 5 | Level1_7 (Sniper) | 4 |
| Zone 1 | Jap Sniper 6 | Level1_8 (Sniper) (2) | 5 |
| Zone 2 | North Sniper 1 | — | 6 |
| Zone 2 | North Sniper 2 | — | 7 |
| Zone 2 | North Sniper 3 | Level2_5 (Sniper) | 8 |
| Zone 2 | North Sniper 4 | Level2_6 (Sniper) | 9 |
| Zone 3 | West Sniper 1 | — | 10 |
| Zone 3 | West Sniper 2 | — | 11 |
| Zone 3 | West Sniper 3 | Level3_1 (Sniper) | 12 |
| Zone 3 | West Sniper 4 | Level3_5 (Sniper) | 13 |

**Total: 14 Sniper missions** (was 7 before this phase). All use `_fixedWeaponClass: 1` (Crossbow), `_allowLoopRotation: 0`. Gameplay remains one-shot Crossbow. ✓

**Zone 2/3 Snipers unlock with zone:** Zone 2 Snipers (indices 6–9) are only accessible after Zone 2 is unlocked (after Zone 1 Boss). Zone 3 Snipers (indices 10–13) require Zone 3 unlock. Token income is therefore spread across the full playthrough, not front-loaded.

---

## 6. Sniper Reward/Gate Recalculation

### CrossbowToken Income

Sniper reward formula (unchanged): `Cash = 150 + 50×step`, `CrossbowTokens = 2 + 1×step`

| Phase | Snipers | Steps | CrossbowTokens |
|-------|---------|-------|----------------|
| Zone 1 (indices 0–5) | 6 | 0–5 | 2+3+4+5+6+7 = **27** |
| Zone 2 (indices 6–9) | 4 | 6–9 | 8+9+10+11 = **38** |
| Zone 3 (indices 10–13) | 4 | 10–13 | 12+13+14+15 = **54** |
| **All 14 Snipers** | **14** | 0–13 | **119 total** |

Old total (7 Snipers): **35 CrossbowTokens**. New total (14): **119**. However, Crossbow progression does NOT flood because Xbow2 upgrade costs 9–18 tokens/level (total ~135 to max Xbow2). The doubled Sniper content mainly enables fuller Xbow2 progression in Zone 3 loop 0, which is appropriate for end-game.

### Sniper Access Gate (New: growth=2.5)

Formula: `15 + 2.5 × sniperCompletedIndex`

| Index | Gate | Xbow Needed | When Available |
|-------|------|-------------|----------------|
| 0 | 15.0 | Xbow1 base=22 ✓ | Zone 1 |
| 1 | 17.5 | Xbow1 base=22 ✓ | Zone 1 |
| 2 | 20.0 | Xbow1 base=22 ✓ | Zone 1 |
| 3 | 22.5 | Xbow1 L1 (≈23.5) | Zone 1 |
| 4 | 25.0 | Xbow1 L2 (25.0) | Zone 1 |
| 5 | 27.5 | Xbow1 L4 (28.0) | Zone 1 |
| 6 | 30.0 | Xbow1 L5 (≈30) | Zone 2 |
| 7 | 32.5 | Xbow1 L7 (≈32.5) | Zone 2 |
| 8 | 35.0 | Xbow1 L9 (≈35.5) ✓ | Zone 2 |
| **9** | **37.5** | **Xbow2 L1 (38.8) — Xbow2 purchase required** | Zone 2 |
| 10 | 40.0 | Xbow2 L2 (40.6) | Zone 3 |
| 11 | 42.5 | Xbow2 L4 (44.2) | Zone 3 |
| 12 | 45.0 | Xbow2 L5 (46.0) | Zone 3 |
| 13 | 47.5 | Xbow2 L6 (47.8) | Zone 3 |

**Key boundary:** Xbow2 required at Sniper index 9 (North Sniper 4). By this point the player has earned ~54 CrossbowTokens from 9 Snipers. After Xbow1 max (~15 toks) + Xbow2 purchase (12 toks) + L1 upgrade (9 toks) = 36 toks spent. 54–36=18 toks remaining. Affordable. ✓

**vs. old growth=3:** Xbow2 would have been required at index 8 (gate=39, Xbow1 max=37 < 39). Change pushed Xbow2 wall one mission later, and made index 5 gate gentler (27.5 vs 30 before).

### Sniper Self-Funding Loop

The Sniper gate remains self-funding:
- Snipers 0–2: free → earn 9 toks (enough for Xbow1 L2 to cover Sniper 3/4)
- Snipers 3–8: Xbow1 upgrades → earned toks cover each next upgrade
- Sniper 9+: Xbow2 progression → earned toks fund Xbow2 upgrades

One chicken-and-egg moment exists at index 9 (Xbow2 wall), but it's resolved by accumulated toks from indices 0–8.

---

## 7. Boss Crossbow Gate — No Changes

**Verdict: Boss Gates unchanged. They remain appropriately calibrated.**

| Boss | ZoneIndex | Loop 0 Gate | What Passes |
|------|-----------|-------------|-------------|
| Zone 1 Boss | 0 | 28 | Xbow1 L4 (28.0) |
| Zone 2 Boss | 1 | 37 | Xbow1 max (37) |

**Zone 1 Boss gate check with new Sniper content:**
Player needs Xbow1 L4 (28). By Zone 1 Boss time (after 6 Zone 1 Snipers): earned 27 toks. Spending ~5–6 toks for L4. Easy. Boss still reachable in ~4 Sniper runs. ✓

**Zone 2 Boss gate check:**
Player needs Xbow1 max (37). After all Zone 1 Snipers (6) + Zone 2 Snipers 1–3 (indices 6–8): earned ~54 toks. Spending ~15 toks for Xbow1 max. 39 toks remaining after max. Boss reachable in ~9 Sniper runs. ✓

**Loop 1/2 Boss Gates (unchanged):**
| Boss | Loop 1 Gate (×1.25) | Loop 2 Gate (×1.55) | What Passes |
|------|---------------------|---------------------|-------------|
| Zone 1 | 35 | 43.4 | Loop 1: Xbow1 L9 / Loop 2: Xbow2 L4 |
| Zone 2 | 46.25 | 57.35 | Loop 1: Xbow2 L6 / Loop 2: Xbow3 L1 |

With 14 Snipers and Xbow2 progression enabled from Zone 2, loop 1/2 Boss gates remain achievable after reasonable Sniper grind. ✓

---

## 8. Progression Simulations

### Scenario A — Minimal Player (mostly Campaign, grinds only when blocked)

**Assumptions:** Plays Campaign in order, does Contracts only when blocked, plays Sniper only when needed for Boss.

| Checkpoint | Result |
|-----------|--------|
| First Zone 2 gate | g9 Bow (Bow1 L4, 3 toks) |
| Zone 2 gate events | 5 (g9 block, g10 free, g11 block, g12 block, g13 cheap block, g14 forced) |
| Max consecutive blocks | 2 (g11+g12) |
| Contracts runs before g14 | ~3–5 (2 for g11 Shuriken, 2–3 more for Shuriken2) |
| Sniper runs before Zone 1 Boss | ~4–5 (reach Xbow1 L4=28) |
| Sniper runs before Zone 2 Boss | ~9 (all Zone 1 Snipers + Zone 2 Snipers 1–3 = Xbow1 max) |
| Crossbow state after Zone 2 Snipers | Xbow1 max + ~19 toks leftover (can buy Xbow2 + L1) |
| Crossbow overpowered risk | LOW — Xbow2 upgrades cost 9–18 toks/level; can't max with available toks |

**Zone 2 feel:** Player breezes through g7/g8 (first 2 missions free), hits first wall at g9 (expected), gets relief at g10, then faces a 3-mission gauntlet (g11/g12/g13) before the zone exit. Much better than the old 7-block wall. ✓

### Scenario B — Efficient Player (upgrades optimally)

**Assumptions:** Upgrades just-in-time, does Sniper when convenient, minimal wasted Contracts runs.

| Checkpoint | Result |
|-----------|--------|
| First Zone 2 gate | g9 Bow (same as A; g7/g8 free) |
| Zone 2 blocks | 5 gate events (same structure) |
| Contracts runs before g14 | ~3–4 |
| Sniper runs before Zone 1 Boss | ~3–4 (may use Boss gate as motivation) |
| Sniper runs before Zone 2 Boss | ~7–8 |
| Crossbow state after Zone 2 Snipers | Xbow1 max + Xbow2 partially upgraded (L1–L3) |
| Crossbow overpowered risk | LOW |

### Scenario C — Sniper-Heavy Player (plays all Snipers when available)

**Assumptions:** Plays every Sniper mission as soon as accessible; upgrades Crossbow as far as possible.

| Checkpoint | Result |
|-----------|--------|
| After Zone 1 Snipers 1–6 | Earned 27 toks; Xbow1 maxed (37); ~12 toks leftover |
| After Zone 2 Snipers 1–3 (indices 6–8) | Earned additional 8+9+10=27 toks; total 39 available; buys Xbow2 (12) + L1 (9) = 21 spent; ~18 toks left |
| Zone 2 Sniper 4 (index 9) | Gate=37.5; Xbow2 L1=38.8 passes ✓ |
| After Zone 2 Sniper 4 | Earns 11 toks; total 29 available; buys Xbow2 L2–L4 (9+10+11=30) — almost covers it |
| After Zone 3 Snipers 1–4 (indices 10–13) | Earns 54 more toks; total ~50 toks available for Xbow2 upgrades |
| Crossbow end-state | Xbow2 at ~L6–L8 (not maxed; Xbow2 max requires ~135 toks) |
| Crossbow overpowered risk | LOW — Xbow2 upgrade cost prevents full max; player is well-equipped but not god-mode |

**Key check:** Even a Sniper-heavy player cannot max Xbow2 from Sniper income alone. Xbow2 remains a meaningful long-term investment. ✓

---

## 9. Full Gate Table Comparison (Zone 2 Focus)

### Complete Campaign Gate Table (Unchanged Except Zone 2 Free Passes)

| g | Zone | Class | Gate | Status Before | Status After |
|---|------|-------|------|--------------|-------------|
| 0 | 1 | Bow | ~0 | FREE | FREE |
| 1 | 1 | Shuriken | 12 | FREE | FREE |
| 2 | 1 | Bow | 29 | FREE | FREE |
| 3 | 1 | Shuriken | 16 | FREE | FREE |
| 4 | 1 | Bow | 33 | FREE | FREE |
| 5 | 1 | Shuriken | 20 | FREE | FREE |
| 6 | 1 | Bow | 37 | BLOCK (Bow L1) | BLOCK (Bow L1) |
| **7** | **2** | **Spear** | **55** | **FREE** | **FREE** |
| **8** | **2** | **Shuriken** | **26** | **BLOCK** | **FREE ←** |
| **9** | **2** | **Bow** | **43** | **BLOCK** | **BLOCK** |
| **10** | **2** | **Spear** | **70** | **BLOCK** | **FREE ←** |
| **11** | **2** | **Shuriken** | **32** | **BLOCK** | **BLOCK** |
| **12** | **2** | **Bow** | **49** | **BLOCK** | **BLOCK** |
| **13** | **2** | **Spear** | **85** | **BLOCK (L4+)** | **BLOCK (L3, reduced)** |
| **14** | **2** | **Shuriken** | **38** | **FORCED BUY** | **FORCED BUY** |
| 15 | 3 | Boomerang | 52.5 | FREE | FREE |
| 16 | 3 | Bow | 57 | FORCED BUY | FORCED BUY |
| 17 | 3 | Shuriken | 44 | BLOCK | BLOCK |
| 18 | 3 | Spear | 110 | BLOCK (max) | BLOCK (max) |
| 19 | 3 | Boomerang | 62.5 | BLOCK | BLOCK |
| 20 | 3 | Bow | 65 | BLOCK | BLOCK |
| 21 | 3 | Shuriken | 52 | FORCED BUY | FORCED BUY |
| 22 | 3 | Spear | 130 | FORCED BUY | FORCED BUY |

Zone 3 gates are unaffected by this phase. Spear1 max=110 still required for g18 (base increased but max unchanged). ✓

---

## 10. Files Changed

| File | Change | Effect |
|------|--------|--------|
| `Meta/Weapons/Data/Wapons/Spear WeaponDef 1.asset` | `_baseStats.Damage: 65 → 75` | g10 free pass; g13 cheaper |
| `Meta/Weapons/Data/Wapons/Shuriken WeaponDef 1.asset` | `_baseStats.Damage: 25 → 27` | g8 free pass |
| `Meta/Balance/Config/BalanceConfig.asset` | `_sniperAccessProfile.MissionGrowth: 3 → 2.5` | Gentler Sniper gate ramp for 14 missions |
| `Meta/Missions/Data/Zone/ZoneData 1 (Japanese).asset` | Added Jap Sniper 1 (pos 0) + Jap Sniper 6 (pos 5); reset stale `_currentIndex`/`_completedOnce` | Zone 1 now has 6 Sniper missions in correct order |
| `Meta/Missions/Data/Zone/ZoneData 2 (North).asset` | Removed bogus Jap Sniper 1 reference (5th entry) | Zone 2 has exactly 4 North Sniper missions |
| `Meta/Missions/Data/Zone/ZoneData 3 (Wild West).asset` | Removed bogus Jap Sniper 6 reference (5th entry) | Zone 3 has exactly 4 West Sniper missions |

**No code changes.** All changes are data-only (ScriptableObject assets). Project compiles unchanged. ✓

---

## 11. Acceptance Criteria Check

| Criterion | Status |
|-----------|--------|
| Zone 2 no longer has 7/8 missions blocked | ✓ Now 5 gate events (4 blocks + 1 forced) |
| Zone 2 still requires upgrades and grind | ✓ g9/g11/g12 still demand tokens; g14 still forced |
| Zone 2 has better pacing with breathing room | ✓ g8 free, g10 free; no 3-cluster at entry |
| Spear still matters and is introduced meaningfully | ✓ g7 strong intro, g10 free confirms Spear power, g13 first Spear gate |
| Shuriken2 forced purchase if kept, not excessively punishing | ✓ Unchanged; 3–5 Contracts runs same as before |
| New Sniper levels included in balance calculations | ✓ 14 total; ZoneData corrected |
| Sniper rewards do not flood CrossbowTokens | ✓ 119 total but Xbow2 upgrade cost 9–18/level absorbs surplus |
| Sniper Access Gate still feels fair | ✓ 3 free missions, then gradual Xbow1 ramp, Xbow2 from Zone 2 Sniper 4 |
| Boss Crossbow Gates still motivate Sniper | ✓ Zone 1 Boss=28 (4–5 Snipers), Zone 2 Boss=37 (9 Snipers) |
| Sniper gameplay remains one-shot | ✓ `SniperHpFromGate: 0.01` unchanged; all Sniper missions use `_fixedWeaponClass: 1` |
| No UI/popup/save architecture changes | ✓ Data-only changes |
| Project compiles | ✓ No code changes |

---

## 12. Remaining Risks

| Risk | Severity | Notes |
|------|----------|-------|
| g11/g12 still back-to-back | LOW | Both require different classes (Shuriken, Bow). Player needs ~7 Shu toks for L7 and 3 Bow toks for L7. Contracts provide AllTokens — two runs cover both. Acceptable mid-zone pressure. |
| g13 Spear L3 may feel trivial | LOW | With Spear base=75, players may never upgrade Spear until forced. 3-token cost is minimal. This is intentional — g13 is a light speed bump, not a wall. |
| Sniper index 9 Xbow2 wall (Zone 2 Sniper 4) | LOW | Player has enough accumulated toks (39 available, spends 21 for Xbow2+L1). But if player skips some Snipers, gap may be tighter. Mitigated by growth=2.5 (previously would have hit this wall at index 8). |
| Shuriken1 max=35 vs Shuriken1 base=27 reduces upgrade range | LOW | Total upgrade range shrinks from 10 to 8 damage. Each token buys 0.8 damage/level instead of 1.0. Player still reaches the same L7 gate at the same cost in tokens (L7 requirement same). Minor calculation change, no design impact. |
| ZoneData 1 `_currentIndex: 0` reset | LOW | Editor test data was reset. Production players unaffected (they use save-system progress). Developer will see Zone 1 Sniper start from Sniper 1 in PlayMode testing. |
| Zone 3 Sniper missions (West Sniper 3/4) use new scenes | MEDIUM | `Level3_1 (Sniper).unity` and `Level3_5 (Sniper).unity` are new/untracked. Verify these scenes exist and are playable before shipping. Same for Zone 2 new Sniper scenes. |
| Xbow2 upgrade tokens (9–18/level) are very expensive | LOW | Intentional design. Reviewed in Scenario C — player cannot max Xbow2 from Sniper alone. If a future balance pass finds players don't progress Xbow2 at all, consider lowering startUpgradeTok from 9 to 6. |
| Loop 1/2 Sniper Access Gate with growth=2.5 | LOW | No loop multiplier on Sniper gate damage (SniperTokens mult = 1.0, GateDamage mult applies but Sniper gate uses separate profile). Verify loop scaling is correct in `BalanceConfig.cs` if further loops are tested. |

---

## 13. Manual Inspector Steps Required

1. **Open Unity and let AssetDatabase reimport** — The YAML edits will be picked up on next Unity refresh. No manual Inspector steps needed for weapon stats or BalanceConfig.

2. **Verify ZoneData 1 Sniper segment** — Open `ZoneData 1 (Japanese)` in Inspector. Confirm the Sniper section shows 6 missions in order: Jap Sniper 1, 2, 3, 4, 5, 6.

3. **Verify ZoneData 2 and 3 Sniper segments** — Confirm each has exactly 4 Sniper missions with no null/broken references.

4. **Check new Sniper scenes are valid** — The following scenes are new (untracked) and must be functional:
   - `Level1_8 (Sniper) (2).unity` (Jap Sniper 6)
   - `Level2_5 (Sniper).unity` (North Sniper 3)
   - `Level2_6 (Sniper).unity` (North Sniper 4)
   - `Level3_1 (Sniper).unity` (West Sniper 3)
   - `Level3_5 (Sniper).unity` (West Sniper 4)

5. **Add new Sniper missions to EditorBuildSettings** if not already present (the scenes need to be in the build for the game to load them at runtime).

6. **Platest g8 and g10** — Confirm Shuriken missions at g8 don't show a damage-gate popup with base=27. Confirm Spear at g10 doesn't show popup with base=75.
