using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] private float _lifetime;
    [SerializeField] private bool _onStart = true;

    private void Start()
    {
        if(_onStart)
            StartLifetime();
    }

    public void StartLifetime()
    {
        StartCoroutine(CountLifetime());
    }

    private IEnumerator CountLifetime()
    {
        yield return new WaitForSeconds(_lifetime);
        Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
