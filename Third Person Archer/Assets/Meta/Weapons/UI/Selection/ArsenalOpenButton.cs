using System;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// Map "Weapons" button: opens the weapon screen directly (classes are tabs there).
    /// Starts on a class with something to buy, else the class of the current mission.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ArsenalOpenButton : MonoBehaviour
    {
        [SerializeField] private string _weaponScreenId = "weapons";

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Open);
        }

        private void Open()
        {
            if (!ServiceLocator.TryResolve<IUINavigator>(out var nav) || nav == null) return;
            nav.Open(_weaponScreenId, new WeaponSelectionArgs(PickClass(), ArsenalAttention.CompanyLevel), reuseCached: true);
        }

        private static WeaponClass PickClass()
        {
            var ctx = MainMenuRuntime.Instance != null ? MainMenuRuntime.Instance.Services?.Context?.BuildSelectedContext() : null;
            var current = ctx != null && ctx.IsValid ? ctx.RequiredWeaponClass : WeaponClass.Bow;

            if (ArsenalAttention.IsClassUnlocked(current) && ArsenalAttention.HasSomethingToBuy(current))
                return current;

            foreach (WeaponClass cls in Enum.GetValues(typeof(WeaponClass)))
                if (ArsenalAttention.IsClassUnlocked(cls) && ArsenalAttention.HasSomethingToBuy(cls))
                    return cls;

            return ArsenalAttention.IsClassUnlocked(current) ? current : WeaponClass.Bow;
        }
    }
}
