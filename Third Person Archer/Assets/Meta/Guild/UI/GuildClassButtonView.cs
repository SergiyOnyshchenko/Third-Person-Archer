using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Weapon class choice on the Guild Board (any unlocked class can take a contract).</summary>
public sealed class GuildClassButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private GameObject _selected;
    [SerializeField] private GameObject _orderMark;

    public event Action Clicked;

    private void Awake()
    {
        if (_button != null)
            _button.onClick.AddListener(() => Clicked?.Invoke());
    }

    public void Bind(string label, bool playable, bool selected, bool isOrder)
    {
        if (_label != null) _label.text = label;
        if (_button != null) _button.interactable = playable;
        if (_selected != null) _selected.SetActive(selected && playable);
        if (_orderMark != null) _orderMark.SetActive(isOrder);
    }
}
