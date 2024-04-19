using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Actor
{
    public class NavmeshMover : Mover
    {
        private NavMeshAgent _agent;

        public override void Move(Transform destination)
        {
            Move(destination.position);
        }

        public override void Move(Vector3 position)
        {
            _agent.speed = _speed.Value;
            _agent.acceleration = _acceleration.Value;

            TargetPosition = position;
            _agent.destination = position;
        }

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);
            _agent = actor.GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_agent == null)
                return;

            Velocity = _agent.velocity;
        }
    }
}
