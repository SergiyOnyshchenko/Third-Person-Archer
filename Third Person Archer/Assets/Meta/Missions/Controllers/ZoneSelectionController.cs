using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ZoneSelectionController : MonoBehaviour
{
    [SerializeField] private MissionProgressData _progressData;
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;

    private void Start()
    {
        _leftButton.onClick.AddListener(() => ChangeZone(-1));
        _rightButton.onClick.AddListener(() => ChangeZone(1));

        int lastUnlockedIndex = _progressData.GetLastUnlockedZoneIndex();
        _progressData.SelectZone(lastUnlockedIndex);
    }

    private void OnDestroy()
    {
        _leftButton.onClick.RemoveAllListeners();
        _rightButton.onClick.RemoveAllListeners();
    }

    private void ChangeZone(int direction)
    {
        int zoneCount = _progressData.AllZones.Count;

        if (zoneCount == 0)
            return;

        int currentIndex = _progressData.GetCurrentZoneIndex();
        int nextIndex = (currentIndex + direction + zoneCount) % zoneCount;

        _progressData.SelectZone(nextIndex);
    }
}
