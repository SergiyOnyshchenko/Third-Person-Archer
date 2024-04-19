using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class ShootingInput : Input
    {
        public void StartShooting()
        {
            IsActive = true;
        }

        public void FinishShooting()
        {
            IsActive = false;
        }
    }
}
