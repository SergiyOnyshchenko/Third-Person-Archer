using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class WeaponUnlockView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private Image _icon;
    [SerializeField] private Slider _slider;
    public UnityEvent OnInited = new UnityEvent();
    public UnityEvent OnUnlocked = new UnityEvent();

    public void Init(WeaponData data, float ratio)
    {
        _title.text = data.Name;
        _icon.sprite = data.SkinData.Icon;
        _slider.value = ratio;

        OnInited?.Invoke();
    }

    public void Play(float ratio)
    {
        _slider.DOValue(ratio, 0.5f).SetDelay(1).OnComplete(() =>
        {
            if (ratio >= 1f)
            {
                OnUnlocked?.Invoke();
            }
        });
    }
}
