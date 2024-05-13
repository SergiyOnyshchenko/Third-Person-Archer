using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class Aimer : System, IActorIniter
    {
        private PerceptionInput _input;
        public ITarget Target { get; private set; }

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetInput(out PerceptionInput input))
            {
                _input = input;
                _input.OnTargetFinded.AddListener(SetTarget);
            }
        }

        private void Update()
        {
            if (Target == null)
                return;


        }

        public void SetTarget()
        {
            Target = _input.Target;
        }
    }
}
