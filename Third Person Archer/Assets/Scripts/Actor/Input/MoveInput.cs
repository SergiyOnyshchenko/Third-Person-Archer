using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class MoveInput : Input, IMoveInput, IActorIniter
    {
        private Mover _mover;
        [field: SerializeField] public bool IsMoving { get; private set; }
        public Vector3 MovePostion { get; private set; }

        private void Start()
        {
            IsActive = true;
        }

        public void InitActor(ActorController actor)
        {
            if(actor.TryGetSystem(out Mover mover))
                _mover = mover;
        }

        public void MoveToDestination(Transform destination)
        {
            MoveToDestination(destination.position);
        }

        public void MoveToDestination(Vector3 destination)
        {
            MovePostion = destination;
            IsMoving = true;

            _mover.OnMovingFinished.AddListener(Stop);
        }

        private void Stop()
        {
            _mover.OnMovingFinished.RemoveListener(Stop);
            IsMoving = false;
        }
    }
}