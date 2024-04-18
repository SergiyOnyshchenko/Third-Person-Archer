using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Actor.Properties;
using DG.Tweening;

namespace Actor
{
    public class ActorController : MonoBehaviour
    {
        private Input[] _inputs;
        private System[] _systems;
        private Property[] _properties;

        private void Awake()
        {
            InitInputs();
            InitProperties();
            InitSystems();

            InitChilds();
        }

        public bool TryGetInput<T>(out T t) where T : Input => TryGetActorComponent<T, Input>(out t, _inputs);
        public bool TryGetSystem<T>(out T t) where T : System => TryGetActorComponent<T, System>(out t, _systems);
        public bool TryGetProperty<T>(out T t) where T : Property => TryGetActorComponent<T, Property>(out t, _properties);


        private bool TryGetActorComponent<T, B>(out T t, B[] array) where T : MonoBehaviour
        {
            for (int i = 0; i < array.Length; i++)
            {
                var type = array[i].GetType();

                while (type != typeof(B))
                {
                    if (type == typeof(T))
                    {
                        t = array[i] as T;
                        return true;
                    }

                    type = type.BaseType;
                }
            }

            t = null;
            return false;
        }

        private void InitInputs()
        {
            _inputs = GetComponentsInChildren<Input>();
        }
        private void InitProperties()
        {
            _properties = GetComponentsInChildren<Property>();
        }

        private void InitSystems()
        {
            _systems = GetComponentsInChildren<System>(true);
        }

        private void InitChilds()
        {
            var initers = GetComponentsInChildren<IActorIniter>(true);

            foreach (var initer in initers)
                initer.InitActor(this);
        }
    }
}