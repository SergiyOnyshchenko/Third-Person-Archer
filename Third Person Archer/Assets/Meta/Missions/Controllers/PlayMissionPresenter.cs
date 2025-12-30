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
        _playButtonView.SetInteractable(avail.CanPlay);
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
        // Gate popup ONLY for campaign damage too low
        if (result.Availability != null &&
            result.Availability.Reason == AvailabilityBlockReason.CampaignDamageTooLow &&
            result.Gate != null)
        {
            ShowDamageGatePopup(result.Gate);
            return;
        }

        // Your existing messaging (toast etc.) can stay here if you want.
        // For now we just log.
        Debug.LogWarning("Cannot start mission: " + (result.Availability?.Reason.ToString() ?? result.FailReason.ToString()));
    }

    private void ShowDamageGatePopup(MissionGateResult gate)
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
            return;

        // Company level = global campaign index + 1
        var ctx = _services.Context.BuildSelectedContext();
        int companyLevel = (ctx != null && ctx.IsValid) ? (ctx.GlobalCampaignIndex + 1) : 1;

        var args = new DamageGatePopupArgs(
            weaponClass: gate.RequiredWeaponClass,
            campaignLevel: companyLevel,
            currentDamage: gate.CurrentDamage,
            requiredDamage: gate.RequiredDamage,
            weaponScreenId: _weaponSelectionScreenId
        );

        nav.ShowPopup(_damageGatePopupId, args); // Popup layer 
    }
}