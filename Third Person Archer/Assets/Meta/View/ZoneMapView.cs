using UnityEngine;
using TMPro;

public class ZoneMapView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _zoneNameText;
    [SerializeField] private GameObject _lockedIndicator;

    public void Bind(ZoneData zone)
    {
        if (zone == null) return;

        _zoneNameText.text = zone.ZoneName;
        _lockedIndicator.SetActive(!zone.IsZoneUnlocked);
    }
}

