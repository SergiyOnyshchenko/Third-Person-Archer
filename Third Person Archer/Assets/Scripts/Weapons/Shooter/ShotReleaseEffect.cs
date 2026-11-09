using UnityEngine;

namespace Actor
{
    /// <summary>A puff of air where the player's shot leaves the weapon, stronger the harder the draw.</summary>
    public class ShotReleaseEffect : System, IActorIniter
    {
        [Tooltip("How far along the shot the puff appears (m), so it is not hidden inside the weapon.")]
        [SerializeField] private float _forward = 0.35f;

        private PlayerProjectileShooter[] _shooters;

        public void InitActor(ActorController actor)
        {
            _shooters = actor.GetComponentsInChildren<PlayerProjectileShooter>(true);
            foreach (var shooter in _shooters)
                shooter.OnShooted.AddListener(Play);
        }

        private void OnDestroy()
        {
            if (_shooters == null)
                return;

            foreach (var shooter in _shooters)
                if (shooter != null)
                    shooter.OnShooted.RemoveListener(Play);
        }

        private void Play(Projectile projectile)
        {
            if (projectile == null)
                return;

            Vector3 direction = projectile.transform.forward;
            ImpactEffects.Play(ImpactSurface.Release, projectile.transform.position + direction * _forward, direction, direction, projectile.Power);
        }
    }
}
