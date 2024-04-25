using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class TargetActorSetter : MonoBehaviour
{
    [SerializeField] private ActorController _targetActor;
    [SerializeField] private bool _invokeOnAvake = true;

    private void Start()
    {
        if (_invokeOnAvake)
            Set();
    }

    public void Set()
    {
        var holders = GetComponentsInChildren<IActorIniter>();

        foreach (var holder in holders)
            holder.InitActor(_targetActor);
    }
}
