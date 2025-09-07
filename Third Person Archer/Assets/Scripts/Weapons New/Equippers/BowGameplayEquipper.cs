using Actor;
using Meta.Weapons;
using UnityEngine;
using Game.Weapons;
using Actor.Properties;

namespace Game.Weapons
{
    public sealed class BowGameplayEquipper : WeaponGameplayEquipper
    {
        public BowGameplayEquipper(IProjectileResolver p, IWeaponVisualResolver v) : base(p, v) { }
        protected override WeaponClass HandledClass => WeaponClass.Bow;

        protected override void ApplyProjectile(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out BowController bow) && bow.TryGetComponent(out ProjectileShooter shooter))
            {
                ConfigureShooter(shooter, def, stats);
            }
        }

        protected override void ApplyStats(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            if (actor.TryGetSystem(out BowController bow)) {
                bow.SetReloadDuration(stats.ReloadTime);
            }

            if (actor.TryGetProperty(out BowAmmo ammo)) {
                ammo.SetMaxCount(Mathf.RoundToInt(stats.AmmoCount));
            }

            ApplyZoomStat(actor, stats.Zoom);
        }

        protected override void ApplyVisuals(ActorController actor, WeaponDef def, WeaponStats stats)
        {
            var v = visuals.Resolve(def);
            var fpvId = string.IsNullOrEmpty(v.FirstPersonWeaponId) ? def.Id : v.FirstPersonWeaponId;

            ApplyHolders<BowHolder>(actor, v.ThirdPersonModel, fpv =>
            {
                fpv.GetComponentInChildren<BowFpvSkinView>(true)?.SetView(fpvId);
            });
        }
    }
}