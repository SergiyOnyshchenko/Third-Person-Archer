using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class EnemyAttackInput : AttackInput
    {
        public void Attack()
        {
            StartCoroutine(InstantAttack());
        }

        private IEnumerator InstantAttack()
        {
            _isHold = true;

            yield return new WaitForSeconds(0.1f);

            OnAttackRelease?.Invoke();
            _isHold = false;
        }
    }
}