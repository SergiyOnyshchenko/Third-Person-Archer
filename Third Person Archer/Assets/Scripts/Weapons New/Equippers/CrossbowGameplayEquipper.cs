using UnityEngine;
using Meta.Weapons;
using Game.Weapons;
using Actor;
using Actor.Properties;

namespace Game.Weapons
{
    public sealed class CrossbowGameplayEquipper : WeaponGameplayEquipper
    {
        public CrossbowGameplayEquipper(IProjectileResolver p, IWeaponVisualResolver v) : base(p, v) { }
        protected override WeaponClass HandledClass => WeaponClass.Crossbow;

        protected override void ApplyProjectile(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out CrossbowController xbow) && xbow.TryGetComponent(out ProjectileShooter shooter))
            {
                ConfigureShooter(shooter, def, stats);
            }
        }

        protected override void ApplyStats(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out CrossbowController xbow)){
                xbow.SetReloadDuration(stats.ReloadTime);
            }

            if (actor.TryGetProperty(out CrossbowAmmo ammo)) {
                ammo.SetMaxCount(Mathf.RoundToInt(stats.AmmoCount));
            }

            ApplyZoomStat(actor, stats.Zoom);
        }

        protected override void ApplyVisuals(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            var v = visuals.Resolve(def);
            var fpvId = string.IsNullOrEmpty(v.FirstPersonWeaponId) ? def.Id : v.FirstPersonWeaponId;

            ApplyHolders<CrossbowHolder>(actor, v.ThirdPersonModel, fpv =>
            {
                fpv.GetComponentInChildren<CrossbowFpvSkinView>(true)?.SetView(fpvId);
            });
        }
    }
}