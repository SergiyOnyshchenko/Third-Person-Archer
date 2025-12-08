using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// Reusable view for a single weapon stat.
    /// Handles current value and optional "next upgrade" preview.
    /// </summary>
    public class WeaponStatView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _labelText;

        [Header("Current")]
        [SerializeField] private Slider _currentSlider;
        [SerializeField] private TextMeshProUGUI _currentValueText;

        [Header("Upgrade Preview")]
        [SerializeField] private GameObject _previewRoot;
        [SerializeField] private Slider _previewSlider;
        [SerializeField] private TextMeshProUGUI _previewValueText;

        /// <summary>
        /// Optional: set static label (e.g. "Damage", "Reload").
        /// </summary>
        public void SetLabel(string label)
        {
            if (_labelText != null)
                _labelText.text = label;
        }

        /// <summary>
        /// Sets current stat UI (slider + text).
        /// min/max define slider range for this stat (usually base->max).
        /// </summary>
        public void SetCurrent(float value, float min, float max)
        {
            if (_currentSlider != null)
            {
                _currentSlider.minValue = min;
                _currentSlider.maxValue = max;
                _currentSlider.value    = Mathf.Clamp(value, min, max);
            }

            if (_currentValueText != null)
                _currentValueText.text = Mathf.RoundToInt(value).ToString();
        }

        /// <summary>
        /// Shows or hides upgrade preview.
        /// If show == false, preview root is hidden.
        /// If show == true, we display current -> next values.
        /// </summary>
        public void SetPreview(float current, float next, bool show)
        {
            if (_previewRoot != null)
                _previewRoot.SetActive(show);

            if (!show)
                return;

            float min = Mathf.Min(current, next);
            float max = Mathf.Max(current, next);

            if (_previewSlider != null)
            {
                // Range is between current and next (simple and readable).
                _previewSlider.minValue = min;
                _previewSlider.maxValue = max;
                _previewSlider.value    = next;
            }

            if (_previewValueText != null)
            {
                int cur = Mathf.RoundToInt(current);
                int nxt = Mathf.RoundToInt(next);
                _previewValueText.text = $"{cur} → {nxt}";
            }
        }
    }
}