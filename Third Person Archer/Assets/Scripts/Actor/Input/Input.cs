using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public abstract class Input : MonoBehaviour
    {
        public bool IsActive { get; protected set; }
    }
}