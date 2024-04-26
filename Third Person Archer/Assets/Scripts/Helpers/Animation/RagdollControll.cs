using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollControll : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private Rigidbody[] _allRigidbodys;

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
    }

    public void MakePhysical()
    {
        if (_animator != null)
            _animator.enabled = false;

        foreach (Rigidbody rigidbody in _allRigidbodys)
        {
            rigidbody.isKinematic = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
}
