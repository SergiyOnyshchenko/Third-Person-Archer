using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class SpawnPoint : Vector3Property, IActorIniter
    {
        public override void InitActor(ActorController actor)
        {
            SetValue(actor.transform.position);
        }
    }
}
