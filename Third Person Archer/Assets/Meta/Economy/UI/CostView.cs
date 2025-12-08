using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    public class CostView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _valueText;

        public void SetMoney(Sprite icon, int required)
        {
            if (_iconImage != null)
                _iconImage.sprite = icon;

            if (_valueText != null)
                _valueText.text = required.ToString();
        }

        /// <summary>
        /// Token display: "required / owned".
        /// </summary>
        public void SetToken(Sprite icon, int required, int owned)
        {
            if (_iconImage != null)
                _iconImage.sprite = icon;

            if (_valueText != null)
                _valueText.text = $"{required}/{owned}";
        }

        public void Clear()
        {
            if (_valueText != null)
                _valueText.text = string.Empty;
        }
    }
}