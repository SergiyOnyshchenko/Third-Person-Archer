using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "SpearData", menuName = "Data/Weapon/Spear")]
public class SpearData : ProjectileWeaponData
{
    public override WeaponType Type => WeaponType.Spear;
    public override void Equip(ActorController actor)
    {
        SpearSkinData bowSkinData = (SpearSkinData)SkinData;

        if (actor.TryGetSystem(out SpearController bowController))
        {
            if (bowController.TryGetComponent(out ProjectileShooter shooter))
                shooter.SetProjectile(Projectile);
        }

        if (actor.TryGetPropertys(out WeaponHolder[] holders))
        {
            foreach (var holder in holders)
            {
                if (holder is SpearHolder)
                {
                    holder.SetWeapon(bowSkinData.WeaponModel.Value);
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
