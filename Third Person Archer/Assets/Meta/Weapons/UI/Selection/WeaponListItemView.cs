using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    public class WeaponListItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _damageText;

        [Header("State Roots")]
        [SerializeField] private GameObject _lockedRoot;
        [SerializeField] private GameObject _equippedBadgeRoot;
        [SerializeField] private GameObject _selectedRoot;

        [Header("Icons")]
        [SerializeField] private Image _classIconImage;

        [Header("Interaction")]
        [SerializeField] private Button _button;

        private Action _onClick;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// Set data for this list item.
        /// </summary>
        public void SetData(
            string weaponName,
            float damage,
            bool isLocked,
            bool isEquipped,
            Sprite classIcon,
            Action onClick)
        {
            if (_nameText != null)
                _nameText.text = weaponName;

            if (_damageText != null)
                _damageText.text = Mathf.RoundToInt(damage).ToString();

            if (_lockedRoot != null)
                _lockedRoot.SetActive(isLocked);

            if (_equippedBadgeRoot != null)
                _equippedBadgeRoot.SetActive(isEquipped);

            if (_classIconImage != null)
                _classIconImage.sprite = classIcon;

            if (_selectedRoot != null)
                _selectedRoot.SetActive(false); // default: not selected

            _onClick = onClick;
        }

        /// <summary>
        /// Visual highlight for current selection.
        /// Only toggles a dedicated GameObject.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_selectedRoot != null)
                _selectedRoot.SetActive(selected);
        }

        private void HandleClick()
        {
            _onClick?.Invoke();
        }
    }
}