using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

[CreateAssetMenu(fileName = "BowSkinData", menuName = "Data/Skin/Bow")]
public class BowSkinData : WeaponSkinData
{
    [field: SerializeField] public IndexData FpvIndex { get; private set; }

    public override void Apply(ActorController actor)
    {
        if (actor.TryGetSystem(out BowViewController bowSkinController))
            bowSkinController.SetView(FpvIndex.Value);
    }
}
