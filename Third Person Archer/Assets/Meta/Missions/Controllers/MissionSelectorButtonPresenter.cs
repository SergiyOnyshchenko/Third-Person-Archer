using DG.Tweening;
using TMPro;
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
    [Tooltip("Optional progress badge: Sniper \"2/5\", Guild \"Rank 3 · x2 ×5\".")]
    [SerializeField] private GameObject _badgeRoot;
    [SerializeField] private TMP_Text _badgeText;

    [Header("Wiring")]
    [SerializeField] private MissionTypeSelectionPresenter _selectionPresenter;
    [SerializeField] private MissionPanelView _panel;
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
        transform.DOKill(complete: true);
        transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8, 0.6f).SetLink(gameObject);

        // Guild contracts live on their own board; locked state is explained there too.
        if (_missionType == MissionType.Contracts && IsUnlocked())
        {
            GuildBoardNavigation.Open();
            return;
        }

        // We always allow selecting the mode (even if it’s locked): the panel explains why it’s blocked.
        if (_selectionPresenter != null && _missionType != MissionType.Contracts)
            _selectionPresenter.SelectMissionType(_missionType);

        if (_panel != null && _missionType != MissionType.Contracts)
            _panel.ShowMode(_missionType);
    }

    private bool IsUnlocked()
    {
        if (_services == null) return false;
        var avail = _services.Availability.GetAvailability(BuildContextFor(_missionType));
        return avail.CanPlay || avail.Reason != AvailabilityBlockReason.ContractsLockedByCompanyLevel;
    }

    private void Refresh()
    {
        if (_services == null) return;

        var zone = _services.Progress.CurrentZone;

        // Campaign levels are drawn as a path of nodes (CampaignPathView); the old landmark is gone.
        if (_missionType == MissionType.Campaign)
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

        RefreshBadge(zone, showLocked);
    }

    private void RefreshBadge(ZoneData zone, bool locked)
    {
        if (_badgeRoot == null) return;

        string text = null;
        if (!locked && zone != null)
        {
            switch (_missionType)
            {
                case MissionType.Sniper:
                    var sniper = zone.GetSegmentByType(MissionType.Sniper);
                    if (sniper != null && sniper.Missions.Count > 0)
                        text = $"{sniper.GetCompletedOnceCount()}/{sniper.Missions.Count}";
                    break;

                case MissionType.Contracts:
                    var board = _services.ContractBoard;
                    if (board != null)
                        text = board.DailyBonusLeft > 0
                            ? $"Rank {board.Rank} · x2 left: {board.DailyBonusLeft}"
                            : $"Rank {board.Rank}";
                    break;
            }
        }

        _badgeRoot.SetActive(text != null);
        if (_badgeText != null && text != null) _badgeText.text = text;
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

        // Re-create a context with selected type & mission but keep zone/weapon class (weapon class is per mission anyway)
        return new MissionContext(
            baseCtx.Zone,
            baseCtx.ZoneIndex,
            type,
            mission,
            globalIndex,
            companyLevel,
            baseCtx.RequiredWeaponClass
        );
    }
}