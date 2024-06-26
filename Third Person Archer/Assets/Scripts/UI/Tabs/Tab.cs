using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using EventSystem = UnityEngine.EventSystems.EventSystem;

[System.Serializable]
public class Tab
{
    [SerializeField] private SelectableButton _button;
    [SerializeField] private GameObject _content;
    public Button Button { get => _button.Button; }
    public GameObject Content { get => _content; }
    public UnityEvent OnOpened = new UnityEvent();
    public event Action<Tab> OnSelected;

    public void Open()
    {
        _button.Select();
        _content.SetActive(true);

        OnOpened?.Invoke();
    }

    public void Close() 
    {
        _button.Deselect();
        _content.SetActive(false);
    }

    public void Subscribe()
    {
        Button.onClick.AddListener(SendSelectEvent);
    }

    public void Unsubscribe()
    {
        Button.onClick.RemoveListener(SendSelectEvent);
    }

    private void SendSelectEvent()
    {
        OnSelected?.Invoke(this);
    }
}
