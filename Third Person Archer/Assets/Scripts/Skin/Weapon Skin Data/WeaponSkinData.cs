using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.UI;

public abstract class WeaponSkinData : ScriptableObject
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public PrefabData WeaponModel { get; private set; }

    public abstract void Apply(ActorController actor);
}
