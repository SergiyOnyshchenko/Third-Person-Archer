using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;
using Meta.Weapons;

public class WeaponBalance : Property
{
    [SerializeField] private float _minSpeed = 0.75f;
    [SerializeField] private float _maxSpeed = 5f;
    private float _speed = 1;
    public float Speed => _speed; 

    public void Initialize(WeaponStats stats)
    {
        float balance = stats.Balance / 100f;
        _speed = Mathf.Lerp(_minSpeed, _maxSpeed, balance);
    }
}