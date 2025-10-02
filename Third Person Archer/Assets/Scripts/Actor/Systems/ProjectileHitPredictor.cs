using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public class ProjectileHitPredictor : System, IActorIniter
    {
        private Transform _transform;

        private Damage _damage;
        private ProjectileDirection _direction;
        //private ProjectileHitLayermask _hitLayermask;
        private ProjectileEnemiesLayermask _enemyLayermask;

        public void InitActor(ActorController actor)
        {
            _transform = actor.transform;

            if (actor.TryGetProperty(out _damage)) { }
            if (actor.TryGetProperty(out _direction)) { }
            //if (actor.TryGetProperty(out _hitLayermask)) { }
            if (actor.TryGetProperty(out _enemyLayermask)) { }
        }

        public RaycastHit GetPredictiveHit()
        {
            RaycastHit hit;
            Physics.Raycast(_transform.position + _direction.Value * 2, _direction.Value, out hit, _enemyLayermask.Value);
            return hit;
        }


        public bool PreCheckTargetDeath()
        {
            RaycastHit hit;

            if (Physics.Raycast(_transform.position + _direction.Value * 2, _direction.Value, out hit, _enemyLayermask.Value))
            {
                if (hit.collider.TryGetComponent(out IDamageChecker damageChecker))
                {
                    if (damageChecker.GetHealthAfterDamage(_damage.Value) > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}