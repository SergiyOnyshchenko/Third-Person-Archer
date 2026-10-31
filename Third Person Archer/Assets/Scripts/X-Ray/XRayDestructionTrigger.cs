using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayDestructionTrigger : MonoBehaviour
{
    [Tooltip("Speed (m/s) the bone shards burst out from the projectile's path.")]
    [SerializeField] private float _burst = 2.5f;
    [Tooltip("Speed (m/s) the shards are carried along the projectile's flight.")]
    [SerializeField] private float _carry = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody rb) && rb.isKinematic)
        {
            rb.isKinematic = false;

            Vector3 forward = transform.forward;
            Vector3 away = Vector3.ProjectOnPlane(rb.worldCenterOfMass - transform.position, forward);
            away = away.sqrMagnitude > 0.0001f ? away.normalized : Random.onUnitSphere;

            rb.AddForce(away * (_burst * Random.Range(0.6f, 1.2f)) + forward * _carry, ForceMode.VelocityChange);
            rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.VelocityChange);
        }
    }
}
