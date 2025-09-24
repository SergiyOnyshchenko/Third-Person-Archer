using UnityEngine;
using Actor;
using Actor.Properties;
using Meta.Weapons;
using Game.Weapons; 

namespace Game.Weapons
{
    public class BoomerangGameplayEquipper : WeaponGameplayEquipper
    {
        public BoomerangGameplayEquipper(IProjectileResolver p, IWeaponVisualResolver v) : base(p, v) { }
        protected override WeaponClass HandledClass => WeaponClass.Boomerang;

        protected override void ApplyProjectile(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out BoomerangController controller) && controller.TryGetComponent(out ProjectileShooter shooter))
            {
                ConfigureShooter(shooter, def, stats);
            }
        }

        protected override void ApplyStats(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out BoomerangController controller)){
                controller.SetReloadDuration(stats.ReloadTime);
            }

            if (actor.TryGetProperty(out BoomerangAmmo ammo)) {
                ammo.SetMaxCount(Mathf.RoundToInt(stats.AmmoCount));
            }

            ApplyZoomStat(actor, stats.Zoom);
        }

        protected override void ApplyVisuals(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            var v = visuals.Resolve(def);
            var fpvId = string.IsNullOrEmpty(v.FirstPersonWeaponId) ? def.Id : v.FirstPersonWeaponId;

            ApplyHolders<BoomerangHolder>(actor, v.ThirdPersonModel, fpv =>
            {
                fpv.GetComponentInChildren<BoomerangFpvSkinView>(true)?.SetView(fpvId);
            });
        }
    }
}