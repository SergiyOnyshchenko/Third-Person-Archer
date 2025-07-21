using UnityEngine;
using UnityEngine.UI;

public class BossProgressView : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    public void Bind(ZoneData zone)
    {
        if (zone == null || _slider == null) return;
        _slider.value = zone.BossUnlockProgress;
    }

    public void UpdateProgress(float progress)
    {
        if (_slider != null)
            _slider.value = Mathf.Clamp01(progress);
    }
}