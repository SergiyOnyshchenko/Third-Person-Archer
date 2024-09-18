using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using DG.Tweening;

public class OnHitTransition : StateTransition, IActorIniter
{
    private Health _health;
    
    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out Health health))
            _health = health;
    }

    private void OnEnable()
    {
        _health.OnHealthModified.AddListener(TryHitTranaition);
    }

    private void OnDisable()
    {
        _health.OnHealthModified.RemoveListener(TryHitTranaition);
    }

    private void TryHitTranaition(int health, int maxHealth)
    {
        if (health <= 0)
            return;

        DOVirtual.DelayedCall(0.1f, DoTransition);
    }
}
