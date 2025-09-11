using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using RayFire;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

namespace Actor
{
    public class ProjectileHitHandler : System, IActorIniter
    {
        [SerializeField] private ProjectileRangeConfig _projectileRangeConfig;
        [SerializeField] private bool _destroyAfterHit;

        private GameObject _gameObject;
        private CollisionTriggerHandler _collisionTriggerHandler;

        private ProjectileDirection _direction;
        private TraveledDistance _traveledDistance;
        private Range _range;
        private Damage _damage;
        private ElementalProperty _elemental;

        private ProjectileHitLayermask _layerMask;
        private ProjectileLastHitData _hitData;

        public UnityEvent OnHited = new UnityEvent();
        public UnityEvent OnTargetHited = new UnityEvent();

        public void InitActor(ActorController actor)
        {
            _gameObject = actor.gameObject;
            _collisionTriggerHandler = actor.GetComponent<CollisionTriggerHandler>();

            if (actor.TryGetProperty(out _damage)) { }

            if (actor.TryGetProperty(out _layerMask)) { }
            if (actor.TryGetProperty(out _hitData)) { }
            if (actor.TryGetProperty(out _elemental)) { }

            if (actor.TryGetProperty(out _direction)) { }
            if (actor.TryGetProperty(out _traveledDistance)) { }
            if (actor.TryGetProperty(out _range)) { }


            if (_collisionTriggerHandler != null)
                _collisionTriggerHandler.CollisionEnter.AddListener(CollisionHandler);
        }

        private void OnDestroy()
        {
            if (_collisionTriggerHandler != null)
                _collisionTriggerHandler.CollisionEnter.RemoveListener(CollisionHandler);
        }

        private void CollisionHandler(Collision collision)
        {
            if ((_layerMask.Value.value & (1 << collision.transform.gameObject.layer)) > 0)
            {
                _hitData.SetValue(collision);
                Hit(collision);

                if (_destroyAfterHit)
                    Destroy(_gameObject);
            }
        }

        private void Hit(Collision collision)
        {
            if (_elemental.Value == ElementalType.NULL)
            {
                if (collision.collider.TryGetComponent(out IDamageable damager3))
                {
                    damager3.DoDamage(CalculateDamage());
                    OnTargetHited?.Invoke();
                }

                if (collision.collider.TryGetComponent(out Rigidbody rigidbody))
                {
                    Vector3 pushDirection = _direction.Value + (Vector3.up * 0.25f);

                    float pushPower = 100f;
                    if (gameObject.activeInHierarchy)
                        StartCoroutine(PushWithDelay(rigidbody, pushDirection.normalized + (Vector3.up * 0.6f), pushPower, 0.1f));
                }
            }

            _gameObject.transform.SetParent(collision.transform);

            OnHited?.Invoke();
        }

        private IEnumerator PushWithDelay(Rigidbody rigidbody, Vector3 direction, float power, float delay)
        {
            yield return new WaitForSeconds(delay);
            rigidbody.AddForce(direction * power, ForceMode.VelocityChange);
        }

        private int CalculateDamage()
        {
            var damageFactor = _projectileRangeConfig.EvaluateDamageFactor(_traveledDistance.Value, _range.BaseValue);
            float damage = _damage.Value * damageFactor;
            return Mathf.RoundToInt(damage);
        }
    }
}