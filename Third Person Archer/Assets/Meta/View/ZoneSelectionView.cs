using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZoneSelectionView : MonoBehaviour
{
    [SerializeField] private GameObject[] _zoneMapObjects;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private TextMeshProUGUI _zoneNameText;
    private MissionProgressData _progress;

    private void Awake()
    {
        if (DataManager.Instance.TryGetData(out _progress))
        {
            UpdateView(_progress.GetLastUnlockedZoneIndex());
            _progress.OnZoneChanged.AddListener(() => UpdateView(_progress.GetCurrentZoneIndex()));
        }
    }

    private void OnDestroy()
    {
        _progress.OnZoneChanged.RemoveAllListeners();
    }

    private void UpdateView(int zoneIndex)
    {
        int currentIndex = zoneIndex;
        for (int i = 0; i < _zoneMapObjects.Length; i++)
            _zoneMapObjects[i].SetActive(i == currentIndex);

        var zone = _progress.Zone;
        _zoneNameText.text = zone.Name;

        bool isLocked = !_progress.IsZoneUnlocked(zone);
        _lockedOverlay.SetActive(isLocked);
    }
}
