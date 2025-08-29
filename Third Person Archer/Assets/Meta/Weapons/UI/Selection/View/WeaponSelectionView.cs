using System;
using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    public class WeaponSelectionView : MonoBehaviour
    {
        [Header("3D Display")]
        [SerializeField] private WeaponDisplayController displayController;

        [Header("Dynamic Tabs")]
        [SerializeField] private Transform tabsContainer;
        [SerializeField] private WeaponTypeTabView tabPrefab;

        [Header("Weapon List")]
        [SerializeField] private Transform weaponListContainer;
        [SerializeField] private WeaponListItemView weaponListItemPrefab;

        [Header("Info Panel")]
        [SerializeField] private WeaponInfoPanelView infoPanelView;

        public WeaponDisplayController DisplayController => displayController;
        public WeaponInfoPanelView InfoPanelView => infoPanelView;

        public event Action<WeaponClass> OnTabSelected;
        public event Action<string> OnWeaponSelected;

        // Tabs
        public void ClearTabs()
        {
            for (int i = tabsContainer.childCount - 1; i >= 0; i--)
                Destroy(tabsContainer.GetChild(i).gameObject);
        }
        public WeaponTypeTabView CreateTab() => Instantiate(tabPrefab, tabsContainer);
        public void HookTab(WeaponTypeTabView view, WeaponClass weaponClass) => view.OnClick = () => OnTabSelected?.Invoke(weaponClass);

        // Weapon list
        public void ClearWeaponList()
        {
            for (int i = weaponListContainer.childCount - 1; i >= 0; i--)
                Destroy(weaponListContainer.GetChild(i).gameObject);
        }
        public WeaponListItemView CreateWeaponListItem() => Instantiate(weaponListItemPrefab, weaponListContainer);
        public void HookWeaponItem(WeaponListItemView view, string weaponId) => view.OnSelect = () => OnWeaponSelected?.Invoke(weaponId);
    }
}