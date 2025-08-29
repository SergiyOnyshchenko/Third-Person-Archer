using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Meta.Weapons.UI.Selection
{
    /// <summary>
    /// Handles "Equip" vs "Equipped" visuals and interactivity.
    /// </summary>
    public class EquipButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;

        [Header("Texts")]
        [SerializeField] private string equipText = "Equip";
        [SerializeField] private string equippedText = "Equipped";

        [Header("Colors")]
        [SerializeField] private Color equipBackgroundColor = new(0.15f, 0.55f, 1f); // blue
        [SerializeField] private Color equipTextColor = Color.white;
        [SerializeField] private Color equippedBackgroundColor = new(0.35f, 0.35f, 0.35f); // gray
        [SerializeField] private Color equippedTextColor = new(0.9f, 0.9f, 0.9f);

        public Action OnEquipClicked;

        private bool isEquipped;

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void SetStateEquipped(bool equipped)
        {
            isEquipped = equipped;

            if (labelText) labelText.text = equipped ? equippedText : equipText;

            if (backgroundImage) backgroundImage.color = equipped ? equippedBackgroundColor : equipBackgroundColor;
            if (labelText) labelText.color = equipped ? equippedTextColor : equipTextColor;

            if (button) button.interactable = !equipped;
        }

        private void Awake()
        {
            if (button) button.onClick.AddListener(() =>
            {
                if (!isEquipped) OnEquipClicked?.Invoke();
            });
        }
    }
}