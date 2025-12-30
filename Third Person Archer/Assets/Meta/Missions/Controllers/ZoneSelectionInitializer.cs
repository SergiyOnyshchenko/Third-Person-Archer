using UnityEngine;

/// <summary>
/// New rule: player cannot browse zones from menu.
/// We force-select the last unlocked zone on menu load.
/// </summary>
public sealed class ZoneSelectionInitializer : MonoBehaviour
{
    [SerializeField] private MissionProgressData _progressData;

    private void Awake()
    {
        if (_progressData == null)
        {
            // Fallback if you prefer not to assign in inspector
            DataManager.Instance.TryGetData(out _progressData);
        }

        if (_progressData == null)
        {
            Debug.LogError("ZoneSelectionInitializer: MissionProgressData not found.");
            enabled = false;
            return;
        }

        int lastUnlocked = _progressData.GetLastUnlockedZoneIndex();
        _progressData.SelectZone(lastUnlocked);
    }
}