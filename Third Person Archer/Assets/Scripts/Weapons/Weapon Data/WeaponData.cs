using System.Collections;
using System.Collections.Generic;
using Actor;
using Playgama;
using UnityEngine;

public enum WeaponState
{
    Locked = 0,
    Unlocked = 1,
    Equipped = 2
}

[SerializeField]
public abstract class WeaponData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [Space]
    [SerializeField] private WeaponState _state;
    [Space]
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _reloadSpeedMult = 1f;
    [Space]
    [SerializeField] private WeaponSkinData _skinData;
    private string _saveStateName = "weapon_data_state";
    public string ID { get => _id; }
    public string Name { get => _name; }
    public int Damage { get => _damage; }
    public float ReloadSpeedMult { get => _reloadSpeedMult; }
    public WeaponSkinData SkinData { get => _skinData;}
    public abstract WeaponType Type { get; }
    public WeaponState State { get => _state; }
    public abstract void Equip(ActorController actor);
    public abstract void Unequip(ActorController actor);

    public void Save()
    {
        Bridge.storage.Set(_saveStateName + _id, ((int)_state).ToString(), null);
    }

    public void Load()
    {
        Bridge.storage.Get(_saveStateName + _id, (success, value) =>
        {
            if (success && int.TryParse(value, out int result))
            {
                _state = (WeaponState)result;
            }
        });
    }

    public void Unlock()
    {
        _state = WeaponState.Unlocked;
        Save();
    }

    public void Equip(bool value)
    {
        if (value)
        {
            _state = WeaponState.Equipped;
        }
        else
        {
            _state = WeaponState.Unlocked;
        }
        
        Save();
    }
}

