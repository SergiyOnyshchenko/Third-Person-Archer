using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;

namespace Actor
{
    public class ActorController : MonoBehaviour
    {
        private Input[] _inputs;
        private System[] _systems;
        private Property[] _properties;
        public bool IsDead { get; private set; }

        protected virtual void Awake()
        {
            InitInputs();
            InitProperties();
            InitSystems();

            InitChilds();
        }

        public void DeathHandler()
        {
            IsDead = true;
        }

        public bool TryGetInput<T>(out T t) where T : Input => TryGetActorComponent<T, Input>(out t, _inputs);
        public bool TryGetSystem<T>(out T t) where T : System => TryGetActorComponent<T, System>(out t, _systems);
        public bool TryGetProperty<T>(out T t) where T : Property => TryGetActorComponent<T, Property>(out t, _properties);

        public bool TryGetInputs<T>(out T[] t) where T : Input => TryGetActorComponents<T, Input>(out t, _inputs);
        public bool TryGetSystems<T>(out T[] t) where T : System => TryGetActorComponents<T, System>(out t, _systems);
        public bool TryGetPropertys<T>(out T[] t) where T : Property => TryGetActorComponents<T, Property>(out t, _properties);

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

        private bool TryGetActorComponents<T, B>(out T[] t, B[] array) where T : MonoBehaviour
        {
            List<T> components = new List<T>();

            for (int i = 0; i < array.Length; i++)
            {
                var type = array[i].GetType();

                while (type != typeof(B))
                {
                    if (type == typeof(T))
                    {
                        components.Add(array[i] as T);
                    }

                    type = type.BaseType;
                }
            }

            if (components.Count == 0)
            {
                t = null;
                return false;
            }
            else
            {
                t = components.ToArray();
                return true;
            }
        }

        public void InitInputs()
        {
            _inputs = GetComponentsInChildren<Input>();
        }
        public void InitProperties()
        {
            _properties = GetComponentsInChildren<Property>();
        }

        public void InitSystems()
        {
            _systems = GetComponentsInChildren<System>();
        }

        private void InitChilds()
        {
            var initers = GetComponentsInChildren<IActorIniter>(true);

            foreach (var initer in initers)
                initer.InitActor(this);
        }
    }
}