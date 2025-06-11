using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lifetime : MonoBehaviour
{
    [SerializeField] private float _lifetime;
    [SerializeField] private bool _onStart = true;

    private IEnumerator _counter;

    public UnityEvent OnLifetimeEnded = new UnityEvent();

    private void Start()
    {
        if(_onStart)
            StartLifetime();
    }

    public void StartLifetime(float lifetime)
    {
        _lifetime = lifetime;
        StartLifetime();
    }

    public void StartLifetime()
    {
        if (_counter != null)
            StopCoroutine(_counter);

        _counter = CountLifetime();

        StartCoroutine(_counter);
    }

    private IEnumerator CountLifetime()
    {
        yield return new WaitForSeconds(_lifetime);
        Die();
    }

    private void Die()
    {
        OnLifetimeEnded?.Invoke();
        Destroy(gameObject);
    }
}
