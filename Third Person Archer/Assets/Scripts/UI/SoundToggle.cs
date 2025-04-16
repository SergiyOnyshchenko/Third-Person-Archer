using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Tools;
using Playgama;
using UnityEngine;
using UnityEngine.UI;

public class SoundToggle : MonoBehaviour
{
    [SerializeField] private Sprite _activeToggleView;
    [SerializeField] private Sprite _unactiveToggleView;
    [Space]
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Image _icon;
    private MMSoundManager _soundManager;
    private const string SoundKey = "sound";

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(UpdateSettings);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(UpdateSettings);
        Save();
    }

    private void Start()
    {
        _soundManager = FindObjectOfType<MMSoundManager>();

        DOVirtual.DelayedCall(0.02f, () =>
        {
            Load();
        });
    }

    private void Load()
    {
        Bridge.storage.Get(SoundKey, (success, value) =>
        {
            if (success && float.TryParse(value, out float result))
            {
                _toggle.isOn = result != 0;
            }
            else
            {
                _toggle.isOn = false;
            }

            UpdateSettings(_toggle.isOn);
        });
    }

    private void Save()
    {
        Bridge.storage.Set(SoundKey, _toggle.isOn ? "1" : "0", null);
    }

    private void UpdateSettings(bool value)
    {
        if (value)
        {
            _soundManager.UnmuteMaster();
            _soundManager.SetVolumeMaster(1f);
            _icon.sprite = _activeToggleView;
        }
        else
        {
            _soundManager.MuteMaster();
            _soundManager.SetVolumeMaster(0f);
            _icon.sprite = _unactiveToggleView;
        }

        Save();
    }
}
