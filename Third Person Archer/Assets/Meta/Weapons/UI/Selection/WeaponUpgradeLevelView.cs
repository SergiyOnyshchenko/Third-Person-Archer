using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    public sealed class WeaponUpgradeLevelView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Slider _levelSlider;

        /// <summary>
        /// currentUpgradeLevel is expected to be in [0..maxUpgradeLevel]
        /// Text shows "Lvl{current+1}" (so upgrade level 0 => "Lvl1").
        /// </summary>
        public void SetLevel(int currentUpgradeLevel, int maxUpgradeLevel, bool visible)
        {
            SetLevel(currentUpgradeLevel, 0, maxUpgradeLevel, visible);
        }

        /// <summary>Class level inside the current tier: slider spans [minUpgradeLevel..maxUpgradeLevel].</summary>
        public void SetLevel(int currentUpgradeLevel, int minUpgradeLevel, int maxUpgradeLevel, bool visible)
        {
            if (_root != null)
                _root.SetActive(visible);

            if (!visible)
                return;

            maxUpgradeLevel     = Mathf.Max(0, maxUpgradeLevel);
            minUpgradeLevel     = Mathf.Clamp(minUpgradeLevel, 0, maxUpgradeLevel);
            currentUpgradeLevel = Mathf.Clamp(currentUpgradeLevel, minUpgradeLevel, maxUpgradeLevel);

            if (_levelText != null)
                _levelText.text = $"Lvl{currentUpgradeLevel + 1}";

            if (_levelSlider != null)
            {
                _levelSlider.wholeNumbers = true;
                _levelSlider.minValue = minUpgradeLevel;
                _levelSlider.maxValue = maxUpgradeLevel;
                _levelSlider.value = currentUpgradeLevel;
            }
        }
    }
}