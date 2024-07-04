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
        private AttackInput _attackInput;

        public void InitActor(ActorController actor)
        {
            if(actor.TryGetSystem(out CrossbowController crossbow))
                _crossbowController = crossbow;

            if(actor.TryGetInput(out AttackInput attackInput))
                _attackInput = attackInput;
        }

        public override void Enter()
        {
            base.Enter();

            _crossbowController.ShowView();

            _attackInput.OnAttackRelease.AddListener(Shoot);
        }

        private void Update()
        {
            //if (UnityEngine.Input.GetMouseButtonUp(0))
            //    Shoot();
        }

        private void LateUpdate()
        {
            _crossbowController.UpdateHands();
        }

        private void Shoot()
        {
            _attackInput.OnAttackRelease.RemoveListener(Shoot);
            _crossbowController.Shoot(null);

            DOVirtual.DelayedCall(0.5f, FinishProcess);
        }
    }
}