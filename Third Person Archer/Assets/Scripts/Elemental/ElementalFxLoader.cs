using Actor.Properties;
using UnityEngine;

namespace Actor
{
    /// <summary>
    /// Fire and frost effects are heavy (dozens of particle systems) and almost never used, so a projectile no longer
    /// carries them: they live in a separate prefab that is attached only when the projectile gets an element.
    /// Each top child of that prefab has an <see cref="ElementalFxMount"/> saying where in the projectile it belongs.
    /// </summary>
    public class ElementalFxLoader : System, IActorIniter
    {
        [SerializeField] private GameObject _prefab;

        private ActorController _actor;
        private ElementalProperty _elemental;
        private bool _loaded;

        public void InitActor(ActorController actor)
        {
            _actor = actor;

            if (actor.TryGetProperty(out _elemental))
            {
                _elemental.OnPropertyChanged += Load;
                Load();
            }
        }

        private void OnDestroy()
        {
            if (_elemental != null)
                _elemental.OnPropertyChanged -= Load;
        }

        private void Load()
        {
            if (_loaded || _prefab == null || _elemental == null || _elemental.Value == ElementalType.NULL)
                return;

            _loaded = true;

            GameObject fx = Instantiate(_prefab, _actor.transform);
            ElementalView view = fx.GetComponent<ElementalView>();

            foreach (var mount in fx.GetComponentsInChildren<ElementalFxMount>(true))
                mount.Attach(_actor.transform);

            // Its view switches the attached parts on and off with the element, as the projectile's own did.
            if (view != null)
                view.InitActor(_actor);
        }
    }
}
