using UnityEngine;
using UnityEngine.UI;

public class BossProgressView : MonoBehaviour
{
    [SerializeField] private ZoneData _data;
    [SerializeField] private Slider _slider;

    private void OnEnable()
    {
        UpdateProgress(_data.BossProgress);
    }

    public void UpdateProgress(float progress)
    {
        if (_slider != null)
            _slider.value = Mathf.Clamp01(progress);
    }
}