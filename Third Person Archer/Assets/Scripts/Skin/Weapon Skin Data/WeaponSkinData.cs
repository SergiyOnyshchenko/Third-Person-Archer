using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public abstract class WeaponSkinData : ScriptableObject
{
    [field: SerializeField] public PrefabData WeaponModel { get; private set; }
    public abstract void Apply(ActorController actor);
}
