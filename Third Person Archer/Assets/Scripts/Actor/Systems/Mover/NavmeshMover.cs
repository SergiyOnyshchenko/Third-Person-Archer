using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Actor
{
    public class NavmeshMover : Mover
    {
        private NavMeshAgent _agent;
        private IEnumerator _movingProcess;

        public override void Move(Transform destination, UnityAction onCompleted = null)
        {
            Move(destination.position, onCompleted);
        }

        public override void Move(Vector3 position, UnityAction onCompleted = null)
        {
            _agent.speed = _speed.Value;
            _agent.acceleration = _acceleration.Value;

            TargetPosition = position;
            _agent.destination = position;

            if (_movingProcess != null)
                StopCoroutine(_movingProcess);

            _movingProcess = MovingProcess(position, onCompleted);

            StartCoroutine(_movingProcess);
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

        private IEnumerator MovingProcess(Vector3 targetPosition, UnityAction onCompleted)
        {
            while(Vector3.Distance(_agent.transform.position, targetPosition) >= 0.25f)
                yield return null;

            onCompleted?.Invoke();
            OnMovingFinished?.Invoke();
        }
    }
}
