using System;
using UnityEngine;
using Meta.Weapons.UI.Upgrade;

namespace Meta.Weapons.UI
{
    public class UpgradeBottomBarView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private UpgradeOptionItemView itemPrefab;

        public event Action<string> OnItemSelected; // partId

        public void Clear()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);
        }

        public UpgradeOptionItemView CreateItem()
        {
            return Instantiate(itemPrefab, content);
        }

        public void Hook(UpgradeOptionItemView item, string partId)
        {
            item.OnSelect = () => OnItemSelected?.Invoke(partId);
        }
    }
}
