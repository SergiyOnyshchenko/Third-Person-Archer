using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;


public abstract class WeaponData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [Space]
    [SerializeField] private WeaponSkinData _skinData;
    public string ID { get => _id; }
    public string Name { get => _name; }
    public WeaponSkinData SkinData { get => _skinData;}
    public abstract WeaponType Type { get; }
    public abstract void Equip(ActorController actor);
    public abstract void Unequip(ActorController actor);
}

