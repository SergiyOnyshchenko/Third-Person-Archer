using UnityEngine;
using UnityEngine.UI;

public class MissionSelectionController : MonoBehaviour
{
    [SerializeField] private ZoneData _zoneData;
    private MissionSelector _selector = new MissionSelector();
    private MissionSelectorView _selectorView;

    private void Awake()
    {
        _selectorView = GetComponentInChildren<MissionSelectorView>();
        _selectorView.Bind(_selector);
    }

    private void OnEnable()
    {
        MissionTypeSelectHandler(_selector.Selected);
        _selector.OnMissionSelected += MissionTypeSelectHandler;
    }

    private void OnDisable()
    {
        _selector.OnMissionSelected -= MissionTypeSelectHandler;
    }

    private void MissionTypeSelectHandler(MissionType type)
    {
        Debug.Log("SELECTED MISSION " + type.ToString());
    }
}
