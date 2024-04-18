using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public abstract class Mover : System, IMover, IActorIniter
    {
        protected Speed _speed;
        protected Acceleration _acceleration;

        public abstract void Move(Transform destination);
        public abstract void Move(Vector3 position);

        public virtual void InitActor(ActorController actor)
        {
            if(actor.TryGetProperty(out Speed speed))
                _speed = speed;

            if (actor.TryGetProperty(out Acceleration acceleration))
                _acceleration = acceleration;
        }
    }
}