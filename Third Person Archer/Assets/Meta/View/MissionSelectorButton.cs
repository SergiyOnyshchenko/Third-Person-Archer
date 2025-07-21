using UnityEngine;
using UnityEngine.UI;

public class MissionSelectorButton : MonoBehaviour
{
    [SerializeField] private MissionType _missionType;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _exclamationIcon;

    private MissionSelector _selector;
    private MetaGameController _metaGameController;

    public void Bind(MissionSelector selector, MetaGameController controller)
    {
        _selector = selector;
        _metaGameController = controller;

        _button.onClick.AddListener(OnClick);
        _selector.OnMissionSelected += UpdateView;

        UpdateView(_selector.Selected);
        UpdateExclamation();
    }

    private void OnClick()
    {
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

        _button.onClick.RemoveListener(OnClick);
    }
} 