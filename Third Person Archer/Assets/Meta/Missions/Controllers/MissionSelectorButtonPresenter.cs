using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class MissionSelectorButtonPresenter : MonoBehaviour
{
    [SerializeField] private MissionType _missionType;
    [SerializeField] private Button _button;

    [Header("Visuals")]
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _lockedIcon;
    [SerializeField] private GameObject _recommendedHint;

    [Header("Wiring")]
    [SerializeField] private MissionTypeSelectionPresenter _selectionPresenter;
    private bool _isInitialized;
    private MainMenuServices _services;
    private Tween _hintTween;

    public void Init(MainMenuServices services)
    {
        if (_isInitialized) return;

        _isInitialized = true;
        _services = services;

        if (_services != null)
            _services.OnMenuStateChanged += Refresh;

        Refresh();
    }

    private void OnEnable()
    {
        if (_button != null)
            _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }

    private void OnDestroy()
    {
        if (_services != null)
            _services.OnMenuStateChanged -= Refresh;

        _hintTween?.Kill();
    }

    private void OnClick()
    {
        // We always allow selecting the mode (even if it’s locked).
        // The Play button + info panel will explain why it’s blocked.
        if (_selectionPresenter != null)
            _selectionPresenter.SelectMissionType(_missionType);
    }

    private void Refresh()
    {
        if (_services == null) return;

        var zone = _services.Progress.CurrentZone;

        // Hide Campaign selector when campaign is done and boss is unlocked
        if (_missionType == MissionType.Campaign &&
            zone != null &&
            zone.IsCampaignComplete() &&
            zone.IsBossUnlocked())
        {
            SetRecommendedHint(false);
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        var selected = _services.Progress.SelectedMissionType;
        if (_highlight != null)
            _highlight.SetActive(selected == _missionType);

        var ctx = BuildContextFor(_missionType);
        var avail = _services.Availability.GetAvailability(ctx);

        bool showLocked =
            !avail.CanPlay &&
            avail.Reason != AvailabilityBlockReason.CampaignDamageTooLow;

        if (_lockedIcon != null)
            _lockedIcon.SetActive(showLocked);

        var recommended = RecommendedMissionHintService.GetRecommendedType(_services);
        SetRecommendedHint(recommended == _missionType);
    }

    private void SetRecommendedHint(bool show)
    {
        if (_recommendedHint == null) return;

        if (!show)
        {
            _hintTween?.Kill();
            _hintTween = null;
            _recommendedHint.transform.localScale = Vector3.one;
            _recommendedHint.SetActive(false);
            return;
        }

        _recommendedHint.SetActive(true);

        if (_hintTween != null && _hintTween.IsActive())
            return;

        _hintTween = _recommendedHint.transform
            .DOScale(1.2f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(_recommendedHint);
    }

    private MissionContext BuildContextFor(MissionType type)
    {
        // Build normal context then swap selected type + mission reference.
        // This avoids mutating progress data.
        var baseCtx = _services.Context.BuildSelectedContext();

        var mission = _services.Progress.GetMission(type);
        int globalIndex = baseCtx.GlobalCampaignIndex; // may be -1 for non-campaign; ok
        int companyLevel = baseCtx.CompanyLevel;

        // Re-create a context with selected type & mission but keep zone/loop/weapon class (weapon class is per mission anyway)
        return new MissionContext(
            baseCtx.Zone,
            baseCtx.ZoneIndex,
            type,
            mission,
            globalIndex,
            companyLevel,
            baseCtx.LoopIndex,
            baseCtx.BalanceLoopIndex,
            baseCtx.RequiredWeaponClass
        );
    }
}