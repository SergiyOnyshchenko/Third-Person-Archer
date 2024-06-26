using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Button))]
public class SelectableButton : MonoBehaviour, ISelector, ISelectable
{
    [field: SerializeField] public Button Button { get; private set; }
    public bool IsSelected { get; private set; }
    public event Action OnSelected;
    public event Action OnDeselected;

    private void Awake()
    {
        Button = GetComponent<Button>();
    }

    public void Select()
    {
        OnSelected?.Invoke();
    }

    public void Deselect()
    {
        OnDeselected?.Invoke();
    }
}
