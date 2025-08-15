using UnityEngine;
using UnityEngine.UI;

public class MissionSelectionController : MonoBehaviour
{
    [SerializeField] private ZoneData _zoneData;
    private MissionSelector _selector = new MissionSelector();
    private MissionSelectorView _selectorView;
    private MissionProgressData _missionProgressData;

    private void Awake()
    {
        InjectChilds();
        _selectorView = GetComponentInChildren<MissionSelectorView>();

        if (DataManager.Instance.TryGetData(out _missionProgressData)) { }
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
        if (_missionProgressData == null)
        {
            Debug.LogError("MissionProgressData not found.");
            return;
        }

        // Update selected mission type
        _missionProgressData.SelectMissionType(type);

        Debug.Log("SELECTED MISSION " + type.ToString());
    }

    private void InjectChilds()
    {
        var childs = GetComponentsInChildren<IMissionSelectorInjectable>(true);

        foreach (var child in childs)
            child.InjectMissionSelector(_selector);
    }
}
