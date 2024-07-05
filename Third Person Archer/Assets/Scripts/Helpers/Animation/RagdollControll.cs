using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollControll : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private Rigidbody[] _allRigidbodys;
    private bool _isPhysical;

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

        IEnumerator Delay()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            foreach (Rigidbody rigidbody in _allRigidbodys)
            {
                rigidbody.isKinematic = false;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        _isPhysical = true;

        StartCoroutine(Delay());
        StartCoroutine(Freeze());
    }

    private IEnumerator Freeze()
    {
        yield return new WaitForSeconds(5);

        foreach (Rigidbody rigidbody in _allRigidbodys)
            rigidbody.isKinematic = true;
    }
}
