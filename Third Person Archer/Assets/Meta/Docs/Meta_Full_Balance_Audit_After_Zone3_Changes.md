# Meta Full Balance Audit — After Zone 3 Campaign Changes

**Date:** 2026-06-01
**Auditor:** Claude Code (claude-sonnet-4-6)
**Based on prior docs:**
- `Meta_Balance_Zone2_Sniper_Update.md`
- `Meta_Balance_After_BossGate.md`
- `Meta_Boss_Crossbow_Gate.md`
- `Meta_Balance_Pacing_Validation.md`

**Audit type:** Read-only. No code, assets, or ZoneData were changed.

---

## 1. Audit Summary

### What Changed (Designer Changes This Phase)

Zone 3 Campaign mission weapon classes were reassigned:
- Boomerang now appears every other mission (4 of 8 slots, was 2 of 8).
- The final Zone 3 Campaign mission (West Camp 8) is now **Crossbow** (class 1), replacing the old Spear2 forced buy.
- As a result, Shuriken moved from g21 to g18, and Spear moved from g18/g22 to g20.

### Critical Findings

| Severity | Finding |
|----------|---------|
| CRITICAL | Zone 3 positions 7 and 8 (g21, g22) are broken references — the intended missions are unreachable |
| CRITICAL | Zone 2 position 7 is a broken reference and position 8 is missing entirely — g13 and g14 are unreachable |
| CRITICAL | West Camp 7 and West Camp 8 share a duplicate GUID in their `.meta` files |
| CRITICAL | North Camp 7 and North Camp 8 share a duplicate GUID in their `.meta` files |
| CRITICAL | Loop 2 makes the Crossbow final gate (g22) impossible: 52×1.55=80.6 > Xbow3 max=80 |
| MAJOR | Zone 3 now has 3 forced purchases in the first 6 working missions (Bow2, Shu2, Spear2) |
| MAJOR | Shuriken2 forced buy moved from g21 to g18 (3 missions earlier), creating a dense gate cluster early in Zone 3 |
| MAJOR | Spear2 forced buy at g20 is a new requirement that did not exist in the old design |
| MINOR | Crossbow g22 gate=52 is trivially passed by Xbow3 base=55 — gate value may be slightly low |
| MINOR | Boomerang L5 at g21 requires 5 Boomerang tokens without any Boomerang investment before Zone 3 |

### Overall Verdict

The designer's Boomerang intent is achieved: Zone 3 Boomerang pacing is now much stronger (4 missions, clean L1→L3→L5 ramp). The final Crossbow Campaign gate is economically viable and correctly rewards Sniper play.

However, two critical bugs prevent g21 and g22 from being reachable at all. Until the GUID/ZoneData issues are fixed, the Crossbow final mission cannot be experienced. Additionally, the new Zone 3 gate cluster (g16–g20) is denser and more expensive than the previous design, which may create frustration.

---

## 2. Files and Assets Reviewed

| File | Notes |
|------|-------|
| `ZoneData 1 (Japanese).asset` | 7 Campaign, 4 Sniper, 1 Boss — all valid GUIDs |
| `ZoneData 2 (North).asset` | 7 Campaign (1 broken ref, 1 missing), 4 Sniper, 1 Boss |
| `ZoneData 3 (Wild West).asset` | 8 Campaign (2 broken refs), 4 Sniper, no Boss |
| `Mission West Camp 1–8.asset` | All read; Camp 7 & 8 have duplicate GUID in meta files |
| `Mission North Camp 1–8.asset` | All read; Camp 7 & 8 have duplicate GUID in meta files |
| `Mission Jap Camp 1–7.asset` | Classes confirmed via file read |
| `BalanceConfig.asset` | Gate profiles, loop multipliers, reward profiles read |
| `Bow WeaponDef 1.asset` | base=35, max=55 |
| `Spear WeaponDef 1.asset` | base=75, max=110 |
| `Shuriken WeaponDef 1.asset` | base=27, max=35 |
| `Boomerang WeaponDef 1.asset` | base=55, max=80 |
| `Crossbow WeaponDef 1.asset` | base=22, max=37 |

---

## 3. ZoneData Integrity Audit

### Zone 1 (Japanese) — All Valid

| ZoneData Pos | GUID (partial) | Asset | Valid |
|-------------|---------------|-------|-------|
| Camp 1 | d0ad0740... | Jap Camp 1 | YES |
| Camp 2 | 540fd65b... | Jap Camp 2 | YES |
| Camp 3 | 0e78604e... | Jap Camp 3 | YES |
| Camp 4 | 917a56f5... | Jap Camp 4 | YES |
| Camp 5 | fbda53e0... | Jap Camp 5 | YES |
| Camp 6 | 32ce5469... | Jap Camp 6 | YES |
| Camp 7 | 640f578d... | Jap Camp 7 | YES |
| Sniper 1–4 | (Jap Sniper 2–5) | YES | YES |
| Boss | a5cf6cf3... | Jap Boss | YES |

**Zone 1: CLEAN. All references valid.**

### Zone 2 (North) — 2 Issues

| ZoneData Pos | GUID (partial) | Asset | Valid |
|-------------|---------------|-------|-------|
| Camp 1 | 54cd21f1... | North Camp 1 | YES |
| Camp 2 | 4478d580... | North Camp 2 | YES |
| Camp 3 | 52a61d8e... | North Camp 3 | YES |
| Camp 4 | 283a1b0e... | North Camp 4 | YES |
| Camp 5 | e0423b88... | North Camp 5 | YES |
| Camp 6 | 91070c34... | North Camp 6 | YES |
| Camp 7 | 77ffff49... | **NO MATCHING ASSET** | **BROKEN** |
| Camp 8 | (missing) | **NOT IN ZoneData** | **MISSING** |
| Sniper 1–4 | North Sniper 1–4 | YES | YES |
| Boss | 5456a8f1... | North Boss | YES |

**North Camp 7 meta GUID** = `13ef53d1330095f498059ae0fee7ff11`
**North Camp 8 meta GUID** = `13ef53d1330095f498059ae0fee7ff11` (DUPLICATE — same as Camp 7)
**ZoneData Camp 7 reference** = `77ffff496b5ffc242b4683ccefb2798e` (exists in NO meta file)

**Root cause:** North Camp 7 and 8 were likely created by OS-level file copy instead of Unity Editor duplication. The `.meta` files were duplicated with the same GUID. Unity assigned new internal GUIDs for the ZoneData edits, but those GUIDs were never matched to `.meta` files. North Camp 8 was never added to ZoneData at all.

### Zone 3 (Wild West) — 2 Issues

| ZoneData Pos | GUID (partial) | Asset | Valid |
|-------------|---------------|-------|-------|
| Camp 1 | a7ef7c0b... | West Camp 1 | YES |
| Camp 2 | 96e001c9... | West Camp 2 | YES |
| Camp 3 | 1b32acb7... | West Camp 3 | YES |
| Camp 4 | 7be7fb18... | West Camp 4 | YES |
| Camp 5 | dfe03cc9... | West Camp 5 | YES |
| Camp 6 | 91cdd233... | West Camp 6 | YES |
| Camp 7 | 1d598891... | **NO MATCHING ASSET** | **BROKEN** |
| Camp 8 | 508680ce... | **NO MATCHING ASSET** | **BROKEN** |
| Sniper 1–4 | West Sniper 1–4 | YES | YES |

**West Camp 7 meta GUID** = `96cbd5b6a7a40b448a30c959d136ef5d`
**West Camp 8 meta GUID** = `96cbd5b6a7a40b448a30c959d136ef5d` (DUPLICATE — same as Camp 7)
**ZoneData Camp 7 ref** = `1d598891fe557274b88e5999455d8bd3` (exists in NO meta file)
**ZoneData Camp 8 ref** = `508680ceaa7d9404389428edf081cb8f` (exists in NO meta file)

**Root cause:** Same pattern as Zone 2. West Camp 7 and 8 were OS-copied. Both `.meta` files share one GUID. ZoneData was updated to reference new intended GUIDs that were never written to `.meta` files.

**Impact:** The Boomerang mission at g21 and the final Crossbow Campaign mission at g22 are UNREACHABLE at runtime. Unity will encounter null references at these ZoneData positions.

---

## 4. Weapon Class Reference

| Class | Weapon | Gate Formula | Base | Max |
|-------|--------|-------------|------|-----|
| 0 | Bow | 25 + 2×g | 35 | 55 |
| 1 | Crossbow | 8 + 2×g | 22 (Xbow1) | 37 (Xbow1) |
| 2 | Spear | 20 + 5×g | 75 | 110 |
| 3 | Shuriken | 10 + 2×g | 27 | 35 |
| 4 | Boomerang | 15 + 2.5×g | 55 | 80 |

Crossbow extended: Xbow2 base=37 max=55 (purchase: 12 CrossbowTokens), Xbow3 base=55 max=80 (purchase: 17 CrossbowTokens, unlock@level 15).

Gate uses the global campaign index `g` (0-based, cumulative across all zones).

---

## 5. Full Campaign Gate Table — Loop 0

### Zone 1 (g0–g6)

| g | Zone Pos | Mission | Class | Gate | Best Weapon | Status |
|---|----------|---------|-------|------|-------------|--------|
| 0 | Z1-1 | Jap Camp 1 | Bow | 25 | Bow1 base=35 | FREE |
| 1 | Z1-2 | Jap Camp 2 | Shuriken | 12 | Shu1 base=27 | FREE |
| 2 | Z1-3 | Jap Camp 3 | Bow | 29 | Bow1 base=35 | FREE |
| 3 | Z1-4 | Jap Camp 4 | Shuriken | 16 | Shu1 base=27 | FREE |
| 4 | Z1-5 | Jap Camp 5 | Bow | 33 | Bow1 base=35 | FREE |
| 5 | Z1-6 | Jap Camp 6 | Shuriken | 20 | Shu1 base=27 | FREE |
| 6 | Z1-7 | Jap Camp 7 | Bow | 37 | Bow1 L1 (37) | BLOCK — 1 Bow tok |

Zone 1 gate events: **1 BLOCK** at the final mission. Excellent intro pacing.

### Zone 2 (g7–g14) — INTENDED (2 missions currently broken)

| g | Zone Pos | Mission | Class | Gate | Best Weapon | Status | ZoneData |
|---|----------|---------|-------|------|-------------|--------|----------|
| 7 | Z2-1 | North Camp 1 | Spear | 55 | Spear1 base=75 | FREE | OK |
| 8 | Z2-2 | North Camp 2 | Shuriken | 26 | Shu1 base=27 | FREE | OK |
| 9 | Z2-3 | North Camp 3 | Bow | 43 | Bow1 L4 (43) | BLOCK — 4 toks | OK |
| 10 | Z2-4 | North Camp 4 | Spear | 70 | Spear1 base=75 | FREE | OK |
| 11 | Z2-5 | North Camp 5 | Shuriken | 32 | Shu1 L7 (32.6) | BLOCK — 7 toks | OK |
| 12 | Z2-6 | North Camp 6 | Bow | 49 | Bow1 L7 (49) | BLOCK — 7 toks | OK |
| **13** | Z2-7 | North Camp 7 | Spear | 85 | Spear1 L3 (85.5) | BLOCK — 3 toks | **BROKEN** |
| **14** | Z2-8 | North Camp 8 | Shuriken | 38 | Shu2 (forced buy) | FORCED BUY | **MISSING** |

Zone 2 intended gate events: 4 BLOCKS + 1 FORCED BUY = 5.
Zone 2 currently playable gate events: 3 BLOCKS (g9, g11, g12 working).
**g13 and g14 are unreachable.** The designed zone exit gate (Shu2 forced buy at g14) is missing.

### Zone 3 (g15–g22) — INTENDED (2 missions currently broken)

| g | Zone Pos | Mission | Class | Gate | Best Weapon | Status | ZoneData | Change vs Old |
|---|----------|---------|-------|------|-------------|--------|----------|---------------|
| 15 | Z3-1 | West Camp 1 | Boomerang | 52.5 | Boom1 base=55 | FREE | OK | Same |
| 16 | Z3-2 | West Camp 2 | Bow | 57 | Bow2 (forced buy) | FORCED BUY | OK | Same |
| **17** | Z3-3 | West Camp 3 | **Boomerang** | **57.5** | Boom1 L1 (57.5) | **LIGHT BLOCK — 1 tok** | OK | **Was: Shu BLOCK** |
| **18** | Z3-4 | West Camp 4 | **Shuriken** | **46** | Shu2 (forced buy) | **FORCED BUY** | OK | **Was: Spear max BLOCK** |
| **19** | Z3-5 | West Camp 5 | Boomerang | 62.5 | Boom1 L3 (62.5) | BLOCK — 3 toks | OK | Same |
| **20** | Z3-6 | West Camp 6 | **Spear** | **120** | Spear2 (forced buy) | **FORCED BUY** | OK | **Was: Bow L7 BLOCK** |
| **21** | Z3-7 | West Camp 7 | **Boomerang** | **67.5** | Boom1 L5 (67.5) | **BLOCK — 5 toks** | **BROKEN** | **Was: Shu2 FORCED BUY** |
| **22** | Z3-8 | West Camp 8 | **Crossbow** | **52** | Xbow3 base=55 | **HARD GATE** | **BROKEN** | **Was: Spear2 FORCED BUY** |

Zone 3 intended gate events: 3 FORCED BUYS + 3 BLOCKS + 1 HARD GATE = 7 out of 8 missions.
Zone 3 currently playable gate events through g20: Forced Buy (g16), Light Block (g17), Forced Buy (g18), Block (g19), Forced Buy (g20).
**g21 and g22 are unreachable.**

---

## 6. Zone 3 Detailed Analysis

### 6.1 New Zone 3 Weapon Class Assignment

Old assignment: Boom, Bow, Shu, Spear, Boom, Bow, Shu, Spear
New assignment: **Boom, Bow, Boom, Shu, Boom, Spear, Boom, Xbow**

The old Zone 3 alternated between the two weapon pairs (Boom/Shu and Bow/Spear). The new Zone 3 places Boomerang every other mission (positions 1, 3, 5, 7) with Bow, Shuriken, Spear, Crossbow filling the even slots.

### 6.2 Boomerang Analysis

| g | Boom Mission | Gate | Level Needed | Prev Gate |
|---|-------------|------|-------------|-----------|
| 15 | Z3 Boom 1 | 52.5 | L0 (base=55 passes) | Same |
| 17 | Z3 Boom 2 | 57.5 | L1 (57.5) | NEW |
| 19 | Z3 Boom 3 | 62.5 | L3 (62.5) | Same (was at g19) |
| 21 | Z3 Boom 4 | 67.5 | L5 (67.5) | NEW [BROKEN] |

**Verdict — Boomerang pacing is significantly improved:**
- 4 Boomerang missions (was 2). 50% of Zone 3 is Boomerang.
- Gate arc: FREE → L1 → L3 → L5. Clean ramp with exactly +2 levels per step.
- Total Boomerang token cost to reach g21: 1+1+1+2+2 = 7 tokens (L5 cumulative).
- Each Boomerang mission reinforces the weapon without being a wall.
- The ramp feels intentional and satisfying — the designer's goal is achieved mechanically.

**Risk — No Boomerang before Zone 3:**
Boomerang1 is free and unlocks at campaign level 0, but players have never been required to use it before Zone 3. They arrive at Zone 3 with Boom1 at base and must upgrade it 5 levels across 7 missions. Total cost is modest (7 toks), but players may not realize Boomerang tokens come from Campaign and Contracts (all-token source). Onboarding for Boomerang intro popup (SpearIntroPresenter equivalent) becomes especially important.

**Risk — g21 is BROKEN:**
The 4th and final Boomerang gate (g21) is unreachable due to broken ZoneData. Boomerang's climax moment is currently inaccessible.

### 6.3 Shuriken2 Forced Buy — Moved to g18

**Old:** Shuriken2 forced buy at g21 (7th Zone 3 mission, late zone).
**New:** Shuriken2 forced buy at g18 (4th Zone 3 mission, early zone).

Shuriken1 max=35 < 46 (g18 gate). Shuriken2 must be purchased to proceed.

**Gate cluster formed:**
```
g16: FORCED BUY Bow2       — expensive (cash)
g17: LIGHT BLOCK Boom L1   — cheap (1 tok), just barely breaks the cluster
g18: FORCED BUY Shu2       — expensive (cash + Shu tokens)
```

Two forced purchases within 3 missions at Zone 3 entry is heavy. The original design spread forced buys across the zone (g16, g21, g22). Now they pile up at g16 and g18 with only a light gate between them.

**Consequence:** Players entering Zone 3 face: free intro (g15), wall (g16 Bow2), tiny relief (g17), wall (g18 Shu2). This doubles the "entry fee" for Zone 3 in the first 4 missions.

### 6.4 New Spear2 Forced Buy at g20

**Old g20:** Bow gate=65. Bow1 max=55 < 65, so this was Bow L7 block (~3 toks, no purchase).
**New g20:** Spear gate=120. Spear1 max=110 < 120 → **Spear2 forced purchase.**

This is an entirely new forced purchase that did not exist before. In the old design, Spear2 was required at g22 (last mission). Now it is required at g20 (6th mission of Zone 3, 2 missions earlier than the old Spear2 gate).

Combined with the Shu2 forced buy at g18, Zone 3 now has 3 forced purchases in the first 6 working missions:
- g16: Bow2
- g18: Shu2
- g20: Spear2

Previously Zone 3 had 3 forced purchases spread across all 8 missions (g16, g21, g22).

**Severity:** MAJOR. Zone 3 now requires 3 different weapon purchases before the Boomerang/Crossbow climax. The economy cost spike is substantial.

---

## 7. Crossbow Final Campaign Mission Analysis

### 7.1 Context

West Camp 8 has `_baseWeaponClass: 1` (Crossbow). This makes g22 a Campaign mission requiring Crossbow weapon class. This is intentional — the designer is using the final Zone 3 Campaign mission as a "hidden Boss substitute" to motivate Crossbow/Sniper progression.

### 7.2 Gate Calculation

Gate formula (Class 1, Crossbow): `8 + 2×g`
At g=22: `8 + 2×22 = 52`
DamageCap for Class 1: 120 (not reached at Loop 0).

Crossbow weapon chain:
- Xbow1 max=37 < 52 → BLOCKED
- Xbow2 base=37, max=55 → need level (52-37)/(55-37)×10 ≈ L8.3 → Xbow2 L9 needed (very expensive)
- Xbow3 base=55 > 52 → **FREE with Xbow3 purchase** ✓

**Practical path:** Buy Xbow3 (17 CrossbowTokens + 700 cash + level 15 unlock), pass with base. The gate is effectively a "buy Xbow3" gate rather than an upgrade gate.

### 7.3 Does Campaign Gate Logic Support Crossbow Correctly?

The Campaign gate uses the weapon class gate profile, which includes Class 1 (Crossbow) with BaseDamage=8 and MissionGrowth=2. There is no code reason Crossbow Campaign missions would behave differently from other classes. `_allowLoopRotation: 1` on West Camp 8 means loop rotation applies. In loops, the weapon class could rotate, so the loop analysis below addresses this.

### 7.4 Sniper Economy Check

CrossbowToken income (all 14 Snipers played):
| Phase | Snipers | Steps | Tokens |
|-------|---------|-------|--------|
| Zone 1 (indices 0–5) | 6 | 0–5 | 2+3+4+5+6+7 = 27 |
| Zone 2 (indices 6–9) | 4 | 6–9 | 8+9+10+11 = 38 |
| Zone 3 (indices 10–13) | 4 | 10–13 | 12+13+14+15 = 54 |
| **Total** | **14** | — | **119** |

Cost to unlock Xbow3 (minimum path):
| Step | Cost | Running Total |
|------|------|---------------|
| Xbow1 max (L1–L10) | 15 toks | 15 |
| Xbow2 purchase | 12 toks | 27 |
| Xbow2 L1 (for Sniper index 9 access) | 9 toks | 36 |
| Xbow3 purchase | 17 toks | 53 |

**Scenario A — Minimal Sniper player (only does Snipers for Boss gates):**
- Zone 1 Snipers 0–5: earns 27 toks
- Zone 2 Snipers 0–2 (indices 6–8): earns 8+9+10=27 more = 54 total
- Spends 15 toks (Xbow1 max for Zone 2 Boss gate)
- Remaining: 39 toks before Zone 3
- Can afford: Xbow2 buy (12) + Xbow3 buy (17) = 29 toks → 10 remaining
- **Xbow3 base=55 > 52. Gate PASSES. ✓**

**Scenario B — Efficient Sniper player (all Zone 1–2 Snipers, some Zone 3):**
- Before g22: earns 65–119 toks depending on Zone 3 Sniper progress
- After Xbow1 max + Xbow2 + Xbow3: 65–119 − 53 = 12–66 remaining
- Passes g22 easily, with tokens left for Xbow3 upgrades. ✓

**Scenario C — Sniper-heavy player (all 14 Snipers):**
- 119 toks total. After 53 for Xbow1 max + Xbow2 + Xbow3: 66 remaining.
- Can upgrade Xbow3 to L7–L8 (comfortable for Loop 1). ✓

**Key finding:** Even a minimal Sniper player who never does Zone 3 Snipers can afford Xbow3 and pass g22. The gate encourages Sniper engagement but does not mandate it. Appropriate.

### 7.5 Does it Create Good Motivation?

Yes. Zone 3 has no Boss. Previously the only Crossbow motivation in Zone 3 was Sniper rewards continuing to accumulate — but there was no hard gate requiring Crossbow power. The final Campaign mission at g22 now creates a concrete Crossbow milestone similar to how Boss gates work in Zones 1–2. Players who ignored Crossbow entirely will be stopped here and directed toward Sniper content.

**Caveat:** Currently the gate is unreachable due to GUID bugs. The motivation exists only on paper until the asset references are fixed.

---

## 8. Loop Analysis

BalanceConfig confirms: `_maxSupportedLoopIndex: 2`. Loops 0, 1, 2 are the full range. No Loop 3.

### Loop Multipliers

| Loop | CampaignDifficulty | GateDamage | EconomyCost | CampaignCash |
|------|-------------------|-----------|-------------|-------------|
| 0 | 1.0× | 1.00× | 1.0× | 1.0× |
| 1 | 1.3× | 1.25× | 1.0× | 1.1× |
| 2 | 1.5× | 1.55× | 1.0× | 1.2× |

Sniper/Contracts gate multipliers: not affected by GateDamage (Sniper uses `_sniperAccessProfile`, Boss uses `_bossAccessProfile`). Only Campaign mission gates scale with GateDamage.

### Loop 0 — Zone 3 Gates (Already Detailed in Section 5)

All gates pass with Boom1/Shu2/Spear2/Bow2/Xbow3 as outlined.

### Loop 1 — Zone 3 Gates (GateDamage ×1.25)

| g | Class | L0 Gate | L1 Gate | Min Weapon | Status |
|---|-------|---------|---------|-----------|--------|
| 15 | Boom | 52.5 | 65.6 | Boom1 L4 (65) → need L5 | BLOCK (upgraded) |
| 16 | Bow | 57 | 71.25 | Bow2 needed, with upgrades | BLOCK/FORCED BUY |
| 17 | Boom | 57.5 | 71.9 | Boom1 L7 (72.5) | BLOCK |
| 18 | Shu | 46 | 57.5 | Shu2 needed with upgrades | FORCED BUY+ |
| 19 | Boom | 62.5 | 78.1 | Boom1 L9 (79.0) | BLOCK |
| 20 | Spear | 120 | 150 | Spear2 needed with upgrades | FORCED BUY+ |
| 21 | Boom | 67.5 | 84.4 | Boom1 max=80 < 84.4 → Boom2 | FORCED BUY [BROKEN] |
| 22 | Xbow | 52 | 65 | Xbow3 L4 (≈66) | BLOCK (affordable) |

**Loop 1 g21 critical issue:** Boom1 max=80 < 84.4. Boomerang2 required. Boom2 is a new purchase that doesn't appear in Loop 0 at all. This is unexpected and expensive for Loop 1.

**Loop 1 g22:** Xbow3 L4 ≈ 55 + 25×0.4 = 65 > 65. Just passes. Manageable. ✓

### Loop 2 — Zone 3 Gates (GateDamage ×1.55)

| g | Class | L0 Gate | L2 Gate | Min Weapon | Status |
|---|-------|---------|---------|-----------|--------|
| 15 | Boom | 52.5 | 81.4 | Boom1 max=80 < 81.4 → Boom2 base | FORCED BUY |
| 16 | Bow | 57 | 88.4 | Bow2 upgrades | BLOCK/FORCED BUY |
| 17 | Boom | 57.5 | 89.1 | Boom2 upgrades | BLOCK |
| 18 | Shu | 46 | 71.3 | Shu2 upgrades | BLOCK |
| 19 | Boom | 62.5 | 96.9 | Boom2 upgrades | BLOCK |
| 20 | Spear | 120 | 186 | Spear2/3 needed | FORCED BUY+ |
| 21 | Boom | 67.5 | 104.6 | Boom2 upgrades | BLOCK [BROKEN] |
| **22** | **Xbow** | **52** | **80.6** | **Xbow3 max=80 < 80.6** | **IMPOSSIBLE** |

**CRITICAL — Loop 2 g22 is impossible with Xbow3:**
Gate = 52 × 1.55 = 80.6. DamageCap for Crossbow = 120, so cap does not apply.
Xbow3 max damage = 80. This is less than 80.6 by a margin of 0.6.
The gate cannot be passed with Xbow3 even at maximum upgrade. Xbow4 or higher is required.

Crossbow WeaponDef 4–7 exist in the project. If Xbow4 base ≥ 80.6, the gate is passable with Xbow4 purchase. If Xbow4 base < 80.6, it still requires some upgrade. This must be verified against Xbow4 data.

**Loop 2 g20 Spear:** 120 × 1.55 = 186. Spear1 max=110, Spear2 max is unknown but likely ~140–160. Gate=186 may require Spear3.

**Loop 2 g15 Boom:** Boom1 max=80 < 81.4. Boom2 required even for the first Zone 3 mission. This is a harsh loop escalation.

**Loops 1 and 2 require Boomerang2 purchases** that are never gated in Loop 0. The Boomerang chain has been elevated to a multi-weapon progression path just by adding 4 Boom missions to Zone 3 — but the economy for Boom2 tokens is not yet established.

---

## 9. Sniper Economy Analysis

### Sniper Gate Ramp (Growth=2.5, BaseDamage=15)

| Index | Zone | Sniper | Gate | Xbow Needed |
|-------|------|--------|------|-------------|
| 0 | Z1 | Jap Sniper 1 | 15.0 | Xbow1 base=22 ✓ |
| 1 | Z1 | Jap Sniper 2 | 17.5 | Xbow1 base=22 ✓ |
| 2 | Z1 | Jap Sniper 3 | 20.0 | Xbow1 base=22 ✓ |
| 3 | Z1 | Jap Sniper 4 | 22.5 | Xbow1 L1 (≈23.5) |
| 4 | Z1 | Jap Sniper 5 | 25.0 | Xbow1 L2 (≈25) |
| 5 | Z1 | Jap Sniper 6 | 27.5 | Xbow1 L4 (≈28) |
| 6 | Z2 | North Sniper 1 | 30.0 | Xbow1 L5 (≈30) |
| 7 | Z2 | North Sniper 2 | 32.5 | Xbow1 L7 (≈32.5) |
| 8 | Z2 | North Sniper 3 | 35.0 | Xbow1 L9 (≈35.5) |
| 9 | Z2 | North Sniper 4 | 37.5 | Xbow2 L1 (≈38.8) — Xbow2 required |
| 10 | Z3 | West Sniper 1 | 40.0 | Xbow2 L2 (≈40.6) |
| 11 | Z3 | West Sniper 2 | 42.5 | Xbow2 L4 (≈44.2) |
| 12 | Z3 | West Sniper 3 | 45.0 | Xbow2 L5 (≈46.0) |
| 13 | Z3 | West Sniper 4 | 47.5 | Xbow2 L6 (≈47.8) |

Sniper ramp unchanged from Phase 5. Self-funding loop confirmed: earnings from 9 Snipers (54 toks) cover Xbow1 max (15) + Xbow2 buy (12) + Xbow2 L1 (9) = 36 toks with 18 left over. ✓

Sniper economy with Zone 3 Crossbow gate is analyzed in Section 7. No changes needed to Sniper configuration.

### Boss Crossbow Gates (Unchanged)

| Boss | ZoneIndex | L0 Gate | L1 Gate (×1.25) | L2 Gate (×1.55) | What Passes |
|------|-----------|---------|-----------------|-----------------|-------------|
| Zone 1 Boss | 0 | 28 | 35.0 | 43.4 | L0:Xbow1 L4 / L1:Xbow1 L9 / L2:Xbow2 L4 |
| Zone 2 Boss | 1 | 37 | 46.25 | 57.35 | L0:Xbow1 max / L1:Xbow2 L6 / L2:Xbow3 L1 |

Boss gates remain correctly calibrated. No changes needed. ✓

---

## 10. Progression Simulations

### Scenario A — Minimal Player

Plays Campaign first. Farms Contracts/Sniper only when blocked.

| Checkpoint | State |
|-----------|-------|
| Zone 1 complete | Bow1 L1, Shu1 L0 (all free except g6 Bow L1) |
| Zone 1 Boss gate | Needs Xbow1 L4 (28). Does ~5 Snipers. Earns ~17 toks. ✓ |
| Zone 2 entry | Spear1 base=75 passes g7, Shu1 base passes g8. First block: g9 Bow L4. |
| Zone 2 blocks (intended) | 3 BLOCKS (g9, g11, g12) + g13 BROKEN + g14 MISSING |
| Zone 2 current state | Zone 2 ends at g12 due to broken refs. Blocks: g9, g11, g12. |
| Zone 2 Boss gate | Needs Xbow1 max=37. Has done ~9 Snipers: 27+38=65 toks cumulative. ✓ |
| Zone 3 entry state | Boom1 base, Bow2 needed (g16). Has ~39 toks remaining. |
| Zone 3 g16 | FORCED BUY Bow2 (cash). Blocked on cash, not tokens. |
| Zone 3 g17 | Boom L1 (1 Boom tok). Light block. |
| Zone 3 g18 | FORCED BUY Shu2 (cash + Shu tokens). Second expensive hit. |
| Zone 3 g19 | Boom L3 (2 more toks). Moderate. |
| Zone 3 g20 | FORCED BUY Spear2 (cash + Spear tokens). Third expensive hit. |
| Zone 3 g21 | BROKEN — unreachable. |
| Zone 3 g22 | BROKEN — unreachable. |
| Crossbow state at "Zone 3 end" | Xbow2+L1 for Sniper gate. Has ~30 toks. Can buy Xbow3 (17 toks) for g22 when fixed. ✓ |

**Simulation finding:** Current play experience ends at g20 due to bugs. Intended experience: 3 consecutive forced buys in Zone 3 before the Boomerang/Crossbow climax is heavy but achievable.

### Scenario B — Efficient Player

Upgrades optimally, does Sniper when convenient.

| Checkpoint | State |
|-----------|-------|
| Zone 2 intended complete | 5 gate events experienced (as designed). |
| Zone 3 first 6 missions | 3 forced buys (Bow2, Shu2, Spear2) experienced. Player expected to have cash reserves. |
| g22 Crossbow gate | Buys Xbow3 with ~39 remaining toks. Passes with base=55. 22 toks left. ✓ |
| Overpowered risk | LOW. Xbow3 upgrades cost significantly. Player cannot max Xbow3. |

### Scenario C — Sniper-Heavy Player

Plays every Sniper mission when available.

| Checkpoint | Crossbow State |
|-----------|--------------|
| After Zone 1 Snipers (6) | Xbow1 max (37). 12 toks remaining. |
| After Zone 2 Snipers (4) | Xbow2 purchased + L1. 30 toks remaining. |
| After Zone 3 Snipers (4) | 30+54=84 toks total available. Xbow3 bought + L3-L4 upgraded. |
| g22 gate=52 | Xbow3 base=55 passes trivially. Could also have Xbow2 very high. ✓ |
| Crossbow ceiling | Xbow3 at L3-L4. Not maxed. No overpowered concern. ✓ |

---

## 11. Problems Found

### CRITICAL

**P1 — Broken ZoneData References (Zone 2 g13, Zone 3 g21/g22)**

The most urgent issue. Four missions are unreachable:
- Zone 2: g13 (North Camp 7, Spear block) — broken ref
- Zone 2: g14 (North Camp 8, Shuriken2 forced buy) — missing entry
- Zone 3: g21 (West Camp 7, Boomerang L5 block) — broken ref
- Zone 3: g22 (West Camp 8, Crossbow final gate) — broken ref

**Evidence:** ZoneData GUIDs `77ffff49...`, `1d598891...`, `508680ce...` do not appear in any `.meta` file in the project. The missions West Camp 7/8 and North Camp 7/8 exist as files but their `.meta` files have stale/duplicate GUIDs.

**Affected files:**
- `ZoneData 2 (North).asset` — position 7 is broken, position 8 missing
- `ZoneData 3 (Wild West).asset` — positions 7 and 8 are broken
- `Mission West Camp 7.asset.meta` — shares GUID `96cbd5b6...` with West Camp 8
- `Mission West Camp 8.asset.meta` — same GUID as West Camp 7
- `Mission North Camp 7.asset.meta` — shares GUID `13ef53d1...` with North Camp 8
- `Mission North Camp 8.asset.meta` — same GUID as North Camp 7

**Recommended fix:**
1. Open Unity Editor. Unity may auto-regenerate GUIDs for the duplicate `.meta` files on import.
2. If not auto-resolved: delete the `.meta` files for West Camp 7, West Camp 8, North Camp 7, North Camp 8 from Windows Explorer. Unity will regenerate new unique GUIDs on next import.
3. Update `ZoneData 2 (North).asset` position 7 with the new North Camp 7 GUID; add position 8 with the new North Camp 8 GUID.
4. Update `ZoneData 3 (Wild West).asset` positions 7 and 8 with the new West Camp 7 and West Camp 8 GUIDs.
5. Verify in Inspector that all 8 mission slots are non-null.

**Risk:** LOW. The fix is mechanical GUID reassignment. Mission asset content is correct (classes are set). No balance values need to change for this fix.

---

**P2 — Loop 2 g22 Crossbow Gate Impossible**

At Loop 2, the Crossbow Campaign gate at g22 = 52 × 1.55 = **80.6**. Xbow3 max = **80**. This is 0.6 damage short. The gate cannot be passed with Xbow3 even at maximum upgrade. Xbow4 would be required.

**Evidence:**
- GateDamage loop 2 multiplier = 1.55 (confirmed from BalanceConfig.asset)
- Crossbow DamageCap = 120 (does not apply at 80.6)
- Xbow3 `_maxStats.Damage: 80` (confirmed from asset read)
- 80 < 80.6 → impossible

**Affected files:**
- `BalanceConfig.asset` (GateDamage Loop 2 multiplier)
- `Mission West Camp 8.asset` (Crossbow class gate)
- `Crossbow WeaponDef 3.asset` (Xbow3 max damage)
- `Crossbow WeaponDef 4.asset` (Xbow4 stats — not yet read; may resolve if base > 80.6)

**Recommended fix (option A):** Reduce the g22 gate base damage slightly. Current: `8 + 2×22 = 52`. If reduced to `7 + 2×22 = 51`, then Loop 2 gate = 51 × 1.55 = 79.05 < 80 (Xbow3 max). This requires lowering the Crossbow class `BaseDamage` from 8 to 7 globally, which affects all Crossbow Campaign gates across all zones (g22 is the only one, so impact is isolated). OR: change only the gate for this specific mission using `_useRewardOverride` equivalent for gates, if the system supports it.

**Recommended fix (option B):** Increase Xbow3 max damage by 1–2 points (82). This adds 1–2 damage room above the Loop 2 gate. Minor weapon buff with negligible gameplay impact.

**Recommended fix (option C):** Set `_allowLoopRotation: 0` and `_useFixedWeaponClass: 1` on West Camp 8. This freezes the weapon class to Crossbow regardless of loop. It also prevents the class from rotating in loops, ensuring the Crossbow gate always applies. Then the DamageCap may cap the scaled gate differently. Note: this changes loop behavior semantically.

**Risk:** MODERATE. Changing the gate formula affects the entire Crossbow class gate profile (affects Sniper access gate indirectly if the same profile is shared — verify code separation). Changing Xbow3 max is safer and more isolated.

---

### MAJOR

**P3 — Zone 3 Forced Buy Cluster Too Dense Early**

Zone 3 now has 3 forced purchases in the first 6 working missions:
- g16: Bow2 forced buy
- g17: Boom L1 (1 tok — only minor relief)
- g18: Shu2 forced buy
- g19: Boom L3 block
- g20: Spear2 forced buy

Two forced purchases (Bow2 and Shu2) are separated by only a 1-token Boomerang gate. Then a third forced purchase (Spear2) arrives at g20. The old design spread forced buys as g16, g21, g22 — one per cluster.

**Old Zone 3 cluster:**
```
g15: FREE (Boom)
g16: FORCED BUY Bow2
g17: BLOCK Shu
g18: BLOCK Spear max
g19: BLOCK Boom L3
g20: BLOCK Bow L7
g21: FORCED BUY Shu2
g22: FORCED BUY Spear2
```

**New Zone 3 cluster:**
```
g15: FREE (Boom)
g16: FORCED BUY Bow2
g17: LIGHT BLOCK Boom L1      ← very light
g18: FORCED BUY Shu2          ← 2nd forced buy very early
g19: BLOCK Boom L3
g20: FORCED BUY Spear2        ← 3rd forced buy mid-zone
g21: BLOCK Boom L5            [BROKEN]
g22: Crossbow hard gate       [BROKEN]
```

**Affected files:** Mission West Camp 3, 4, 5, 6 weapon classes.

**Recommended fix:** Consider moving the Shu2 forced buy back later, or inserting a heavier block between g16 and g18. For example, West Camp 3 could be a Shuriken or Bow block (requiring upgrades but not a purchase), and Shu2 could be moved to g20 or g21, with Spear2 moved to g21 or later. This spreads the Zone 3 forced purchase density more evenly.

**Risk:** MEDIUM. Changes require modifying weapon class assignments on mission assets, which triggers scene/gameplay changes. Requires coordination with level designer for thematic consistency.

---

**P4 — New Spear2 Forced Buy (g20) Was Not Planned**

The old Zone 3 end-game required Spear1 max (110) as a block at g18, then Spear2 as a forced buy at g22. The new design moves Spear to g20 (gate=120) with Spear1 max=110 < 120, creating a forced Spear2 buy at g20. The old Spear2-at-end design is now Spear2-in-middle.

This is a balance concern because:
1. Players must buy 3 weapon upgrades (Bow2, Shu2, Spear2) in Zone 3 before reaching the Crossbow climax.
2. The Spear2 forced buy is new — players who were not expecting it will be surprised.
3. Cash cost of Spear2 is unknown (needs verification with Spear WeaponDef 2 data), but all forced purchases together represent a significant Zone 3 economy barrier.

**Recommended fix:** Evaluate Spear2 purchase cost. If affordable alongside Bow2 and Shu2 in the Zone 3 economy, accept as intended. If too expensive in combined cash, consider lowering the g20 Spear gate to 109 (one below Spear1 max), making it a hard Spear1 max block rather than a forced Spear2 purchase.

**Risk:** LOW to MEDIUM. Changing gate value is a data-only change if done through BalanceConfig or mission override.

---

### MINOR

**P5 — Crossbow g22 Gate Slightly Low for Intent**

Gate=52 is passed trivially by Xbow3 base=55 (3 damage margin). The gate is effectively "buy Xbow3, pass immediately." Purchasing Xbow3 grants 3 damage headroom at Loop 0 with no need for upgrades.

A gate of 58 would require Xbow3 L1 (55+25×0.1=57.5, so L2 at 60 would pass). This makes Xbow3 slightly more meaningful — the player must invest 1–2 upgrades after purchase, establishing Xbow3 as a weapon worth upgrading, not just owning.

At 58, Loop 2 gate = 58×1.55 = 89.9. Xbow3 max=80 still fails — so P2 would still exist. But Xbow4 base (unknown) likely exceeds 89.9.

**Recommendation:** After fixing P2, revisit gate value. A range of 56–60 is reasonable.

---

**P6 — Boomerang Unlock Before Zone 3 Has No Teaching Gate**

Boomerang1 unlocks at campaign level 0 (free, always available), but is never required before Zone 3. Players arrive at Zone 3 with Boom1 at base, never having been incentivized to upgrade it. The g17 Boom L1 gate (1 tok) is very gentle, but some players may not realize they need to invest in Boom at all.

**Recommendation:** Verify BoomIntroPresenter (onboarding popup) fires correctly when Zone 3 is unlocked. If no intro popup exists for Boomerang at Zone 3 entry, consider adding one. This is an onboarding issue, not a balance issue.

---

**P7 — Loop 1 Requires Boomerang2 (Unexpected)**

At Loop 1, the Boomerang gate at g21 = 67.5 × 1.25 = 84.4 > Boom1 max (80). This introduces Boom2 as a requirement in Loop 1 without establishing Boom2 in Loop 0. Loop 1 may feel like an unexpected economy spike for Boomerang.

**Recommendation:** After fixing the Loop 2 Crossbow issue, evaluate whether Boom2 costs are reasonable in Loop 1 economy. If Boom2 is expensive, reduce the Boomerang gate ramp slightly (e.g., g21 moved to g20 slot with gate=67.5 instead of something higher).

---

## 12. Recommendations Summary

| # | Severity | Recommendation | Files Affected | Risk |
|---|----------|---------------|----------------|------|
| R1 | CRITICAL | Fix duplicate GUIDs in West/North Camp 7/8 meta files; update ZoneData 2 and 3 references | West Camp 7/8 .meta, North Camp 7/8 .meta, ZoneData 2, ZoneData 3 | LOW |
| R2 | CRITICAL | Fix Loop 2 Crossbow gate impossibility: either raise Xbow3 max by 1–2 damage, or reduce Crossbow BaseDamage from 8 to 7 | `BalanceConfig.asset` (Class 1 BaseDamage) OR `Crossbow WeaponDef 3.asset` (max damage) | MEDIUM |
| R3 | MAJOR | Spread Zone 3 forced buys: consider keeping Shu2 at g21 and inserting a cheaper block at g18 | Mission West Camp 4 (and 7) weapon class assignments | MEDIUM |
| R4 | MAJOR | Evaluate Spear2 gate at g20: consider reducing to 109 (Spear1 max block) if Spear2 cash cost is high | `Mission West Camp 6.asset` weapon class or gate value | LOW-MEDIUM |
| R5 | MINOR | After GUID fix, increase Crossbow g22 gate to 56–58 for more meaningful Xbow3 engagement | `Mission West Camp 8.asset` or BalanceConfig Crossbow BaseDamage | LOW |
| R6 | MINOR | Verify Boomerang onboarding popup fires at Zone 3 unlock | `BoomIntroPresenter` or equivalent | LOW |
| R7 | MINOR | Read Xbow4 stats; confirm Xbow4 base > 80.6 to validate Loop 2 Zone 3 end is solvable | `Crossbow WeaponDef 4.asset` | INFO |

---

## 13. Acceptance Criteria Check

| Criterion | Status |
|-----------|--------|
| No code/assets changed | PASS — audit only |
| Zone 3 Boomerang changes respected | PASS — Boomerang at g15/g17/g19/g21 respected; analysis confirms strong pacing |
| Final Zone 3 Crossbow mission respected | PASS — West Camp 8 Crossbow evaluated as valid design; gate economics confirmed workable |
| Japanese Sniper missions in later zones not treated as errors | PASS — Jap Sniper 1 in Zone 2 and Jap Sniper 6 in Zone 3 confirmed intentional from prior docs |
| Full balance recalculated for Zones 1–3 | PASS — all 23 intended Campaign missions gated and classified |
| Loops recalculated | PASS — Loop 0/1/2 analyzed; maxSupportedLoopIndex=2 confirmed |
| Final Crossbow mission evaluated against Sniper economy | PASS — Section 7 confirms gate is reachable for all player types |
| Boomerang presence evaluated | PASS — Section 6.2 confirms significantly improved pacing and ramp |
| Document created | PASS |

---

## 14. Recommended Next Balance Pass

**Phase 1 (Immediate — bug fix):**
- Fix GUID duplication and ZoneData broken references (R1). This unblocks all further testing.

**Phase 2 (Balance — after GUID fix):**
- Read Xbow4 stats (R7) to determine scope of Loop 2 Crossbow fix.
- Fix Loop 2 Crossbow gate impossibility (R2).

**Phase 3 (Pacing — optional):**
- Evaluate Zone 3 forced buy density (R3 and R4). If playtesting shows Zone 3 entry feels like a "purchase wall," re-spread forced buys.
- Adjust g22 gate value slightly upward (R5) for better Xbow3 engagement.

**Phase 4 (Polish):**
- Verify Boomerang intro popup (R6).
- Playtest Zone 3 under Loop 1/2 conditions once GUID fix is live.
