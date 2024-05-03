using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public abstract class Mover : System, IMover, IActorIniter
    {
        protected Speed _speed;
        protected Acceleration _acceleration;
        public Vector3 Velocity { get; protected set;  }
        public Vector3 TargetPosition { get; protected set; }
        public UnityEvent OnMovingFinished = new UnityEvent();

        public abstract void Move(Transform destination, UnityAction onCompleted = null);
        public abstract void Move(Vector3 position, UnityAction onCompleted = null);

        public virtual void InitActor(ActorController actor)
        {
            if(actor.TryGetProperty(out Speed speed))
                _speed = speed;

            if (actor.TryGetProperty(out Acceleration acceleration))
                _acceleration = acceleration;
        }
    }
}