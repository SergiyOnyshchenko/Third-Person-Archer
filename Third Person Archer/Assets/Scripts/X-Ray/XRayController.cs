using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

namespace Actor
{
    public class XRayController : System
    {
        private XRayInstance[] _instances;

        private void Awake()
        {
            _instances = GetComponentsInChildren<XRayInstance>();
        }

        public void Show(bool value)
        {
            foreach (var instance in _instances)
            {
                instance.Show(value);
            }
        }
    }
}