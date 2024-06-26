using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EquipEventsReciever : MonoBehaviour
{
    private IEquipable _equipable;
    public UnityEvent OnEquipped = new UnityEvent(); 
    public UnityEvent OnUnequipped = new UnityEvent();

    private void Awake()
    {
        _equipable = GetComponent<IEquipable>();
    }

    private void OnEnable()
    {
        _equipable.OnEquipped += SendEquipEvent;
        _equipable.OnUnequipped += SendUnequipEvent;
    }

    private void OnDisable()
    {
        _equipable.OnEquipped -= SendEquipEvent;
        _equipable.OnUnequipped -= SendUnequipEvent;
    }

    private void SendEquipEvent()
    {
        OnEquipped?.Invoke();
    }

    private void SendUnequipEvent()
    {
        OnUnequipped?.Invoke(); 
    }
}
