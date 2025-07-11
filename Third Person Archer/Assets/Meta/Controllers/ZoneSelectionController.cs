using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoneSelectionController : MonoBehaviour
{
    [SerializeField] private ZoneProgressData _progressData;
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;

    private void Start()
    {
        _leftButton.onClick.AddListener(() => ChangeZone(-1));
        _rightButton.onClick.AddListener(() => ChangeZone(1));
    }

    private void OnDestroy()
    {
        _leftButton.onClick.RemoveAllListeners();
        _rightButton.onClick.RemoveAllListeners();
    }

    private void ChangeZone(int direction)
    {
        int zoneCount = _progressData.AllZones.Count;
        if (zoneCount == 0) return;

        int currentIndex = GetCurrentZoneIndex();
        int nextIndex = (currentIndex + direction + zoneCount) % zoneCount;

        _progressData.SelectZone(nextIndex);
        Debug.Log($"Zone viewed: {_progressData.CurrentZone.ZoneName}");
    }

    private int GetCurrentZoneIndex()
    {
        for (int i = 0; i < _progressData.AllZones.Count; i++)
        {
            if (_progressData.CurrentZone == _progressData.AllZones[i])
                return i;
        }
        return 0;
    }
}
