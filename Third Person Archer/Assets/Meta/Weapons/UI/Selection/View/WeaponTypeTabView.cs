using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Meta.Weapons.UI.Selection
{
    public class WeaponTypeTabView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedHighlight;

        public System.Action OnClick;

        public void Set(string text, bool isSelected)
        {
            if (labelText) labelText.text = text;
            if (selectedHighlight) selectedHighlight.SetActive(isSelected);
        }

        private void Awake()
        {
            if (button) button.onClick.AddListener(() => OnClick?.Invoke());
        }
    }
}

