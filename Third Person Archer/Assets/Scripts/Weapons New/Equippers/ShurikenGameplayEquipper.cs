using UnityEngine;
using Actor;
using Actor.Properties;
using Meta.Weapons;
using Game.Weapons; 

namespace Game.Weapons
{
    public class ShurikenGameplayEquipper : WeaponGameplayEquipper
    {
        public ShurikenGameplayEquipper(IProjectileResolver p, IWeaponVisualResolver v) : base(p, v) { }
        protected override WeaponClass HandledClass => WeaponClass.Shuriken;

        protected override void ApplyProjectile(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out ShurikenController spear) && spear.TryGetComponent(out ProjectileShooter shooter))
            {
                ConfigureShooter(shooter, def, stats);
            }
        }

        protected override void ApplyStats(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out ShurikenController controller))
            {
                controller.SetReloadDuration(stats.ReloadTime);
            }

            if (actor.TryGetProperty(out ShurikenAmmo ammo)) {
                ammo.SetMaxCount(Mathf.RoundToInt(stats.AmmoCount));
            }

            ApplyZoomStat(actor, stats.Zoom);
        }

        protected override void ApplyVisuals(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            var v = visuals.Resolve(def);
            var fpvId = string.IsNullOrEmpty(v.FirstPersonWeaponId) ? def.Id : v.FirstPersonWeaponId;

            ApplyHolders<ShurikenHolder>(actor, v.ThirdPersonModel, fpv =>
            {
                fpv.GetComponentInChildren<ShurikenFpvSkinView>(true)?.SetView(fpvId);
            });
        }
    }
}