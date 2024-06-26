using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "BowData", menuName = "Data/Weapon/Bow")]
public class BowData : ProjectileWeaponData
{
    public override WeaponType Type => WeaponType.Bow;

    public override void Equip(ActorController actor)
    {
        BowSkinData bowSkinData = (BowSkinData) SkinData;

        if (actor.TryGetSystem(out BowController bowController))
        {
            if (bowController.TryGetComponent(out ProjectileShooter shooter))
                shooter.SetProjectile(Projectile);
        }

        if (actor.TryGetPropertys(out WeaponHolder[] holders))
        {
            foreach (var holder in holders)
            {
                if (holder is BowHolder)
                {
                    if (holder.Pov == PovType.ThirdPerson)
                    {
                        holder.SetWeapon(bowSkinData.WeaponModel.Value);
                    }
                    else
                    {
                        if (actor.TryGetSystem(out BowViewController bowViewController))
                        {
                            bowViewController.SetView(bowSkinData.FpvIndex.Value);
                        }
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
