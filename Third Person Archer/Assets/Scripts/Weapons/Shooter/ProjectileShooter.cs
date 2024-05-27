using System.Collections;
using System.Collections.Generic;
using Actor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using EventSystem = Actor.EventSystem;

public class ProjectileShooter : Shooter, IActorIniter
{
    [SerializeField] private Projectile _prefab;
    private EventSystem _eventSystem;
    public UnityEvent OnShooted = new UnityEvent();

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out EventSystem eventSystem))
            _eventSystem = eventSystem;
    }

    public override void Shoot(Vector3 direction, float multiplier, UnityAction onHited)
    {
        onHited += SetTargetHitedEvent;

        Projectile arrow = Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);
        arrow.Shoot(direction, multiplier, onHited);

        OnShooted?.Invoke();
    }

    private void SetTargetHitedEvent()
    {
        if (_eventSystem == null)
            return;

        _eventSystem.TryInvokeEvent("TargetHited");
    }
}
