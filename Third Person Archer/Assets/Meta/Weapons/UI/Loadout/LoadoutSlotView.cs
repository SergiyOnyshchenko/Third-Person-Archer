using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    public class LoadoutSlotView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _damageText;

        [Header("State")]
        [SerializeField] private GameObject _lockedRoot;
        [SerializeField] private GameObject _damageRoot;

        [Header("Interaction")]
        [SerializeField] private Button _button;

        private Action _onClick;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
        }

        public void SetData(
            Sprite icon,
            string weaponName,
            float damage,
            bool interactable,
            Action onClick)
        {
            /*
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }
            */

            if (_nameText != null)
                _nameText.text = weaponName;

            if (_damageText != null)
                _damageText.text = Mathf.RoundToInt(damage).ToString();

            _onClick = interactable ? onClick : null;

            if (_button != null)
                _button.interactable = interactable;
        }

        public void SetLocked(bool locked)
        {
            if (_lockedRoot != null)
                _lockedRoot.SetActive(locked);

            if(_damageRoot != null)
                _damageRoot.SetActive(!locked);

            if (_button != null)
                _button.interactable = !locked;

            if (locked)
                _onClick = null;
        }

        private void HandleClick()
        {
            _onClick?.Invoke();
        }
    }
}