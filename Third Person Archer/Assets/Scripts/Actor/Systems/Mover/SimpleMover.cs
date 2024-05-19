using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UIElements;

namespace Actor
{
    public class SimpleMover : Mover
    {
        private Transform _transform;

        public override void InitActor(ActorController actor)
        {
            base.InitActor(actor);
            _transform = actor.transform;
        }

        public override void Move(Transform destination, UnityAction onCompleted = null)
        {
            Move(destination.position, onCompleted);
        }

        public override void Move(Vector3 position, UnityAction onCompleted = null)
        {
            Debug.Log("MOOOOOVE");

            float distance = Vector3.Distance(_transform.position, position);
            float duration = distance / _speed.Value;

            _transform.DOMove(position, duration).OnComplete(() =>
            {
                onCompleted?.Invoke();
                OnMovingFinished?.Invoke();
            });   
        }

        public override void Stop()
        {
            
        }
    }
}