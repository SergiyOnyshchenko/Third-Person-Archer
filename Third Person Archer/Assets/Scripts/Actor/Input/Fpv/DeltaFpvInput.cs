using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class DeltaFpvInput : FpvInput
    {
        [SerializeField] private float _localSensitivity = 25f;
        [SerializeField] private float _overlimitSensitivity = 0.5f;
        [SerializeField] private int _frameLimit = 100;
        private Vector2 _delta;
        private Vector2 MousePosition => UnityEngine.Input.mousePosition;
        private Vector2 _inputAccelerator = new Vector2();
        private Dictionary<int, Vector2> _lastTouchPositions = new Dictionary<int, Vector2>();

        void Update()
        {
            //if (IsFrozen)
            //    return;

            Horizontal = 0;
            Vertical = 0;

            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                Touch touch = UnityEngine.Input.GetTouch(i);
                int fingerId = touch.fingerId;
                Vector2 pos = touch.position;

                if (pos.x > Screen.width * 0.5f)
                {
                    _lastTouchPositions.Remove(fingerId);
                    continue;
                }

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _lastTouchPositions[fingerId] = pos;
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_lastTouchPositions.TryGetValue(fingerId, out Vector2 lastPos))
                        {
                            Vector2 delta = (pos - lastPos) * _localSensitivity;
                            Horizontal += delta.x;
                            Vertical += delta.y;
                            _lastTouchPositions[fingerId] = pos;
                        }
                        else
                        {
                            _lastTouchPositions[fingerId] = pos;
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        _lastTouchPositions.Remove(fingerId);
                        break;
                }
            }

#if UNITY_EDITOR
            // Optional: Editor mouse fallback
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (MousePosition.x <= Screen.width * 0.5f)
                    _lastTouchPositions[-1] = MousePosition;
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                if (MousePosition.x <= Screen.width * 0.5f && _lastTouchPositions.TryGetValue(-1, out Vector2 lastMousePos))
                {
                    Vector2 delta = (MousePosition - lastMousePos) * _localSensitivity;
                    Horizontal += delta.x;
                    Vertical += delta.y;
                    _lastTouchPositions[-1] = MousePosition;
                }
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _lastTouchPositions.Remove(-1);
            }
#endif

            /*
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _delta = MousePosition;
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                Vector2 input = Vector2.zero;

                Vector2 mousePos = MousePosition;
                if (mousePos.x > Screen.width - _frameLimit)
                {
                    _inputAccelerator.x = Mathf.Lerp(_inputAccelerator.x, _overlimitSensitivity, Time.deltaTime * 5 / Time.timeScale);
                    input.x = _inputAccelerator.x;
                }
                else if (mousePos.x < _frameLimit)
                {
                    _inputAccelerator.x = Mathf.Lerp(_inputAccelerator.x, -_overlimitSensitivity, Time.deltaTime * 5 / Time.timeScale);
                    input.x = _inputAccelerator.x;
                }
                else if (mousePos.y > Screen.height - _frameLimit)
                {
                    _inputAccelerator.y = Mathf.Lerp(_inputAccelerator.y, _overlimitSensitivity, Time.deltaTime * 5 / Time.timeScale);
                    input.y = _inputAccelerator.y;
                }
                else if (mousePos.y < _frameLimit)
                {
                    _inputAccelerator.y = Mathf.Lerp(_inputAccelerator.y, -_overlimitSensitivity, Time.deltaTime * 5 / Time.timeScale);
                    input.y = _inputAccelerator.y;
                }
                else
                {
                    _inputAccelerator.x = 0;
                    _inputAccelerator.y = 0;
                    input = (MousePosition - _delta) * _localSensitivity;
                }

                Horizontal = input.x;
                Vertical = input.y;

                _delta = MousePosition;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                Horizontal = 0;
                Vertical = 0;
            }
            */
        }
    }
}