using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public abstract class VolumeSlider : MonoBehaviour
{
    [SerializeField] protected Slider _slider;

    protected MMSoundManager _soundManager;

    protected abstract MMSoundManager.MMSoundManagerTracks Track { get; }
    protected abstract string PlayerPrefsKey { get; }

    protected virtual void Awake()
    {
        _soundManager = FindObjectOfType<MMSoundManager>();
    }

    protected virtual void OnEnable()
    {
        _slider.onValueChanged.AddListener(OnValueChanged);
        Load();
    }

    protected virtual void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    protected virtual void Load()
    {
        float value = PlayerPrefs.GetFloat(PlayerPrefsKey, 1f);
        _slider.SetValueWithoutNotify(value);
        _soundManager.SetTrackVolume(Track, value);
    }

    protected virtual void OnValueChanged(float value)
    {
        _soundManager.SetTrackVolume(Track, value);
        PlayerPrefs.SetFloat(PlayerPrefsKey, value);
    }
}