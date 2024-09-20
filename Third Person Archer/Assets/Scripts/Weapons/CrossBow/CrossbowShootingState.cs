using DG.Tweening;
using System.Diagnostics;

namespace Actor
{
    public class CrossbowShootingState : ProcessState, IActorIniter
    {
        private CrossbowController _crossbowController; 
        private AttackInput _attackInput;

        Tween _tween;

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

            _tween?.Kill();

            _crossbowController.ShowView();

            _attackInput.OnAttackRelease.AddListener(Shoot);
        }

        public override void Exit()
        {
            base.Exit();

            _attackInput.OnAttackRelease.RemoveListener(Shoot);
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
            _tween?.Kill();

            _attackInput.OnAttackRelease.RemoveListener(Shoot);
            _crossbowController.Shoot(null);

            _tween = DOVirtual.DelayedCall(0.5f, FinishProcess);
        }
    }
}