# Meta_Popup_Startup_Coordinator.md

**Date:** 2026-05-29
**Branch:** french
**Based on:** Meta_MainMenu_Popup_Startup_Audit.md, Meta_Core_Clarity_PhaseA.md, Meta_Reward_Beats_PhaseB.md, Meta_Onboarding_PhaseC.md

---

## 1. Audit Summary (from Meta_MainMenu_Popup_Startup_Audit.md)

Six startup presenters all called `nav.ShowPopup()` independently in their `OnEnable()` phase.
`UINavigator.ShowPopup()` has no single-active-popup rule: every call instantiates a new popup
instance under PopupsRoot. Multiple popups could stack visibly in the same frame.

**Critical scenarios:**
- ContractsUnlockPresenter + SniperUnlockPresenter both default to company level 5 → guaranteed
  collision every new player session.
- ZoneCompletePresenter + ContractsUnlockPresenter + SniperUnlockPresenter can all fire
  simultaneously on Zone 1 completion (up to 3 stacked celebration popups).
- WeaponUnlockPopupsController fired in `Start()` stacking on top of OnEnable celebration popups.
- Maximum case: 5 popups stacked in the same frame.

**Additional design issue:** Popups could open before the map screen was visible to the player
(same frame as scene startup).

---

## 2. Map Screen Detection Method

The map screen registry ID is **`"map"`** (verified in `Assets/Scripts/UI New/Prefabs/!ScreenRegistry.asset`).

The coordinator uses two complementary mechanisms to track when the map screen is active:

**Mechanism A — `IUINavigator.ScreenOpened` event:**
Fires when any full screen is opened via `nav.Open()`. When `view.ScreenId == "map"`, the
coordinator learns about the map screen and subscribes to its focus events. This covers
the initial navigation to the map screen.

**Mechanism B — `ScreenView.OnFocusGained` / `OnFocusLost`:**
`UINavigator.BackRoutine()` does NOT fire `ScreenOpened` for the screen that re-appears
after the top screen is popped. It does fire `newTop.OnFocusGained.Invoke()`.
By subscribing to the map screen's `OnFocusGained/Lost` events (captured once via
Mechanism A), the coordinator correctly detects when the player navigates back to the map.

**Conclusion:** Both mechanisms are needed. Together they cover all navigation paths:
- Initial open → ScreenOpened fires
- Navigate away (to weapons, loadout, etc.) → map's OnFocusLost fires
- Navigate back (GoBack) → map's OnFocusGained fires

The `_mapActive` flag is only true when the map screen is the current focused full screen.
The coordinator will not show any queued popup while `_mapActive = false`.

---

## 3. Coordinator Design

### Component

`StartupPopupCoordinator : MonoBehaviour, IStartupPopupCoordinator`

File: `Assets/Meta/Missions/Gate/StartupPopupCoordinator.cs`

Registered in `ServiceLocator` as `IStartupPopupCoordinator` during `Awake()`.
Marked `[DefaultExecutionOrder(-100)]` to ensure registration happens before any
default-order presenter's `Awake()` or `OnEnable()`.

### Interface

```
IStartupPopupCoordinator
    void Submit(StartupPopupRequest request)
```

File: `Assets/Meta/Missions/Gate/IStartupPopupCoordinator.cs`

### Request

```
StartupPopupRequest
    int    Priority           // lower = higher priority; use StartupPopupPriority constants
    string LogicalId          // deduplication key (optional)
    string PopupId            // ScreenRegistry ID; ignored if CustomShowAction is set
    object Args               // IReceivesArgs args; null is valid
    Action OnBeforeShow       // called immediately before display; write one-time save flag here
    Action CustomShowAction   // optional override: called instead of nav.ShowPopup(PopupId, Args)
```

File: `Assets/Meta/Missions/Gate/StartupPopupRequest.cs`

### Priority constants

```
StartupPopupPriority.LoopTransition     = 10   (shown first)
StartupPopupPriority.BossVictory        = 20
StartupPopupPriority.ZoneComplete       = 30
StartupPopupPriority.ContractsUnlock    = 40
StartupPopupPriority.SniperUnlock       = 50
StartupPopupPriority.WeaponClassUnlock  = 60
StartupPopupPriority.WeaponUnlock       = 70
StartupPopupPriority.ContractsMilestone = 80   (shown last)
```

File: `Assets/Meta/Missions/Gate/StartupPopupPriority.cs`

### Internal state

- `_pending` — `List<StartupPopupRequest>` kept sorted in ascending priority order (index 0 = next to show). Equal-priority requests are ordered by submission time (stable insert).
- `_pendingIds` — `HashSet<string>` of LogicalIds currently in `_pending`, for O(1) deduplication.
- `_popupActive` — true while waiting for the current popup to close.
- `_mapActive` — true when map screen has focus.
- `_awaitingPopupShown` — true between calling `ShowRequest()` and receiving `PopupShown` event.
- `_mapScreenView` — captured `ScreenView` of the map screen; used to subscribe to focus events.

### Show flow

```
Submit(request)
  → insert into _pending (sorted by priority)
  → TryShowNext()

TryShowNext()
  → guard: _popupActive || !_mapActive || empty queue || _awaitingPopupShown → return
  → dequeue head request
  → ShowRequest(request)

ShowRequest(request)
  → _popupActive = true
  → _awaitingPopupShown = true
  → request.OnBeforeShow?.Invoke()          ← save the one-time flag here
  → subscribe to nav.PopupShown (once)
  → call CustomShowAction OR nav.ShowPopup(PopupId, Args)
  → [nav internally enqueues ShowPopupRoutine; may run immediately or next frame]

OnPopupShownForCurrentRequest(view)         ← fires when nav actually shows the popup
  → unsubscribe from nav.PopupShown
  → _awaitingPopupShown = false
  → StartCoroutine(WaitForClose(view))

WaitForClose(view)
  → yield return null each frame while view != null
  → popup was destroyed by its own close button handler
  → _popupActive = false
  → TryShowNext()                           ← show next queued popup if any
```

### PopupShown timing

`UINavigator.ShowPopup()` → `Enqueue(ShowPopupRoutine)`. If the navigator's internal RunQueue
is currently processing another routine (e.g., the map screen open transition), `ShowPopupRoutine`
is queued and runs in the next RunQueue iteration (same or next frame).  `PopupShown` therefore
fires asynchronously relative to the `Submit()` call.

The coordinator keeps its `nav.PopupShown` subscription open until the event fires.  It does NOT
assume synchronous execution.  `_awaitingPopupShown` prevents `TryShowNext()` from double-submitting
while waiting.

---

## 4. Final Priority Order

| Priority | Popup type                | Presenter                    | Rationale |
|----------|---------------------------|------------------------------|-----------|
| 10       | Loop Transition           | LoopTransitionPresenter      | Rare, once-ever milestone; most important context |
| 20       | Boss Defeated             | BossVictoryPresenter         | Zone-major unlock, explains next zone |
| 30       | Zone Campaign Complete    | ZoneCompletePresenter        | Gate to boss; player needs to understand next step |
| 40       | Contracts Unlocked        | ContractsUnlockPresenter     | Feature intro; shown once |
| 50       | Sniper Unlocked           | SniperUnlockPresenter        | Feature intro; shown once |
| 60       | Weapon Class Unlocked     | WeaponUnlockPopupsController | New gameplay tool; shown once per class |
| 70       | Weapon Unlocked           | WeaponUnlockPopupsController | New item; shown once per weapon |
| 80       | Contracts Milestone       | ContractsMilestonePresenter  | Repeating reward beat; lowest urgency |

---

## 5. Changed Presenters

### LoopTransitionPresenter
**Before:** called `nav.ShowPopup()` directly, saved flag before show.
**After:** submits `StartupPopupRequest` with `Priority=10`, `LogicalId="loop_transition_shown"`,
`OnBeforeShow = () => SaveSystem.Save("loop_transition_shown", true)`.
**Fallback:** if no coordinator, saves flag and calls `nav.ShowPopup()` directly (original behaviour).

### ZoneCompletePresenter
**Before:** saved flag + called `nav.ShowPopup()` per zone, broke after first.
**After:** submits one coordinator request per session (first unclaimed zone), with captured
zone-specific `saveKey` in `OnBeforeShow` lambda.
**Fallback:** same as before if coordinator absent.

### BossVictoryPresenter
**Before:** saved flag + called `nav.ShowPopup()` per zone boss, broke after first.
**After:** submits one coordinator request (first unclaimed boss zone), with zone-specific
`capturedKey` in `OnBeforeShow` lambda.
**Fallback:** same as before if coordinator absent.

### ContractsMilestonePresenter
**Before:** saved `targetMilestone` as int + called `nav.ShowPopup()`.
**After:** `OnBeforeShow = () => SaveSystem.Save(SaveKey, capturedMilestone)`.
`LogicalId = "contracts_milestone_{N}"` for deduplication.
**Fallback:** same as before if coordinator absent.

### ContractsUnlockPresenter
**Before:** saved bool flag + called `nav.ShowPopup()`.
**After:** `OnBeforeShow = () => SaveSystem.Save(SaveKey, true)`.
**Fallback:** same as before if coordinator absent.

### SniperUnlockPresenter
**Before:** saved bool flag + called `nav.ShowPopup()`.
**After:** `OnBeforeShow = () => SaveSystem.Save(SaveKey, true)`.
**Fallback:** same as before if coordinator absent.

### WeaponUnlockPopupsController
**Before:** `Start()` called `RebuildAndStart()` → `ShowNextIfAny()` immediately.
**After:** `Start()` builds the queue, then if queue is non-empty, submits ONE coordinator
request with `CustomShowAction = ShowNextIfAny`. Priority is `WeaponClassUnlock(60)` or
`WeaponUnlock(70)` based on the type of the first item in the queue.
`RebuildAndStart()` is unchanged — still calls `ShowNextIfAny()` directly for mid-session
refreshes (e.g. after a weapon upgrade), bypassing the coordinator intentionally.
**Fallback:** if coordinator absent, calls `ShowNextIfAny()` directly.

### BossIntroPresenter
**Not changed.** Does not fire on startup — fires on `OnMissionTypeChanged` (Boss tab selected).
Not a startup collision source.

### PlayMissionPresenter
**Not changed.** Fires only on Play button click. Not a startup popup.

---

## 6. Save Flag Behaviour

### Old behaviour (before this phase)
Presenters called `SaveSystem.Save(key, value)` BEFORE `nav.ShowPopup()`.
Risk: if `nav` was unavailable, the flag was saved but the popup was never shown → popup lost.

### New behaviour
`OnBeforeShow` is invoked by the coordinator immediately before it calls `nav.ShowPopup()` (or
`CustomShowAction`).  This means:

- Flag is saved only when the popup is actually about to be displayed.
- If the game is closed before the map screen opens (coordinator never reaches that request),
  the flag is NOT saved → popup is re-checked and re-submitted next session. ✓
- If the game crashes between `OnBeforeShow` and the popup appearing on screen, the flag is
  saved but the player did not see the popup. This is the same risk as the old behaviour, and
  is considered acceptable (extremely rare).
- If the coordinator never shows the popup (e.g., coordinator destroyed before map opens),
  the flag is not saved and the popup will be attempted again next session. ✓

### Popup NOT marked as shown at queue time
Presenters check the save flag BEFORE submitting to the coordinator.  If the flag is already
true, no request is submitted.  The coordinator does NOT mark anything at queue time.

This means: if the same condition is triggered in two consecutive sessions without the popup
ever being shown (because the map screen was never opened), the popup is attempted again each
time.  This is intentional — the popup should eventually be seen.

---

## 7. Popup Close / Continue Behaviour

### Celebration and loop-transition popups
`CelebrationPopupController` and `LoopTransitionPopupController` close themselves via
`Destroy(gameObject)` on close-button press.  The `WaitForClose` coroutine in the coordinator
polls `popup != null` each frame.  When the popup is destroyed, `WaitForClose` exits, sets
`_popupActive = false`, and calls `TryShowNext()`.  If another request is queued and the map
screen is active, the next popup is shown immediately in the same frame.

### Weapon unlock popups
The first weapon popup is shown via `CustomShowAction = ShowNextIfAny`.  When the player
closes the first popup, `WeaponClassUnlockedPopupScreen.OnClose()` calls
`IWeaponUnlockPopupFlow.CloseAndContinue(this)`, which calls `ShowNextIfAny()` directly.
The coordinator's `WaitForClose` detects the first popup destroyed and sets `_popupActive = false`,
then tries `TryShowNext()` → finds empty coordinator queue → does nothing.
`WeaponUnlockPopupsController` continues its own internal multi-popup sequence independently.

This is safe because WeaponClassUnlock(60)/WeaponUnlock(70) are the lowest-priority coordinator
requests and are only reached after all celebration/feature popups have already been shown.
By the time the weapon flow starts, the coordinator queue is empty.

### Player navigates away during popup
If the player navigates away from the map while a popup is open (e.g. presses GoToWeapons in a
weapon unlock popup), the coordinator does not interfere.  The popup is destroyed as part of
navigation.  `WaitForClose` detects destruction → `_popupActive = false`.  `TryShowNext()` is
called but `_mapActive = false` (the weapons screen is now active) → no further popup shown.
When the player returns to the map, `OnMapFocusGained()` fires → `TryShowNext()` → resumes queue.

---

## 8. Files Changed

| File | Status |
|------|--------|
| `Assets/Meta/Missions/Gate/StartupPopupPriority.cs` | New |
| `Assets/Meta/Missions/Gate/IStartupPopupCoordinator.cs` | New |
| `Assets/Meta/Missions/Gate/StartupPopupRequest.cs` | New |
| `Assets/Meta/Missions/Gate/StartupPopupCoordinator.cs` | New |
| `Assets/Meta/Missions/Gate/LoopTransitionPresenter.cs` | Modified |
| `Assets/Meta/Missions/Gate/ZoneCompletePresenter.cs` | Modified |
| `Assets/Meta/Missions/Gate/BossVictoryPresenter.cs` | Modified |
| `Assets/Meta/Missions/Gate/ContractsMilestonePresenter.cs` | Modified |
| `Assets/Meta/Missions/Gate/ContractsUnlockPresenter.cs` | Modified |
| `Assets/Meta/Missions/Gate/SniperUnlockPresenter.cs` | Modified |
| `Assets/Meta/Weapons/Class Unlock/Popups/WeaponUnlockPopupsController.cs` | Modified |
| `Assets/Meta/Docs/Meta_Popup_Startup_Coordinator.md` | New (this file) |

**Not changed:** UINavigator.cs, ScreenRegistry.asset, DamageGatePopupController.cs,
PlayMissionPresenter.cs, BossIntroPresenter.cs, MissionInfoPopupController.cs,
CelebrationPopupController.cs, LoopTransitionPopupController.cs, all gate/balance files.

---

## 9. Inspector Setup Required

### A. Add StartupPopupCoordinator to the main menu scene

1. Open `Assets/Scenes/MainMenu.unity`.
2. Create an empty GameObject named `StartupPopupCoordinator`.
3. Attach the `StartupPopupCoordinator` component.
4. Set `_mapScreenId` to `"map"` (default — verify this matches the ScreenView ScreenId on the
   map screen prefab, i.e. the prefab at `Assets/Scripts/UI New/Prefabs/!ScreenRegistry.asset`
   entry `Id: map`).

### B. Verify popup IDs on all presenters in the scene

Confirm that the serialized `_popupId` fields on each presenter match the exact string IDs in
`!ScreenRegistry Popups.asset`:

| Presenter | Expected _popupId value |
|-----------|------------------------|
| LoopTransitionPresenter | `loop_transition_popup` |
| ZoneCompletePresenter | `celebration_popup` |
| BossVictoryPresenter | `celebration_popup` |
| ContractsMilestonePresenter | `celebration_popup` |
| ContractsUnlockPresenter | `celebration_popup` |
| SniperUnlockPresenter | `celebration_popup` |

If the scene was created with old default values (PascalCase), update them in the Inspector.

### C. Verify WeaponUnlockPopupsController popup IDs

On the `WeaponUnlockPopupsController` component, confirm:
- `_weaponUnlockedPopupId` = `weapon_unlocked_popup`
- `_weaponClassUnlockedPopupId` = `weapon_class_unlocked_popup`

### D. Verify Script Execution Order (optional safety)

`StartupPopupCoordinator` uses `[DefaultExecutionOrder(-100)]` to ensure it runs before
default-order components.  If UINavigator or WeaponsInitializer use a custom execution order
that is lower than -100, add an explicit Script Execution Order entry for
`StartupPopupCoordinator` with a value lower than those components.

In practice the default should work because UINavigator registers `IUINavigator` in Awake, and
the coordinator resolves it in Start — a later phase.

---

## 10. Test Checklist

### Core coordinator behaviour

- [ ] Only one popup appears at a time when multiple conditions are true simultaneously.
- [ ] Popups do NOT appear immediately on scene load — they appear only after the map screen
      transition completes (fade-in finished).
- [ ] With ContractsUnlock + SniperUnlock both triggered, they appear one after the other,
      NOT stacked.
- [ ] With ZoneComplete + ContractsUnlock + SniperUnlock all triggered, they appear sequentially
      in the correct priority order: ZoneComplete first (priority 30), ContractsUnlock second (40),
      SniperUnlock third (50).
- [ ] Closing one popup shows the next queued popup automatically.
- [ ] If no popups are queued, the map screen shows cleanly with no popup.

### Map-screen-only behaviour

- [ ] Navigating to the Weapons screen during a popup: no next popup appears on the Weapons screen.
- [ ] Returning from the Weapons screen to the map: if there are still queued popups, the next
      one appears.
- [ ] If the player navigates away and back multiple times, the queue resumes correctly each time.

### Priority order

- [ ] LoopTransition (priority 10) appears before all others when triggered simultaneously.
- [ ] BossVictory (priority 20) appears before ZoneComplete (30) in the same session.
- [ ] WeaponClassUnlock (60) appears after ContractsUnlock (40) and SniperUnlock (50).
- [ ] ContractsMilestone (80) always appears last.

### Save flag behaviour

- [ ] After a popup is shown, it does NOT appear again in the next session (flag saved).
- [ ] If the main menu scene is loaded but the map screen is never opened (edge case),
      the popup IS shown in the next session (flag not saved if never shown).
- [ ] Delete save data → all applicable popups appear again on next progression trigger.

### Fallback (coordinator absent)

- [ ] Removing the StartupPopupCoordinator GameObject from the scene and running:
      presenters fall back to direct nav.ShowPopup. No crash. Popups may stack (old behaviour).

### Damage gate popup (regression)

- [ ] Damage gate popup still opens immediately when Play button is pressed.
- [ ] Gate popup is NOT affected by coordinator (PlayMissionPresenter uses nav directly).
- [ ] Gate popup can appear while a queued coordinator popup is not yet shown.

### Mission info popup (regression)

- [ ] MissionInfoPopup still opens when the info button is pressed.
- [ ] Not affected by coordinator.

### Weapon unlock flow (regression)

- [ ] Spear class unlock popup appears when Spear is first unlocked.
- [ ] Closing the class popup shows the next weapon popup (if any) in WeaponUnlockPopupsController's
      internal sequence (coordinator is no longer involved after first popup).
- [ ] GoToWeaponClass button navigates to weapons screen and clears weapon queue.
- [ ] On return to map, no stale weapon popup reappears.

### BossIntroPresenter (not changed — regression)

- [ ] Boss intro popup still fires when Boss tab is selected for the first time.
- [ ] Does not appear on startup before the player interacts.

---

## 11. Remaining Risks

### R1 — PopupShown async timing in deferred transition case

When the coordinator shows a popup while UINavigator's RunQueue is still processing the map
screen transition, `nav.ShowPopup()` enqueues the popup rather than showing it immediately.
The `PopupShown` subscription stays open until the event fires in the next RunQueue iteration.
During this window, a user-triggered popup (e.g. fast Play click) could theoretically fire
`PopupShown` first, causing the coordinator to track the wrong popup.

**Likelihood:** Very low. The map transition completes before the player can interact.
**Impact:** Coordinator would wait for the wrong popup to close, then resume queue.
**Mitigation:** None implemented; acceptable risk for this project scale.

### R2 — Popup ID mismatch causes stuck coordinator

If `request.PopupId` does not exist in any registry, `UINavigator.InstantiateView` throws
`ArgumentException`. The coroutine aborts. `PopupShown` never fires. `_awaitingPopupShown`
stays true and `_popupActive` stays true. No further coordinator popups are shown for the session.

**Detection:** The exception will appear in the Unity console.
**Fix:** Verify all popup IDs in the inspector match the registry (see Section 9B).

### R3 — WeaponUnlockPopupsController second-and-later popups bypass map-screen guard

After the first weapon popup closes and `CloseAndContinue` calls `ShowNextIfAny()` directly,
subsequent weapon popups are shown via `nav.ShowPopup()` without coordinator involvement.
If the player navigated away from the map between closing the first and the next popup appearing
(extremely rare: they'd have to navigate away within one frame), the next weapon popup could
appear on a non-map screen.

**Likelihood:** Effectively zero in practice.

### R4 — ContractsUnlockPresenter and SniperUnlockPresenter thresholds still identical

Both default to company level 5. They fire in the same session but now appear sequentially
(ContractsUnlock first at priority 40, SniperUnlock second at priority 50). The collision is
resolved by the coordinator. However, if the design goal is to stagger the reveals across
different sessions, the thresholds should be changed (not in scope here).

### R5 — Scene must contain StartupPopupCoordinator

If the coordinator is not in the scene, all presenters fall back to direct `nav.ShowPopup()` —
restoring the old collision behaviour. The fallback is intentional for resilience, but it means
the fix requires the coordinator to actually be present in the scene.
