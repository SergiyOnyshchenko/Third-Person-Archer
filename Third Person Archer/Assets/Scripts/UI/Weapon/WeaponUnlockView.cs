using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUnlockView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private Image _icon;

    public void Init(WeaponData data)
    {
        _title.text = data.Name;
        _icon.sprite = data.SkinData.Icon;
    }
}
