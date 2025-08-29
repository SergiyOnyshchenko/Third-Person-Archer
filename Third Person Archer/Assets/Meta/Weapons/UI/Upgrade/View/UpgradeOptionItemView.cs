using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Meta.Weapons.UI.Upgrade
{
    public class UpgradeOptionItemView : MonoBehaviour
    {
        [Header("Basics")]
        [SerializeField] private TMP_Text partNameText;
        [SerializeField] private TMP_Text levelText; 
        [SerializeField] private Image selectionHighlightImage;

        [Header("Progress / Timer")]
        [SerializeField] private Slider levelProgressSlider;   // optional visual, 0..1
        [SerializeField] private TMP_Text buildTimerText;      // "03:12"

        [Header("State Badges")]
        [SerializeField] private GameObject inProgressBadgeRoot;
        [SerializeField] private GameObject maxedBadgeRoot;

        public System.Action OnSelect;

        public void SetBase(string displayName, int currentLevel, int maxLevel)
        {
            if (partNameText) partNameText.text = displayName;
            if (levelText) levelText.text = $"Lv {currentLevel}/{maxLevel}";
            if (levelProgressSlider) levelProgressSlider.value = Mathf.Clamp01(maxLevel > 0 ? (float)currentLevel / maxLevel : 0f);
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlightImage) selectionHighlightImage.enabled = selected;
        }

        public void SetTimer(string text, bool visible)
        {
            if (buildTimerText)
            {
                buildTimerText.gameObject.SetActive(visible);
                buildTimerText.text = text;
            }
        }

        public void SetBadges(bool inProgress, bool maxed)
        {
            if (inProgressBadgeRoot) inProgressBadgeRoot.SetActive(inProgress);
            if (maxedBadgeRoot) maxedBadgeRoot.SetActive(maxed);
        }

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button) button.onClick.AddListener(() => OnSelect?.Invoke());
        }
    }
}