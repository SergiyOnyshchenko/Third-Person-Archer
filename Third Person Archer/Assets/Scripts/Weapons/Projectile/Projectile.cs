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
    private ProjectileState _state = ProjectileState.Loaded;
    private Vector3 _direction;
    private float _power;
    public UnityEvent OnShooted = new UnityEvent();
    public UnityEvent OnHited = new UnityEvent();
    public UnityEvent OnTargetHited = new UnityEvent();

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

    public bool PreCheckTargetDeath()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + _direction * 2, _direction, out hit, _hitLayers))
        {
            if (hit.collider.TryGetComponent(out IDamageChecker damageChecker))
            {
                if (damageChecker.GetHealthAfterDamage(_damage) > 0)
                    return false;
                else
                    return true;
            }
        }

        return false;
    }

    public void FixedUpdate()
    {
        if (_state == ProjectileState.Flying)
        {
            _rigidbody.velocity = _direction * _speed * Time.fixedDeltaTime;
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
        //transform.position -= transform.forward * 0.6f;

        if (collision.collider.TryGetComponent(out IDamageable damager))
        {
            damager.DoDamage(Mathf.RoundToInt(_damage * _power));
            OnTargetHited?.Invoke();
        }

        if (collision.collider.TryGetComponent(out Rigidbody rigidbody))
        {
            Vector3 pushDirection = _direction + (Vector3.up * 0.25f);
            StartCoroutine(PushWithDelay(rigidbody, pushDirection.normalized, 50f * _power, 0.1f));
        }

        OnHited?.Invoke();

        if(_destroyAfterHit)
            Destroy(gameObject);
    }

    private IEnumerator PushWithDelay(Rigidbody rigidbody, Vector3 direction, float power, float delay)
    {
        yield return new WaitForSeconds(delay);
        rigidbody.AddForce(direction * power, ForceMode.VelocityChange);
    }

}
