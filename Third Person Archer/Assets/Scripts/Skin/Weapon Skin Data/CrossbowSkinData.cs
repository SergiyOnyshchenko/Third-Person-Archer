using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

[CreateAssetMenu(fileName = "CrossbowSkinData", menuName = "Data/Skin/Crossbow")]
public class CrossbowSkinData : WeaponSkinData
{
    [field: SerializeField] public MaterialData FpvMaterial { get; private set; }

    public override void Apply(ActorController actor)
    {
        
    }
}
