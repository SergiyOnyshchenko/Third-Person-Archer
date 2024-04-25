using System.Collections;
using System.Collections.Generic;
using CustomAnimation.Body;
using UnityEngine;
using DG.Tweening;

namespace Actor
{
    public class CrossbowShootingState : ProcessState, IActorIniter
    {
        private CrossbowController _crossbowController; 

        public void InitActor(ActorController actor)
        {
            if(actor.TryGetSystem(out CrossbowController crossbow))
                _crossbowController = crossbow;
        }

        public override void Enter()
        {
            base.Enter();

            _crossbowController.ShowView();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
                Shoot();
        }

        private void LateUpdate()
        {
            _crossbowController.UpdateHands();
        }

        private void Shoot()
        {
            _crossbowController.Shoot(null);
            DOVirtual.DelayedCall(1, FinishProcess);
        }
    }
}