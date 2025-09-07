using UnityEngine;
using Meta.Weapons;

namespace Game.Weapons
{
    public interface IWeaponVisualResolver
    {
        Visuals Resolve(WeaponDef definition);

        public readonly struct Visuals
        {
            public readonly GameObject ThirdPersonModel;
            public readonly string FirstPersonWeaponId; // if null/empty, use definition.Id

            public Visuals(GameObject thirdPersonModel, string firstPersonWeaponId)
            {
                ThirdPersonModel = thirdPersonModel;
                FirstPersonWeaponId = firstPersonWeaponId;
            }
        }
    }
}
