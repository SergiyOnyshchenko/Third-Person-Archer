using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class OnCollisionEnterTransition : StateTransition, IActorIniter
{
    private CollisionTriggerHandler _collisionTriggerHandler;
    private ProjectileHitLayermask _layerMask;

    public void InitActor(ActorController actor)
    {
        _collisionTriggerHandler = actor.GetComponent<CollisionTriggerHandler>(); 
        
        if(actor.TryGetProperty(out _layerMask)) { }
    }

    public override void Enter()
    {
        base.Enter();
        _collisionTriggerHandler.CollisionEnter.AddListener(CollisionHandler);
    }

    public override void Exit()
    {
        _collisionTriggerHandler.CollisionEnter.RemoveListener(CollisionHandler);
        base.Exit();
    }

    private void CollisionHandler(Collision collision)
    {
        if ((_layerMask.Value.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            DoTransition();
        }
    }
}
