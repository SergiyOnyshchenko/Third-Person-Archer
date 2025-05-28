using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Actor;
using Actor.Properties;

public class Projectile : MonoBehaviour, IActorIniter
{
    [field: SerializeField] public ActorController Actor {  get; private set; }

    //Properties
    [SerializeField] private ProjectileHitLayermask _hitLayermask;
    private ProjectileDirection __direction;
    private ElementalProperty _elemental;
    private Speed __speed;

    //Systems
    private FeedbackManager _feedbackManager;
    private ProjectileHitHandler _hitHandler;
    private ProjectileHitPredictor _hitPredictor;


    [SerializeField] private int _damage = 100;
    [SerializeField] private float _speed = 50f;
    [Space]
    [SerializeField] private LayerMask _hitLayers;
    [SerializeField] private LayerMask _enemyLayers;
    [Space]
    [SerializeField] private bool _destroyAfterHit;
    [Space]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;
    [SerializeField] private Collider _elementalTrigger;
    [SerializeField] private ElementalView _elementalView;
    [SerializeField] private GameObject _feedbacks;
    private ProjectileState _state = ProjectileState.Loaded;
    private ElementalType _elementalType = ElementalType.NULL;
    private Vector3 _direction;
    private float _power;
    public UnityEvent OnShooted = new UnityEvent();
    public UnityEvent OnHited = new UnityEvent();
    public UnityEvent OnTargetHited = new UnityEvent();
    public float Speed => _speed;
    public LayerMask HitLayers { get => _hitLayermask.Value; }
    public int Damage { get => _damage; }


    private void Awake()
    {
        Actor = GetComponent<ActorController>();

        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out __direction)) { }
        if (actor.TryGetProperty(out _elemental)) { }
        if (actor.TryGetProperty(out __speed)) { }

        if (actor.TryGetSystem(out _feedbackManager)) { }
        if (actor.TryGetSystem(out _hitHandler)) { }
        if (actor.TryGetSystem(out _hitPredictor)) { }
    }

    public void Shoot(Vector3 direction, float power, UnityAction onHited)
    {
        __direction.SetValue(direction);

        OnShooted?.Invoke();
        _hitHandler.OnTargetHited.AddListener(onHited);

        _hitHandler.OnHited = OnHited;

        PreCheckTargetDeath();

        /*
        _direction = direction.normalized;
        _power = power;
        _state = ProjectileState.Flying;


        OnShooted?.Invoke();
        OnTargetHited.AddListener(onHited);

        PreCheckTargetDeath();
        */
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void SetElementalType(ElementalType type)
    {
        _elemental.SetValue(type);
    }

    public void SetSpeed(float newSpeed)
    {
        if (__speed == null)
            return;

        __speed.SetValue(newSpeed);
    }

    public void EnableFeedbacks(bool value)
    {
        if (_feedbackManager == null)
            return;

        _feedbackManager.SetAllFeedbacksEnabled(value);
    }

    public RaycastHit GetPredictiveHit()
    {
        return _hitPredictor.GetPredictiveHit();
    }

    public bool PreCheckTargetDeath()
    {
        return _hitPredictor.PreCheckTargetDeath();
    }

    /*
    public void FixedUpdate()
    {
        if (_state == ProjectileState.Flying)
        {
            _rigidbody.velocity = _direction * _speed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
        }
    }
    */

    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (_state == ProjectileState.Flying)
        {
            if ((_hitLayers.value & (1 << collision.transform.gameObject.layer)) > 0)
            {
                Hit(collision);
            }
        }
    }
    */

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (_elementalType == ElementalType.NULL)
            return;

        OnTargetHited?.Invoke();
        OnHited?.Invoke();

        if ((_enemyLayers.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            _collider.enabled = false;
            _elementalTrigger.enabled = false;

            _state = ProjectileState.Hited;
            _rigidbody.isKinematic = true;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4f, _enemyLayers);

            foreach (var hitCollider in hitColliders)
            {
                switch (_elementalType)
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
    */

    /*
    private void Hit(Collision collision)
    {
        _collider.enabled = false;

        _state = ProjectileState.Hited;
        _rigidbody.isKinematic = true;

        transform.parent = collision.transform;
        transform.position = collision.contacts[0].point;

        switch (_elementalType)
        {
            case ElementalType.FIRE:

                break;
            case ElementalType.FROST:

                break;
            default:

                if (collision.collider.TryGetComponent(out IDamageable damager3))
                {
                    damager3.DoDamage(Mathf.RoundToInt(_damage * _power));
                    OnTargetHited?.Invoke();
                }

                if (collision.collider.TryGetComponent(out Rigidbody rigidbody))
                {
                    Vector3 pushDirection = _direction + (Vector3.up * 0.25f);

                    if(gameObject.activeInHierarchy)
                        StartCoroutine(PushWithDelay(rigidbody, pushDirection.normalized, 50f * _power, 0.1f));
                }

                break;
        }

        OnHited?.Invoke();

        if (_destroyAfterHit)
            Destroy(gameObject);
    }


    private IEnumerator PushWithDelay(Rigidbody rigidbody, Vector3 direction, float power, float delay)
    {
        yield return new WaitForSeconds(delay);
        rigidbody.AddForce(direction * power, ForceMode.VelocityChange);
    }
    */


}
