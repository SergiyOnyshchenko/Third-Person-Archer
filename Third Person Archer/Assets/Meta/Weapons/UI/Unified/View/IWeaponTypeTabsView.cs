using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI
{
    public interface IWeaponTypeTabsView
    {
        void SetVisible(bool visible);
        void BuildTabs(IReadOnlyList<WeaponTypeTabData> tabs, WeaponClass initiallySelected);
        void SelectTab(WeaponClass cls);
    }

    public readonly struct WeaponTypeTabData
    {
        public readonly WeaponClass Class;
        public readonly string Title;

        public WeaponTypeTabData(WeaponClass @class, string title)
        {
            Class = @class;
            Title = title;
        }
    }
}