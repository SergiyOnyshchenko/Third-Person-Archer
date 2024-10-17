using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollControll : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody[] _allRigidbodys;
    [SerializeField] private bool _isPhysical;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        _allRigidbodys = GetComponentsInChildren<Rigidbody>(true);

        MakeKinematic();
    }

    public void MakeKinematic()
    {
        if (_animator != null)
            _animator.enabled = true;

        foreach (Rigidbody rigidbody in _allRigidbodys)
            rigidbody.isKinematic = true;

        _isPhysical = false;
    }

    public void MakePhysical()
    {
        if (_isPhysical)
            return;

        if (_animator != null)
            _animator.enabled = false;

        _isPhysical = true;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Delay());
            StartCoroutine(Freeze());
        }
        else
        {
            foreach (Rigidbody rigidbody in _allRigidbodys)
            {
                rigidbody.isKinematic = false;
                rigidbody.velocity = Vector3.zero;
            }
            for (int i = 0; i < 7; i++)
            {
                foreach (Rigidbody rigidbody in _allRigidbodys)
                {
                    rigidbody.velocity = Vector3.zero;
                }
            }
        }
    }

    private IEnumerator Delay()
    {
        foreach (Rigidbody rigidbody in _allRigidbodys)
        {
            rigidbody.isKinematic = false;
            rigidbody.velocity = Vector3.zero;
        }
        for (int i = 0; i < 7; i++)
        {
            yield return null;
            foreach (Rigidbody rigidbody in _allRigidbodys)
            {
                rigidbody.velocity = Vector3.zero;
            }
        }
    }

    private IEnumerator Freeze()
    {
        yield return new WaitForSeconds(5);

        foreach (Rigidbody rigidbody in _allRigidbodys)
            rigidbody.isKinematic = true;
    }
}
