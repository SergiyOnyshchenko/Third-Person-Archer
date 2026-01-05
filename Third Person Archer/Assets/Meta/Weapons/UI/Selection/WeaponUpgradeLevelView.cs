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
            if (_root != null)
                _root.SetActive(visible);

            if (!visible)
                return;

            currentUpgradeLevel = Mathf.Clamp(currentUpgradeLevel, 0, Mathf.Max(0, maxUpgradeLevel));
            maxUpgradeLevel     = Mathf.Max(0, maxUpgradeLevel);

            if (_levelText != null)
                _levelText.text = $"Lvl{currentUpgradeLevel + 1}";

            if (_levelSlider != null)
            {
                _levelSlider.wholeNumbers = true;
                _levelSlider.minValue = 0;
                _levelSlider.maxValue = maxUpgradeLevel;
                _levelSlider.value = currentUpgradeLevel;
            }
        }
    }
}