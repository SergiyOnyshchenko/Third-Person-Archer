using System;
using UnityEngine;
using Meta.Weapons.UI.Selection; // reuse Display + InfoPanel + StatsPanel

namespace Meta.Weapons.UI.Upgrade
{
    public class WeaponUpgradeScreenView : MonoBehaviour
    {
        [Header("3D Display (rotation disabled by presenter)")]
        [SerializeField] private WeaponDisplayController displayController;

        [Header("Bottom Upgrades List")]
        [SerializeField] private Transform upgradesContainer;
        [SerializeField] private UpgradeOptionItemView upgradeItemPrefab;

        [Header("Info Panel (same as selection)")]
        [SerializeField] private WeaponInfoPanelView infoPanelView;

        [Header("Navigation")]
        [SerializeField] private UnityEngine.UI.Button returnToSelectionButton;

        public WeaponDisplayController Display => displayController;
        public WeaponInfoPanelView Info => infoPanelView;

        public event Action OnReturnToSelection;
        public event Action<string> OnUpgradeItemSelected; // partId

        public void ClearUpgradeList()
        {
            for (int i = upgradesContainer.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(upgradesContainer.GetChild(i).gameObject);
        }

        public UpgradeOptionItemView CreateItem() => UnityEngine.Object.Instantiate(upgradeItemPrefab, upgradesContainer);

        public void HookItem(UpgradeOptionItemView item, string partId)
        {
            item.OnSelect = () => OnUpgradeItemSelected?.Invoke(partId);
        }

        private void Awake()
        {
            if (returnToSelectionButton) returnToSelectionButton.onClick.AddListener(() => OnReturnToSelection?.Invoke());
        }
    }
}
