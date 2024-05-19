using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    [Serializable]
    public class ActorAction
    {
        [SerializeField] private string _actionName;
        public string ActionName { get => _actionName; }
        public UnityEvent OnAction = new UnityEvent();

        public bool TryDoAction(string actionName)
        {
            if(_actionName != actionName)
                return false;

            OnAction?.Invoke();
            return true;
        }
    }

    public class ActionInput : Input
    {
        [SerializeField] private ActorAction[] _actorActions;
        public UnityEvent<string> OnAction = new UnityEvent<string>();

        public bool TryDoAction(string actionName)
        {
            foreach (var actorAction in _actorActions)
            {
                if (actorAction.TryDoAction(actionName))
                {
                    OnAction?.Invoke(actionName);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAction(string actionName, out ActorAction targetAction)
        {
            targetAction = null;

            foreach (var actorAction in _actorActions)
            {
                if (actorAction.ActionName == actionName)
                {
                    targetAction = actorAction;
                    return true;
                }
            }

            return false;
        }
    }
}