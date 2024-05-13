using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class EnemyAimInput : AimInput, IActorIniter
    {
        [SerializeField] private Transform _aimPoint;
        private PerceptionInput _perception;

        public override Vector3 GetAimDirection()
        {
            Transform aimTarget = GetAimTarget();

            if (aimTarget == null)
                return _aimPoint.forward;
            else
                return (aimTarget.position - _aimPoint.position).normalized;
        }

        public override Transform GetAimTarget()
        {
            if (_perception == null)
                return null;

            if(_perception.Target == null)
                return null;

            return _perception.Target.TargetPoint;
        }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out PerceptionInput perception))
                _perception = perception;
        }
    }
}