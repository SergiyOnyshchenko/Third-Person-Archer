using Meta.Weapons;
using UI.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PlayMissionPresenter : MonoBehaviour
{
    [SerializeField] private PlayButtonView _playButtonView;

    [Header("UI Ids")]
    [Tooltip("Popup id from ScreenRegistry.")]
    [SerializeField] private string _damageGatePopupId = "DamageGatePopup";

    [Tooltip("Weapon selection screen id from ScreenRegistry.")]
    [SerializeField] private string _weaponSelectionScreenId = "WeaponSelectionScreen";

    // Save keys for one-time tutorial flags
    private const string FirstCampaignGateKey = "tutorial_first_campaign_gate";
    private const string FirstBossGateKey = "tutorial_first_boss_gate";
    private const string FirstForcedPurchaseKey = "tutorial_first_forced_purchase_gate";

    // Loaded once in Init(); written when the tutorial is first shown
    private bool _firstCampaignGateShown;
    private bool _firstBossGateShown;
    private bool _firstForcedPurchaseShown;

    private MainMenuServices _services;
    private bool _initialized;

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
        if (_services != null)
            _services.OnMenuStateChanged -= Refresh;
    }

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

    public void Init(MainMenuServices services)
    {
        if (_initialized) return;
        _initialized = true;

        _services = services;

        _firstCampaignGateShown = SaveSystem.Load(FirstCampaignGateKey, false);
        _firstBossGateShown = SaveSystem.Load(FirstBossGateKey, false);
        _firstForcedPurchaseShown = SaveSystem.Load(FirstForcedPurchaseKey, false);

        if (_services != null)
            _services.OnMenuStateChanged += Refresh;

        if (_playButtonView != null)
            _playButtonView.BindClick(OnPlayClicked);

        Refresh();
    }

    private void Refresh()
    {
        if (_services == null || _playButtonView == null)
            return;

        var ctx = _services.Context.BuildSelectedContext();
        var avail = _services.Availability.GetAvailability(ctx);

        // Allow Play button click when blocked by a gate that shows a popup.
        // The popup explains what upgrade is needed.
        bool canClickPlay =
            avail.CanPlay ||
            avail.Reason == AvailabilityBlockReason.CampaignDamageTooLow ||
            avail.Reason == AvailabilityBlockReason.SniperDamageTooLow ||
            avail.Reason == AvailabilityBlockReason.BossCrossbowDamageTooLow;

        _playButtonView.SetInteractable(canClickPlay);
    }

    private void OnPlayClicked()
    {
        if (_services == null)
            return;

        var result = _services.MissionStart.TryStartSelected();

        if (!result.Started)
        {
            HandleStartFailure(result);
            return;
        }

        var path = result.MissionToLoad.Scene.ScenePath;
        ScenesLoader.Instance.LoadScene(path);
    }

    private void HandleStartFailure(MissionStartResult result)
    {
        if (result.Availability == null || result.Gate == null)
        {
            Debug.LogWarning("Cannot start mission: " + (result.Availability?.Reason.ToString() ?? result.FailReason.ToString()));
            return;
        }

        switch (result.Availability.Reason)
        {
            case AvailabilityBlockReason.CampaignDamageTooLow:
                ShowDamageGatePopup(result.Gate);
                break;

            case AvailabilityBlockReason.SniperDamageTooLow:
                ShowSniperGatePopup(result.Gate);
                break;

            case AvailabilityBlockReason.BossCrossbowDamageTooLow:
                ShowBossGatePopup(result.Gate);
                break;

            default:
                Debug.LogWarning("Cannot start mission: " + result.Availability.Reason);
                break;
        }
    }

    private void ShowDamageGatePopup(MissionGateResult gate)
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        var ctx = _services.Context.BuildSelectedContext();
        int companyLevel = (ctx != null && ctx.IsValid) ? (ctx.GlobalCampaignIndex + 1) : 1;

        string className = gate.RequiredWeaponClass.ToString(); // "Bow", "Shuriken", etc.
        string hint = gate.CanUpgradeToPass
            ? $"Upgrade your {className} or buy a stronger one. {className} Tokens can be earned from Campaign missions and Contracts."
            : $"Your {className} is at its maximum level. Buy a stronger {className}. {className} Tokens can be earned from Campaign missions and Contracts.";

        string tutorialText = null;
        if (!gate.CanUpgradeToPass && !_firstForcedPurchaseShown)
        {
            // Forced purchase tutorial takes priority over the general gate tutorial.
            _firstForcedPurchaseShown = true;
            SaveSystem.Save(FirstForcedPurchaseKey, true);
            // Also mark general gate tutorial shown so it does not appear later on a normal gate.
            if (!_firstCampaignGateShown)
            {
                _firstCampaignGateShown = true;
                SaveSystem.Save(FirstCampaignGateKey, true);
            }
            tutorialText = "TIP: Your weapon is at its maximum level for this class.\n" +
                           "To pass this gate you need to BUY a higher-tier weapon of the same class.\n" +
                           "Open the weapon shop and look for a stronger option.";
        }
        else if (!_firstCampaignGateShown)
        {
            _firstCampaignGateShown = true;
            SaveSystem.Save(FirstCampaignGateKey, true);
            tutorialText = "TIP: Gates appear when your weapon damage is too low.\n" +
                           "Upgrade your weapon or buy a stronger one to continue.\n" +
                           "Weapon Tokens can be earned from Campaign missions and Contracts.";
        }

        var args = new DamageGatePopupArgs(
            weaponClass: gate.RequiredWeaponClass,
            campaignLevel: companyLevel,
            currentDamage: gate.CurrentDamage,
            requiredDamage: gate.RequiredDamage,
            weaponScreenId: _weaponSelectionScreenId,
            canUpgradeToPass: gate.CanUpgradeToPass,
            customHint: hint,
            tutorialText: tutorialText
        );

        nav.ShowPopup(_damageGatePopupId, args);
    }

    private void ShowSniperGatePopup(MissionGateResult gate)
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        var ctx = _services.Context.BuildSelectedContext();
        int companyLevel = (ctx != null && ctx.IsValid) ? (ctx.GlobalCampaignIndex + 1) : 1;

        string hint = gate.CanUpgradeToPass
            ? "Upgrade your Crossbow to access this Sniper mission. Crossbow Tokens can be earned from Sniper missions."
            : "Your Crossbow is at its maximum level. Buy a stronger Crossbow. Crossbow Tokens can be earned from Sniper missions.";

        var args = new DamageGatePopupArgs(
            weaponClass: WeaponClass.Crossbow,
            campaignLevel: companyLevel,
            currentDamage: gate.CurrentDamage,
            requiredDamage: gate.RequiredDamage,
            weaponScreenId: _weaponSelectionScreenId,
            canUpgradeToPass: gate.CanUpgradeToPass,
            customTitle: "Sniper access requires a stronger Crossbow",
            customHint: hint
        );

        nav.ShowPopup(_damageGatePopupId, args);
    }

    private void ShowBossGatePopup(MissionGateResult gate)
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        var ctx = _services.Context.BuildSelectedContext();
        int companyLevel = (ctx != null && ctx.IsValid) ? (ctx.GlobalCampaignIndex + 1) : 1;

        string hint = gate.CanUpgradeToPass
            ? "Boss missions require Crossbow Power. Play Sniper missions to earn Crossbow Tokens, upgrade your Crossbow, then return to the Boss."
            : "Your Crossbow cannot reach the required power. Buy a stronger Crossbow. Crossbow Tokens can be earned from Sniper missions.";

        string tutorialText = null;
        if (!_firstBossGateShown)
        {
            _firstBossGateShown = true;
            SaveSystem.Save(FirstBossGateKey, true);
            tutorialText = "TIP: Boss missions require Crossbow Power — not just any weapon.\n" +
                           "Play Sniper missions to earn Crossbow Tokens.\n" +
                           "Upgrade your Crossbow, then return to the Boss.";
        }

        var args = new DamageGatePopupArgs(
            weaponClass: WeaponClass.Crossbow,
            campaignLevel: companyLevel,
            currentDamage: gate.CurrentDamage,
            requiredDamage: gate.RequiredDamage,
            weaponScreenId: _weaponSelectionScreenId,
            canUpgradeToPass: gate.CanUpgradeToPass,
            customTitle: "Boss mission requires a stronger Crossbow",
            customHint: hint,
            tutorialText: tutorialText
        );

        nav.ShowPopup(_damageGatePopupId, args);
    }
}
