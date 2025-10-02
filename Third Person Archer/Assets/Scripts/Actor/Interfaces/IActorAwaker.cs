using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public interface IActorAwaker
    {
        public void AwakeActor(ActorController actor);
    }
}