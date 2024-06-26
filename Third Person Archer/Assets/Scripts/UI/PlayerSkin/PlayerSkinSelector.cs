using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkinSelector : MonoBehaviour, IPlayerSkinSelector, ISelector, ISelectable
{
    [field: SerializeField] public PlayerSkinData SkinData { get; private set; }
    private Button _button;
    public ISelector Selector => this;
    public bool IsSelected { get; private set; }

    public event Action<IPlayerSkinSelector> OnSkinSelected;
    public event Action OnSelected;
    public event Action OnDeselected;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(SendSkinSelectedEvent);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(SendSkinSelectedEvent);
    }

    public void Select()
    {
        IsSelected = true;
        OnSelected?.Invoke();
    }

    public void Deselect()
    {
        IsSelected = false;
        OnDeselected?.Invoke();
    }

    private void SendSkinSelectedEvent() => OnSkinSelected?.Invoke(this);
}
