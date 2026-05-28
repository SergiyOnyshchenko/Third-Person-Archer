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

        var args = new DamageGatePopupArgs(
            weaponClass: gate.RequiredWeaponClass,
            campaignLevel: companyLevel,
            currentDamage: gate.CurrentDamage,
            requiredDamage: gate.RequiredDamage,
            weaponScreenId: _weaponSelectionScreenId,
            canUpgradeToPass: gate.CanUpgradeToPass
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
            ? "Upgrade your Crossbow to unlock this Sniper mission."
            : "Your Crossbow is fully upgraded. Buy a stronger Crossbow to continue.";

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
            ? "Play Sniper missions to earn Crossbow tokens, then upgrade your Crossbow to unlock this Boss mission."
            : "Your current Crossbow cannot reach the required power. Buy a stronger Crossbow to unlock this Boss mission.";

        var args = new DamageGatePopupArgs(
            weaponClass: WeaponClass.Crossbow,
            campaignLevel: companyLevel,
            currentDamage: gate.CurrentDamage,
            requiredDamage: gate.RequiredDamage,
            weaponScreenId: _weaponSelectionScreenId,
            canUpgradeToPass: gate.CanUpgradeToPass,
            customTitle: "Boss mission requires a stronger Crossbow",
            customHint: hint
        );

        nav.ShowPopup(_damageGatePopupId, args);
    }
}
