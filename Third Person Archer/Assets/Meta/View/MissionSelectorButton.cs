using UnityEngine;
using UnityEngine.UI;

public class MissionSelectorButton : MonoBehaviour
{
    [SerializeField] private MissionType _missionType;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _highlight;

    private MissionSelector _selector;

    public void Bind(MissionSelector selector)
    {
        _selector = selector;
        _button.onClick.AddListener(OnClick);
        _selector.OnMissionSelected += UpdateView;
        UpdateView(_selector.Selected);
    }

    private void OnClick()
    {
        _selector?.Select(_missionType);
    }

    private void UpdateView(MissionType selected)
    {
        _highlight.SetActive(selected == _missionType);
    }

    private void OnDestroy()
    {
        if (_selector != null)
            _selector.OnMissionSelected -= UpdateView;

        _button.onClick.RemoveListener(OnClick);
    }
}
