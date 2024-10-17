using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SoundToggle : MonoBehaviour
{
    [SerializeField] private Sprite _activeToggleView;
    [SerializeField] private Sprite _unactiveToggleView;
    [Space]
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Image _icon;
    private MMSoundManager _soundManager;

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(UpdateSettings);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(UpdateSettings);
    }

    private void Start()
    {
        _soundManager = FindObjectOfType<MMSoundManager>();

        DOVirtual.DelayedCall(0.02f, () =>
        {
            Load();
            UpdateSettings(_toggle.isOn);
        });
    }

    private void Load()
    {
        if (PlayerPrefs.GetFloat("sound", 0) == 0)
        {
            _toggle.isOn = false;
        }
        else
        {
            _toggle.isOn = true;
        }
    }

    private void Save()
    {
        if (_toggle.isOn)
        {
            PlayerPrefs.SetFloat("sound", 1);
        }
        else
        {
            PlayerPrefs.SetFloat("sound", 0);
        }
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
