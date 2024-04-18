using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public interface IMoveInput
    {
        public bool IsMoving { get; }
        public Vector3 MovePostion { get; }
    }
}
