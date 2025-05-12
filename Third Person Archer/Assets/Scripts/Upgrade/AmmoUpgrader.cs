using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

public class AmmoUpgrader : Upgrader
{
    private IAmmoUpgrade _ammo;

    private void Awake()
    {
        _ammo = GetComponent<IAmmoUpgrade>();
    }

    protected override void Upgrade()
    {
        _ammo.SetMaxCount(_upgradeData.FullValue);
    }
}
