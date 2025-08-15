using UnityEngine;
using TMPro;

public class ZoneMapView : MonoBehaviour
{
    [SerializeField] private ZoneData _zoneData;
    [Space]
    [SerializeField] private TextMeshProUGUI _zoneNameText;
    [SerializeField] private GameObject _lockedIndicator;

    private void Start()
    {
        _zoneNameText.text = _zoneData.Name;

        if (DataManager.Instance.TryGetData(out MissionProgressData missionProgress))
            _lockedIndicator.SetActive(!missionProgress.IsZoneUnlocked(_zoneData));
    }
}