using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public enum ShootType
{
    Direct,
    Trajectory
}

public class PlayerProjectileShooter : ProjectileShooter
{
    [SerializeField] private ShootType _shootType = ShootType.Direct;
    [SerializeField] private TrajectoryProfile _profile;
    private PlayerTrajectoryController _trajectory;

    private WeaponPull _weaponPull;
    private HitedEnemyPredictCache _hitedEnemyPredictCache;
    private ProjectileEnemiesLayermask _enemiesLayermask;
    private ShootTypeOverride _shootTypeOverride;

    public TrajectoryProfile Profile => _profile;

    public override void InitActor(ActorController actor)
    {
        base.InitActor(actor);

        if (actor.TryGetProperty(out _weaponPull)) { }
        if (actor.TryGetProperty(out _hitedEnemyPredictCache)) { }
        if (actor.TryGetProperty(out _enemiesLayermask)) { }

        if (actor.TryGetProperty(out _shootTypeOverride))
        {
            _shootType = _shootTypeOverride.Value;
        }

        if (actor.TryGetSystem(out _trajectory)) { }
    }

    public override void Shoot(Vector3 direction, float multiplier, UnityAction onHited)
    {
        onHited += SetTargetHitedEvent;
        _hitedEnemyPredictCache.ResetHit();

        switch (_shootType)
        {
            case ShootType.Direct:
                Projectile directProjectile = Instantiate(Prefab, _shootPoint.position, _shootPoint.rotation);

                if(_damage != null)
                    directProjectile.SetDamage(_damage.Value);

                StartCoroutine(Shooting(directProjectile, direction, multiplier, onHited));
                break;

            case ShootType.Trajectory:
                if (_profile == null || Prefab == null) return;

                Vector3 origin = _trajectory.SafeSpawnPoint(_shootPoint.position);
                Vector3 dir = _trajectory.ComputeAimDirection(origin);
                float speed = ComputeSpeedFromPull(1f, _profile);

                Projectile arrow = Instantiate(Prefab, origin, Quaternion.LookRotation(dir, Vector3.up));

                if(_damage != null)
                    arrow.SetDamage(_damage.Value);

                if (_trajectory.PredictEnemyHit(out GameObject enemy, out Vector3 hitPoint))
                    _hitedEnemyPredictCache.InitHit(enemy, hitPoint, arrow.Damage);

                arrow.SetGravity(_profile.Gravity);
                arrow.SetMoveType(ProjectileMoveType.Trajectory);
                arrow.Shoot(dir * speed, _weaponPull.Value, onHited);
                OnShooted?.Invoke(arrow);
                break;
        }
    }

    private float ComputeSpeedFromPull(float pull01, TrajectoryProfile profile)
    {
        float factor = Mathf.Clamp01(profile.PullToSpeedCurve.Evaluate(Mathf.Clamp01(pull01)));
        return Mathf.Max(0f, profile.MaxSpeed * factor);
    }

    private IEnumerator Shooting(Projectile arrow, Vector3 direction, float multiplier, UnityAction onHited)
    {
        RaycastHit hit;

        if (Physics.Raycast(_aimInput.GetAimRoot(), direction, out hit, Mathf.Infinity, arrow.HitLayers))
        {
            direction = (hit.point - _shootPoint.position).normalized;
        }
        else
        {
            direction = (PointAlongDirection(_aimInput.GetAimRoot(), direction, 100f) - _shootPoint.position).normalized;
        }

        if (Physics.Raycast(_aimInput.GetAimRoot(), direction, out hit, Mathf.Infinity, _enemiesLayermask.Value))
            _hitedEnemyPredictCache.InitHit(hit.transform.gameObject, hit.point, arrow.Damage);

        if (_shootError != null)
        {
            float horizontalAngle = Random.Range(-_shootError.Value.x, _shootError.Value.x);
            float verticalAngle = Random.Range(-_shootError.Value.y, _shootError.Value.y);

            Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
            Vector3 horizontalRotated = horizontalRotation * direction;

            Vector3 right = Vector3.Cross(Vector3.up, horizontalRotated);
            if (right == Vector3.zero)
                right = Vector3.right;

            Quaternion verticalRotation = Quaternion.AngleAxis(verticalAngle, right);
            Vector3 finalDirection = verticalRotation * horizontalRotated;

            direction = finalDirection.normalized;
        }

        arrow.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        yield return null;

        if (_elementalAttackType != null)
            arrow.SetElementalType(_elementalAttackType.Value);

        arrow.SetMoveType(ProjectileMoveType.Direct);
        arrow.Shoot(direction, multiplier, onHited);

        OnShooted?.Invoke(arrow);
    }

    private Vector3 PointAlongDirection(Vector3 origin, Vector3 direction, float distance)
    {
        return origin + direction.normalized * distance;
    }
}