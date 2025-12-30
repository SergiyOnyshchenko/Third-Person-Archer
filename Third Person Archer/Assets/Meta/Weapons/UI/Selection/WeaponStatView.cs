using System.Globalization;
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
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        private enum NumberFormatMode
        {
            Integer,        // "1"
            OneDecimal,     // "1.5"
            TwoDecimals,    // "1.25"
            Custom          // Use custom format string
        }

        private enum RoundingMode
        {
            None,
            Floor,
            Round,
            Ceil
        }

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _labelText;

        [Header("Current")]
        [SerializeField] private Slider _currentSlider;
        [SerializeField] private TextMeshProUGUI _currentValueText;

        [Header("Upgrade Preview")]
        [SerializeField] private GameObject _previewRoot;
        [SerializeField] private Slider _previewSlider;
        [SerializeField] private TextMeshProUGUI _previewValueText;

        [Header("Value Formatting")]
        [SerializeField] private NumberFormatMode _formatMode = NumberFormatMode.Integer;

        [Tooltip("Used only when Format Mode = Custom (e.g. \"0.##\", \"0.0\", \"0\").")]
        [SerializeField] private string _customFormat = "0.##";

        [Tooltip("Applied before formatting. Example: 100 for percent, 1 for meters, etc.")]
        [SerializeField] private float _displayMultiplier = 1f;

        [Tooltip("Optional rounding AFTER multiplier and BEFORE formatting.")]
        [SerializeField] private RoundingMode _rounding = RoundingMode.None;

        [Tooltip("Text added before the number (e.g. \"+\", \"≈\").")]
        [SerializeField] private string _prefix = "";

        [Tooltip("Text added after the number (e.g. \"m\", \"x\", \"s\").")]
        [SerializeField] private string _suffix = "";

        [Tooltip("If true and suffix is not empty, adds a space before suffix: \"12 m\" instead of \"12m\".")]
        [SerializeField] private bool _spaceBeforeSuffix = false;

        /// <summary>
        /// Optional: set static label (e.g. "Damage", "Reload"). Override per stat from code if needed.
        /// </summary>
        public void SetLabel(string label)
        {
            if (_labelText != null)
                _labelText.text = label;
        }

        /// <summary>
        /// Sets current stat UI (slider + text).
        /// min/max define slider range for this stat.
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
                _currentValueText.text = FormatValue(value);
        }

        /// <summary>
        /// Shows or hides upgrade preview.
        /// If show == false, preview root is hidden.
        /// If show == true, we display current -> next values.
        /// </summary>
        public void SetPreview(float current, float next, bool show)
        {
            if (_previewRoot != null)
                _previewRoot.SetActive(false);

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
                _previewValueText.text = $"{FormatValue(current)} → {FormatValue(next)}";
        }

        private string FormatValue(float raw)
        {
            float v = raw * _displayMultiplier;

            v = _rounding switch
            {
                RoundingMode.Floor => Mathf.Floor(v),
                RoundingMode.Round => Mathf.Round(v),
                RoundingMode.Ceil  => Mathf.Ceil(v),
                _ => v
            };

            string number = _formatMode switch
            {
                NumberFormatMode.Integer    => ((int)v).ToString(_culture),
                NumberFormatMode.OneDecimal => v.ToString("0.0", _culture),
                NumberFormatMode.TwoDecimals=> v.ToString("0.00", _culture),
                NumberFormatMode.Custom     => v.ToString(string.IsNullOrWhiteSpace(_customFormat) ? "0.##" : _customFormat, _culture),
                _ => v.ToString(_culture)
            };

            if (!string.IsNullOrEmpty(_suffix) && _spaceBeforeSuffix)
                return $"{_prefix}{number} {_suffix}";

            return $"{_prefix}{number}{_suffix}";
        }
    }
}