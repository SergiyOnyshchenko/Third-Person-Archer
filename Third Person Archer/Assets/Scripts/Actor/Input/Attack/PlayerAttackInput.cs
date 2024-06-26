using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class PlayerAttackInput : AttackInput
    {
        private void Update()
        {
            if (!IsActive)
                return;

            if (UnityEngine.Input.GetMouseButton(0))
            {
                if(!_isHold)
                {
                    _isHold = true;
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                SendAttackRelease();
                _isHold = false;
            }
        }
    }
}
