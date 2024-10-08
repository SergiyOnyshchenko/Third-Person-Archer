using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class PlayerAttackInput : AttackInput
    {
        private float _delay = 0.1f;
        private float _timer;

        private void Update()
        {
            if (!IsActive)
                return;

            if (UnityEngine.Input.GetMouseButton(0))
            {
                _timer += Time.unscaledDeltaTime;

                if (!_isHold && _timer >= _delay)
                {
                    SendAttackStart();
                    _isHold = true;
                }
            }
            else
            {
                if (_isHold)
                {
                    SendAttackRelease();
                    _isHold = false;
                }

                _timer = 0;
            }
        }
    }
}
