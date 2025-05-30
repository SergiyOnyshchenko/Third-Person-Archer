using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayDestructionHelper : MonoBehaviour
{
    [SerializeField] private Rigidbody[] _destrParts;

    private const float _mass = 1f;
    private const float _drag = 20f;
    private const float _angularDrag = 2f;

    private void OnEnable()
    {
        foreach (var part in _destrParts)
        {
            ApplySettings(part);
        }
    }

    private void ApplySettings(Rigidbody rb)
    {
        rb.mass = _mass;
        rb.drag = _drag;
        rb.angularDrag = _angularDrag;
        rb.isKinematic = true;
    }
}
