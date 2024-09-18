using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

[CreateAssetMenu(fileName = "SpearSkinData", menuName = "Data/Skin/Spear")]
public class SpearSkinData : WeaponSkinData
{
    [field: SerializeField] public IndexData FpvIndex { get; private set; }

    public override void Apply(ActorController actor)
    {
        if (actor.TryGetSystem(out SpearViewController spearSkinController))
            spearSkinController.SetView(FpvIndex.Value);
    }
}
