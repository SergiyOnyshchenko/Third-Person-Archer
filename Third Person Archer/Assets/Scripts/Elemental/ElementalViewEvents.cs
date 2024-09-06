using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ElementalViewEvents
{
    [SerializeField] private ElementalType _type;
    public ElementalType Type { get => _type; }
    public UnityEvent OnViewSeted = new UnityEvent();
    public UnityEvent OnViewReseted = new UnityEvent();

    public void Apply() => OnViewSeted?.Invoke();
    public void Reset() => OnViewReseted?.Invoke();
}
