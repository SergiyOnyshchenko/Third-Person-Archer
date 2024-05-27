using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using EventSystem = Actor.EventSystem;

public abstract class Shooter : MonoBehaviour
{
    [SerializeField] protected Transform _shootPoint;

    public abstract void Shoot(Vector3 direction, float multiplier, UnityAction onHited);
}
