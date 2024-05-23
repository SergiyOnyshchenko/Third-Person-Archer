using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Actor
{
    [Serializable]
    public class ActorEvent
    {
        [SerializeField] private string _name;
        public string Name { get => _name; }
        public UnityEvent Event = new UnityEvent();

        public bool TryInvoke(string name)
        {
            if (_name != name)
                return false;

            Invoke();
            return true;
        }

        public void Invoke()
        {
            Event?.Invoke();
        }
    }

    public class EventSystem : System
    {
        [SerializeField] private ActorEvent[] _actorEvents;

        public bool TryInvokeEvent(string name)
        {
            foreach (var actorEvent in _actorEvents)
            {
                if(actorEvent.TryInvoke(name))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetEvent(string name, out ActorEvent targetEvent)
        {
            targetEvent = null;

            foreach (var actorEvent in _actorEvents)
            {
                if (actorEvent.Name == name)
                {
                    targetEvent = actorEvent;
                    return true;
                }
            }

            return false;
        }
    }
}