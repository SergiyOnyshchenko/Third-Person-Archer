using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public class ProjectileElementalTrigger : System, IActorIniter
    {
        [SerializeField] private Collider _elementalTrigger;

        private CollisionTriggerHandler _collisionTriggerHandler;
        private ElementalProperty _elemental;
        private ProjectileEnemiesLayermask _enemiesLayermask;

        public void InitActor(ActorController actor)
        {
            _collisionTriggerHandler = actor.GetComponent<CollisionTriggerHandler>();

            if (actor.TryGetProperty(out _elemental)) { }
            if (actor.TryGetProperty(out _enemiesLayermask)) { }

            if (_collisionTriggerHandler != null)
                _collisionTriggerHandler.TriggerEnter.AddListener(TriggerHandler);

            if (_elemental != null)
                _elemental.OnPropertyChanged += ElementalChangeHandler;
        }

        private void OnDestroy()
        {
            if (_collisionTriggerHandler != null)
                _collisionTriggerHandler.TriggerEnter.RemoveListener(TriggerHandler);

            if (_elemental != null)
                _elemental.OnPropertyChanged -= ElementalChangeHandler;
        }

        private void TriggerHandler(Collider other)
        {
            if (_elemental == null || _elemental.Value == ElementalType.NULL)
                return;

            if ((_enemiesLayermask.Value.value & (1 << other.transform.gameObject.layer)) > 0)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4f, _enemiesLayermask.Value);

                foreach (var hitCollider in hitColliders)
                {
                    switch (_elemental.Value)
                    {
                        case ElementalType.FIRE:

                            if (hitCollider.TryGetComponent(out ITriggerReciever fireTrigger))
                            {
                                fireTrigger.ReciveTrigger("Burn", gameObject);
                            }

                            break;
                        case ElementalType.FROST:

                            if (hitCollider.TryGetComponent(out ITriggerReciever frostTrigger))
                            {
                                frostTrigger.ReciveTrigger("Freeze", gameObject);
                            }

                            break;
                    }
                }
            }
        }

        private void ElementalChangeHandler()
        {
            if (_elemental.Value != ElementalType.NULL && _elementalTrigger != null)
            {
                _elementalTrigger.enabled = true;
            }
        }
    }
}