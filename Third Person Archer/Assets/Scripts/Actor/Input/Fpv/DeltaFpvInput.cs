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

        void Update()
        {
            if (IsFrozen)
                return;

         /*   Debug.Log(Time.timeScale);*/
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
        }
    }
}