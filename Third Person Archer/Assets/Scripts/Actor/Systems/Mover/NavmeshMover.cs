using System.Collections;
using System.Collections.Generic;
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
            _agent.destination = position;
        }

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);
            _agent = actor.GetComponent<NavMeshAgent>();
        }
    }
}
