using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int _damage = 100;
    [SerializeField] private float _speed = 50f;
    [Space]
    [SerializeField] private LayerMask _hitLayers;
    [SerializeField] private Rigidbody _rigidbody;
    private ProjectileState _state = ProjectileState.Loaded;
    private Vector3 _direction;
    private float _power;

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction, float power)
    {
        _direction = direction.normalized;
        _power = power;
        _state = ProjectileState.Flying;
        transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
    }

    public void FixedUpdate()
    {
        if (_state == ProjectileState.Flying)
        {
            //transform.position += _direction * Time.fixedDeltaTime * _speed;
            _rigidbody.velocity = _direction * _speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_state == ProjectileState.Flying)
        {
            if ((_hitLayers.value & (1 << collision.transform.gameObject.layer)) > 0)
            {
                _state = ProjectileState.Hited;
                _rigidbody.isKinematic = true;
            }
        }
    }
}
