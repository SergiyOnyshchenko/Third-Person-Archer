using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public class PlayerAttackInput : AttackInput
    {
        [SerializeField] private UIButtonHold _button;

        private float _delay = 0.1f;
        private float _timer;
        private bool _isFrozen;

        public void FreezeAttack()
        {
            _isHold = false;
            _isFrozen = true;
            _timer = 0;
        }

        public void UnfreezeAttack()
        {
            _isFrozen = false;
        }

        private void Update()
        {
            if (_isFrozen)
                return;

            if (!IsActive)
                return;

#if UNITY_EDITOR
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(0))
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
#else

            if (_button.IsHolding)
            {
                if (!_isHold)
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
            }

#endif

        }

    }

    }
