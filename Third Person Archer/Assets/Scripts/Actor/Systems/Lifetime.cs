using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class Lifetime : System, IActorAwaker
    {
        [SerializeField] private float _maxLifetime = 5;
        private float _currentLifetime;
        public float CurrentLifetime { get => _currentLifetime;}
        public bool IsLifetimeEnded { get => _currentLifetime <= 0;}
        public UnityEvent OnLifetimeEnded = new UnityEvent();

        public void AwakeActor(ActorController actor)
        {
            _currentLifetime = _maxLifetime;
        }

        public void Tick()
        {
            if(IsLifetimeEnded)
                return;

            _currentLifetime -= Time.deltaTime;

            if(IsLifetimeEnded)
                OnLifetimeEnded?.Invoke();
        }
    }
}