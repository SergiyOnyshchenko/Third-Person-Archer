using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using EventSystem = Actor.EventSystem;

public abstract class Shooter : MonoBehaviour, IActorIniter
{
    [SerializeField] protected Transform _shootPoint;
    private EventSystem _eventSystem;
    protected ElementalAttackType _elementalAttackType;
    protected AimInput _aimInput;
    protected ShootError _shootError;
    protected Damage _damage;
    public Transform ShootPoint => _shootPoint;

    public abstract void Shoot(Vector3 direction, float multiplier,
        UnityAction<ActorController> onTargetHited, UnityAction onAnyHit);

    public virtual void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out _aimInput)) { }
        if (actor.TryGetSystem(out _eventSystem)) { }
        if (actor.TryGetProperty(out _elementalAttackType)) { }
        if (actor.TryGetProperty(out _shootError)) { }
        if (actor.TryGetProperty(out _damage)) { }
    }

    protected void SetTargetHitedEvent()
    {
        if (_eventSystem == null)
            return;

        _eventSystem.TryInvokeEvent("TargetHited");
    }
}