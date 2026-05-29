# Meta: Recommended Mission Hint on Map

## Audit Findings

### MissionSelectorButtonPresenter
- One instance per mission type (Campaign, Boss, Contracts, Sniper) as children of `ZoneMapPresenter`.
- Serialized `_missionType` field identifies which type each button handles.
- Already has `_highlight` (selected state) and `_lockedIcon` (unavailable state) visual slots.
- `Refresh()` subscribes to `MainMenuServices.OnMenuStateChanged` — fires on every state change (mission select, mission complete, weapon upgrade, etc.).
- `BuildContextFor(type)` clones the current selected context and swaps the type+mission reference. Used for per-type availability checks without mutating progress.
- Campaign button is hidden (`gameObject.SetActive(false)`) when campaign is complete AND boss is unlocked.

### ZoneMapPresenter
- Discovers all `MissionSelectorButtonPresenter` children in `OnEnable`, calls `Init(services)` on each, then calls `services.NotifyMenuStateChanged()` to force an initial refresh.
- No explicit list kept; buttons refresh themselves via the event.

### Availability checks (MissionAvailabilityService)
| Type | CanPlay = true when | Block reasons |
|---|---|---|
| Campaign | `CheckCampaignGate` passes | `CampaignDamageTooLow` |
| Boss | IsBossUnlocked() AND `CheckBossGate` passes | `BossNotUnlocked`, `BossCrossbowDamageTooLow` |
| Contracts | companyLevel >= unlock threshold | `ContractsLockedByCompanyLevel` |
| Sniper | companyLevel >= unlock threshold AND `CheckSniperGate` passes | `SniperLockedByCompanyLevel`, `SniperDamageTooLow` |

### ZoneData helpers used
- `zone.IsCampaignComplete()` — all campaign missions in this zone done.
- `zone.HasBoss()` — zone has a boss segment with at least one mission.
- `zone.IsBossUnlocked()` — HasBoss() && IsCampaignComplete().

### Map refresh triggers
Every call to `services.NotifyMenuStateChanged()` triggers `Refresh()` on all buttons.
This is called after: mission completion, weapon purchase/upgrade, mission type selection, zone change, and map screen open.

---

## Recommendation Priority Logic

Implemented in `RecommendedMissionHintService.GetRecommendedType(services)`:

```
1. Campaign NOT complete
   a. GetAvailability(Campaign).CanPlay == true  →  Campaign
   b. Gate blocked (CampaignDamageTooLow)
      - GetAvailability(Contracts).CanPlay == true  →  Contracts
      - Contracts locked by company level           →  Campaign  (fallback: must keep playing to unlock Contracts)

2. Campaign complete, zone has no boss (Zone 3)
   →  null  (loop transition handles next progression step; no hint shown)

3. Campaign complete, zone has boss
   a. GetAvailability(Boss).CanPlay == true           →  Boss
   b. Reason == BossCrossbowDamageTooLow
      - GetAvailability(Sniper).CanPlay == true       →  Sniper
      - Sniper locked or damage too low               →  null
   c. Other boss block reason (shouldn't occur)       →  null
```

### Fallback behavior summary
| Scenario | Fallback |
|---|---|
| Campaign gate blocked, Contracts not unlocked yet | Campaign (still the primary path) |
| Boss gate blocked, Sniper locked by company level | null — no hint shown |
| Boss gate blocked, Sniper damage gate also blocked | null — no hint shown |
| Zone 3 (no boss), campaign complete | null — no hint shown |

---

## Visual / Animation

- Each `MissionSelectorButtonPresenter` has a new `[SerializeField] private GameObject _recommendedHint` field.
- The hint is a child GameObject that contains an "!" icon (Image + sprite set up in prefab).
- `SetRecommendedHint(bool show)` method:
  - **Show**: activates GO, starts a DOTween `DOScale(1.2f, 0.6f)` infinite Yoyo ping-pong with `Ease.InOutSine`. Guard: if tween is already active, does nothing.
  - **Hide**: kills tween, resets `localScale` to `Vector3.one`, deactivates GO.
- `SetLink(_recommendedHint)` ensures the tween auto-kills if the hint GO is destroyed.
- `OnDestroy` kills the tween for safety.
- Repeated `Refresh()` calls do not create duplicate tweens.

---

## Files Changed

| File | Change |
|---|---|
| `Assets/Meta/Missions/Gate/RecommendedMissionHintService.cs` | **New** — static service, returns `MissionType?` |
| `Assets/Meta/Missions/Controllers/MissionSelectorButtonPresenter.cs` | **Modified** — `using DG.Tweening`, `_recommendedHint` field, `_hintTween` field, `SetRecommendedHint()`, updated `Refresh()`, `OnDestroy` tween kill |

No balance values, gate rules, unlock rules, or rewards were changed.

---

## Inspector Setup Required

For each of the 4 `MissionSelectorButtonPresenter` prefab variants (Campaign, Boss, Contracts, Sniper):

1. Add a child GameObject named **`Recommended Hint`** (or any name).
2. Add an `Image` component to it; assign the **"!" exclamation sprite**.
3. Position/size as desired (small badge in a corner of the button).
4. Leave `Recommended Hint` **inactive by default** in the prefab (the script will activate it when needed).
5. In the `MissionSelectorButtonPresenter` Inspector, assign the new **Recommended Hint** slot.

If a button's `_recommendedHint` is left unassigned, `SetRecommendedHint` silently does nothing — the hint feature is opt-in per button.

---

## Test Checklist

### Hint appears on the correct button
- [ ] Fresh player, first few Campaign missions available: hint on Campaign.
- [ ] Campaign gate blocked (underpowered weapon): hint on Contracts.
- [ ] Campaign gate blocked, Contracts not yet unlocked (early company level): hint on Campaign.
- [ ] All Campaign missions complete in Zone 1, Boss gate passes: hint on Boss.
- [ ] All Campaign missions complete, Boss Crossbow gate blocked: hint on Sniper.
- [ ] All Campaign missions complete, Boss blocked, Sniper locked by company level: no hint.
- [ ] Zone 3 campaign complete (no boss): no hint.

### Only one hint is active
- [ ] At no point are two buttons showing the hint simultaneously.

### Hint updates correctly after events
- [ ] Upgrade weapon past Campaign gate: hint moves from Contracts → Campaign.
- [ ] Complete last Campaign mission in zone: hint moves from Campaign → Boss or Sniper.
- [ ] Upgrade Crossbow past Boss gate: hint moves from Sniper → Boss.
- [ ] Return to map after mission complete: hint reflects updated state immediately.

### Animation
- [ ] Hint animates (pulse scale) when visible.
- [ ] No duplicate tweens after multiple rapid `Refresh()` calls.
- [ ] Hint scale resets to 1 after hiding.
- [ ] Opening/closing the map screen multiple times does not accumulate tweens.

### Edge cases
- [ ] Campaign button hidden (campaign complete + boss unlocked): no tween leak, hint GO inactive.
- [ ] Unassigned `_recommendedHint` slot: no NullReferenceException.

---

## Risks / Known Limitations

| Risk | Mitigation |
|---|---|
| `RecommendedMissionHintService` calls `BuildSelectedContext()` once per button per refresh (up to 4 calls). | Negligible cost with 4 buttons; service is pure/stateless. |
| Context built for non-Campaign types uses `GlobalCampaignIndex`/`CompanyLevel` from the *currently selected* mission rather than the target type. This is the same approach as `MissionSelectorButtonPresenter.BuildContextFor()` and does not affect gate checks for Boss/Contracts/Sniper. | Acceptable; consistent with existing pattern. |
| Zone 3 shows no hint after campaign complete (intentional). This may confuse some players. | Loop transition popup already handles this moment. Document in QA. |
| If `_recommendedHint` is not configured in the prefab, the feature silently does nothing for that button. | Inspector checklist above. No runtime error. |
