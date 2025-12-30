using UnityEngine;
using UnityEngine.UI;

public sealed class MissionSelectorButtonPresenter : MonoBehaviour
{
    [SerializeField] private MissionType _missionType;
    [SerializeField] private Button _button;

    [Header("Visuals")]
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _lockedIcon;

    [Header("Wiring")]
    [SerializeField] private MissionTypeSelectionPresenter _selectionPresenter;
    private bool _isInitialized;
    private MainMenuServices _services;

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

        // Highlight if selected
        var selected = _services.Progress.SelectedMissionType;
        if (_highlight != null)
            _highlight.SetActive(selected == _missionType);

        // Locked icon: based on availability for THAT mode
        // We temporarily evaluate availability by building a context and pretending the selected type is this button’s type.
        var ctx = BuildContextFor(_missionType);
        var avail = _services.Availability.GetAvailability(ctx);

        if (_lockedIcon != null)
            _lockedIcon.SetActive(!avail.CanPlay);
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