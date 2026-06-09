# Post-Mission Next Step Guidance Plan

---

## 1. Goal

After the player returns to the map/main menu from any mission, the game should detect whether the player is blocked on the next progression target and proactively guide them toward the right action — upgrade, buy, grind Contracts, or grind Sniper — with a contextual popup and a clear action button.

This is not a one-time tutorial. The popups repeat whenever the situation is relevant.
The existing "Recommended Mission Hint" dot on the map is preserved and complemented by this flow.

---

## 2. Current Flow Audit

### 2.1 PlayMissionPresenter
**File:** `Assets/Meta/Missions/Controllers/PlayMissionPresenter.cs`

- Handles the Play button click via `_services.MissionStart.TryStartSelected()`.
- If start fails with a damage gate reason, calls one of three methods:
  - `ShowDamageGatePopup(gate)` — CampaignDamageTooLow
  - `ShowSniperGatePopup(gate)` — SniperDamageTooLow
  - `ShowBossGatePopup(gate)` — BossCrossbowDamageTooLow
- All three call `nav.ShowPopup(_damageGatePopupId, args)` directly — **not** through `StartupPopupCoordinator`.
- Builds a long, verbose `customHint` string inline.
- Has three one-time tutorial flags (`tutorial_first_campaign_gate`, `tutorial_first_boss_gate`, `tutorial_first_forced_purchase_gate`) managed locally.
- The popup always has only one action: "Go to Weapons" (weapon screen).

### 2.2 DamageGatePopupController / DamageGatePopupArgs
**Files:** `Assets/Meta/Missions/Gate/DamageGatePopupController.cs`, `DamageGatePopupArgs.cs`

- `DamageGatePopupArgs` fields: `WeaponClass, CampaignLevel, CurrentDamage, RequiredDamage, WeaponScreenId, CanUpgradeToPass, CustomTitle?, CustomHint?, TutorialText?`
- `DamageGatePopupController` displays: title, current/required damage numbers, hint text, optional tutorial block.
- Single action button "Go to Weapons" → `nav.Open(WeaponScreenId, WeaponSelectionArgs)`.
- Close via `Destroy(gameObject)`.
- Supports custom title/hint via args but always routes to the weapon screen.

### 2.3 MissionGateService
**File:** `Assets/Meta/Missions/Services/MissionGateService.cs`

- `CheckCampaignGate(ctx)` → `MissionGateResult`
- `CheckSniperGate(ctx, sniperIndex)` → `MissionGateResult`
- `CheckBossGate(ctx)` → `MissionGateResult`
- `MissionGateResult` fields: `Passed, RequiredDamage, CurrentDamage, RequiredWeaponClass, CanUpgradeToPass`
- `CanUpgradeToPass` = true if the currently equipped weapon, when fully upgraded, reaches `RequiredDamage`. False means a higher-tier weapon purchase is required.

### 2.4 MissionAvailabilityService
**File:** `Assets/Meta/Missions/Services/MissionAvailabilityService.cs`

- `GetAvailability(MissionContext ctx)` → `MissionAvailability { CanPlay, Reason }`
- Block reasons relevant to this plan:
  - `CampaignDamageTooLow` — Campaign gate blocked by weapon damage
  - `SniperDamageTooLow` — Sniper access gate (Crossbow) blocked
  - `BossCrossbowDamageTooLow` — Boss gate (Crossbow) blocked
  - `ContractsLockedByCompanyLevel` — Contracts not yet unlocked
  - `SniperLockedByCompanyLevel` — Sniper not yet unlocked
  - `BossNotUnlocked` — Campaign in zone not complete yet

### 2.5 MissionStartService
**File:** `Assets/Meta/Missions/Launch/MissionStartService.cs`

- `TryStartSelected()` → `MissionStartResult { Started, FailReason, Availability, Gate, MissionToLoad, RequiredWeaponClass }`
- If `Started = false` and `Gate != null`, full gate info is available.
- For Contracts: randomly selects an eligible contract mission from pool.

### 2.6 WeaponSelectionScreen / loadout navigation
**File:** `Assets/Meta/Weapons/UI/Selection/WeaponSelectionScreen.cs`

- `IReceivesArgs<WeaponSelectionArgs>` where `WeaponSelectionArgs(WeaponClass, CampaignLevel, PreselectWeaponId?)`
- Opened via `nav.Open(screenId, args, reuseCached: true)`.
- Already pre-selects the weapon class tab from args.
- Has upgrade (`OnUpgradeClicked`) and purchase (`OnPurchaseClicked`) buttons in `WeaponPricePanelView`.
- No existing highlight mechanism for specific buttons.

### 2.7 Weapon Upgrade/Purchase Services
**Files:** `Assets/Meta/Weapons/Services/Upgrades/IUpgradeService.cs`

- `IUpgradeService.CanUpgrade(weaponId)` — checks wallet affordability.
- `IUpgradeService.TryUpgradeWithCurrencies(weaponId)` — spends Cash + class token.
- `IUpgradeService.OnWeaponUpgraded` event — `(weaponId, newLevel)`.
- No `IPurchaseService` interface found separately; purchase appears handled inside `WeaponSelectionScreen` via `WeaponPricePanelView.OnPurchaseClicked`.
- Purchase events: need to confirm whether `IWeaponRepository` or equivalent emits an unlock event. **Open question.**

### 2.8 Resource/Currency Services
**File:** `Assets/Meta/Economy/IWallet.cs`, `WalletService.cs`

- `IWallet.BalanceChanged` event.
- `IWallet.CanAfford(CurrencyType, amount)`.
- `IWallet.Get(CurrencyType)`.
- Crossbow tokens: `CurrencyType.CrossbowToken` (via `WeaponCurrencyUtility.GetTokenCurrency(WeaponClass.Crossbow)`).

### 2.9 StartupPopupCoordinator
**File:** `Assets/Meta/Missions/Gate/StartupPopupCoordinator.cs`

- Coordinates one popup at a time, only shown when map screen is active.
- `Submit(StartupPopupRequest)` with deduplication by `LogicalId`.
- After a popup is dequeued and shown, its `LogicalId` is removed from `_pendingIds` — re-submission with the same LogicalId is allowed after dismissal.
- Map focus detection: `ScreenOpened` + `OnFocusGained/Lost` on the map `ScreenView`.
- After popup closes (via `Destroy(gameObject)`), calls `TryShowNext()`.
- Priority list (existing): LoopTransition(10) > BossVictory(20) > ZoneComplete(30) > ContractsUnlock(40) > SniperUnlock(50) > WeaponClassUnlock(60) > WeaponUnlock(70) > ContractsMilestone(80).

### 2.10 RecommendedMissionHintService
**File:** `Assets/Meta/Missions/Gate/RecommendedMissionHintService.cs`

- Static `GetRecommendedType(MainMenuServices)` → `MissionType?`
- Priority: Campaign passable → Campaign; Campaign blocked → Contracts (if unlocked); Campaign complete + boss passable → Boss; Boss blocked by Crossbow gate → Sniper.
- Used by map tab buttons to show the "recommended" indicator dot.
- This plan does NOT modify this service.

### 2.11 Main Menu Map Screen Flow

- Map screen ID in ScreenRegistry: `"map"`.
- `IUINavigator.ScreenOpened` fires when map is navigated to.
- `ScreenView.OnFocusGained` fires when map regains focus after `GoBack()`.
- Both paths are already captured by `StartupPopupCoordinator`.
- Tab/mission type selection appears to go through `IMissionContextService` — exact mechanism for programmatic tab switching is an open question.

---

## 3. Proposed Player Flow

```
[Player finishes mission] → [Main menu loads] → [Map screen opens / gains focus]
        ↓
[PostMissionNextStepPresenter evaluates recommendation]
        ↓
[Recommendation = None?] → [Nothing shown, map is normal]
        ↓
[Recommendation = UpgradeWeapon or BuyWeapon?]
    → [Submit Popup A to coordinator, priority 90]
        ↓
[Popup A shown: "Your [Class] is too weak"]
    → [Player taps "Open Weapons"] → [WeaponSelectionScreen opens, class pre-selected, action highlighted]
    → [Player upgrades/buys] → [Returns to map] → [Re-evaluate]
    → [Player taps "Later"] → [Popup dismissed, state recorded, no re-show unless something changes]
        ↓
[Recommendation = PlayContracts or PlaySniper?]
    → [Submit Popup B to coordinator, priority 90]
        ↓
[Popup B shown: "Not enough resources"]
    → [Player taps "Play Contracts" / "Play Sniper"] → [Map tab selected] → [Player plays mission]
    → [Player taps "Later"] → [Popup dismissed, state recorded]
```

**Priority rule:**
- Popup A is always evaluated first.
- Only if A does not apply (cannot upgrade/buy now), evaluate B.
- Never submit A and B at the same time.
- After any action (upgrade, purchase, mission), re-evaluate from scratch.

---

## 4. Recommendation Decision Logic

### 4.1 New Class: `NextStepRecommendationService`

**File (new):** `Assets/Meta/Missions/Gate/NextStepRecommendationService.cs`

This service is the single source of truth. It should be a plain C# class (not MonoBehaviour), instantiated by `MainMenuRuntime` alongside other services, and resolvable via `MainMenuServices`.

**Dependencies:**
- `IMissionAvailabilityService`
- `IMissionGateService`
- `IMissionContextService`
- `IUpgradeService`
- `IWeaponRepository` (to find owned weapons of a class and check max tier)
- `IWallet`
- `WeaponCurrencyUtility` (for token type lookup)
- `MissionProgressData` (to know campaign state, zone, boss unlocked)

### 4.2 Recommendation Result Type

```
Assets/Meta/Missions/Gate/NextStepRecommendation.cs (new)

public enum NextStepRecommendationType
{
    None,
    PlayCampaign,   // player can proceed, no guidance needed (reserved for future use)
    PlayBoss,       // player can proceed to boss (reserved for future use)
    UpgradeWeapon,  // Popup A: upgrade current weapon
    BuyWeapon,      // Popup A: buy a stronger weapon in same class
    PlayContracts,  // Popup B: grind contracts for cash/tokens
    PlaySniper,     // Popup B: grind sniper for Crossbow tokens
}

public sealed class NextStepRecommendation
{
    public NextStepRecommendationType Type;
    public WeaponClass RequiredWeaponClass;
    public float CurrentDamage;
    public float RequiredDamage;
    public string TargetWeaponId;   // for UpgradeWeapon: the weapon to highlight
    public bool SniperAvailable;    // for PlaySniper: whether Sniper is unlocked
    public bool ContractsAvailable; // for PlayContracts: whether Contracts is unlocked
}
```

### 4.3 Decision Algorithm

```
NextStepRecommendation Evaluate(MissionContext ctx):

1. Get next target:
   - If Campaign incomplete in current zone:
       targetType = MissionType.Campaign
   - Else if zone has boss and boss not yet unlocked:
       targetType = MissionType.Boss (but BossNotUnlocked reason → no gate, no guidance needed)
   - Else if zone has boss and boss unlocked:
       targetType = MissionType.Boss
   - Else:
       return None  (Zone 3 / no boss — loop transition handles this)

2. Check gate:
   avail = availability.GetAvailability(BuildContext(targetType))
   if avail.CanPlay → return None  (no guidance needed, player can proceed)
   if avail.Reason == BossNotUnlocked → return None  (campaign not complete, not a damage gate)
   if avail.Reason not in {CampaignDamageTooLow, BossCrossbowDamageTooLow} → return None

3. Get gate result:
   gate = gateService.CheckCampaignGate(ctx) or CheckBossGate(ctx)
   requiredClass = gate.RequiredWeaponClass

4. Determine if player can act now (Popup A conditions):
   a) gate.CanUpgradeToPass = true:
      equippedWeaponId = equipmentService.GetEquipped(requiredClass)
      if upgradeService.CanUpgrade(equippedWeaponId) → return UpgradeWeapon(equippedWeaponId)
   b) gate.CanUpgradeToPass = false OR cannot afford upgrade:
      nextTierWeapon = first locked weapon in same class with BaseDamage >= RequiredDamage
                        AND unlock campaign level <= current campaign level
      if nextTierWeapon != null:
          canAffordPurchase = wallet.CanAfford(CurrencyType.Cash, price) && wallet.CanAfford(tokenType, tokenPrice)
          if canAffordPurchase → return BuyWeapon(nextTierWeapon.Id)

5. If no Popup A action available → Popup B:
   if requiredClass == Crossbow:
       sniperAvail = availability.GetAvailability(BuildContext(MissionType.Sniper))
       if sniperAvail.CanPlay → return PlaySniper
       else → return PlayContracts  (fallback if Sniper locked)
   else:
       contractsAvail = availability.GetAvailability(BuildContext(MissionType.Contracts))
       if contractsAvail.CanPlay → return PlayContracts
       else → return None  (Contracts locked, no useful guidance)
```

**Note on step 4b:** "next tier weapon" lookup requires reading the weapon catalog. The service needs `IWeaponRepository` or `WeaponCatalog` to enumerate weapons by class. Exact API depends on how `WeaponCatalog` exposes data — this is an open question to resolve before implementation.

**Note on step 5 Crossbow fallback:** If Sniper is locked by company level, Popup B should still show but target Contracts and mention that Sniper unlocks later. Or suppress entirely. Document as open question.

### 4.4 Context Building

Mirrors `RecommendedMissionHintService.BuildContext()` — reuse or extract a shared helper to avoid duplicating the pattern.

---

## 5. Popup A — Weapon Too Weak

### 5.1 Purpose
Player has enough resources to upgrade or buy the needed weapon right now. Guide them to the weapon screen with a clear action.

### 5.2 When shown
`NextStepRecommendation.Type` is `UpgradeWeapon` or `BuyWeapon`.

### 5.3 Content
- **Title:** "Your [WeaponClass] is too weak" (e.g., "Your Bow is too weak")
  - For Crossbow/Boss: "Crossbow too weak for the Boss"
- **Body:** "You need [RequiredDamage] damage. You have [CurrentDamage]."
- **Action button label:**
  - `UpgradeWeapon` → "Upgrade [WeaponClass]"
  - `BuyWeapon` → "Buy Stronger [WeaponClass]"
- **Later button:** dismiss popup

### 5.4 Action behavior
Button tapped → close popup → open `WeaponSelectionScreen` with:
- `WeaponClass = requiredClass`
- `CampaignLevel = ctx.GlobalCampaignIndex + 1`
- `PreselectWeaponId = targetWeaponId` (the specific weapon to highlight)
- `HighlightMode = Upgrade` or `HighlightMode = Buy` (new field, see §8)

### 5.5 Args class (new)

```
Assets/Meta/Missions/Gate/WeaponTooWeakPopupArgs.cs

public sealed class WeaponTooWeakPopupArgs
{
    public WeaponClass RequiredWeaponClass;
    public float CurrentDamage;
    public float RequiredDamage;
    public bool IsUpgrade;         // true = upgrade action, false = buy action
    public string TargetWeaponId;  // weapon to preselect
    public string WeaponScreenId;
    public int CampaignLevel;
}
```

### 5.6 Controller class (new)

```
Assets/Meta/Missions/Gate/WeaponTooWeakPopupController.cs
implements IReceivesArgs<WeaponTooWeakPopupArgs>
```

- Renders title, damage numbers, button label.
- "Open Weapons" button: closes popup, opens `WeaponSelectionScreen` with `HighlightMode`.
- "Later" button: closes popup (`Destroy(gameObject)`).

### 5.7 Popup registration
- ScreenRegistry ID: `"weapon_too_weak_popup"` (new entry, set in Unity Inspector).
- Prefab: new prefab in `Assets/Meta/Missions/Gate/` or reuse `DamageGatePopup` prefab if it can be extended.

---

## 6. Popup B — Not Enough Resources

### 6.1 Purpose
Player's next mission is blocked and they cannot currently upgrade or buy. They need to grind resources first.

### 6.2 When shown
`NextStepRecommendation.Type` is `PlayContracts` or `PlaySniper`.

### 6.3 Content
- **Title:**
  - PlayContracts: "Not enough resources"
  - PlaySniper: "Not enough Crossbow Tokens"
- **Body:**
  - PlayContracts: "Earn more cash and tokens by completing Contracts, then upgrade your [WeaponClass]."
  - PlaySniper: "Earn Crossbow Tokens by playing Sniper missions, then upgrade your Crossbow."
- **Action button label:**
  - PlayContracts → "Play Contracts"
  - PlaySniper → "Play Sniper"
- **Later button:** dismiss popup

### 6.4 Action behavior
Button tapped → close popup → select Contracts or Sniper tab on the map.

Implementation approach (see §9 for full plan):
- Call `IMissionContextService.SelectType(MissionType.Contracts)` or `.SelectType(MissionType.Sniper)` to change the selected tab.
- The player then presses Play themselves — we do NOT auto-start.
- This mirrors how DamageGatePopup opens WeaponSelectionScreen: it navigates to the right context and lets the player act.

### 6.5 Args class (new)

```
Assets/Meta/Missions/Gate/NotEnoughResourcesPopupArgs.cs

public sealed class NotEnoughResourcesPopupArgs
{
    public bool IsCrossbowGate;         // true → go to Sniper
    public WeaponClass RequiredWeaponClass;
    public float CurrentDamage;
    public float RequiredDamage;
}
```

### 6.6 Controller class (new)

```
Assets/Meta/Missions/Gate/NotEnoughResourcesPopupController.cs
implements IReceivesArgs<NotEnoughResourcesPopupArgs>
```

- Renders title, body, action button.
- Action button: closes popup, then selects the appropriate mission type tab.
- "Later" button: closes popup (`Destroy(gameObject)`).

### 6.7 Popup registration
- ScreenRegistry ID: `"not_enough_resources_popup"` (new entry).
- New prefab.

---

## 7. DamageGatePopup Simplification

### 7.1 Current state
The DamageGatePopup is shown when the player taps Play on a blocked mission. It has:
- Long inline `customHint` strings (assembled per gate type in `PlayMissionPresenter`)
- Separate tutorial block (one-time, shown once per gate type)
- Always routes to weapons screen

### 7.2 Problem
Text is verbose and the tutorial duplication with the new Popup A/B system causes redundancy. The gate popup is not the right place to explain grind strategy — that's Popup B's job.

### 7.3 Proposed changes

**Shortened content:**
- Title: `"Your {WeaponClass} is too weak"` (remove custom long titles)
- Current/Required damage: keep as-is
- Hint: single short line only — no multi-sentence explanation
  - `CanUpgradeToPass = true`: `"Upgrade your {WeaponClass} or buy a stronger one."`
  - `CanUpgradeToPass = false`: `"Buy a stronger {WeaponClass} to pass this gate."`
- **Remove tutorial text block entirely.** The new Popup A/B system handles progressive guidance more naturally.

**Action button:**
Extend `DamageGatePopupArgs` with a new field `ActionType`:

```csharp
public enum GatePopupAction
{
    GoToWeapons,    // existing behavior
    PlayContracts,  // new: navigate to Contracts tab
    PlaySniper,     // new: navigate to Sniper tab
}
```

- If `ActionType = GoToWeapons` → "Open Weapons" (existing)
- If `ActionType = PlayContracts` → "Play Contracts"
- If `ActionType = PlaySniper` → "Play Sniper"

**Who decides the action?**
`PlayMissionPresenter.ShowDamageGatePopup()` should call `NextStepRecommendationService.Evaluate()` to determine the action type instead of building hints inline. This ensures the gate popup and the proactive popup A/B use the same decision logic.

**Tutorial flags:**
- Remove `_firstCampaignGateShown`, `_firstBossGateShown`, `_firstForcedPurchaseShown` from `PlayMissionPresenter` after the tutorial text block is removed from the popup.
- The new Popup A/B system provides the "first time" educational value. This simplifies `PlayMissionPresenter` substantially.

**Note:** Do not remove the `TutorialText` field and `_tutorialRoot` from the prefab immediately — mark them obsolete and clean up in a separate pass after verifying the new popups are working.

---

## 8. Weapon Screen Highlight Plan

### 8.1 Goal
When navigating to WeaponSelectionScreen from Popup A, highlight the relevant action button so the player knows what to do without hunting.

### 8.2 New arg field

Add to `WeaponSelectionArgs`:

```csharp
public enum WeaponHighlightMode { None, Upgrade, Buy }
public WeaponHighlightMode HighlightMode { get; }
```

Constructor gains optional `highlightMode = WeaponHighlightMode.None`.

### 8.3 Implementation in WeaponSelectionScreen

After `ApplyArgs(args)` applies the class filter and preselects the weapon:
- If `args.HighlightMode == Upgrade`: apply a DOTween pulse to the Upgrade button in `WeaponPricePanelView`.
- If `args.HighlightMode == Buy`: apply a DOTween pulse to the Purchase button in `WeaponPricePanelView`.

DOTween pulse pattern (consistent with existing use in the project):
```
// Scale pulse: 1.0 → 1.05 → 1.0, looping, ease InOutSine
button.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)
```

Stop the tween when:
- The screen closes (OnDestroy or on args change)
- The player taps the highlighted button (upgrade/purchase success)

### 8.4 Scope constraint
Only highlight within the already-selected weapon entry. Do not auto-scroll to or force-select a specific weapon in the list — `PreselectWeaponId` already handles selection; highlighting is just a visual pulse on the action button of the selected item.

### 8.5 WeaponPricePanelView access
`WeaponSelectionScreen` already holds a `_pricePanel` reference and subscribes to its events. The highlight tween should be applied to `_pricePanel`'s upgrade or buy button via a new public method:

```csharp
// in WeaponPricePanelView:
public void PulseUpgradeButton() { ... }
public void PulseBuyButton() { ... }
public void StopPulse() { ... }
```

---

## 9. Contracts/Sniper Action Button Plan

### 9.1 Goal
Popup B's action button should navigate the player to the right mission type on the map so they can press Play themselves, with minimal friction.

### 9.2 Chosen approach: Select tab via IMissionContextService

**Do NOT auto-start a mission.** Reasons:
- Auto-starting bypasses the map UX and player agency.
- Contracts missions are randomly selected internally by `MissionStartService`; starting from a popup context is safe but semantically inconsistent.
- If the auto-start fails (no eligible missions), the popup would silently do nothing — bad UX.
- The existing `DamageGatePopupController.GoToWeapons()` sets context and opens a screen — the same pattern applies here.

**Implementation:**
```
1. Close popup (Destroy(gameObject))
2. Call IMissionContextService.SelectType(MissionType.Contracts or Sniper)
   — This changes the selected tab on the map without navigating away.
3. Player now sees the map with the correct tab highlighted and can press Play.
```

**Open question:** Does `IMissionContextService` expose `SelectType(MissionType)` or equivalent? If not, an alternative is to look for `IMissionSelectorService` or fire a known event. Must verify the interface before implementation.

**Alternative fallback (if SelectType is not available):**
Navigate to the map screen with args: `nav.Open("map", new MapSelectArgs { Type = MissionType.Contracts })`. This requires the map screen to implement `IReceivesArgs<MapSelectArgs>`. Less preferred but architecturally clean.

### 9.3 Why not direct-start?
Even though `MissionStartService.TryStartSelected()` could be called after selecting the tab, triggering a scene load from inside a popup controller is fragile and bypasses the Play button flow. The player should press Play themselves.

---

## 10. Integration With StartupPopupCoordinator

### 10.1 New presenter: PostMissionNextStepPresenter

**File (new):** `Assets/Meta/Missions/Gate/PostMissionNextStepPresenter.cs`

MonoBehaviour, added to a GameObject in the MainMenu scene (alongside other startup presenters).

**Responsibilities:**
- Listen for map focus events (via coordinator's map tracking or directly via `IUINavigator.ScreenOpened` + ScreenView focus events).
- Listen for state-change triggers (upgrade, purchase, wallet change).
- On each trigger: call `NextStepRecommendationService.Evaluate()`.
- Apply anti-spam logic (§11).
- If recommendation warrants a popup: `Submit(StartupPopupRequest)` to coordinator.

### 10.2 Priority

Add to `StartupPopupPriority`:
```csharp
public const int NextStepGuidance = 90;
```

These popups appear after all one-time progression popups (ContractsMilestone = 80) but are still shown before anything else if the coordinator queue is empty. Lower number = higher priority, so 90 means they appear last in queue, after all celebratory/unlock popups.

**Rationale:** Zone complete, unlock, and milestone celebrations should always show first. Next-step guidance is practical, not celebratory, and should not interrupt the positive reward flow.

### 10.3 LogicalId strategy

Unlike one-time popups, these can repeat. The coordinator's dedup by LogicalId works per-session (cleared when dequeued). Use:
- Popup A LogicalId: `"next_step_weapon_too_weak"`
- Popup B LogicalId: `"next_step_not_enough_resources"`

The anti-spam logic in the presenter (§11) controls whether to re-submit after dismissal. The coordinator handles dedup within one map visit (prevents double-submission if map gains focus twice rapidly).

### 10.4 OnBeforeShow callback
For these repeatable popups, **do not** save a one-time flag in `OnBeforeShow`. Instead, use `OnBeforeShow` to record the "last shown state" for anti-spam tracking.

### 10.5 CustomShowAction
Not needed. Submit as a normal `PopupId` + `Args` request.

---

## 11. Anti-Spam / Repeat Rules

### 11.1 Core rule
After a popup is dismissed with "Later" (or any dismissal), do not show the same type of popup again unless something material has changed.

### 11.2 Material change events
- `IUpgradeService.OnWeaponUpgraded` — player upgraded a weapon
- Weapon purchased — need to subscribe to a purchase event (see §2.7 open question)
- `IWallet.BalanceChanged` — resources changed (currency earned)
- Mission completed — player played a mission (main menu re-initialized)

### 11.3 State version tracking

In `PostMissionNextStepPresenter`:
```
_stateVersion: int (incremented on any material change)
_stateVersionAtLastShow: int (saved when popup is submitted)
_lastShownType: NextStepRecommendationType (saved when popup is submitted)
```

**Re-submit condition:**
```
currentRec.Type != None
AND (currentRec.Type != _lastShownType OR _stateVersion != _stateVersionAtLastShow)
```

This means:
- Same recommendation + nothing changed → no re-show.
- Same recommendation + player upgraded but still blocked → re-show (state changed).
- Different recommendation → re-show.

### 11.4 Map re-entry after weapon screen
When player returns from WeaponSelectionScreen to the map:
- `OnMapFocusGained` fires in the coordinator → `TryShowNext()` is called.
- If the presenter previously submitted and the request is still pending, `TryShowNext` will show it.
- If the presenter needs to re-evaluate: subscribe to `OnFocusGained` on the map screen view, re-run `Evaluate()`, and re-submit if appropriate.
- The coordinator's dedup prevents double-queueing if the same LogicalId is already pending.

### 11.5 Closing DamageGatePopup triggers re-evaluation
When `DamageGatePopupController` closes (player dismissed it or opened weapons):
- The popup is destroyed → coordinator's `WaitForClose` completes → `TryShowNext()`.
- The presenter may submit a Popup A/B request at this point if relevant.
- To support this: `PostMissionNextStepPresenter` should evaluate whenever the coordinator signals "popup closed and map is active," which is implicitly handled by the coordinator calling `TryShowNext` after every popup closes. The presenter just needs to ensure its request is already in the queue.

**Preferred trigger:** Re-evaluate on every map `OnFocusGained` event. This covers all re-entry scenarios cleanly.

---

## 12. Files Likely To Change

### New files
| File | Purpose |
|------|---------|
| `Assets/Meta/Missions/Gate/NextStepRecommendation.cs` | Result type + enum |
| `Assets/Meta/Missions/Gate/NextStepRecommendationService.cs` | Decision logic |
| `Assets/Meta/Missions/Gate/PostMissionNextStepPresenter.cs` | Map-aware presenter, submits to coordinator |
| `Assets/Meta/Missions/Gate/WeaponTooWeakPopupArgs.cs` | Popup A args |
| `Assets/Meta/Missions/Gate/WeaponTooWeakPopupController.cs` | Popup A controller |
| `Assets/Meta/Missions/Gate/NotEnoughResourcesPopupArgs.cs` | Popup B args |
| `Assets/Meta/Missions/Gate/NotEnoughResourcesPopupController.cs` | Popup B controller |

### Modified files
| File | Change |
|------|--------|
| `StartupPopupPriority.cs` | Add `NextStepGuidance = 90` |
| `DamageGatePopupArgs.cs` | Add `GatePopupAction ActionType` field |
| `DamageGatePopupController.cs` | Route action button by `ActionType`; shorten displayed text |
| `PlayMissionPresenter.cs` | Use `NextStepRecommendationService` for action type; remove inline hint strings; remove tutorial text/flags |
| `WeaponSelectionArgs.cs` | Add `WeaponHighlightMode HighlightMode` |
| `WeaponSelectionScreen.cs` | Apply DOTween pulse based on `HighlightMode` |
| `WeaponPricePanelView.cs` | Add `PulseUpgradeButton()`, `PulseBuyButton()`, `StopPulse()` |
| `MainMenuServices.cs` (or equivalent runtime wiring) | Instantiate and expose `NextStepRecommendationService` |

### New Unity assets (created in Editor, not via code)
| Asset | Purpose |
|-------|---------|
| `weapon_too_weak_popup` prefab | Popup A visual |
| `not_enough_resources_popup` prefab | Popup B visual |
| ScreenRegistry entries for two new popup IDs | Navigator routing |

---

## 13. Implementation Phases

### Phase 1 — Decision Service (no UI)
1. Create `NextStepRecommendation.cs` (type + enum).
2. Create `NextStepRecommendationService.cs` with full `Evaluate()` logic.
3. Wire into `MainMenuServices` / `MainMenuRuntime`.
4. Write unit-level smoke test in isolation (manual, in-editor log output).

**Goal:** Verify the logic produces correct recommendations for each gate scenario before any UI.

### Phase 2 — DamageGatePopup Simplification
1. Add `GatePopupAction` enum and `ActionType` field to `DamageGatePopupArgs`.
2. Update `DamageGatePopupController` to route button by `ActionType`.
3. Shorten hint text in `DamageGatePopupController` default strings.
4. Update `PlayMissionPresenter` to call `NextStepRecommendationService.Evaluate()` for action type, remove inline hint assembly and tutorial text/flags.
5. Remove tutorial text panel from prefab (or hide it permanently by removing the field default).

**Goal:** Gate popup is cleaner and uses the same decision logic as the new system.

### Phase 3 — Popup A (Weapon Too Weak)
1. Create `WeaponTooWeakPopupArgs.cs` and `WeaponTooWeakPopupController.cs`.
2. Create prefab and register in ScreenRegistry as `"weapon_too_weak_popup"`.
3. Create `PostMissionNextStepPresenter.cs` — map focus + state tracking, submission logic (Popup A only).
4. Add to MainMenu scene.
5. Test: block Campaign, verify Popup A shows on map return; verify weapon screen opens with correct class.

### Phase 4 — Popup B (Not Enough Resources)
1. Create `NotEnoughResourcesPopupArgs.cs` and `NotEnoughResourcesPopupController.cs`.
2. Create prefab and register in ScreenRegistry as `"not_enough_resources_popup"`.
3. Extend `PostMissionNextStepPresenter` to evaluate and submit Popup B when A does not apply.
4. Implement Contracts/Sniper tab selection in popup controller action button (verify `IMissionContextService.SelectType` or equivalent).
5. Test: block Campaign with no resources, verify Popup B shows; verify correct tab is selected on map.

### Phase 5 — Weapon Screen Highlight
1. Add `WeaponHighlightMode` to `WeaponSelectionArgs`.
2. Add `PulseUpgradeButton()` / `PulseBuyButton()` / `StopPulse()` to `WeaponPricePanelView`.
3. Update `WeaponSelectionScreen.ApplyArgs()` to trigger the pulse.
4. Update Popup A controller to pass `HighlightMode` in `WeaponSelectionArgs`.
5. Test: verify pulse appears on correct button and stops cleanly.

### Phase 6 — Anti-Spam Verification
1. Test: dismiss Popup A with "Later" → return to map → verify no re-show.
2. Test: dismiss Popup A → upgrade one level (still blocked) → return to map → verify Popup A re-shows.
3. Test: Popup A shows → player buys weapon and passes gate → return to map → verify no popup.
4. Test: Popup A → B chaining scenario (upgrade enough to clear "can upgrade" but not enough resources).

---

## 14. Risks / Open Questions

### 14.1 Popup spam if wallet fluctuates
**Risk:** `IWallet.BalanceChanged` fires frequently (e.g., on every token increment during a reward animation). Each change triggers re-evaluation and potentially re-submission.
**Mitigation:** In `PostMissionNextStepPresenter`, debounce wallet change events: only re-evaluate after the change settles (1–2 frame delay using a coroutine or a dirty flag checked on next `LateUpdate`).

### 14.2 Conflict with StartupPopupCoordinator queue
**Risk:** If a zone complete, loop transition, or weapon unlock popup is queued at high priority, Popup A/B (priority 90) will be delayed indefinitely if many higher-priority popups are pending.
**Mitigation:** Priority 90 is the lowest (last) in the queue — this is intentional. Celebratory popups show first, guidance follows. The player sees one popup at a time; this is the correct UX. Document it as designed behavior.

### 14.3 Player pulled away from map too often
**Risk:** If Popup A shows every time the player opens the map while blocked, it becomes annoying.
**Mitigation:** The anti-spam rule (§11.3) prevents re-show if nothing changed since last dismissal. After "Later", the popup stays dormant until a material state change occurs. This is the key protection.

### 14.4 Recommendation logic diverging from actual gate logic
**Risk:** `NextStepRecommendationService` re-implements gate evaluation independently from `MissionGateService`. If gate formulas change (e.g., loop multiplier adjustments), only one may be updated.
**Mitigation:** `NextStepRecommendationService` must call `MissionGateService` directly for gate checks — it should **not** re-implement gate math. The `MissionGateResult` returned by gate service is the authoritative gate data. The service only adds the "can I act on this now?" layer on top.

### 14.5 Resources change after popup is queued but before shown (RESOLVED in Phase 1 investigation)
`StartupPopupRequest` has `OnBeforeShow` (Action) but **no** `Func<bool> ShouldShow` / pre-show validation.
**Resolution:** Add `Func<bool> ShouldShow` to `StartupPopupRequest` in a future phase when needed. The coordinator would call it just before displaying and skip the popup if it returns false.
For Phase 2 (`PostMissionNextStepPresenter`), use the `OnBeforeShow` callback to re-run `Evaluate()` and call `Destroy` on the popup immediately if the recommendation is now stale. This avoids modifying the coordinator.

### 14.6 Contracts not unlocked yet
**Risk:** `PlayContracts` recommendation is computed, but Contracts are locked by company level.
**Mitigation:** The decision algorithm already checks `contractsAvail.CanPlay`. If Contracts is locked, return `None` rather than `PlayContracts`. The player cannot act; showing a popup directing them somewhere they cannot access is harmful UX.

### 14.7 Sniper not unlocked for Crossbow gate guidance
**Risk:** Crossbow gate is blocked and player should go to Sniper, but Sniper is locked.
**Mitigation:** If Sniper is locked, fall back to `PlayContracts` with body text adjusted: "Earn tokens from Contracts for now. Sniper missions unlock later." Alternatively suppress Popup B entirely if neither Contracts nor Sniper can help. Decide per design preference — document as open question for designer.

### 14.8 Weapon purchase event (RESOLVED in Phase 1 investigation)
No `IPurchaseService` exists. Purchase is handled inside `WeaponSelectionScreen` via `WeaponPricePanelView.OnPurchaseClicked` with no public event.
**Resolution (designer-confirmed):** Use `IWallet.BalanceChanged` as proxy for re-evaluation. A wallet change after returning to the map is sufficient signal to re-run `Evaluate()`. Map focus re-evaluation also covers most cases. No new event needed for Phase 2.

### 14.9 Direct-start path for Popup B (RESOLVED in Phase 1 investigation)
`IMissionContextService` only has `BuildSelectedContext()` — it does NOT support type switching.
However, `IMetaProgressWrite` (already on `MainMenuServices.Progress`) has `SelectMissionType(MissionType)`.
`MissionStartService.TryStartSelected()` calls `_context.BuildSelectedContext()` internally, which reads `_progress.SelectedMissionType`. So calling `SelectMissionType()` first and then `TryStartSelected()` produces a valid Contracts or Sniper start.

**Recommended direct-start sequence for Popup B controller:**
```
1. services.Progress.SelectMissionType(MissionType.Contracts)  // or Sniper
2. var result = services.MissionStart.TryStartSelected()
3. if result.Started → ScenesLoader.Instance.LoadScene(result.MissionToLoad.Scene.ScenePath)
4. else → fallback: log warning, dismiss popup gracefully
```

Side effect: `SelectMissionType` fires `ProgressData.OnMissionTypeChanged` → `NotifyStateChanged` → all
presenters refresh. This is harmless — it correctly updates the map UI to show the selected tab.

Popup B controller will need a reference to `MainMenuServices`. Obtain via `MainMenuRuntime.Instance.Services`.

### 14.10 Zone 3 / no-boss edge case
**Risk:** Zone 3 has no boss. After Campaign complete in Zone 3, `GetRecommendedType()` returns null (loop transition handles this). The new service should match this behavior.
**Mitigation:** In `NextStepRecommendationService`, after Campaign complete: if `!zone.HasBoss()` → return `None`. The loop transition popup (priority 10) already handles this case.

### 14.11 CanUpgradeToPass vs CanAffordUpgrade confusion
**Risk:** `MissionGateResult.CanUpgradeToPass` means "upgrade path exists to max" — not "player can afford the next upgrade level now." The recommendation service must check wallet separately.
**Mitigation:** Always check `IUpgradeService.CanUpgrade(weaponId)` (which internally checks the wallet) after confirming `CanUpgradeToPass = true`. These are two distinct conditions that must both be true for a `UpgradeWeapon` recommendation.

### 14.12 Next-tier weapon unlock level check
**Risk:** The `BuyWeapon` recommendation requires checking that the next-tier weapon is available for purchase (unlock campaign level ≤ current level). Getting this data requires `WeaponCatalog` access and knowing the unlock level per weapon definition.
**Mitigation:** `WeaponCatalog` is already used in `WeaponSelectionScreen`. Check if it's resolvable via `WeaponsInitializer.WeaponCatalog` from a non-MonoBehaviour service, or inject it at construction. If `WeaponCatalog` is a `ScriptableObject` it may need to be passed as a dependency. Design this injection point before Phase 1.
