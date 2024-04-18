using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class TargetActorSetter : MonoBehaviour
{
    [SerializeField] private ActorController _targetActor;
    [SerializeField] private bool _invokeOnAvake = true;

    private void Awake()
    {
        if (_invokeOnAvake)
            Set();
    }

    public void Set()
    {
        var holders = GetComponentsInChildren<ITargetActor>();

        foreach (var holder in holders)
            holder.TargetActor = _targetActor;
    }
}
