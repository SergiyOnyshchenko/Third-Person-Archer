using System.Collections;
using System.Collections.Generic;
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
        Load();
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

    public void Load()
    {
        _progressionIndex = PlayerPrefs.GetInt(_saveWeaponsProgressName, 0);
        _currentProgress = PlayerPrefs.GetFloat(_saveLocalProgressName, 0);
    }

    public void Save()
    {
        PlayerPrefs.SetInt(_saveWeaponsProgressName, _progressionIndex);
        PlayerPrefs.SetFloat(_saveLocalProgressName, _currentProgress);
    }
}