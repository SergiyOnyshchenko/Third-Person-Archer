using System.Collections;
using Meta.Economy;
using Meta.Weapons;
using Meta.Weapons.UI;
using UI.Core;
using UnityEngine;

/// <summary>
/// Controls which strategy PostMissionNextStepPresenter uses when the player returns
/// to the map and their next gate can be solved by upgrading or buying a weapon.
/// </summary>
public enum PostMissionGuidanceMode
{
    /// <summary>
    /// Default. Shows weapon_too_weak_popup (Popup A) for UpgradeWeapon / BuyWeapon.
    /// Player taps the popup action button to open the weapon screen.
    /// </summary>
    PopupFirst,

    /// <summary>
    /// Experimental. Skips Popup A and opens WeaponSelectionScreen automatically
    /// when the coordinator is idle and the map is active.
    /// Popup B (not_enough_resources_popup) is still shown for PlayContracts/PlaySniper.
    /// </summary>
    AutoOpenWeapons,
}

/// <summary>
/// Evaluates the player's next-step recommendation after returning to the map and
/// submits a single guidance popup to StartupPopupCoordinator when relevant.
///
/// POPUP A — Weapon Too Weak (popup id: weapon_too_weak_popup)
///   Shown when NextStepRecommendationService returns UpgradeWeapon or BuyWeapon
///   AND _guidanceMode == PopupFirst.
///
/// AUTO OPEN — WeaponSelectionScreen opened directly
///   Triggered when NextStepRecommendationService returns UpgradeWeapon or BuyWeapon
///   AND _guidanceMode == AutoOpenWeapons.
///   Respects coordinator priority: only fires when coordinator.IsIdle == true.
///   The target weapon is pre-selected and the correct button is highlighted.
///
/// POPUP B — Not Enough Resources (popup id: not_enough_resources_popup)
///   Shown when NextStepRecommendationService returns PlayContracts or PlaySniper.
///   Behavior is identical in both modes.
///
/// ANTI-SPAM
///   A, B, and auto-open all share one version-based state tracker.
///   After a popup is shown or the screen is opened, the same recommendation is not
///   re-submitted / re-opened until a material state change occurs:
///     • IWallet.BalanceChanged  — any currency change (proxy for purchases too)
///     • IUpgradeService.OnWeaponUpgraded — explicit upgrade event
///   Returning from a mission reloads the main menu scene → presenter resets →
///   fresh evaluation on next map open (intentional: one interaction per mission is fine).
///
/// SETUP
///   1. Add this MonoBehaviour to the main menu scene.
///   2. Set _weaponScreenId to match the WeaponSelectionScreen ScreenRegistry ID.
///   3. Set _mapScreenId to match the map ScreenRegistry ID (default: "map").
///   4. Ensure StartupPopupCoordinator is also in the scene.
///   5. Register weapon_too_weak_popup and not_enough_resources_popup in ScreenRegistry.
/// </summary>
public sealed class PostMissionNextStepPresenter : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("PopupFirst shows weapon_too_weak_popup before opening the weapon screen. " +
             "AutoOpenWeapons skips the popup and opens the screen directly.")]
    [SerializeField] private PostMissionGuidanceMode _guidanceMode = PostMissionGuidanceMode.PopupFirst;

    [Header("Popup IDs (must match ScreenRegistry)")]
    [SerializeField] private string _weaponTooWeakPopupId      = "weapon_too_weak_popup";
    [SerializeField] private string _notEnoughResourcesPopupId = "not_enough_resources_popup";

    [Header("Screen IDs")]
    [SerializeField] private string _weaponScreenId = "WeaponSelectionScreen";
    [SerializeField] private string _mapScreenId    = "map";

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private MainMenuServices _services;
    private bool _initialized;

    private IUINavigator _navigator;
    private ScreenView   _mapScreenView;

    // Version-based anti-spam (shared across popup and auto-open paths).
    private NextStepRecommendationType _lastSubmittedType        = NextStepRecommendationType.None;
    private int _stateVersion                                    = 0;
    private int _stateVersionAtLastSubmit                        = -1;

    // Coroutine handle — cancel if a new map-focus event fires before the previous
    // one-frame delay has elapsed.
    private Coroutine _pendingAutoOpenCoroutine;

    // Logical ID shared by both popup A and B.
    private const string LogicalId = "next_step_guidance";

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        TryInitOrSubscribe();
    }

    private void OnDisable()
    {
        if (!_initialized)
            MainMenuRuntime.Ready -= OnRuntimeReady;
    }

    private void OnDestroy()
    {
        UnsubscribeExternalEvents();
    }

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    private void TryInitOrSubscribe()
    {
        var runtime = MainMenuRuntime.Instance;
        if (runtime != null && runtime.Services != null)
        {
            Init(runtime.Services);
            return;
        }

        MainMenuRuntime.Ready -= OnRuntimeReady;
        MainMenuRuntime.Ready += OnRuntimeReady;
    }

    private void OnRuntimeReady(MainMenuServices services)
    {
        MainMenuRuntime.Ready -= OnRuntimeReady;
        Init(services);
    }

    private void Init(MainMenuServices services)
    {
        if (_initialized) return;
        _initialized = true;
        _services = services;

        // Subscribe to material-change events.
        var wallet = Meta.Economy.Economy.Wallet;
        if (wallet != null)
            wallet.BalanceChanged += OnBalanceChanged;

        var upgradeService = WeaponsInitializer.Instance?.UpgradeService;
        if (upgradeService != null)
            upgradeService.OnWeaponUpgraded += OnWeaponUpgraded;

        // Subscribe to navigator screen events for auto-open map focus tracking.
        if (ServiceLocator.TryResolve<IUINavigator>(out var nav))
        {
            _navigator = nav;
            nav.ScreenOpened += OnScreenOpened;
        }

        // Initial evaluation (covers "returned from mission" reload).
        EvaluateAndSubmit();
    }

    private void UnsubscribeExternalEvents()
    {
        var wallet = Meta.Economy.Economy.Wallet;
        if (wallet != null)
            wallet.BalanceChanged -= OnBalanceChanged;

        var upgradeService = WeaponsInitializer.Instance?.UpgradeService;
        if (upgradeService != null)
            upgradeService.OnWeaponUpgraded -= OnWeaponUpgraded;

        if (_navigator != null)
            _navigator.ScreenOpened -= OnScreenOpened;

        if (_mapScreenView != null)
            _mapScreenView.OnFocusGained.RemoveListener(OnMapFocusGained);
    }

    // -------------------------------------------------------------------------
    // Map focus tracking (AutoOpenWeapons)
    // -------------------------------------------------------------------------

    private void OnScreenOpened(ScreenView view)
    {
        if (view.ScreenId != _mapScreenId) return;

        // Capture ScreenView on first encounter to subscribe to GoBack focus events.
        if (_mapScreenView == null)
        {
            _mapScreenView = view;
            view.OnFocusGained.AddListener(OnMapFocusGained);
        }

        OnMapFocusGained();
    }

    private void OnMapFocusGained()
    {
        // Cancel any already-pending auto-open coroutine (e.g. rapid focus events).
        if (_pendingAutoOpenCoroutine != null)
        {
            StopCoroutine(_pendingAutoOpenCoroutine);
            _pendingAutoOpenCoroutine = null;
        }

        _pendingAutoOpenCoroutine = StartCoroutine(TryAutoOpenNextFrame());
    }

    // Wait one frame so the coordinator can finish processing its own ScreenOpened
    // handler (TryShowNext) before we inspect IsIdle.
    private IEnumerator TryAutoOpenNextFrame()
    {
        yield return null;
        _pendingAutoOpenCoroutine = null;
        TryAutoOpen();
    }

    private void TryAutoOpen()
    {
        if (_guidanceMode != PostMissionGuidanceMode.AutoOpenWeapons) return;
        if (_services?.NextStep == null) return;

        var rec = _services.NextStep.Evaluate();

        if (rec.Type != NextStepRecommendationType.UpgradeWeapon &&
            rec.Type != NextStepRecommendationType.BuyWeapon)
        {
            // Could be PlayContracts/PlaySniper — the coordinator submission path
            // (EvaluateAndSubmit) already handles those.
            return;
        }

        // Anti-spam: same version guard as the popup path.
        if (rec.Type == _lastSubmittedType && _stateVersion == _stateVersionAtLastSubmit)
            return;

        // Only open when no celebration/unlock popup is queued or visible.
        if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord) && !coord.IsIdle)
            return;

        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        // Record before opening to prevent re-trigger on the same state.
        _lastSubmittedType        = rec.Type;
        _stateVersionAtLastSubmit = _stateVersion;

        int campaignLevel = ResolveCampaignLevel();
        var highlightMode = rec.Type == NextStepRecommendationType.UpgradeWeapon
            ? WeaponHighlightMode.Upgrade
            : WeaponHighlightMode.Buy;

        var weaponArgs = new WeaponSelectionArgs(
            rec.RequiredWeaponClass,
            campaignLevel,
            rec.TargetWeaponId,
            highlightMode);

        nav.Open(_weaponScreenId, weaponArgs, reuseCached: true);
    }

    // -------------------------------------------------------------------------
    // Material-change handlers
    // -------------------------------------------------------------------------

    private void OnBalanceChanged(CurrencyType type, int newBalance, int delta)
    {
        _stateVersion++;
        EvaluateAndSubmit();
    }

    private void OnWeaponUpgraded(string weaponId, int newLevel)
    {
        _stateVersion++;
        EvaluateAndSubmit();
    }

    // -------------------------------------------------------------------------
    // Core popup-submission logic (PopupFirst mode, and Popup B in all modes)
    // -------------------------------------------------------------------------

    private void EvaluateAndSubmit()
    {
        if (_services?.NextStep == null) return;

        var rec = _services.NextStep.Evaluate();

        if (rec.Type == NextStepRecommendationType.None) return;

        // Anti-spam.
        if (rec.Type == _lastSubmittedType && _stateVersion == _stateVersionAtLastSubmit)
            return;

        _lastSubmittedType        = rec.Type;
        _stateVersionAtLastSubmit = _stateVersion;

        Submit(rec);
    }

    private void Submit(NextStepRecommendation rec)
    {
        if (!ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
            return;

        switch (rec.Type)
        {
            case NextStepRecommendationType.UpgradeWeapon:
            case NextStepRecommendationType.BuyWeapon:
                // In AutoOpenWeapons mode these are handled by TryAutoOpen on map focus.
                if (_guidanceMode == PostMissionGuidanceMode.AutoOpenWeapons)
                    return;
                SubmitWeaponTooWeak(coord, rec);
                break;

            case NextStepRecommendationType.PlayContracts:
            case NextStepRecommendationType.PlaySniper:
                SubmitNotEnoughResources(coord, rec);
                break;
        }
    }

    private void SubmitWeaponTooWeak(IStartupPopupCoordinator coord, NextStepRecommendation rec)
    {
        int campaignLevel = ResolveCampaignLevel();

        var args = new WeaponTooWeakPopupArgs(
            requiredWeaponClass: rec.RequiredWeaponClass,
            currentDamage:       rec.CurrentDamage,
            requiredDamage:      rec.RequiredDamage,
            isUpgrade:           rec.Type == NextStepRecommendationType.UpgradeWeapon,
            targetWeaponId:      rec.TargetWeaponId,
            weaponScreenId:      _weaponScreenId,
            campaignLevel:       campaignLevel);

        coord.Submit(new StartupPopupRequest
        {
            Priority  = StartupPopupPriority.NextStepGuidance,
            LogicalId = LogicalId,
            PopupId   = _weaponTooWeakPopupId,
            Args      = args,
        });
    }

    private void SubmitNotEnoughResources(IStartupPopupCoordinator coord, NextStepRecommendation rec)
    {
        var args = new NotEnoughResourcesPopupArgs(
            isCrossbowGate: rec.IsCrossbowPath,
            currentDamage:  rec.CurrentDamage,
            requiredDamage: rec.RequiredDamage);

        coord.Submit(new StartupPopupRequest
        {
            Priority  = StartupPopupPriority.NextStepGuidance,
            LogicalId = LogicalId,
            PopupId   = _notEnoughResourcesPopupId,
            Args      = args,
        });
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private int ResolveCampaignLevel()
    {
        if (_services == null) return 1;

        var ctx = _services.Context.BuildSelectedContext();

        if (ctx != null && ctx.GlobalCampaignIndex >= 0)
            return ctx.GlobalCampaignIndex + 1;

        var campaignMission = _services.Progress.GetMission(MissionType.Campaign);
        if (campaignMission == null) return 1;

        int idx = _services.Catalog.GetGlobalCampaignIndexOrMinusOne(campaignMission);
        return idx >= 0 ? idx + 1 : 1;
    }
}
