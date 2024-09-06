using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using EventSystem = Actor.EventSystem;

public abstract class Shooter : MonoBehaviour, IActorIniter
{
    [SerializeField] protected Transform _shootPoint;
    private EventSystem _eventSystem;
    protected ElementalAttackType _elementalAttackType;

    public abstract void Shoot(Vector3 direction, float multiplier, UnityAction onHited);

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out EventSystem eventSystem))
            _eventSystem = eventSystem;

        if(actor.TryGetProperty(out ElementalAttackType elementalAttackType))
            _elementalAttackType = elementalAttackType;
    }

    protected void SetTargetHitedEvent()
    {
        if (_eventSystem == null)
            return;

        _eventSystem.TryInvokeEvent("TargetHited");
    }
}
