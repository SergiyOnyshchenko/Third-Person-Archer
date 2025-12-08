using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    public class LoadoutSlotView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _equippedBadge;

        private Action _onClick;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        public void SetData(Sprite icon, string weaponName, float damage, bool isEquipped, Action onClick)
        {
            if (_iconImage != null)
                _iconImage.sprite = icon;

            if (_nameText != null)
                _nameText.text = weaponName ?? "-";

            if (_damageText != null)
                _damageText.text = Mathf.RoundToInt(damage).ToString();

            if (_equippedBadge != null)
                _equippedBadge.SetActive(isEquipped);

            _onClick = onClick;
        }

        private void HandleClick()
        {
            _onClick?.Invoke();
        }
    }
}