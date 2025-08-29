using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Meta.Weapons.UI.Selection
{
    public class WeaponListItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private GameObject lockOverlayRoot;
        [SerializeField] private GameObject equippedBadgeRoot;
        [SerializeField] private Button selectButton;

        public System.Action OnSelect;

        public void Bind(Sprite icon, string displayName, bool owned, bool equipped)
        {
            if (iconImage) iconImage.sprite = icon;
            if (nameText)  nameText.text = displayName;
            if (lockOverlayRoot) lockOverlayRoot.SetActive(!owned);
            if (equippedBadgeRoot) equippedBadgeRoot.SetActive(equipped);
        }

        private void Awake()
        {
            if (selectButton) selectButton.onClick.AddListener(() => OnSelect?.Invoke());
        }
    }
}