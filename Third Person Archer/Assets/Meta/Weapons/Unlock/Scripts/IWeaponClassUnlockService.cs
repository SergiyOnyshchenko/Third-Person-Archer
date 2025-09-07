// Assets/Scripts/Meta/Weapons/Unlocks/IWeaponClassUnlockService.cs
using System.Collections.Generic;

namespace Meta.Weapons.Unlocks
{
    public readonly struct ClassProgress
    {
        public readonly WeaponClass Class;
        public readonly int Current;
        public readonly int Required;
        public readonly bool Unlocked;
        public ClassProgress(WeaponClass cls, int cur, int req, bool un) { Class = cls; Current = cur; Required = req; Unlocked = un; }
    }

    public readonly struct ClassProgressDelta
    {
        public readonly ClassProgress Before;
        public readonly ClassProgress After;
        public bool NewlyUnlocked => !Before.Unlocked && After.Unlocked;
        public ClassProgressDelta(ClassProgress before, ClassProgress after) { Before = before; After = after; }
    }

    public interface IWeaponClassUnlockService
    {
        bool IsUnlocked(WeaponClass cls);
        ClassProgress GetProgress(WeaponClass cls);
        IReadOnlyList<ClassProgressDelta> RegisterMissionComplete(string missionId);
        WeaponClass? GetNextLockedClass(); // in configured order
    }
}