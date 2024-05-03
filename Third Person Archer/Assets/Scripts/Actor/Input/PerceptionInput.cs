using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Actor
{
    public class PerceptionInput : Input
    {
        [SerializeField] private ActorController _target;
        public ActorController Target { get => _target; }

        private void Start()
        {
            ActivatePerception(true);
        }

        public void ActivatePerception(bool value)
        {
            IsActive = value;
        }
    }
}
