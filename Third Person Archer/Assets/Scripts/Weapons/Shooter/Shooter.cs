using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shooter : MonoBehaviour
{
    [SerializeField] protected Transform _shootPoint;

    public abstract void Shoot(Vector3 direction, float multiplier);
    public  void Shoot(Vector3 direction)
    {
        Shoot(direction, 1);
    }
}
