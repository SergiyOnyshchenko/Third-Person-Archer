# Meta Onboarding — Phase C

**Date:** 2026-05-28
**Branch:** french
**Based on:** Meta_Core_Clarity_PhaseA.md, Meta_Reward_Beats_PhaseB.md
**Scope:** First-time intro popups for Contracts, Sniper, Boss, Spear, Boomerang; forced purchase tutorial; mission type info cards

---

## Audit Findings

### 1. One-time flag storage pattern (Phase A)

`SaveSystem.Load<bool>(key, false)` / `SaveSystem.Save<bool>(key, true)`.
Keys stored in `Application.persistentDataPath` as JSON.
No `PlayerPrefs` used anywhere in meta code.
**Pattern confirmed. All Phase C flags use the same mechanism.**

### 2. Phase B celebration popup — reusable for all Phase C popups

`CelebrationPopupController` accepts `CelebrationPopupArgs(title, body)` via `IReceivesArgs<CelebrationPopupArgs>`.
Registered in ScreenRegistry as `"CelebrationPopup"`.
**All Phase C intro popups reuse this popup and this ID. No new popup controller or prefab needed.**

### 3. Contracts/Sniper unlock availability

`MissionAvailabilityService` checks `companyLevel >= _modeUnlocks.ContractsUnlockCompanyLevel` (and same for Sniper).
`MetaModeUnlockConfig.ContractsUnlockCompanyLevel` and `SniperUnlockCompanyLevel` default to `5`.
Company level = completed global campaign missions + 1 (via `MissionProgressData.GetCompanyLevel()`).
`MainMenuServices.ProgressData` exposes `GetCompanyLevel()` — accessible in any presenter.
**Unlock threshold must be serialized on each presenter to match MetaModeUnlockConfig. Default 5.**

### 4. Boss availability detection

`ZoneData.IsBossUnlocked()` returns true when `IsCampaignComplete()` is true and the zone has a Boss.
`services.ProgressData.Zone` is the current zone. `services.ProgressData.AllZones` is all zones.
`MissionProgressData.MissionType` is the currently selected mission type.
`ProgressData.OnMissionTypeChanged` is a `UnityEvent` that fires when the player switches tabs.
**Boss intro popup fires when player first selects Boss tab after IsBossUnlocked() → avoids same-session collision with ZoneCompletePresenter (Phase B).**

### 5. Spear/Boomerang first Campaign mission detection

`MissionSegmentData.GetCurrentMission()` returns the current active (not yet completed) mission in the segment.
`IWeaponRequirementService.GetRequiredWeaponClass(mission, loopIndex)` returns the effective weapon class for a mission at a given loop index.
`services.WeaponRequirement` is exposed on `MainMenuServices`.
`services.LoopData.CurrentLoopIndex` is the current loop index from `MetaLoopProgressData`.
**WeaponClassIntroPresenter checks current zone's current Campaign mission weapon class on startup.**

Existing `WeaponUnlockPopupsController` also fires class popups for Spear/Boomerang (says "New Weapon Class Unlocked" with icon). Phase C popups are separate and fire later when the class becomes required by Campaign — different content, different trigger.

### 6. Forced purchase gate result

`MissionGateResult.CanUpgradeToPass` (bool) — already computed by `MissionGateService` and exposed in the existing `MissionGateResult`. Already used in `PlayMissionPresenter.ShowDamageGatePopup`.
Phase A added `DamageGatePopupArgs.TutorialText` and `DamageGatePopupController._tutorialRoot/_tutorialBodyText` for one-time tutorial text in the damage gate popup.
**Phase C adds `FirstForcedPurchaseKey` check inside `ShowDamageGatePopup`. Forced purchase tutorial takes priority over the general first-gate tutorial when `CanUpgradeToPass == false`.**

### 7. Mission type tabs/buttons

`MissionTypeSelectionPresenter.SelectMissionType(MissionType)` handles tab selection.
No existing info/help button on mission type tabs. Info cards are a new addition.
**`MissionTypeInfoButton` MonoBehaviour placed on a Button GameObject near each tab. On click: shows `CelebrationPopup` with pre-written info text. No one-time flag — can be shown repeatedly.**

### 8. Files that need changes

| File | Change Type |
|------|------------|
| `Assets/Meta/Missions/Gate/ContractsUnlockPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/SniperUnlockPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/BossIntroPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/WeaponClassIntroPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/MissionTypeInfoButton.cs` | New |
| `Assets/Meta/Missions/Controllers/PlayMissionPresenter.cs` | Modified |

---

## Changes Made

### 1. `ContractsUnlockPresenter.cs` (new)

Pattern: `OnEnable` → if runtime ready call `CheckAndShow`; else subscribe to `Ready`.
Condition: `ProgressData.GetCompanyLevel() >= _contractsUnlockCompanyLevel && !SaveSystem.Load(SaveKey, false)`.
Action: saves flag, shows `CelebrationPopup` with contracts intro text.
Save key: `"contracts_unlock_popup_shown"`.

### 2. `SniperUnlockPresenter.cs` (new)

Same pattern as ContractsUnlockPresenter.
Condition: company level >= `_sniperUnlockCompanyLevel`.
Action: saves flag, shows `CelebrationPopup` with sniper intro text.
Save key: `"sniper_unlock_popup_shown"`.

### 3. `BossIntroPresenter.cs` (new)

Pattern: subscribes to `ProgressData.OnMissionTypeChanged` after Init.
Condition fires when tab changes: `MissionType == Boss && zone.IsBossUnlocked() && !SaveSystem.Load(saveKey, false)`.
Save key format: `"boss_intro_popup_shown_{zone.ID}"` — one key per zone.
Action: saves flag, shows `CelebrationPopup` with boss intro text including zone name.

**Why on tab change instead of startup:** avoids same-session collision with `ZoneCompletePresenter` (Phase B), which fires automatically on startup when Campaign is newly complete. Both would fire in the same session if both checked on startup. By firing on Boss tab selection, BossIntroPresenter fires only when the player intentionally navigates to the Boss tab.

### 4. `WeaponClassIntroPresenter.cs` (new)

Pattern: `OnEnable` → startup check (same as LoopTransitionPresenter / ZoneCompletePresenter).
Checks: current zone's Campaign segment's current active mission's effective weapon class.
If Spear → saves `"spear_intro_popup_shown"`, shows Spear intro.
If Boomerang → saves `"boomerang_intro_popup_shown"`, shows Boomerang intro.
Only one popup per startup (only one weapon class can be "current" at a time).

### 5. `MissionTypeInfoButton.cs` (new)

MonoBehaviour for an Info (?) button near each mission type tab.
Serialized `_missionType` field — set per instance in Inspector.
On click: `ServiceLocator.TryResolve<IUINavigator>().ShowPopup("CelebrationPopup", args)`.
Pre-written info text for all four types (Campaign, Contracts, Sniper, Boss).
No save flag — the info card can be shown repeatedly.

### 6. `PlayMissionPresenter.cs` (modified)

Added `FirstForcedPurchaseKey = "tutorial_first_forced_purchase_gate"` (string const).
Added `_firstForcedPurchaseShown` (bool field).
Loaded in `Init()` alongside existing flags.

Updated `ShowDamageGatePopup` tutorial logic (priority order):
1. If `!gate.CanUpgradeToPass && !_firstForcedPurchaseShown`:
   → forced purchase tutorial fires; also marks general gate tutorial shown
2. Else if `!_firstCampaignGateShown`:
   → general first-gate tutorial fires
3. Else: no tutorial

This ensures that if the player's very first gate is a forced purchase, they get the buy-a-new-weapon explanation rather than the generic upgrade explanation.

---

## Player-Facing English Texts Added

### Contracts Unlock popup

**Title:** `Contracts Unlocked!`

**Body:**
> Contracts are repeatable grind missions.
>
> Run any Campaign mission you have already completed. Every Contract run rewards tokens for ALL weapon classes — not just one.
>
> Rewards: Cash and tokens for every weapon class.
>
> Use Contracts regularly to keep your weapons upgraded and stay ahead of campaign gates.

---

### Sniper Unlock popup

**Title:** `Sniper Unlocked!`

**Body:**
> Sniper missions use the Crossbow — one precise shot to eliminate each target.
>
> The Sniper gameplay is different from Campaign: take your time, aim carefully, and make every shot count.
>
> Rewards: Cash and Crossbow Tokens.
>
> Crossbow Tokens are essential for upgrading your Crossbow — which you will need to unlock access to Sniper tiers and pass Boss Crossbow gates.

---

### Boss Intro popup

**Title:** `Boss Mission!`

**Body:**
> The Boss is the final challenge of {ZoneName}.
>
> Defeating the Boss opens the next zone and gives a large reward.
>
> Boss missions require Crossbow Power — not just any weapon. Play Sniper missions to earn Crossbow Tokens and prepare your Crossbow before you challenge the Boss.
>
> When ready, select the Boss tab and hit Play.

---

### Spear Missions Ahead popup

**Title:** `Spear Missions Ahead!`

**Body:**
> Your Campaign is entering Spear territory.
>
> The Spear is a high-damage weapon built for power — it hits hard and is effective against tough enemies.
>
> Upcoming Campaign missions will check your Spear damage. Upgrade your Spear to keep advancing.
>
> Spear Tokens can be earned from Campaign missions and Contracts.

---

### Boomerang Missions Ahead popup

**Title:** `Boomerang Missions Ahead!`

**Body:**
> Your Campaign is entering Boomerang territory.
>
> The Boomerang is a returning weapon — it launches, arcs back, and can hit multiple targets in one throw.
>
> Upcoming Campaign missions will check your Boomerang damage. Upgrade your Boomerang to keep advancing.
>
> Boomerang Tokens can be earned from Campaign missions and Contracts.

---

### First Forced Purchase tutorial (in DamageGatePopup TutorialText section)

> TIP: Your weapon is at its maximum level for this class.
> To pass this gate you need to BUY a higher-tier weapon of the same class.
> Open the weapon shop and look for a stronger option.

---

### Mission Type Info Cards

**Campaign info:**
> Campaign is the main story mode.
>
> Complete Campaign missions in order to progress through zones and unlock new content.
>
> Rewards: Cash and tokens for the weapon class required by that mission.
>
> Use Campaign to advance the story, unlock Contracts and Sniper modes, and unlock Boss fights at the end of each zone.

**Contracts info:**
> Contracts are repeatable grind missions.
>
> Play any Campaign mission you have already completed. Every Contract run rewards tokens for ALL weapon classes.
>
> Rewards: Cash and tokens for every weapon class.
>
> Use Contracts to stockpile tokens and upgrade weapons of any class — especially when preparing for tough gates or the Boss.

**Sniper info:**
> Sniper missions use the Crossbow — one precise shot to eliminate each target.
>
> The gameplay is different from Campaign: take your time, aim carefully, and make every shot count.
>
> Rewards: Cash and Crossbow Tokens.
>
> Use Sniper to upgrade your Crossbow and prepare for Boss missions, which require Crossbow Power.

**Boss info:**
> Boss is the final challenge of each zone.
>
> Defeating the Boss opens the next zone and gives a large reward.
>
> Boss missions require high Crossbow Power. Play Sniper missions to earn Crossbow Tokens before attempting the Boss.
>
> Rewards: Large cash bonus and zone progression to the next area.

---

## Save Keys / One-Time Flags Added

| Key | Type | Default | When set |
|-----|------|---------|----------|
| `"contracts_unlock_popup_shown"` | bool | false | First time Contracts become available |
| `"sniper_unlock_popup_shown"` | bool | false | First time Sniper becomes available |
| `"boss_intro_popup_shown_{zone.ID}"` | bool | false | First time Boss tab selected after Boss unlocked (per zone) |
| `"spear_intro_popup_shown"` | bool | false | First menu startup where Campaign mission requires Spear |
| `"boomerang_intro_popup_shown"` | bool | false | First menu startup where Campaign mission requires Boomerang |
| `"tutorial_first_forced_purchase_gate"` | bool | false | First Campaign gate hit where CanUpgradeToPass == false |

**Unchanged Phase A keys:** `"tutorial_first_campaign_gate"`, `"tutorial_first_boss_gate"`

---

## Files Changed

| File | Status |
|------|--------|
| `Assets/Meta/Missions/Gate/ContractsUnlockPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/SniperUnlockPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/BossIntroPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/WeaponClassIntroPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/MissionTypeInfoButton.cs` | New |
| `Assets/Meta/Missions/Controllers/PlayMissionPresenter.cs` | Modified (forced purchase tutorial) |
| `Assets/Meta/Docs/Meta_Onboarding_PhaseC.md` | New (this file) |

**Unchanged files (confirmed):**
- `DamageGatePopupArgs.cs` — no changes needed
- `DamageGatePopupController.cs` — no changes needed
- `BalanceConfig.cs` / `BalanceConfig.GateModule.cs` — no changes
- `MissionGateService.cs` / `MissionAvailabilityService.cs` — no changes
- `CelebrationPopupController.cs` — no changes; reused as-is

---

## Assets / Prefabs — Manual Inspector Setup Required

### A. Add presenter MonoBehaviours to main menu scene

Open `Assets/Scenes/MainMenu.unity`.

Add new empty GameObjects (can be parented under a single `OnboardingPresenters` parent):

| GameObject Name | Component | Inspector Fields |
|---|---|---|
| `ContractsUnlockPresenter` | `ContractsUnlockPresenter` | `_popupId = "CelebrationPopup"`, `_contractsUnlockCompanyLevel = 5` |
| `SniperUnlockPresenter` | `SniperUnlockPresenter` | `_popupId = "CelebrationPopup"`, `_sniperUnlockCompanyLevel = 5` |
| `BossIntroPresenter` | `BossIntroPresenter` | `_popupId = "CelebrationPopup"` |
| `WeaponClassIntroPresenter` | `WeaponClassIntroPresenter` | `_popupId = "CelebrationPopup"` |

**Important:** `_contractsUnlockCompanyLevel` and `_sniperUnlockCompanyLevel` must match the values in `MetaModeUnlockConfig`. Default is 5 for both. If MetaModeUnlockConfig values are changed in the future, update these fields to match.

### B. Add Info buttons to mission type tab UI

For each mission type tab (Campaign, Contracts, Sniper, Boss):
1. Add a small Button GameObject (e.g., a "?" icon button) near the tab in the scene or its prefab.
2. Attach `MissionTypeInfoButton` component.
3. Set `_missionType` to the matching `MissionType` enum value.
4. Set `_button` to the Button component (or leave null; `GetComponent<Button>()` is used as fallback).
5. Set `_popupId = "CelebrationPopup"`.

**No ScreenRegistry changes needed** — all info cards use the existing `"CelebrationPopup"` entry from Phase B.

### C. No new popup prefabs needed

All Phase C popups reuse `Celebration Popup Screen.prefab` (Phase B) via the `"CelebrationPopup"` ScreenRegistry ID. No new prefabs, no new registry entries.

---

## Test Checklist

### Contracts Unlock popup
- [ ] New save / complete Campaign mission 4 (→ company level 5): "Contracts Unlocked!" popup appears on next menu startup.
- [ ] Dismiss popup, return to menu again: popup does NOT appear again.
- [ ] Body explains repeatable missions, all-class tokens, and upgrade use.
- [ ] Clear save data: popup reappears on the first menu startup after level 5 is reached.

### Sniper Unlock popup
- [ ] New save / reach company level 5: "Sniper Unlocked!" popup appears on next menu startup.
- [ ] If Contracts and Sniper unlock on the same session: both popups appear (acceptable; see Risk R1).
- [ ] Dismiss, return: popup does NOT appear again.
- [ ] Body explains Crossbow gameplay, Crossbow Tokens, and Boss gate relevance.

### Boss Intro popup
- [ ] Complete Zone 1 Campaign, return to menu: Zone Campaign Complete popup (Phase B) fires, NOT the Boss Intro.
- [ ] Tap the Boss tab for the first time: "Boss Mission!" popup appears.
- [ ] Dismiss, tap Boss tab again: popup does NOT appear again.
- [ ] Same zone's Boss tab on a different session: popup does NOT appear again.
- [ ] Zone 2 Boss tab (after Zone 2 Campaign complete): popup fires once for Zone 2 as well (separate save key).
- [ ] Body includes zone name and explains Crossbow Power requirement.

### Spear intro popup
- [ ] Reach the first Campaign mission that requires Spear (based on BaseWeaponClass = Spear, loop 0).
  Return to menu after prior mission completed: "Spear Missions Ahead!" popup appears.
- [ ] Dismiss, return to menu: popup does NOT appear again.
- [ ] Body explains high-damage identity, gate requirement, and token source.

### Boomerang intro popup
- [ ] Same as Spear, for Boomerang class missions. "Boomerang Missions Ahead!" popup appears once.

### Forced purchase tutorial (Phase C item 6)
- [ ] On a fresh save, the first time a Campaign gate is hit with CanUpgradeToPass = false:
  DamageGatePopup shows the TutorialText section with the forced purchase TIP.
- [ ] On subsequent gate hits (CanUpgradeToPass = false): tutorial section NOT shown.
- [ ] If the player's very first Campaign gate is a normal gate (CanUpgradeToPass = true):
  The general gate tutorial fires (Phase A). Forced purchase tutorial fires separately later on first forced-purchase gate hit.
- [ ] If Phase A general gate tutorial has already been shown but Phase C forced purchase has not:
  Only the forced purchase tutorial fires on the first forced-purchase gate hit.
- [ ] `DamageGatePopupController._tutorialRoot` must be wired in prefab (Phase A setup) for the tutorial section to appear.

### Mission type info cards
- [ ] Info button near Campaign tab: tap shows Campaign info popup.
- [ ] Info button near Contracts tab: tap shows Contracts info popup.
- [ ] Info button near Sniper tab: tap shows Sniper info popup.
- [ ] Info button near Boss tab: tap shows Boss info popup.
- [ ] Info cards can be shown multiple times (no one-time flag).
- [ ] Popup text matches English texts in this document.
- [ ] Popup closes on button press (CelebrationPopupController destroys itself on close).

### Regression
- [ ] Campaign mission launch still works.
- [ ] Boss mission launch still works.
- [ ] Contracts mission launch still works.
- [ ] Sniper mission launch still works.
- [ ] Phase A DamageGatePopup tutorials (first campaign gate, first boss gate) still work.
- [ ] Phase B popups (ZoneComplete, BossVictory, ContractsMilestone, LoopTransition) still work.
- [ ] No null reference exceptions on menu startup.
- [ ] WeaponUnlockPopupsController Spear/Boomerang class unlock popups still fire correctly (separate system).

---

## Risks and Limitations

### R1 — Multiple popups in one session
If the player reaches company level 5 for the first time (unlocking both Contracts and Sniper) on the same menu session, two unlock popups appear in sequence. In normal play this requires both to be unlocked at the exact same threshold (default: both at 5). Acceptable risk — same as Phase B Risk R1.

### R2 — BossIntroPresenter fires on tab change, not startup
If the player exits the scene before ever tapping the Boss tab, the intro never fires. This is intentional — it fires when the player chooses to engage with the Boss. The Zone Campaign Complete popup (Phase B) already fires at startup to say "Boss is now unlocked."

### R3 — WeaponClassIntroPresenter loop rotation edge case
WeaponClassIntroPresenter checks the current Campaign mission's weapon class using `CurrentLoopIndex`. In loop 0, missions have fixed `BaseWeaponClass` values. In loop 1+, the class rotates by `rotationStep = 1`. If the player is in loop 1 and the current mission (originally Bow) rotates to Crossbow but a later mission (originally Spear) rotates to Shuriken, the Spear intro flag might never fire because the rotated classes don't match Spear. **This is acceptable** — weapon class intros are designed for loop 0 first-time players. In higher loops, the player already knows the mechanics.

### R4 — ContractsUnlockPresenter threshold serialized separately
`_contractsUnlockCompanyLevel` and `_sniperUnlockCompanyLevel` are serialized on the presenter MonoBehaviours, not linked to `MetaModeUnlockConfig`. If MetaModeUnlockConfig values are changed in the Inspector, the presenter fields must be manually updated to match. Low risk in practice (threshold rarely changes after shipping).

### R5 — Forced purchase tutorial requires prefab setup
`DamageGatePopupController._tutorialRoot` and `_tutorialBodyText` were added in Phase A and must be wired in the prefab. If not wired, the tutorial text is silently ignored (no crash). Detected via test checklist item "TutorialText section visible."

### R6 — No info buttons exist in the scene yet
`MissionTypeInfoButton` is a script only. The Info button GameObjects must be added manually to the main menu scene or the mission type tab prefab. Until done, the info cards are inaccessible to players. The script compiles cleanly regardless.

### R7 — WeaponClassIntroPresenter and WeaponUnlockPopupsController may fire in same session
When the player first reaches a Spear mission, `WeaponClassIntroPresenter` fires its "Spear Missions Ahead!" popup. `WeaponUnlockPopupsController` fires its "New Weapon Class Unlocked: Spear" popup separately. Both can appear in the same session. They serve different purposes (class availability vs. campaign context) and are shown from different systems. Acceptable — same as Phase B Risk R1.

### R8 — Navigator availability at menu startup
Same risk as Phase A (R4) / Phase B (R4). If `ServiceLocator.TryResolve<IUINavigator>` fails (navigator not yet registered), the popup is not shown but the flag IS already saved. The popup will never appear. Mitigation: navigator is registered in UINavigator.Awake(), which runs before MainMenuRuntime.Awake() if the UINavigator GameObject is earlier in scene order.

---

## Not Done in Phase C (Future Phases)

- **Phase D:** Shortcut button in gate popup → goes directly to Contracts/Sniper
- **Phase D:** Sniper progress tracker (Crossbow damage vs Boss gate requirement on Sniper tab UI)
- **Phase E:** Zone progress bar (how many Campaign missions left before Boss unlock)
- **Phase E:** Loop badge indicator (which loop the player is on)
- **Phase B follow-up:** Mid-zone reward beat for Zone 2
- **Phase B follow-up:** Contracts milestone currency rewards
