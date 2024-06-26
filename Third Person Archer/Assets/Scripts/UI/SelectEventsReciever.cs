using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectEventsReciever : MonoBehaviour
{
    private ISelectable _selectable;
    public UnityEvent OnSelected = new UnityEvent();
    public UnityEvent OnDeselected = new UnityEvent();

    private void Awake()
    {
        _selectable = GetComponent<ISelectable>();
    }

    private void OnEnable()
    {
        _selectable.OnSelected += SendSelectEvent;
        _selectable.OnDeselected += SendDeselectEvent;
    }

    private void OnDisable()
    {
        _selectable.OnSelected -= SendSelectEvent;
        _selectable.OnDeselected -= SendDeselectEvent;
    }

    private void SendSelectEvent()
    {
        OnSelected?.Invoke();
    }

    private void SendDeselectEvent()
    {
        OnDeselected?.Invoke();
    }
}
