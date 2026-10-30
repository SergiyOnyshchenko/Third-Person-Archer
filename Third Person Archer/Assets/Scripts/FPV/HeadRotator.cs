using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;
using Cinemachine;

namespace Actor
{
    public class HeadRotator : System, IActorIniter
    {
        [Range(0.1f, 5f)][SerializeField] float sensitivity = 2f;
        [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
        [Range(0f, 90f)][SerializeField] float xRotationLimit = 90f;
        [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
        [Header("Feel")]
        [Tooltip("How fast the camera catches up with the finger (1/s, real time — slow-mo doesn't make it lag).")]
        [SerializeField, Min(1f)] private float _followRate = 17f;
        [Tooltip("Sensitivity at full draw (1 = unchanged). Drawing steadies the aim for the final adjustment.")]
        [SerializeField, Range(0.2f, 1f)] private float _drawSensitivity = 0.8f;
        [Tooltip("Scale sensitivity with the zoom so the crosshair moves across the screen at the same speed.")]
        [SerializeField] private bool _zoomCompensation = true;
        [Tooltip("How fast sensitivity changes (draw, zoom, aim slow) blend in (1/s).")]
        [SerializeField, Min(0.1f)] private float _sensitivityBlendRate = 10f;
        private Vector2 _rotation = Vector2.zero;
        private FpvInput _input;
        private WeaponPull _pull;
        private NormalFov _normalFov;
        private CinemachineVirtualCamera _camera;
        private AimAssist _assist;
        private float _sensitivityScale = 1f;
        public float Sensitivity { get { return sensitivity; } set { sensitivity = value; } }

        private void OnEnable()
        {
            _rotation = Vector2.zero;
            _sensitivityScale = 1f;
            transform.localEulerAngles = Vector3.zero;
        }

        private void OnDisable()
        {
            if (_assist != null)
                _assist.Release();
        }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out FpvInput input))
                _input = input;

            if (actor.TryGetProperty(out _pull)) { }
            if (actor.TryGetProperty(out _normalFov)) { }
            if (actor.TryGetSystem(out _assist)) { }

            _camera = GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            if (_input == null)
                return;

            float dt = TimeScaleUtils.AimDeltaTime;
            _sensitivityScale = Mathf.Lerp(_sensitivityScale, TargetSensitivityScale(), TimeScaleUtils.Damp(_sensitivityBlendRate, dt));

            // The input already carries the external multiplier (aim slow); it's eased here via _sensitivityScale.
            float multiplier = _input.SensitivityMultiplier > 0f ? _input.SensitivityMultiplier : 1f;
            float gain = sensitivity * _sensitivityScale / multiplier;

            var delta = new Vector2(_input.Horizontal, _input.Vertical) * gain;
            if (_assist != null)
                delta += _assist.Tick(delta);

            _rotation += delta;

            _rotation.x = Mathf.Clamp(_rotation.x, -xRotationLimit, xRotationLimit);
            _rotation.y = Mathf.Clamp(_rotation.y, -yRotationLimit, yRotationLimit);

            var xQuat = Quaternion.AngleAxis(_rotation.x, Vector3.up);
            var yQuat = Quaternion.AngleAxis(_rotation.y, Vector3.left);

            if (IsNaN(xQuat) || IsNaN(yQuat))
                return;

            transform.localRotation = Quaternion.Slerp(transform.localRotation, xQuat * yQuat, TimeScaleUtils.Damp(_followRate, dt));
        }

        private float TargetSensitivityScale()
        {
            float scale = _input.SensitivityMultiplier > 0f ? _input.SensitivityMultiplier : 1f;

            if (_pull != null)
                scale *= Mathf.Lerp(1f, _drawSensitivity, _pull.Value);

            if (_zoomCompensation && _camera != null && _normalFov != null && _normalFov.Value > 0f)
                scale *= Mathf.Clamp(FovUtils.ZoomRatio(_camera.m_Lens.FieldOfView, _normalFov.Value), 0.25f, 1f);

            return scale;
        }

        private bool IsNaN(Quaternion q)
        {
            return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
        }
    }
}
