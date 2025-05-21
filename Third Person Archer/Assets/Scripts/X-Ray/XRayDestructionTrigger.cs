using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayDestructionTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }
    }
}
