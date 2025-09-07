using System;
using Actor;
using Actor.Properties;
using Meta.Weapons;// WeaponDef, WeaponStats, WeaponClass (matches your files)
using UnityEngine;
using Game.Weapons;// IGameplayEquipper, IProjectileResolver, IWeaponVisualResolver

namespace Game.Weapons
{
    public abstract class WeaponGameplayEquipper : IGameplayEquipper
    {
        protected readonly IProjectileResolver projectiles;
        protected readonly IWeaponVisualResolver visuals;

        protected WeaponGameplayEquipper(IProjectileResolver projectiles, IWeaponVisualResolver visuals)
        {
            this.projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
            this.visuals = visuals ?? throw new ArgumentNullException(nameof(visuals));
        }

        protected abstract WeaponClass HandledClass { get; }

        public bool TryEquip(ActorController actor, WeaponDef definition, WeaponStats stats)
        {
            if (definition == null || actor == null) return false;
            if (definition.Class != HandledClass) return false;

            // 1) Projectile (spawn/configure on shooter)
            ApplyProjectile(actor, definition, stats);

            // 2) Stats (reload, handling, etc.)
            ApplyStats(actor, definition, stats);

            // 3) Visuals (TPV holder model + FPV skin/material)
            ApplyVisuals(actor, definition, stats);

            // 4) Optional: push aggregate stats into a bridge (if present)
            //actor.GetComponentInChildren<ActorWeaponStatsBridge>(true)
            //     ?.Apply(definition, stats);

            if (actor.TryGetProperty(out EquippedWeaponDef equipped))
            {
                equipped.SetValue(definition);
                Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA " + definition.name);
            }

            return true;
        }

        // ---------- Steps to implement/override ----------

        protected abstract void ApplyProjectile(ActorController actor, WeaponDef def, WeaponStats stats);
        protected abstract void ApplyStats(ActorController actor, WeaponDef def, WeaponStats stats);
        protected abstract void ApplyVisuals(ActorController actor, WeaponDef def, WeaponStats stats);

        // ---------- Common helpers ----------

        protected void ConfigureShooter(ProjectileShooter shooter, WeaponDef def, WeaponStats stats)
        {
            if (shooter == null) return;
            var proj = projectiles.ResolveProjectile(def);
            if (proj == null) return;

            proj.SetDamage(Mathf.RoundToInt(stats.Damage));
            proj.SetRange(stats.Distance);
            
            shooter.SetProjectile(proj);
        }

        /// <summary>Generic holders application by type. Shows target holders, hides others.</summary>
        protected void ApplyHolders<TSpecificHolder>(
            ActorController actor,
            GameObject thirdPersonModel,
            Action<TSpecificHolder> applyFirstPerson = null)
            where TSpecificHolder : WeaponHolder
        {
            if (!actor.TryGetPropertys(out WeaponHolder[] holders) || holders == null) return;

            foreach (var holder in holders)
            {
                if (holder is TSpecificHolder specific)
                {
                    if (holder.Pov == PovType.ThirdPerson && thirdPersonModel != null)
                        holder.SetWeapon(thirdPersonModel);

                    if (holder.Pov == PovType.FirstPerson)
                        applyFirstPerson?.Invoke(specific);

                    holder.ShowWeapon(true);
                }
                else
                {
                    holder.ShowWeapon(false);
                }
            }
        }

        protected void ApplyZoomStat(ActorController actor, float value)
        {
            if (actor.TryGetProperty(out ZoomMagnification zoomMagnification)) {
                zoomMagnification.SetValue(value);
            }
        }
    }
}