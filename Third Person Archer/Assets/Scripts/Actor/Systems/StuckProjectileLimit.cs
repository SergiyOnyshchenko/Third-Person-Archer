using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    /// <summary>
    /// Projectiles stay stuck where they land for the rest of the level. Past a limit the oldest ones go,
    /// so a long level doesn't pile up hundreds of them (each still carries its renderers and effects).
    /// </summary>
    public class StuckProjectileLimit : System, IActorIniter
    {
        [SerializeField] private int _limit = 24;
        [Tooltip("A projectile is never removed sooner than this after it landed (s), e.g. while the kill cam looks at it.")]
        [SerializeField] private float _minAge = 8f;

        private static readonly LinkedList<(GameObject projectile, float time)> _stuck = new LinkedList<(GameObject, float)>();

        private GameObject _projectile;
        private ProjectileHitHandler _hitHandler;

        public void InitActor(ActorController actor)
        {
            _projectile = actor.gameObject;
            if (actor.TryGetSystem(out _hitHandler))
                _hitHandler.OnHited.AddListener(Register);
        }

        private void OnDestroy()
        {
            if (_hitHandler != null)
                _hitHandler.OnHited.RemoveListener(Register);
        }

        private void Register()
        {
            _stuck.AddLast((_projectile, Time.time));

            // Already gone (scene change, the target removed with its arrows) don't count.
            for (var node = _stuck.First; node != null;)
            {
                var next = node.Next;
                if (node.Value.projectile == null)
                    _stuck.Remove(node);
                node = next;
            }

            while (_stuck.Count > _limit && Time.time - _stuck.First.Value.time >= _minAge)
            {
                Destroy(_stuck.First.Value.projectile);
                _stuck.RemoveFirst();
            }
        }
    }
}
