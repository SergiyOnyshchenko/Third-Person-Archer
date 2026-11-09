using Actor.Properties;
using UnityEngine;

namespace Actor
{
    /// <summary>Plays the surface impact (<see cref="ImpactEffects"/>) where the projectile lands.</summary>
    public class ProjectileImpact : System, IActorIniter
    {
        [Tooltip("Off for projectiles aimed at the player: their hits on him have their own screen feedback.")]
        [SerializeField] private bool _onActors = true;

        private Transform _transform;
        private Projectile _projectile;
        private ProjectileHitHandler _hitHandler;
        private ProjectileLastHitData _hitData;

        public void InitActor(ActorController actor)
        {
            _transform = actor.transform;
            _projectile = actor.GetComponent<Projectile>();

            if (actor.TryGetProperty(out _hitData)) { }
            if (actor.TryGetSystem(out _hitHandler))
                _hitHandler.OnHited.AddListener(Play);
        }

        private void OnDestroy()
        {
            if (_hitHandler != null)
                _hitHandler.OnHited.RemoveListener(Play);
        }

        private void Play()
        {
            Collision collision = _hitData != null ? _hitData.Value : null;
            if (collision == null || collision.collider == null)
                return;

            if (!_onActors && collision.collider.TryGetComponent(out Hitbox _))
                return;

            Vector3 point = _transform.position;
            Vector3 normal = -_transform.forward;
            if (collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                point = contact.point;
                normal = contact.normal;
            }

            // The surface faces the shooter, whichever way the contact reports it.
            if (Vector3.Dot(normal, _transform.forward) > 0f)
                normal = -normal;

            float strength = _projectile != null ? _projectile.Power : 1f;
            ImpactEffects.Play(collision.collider, point, normal, _transform.forward, strength);
        }
    }
}
