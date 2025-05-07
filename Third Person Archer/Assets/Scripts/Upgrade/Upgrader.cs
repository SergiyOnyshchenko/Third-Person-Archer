using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Upgrader : MonoBehaviour
{
    [SerializeField] protected UpgradeData _upgradeData;

    private void OnEnable()
    {
        _upgradeData.OnUpgraded.AddListener(Upgrade);
        Upgrade();
    }

    private void OnDisable()
    {
        _upgradeData.OnUpgraded.RemoveListener(Upgrade);
    }

    protected abstract void Upgrade();
}
