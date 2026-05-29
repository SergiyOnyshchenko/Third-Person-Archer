# Meta Core Clarity — Phase A

**Date:** 2026-05-28
**Branch:** french
**Based on:** Meta_GameDesign_Audit_RU.md, Meta_Player_Motivation_Ideas_RU.md
**Scope:** Gate popup clarity, first-time gate tutorials, loop transition screen

---

## Audit Findings

### 1. DamageGatePopupController — current state

File: `Assets/Meta/Missions/Gate/DamageGatePopupController.cs`

- Has `_hintText` (TMP), `_titleText` (TMP), current/required damage texts, close + go buttons.
- Has `_hintFormat` serialized string (default: `"Upgrade your {0}, or buy a new one."`)
- Has `_forcedPurchaseHintFormat` serialized string (default: `"Your {0} is fully upgraded. Buy a stronger weapon of the same class."`)
- Supports `CustomTitle` and `CustomHint` via `DamageGatePopupArgs` — overrides serialized defaults when non-null.
- `CustomHint` is also passed through `string.Format(hint, weaponClass.ToLowerInvariant())`.
- **No tutorial section** — no way to show one-time explanatory text below the hint.

### 2. PlayMissionPresenter — current state

File: `Assets/Meta/Missions/Controllers/PlayMissionPresenter.cs`

Three popup methods: `ShowDamageGatePopup`, `ShowSniperGatePopup`, `ShowBossGatePopup`.

| Gate | CustomTitle | CustomHint before Phase A |
|------|-------------|--------------------------|
| Campaign (normal) | none | none (uses `_hintFormat` default) |
| Campaign (forced purchase) | none | none (uses `_forcedPurchaseHintFormat` default) |
| Sniper (normal) | "Sniper access requires a stronger Crossbow" | "Upgrade your Crossbow to unlock this Sniper mission." |
| Sniper (forced purchase) | same | "Your Crossbow is fully upgraded. Buy a stronger Crossbow to continue." |
| Boss (normal) | "Boss mission requires a stronger Crossbow" | "Play Sniper missions to earn Crossbow tokens, then upgrade your Crossbow to unlock this Boss mission." |
| Boss (forced purchase) | same | "Your current Crossbow cannot reach the required power. Buy a stronger Crossbow to unlock this Boss mission." |

**Missing:** All hints lacked WHERE to get tokens (except Boss gate CanUpgrade=true which mentioned Sniper). No first-time tutorial logic anywhere.

### 3. DamageGatePopupArgs — current state

File: `Assets/Meta/Missions/Gate/DamageGatePopupArgs.cs`

Has: `WeaponClass`, `CampaignLevel`, `CurrentDamage`, `RequiredDamage`, `WeaponScreenId`, `CanUpgradeToPass`, `CustomTitle`, `CustomHint`.
**Missing:** No `TutorialText` field — no way to pass one-time tutorial content.

### 4. One-time flag storage — existing pattern

`MetaLoopProgressData` uses `SaveSystem.Load("meta_loop_current_loop", 0)` — JSON file at `Application.persistentDataPath`.
**Pattern to follow:** `SaveSystem.Load<bool>(key, false)` / `SaveSystem.Save<bool>(key, true)`.
No `PlayerPrefs` used in meta progression code.

### 5. Loop transition — current state

`MissionCompletionService.AdvanceToNextZoneOrLoop` is called ONLY from `MissionType.Boss` completion.
Zone 3 (Wild West) has NO Boss. Therefore `OnFullCampaignAndAllBossesCompleted()` is NEVER called from Zone 3 Campaign.
After Zone 3 Campaign is complete: player stays on Zone 3, no popup, no loopIndex increment.
There is NO current loop transition UI or trigger for Zone 3 Campaign completion.
`MetaLoopProgressData.CurrentLoopIndex` stays at 0 if Zone 3 has no Boss.

**Decision:** Implement UI-only check — detect Zone 3 Campaign complete on menu startup and show a one-time informational popup. Does NOT change any progression logic, does NOT increment loopIndex.

### 6. UINavigator — popup API

`nav.ShowPopup(popupId, args)` — instantiates popup from ScreenRegistry, calls `ApplyArgs` if args non-null and component implements `IReceivesArgs<T>`.
`LoopTransitionPopupController` uses `ShowPopup(_popupId, null)` — no args needed (all text serialized).

### 7. Files that need changes

| File | Change Type |
|------|------------|
| `DamageGatePopupArgs.cs` | Add `TutorialText` field |
| `DamageGatePopupController.cs` | Add `_tutorialRoot` / `_tutorialBodyText` fields + handling |
| `PlayMissionPresenter.cs` | Update all hint strings + add first-time tutorial flags |
| `LoopTransitionPopupController.cs` | NEW — simple popup for loop transition |
| `LoopTransitionPresenter.cs` | NEW — MonoBehaviour that checks condition and triggers popup |

---

## Changes Made

### 1. `DamageGatePopupArgs.cs`

Added `TutorialText` property (nullable string) and corresponding optional constructor parameter.

```
TutorialText = null  →  tutorial section hidden
TutorialText = "..."  →  tutorial section shown with that text
```

### 2. `DamageGatePopupController.cs`

Added two new serialized fields:
- `_tutorialRoot` (GameObject) — parent of the tutorial section; toggled active/inactive
- `_tutorialBodyText` (TextMeshProUGUI) — receives `args.TutorialText`

In `ApplyArgs`: tutorial root is activated only when `args.TutorialText` is non-null/non-empty.

### 3. `PlayMissionPresenter.cs`

**New save keys:**
- `"tutorial_first_campaign_gate"` — bool, true after first Campaign gate popup
- `"tutorial_first_boss_gate"` — bool, true after first Boss gate popup

**Loaded** in `Init()` from `SaveSystem`.

**Updated `ShowDamageGatePopup`:**
- Now passes `customHint` with token source info:
  - CanUpgrade=true: `"Upgrade your {Class} or buy a stronger one. {Class} Tokens can be earned from Campaign missions and Contracts."`
  - CanUpgrade=false: `"Your {Class} is at its maximum level. Buy a stronger {Class}. {Class} Tokens can be earned from Campaign missions and Contracts."`
- On first call: passes `tutorialText`:
  ```
  "TIP: Gates appear when your weapon damage is too low.
  Upgrade your weapon or buy a stronger one to continue.
  Weapon Tokens can be earned from Campaign missions and Contracts."
  ```
  Saves flag so tutorial is never shown again.

**Updated `ShowSniperGatePopup`:**
- CanUpgrade=true: `"Upgrade your Crossbow to access this Sniper mission. Crossbow Tokens can be earned from Sniper missions."`
- CanUpgrade=false: `"Your Crossbow is at its maximum level. Buy a stronger Crossbow. Crossbow Tokens can be earned from Sniper missions."`
- No first-time tutorial (Sniper gate is for experienced players who already know the system).

**Updated `ShowBossGatePopup`:**
- CanUpgrade=true: `"Boss missions require Crossbow Power. Play Sniper missions to earn Crossbow Tokens, upgrade your Crossbow, then return to the Boss."`
- CanUpgrade=false: `"Your Crossbow cannot reach the required power. Buy a stronger Crossbow. Crossbow Tokens can be earned from Sniper missions."`
- On first Boss gate: passes `tutorialText`:
  ```
  "TIP: Boss missions require Crossbow Power — not just any weapon.
  Play Sniper missions to earn Crossbow Tokens.
  Upgrade your Crossbow, then return to the Boss."
  ```
  Saves flag so tutorial is never shown again.

### 4. `LoopTransitionPopupController.cs` (new)

Path: `Assets/Meta/Missions/Gate/LoopTransitionPopupController.cs`

Simple popup MonoBehaviour:
- Serialized `_titleString` (default: `"Campaign Complete!"`)
- Serialized `_bodyString` (default: full explanation text)
- Sets TMP texts in `Awake()`; close button destroys the GameObject

**Does NOT implement `IReceivesArgs`** — all content is serialized in the prefab.

### 5. `LoopTransitionPresenter.cs` (new)

Path: `Assets/Meta/Missions/Gate/LoopTransitionPresenter.cs`

MonoBehaviour that:
1. Hooks `MainMenuRuntime.Ready` (or checks immediately if services exist)
2. Checks: `SaveSystem.Load("loop_transition_shown", false)` → skip if already shown
3. Checks: last zone has no Boss AND its Campaign is complete
4. Checks: all prior zones are fully complete (confirms full progression, not just Zone 3 Campaign alone)
5. If all pass: saves flag + calls `nav.ShowPopup(_popupId, null)`

Popup ID field: `_popupId = "LoopTransitionPopup"` (must match ScreenRegistry entry).

---

## Player-Facing English Texts Added

### Campaign gate hint (normal)
> Upgrade your {Class} or buy a stronger one. {Class} Tokens can be earned from Campaign missions and Contracts.

Example: "Upgrade your Bow or buy a stronger one. Bow Tokens can be earned from Campaign missions and Contracts."

### Campaign gate hint (forced purchase)
> Your {Class} is at its maximum level. Buy a stronger {Class}. {Class} Tokens can be earned from Campaign missions and Contracts.

Example: "Your Shuriken is at its maximum level. Buy a stronger Shuriken. Shuriken Tokens can be earned from Campaign missions and Contracts."

### First Campaign gate tutorial (shown once)
> TIP: Gates appear when your weapon damage is too low.
> Upgrade your weapon or buy a stronger one to continue.
> Weapon Tokens can be earned from Campaign missions and Contracts.

### Sniper gate hint (normal)
> Upgrade your Crossbow to access this Sniper mission. Crossbow Tokens can be earned from Sniper missions.

### Sniper gate hint (forced purchase)
> Your Crossbow is at its maximum level. Buy a stronger Crossbow. Crossbow Tokens can be earned from Sniper missions.

### Boss gate hint (normal)
> Boss missions require Crossbow Power. Play Sniper missions to earn Crossbow Tokens, upgrade your Crossbow, then return to the Boss.

### Boss gate hint (forced purchase)
> Your Crossbow cannot reach the required power. Buy a stronger Crossbow. Crossbow Tokens can be earned from Sniper missions.

### First Boss gate tutorial (shown once)
> TIP: Boss missions require Crossbow Power — not just any weapon.
> Play Sniper missions to earn Crossbow Tokens.
> Upgrade your Crossbow, then return to the Boss.

### Loop transition popup title
> Campaign Complete!

### Loop transition popup body
> You have cleared all three zones.
>
> A new challenge begins — gates are stronger, but rewards are better.
>
> Your weapons and progress carry over. Keep fighting!

---

## Files Changed

| File | Status |
|------|--------|
| `Assets/Meta/Missions/Gate/DamageGatePopupArgs.cs` | Modified |
| `Assets/Meta/Missions/Gate/DamageGatePopupController.cs` | Modified |
| `Assets/Meta/Missions/Controllers/PlayMissionPresenter.cs` | Modified |
| `Assets/Meta/Missions/Gate/LoopTransitionPopupController.cs` | New |
| `Assets/Meta/Missions/Gate/LoopTransitionPresenter.cs` | New |

---

## Assets / Prefabs — Manual Inspector Setup Required

### A. `Damage Gate Popup Screen Variant.prefab`

1. Open prefab in Unity.
2. Add a child GameObject named `TutorialRoot` (or similar).
3. Inside `TutorialRoot`, add a `TextMeshProUGUI` component for the tutorial body text.
4. On the `DamageGatePopupController` component:
   - Wire `_tutorialRoot` → the new `TutorialRoot` GameObject
   - Wire `_tutorialBodyText` → the `TextMeshProUGUI` inside it
5. The tutorial section should be **hidden by default** (set `TutorialRoot` inactive in prefab). The controller activates it only when `TutorialText` is non-null.
6. Style the tutorial section to look clearly distinct from the main hint (e.g., bordered box, lighter background, smaller font).

### B. New `Loop Transition Popup` prefab

1. Duplicate any existing simple popup prefab (e.g., `Popup Screen.prefab`).
2. Rename to `Loop Transition Popup Screen.prefab`.
3. Add the `LoopTransitionPopupController` component.
4. Wire: `_titleText`, `_bodyText`, `_closeButton`.
5. The default content strings are already set in the component — no need to set them in TMP fields at design time (they are set in `Awake`).

### C. Register in ScreenRegistry Popups

Open `Assets/Scripts/UI New/Prefabs/PopUps/!ScreenRegistry Popups.asset`.
Add entry:
- ID: `"LoopTransitionPopup"`
- Prefab: the new Loop Transition Popup prefab

### D. Add `LoopTransitionPresenter` to main menu scene

1. Open the main menu scene.
2. Add a new empty GameObject named `LoopTransitionPresenter`.
3. Attach `LoopTransitionPresenter` component.
4. Set `_popupId` to `"LoopTransitionPopup"`.

---

## Test Checklist

### Gate popup hints
- [ ] Campaign gate (normal): hint ends with "... Tokens can be earned from Campaign missions and Contracts."
- [ ] Campaign gate (forced purchase, e.g. Shuriken2): hint starts "Your Shuriken is at its maximum level..."
- [ ] Sniper gate: hint mentions "Crossbow Tokens can be earned from Sniper missions."
- [ ] Boss gate (CanUpgrade=true): hint mentions "Play Sniper missions to earn Crossbow Tokens."
- [ ] Boss gate (CanUpgrade=false): hint mentions "Crossbow Tokens can be earned from Sniper missions."

### First Campaign gate tutorial
- [ ] First time hitting a Campaign gate: tutorial section visible in popup with TIP text.
- [ ] Close popup, hit gate again: tutorial section NOT shown.
- [ ] Clear save data, start fresh: tutorial section visible on first gate hit again.

### First Boss gate tutorial
- [ ] First time hitting Boss Crossbow gate: tutorial section visible with Boss TIP text.
- [ ] Hit Boss gate again: tutorial section NOT shown.
- [ ] Tutorial text is different from Campaign gate tutorial text (Boss-specific).

### Loop transition popup
- [ ] Complete Zone 3 (Wild West) last Campaign mission, return to menu: "Campaign Complete!" popup appears.
- [ ] Dismiss popup, return to menu again: popup does NOT appear again.
- [ ] Popup body explains: gates stronger, rewards better, weapons carry over.
- [ ] Clear save data: popup reappears after Zone 3 Campaign is complete again.

### Regression
- [ ] Normal Campaign mission launch still works.
- [ ] Normal Boss launch (when gate passes) still works.
- [ ] Normal Sniper launch still works.
- [ ] Contracts still work.
- [ ] `DamageGatePopupController.ApplyArgs` does not throw when `TutorialText` is null.

---

## Risks and Limitations

### R1 — `_tutorialRoot` not wired in prefab
If `_tutorialRoot` is null (not wired in Inspector), the tutorial section simply doesn't appear. The controller handles this gracefully (`if (_tutorialRoot != null)`). No crash, but no tutorial either.

### R2 — Loop transition trigger requires Zone 3 Campaign complete
The `LoopTransitionPresenter` shows the popup only when Zone 3 Campaign is fully complete AND all prior zones' bosses are done. If the player's progress data is in an inconsistent state, the check may not trigger. Low risk in practice.

### R3 — Loop transition popup fires once globally
The `"loop_transition_shown"` flag is saved after the FIRST time conditions are met. If the game adds a Zone 4 later, this presenter would need to be updated to check the new last zone.

### R4 — LoopTransitionPresenter timing
The presenter hooks `MainMenuRuntime.Ready`. If the popup is triggered while the navigator is still initializing, `ServiceLocator.TryResolve<IUINavigator>` may fail silently. The popup would not appear that session but would appear on the NEXT session (flag is NOT saved if nav is unavailable). This is acceptable.

### R5 — Actual loop (gate multiplier) does not increment from Zone 3
`MetaLoopProgressData.CurrentLoopIndex` does NOT increment when Zone 3 Campaign completes (Zone 3 has no Boss). The popup informs the player that "a new challenge begins" but the loop multipliers only apply in the next loop triggered by a Boss. This is a known design limitation documented in Meta_GameDesign_Audit_RU.md (Section 3.4, Section 7.3). Phase A does NOT resolve this — it only adds UI clarity.

### R6 — SaveSystem.Load/Save with bool
`SaveSystem` wraps values in `Wrapper<T>` JSON. `bool` serializes correctly with Newtonsoft.Json. Consistent with existing usage in `MetaLoopProgressData`.

### R7 — No changes to balance or gates
Confirmed: no `BalanceConfig`, `MissionGateService`, `MissionAvailabilityService`, or `MissionCompletionService` changes in Phase A.

---

## Not Done in Phase A (Future Phases)

The following items from `Meta_Player_Motivation_Ideas_RU.md` are intentionally deferred:

- **Phase C:** First-time Contracts/Sniper/Boss-tab popups (items 1.1, 1.2, 1.3)
- **Phase C:** Spear and Boomerang weapon class unlock popups (items 1.4, 1.5)
- **Phase B:** Zone complete celebration + Boss victory celebration (items 7.1, 7.2)
- **Phase B:** Contracts milestone rewards (item 4.1)
- **Phase D:** Shortcut button in gate popup → goes directly to Contracts/Sniper
- **Phase D:** Sniper progress tracker showing Crossbow vs Boss gate requirement
- **Phase E:** Zone progress bar, mid-zone milestone
- **Phase E:** Loop badge indicator, loop narrative frame per loop index
