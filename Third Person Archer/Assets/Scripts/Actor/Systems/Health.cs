using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class Health : System
    {
        public UnityEvent<float, float> OnHealthModified = new UnityEvent<float, float>();
        public UnityEvent OnHealthZero = new UnityEvent();
    }
}