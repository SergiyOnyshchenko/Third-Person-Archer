using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class FpvAimInput : AimInput, IActorIniter
    {
        private Transform _fpvCameraPoint;

        public override Vector3 GetAimDirection()
        {
            return _fpvCameraPoint.forward;
        }

        public override Transform GetAimTarget()
        {
            return null;
        }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out CameraPOV pov))
                _fpvCameraPoint = pov.FpvCamera.transform;
        }
    }
}
