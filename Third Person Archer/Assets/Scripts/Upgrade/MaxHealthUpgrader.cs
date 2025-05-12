using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;

public class MaxHealthUpgrader : Upgrader
{
    [SerializeField] private MaxHealth _maxHealth;

    private void Awake()
    {
        if(_maxHealth == null)
            _maxHealth = GetComponent<MaxHealth>();
    }

    protected override void Upgrade()
    {
        _maxHealth.SetValue(_upgradeData.FullValue);
    }
}
