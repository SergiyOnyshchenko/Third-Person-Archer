using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class OnDeathTransition : StateTransition, IActorIniter
    {
        private Health _health;

        public void InitActor(ActorController actor)
        {
            if (actor.TryGetSystem(out Health health))
                _health = health;
        }

        public override void Enter()
        {
            base.Enter();

            if (_health == null)
                Debug.Log("NO HEALTH");
            else
                Debug.Log("NO HEALTH");

            _health.OnHealthZero.AddListener(DoTransition);
        }

        public override void Exit()
        {
            _health.OnHealthZero.RemoveListener(DoTransition);

            base.Exit();
        }
    }
}
