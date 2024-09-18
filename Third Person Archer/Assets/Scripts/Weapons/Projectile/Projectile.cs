using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Burst.CompilerServices;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int _damage = 100;
    [SerializeField] private float _speed = 50f;
    [Space]
    [SerializeField] private LayerMask _hitLayers;
    [SerializeField] private bool _destroyAfterHit;
    [Space]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;
    [SerializeField] ElementalView _elementalView;
    private ProjectileState _state = ProjectileState.Loaded;
    private ElementalType _elementalType = ElementalType.NULL;
    private Vector3 _direction;
    private float _power;
    public UnityEvent OnShooted = new UnityEvent();
    public UnityEvent OnHited = new UnityEvent();
    public UnityEvent OnTargetHited = new UnityEvent();

    public float Speed => _speed;
    public LayerMask HitLayers { get => _hitLayers;}

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction, float power, UnityAction onHited)
    {
        _direction = direction.normalized;
        _power = power;
        _state = ProjectileState.Flying;
        transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);

        OnShooted?.Invoke();
        OnTargetHited.AddListener(onHited);

        PreCheckTargetDeath();
    }

    public void SetElementalType(ElementalType type)
    {
        _elementalType = type;

        if (_elementalView != null)
            _elementalView.SetCurrentView(type);
    }

    public RaycastHit GetPredictiveHit()
    {
        RaycastHit hit;

        Physics.Raycast(transform.position + _direction * 2, _direction, out hit, _hitLayers);

        return hit;
    }

    public bool PreCheckTargetDeath()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + _direction * 2, _direction, out hit, _hitLayers))
        {
            if (hit.collider.TryGetComponent(out IDamageChecker damageChecker))
            {
                if (damageChecker.GetHealthAfterDamage(_damage) > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void FixedUpdate()
    {
        if (_state == ProjectileState.Flying)
        {
            _rigidbody.velocity = _direction * _speed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
        }
    }

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

                if (collision.collider.TryGetComponent(out ITriggerReciever fireTrigger))
                {
                    fireTrigger.ReciveTrigger("Burn", gameObject);
                }

                if (collision.collider.TryGetComponent(out IDamageable damager1))
                {
                    damager1.DoDamage(1);
                    OnTargetHited?.Invoke();
                }

                break;
            case ElementalType.FROST:

                if (collision.collider.TryGetComponent(out ITriggerReciever frostTrigger))
                {
                    frostTrigger.ReciveTrigger("Freeze", gameObject);
                }

                if (collision.collider.TryGetComponent(out IDamageable damager2))
                {
                    damager2.DoDamage(1);
                    OnTargetHited?.Invoke();
                }

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

    public void ChangeSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
}
