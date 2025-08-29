using System;
using UnityEngine;
using Meta.Weapons.UI.Selection;

namespace Meta.Weapons.UI
{
    public class SelectionBottomBarView : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private Transform tabsContainer;
        [SerializeField] private WeaponTypeTabView tabPrefab;

        [Header("Weapon List")]
        [SerializeField] private Transform weaponListContainer;
        [SerializeField] private WeaponListItemView weaponListItemPrefab;

        public event Action<WeaponClass> OnTabSelected;
        public event Action<string> OnWeaponSelected;

        public void ClearTabs()
        {
            for (int i = tabsContainer.childCount - 1; i >= 0; i--)
                Destroy(tabsContainer.GetChild(i).gameObject);
        }
        public WeaponTypeTabView CreateTab() => Instantiate(tabPrefab, tabsContainer);
        public void HookTab(WeaponTypeTabView view, WeaponClass cls) => view.OnClick = () => OnTabSelected?.Invoke(cls);

        public void ClearWeaponList()
        {
            for (int i = weaponListContainer.childCount - 1; i >= 0; i--)
                Destroy(weaponListContainer.GetChild(i).gameObject);
        }
        public WeaponListItemView CreateItem() => Instantiate(weaponListItemPrefab, weaponListContainer);
        public void HookItem(WeaponListItemView view, string weaponId) => view.OnSelect = () => OnWeaponSelected?.Invoke(weaponId);
    }
}
