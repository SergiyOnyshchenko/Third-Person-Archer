# Meta_MainMenu_Popup_Startup_Audit.md

Audit of popup flow on Main Menu scene open.
Date: 2026-05-29. No code changes — analysis only.

---

## 1. Summary

**There is a real risk of multiple popups opening simultaneously on Main Menu startup.**

The root cause is twofold:

1. `UINavigator.ShowPopup()` has **no single-active-popup rule**. Each call instantiates a new popup instance and stacks it in `PopupsRoot`. There is no blocking, no queue-with-wait, and no "close previous popup first" logic. Multiple popups can and will overlap visually.

2. Six startup presenters (`LoopTransitionPresenter`, `ZoneCompletePresenter`, `BossVictoryPresenter`, `ContractsMilestonePresenter`, `ContractsUnlockPresenter`, `SniperUnlockPresenter`) all fire independently in the `OnEnable()` phase with no coordination between them. Additionally, `WeaponUnlockPopupsController` fires in `Start()`, potentially stacking on top of whatever the OnEnable presenters already showed.

Several realistic scenarios (detailed in Section 6) result in 2–3 popups stacking on the same frame.

---

## 2. Popup Registry Inventory

Entries from `Assets/Scripts/UI New/Prefabs/PopUps/!ScreenRegistry Popups.asset`:

| Registry ID               | Prefab GUID (first 8)     | Controller Script                  | Startup Risk |
|---------------------------|---------------------------|------------------------------------|--------------|
| `popup_error`             | 72fe2cec                  | unknown (legacy)                   | Unknown      |
| `popup_unlock`            | e1b11211                  | unknown (legacy)                   | Unknown      |
| `popup_warning`           | 7e59b6da                  | unknown (legacy)                   | Unknown      |
| `weapon_unlocked_popup`   | 2a372a35                  | (WeaponUnlockedPopup controller)   | YES (Start)  |
| `weapon_class_unlocked_popup` | 21742656              | WeaponClassUnlockedPopupScreen     | YES (Start)  |
| `damage_gate_popup`       | 496c3b2b                  | DamageGatePopupController          | NO (Play btn)|
| `loop_transition_popup`   | fda2886a                  | LoopTransitionPopupController      | YES (OnEnable)|
| `celebration_popup`       | 58e5c88a                  | CelebrationPopupController         | YES (OnEnable)|
| `mission_info_popup`      | 0aee97b9                  | MissionInfoPopupController         | NO (info btn)|

**Startup Risk = YES** means the popup can open automatically without any user action when the Main Menu scene loads.

---

## 3. Startup Popup Callers

All scripts that can open a popup automatically at Main Menu startup (no user interaction required):

| Script                        | Lifecycle Hook | Popup ID (code default)     | Save Key                                   | One-time? | Fires In   |
|-------------------------------|----------------|-----------------------------|--------------------------------------------|-----------|------------|
| `LoopTransitionPresenter`     | OnEnable       | `"LoopTransitionPopup"`     | `loop_transition_shown`                    | Yes       | OnEnable phase |
| `ZoneCompletePresenter`       | OnEnable       | `"CelebrationPopup"`        | `zone_complete_popup_shown_{zone.ID}`      | Yes/zone  | OnEnable phase |
| `BossVictoryPresenter`        | OnEnable       | `"CelebrationPopup"`        | `boss_victory_popup_shown_{zone.ID}`       | Yes/zone  | OnEnable phase |
| `ContractsMilestonePresenter` | OnEnable       | `"CelebrationPopup"`        | `contracts_milestone_highest_shown`        | Per level | OnEnable phase |
| `ContractsUnlockPresenter`    | OnEnable       | `"CelebrationPopup"`        | `contracts_unlock_popup_shown`             | Yes       | OnEnable phase |
| `SniperUnlockPresenter`       | OnEnable       | `"CelebrationPopup"`        | `sniper_unlock_popup_shown`               | Yes       | OnEnable phase |
| `WeaponUnlockPopupsController`| Start          | `"WeaponUnlockedPopup"` / `"WeaponClassUnlockedPopup"` | `weapon_unlock_popups_progress_v1` | Yes/weapon | Start phase |

Scripts that open popups only on user action (NOT startup risk):

| Script                   | Trigger                    | Popup ID             |
|--------------------------|----------------------------|----------------------|
| `PlayMissionPresenter`   | Play button click          | `damage_gate_popup`  |
| `BossIntroPresenter`     | Boss tab selected          | `"CelebrationPopup"` |
| `MissionTypeInfoButton`  | Info button click          | `mission_info_popup` |

---

## 4. Initialization Order

Unity lifecycle for the Main Menu scene:

```
FRAME 0 — Awake phase (all objects, order by script execution order / hierarchy)
  UINavigator.Awake()
    → registers IUINavigator in ServiceLocator
    → _queue initialized (empty)
    → _isTransitionRunning = false

  WeaponsInitializer.Awake()
    → registers IWeaponClassUnlockService in ServiceLocator

  MainMenuRuntime.Awake()
    → builds all Services
    → Ready?.Invoke(Services)     ← fires event synchronously
      (no subscribers yet — presenters haven't had OnEnable called)

FRAME 0 — OnEnable phase (same frame, after all Awake calls)
  LoopTransitionPresenter.OnEnable()
    → runtime.Services != null → CheckAndShow() → [maybe] nav.ShowPopup(...)

  ZoneCompletePresenter.OnEnable()
    → runtime.Services != null → CheckAndShow() → [maybe] nav.ShowPopup(...)

  BossVictoryPresenter.OnEnable()
    → runtime.Services != null → CheckAndShow() → [maybe] nav.ShowPopup(...)

  ContractsMilestonePresenter.OnEnable()
    → runtime.Services != null → CheckAndShow() → [maybe] nav.ShowPopup(...)

  ContractsUnlockPresenter.OnEnable()
    → runtime.Services != null → CheckAndShow() → [maybe] nav.ShowPopup(...)

  SniperUnlockPresenter.OnEnable()
    → runtime.Services != null → CheckAndShow() → [maybe] nav.ShowPopup(...)

FRAME 0 — Start phase
  WeaponUnlockPopupsController.Start()
    → resolves IUINavigator and IWeaponClassUnlockService from ServiceLocator
    → RebuildAndStart() → ShowNextIfAny() → [maybe] nav.ShowPopup(...)
```

**Key observation**: Because `MainMenuRuntime.Ready` fires during `Awake()`, no presenter receives it via the event subscription path. Instead, all presenters take the fast path in their `OnEnable()`:

```csharp
// Fast path in every presenter:
var runtime = MainMenuRuntime.Instance;
if (runtime != null && runtime.Services != null)
{
    CheckAndShow(runtime.Services);   // fires directly
    return;
}
```

This means all six OnEnable presenters call `CheckAndShow()` in the **same frame**, and any that pass their condition call `nav.ShowPopup()` in the **same frame**.

---

## 5. Popup Queue / Stacking Behavior

### UINavigator internal queue

`UINavigator` has a `Queue<IEnumerator> _queue` with `_isTransitionRunning` guard. All operations (Open, ShowModal, ShowOverlay, ShowPopup) go through `Enqueue(routine)`. This queue **serializes navigation transitions** — it prevents overlapping fade-in/fade-out animations.

**However, for popups this queue provides NO protection:**

```csharp
private IEnumerator ShowPopupRoutine(string popupId, object? args)
{
    var p = InstantiateView(popupId, _popupsRoot);
    ApplyArgsIfAny(p, args);
    _transition.InstantShow(p);      // shown immediately
    p.OnOpened.Invoke();
    PopupShown?.Invoke(p);
    yield break;                     // no wait, no block
}
```

- Uses `InstantShow` — no animation delay.
- `yield break` immediately — the queue drains without waiting for the popup to be closed.
- No check for "is another popup already visible?"
- No reference stored to previous popups.

**Result**: Multiple `ShowPopup()` calls in the same frame = multiple popup instances instantiated under `PopupsRoot`, all visible at once, stacked in z-order.

### WeaponUnlockPopupsController internal queue

This controller has its OWN internal queue (`_pendingArgs`) and a `_popupActive` flag. It shows the next weapon popup only when `CloseAndContinue()` is called from the popup itself. This is a correct per-controller queue.

**But it is isolated** — it has no awareness of the celebration popups shown by the startup presenters, and the startup presenters have no awareness of WeaponUnlockPopupsController.

### Summary

| Mechanism                          | Exists? | Scope                               |
|------------------------------------|---------|-------------------------------------|
| UINavigator popup queue            | YES     | Serializes transitions only; popups drain instantly |
| UINavigator single-active-popup rule | NO    | Multiple popups can stack           |
| Global startup popup coordinator   | NO      | Each presenter fires independently  |
| WeaponUnlockPopupsController queue | YES     | Only for weapon/class unlock popups |
| Priority ordering between presenters | NO    | Determined by scene hierarchy order |

---

## 6. Risk Scenarios

### Scenario A: Player opens Main Menu after completing Zone 1 Campaign for the first time

Conditions: zone_complete flag unset, company level just reached 5 (both Contracts and Sniper unlock simultaneously, which is the default threshold for both).

Popups that attempt to open:
1. `ZoneCompletePresenter` → `celebration_popup` ("Zone Campaign Complete!")
2. `ContractsUnlockPresenter` → `celebration_popup` ("Contracts Unlocked!")
3. `SniperUnlockPresenter` → `celebration_popup` ("Sniper Unlocked!")

All three fire in OnEnable phase. All three call `nav.ShowPopup("CelebrationPopup", ...)`.

**Result**: 3 instances of `celebration_popup` prefab instantiated in PopupsRoot, stacked. Player sees top one. After closing top, sees next. After closing that, sees the third. The experience is confusing and unintentional.

**Risk level: CRITICAL** — Highly realistic scenario for early progression.

---

### Scenario B: Player opens Main Menu after defeating Zone 1 Boss

Conditions: boss_victory flag unset for zone 1. Zone 1 campaign was completed a prior session (zone_complete flag already set). Company level likely >= 5 already (both unlock flags already set).

Popups that attempt to open:
1. `BossVictoryPresenter` → `celebration_popup` ("Boss Defeated!")

Only one fires in typical case (unlock popups already shown). No collision.

**Result**: Safe in isolation. Only becomes a risk if contracts/sniper flags are also unset for some reason.

**Risk level: MINOR** in normal flow.

---

### Scenario C: Player opens Main Menu after completing Zone 3 Campaign (Loop Transition)

Conditions: loop_transition flag unset. Zone 3 is the last zone with no boss. All previous zones complete. Player also hit a new contracts milestone (e.g., 5th contract) in the same play session.

Popups that attempt to open:
1. `LoopTransitionPresenter` → `loop_transition_popup` ("Campaign Complete!")
2. `ContractsMilestonePresenter` → `celebration_popup` ("Contract Milestone: 5!")

Both fire in OnEnable phase.

**Result**: Two different popup types stack. `loop_transition_popup` (LoopTransitionPopupController with hardcoded text) and `celebration_popup` (CelebrationPopupController) are both visible.

**Risk level: MAJOR** — Loop completion is a significant moment; obscuring it with a milestone popup hurts the experience.

---

### Scenario D: Player opens Main Menu — Spear class just unlocked (first time Zone 2 reached)

Conditions: Spear class newly unlocked, `weapon_unlock_popups_progress_v1` doesn't include Spear class.

Popups that attempt to open (OnEnable phase):
- (depends on other flags)

In Start phase:
1. `WeaponUnlockPopupsController.Start()` → enqueues `weapon_class_unlocked_popup` ("New Weapon Class Unlocked — Spear")

If ZoneCompletePresenter also fires (Zone 1 just completed) in OnEnable:
1. `ZoneCompletePresenter` → `celebration_popup`
2. `WeaponUnlockPopupsController` → `weapon_class_unlocked_popup`

**Result**: celebration_popup (from OnEnable) and weapon_class_unlocked_popup (from Start) stack. Different prefabs, same visual layering problem.

**Risk level: MAJOR** — This scenario is EXPECTED at Zone 2 entry, which is a key progression gate.

---

### Scenario E: Player opens Main Menu — both Contracts and Sniper unlock simultaneously

Conditions: Company level just crossed both thresholds (defaults: both at level 5). Both save flags unset.

Popups that attempt to open:
1. `ContractsUnlockPresenter` → `celebration_popup` ("Contracts Unlocked!")
2. `SniperUnlockPresenter` → `celebration_popup` ("Sniper Unlocked!")

Both fire in OnEnable phase. Both use `celebration_popup` ID. Two instances created, stacked.

**Result**: Two identical-prefab instances stacked, each with different args text. Player sees top one ("Sniper Unlocked!"), closes it, sees bottom one ("Contracts Unlocked!") — or the reverse depending on OnEnable order. Message order may be inverted relative to intent.

**Risk level: CRITICAL** — Both unlock thresholds are identical by default, so this happens on EVERY new player reaching that company level.

---

### Scenario F: Maximum collision — new player reaching end of Zone 1 + company level 5 simultaneously

Conditions: All of zone_complete, contracts_unlock, sniper_unlock, and possibly first contracts milestone (if 1 contract done) flags all unset.

Popups that attempt to open:
1. `ZoneCompletePresenter` → `celebration_popup`
2. `ContractsUnlockPresenter` → `celebration_popup`
3. `SniperUnlockPresenter` → `celebration_popup`
4. `ContractsMilestonePresenter` → `celebration_popup` (if >= 1 contract done)
5. `WeaponUnlockPopupsController` → `weapon_class_unlocked_popup` (if Spear just unlocked)

Up to 5 popups stacking. All but the last are the same prefab.

**Result**: Visually broken. Player sees a stack of popups with no context of how many there are.

**Risk level: CRITICAL**.

---

## 7. Duplication / Legacy Popups

### `weapon_class_unlocked_popup` — legacy vs Phase C

Before Phase C, `WeaponClassIntroPresenter` existed as a separate class (now deleted per MEMORY.md — merged into `WeaponUnlockPopupsController`). The weapon class unlock flow is now handled exclusively in `WeaponUnlockPopupsController.BuildQueue()`.

Current state:
- `WeaponUnlockPopupsController` enqueues `WeaponClassUnlockedPopupArgs` for Spear and Boomerang.
- No other script produces `weapon_class_unlocked_popup`.
- No duplication — this path is clean.

### `popup_unlock`, `popup_error`, `popup_warning` — legacy IDs

These three IDs exist in the ScreenRegistry but no caller was found in the audited scripts. They may be used by other UI systems (economy popups, error handling) or may be unused legacy entries. They do not appear to pose a startup collision risk, but their callers were not fully traced.

### `celebration_popup` — shared by 6 presenters

This is the main source of collision risk. Six different presenters all default to the same popup ID ("CelebrationPopup"). The popup itself is generic (title + body text from args). There is no logical issue with reuse, but no coordination layer exists to prevent simultaneous instantiation.

### `LoopTransitionPopupController` — uses hardcoded text

Unlike all other startup popups, `LoopTransitionPopupController` has its text baked into the prefab (Inspector fields), not passed via args. `LoopTransitionPresenter` calls `nav.ShowPopup(_popupId, null)`. This is intentional and correct for this popup type, but it uses a DIFFERENT popup ID than all celebration popups, which means it cannot be accidentally deduplicated by a future queue system unless handled explicitly.

---

## 8. Findings

### CRITICAL

**C1 — No single-active-popup rule in UINavigator**
- UINavigator `ShowPopupRoutine` uses `InstantShow` + `yield break`. No check for existing open popups. No wait mechanism. Multiple calls in the same frame instantiate multiple stacked popup GameObjects.
- Evidence: `UINavigator.cs` lines 220–231, `ShowPopupRoutine`.
- Affected files: `UINavigator.cs`.

**C2 — ContractsUnlockPresenter + SniperUnlockPresenter guaranteed to collide**
- Both default to company level 5. Both use `celebration_popup`. Both fire in the same OnEnable phase.
- If both thresholds are equal (which they are by default), they ALWAYS fire in the same session.
- Evidence: `ContractsUnlockPresenter.cs:19`, `SniperUnlockPresenter.cs:19`.
- Affected files: `ContractsUnlockPresenter.cs`, `SniperUnlockPresenter.cs`.

**C3 — Up to 4-5 startup popups can fire in one frame**
- ZoneCompletePresenter + ContractsUnlockPresenter + SniperUnlockPresenter + ContractsMilestonePresenter + WeaponUnlockPopupsController have no mutual awareness. All can pass their conditions in the same session.
- Evidence: All six presenter `OnEnable()`/`Start()` methods call `nav.ShowPopup()` independently.
- Affected files: all presenter files listed in Section 3.

---

### MAJOR

**M1 — WeaponUnlockPopupsController (Start) stacks on top of OnEnable celebration popups**
- WeaponUnlockPopupsController fires in `Start()`, after all OnEnable presenters. If celebration popups are already shown in OnEnable, weapon popups stack on top of them.
- Evidence: `WeaponUnlockPopupsController.cs:41` (`Start()`), all presenters use `OnEnable()`.
- Affected files: `WeaponUnlockPopupsController.cs` + all OnEnable presenters.

**M2 — ZoneCompletePresenter can collide with BossVictoryPresenter in theory**
- Example: Zone 2 campaign completes in one session AND Zone 1 boss is defeated in the same session. In that case both flags are unset. ZoneCompletePresenter fires for Zone 2, BossVictoryPresenter fires for Zone 1. Both use `celebration_popup`, both fire in OnEnable.
- Evidence: `ZoneCompletePresenter.cs:91` (`break` after first match), `BossVictoryPresenter.cs:87` (`break` after first match). Both can pass simultaneously.
- Affected files: `ZoneCompletePresenter.cs`, `BossVictoryPresenter.cs`.

**M3 — LoopTransitionPresenter can collide with ContractsMilestonePresenter**
- Loop completion is a key UX moment. If a contracts milestone is also reached that session, `ContractsMilestonePresenter` fires a `celebration_popup` on top of the `loop_transition_popup`.
- Evidence: `LoopTransitionPresenter.cs:71`, `ContractsMilestonePresenter.cs:80`.
- Affected files: `LoopTransitionPresenter.cs`, `ContractsMilestonePresenter.cs`.

---

### MINOR

**m1 — Popup ID code defaults may not match ScreenRegistry IDs**
- Code defaults use PascalCase (`"CelebrationPopup"`, `"LoopTransitionPopup"`, `"DamageGatePopup"`, `"WeaponUnlockedPopup"`, `"WeaponClassUnlockedPopup"`), but ScreenRegistry uses snake_case (`celebration_popup`, `loop_transition_popup`, etc.).
- These are `[SerializeField]` fields, so runtime values depend on Inspector. If any presenter was added to the scene using code defaults (e.g., by a drag-and-drop that didn't override the field), it will throw `ArgumentException` at runtime.
- Evidence: `!ScreenRegistry Popups.asset` lines 16–33 vs. presenter default field values.
- Affected files: all presenter `.cs` files + `ScreenRegistry Popups.asset`.

**m2 — No popup close ordering guarantee**
- Popups auto-close via `Destroy(gameObject)` on button press. There is no tracking of which popup was opened first. If 3 popups stack, the player closes them top-to-bottom but the message order is the reverse of intent (last-fired popup is on top).
- Evidence: `CelebrationPopupController.cs:54` (OnClose), `UINavigator.ShowPopupRoutine` — no stack tracking.

**m3 — BossIntroPresenter is safe but fires `celebration_popup` on tab change**
- BossIntroPresenter is NOT a startup risk (fires on `OnMissionTypeChanged`). However, if the player switches to the Boss tab while a startup popup is still open, a second `celebration_popup` instance will appear on top.
- Evidence: `BossIntroPresenter.cs:67` (`OnMissionTypeChanged` listener).

---

## 9. Recommended Next Steps

In priority order (no implementation here — recommendations only):

**1. Implement a startup popup queue / coordinator**

A single component (e.g., `StartupPopupCoordinator`) should:
- Collect all pending startup popup requests after `MainMenuRuntime.Ready`
- Show them one at a time with priority ordering
- Wait for the user to close each before showing the next

All startup presenters (LoopTransitionPresenter, ZoneCompletePresenter, etc.) would register with this coordinator instead of calling `nav.ShowPopup()` directly.

**2. Define popup priority order**

Suggested priority (high to low):
1. Loop Transition (major milestone — own popup type)
2. Boss Victory (major milestone)
3. Zone Campaign Complete (zone gate)
4. Contracts Unlocked (feature unlock)
5. Sniper Unlocked (feature unlock)
6. Boss Intro (contextual)
7. Weapon Class Unlocked (weapon system)
8. Weapon Unlocked (weapon system)
9. Contracts Milestone (repeating reward beat)

**3. Fix C2: Stagger ContractsUnlockPresenter and SniperUnlockPresenter thresholds**

Use different company levels (e.g., Contracts at 5, Sniper at 7) so they never fire in the same session. This is a balance/design decision, not just a technical fix.

**4. Add a single-popup guard to UINavigator (optional lower priority)**

If a `ShowPopup()` call arrives while another popup is visible in `PopupsRoot`, queue it until the current popup is destroyed. This requires `UINavigator` to track active popup instances or listen to their `OnDestroy`.

**5. Verify Inspector popup ID values**

Confirm all serialized `_popupId` fields in presenters match the exact IDs in `!ScreenRegistry Popups.asset` (snake_case). Mismatches cause runtime exceptions.

**6. WeaponUnlockPopupsController — add delay or coordinator hook**

Instead of firing in `Start()` independently, the weapon popup flow should wait for the startup popup coordinator to finish before showing weapon popups. This prevents weapon popups from stacking on top of celebration popups.
