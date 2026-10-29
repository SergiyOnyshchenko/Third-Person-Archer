using Meta.Weapons;
using Meta.Weapons.UI;
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
            _playButtonView.BindClick(PlaySelected);

        // One tap = the next step: start each menu visit on the recommended mode.
        var recommended = RecommendedMissionHintService.GetRecommendedType(_services);
        if (recommended.HasValue)
            _services.Progress.SelectMissionType(recommended.Value);

        Refresh();
    }

    private void Refresh()
    {
        if (_services == null || _playButtonView == null)
            return;

        var ctx = _services.Context.BuildSelectedContext();
        var avail = _services.Availability.GetAvailability(ctx);

        bool canClickPlay =
            avail.CanPlay ||
            avail.Reason == AvailabilityBlockReason.CampaignDamageTooLow ||
            avail.Reason == AvailabilityBlockReason.BossCrossbowDamageTooLow;

        _playButtonView.SetInteractable(canClickPlay);
        _playButtonView.SetPower(GetGateForPowerIndicator(ctx));
        _playButtonView.SetSubtitle(GetSubtitle(ctx));
    }

    private static string GetSubtitle(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid) return "";

        var segment = ctx.Zone.GetSegmentByType(ctx.SelectedType);
        int index = segment != null ? Mathf.Max(0, IndexOf(segment, ctx.Mission)) : 0;

        return ctx.SelectedType switch
        {
            MissionType.Campaign => $"{(ctx.IsHeroic ? "Heroic" : "Level")} {index + 1} · {(ctx.Mission.IsSniper ? "Sniper" : ctx.RequiredWeaponClass.ToString())}",
            MissionType.Boss => "Boss fight!",
            _ => ""
        };
    }

    private static int IndexOf(MissionSegmentData segment, MissionData mission)
    {
        for (int i = 0; i < segment.Missions.Count; i++)
            if (segment.Missions[i] == mission) return i;
        return -1;
    }

    private MissionGateResult GetGateForPowerIndicator(MissionContext ctx)
    {
        if (ctx == null || _services.Gate == null)
            return null;

        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
                return ctx.IsValid ? _services.Gate.CheckCampaignGate(ctx) : null;
            case MissionType.Boss:
                return ctx.IsValid && ctx.Zone.IsBossUnlocked() ? _services.Gate.CheckBossGate(ctx) : null;
            default:
                return null;
        }
    }

    /// <summary>Starts the selected mode's current mission (Play button, mission panel).</summary>
    public void PlaySelected()
    {
        if (_services == null) return;

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
                ShowDamageGatePopup(result.Gate, isCrossbow: false);
                break;

            case AvailabilityBlockReason.BossCrossbowDamageTooLow:
                ShowDamageGatePopup(result.Gate, isCrossbow: true);
                break;

            default:
                Debug.LogWarning("Cannot start mission: " + result.Availability.Reason);
                break;
        }
    }

    // ── Campaign gate (normal weapons) and Boss gate (Crossbow) ──────────────

    private void ShowDamageGatePopup(MissionGateResult gate, bool isCrossbow)
    {
        if (!ServiceLocator.TryResolve<IUINavigator>(out var nav)) return;

        var ctx = _services.Context.BuildSelectedContext();
        int companyLevel = _services.ProgressData.GetCompanyLevel();

        // Ask the recommendation service what action makes most sense right now.
        var rec = _services.NextStep?.Evaluate() ?? NextStepRecommendation.None;
        var (action, preselectId, highlightMode) = RecommendationToAction(rec);

        var args = new DamageGatePopupArgs(
            weaponClass:       gate.RequiredWeaponClass,
            campaignLevel:     companyLevel,
            currentDamage:     gate.CurrentDamage,
            requiredDamage:    gate.RequiredDamage,
            weaponScreenId:    _weaponSelectionScreenId,
            action:            action,
            canUpgradeToPass:  gate.CanUpgradeToPass,
            preselectWeaponId: preselectId,
            highlightMode:     highlightMode);

        nav.ShowPopup(_damageGatePopupId, args);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (GatePopupAction action, string preselectId, WeaponHighlightMode highlight)
        RecommendationToAction(NextStepRecommendation rec)
    {
        return rec.Type switch
        {
            NextStepRecommendationType.UpgradeWeapon =>
                (GatePopupAction.OpenWeapons, rec.TargetWeaponId, WeaponHighlightMode.Upgrade),
            NextStepRecommendationType.BuyWeapon =>
                (GatePopupAction.OpenWeapons, rec.TargetWeaponId, WeaponHighlightMode.Buy),
            NextStepRecommendationType.ReplayLevel =>
                (GatePopupAction.ReplayLevel, null, WeaponHighlightMode.None),
            _ =>
                // None or unrecognised: fall back to weapon screen, no highlight.
                (GatePopupAction.OpenWeapons, null, WeaponHighlightMode.None),
        };
    }
}
