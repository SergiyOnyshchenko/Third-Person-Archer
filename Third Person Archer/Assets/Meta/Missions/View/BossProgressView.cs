using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BossProgressView : MonoBehaviour
{ 
    [SerializeField] private ZoneData _zoneData;
    [SerializeField] private Slider _slider;
    /// <summary>
    /// Presenter provides completed/total campaign count for the current zone.
    /// </summary>
    private void Start()
    {
        SetProgress(_zoneData.BossProgress01);
    }

    public void SetProgress(float p)
    {
        if (_slider != null)
            _slider.value = p;
    }
}