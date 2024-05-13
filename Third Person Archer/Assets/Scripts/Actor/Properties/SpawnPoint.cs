using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class SpawnPoint : Vector3Property, IActorIniter
    {
        public void InitActor(ActorController actor)
        {
            _value = actor.transform.position;
        }
    }
}
