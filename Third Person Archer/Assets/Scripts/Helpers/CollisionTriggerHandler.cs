using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionTriggerHandler : MonoBehaviour
{
    // UnityEvents for collision and trigger events, with "Event" removed from names
    public UnityEvent<Collision> CollisionEnter = new UnityEvent<Collision>();
    public UnityEvent<Collision> CollisionExit = new UnityEvent<Collision>();
    public UnityEvent<Collider> TriggerEnter = new UnityEvent<Collider>();
    public UnityEvent<Collider> TriggerExit = new UnityEvent<Collider>();

    // Collision Enter
    void OnCollisionEnter(Collision collision)
    {
        CollisionEnter.Invoke(collision);
    }

    // Collision Exit
    void OnCollisionExit(Collision collision)
    {
        CollisionExit.Invoke(collision);
    }

    // Trigger Enter
    void OnTriggerEnter(Collider other)
    {
        TriggerEnter.Invoke(other);
    }

    // Trigger Exit
    void OnTriggerExit(Collider other)
    {
        TriggerExit.Invoke(other);
    }
}
