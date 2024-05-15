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
        private const float _breakDistanceThreshold = 0.25f;
        private NavMeshAgent _agent;
        private IEnumerator _moveProcess;
        private UnityAction OnMoveComplete;

        public override void Move(Transform destination, UnityAction onCompleted = null)
        {
            Move(destination.position, onCompleted);
        }

        public override void Move(Vector3 position, UnityAction onCompleted = null)
        {
            _agent.isStopped = false;

            _agent.speed = _speed.Value;
            _agent.acceleration = _acceleration.Value;

            TargetPosition = position;
            _agent.destination = position;

            OnMoveComplete = onCompleted;

            if (_moveProcess != null)
                StopCoroutine(_moveProcess);

            _moveProcess = MoveProcess(position, onCompleted);

            StartCoroutine(_moveProcess);
        }

        public override void Stop()
        {
            _agent.isStopped = true;
        }

        public void FinishMovingManualy()
        {
            FinishMoving();
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

        private IEnumerator MoveProcess(Vector3 targetPosition, UnityAction onCompleted)
        {
            while(Vector3.Distance(_agent.transform.position, targetPosition) >= _breakDistanceThreshold)
                yield return null;

            FinishMoving();
        }

        private void FinishMoving()
        {
            if (OnMoveComplete != null)
                OnMoveComplete.Invoke();

            OnMovingFinished?.Invoke();
        }
    }
}
