using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;

public class MaxHealthUpgrader : Upgrader
{
    [SerializeField] private MaxHealth _maxHealth;
    [SerializeField] private int _upgradeStep = 25;

    private void Awake()
    {
        if(_maxHealth == null)
            _maxHealth = GetComponent<MaxHealth>();
    }

    protected override void Upgrade()
    {
        _maxHealth.SetValue(_maxHealth.BaseValue + (_upgradeStep * _upgradeData.UpgradeValue));
    }
}
