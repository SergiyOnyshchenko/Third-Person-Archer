using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

namespace Actor
{
    public class HeadRotator : System, IActorIniter
    {
        [Range(0.1f, 5f)][SerializeField] float sensitivity = 2f;
        [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
        [Range(0f, 90f)][SerializeField] float xRotationLimit = 90f;
        [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
        private Vector2 _rotation = Vector2.zero;
        private FpvInput _input;
        public float Sensitivity { get { return sensitivity; } set { sensitivity = value; } }

        private void OnEnable()
        {
            _rotation = Vector2.zero;
            transform.localEulerAngles = Vector3.zero;
        }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out FpvInput input))
                _input = input;
        }

        private void Update()
        {
            if (_input == null)
                return;

            _rotation.x += _input.Horizontal * sensitivity/* * _input.Distance*/;
            _rotation.y += _input.Vertical * sensitivity/* * _input.Distance*/;

            _rotation.x = Mathf.Clamp(_rotation.x, -xRotationLimit, xRotationLimit);
            _rotation.y = Mathf.Clamp(_rotation.y, -yRotationLimit, yRotationLimit);

            var xQuat = Quaternion.AngleAxis(_rotation.x, Vector3.up);
            var yQuat = Quaternion.AngleAxis(_rotation.y, Vector3.left);

            transform.localRotation = Quaternion.Lerp(transform.localRotation, xQuat * yQuat, Time.deltaTime* 15);
        }
    }
}