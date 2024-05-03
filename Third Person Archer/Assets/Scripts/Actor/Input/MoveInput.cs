using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class MoveInput : Input, IMoveInput
    {
        public bool IsMoving { get; private set; }
        public Vector3 MovePostion { get; private set; }

        private void Start()
        {
            IsActive = true;
        }

        public void MoveToDestination(Transform destination, UnityAction onComplete = null)
        {
            MoveToDestination(destination.position, onComplete);
        }

        public void MoveToDestination(Vector3 destination, UnityAction onComplete = null)
        {
            MovePostion = destination;
            IsMoving = true;
        }

        public void Stop()
        {
            IsMoving = false;
        }
    }
}