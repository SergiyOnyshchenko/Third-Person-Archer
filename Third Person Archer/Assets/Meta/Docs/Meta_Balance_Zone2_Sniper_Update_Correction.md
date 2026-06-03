# Meta Balance: Zone 2 + Sniper Update — Correction Pass

**Date:** 2026-06-01
**Scope:** ZoneData Sniper segment placement correction + balance re-verification
**Supersedes:** `Meta_Balance_Zone2_Sniper_Update.md` (Phase 5 analysis)

---

## What Happened

During Phase 5 (Zone 2 pacing + Sniper rebalance), GUID cross-referencing of Sniper mission assets revealed that:
- `ZoneData 2 (North)` contained a 5th Sniper entry with GUID `7d629bfad38c8334fb13db7375204e60` (= **Mission Jap Sniper 1**)
- `ZoneData 3 (Wild West)` contained a 5th Sniper entry with GUID `ff0c4061e43d4ea4a82aafd059bad1d4` (= **Mission Jap Sniper 6**)

These were incorrectly treated as ZoneData errors and removed. Phase 5 instead placed both missions in Zone 1's Sniper segment (making Zone 1 a 6-mission sniper zone), which was wrong.

## Designer Clarification

> *"Japanese zone has more Sniper missions produced than other zones can fill. Some Japanese Sniper missions are intentionally reused in later zones even if the visual theme doesn't perfectly match."*

**Intended design:**
- **Jap Sniper 1** → Zone 2 (positioned after North Sniper 4, as 5th Sniper in Zone 2)
- **Jap Sniper 6** → Zone 3 (positioned after West Sniper 4, as 5th Sniper in Zone 3)
- Zone 1 uses only Jap Sniper **2, 3, 4, 5** (4 missions, not 6)

---

## Changes Applied

### ZoneData 1 (Japanese) — Sniper segment
**Before (Phase 5 mistake):** Jap Sniper 1, Jap Sniper 2, 3, 4, 5, Jap Sniper 6 (6 missions)
**After (corrected):** Jap Sniper 2, 3, 4, 5 (4 missions)

Removed: position 0 (`7d629bfad38c8334fb13db7375204e60`) and position 5 (`ff0c4061e43d4ea4a82aafd059bad1d4`)

### ZoneData 2 (North) — Sniper segment
**Before (Phase 5 mistake):** North Sniper 1, 2, 3, 4 (4 missions)
**After (corrected):** North Sniper 1, 2, 3, 4, + Jap Sniper 1 (5 missions)

Added: `{fileID: 11400000, guid: 7d629bfad38c8334fb13db7375204e60, type: 2}` at end

### ZoneData 3 (Wild West) — Sniper segment
**Before (Phase 5 mistake):** West Sniper 1, 2, 3, 4 (4 missions)
**After (corrected):** West Sniper 1, 2, 3, 4, + Jap Sniper 6 (5 missions)

Added: `{fileID: 11400000, guid: ff0c4061e43d4ea4a82aafd059bad1d4, type: 2}` at end

---

## Final Sniper Layout

| Zone | Missions (in order) | Sniper Global Indices | Count |
|------|--------------------|-----------------------|-------|
| Zone 1 | Jap 2, Jap 3, Jap 4, Jap 5 | 0–3 | 4 |
| Zone 2 | North 1, North 2, North 3, North 4, Jap 1 | 4–8 | 5 |
| Zone 3 | West 1, West 2, West 3, West 4, Jap 6 | 9–13 | 5 |
| **Total** | | **0–13** | **14** |

---

## Balance Re-Verification

### Sniper Access Gate — growth = 2.5 (set in Phase 5, unchanged)

Formula: `gate(n) = 15 + 2.5 × n`

| Sniper Index | Zone | Gate Req | Xbow1 covers? | Xbow2 needed? |
|---|---|---|---|---|
| 0 (Jap 2) | Zone 1 | 15 | Xbow1 base=22 ✓ | — |
| 1 (Jap 3) | Zone 1 | 17.5 | ✓ | — |
| 2 (Jap 4) | Zone 1 | 20 | ✓ | — |
| 3 (Jap 5) | Zone 1 | 22.5 | Xbow1 base=22 (L1 needed) ✓ | — |
| 4 (North 1) | Zone 2 | 25 | Xbow1 L4=24 → L5=26 (minor grind) ✓ | — |
| 5 (North 2) | Zone 2 | 27.5 | Xbow1 L6=28 ✓ | — |
| 6 (North 3) | Zone 2 | 30 | Xbow1 L8=31 ✓ | — |
| 7 (North 4) | Zone 2 | 32.5 | Xbow1 L10=37 ✓ | — |
| 8 (Jap 1) | Zone 2 | 35 | Xbow1 max=37 ✓ | — |
| 9 (West 1) | Zone 3 | 37.5 | Xbow1 max=37 → needs Xbow2 | Xbow2 base=37 (borderline) → L1=38.8 ✓ |
| 10 (West 2) | Zone 3 | 40 | — | Xbow2 L2=40.6 ✓ |
| 11 (West 3) | Zone 3 | 42.5 | — | Xbow2 L3=42.4 ✓ |
| 12 (West 4) | Zone 3 | 45 | — | Xbow2 L4=44.2 → L5=46 ✓ |
| 13 (Jap 6) | Zone 3 | 47.5 | — | Xbow2 L5=46 → L6=47.8 ✓ |

**Result:** Zone 1 all Xbow1, Zone 2 all Xbow1 (max by end), Zone 3 forces Xbow2 entry (index 9). Clean progression gate.

### CrossbowToken Income Across All 14 Snipers

Formula: `reward(n) = 2 + 1 × n`

| Sniper Index | Tokens | Running Total |
|---|---|---|
| 0 | 2 | 2 |
| 1 | 3 | 5 |
| 2 | 4 | 9 |
| 3 | 5 | 14 |
| 4 | 6 | 20 |
| 5 | 7 | 27 |
| 6 | 8 | 35 |
| 7 | 9 | 44 |
| 8 | 10 | 54 |
| 9 | 11 | 65 |
| 10 | 12 | 77 |
| 11 | 13 | 90 |
| 12 | 14 | 104 |
| 13 | 15 | 119 |

**119 CrossbowTokens total from first loop.** Xbow2 costs 9+10+11+...+18 = 135 tokens to max. Player needs ~16 extra tokens from Campaign/Boss rewards to fully max Xbow2, which is consistent with multi-zone progression.

### Boss Gates — Unchanged and Still Valid

| Zone | Boss Gate | Required at unlock |
|------|-----------|-------------------|
| Zone 1 | 28 (Xbow1 L4) | After 7 Campaign missions, Xbow1 easily at L4+ |
| Zone 2 | 37 (Xbow1 max or Xbow2 base) | After 15 Campaign missions, reasonable gating |

No changes to Boss gate calculations. Phase 5 Zone 2 pacing fixes (Spear base 65→75, Shuriken base 25→27) remain in effect.

---

## GUID Reference

| Mission | GUID |
|---------|------|
| Jap Sniper 1 | `7d629bfad38c8334fb13db7375204e60` |
| Jap Sniper 2 | `748801458de35344a8ec6f0b3983f828` |
| Jap Sniper 3 | `5fa17e26b1da2f84390b5321dd2bffa7` |
| Jap Sniper 4 | `35d1a2578923aca4994bc6bcec2ddaef` |
| Jap Sniper 5 | `6cafd31b11d73ac4e8f58f0299c99fb1` |
| Jap Sniper 6 | `ff0c4061e43d4ea4a82aafd059bad1d4` |
| North Sniper 1 | `1fb5e9deceb3bee44b239a6a04aff4f8` |
| North Sniper 2 | `bb17ea095086eef4f9fd7120c733e88b` |
| North Sniper 3 | `d422915510ad9f1479f4028a2b9c3d67` |
| North Sniper 4 | `7642584bb1c66474dbabe5daf2b6444c` |
| West Sniper 1 | `381d0912cf2a1e84da686159d2388c4b` |
| West Sniper 2 | `03e440fe59dc8d24c8d50f9501a115ca` |
| West Sniper 3 | `3ecc7a8889ba66f4281187c891bbaa9c` |
| West Sniper 4 | `553bf127e37010d47aae1cc540bea8d6` |
