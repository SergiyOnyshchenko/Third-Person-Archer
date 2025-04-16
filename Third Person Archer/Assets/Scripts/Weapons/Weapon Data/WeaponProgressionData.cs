using System.Collections;
using System.Collections.Generic;
using Playgama;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class WeaponProgressionInstance
{
    [SerializeField] private WeaponData _weaponData;
    [SerializeField] private int _levelsToComplete = 3;
    public WeaponData WeaponData { get => _weaponData; }
    public int LevelsToComplete { get => _levelsToComplete; }
}

[CreateAssetMenu(fileName = "WeaponProgression", menuName = "Data/Weapon/Progression")]
public class WeaponProgressionData : ScriptableObject
{
    [SerializeField] private WeaponProgressionInstance[] _instances;
    [SerializeField] private int _progressionIndex;
    [SerializeField] private float _currentProgress = 0f;

    private string _saveWeaponsProgressName = "ProgressionWeaponsProgress";
    private string _saveLocalProgressName = "ProgressionWeaponLocalProgress";

    public float CurrentProgress { get => _currentProgress; }

    private void Awake()
    {
        Load(null);
    }

    public void IncreaseProgression(UnityAction<WeaponData, float> onWeaponUnlocked)
    {
        if (!TryGetCurrentInstance(out WeaponProgressionInstance instance))
            return;

        float progressPerLevel = 1f / instance.LevelsToComplete;
        _currentProgress += progressPerLevel;

        onWeaponUnlocked?.Invoke(instance.WeaponData, _currentProgress);

        if (_currentProgress >= 1f)
        {
            UnlockNextWeapon();
        }

        Save();
    }

    private void UnlockNextWeapon()
    {
        if (TryGetCurrentInstance(out WeaponProgressionInstance instance))
        {
            WeaponData unlockedWeapon = instance.WeaponData;
            unlockedWeapon.Unlock();

            _progressionIndex ++;
            _currentProgress = 0f;

            if (AppMetricaEventReporter.Instance != null)
                AppMetricaEventReporter.Instance.SendWeaponUnlock(unlockedWeapon);
        }
    }

    public bool TryGetCurrentInstance(out WeaponProgressionInstance instance)
    {
        instance = null;

        if (_progressionIndex >= _instances.Length)
            return false;

        instance = _instances[_progressionIndex];
        return true;
    }

    public void Load(UnityAction onLoaded)
    {
        Bridge.storage.Get(_saveWeaponsProgressName, (success1, value1) =>
        {
            if (success1 && int.TryParse(value1, out var progressIndex))
            {
                _progressionIndex = progressIndex;
            }
            else
            {
                _progressionIndex = 0;
            }

            Bridge.storage.Get(_saveLocalProgressName, (success2, value2) =>
            {
                if (success2 && float.TryParse(value2, out var localProgress))
                {
                    _currentProgress = localProgress;
                }
                else
                {
                    _currentProgress = 0f;
                }

                onLoaded?.Invoke();
            });
        });
    }

    public void Save()
    {
        Bridge.storage.Set(_saveWeaponsProgressName, _progressionIndex.ToString(), null);
        Bridge.storage.Set(_saveLocalProgressName, _currentProgress.ToString(), null);
    }
}