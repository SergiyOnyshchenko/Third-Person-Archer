using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class HitedEnemyPredictCache : Property
    {
        [field: SerializeField] public GameObject Target { get; private set; }
        [field: SerializeField] public Vector3 HitPoint { get; private set; }
        [field: SerializeField] public bool EnemyWillDie { get; private set; }

        public void InitHit(GameObject target, Vector3 hitpoint, int damage)
        {
            Target = target;
            HitPoint = hitpoint;

            if (target.TryGetComponent(out IDamageChecker damageChecker))
            {
                EnemyWillDie = damageChecker.GetHealthAfterDamage(damage) <= 0;
            }
        }

        public void ResetHit()
        {
            EnemyWillDie = false;
            Target = null;
            HitPoint = Vector3.zero;
        }
    }
}