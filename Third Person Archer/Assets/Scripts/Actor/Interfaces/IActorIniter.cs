using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public interface IActorIniter
    {
        public void InitActor(ActorController actor);
    }
}

