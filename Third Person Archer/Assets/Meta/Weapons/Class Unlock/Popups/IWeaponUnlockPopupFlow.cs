#nullable enable

namespace Meta.Weapons
{
    public interface IWeaponUnlockPopupFlow
    {
        int CurrentCompanyLevel { get; }

        void CloseAndContinue(object popupInstance);
        void GoToWeaponClass(object popupInstance, WeaponClass weaponClass);
        void GoToWeapon(object popupInstance, string weaponId);
    }
}