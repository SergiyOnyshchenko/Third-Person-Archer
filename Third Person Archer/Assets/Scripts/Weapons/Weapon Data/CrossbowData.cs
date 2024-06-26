using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "CrossbowData", menuName = "Data/Weapon/Crossbow")]
public class CrossbowData : ProjectileWeaponData
{
    public override WeaponType Type => WeaponType.Crossbow;
    public override void Equip(ActorController actor)
    {
        CrossbowSkinData crossbowSkinData = (CrossbowSkinData) SkinData;

        if (actor.TryGetSystem(out CrossbowController bowController))
        {
            if (bowController.TryGetComponent(out ProjectileShooter shooter))
                shooter.SetProjectile(Projectile);
        }

        if (actor.TryGetPropertys(out WeaponHolder[] holders))
        {
            foreach (var holder in holders)
            {
                if (holder is CrossbowHolder)
                {
                    if (holder.Pov == PovType.ThirdPerson)
                    {
                        holder.SetWeapon(crossbowSkinData.WeaponModel.Value);
                    }
                    else
                    {
                        MeshRenderer[] renderers = holder.Pivot.GetComponentsInChildren<MeshRenderer>();

                        foreach (var renderer in renderers)
                            renderer.material = crossbowSkinData.FpvMaterial.Value;
                    }

                    holder.ShowWeapon(true);
                }
                else
                {
                    holder.ShowWeapon(false);
                }
            }
        }
    }

    public override void Unequip(ActorController actor)
    {

    }
}
