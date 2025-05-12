using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Data/Upgrade/Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Title")]
    [SerializeField] private string _title;
    [Header("Count")]
    [SerializeField] private int _upgradeCount;
    [SerializeField] private int _maxUpgradeCount;
    [Header("Value")]
    [SerializeField] private int _startValue;
    [SerializeField] private int _upgradeStep;
    [Header("Cost")]
    [SerializeField] private int _costStart;
    [SerializeField] private int _costStep;
    [Header("Save")]
    [SerializeField] private string _saveKey;
    [SerializeField] private bool _useSaves = true;

    public bool CanBeUpgraded => _upgradeCount < _maxUpgradeCount;
    public int UpgradeCost => GetUpgradeCost();
    public float UpgradeRatio => (float)_upgradeCount / (float)_maxUpgradeCount;
    public int CurrentStepValue => _upgradeStep;
    public int FullValue => _startValue + (_upgradeCount * _upgradeStep);
    public string Title => _title + ": " + FullValue + (CanBeUpgraded? " (+" + _upgradeStep + ")" : "");

    [HideInInspector] public UnityEvent OnUpgraded;

    private void OnEnable()
    {
        Load();
    }

    public void Upgrade()
    {
        if (!CanBeUpgraded)
            return;

        _upgradeCount++;
        OnUpgraded?.Invoke();

        Save();
    }

    private int GetUpgradeCost()
    {
        int cost = _costStart;

        for (int i = 0; i < _upgradeCount; i++)
        {
            cost += _costStep;
        }

        return cost;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(_saveKey, _upgradeCount);
    }

    private void Load()
    {
        if (_useSaves)
        {
            _upgradeCount = PlayerPrefs.GetInt(_saveKey);
        }
    }
}
