using UnityEngine;
using UnityEngine.UI;

public class MissionSelectorButton : MonoBehaviour, IMetaGameInjectable, IMissionSelectorInjectable
{
    [SerializeField] private MissionType _missionType;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _exclamationIcon;

    private MissionSelector _selector;
    private MetaGameController _metaGameController;

    public void InjectMetaGameController(MetaGameController controller)
    {
        _metaGameController = controller;
        UpdateExclamation();
    }

    public void InjectMissionSelector(MissionSelector selector)
    {
        _selector = selector;

        _selector.OnMissionSelected += UpdateView;
        UpdateView(_selector.Selected);
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        var progress = _metaGameController?.MissionProgress;
        var zone = progress?.Zone;
        var segment = zone?.GetSegmentByType(_missionType);
        var mission = segment?.GetCurrentMission();

        if (segment == null || mission == null || !MissionUnlockService.CanUnlock(mission, zone))
        {
            string reason = MissionUnlockService.GetLockedReason(_missionType, zone);
            PopupManager.Instance.EnqueuePopup(PopupType.Warning, reason);
            return;
        }

        // If playable, select and mark
        _selector?.Select(_missionType);
        _metaGameController?.UnlockNotifier?.MarkSeen(_missionType);
        UpdateExclamation();
    }

    private void UpdateView(MissionType selected)
    {
        _highlight.SetActive(selected == _missionType);
    }

    private void UpdateExclamation()
    {
        bool show = _metaGameController?.UnlockNotifier?.IsNewlyUnlocked(_missionType) ?? false;
        if (_exclamationIcon != null)
        {
            _exclamationIcon.SetActive(show);
        }
    }

    private void OnDestroy()
    {
        if (_selector != null)
            _selector.OnMissionSelected -= UpdateView;
    }
}