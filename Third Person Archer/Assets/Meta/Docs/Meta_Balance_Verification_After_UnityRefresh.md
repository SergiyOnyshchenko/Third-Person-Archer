# Meta Balance Verification — After Unity Refresh
**Date:** 2026-06-01
**Branch:** french
**Scope:** Read-only verification. No code, assets, or ZoneData were modified.

---

## Summary

The previous audit (Meta_Full_Balance_Audit_After_Zone3_Changes.md) reported broken ZoneData references for Zone 2 Camp 7/8 and Zone 3 Camp 7/8. The user reported these appeared resolved in the Unity Inspector after restarting Unity.

**This verification checked the raw YAML asset files directly (no Unity cache involved).**

**Result: The broken references are still present in the YAML. The duplicate GUID bug is unresolved. The Inspector display may have been misleading (Unity sometimes shows a stale or cached state for missing references).**

---

## 1. ZoneData Integrity Verification

### ZoneData 2 (North) — Campaign Segment

ZoneData 2 Campaign slot count: **7 entries** (expected: 8)

| Slot | Global Index | GUID in ZoneData | Matching Meta File | Status |
|------|-------------|------------------|--------------------|--------|
| 0 | g7 | `54cd21f1...` | Mission North Camp 1 | OK |
| 1 | g8 | `4478d580...` | Mission North Camp 2 | OK |
| 2 | g9 | `52a61d8e...` | Mission North Camp 3 | OK |
| 3 | g10 | `283a1b0e...` | Mission North Camp 4 | OK |
| 4 | g11 | `e0423b88...` | Mission North Camp 5 | OK |
| 5 | g12 | `91070c34...` | Mission North Camp 6 | OK |
| 6 | g13 | `77ffff496b5ffc242b4683ccefb2798e` | **NO MATCH** | BROKEN |
| 7 | g14 | *(not present in ZoneData)* | — | MISSING |

**North Camp 7 actual GUID:** `13ef53d1330095f498059ae0fee7ff11`
**North Camp 8 actual GUID:** `13ef53d1330095f498059ae0fee7ff11` ← **same as Camp 7 (OS copy bug)**

The GUID in ZoneData slot 6 (`77ffff496b5ffc242b4683ccefb2798e`) matches no existing meta file — it is an orphaned reference from before the OS copy event. ZoneData slot 7 is absent entirely.

**Verdict: g13 and g14 are unreachable at runtime. North Camp 7 and Camp 8 are both blocked.**

---

### ZoneData 3 (Wild West) — Campaign Segment

ZoneData 3 Campaign slot count: **8 entries** (correct)

| Slot | Global Index | GUID in ZoneData | Matching Meta File | Status |
|------|-------------|------------------|--------------------|--------|
| 0 | g15 | `a7ef7c0b...` | Mission West Camp 1 | OK |
| 1 | g16 | `96e001c9...` | Mission West Camp 2 | OK |
| 2 | g17 | `1b32acb7...` | Mission West Camp 3 | OK |
| 3 | g18 | `7be7fb18...` | Mission West Camp 4 | OK |
| 4 | g19 | `dfe03cc9...` | Mission West Camp 5 | OK |
| 5 | g20 | `91cdd233...` | Mission West Camp 6 | OK |
| 6 | g21 | `1d598891fe557274b88e5999455d8bd3` | **NO MATCH** | BROKEN |
| 7 | g22 | `508680ceaa7d9404389428edf081cb8f` | **NO MATCH** | BROKEN |

**West Camp 7 actual GUID:** `96cbd5b6a7a40b448a30c959d136ef5d`
**West Camp 8 actual GUID:** `96cbd5b6a7a40b448a30c959d136ef5d` ← **same as Camp 7 (OS copy bug)**

The GUIDs in ZoneData slots 6 and 7 match no existing meta file. They are orphaned references.

**Verdict: g21 (West Camp 7, Boomerang) and g22 (West Camp 8, Crossbow) are unreachable at runtime.**

---

### About the Inspector Appearance

Unity's Inspector resolves asset references lazily and can display a cached/stale result. The underlying YAML data is authoritative. The orphaned GUIDs would appear as `None (MissionData)` or `Missing` in a freshly-loaded Inspector, not as the correct missions. The bugs are not resolved.

---

## 2. Crossbow WeaponDef 4 (xbow_04)

**File:** `Assets/Meta/Weapons/Data/Wapons/Crossbow WeaponDef 4.asset`
**Display Name:** Golden Crossbow
**ID:** `xbow_04`

| Stat | Value |
|------|-------|
| Base Damage | **75** |
| Max Damage | **110** |
| Unlock After Campaign Level | 21 (= g21, West Camp 7) |
| Purchase Cash | 950 |
| Purchase Tokens | **23** |
| Upgrade Cash Range | 125 – 275 / level |
| Upgrade Token Range | 19 – 38 / level |
| Max Upgrade Level | 10 |

### Loop 2 g22 Gate Analysis

Loop multipliers (from BalanceConfig.asset):

| Loop Index | GateDamage Multiplier |
|------------|----------------------|
| 0 (first run) | 1.0 |
| 1 | 1.25 |
| 2 | 1.55 |

g22 base gate = 52.

| Loop | Effective Gate | Xbow3 max (80) | Xbow4 base (75) | Xbow4 L2 (~82) |
|------|---------------|----------------|-----------------|----------------|
| 0 | 52.0 | 80 ✓ | 75 ✓ | — |
| 1 | 65.0 | 80 ✓ | 75 ✓ | — |
| 2 | **80.6** | 80 ✗ (−0.6) | 75 ✗ | ~82 ✓ |

Xbow4 L2 approximate damage = 75 + 2 × (110 − 75) / 10 = **82** (linear estimate).

**Verdict: Loop 2 g22 is NOT impossible. It requires Xbow4 upgraded to approximately L2.** This is an appropriate endgame requirement for a third-loop final mission. The previous audit's flag of "IMPOSSIBLE without Xbow4" was correct in spirit — Xbow3 is insufficient at Loop 2 — but "impossible" was too strong; Xbow4 resolves it.

Note: Xbow4 unlocks after campaign level 21 (g21). Since g21 is currently unreachable due to the ZoneData bug, this is moot until the broken references are fixed.

---

## 3. Boomerang WeaponDef 2 (boomerang_02)

**File:** `Assets/Meta/Weapons/Data/Wapons/Boomerang WeaponDef 2.asset`
**Display Name:** Boomerang 2
**ID:** `boomerang_02`

| Stat | Value |
|------|-------|
| Base Damage | **80** |
| Max Damage | **130** |
| Unlock After Campaign Level | 28 |
| Purchase Cash | 1200 |
| Purchase Tokens | **30** |
| Upgrade Cash Range | 70 – 125 / level |
| Upgrade Token Range | 5 – 9 / level |
| Max Upgrade Level | 10 |

### Zone 3 Boomerang Gate Analysis

Boom1 base = 55, Boom1 max = 80.
Boom2 base = 80, Boom2 max = 130.

| Mission | Base Gate | Loop 0 (×1.0) | Loop 1 (×1.25) | Loop 2 (×1.55) |
|---------|-----------|----------------|-----------------|-----------------|
| g15 (Camp 1) | 52.5 | 52.5 — Boom1 base ✓ | 65.6 — Boom1 max ✓ | **81.4** — Boom2 L1 (~85) needed |
| g17 (Camp 3) | 57.5 | 57.5 — Boom1 L1+ | 71.9 — Boom1 max ✓ | **89.1** — Boom2 L2 (~90) needed |
| g19 (Camp 5) | 62.5 | 62.5 — Boom1 L3+ | 78.1 — Boom1 max ✓ | **96.9** — Boom2 L4 (~100) needed |
| g21 (Camp 7) | 67.5 | 67.5 — Boom1 L5+ | **84.4** — Boom2 needed | **104.6** — Boom2 L5 needed |

g21 is currently unreachable (broken ZoneData reference).

**Boom2 unlock timing:** Boom2 unlocks after campaign level 28. With 23 missions in Loop 0, level 28 falls approximately 5 missions into Loop 1 (equivalent to Zone 1 of Loop 1). This means Boom2 is available well before reaching Zone 3 in Loop 1.

**Verdict: Boomerang gates are reasonable.**
- Loop 0: Boom1 handles all reachable gates (g15–g20). Boom1 caps out before g21 but g21 is unreachable.
- Loop 1: Boom1 handles g15–g19. g21 (84.4) is the first Boom1 wall — Boom2 L1 (~85) resolves it. Boom2 is available by Loop 1 entry into Zone 3 given its campaign level 28 unlock.
- Loop 2: Boom2 required from g15 onward (81.4 > Boom1 max 80). Boom2 base (80) also falls just short — L1 (~85) is needed. Since Boom2 is long unlocked by Loop 2, this is an upgrade requirement, not an access wall.

**Potential concern:** Boom2 base (80) does not pass the Loop 2 g15 gate (81.4). A player who has just purchased Boom2 (unupgraded) will be blocked. They need at least L1 to proceed. This is a minor friction point but not a hard design problem.

---

## 4. Files and Assets Reviewed

| File | Purpose |
|------|---------|
| `ZoneData 2 (North).asset` | Campaign slot GUID cross-check |
| `ZoneData 3 (Wild West).asset` | Campaign slot GUID cross-check |
| `Mission North Camp [1-8].asset.meta` | GUID source of truth |
| `Mission West Camp [1-8].asset.meta` | GUID source of truth |
| `Crossbow WeaponDef 4.asset` | Xbow4 stats |
| `Boomerang WeaponDef 2.asset` | Boom2 stats |
| `BalanceConfig.asset` | Loop GateDamage multipliers |

---

## 5. Conclusion

| Finding | Status |
|---------|--------|
| ZoneData 2 broken reference (g13 / North Camp 7) | **CONFIRMED — still broken** |
| ZoneData 2 missing slot (g14 / North Camp 8) | **CONFIRMED — still missing** |
| ZoneData 3 broken references (g21/g22 West Camp 7/8) | **CONFIRMED — still broken** |
| Duplicate GUID bug (North Camp 7=8, West Camp 7=8) | **CONFIRMED — still present** |
| Loop 2 g22 gate impossible without Xbow4 | **CONFIRMED — requires Xbow4 ~L2, not impossible** |
| Loop 2 Zone 3 Boomerang needs Boom2 from g15 onward | **NEW FINDING — Boom2 L1 minimum** |
| No changes made | **CONFIRMED** |

**The Unity refresh did not fix any underlying data.** The duplicate GUID bug must be resolved in the Unity Editor (delete duplicate .meta file to force GUID regeneration, then re-link in ZoneData assets). Until then, g13, g14, g21, and g22 remain unreachable at runtime.
