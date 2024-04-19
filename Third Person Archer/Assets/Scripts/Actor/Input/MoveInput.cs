using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public void MoveToDestination(Transform destination)
        {
            Debug.Log($"Actor {gameObject.name} moving to {destination.name}");
            MovePostion = destination.position;
            IsMoving = true;
        }

        public void Stop()
        {
            IsMoving = false;
        }
    }
}