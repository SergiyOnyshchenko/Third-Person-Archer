# Meta Reward Beats — Phase B

**Date:** 2026-05-28
**Branch:** french
**Based on:** Meta_GameDesign_Audit_RU.md, Meta_Player_Motivation_Ideas_RU.md, Meta_Core_Clarity_PhaseA.md
**Scope:** Zone Complete Celebration, Boss Victory Celebration, Contracts Milestone Rewards, Mid-Zone Beat audit

---

## Audit Findings

### 1. Mission completion flow

`MissionCompletionService.Complete()` (called from gameplay scene via `VictoryScreenSubState`) handles all completion logic:

| Mission Type | Progress step |
|---|---|
| Campaign | `zone.AdvanceMission(Campaign)` — moves to next campaign mission |
| Boss | `zone.AdvanceMission(Boss)` → `AdvanceToNextZoneOrLoop(zoneIndex)` — selects next zone or increments loop |
| Contracts | `_loopData.IncreaseContractsCompleted()` — increments `ContractsCompletedIndex` |
| Sniper | `zone.AdvanceMission(Sniper)` + `_loopData.IncreaseSniperCompleted()` |

After completion, the player clicks Continue on the victory screen → `OnVictoryScreenFinished` fires → main menu scene loads. **All popups are shown in the main menu scene**, not in the gameplay scene.

### 2. Zone complete detection

`ZoneData.IsCampaignComplete()` — returns true when all missions in the Campaign segment are done. Available on every `ZoneData` in `services.ProgressData.AllZones`.

**No existing celebration UI.** When the player finishes the last campaign mission and returns to the menu, there is currently no feedback distinguishing this from completing any other campaign mission.

### 3. Boss complete detection

`ZoneData.IsBossCompleted()` — returns true when the boss segment's first mission has been completed once. `ZoneData.HasBoss()` guards this check.

**No existing celebration UI.** Boss victory currently shows the standard victory screen (title: "Boss Defeated", rewards listed). There is no follow-up popup upon returning to the main menu explaining the unlocked zone or loop transition.

Note: the existing `VictoryHeaderSegment` already shows "Boss Defeated" as the victory screen title in-game. The Phase B popup fires on return to main menu — it is additive, not replacing.

### 4. Contracts completion counter

`MetaLoopProgressData.ContractsCompletedIndex` — incremented by 1 each time a Contracts mission is completed. Accessible via `services.LoopData.ContractsCompletedIndex`. Persisted via `SaveSystem` under key `"meta_loop_contracts_completed"`.

**No milestone system exists.** Contracts runs produce identical rewards regardless of total count. No counter or milestone is shown in the Contracts UI.

### 5. Existing popup/presenter pattern (from Phase A)

`LoopTransitionPresenter.cs` / `LoopTransitionPopupController.cs` establish the exact pattern for this phase:

- A MonoBehaviour presenter hooks `MainMenuRuntime.Ready`
- Checks a condition against `services.ProgressData`
- Saves a one-time flag via `SaveSystem.Save(key, true)` BEFORE showing the popup
- Calls `ServiceLocator.TryResolve<IUINavigator>().ShowPopup(id, args)`
- If the navigator is unavailable, the flag is NOT saved → popup fires on the next session

For dynamic content (zone name, count), `IReceivesArgs<T>` is used (same as `DamageGatePopupController`). For static content (loop transition), serialized strings in `Awake()` are used without `IReceivesArgs`.

Phase B uses `IReceivesArgs<CelebrationPopupArgs>` to support dynamic content across all three celebration types with one reusable popup controller.

### 6. Zone 3 (last zone, no boss) — already handled

`LoopTransitionPresenter` (Phase A) fires when Zone 3 Campaign is complete AND all prior zones' bosses are done. `ZoneCompletePresenter` must NOT fire for the last zone that has no boss, to avoid a duplicate popup in the same session. The implementation skips this case explicitly (`isLastNoBoss` guard).

### 7. Mid-Zone Reward Beat — audit result

**Decision: document as follow-up only.**

A mid-zone beat for Zone 2 would require tracking progress at exactly `completedOnceCount == totalMissions / 2`. The data is available (`MissionSegmentData.GetCompletedOnceCount()`), but it requires per-zone-per-count save keys and is complex to fire exactly once at the mid-point. Given that Zone 2 already gets a Zone Complete popup at the end, the marginal value of a mid-zone popup is low relative to the added complexity. See Recommended Follow-up section.

### 8. Files that need changes

| File | Change Type |
|---|---|
| `Assets/Meta/Missions/Gate/CelebrationPopupArgs.cs` | New — data class |
| `Assets/Meta/Missions/Gate/CelebrationPopupController.cs` | New — popup controller, `IReceivesArgs<CelebrationPopupArgs>` |
| `Assets/Meta/Missions/Gate/ZoneCompletePresenter.cs` | New — presenter MonoBehaviour |
| `Assets/Meta/Missions/Gate/BossVictoryPresenter.cs` | New — presenter MonoBehaviour |
| `Assets/Meta/Missions/Gate/ContractsMilestonePresenter.cs` | New — presenter MonoBehaviour |

No existing files were modified. No balance, gate, weapon, or reward calculation files are touched.

---

## Changes Made

### 1. `CelebrationPopupArgs.cs` (new)

Minimal data class. Holds `Title` (string) and `Body` (string). Passed from any of the three presenters to `CelebrationPopupController` via the navigator.

### 2. `CelebrationPopupController.cs` (new)

Generic popup MonoBehaviour implementing `IReceivesArgs<CelebrationPopupArgs>`.

- Serialized fields: `_titleText` (TMP), `_bodyText` (TMP), `_closeButton` (Button), `_fallbackTitle` (string), `_fallbackBody` (string)
- `Awake()`: applies fallback text, hooks close button
- `ApplyArgs(args)`: overrides text with dynamic content from the presenter
- `OnClose()`: destroys the GameObject (consistent with LoopTransitionPopupController)
- One prefab, one ScreenRegistry entry (`"CelebrationPopup"`) — reused by all three presenters

### 3. `ZoneCompletePresenter.cs` (new)

Checks per zone on each menu startup:
- `zone.IsCampaignComplete()` → true
- `!zone.HasBoss() && i == zones.Count - 1` → skip last no-boss zone (LoopTransitionPresenter already handles it)
- `SaveSystem.Load($"zone_complete_popup_shown_{zone.ID}", false)` → false (not yet shown)

On first match: saves flag, builds `CelebrationPopupArgs`, shows popup. Shows at most one zone complete popup per session (breaks after first match).

**Body text for zones with a boss:**
> You have cleared all Campaign missions in {ZoneName}!
>
> The Boss is now unlocked.
> Upgrade your Crossbow and face it when ready.

**Body text for bossless mid-game zones (edge case):**
> You have cleared all Campaign missions in {ZoneName}!
>
> Continue your journey in {NextZoneName}.

Save key format: `"zone_complete_popup_shown_{zone.ID}"` — one key per zone ID.

### 4. `BossVictoryPresenter.cs` (new)

Checks per zone with a boss on each menu startup:
- `zone.HasBoss()` → true
- `zone.IsBossCompleted()` → true
- `SaveSystem.Load($"boss_victory_popup_shown_{zone.ID}", false)` → false

On first match: saves flag, builds args with next-zone or loop info, shows popup.

**Body text when next zone exists:**
> You have defeated the Boss of {ZoneName}!
>
> {NextZoneName} is now open!
> Keep fighting and push through the next campaign.

**Body text when this was the last zone's boss (loop case):**
> You have defeated the Boss of {ZoneName}!
>
> You have conquered all Bosses!
> A new challenge loop begins — stronger gates, better rewards. Your weapons and progress carry over.

Save key format: `"boss_victory_popup_shown_{zone.ID}"`

### 5. `ContractsMilestonePresenter.cs` (new)

Milestone thresholds: `{1, 5, 10, 20}` completed Contracts.

On menu startup:
1. Reads `services.LoopData.ContractsCompletedIndex` (current total)
2. Reads `SaveSystem.Load("contracts_milestone_highest_shown", 0)` (highest threshold already shown)
3. Finds the highest threshold in `Milestones` where `completed >= m && highestShown < m`
4. If found: saves `targetMilestone` as new highest shown, shows popup

Only one milestone popup per session (highest unclaimed milestone takes priority).

**Title for threshold 1:** `"First Contract Complete!"`
**Title for thresholds 5/10/20:** `"Contract Milestone: {N} Completed!"`

**Body for threshold 1:**
> You completed your first Contract!
>
> Contracts are the best way to earn tokens for all weapon classes.
> Run them regularly to keep your upgrades moving.

**Body for thresholds 5/10/20:**
> You have completed {N} Contracts!
>
> Keep running Contracts to earn tokens and unlock stronger weapons.
> Every run counts toward your next upgrade.

**Note: No actual currency rewards are granted.** The milestone popup is UI feedback only. See Recommended Follow-up for reward values.

---

## Player-Facing English Texts Added

### Zone Complete popup

**Title:** `Zone Campaign Complete!`

**Body (zone with boss):**
> You have cleared all Campaign missions in {ZoneName}!
>
> The Boss is now unlocked.
> Upgrade your Crossbow and face it when ready.

### Boss Victory popup

**Title:** `Boss Defeated!`

**Body (next zone exists):**
> You have defeated the Boss of {ZoneName}!
>
> {NextZoneName} is now open!
> Keep fighting and push through the next campaign.

**Body (last boss, loop begins):**
> You have defeated the Boss of {ZoneName}!
>
> You have conquered all Bosses!
> A new challenge loop begins — stronger gates, better rewards. Your weapons and progress carry over.

### Contracts Milestone popup

**Title (1st):** `First Contract Complete!`

**Title (5/10/20):** `Contract Milestone: {N} Completed!`

**Body (1st):**
> You completed your first Contract!
>
> Contracts are the best way to earn tokens for all weapon classes.
> Run them regularly to keep your upgrades moving.

**Body (5/10/20):**
> You have completed {N} Contracts!
>
> Keep running Contracts to earn tokens and unlock stronger weapons.
> Every run counts toward your next upgrade.

---

## Files Changed

| File | Status |
|---|---|
| `Assets/Meta/Missions/Gate/CelebrationPopupArgs.cs` | New |
| `Assets/Meta/Missions/Gate/CelebrationPopupController.cs` | New |
| `Assets/Meta/Missions/Gate/ZoneCompletePresenter.cs` | New |
| `Assets/Meta/Missions/Gate/BossVictoryPresenter.cs` | New |
| `Assets/Meta/Missions/Gate/ContractsMilestonePresenter.cs` | New |
| `Assets/Meta/Docs/Meta_Reward_Beats_PhaseB.md` | New (this file) |

---

## Assets / Prefabs — Manual Inspector Setup Required

### A. New `Celebration Popup Screen.prefab`

1. Duplicate any existing simple popup prefab (e.g., `Loop Transition Popup Screen.prefab` from Phase A, or another popup).
2. Rename to `Celebration Popup Screen.prefab`.
3. Place in `Assets/Scripts/UI New/Prefabs/PopUps/`.
4. Remove or repurpose the existing content (title text, body text, close button).
5. Add the `CelebrationPopupController` component.
6. Wire the serialized fields:
   - `_titleText` → a `TextMeshProUGUI` for the title
   - `_bodyText` → a `TextMeshProUGUI` for the body (allow word wrap, set preferred height as flexible)
   - `_closeButton` → a `Button` component
   - `_fallbackTitle` → leave as "Milestone!" or clear it (presenters always pass args)
   - `_fallbackBody` → leave empty
7. Style the popup to look celebratory: larger title font, centered text, the close button labeled "Continue" or "OK".

### B. Register in ScreenRegistry Popups

Open `Assets/Scripts/UI New/Prefabs/PopUps/!ScreenRegistry Popups.asset`.
Add one entry:
- ID: `"CelebrationPopup"`
- Prefab: the new `Celebration Popup Screen.prefab`

This single entry is reused by all three presenters (ZoneComplete, BossVictory, ContractsMilestone). Each call to `nav.ShowPopup("CelebrationPopup", args)` instantiates a fresh popup instance.

### C. Add presenter MonoBehaviours to main menu scene

Open the main menu scene (`Assets/Scenes/MainMenu.unity`).

Add three new empty GameObjects:
1. Name: `ZoneCompletePresenter` — attach `ZoneCompletePresenter` component. Set `_popupId = "CelebrationPopup"`.
2. Name: `BossVictoryPresenter` — attach `BossVictoryPresenter` component. Set `_popupId = "CelebrationPopup"`.
3. Name: `ContractsMilestonePresenter` — attach `ContractsMilestonePresenter` component. Set `_popupId = "CelebrationPopup"`.

All three can be parented under one empty parent GameObject named `RewardBeatPresenters` for organisation.

---

## Milestone Reward Values (UI-Only — Reward Granting Not Yet Implemented)

Recommended conservative reward values for a future implementation pass:

| Milestone | Suggested Reward | Rationale |
|---|---|---|
| 1st Contract | +5 AllTokens (1 per class) | Welcome bonus; minimal economy impact |
| 5th Contract | +10 AllTokens (2 per class) | Reinforces habit formation |
| 10th Contract | +20 AllTokens (4 per class) | Mid-game encouragement; equals ~1 upgrade level |
| 20th Contract | +1 Cash bonus (50) | Late-game; cash surplus already high, keep it symbolic |

**Implementation note:** To grant rewards safely, call `Economy.Wallet.Add(CurrencyType.Cash, amount)` and `Economy.Wallet.Add(currencyType, amount)` from within `ContractsMilestonePresenter.CheckAndShow()` BEFORE calling `nav.ShowPopup()`. The flag is already saved before the popup fires, ensuring no double-grant on the same session. Risk: if the Wallet is null (service not yet initialised on menu startup), the grant would be skipped. Guard with `Economy.Wallet != null`.

---

## Test Checklist

### Zone Complete popup

- [ ] Complete the last Campaign mission of Zone 1 (Japanese), return to main menu.
      Popup appears: title "Zone Campaign Complete!", body mentions "Japanese" and "Boss is now unlocked".
- [ ] Dismiss popup, return to main menu again (or reload): popup does NOT appear again.
- [ ] Complete Zone 2 (North) last Campaign mission. Same behaviour. Body mentions "North".
- [ ] Zone 3 (Wild West) last Campaign mission: Zone Complete popup does NOT appear (covered by LoopTransitionPresenter).
- [ ] Zone 1 popup body does NOT mention zone that hasn't been started yet.

### Boss Victory popup

- [ ] Defeat Zone 1 Boss, return to main menu.
      Popup appears: title "Boss Defeated!", body mentions "Japanese" (or Zone 1 name) and "{Zone 2 name} is now open!".
- [ ] Dismiss popup, return to main menu again: popup does NOT appear again.
- [ ] Defeat Zone 2 Boss, return to main menu.
      Popup appears, body mentions Zone 3 opening.
- [ ] If Zone 2 is the last zone with a boss, body instead mentions loop transition.

### Contracts Milestone popup

- [ ] Complete 1st Contract, return to main menu: "First Contract Complete!" popup appears.
- [ ] Complete 2nd, 3rd, 4th: no popup.
- [ ] Complete 5th Contract, return to main menu: "Contract Milestone: 5 Completed!" popup appears.
- [ ] Repeat for 10th and 20th milestones.
- [ ] Milestone popup never appears twice for the same threshold.
- [ ] If a player jumps from 0 to 7 contracts in one session (e.g., debug skip), only the highest milestone (5) popup fires.

### Popup rendering

- [ ] Title text is readable (large, prominent font).
- [ ] Body text wraps correctly on mobile resolution.
- [ ] Close button is tappable on mobile (minimum 44px touch target).
- [ ] Popup appears on top of all other UI (sort order / canvas layer correct).
- [ ] Only one popup is shown at a time (see Risk R4 below).

### Regression

- [ ] Campaign mission launch still works after adding presenters to scene.
- [ ] Boss mission launch still works.
- [ ] Contracts mission launch still works.
- [ ] LoopTransitionPresenter (Phase A) still fires correctly for Zone 3 Campaign complete.
- [ ] DamageGatePopupController tutorial sections (Phase A) still appear correctly.
- [ ] No null reference exceptions in console on main menu startup.

---

## Risks and Limitations

### R1 — Multiple popups in one session

If multiple conditions are simultaneously true (e.g., save state was in an unusual state when testing), two or more popups could appear in the same session. In normal play this cannot happen (only one mission type completed per run). No coordination logic is implemented between presenters; this is acceptable risk for the current scope.

### R2 — Zone ID must be stable

`ZoneCompletePresenter` and `BossVictoryPresenter` use `zone.ID` as part of their save keys. If the `_id` field in any `ZoneData` ScriptableObject is changed, the popup will re-fire for that zone. The `ZoneData` Inspector tooltip already warns "Do not change once shipped." Consistent with existing save patterns in the codebase.

### R3 — LoopTransitionPresenter fires on the same session as ZoneCompletePresenter (Zone 3)

`ZoneCompletePresenter` skips Zone 3 (the last no-boss zone) to prevent this. If a future zone is added as the new last zone without a boss, the `isLastNoBoss` guard must be reviewed. Currently safe.

### R4 — Navigator availability at menu startup

Same risk as `LoopTransitionPresenter` (Phase A, R4). If `ServiceLocator.TryResolve<IUINavigator>` fails, the popup is not shown but the flag IS already saved. The popup will never appear. Mitigation: ensure the navigator is registered before `MainMenuRuntime.Ready` fires, as is already the case.

### R5 — CelebrationPopupController prefab not wired

If `_titleText` or `_bodyText` is null (fields not wired in Inspector), `ApplyArgs` silently skips assignment. The popup will appear with empty/fallback text. No crash. Detected during testing step "Title text is readable".

### R6 — Contracts milestone reward granting not implemented

Milestone popups are UI feedback only. No currency rewards are granted. Economy impact is zero. This is intentional for Phase B. See Milestone Reward Values section for recommended values and a safe implementation path.

### R7 — ContractsMilestonePresenter saves the HIGHEST reached milestone

If the player completes 7 contracts before the milestone at 5 has fired (e.g., first time opening the game after accumulating runs), the save key records `5` as the highest shown. The threshold at 1 is NOT shown separately. This is intentional — only the highest unclaimed threshold fires per session. The player will not see multiple contract milestone popups for "catching up". If showing all missed milestones is desired, the logic must be changed to iterate and show each, with a delay or queue.

---

## Mid-Zone Reward Beat — Recommended Follow-up

**Not implemented in Phase B.** Adding a mid-zone beat (e.g., after mission 4 of 8 in Zone 2) would provide a pacing break in Zone 2's dense gate wall.

**What is needed:**
- A `MidZoneRewardPresenter` similar to `ZoneCompletePresenter`
- Save key per zone: `"mid_zone_popup_shown_{zone.ID}"`
- Condition: `zone.GetSegmentByType(MissionType.Campaign).GetCompletedOnceCount() >= totalMissions / 2`
- Body: "You're halfway through {ZoneName}! Keep going — the Boss awaits at the end."
- Optional: small token bonus (5 AllTokens) to reward the beat

**Why deferred:** `GetCompletedOnceCount()` requires importing `MissionSegmentData` directly in the presenter, which adds coupling. The popup fires at a specific count (half of total), not at full completion — this is harder to test precisely. The payoff is modest given that Zone Complete and the existing gate popups already bookend Zone 2 progress. Recommended as Phase C or later.

---

## Not Done in Phase B (Future Phases)

- **Phase C:** First-time Contracts/Sniper/Boss-tab popups (mode onboarding)
- **Phase C:** Spear and Boomerang weapon class unlock popups
- **Phase B follow-up:** Contracts milestone currency rewards (grant economy rewards in addition to popup)
- **Phase B follow-up:** Mid-zone reward beat for Zone 2
- **Phase D:** Shortcut buttons in gate popups (go directly to Contracts/Sniper)
- **Phase D:** Sniper progress tracker (Crossbow damage vs Boss gate threshold)
- **Phase E:** Zone progress bar, loop badge indicator
