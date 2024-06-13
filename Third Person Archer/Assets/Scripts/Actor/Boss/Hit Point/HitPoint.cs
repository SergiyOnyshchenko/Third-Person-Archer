using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Hitbox))]
public class HitPoint : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    private Hitbox _hitbox;
    private Collider[] _colliders;
    public UnityEvent OnDamaged = new UnityEvent();
    public UnityEvent OnActivated = new UnityEvent();
    public UnityEvent OnDeactivated = new UnityEvent();

    private void Awake()
    {
        _hitbox = GetComponent<Hitbox>();
        _colliders = GetComponentsInChildren<Collider>();
    }

    private void OnEnable()
    {
        _hitbox.OnDamaged += DoDamage;
    }

    private void OnDisable()
    {
        _hitbox.OnDamaged -= DoDamage;
    }

    private void Start()
    {
        Desactivate();
    }

    public void Activate()
    {
        Activate(true);
    }

    public void Desactivate()
    {
        Activate(false);
    }

    public void Activate(bool value)
    {
        _isActive = value;

        if(_colliders == null)
            _colliders = GetComponentsInChildren<Collider>();

        foreach (var collider in _colliders)
            collider.enabled = value;

        if (_isActive)
            OnActivated?.Invoke();
        else
            OnDeactivated?.Invoke();
    }

    private void DoDamage(int damage)
    {
        OnDamaged?.Invoke();
    }
}
