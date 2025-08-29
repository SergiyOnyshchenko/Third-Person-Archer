using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Meta.Weapons.UI.Selection
{
    public class WeaponStatSliderView : MonoBehaviour
    {
        [Header("Label & Values")]
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text numericValueText;
        [SerializeField] private TMP_Text numericDeltaText;   

        [Header("Bars")]
        [SerializeField] private RectTransform barArea;
        [SerializeField] private Image primaryFillImage;       // current
        [SerializeField] private Image positiveDeltaFillImage; // green overlay
        [SerializeField] private Image negativeDeltaFillImage; // red overlay (unused for preview)
        [SerializeField] private Image equippedMarkerImage;

        private const float Epsilon = 0.0001f;

        public void SetLabel(string t) { if (labelText) labelText.text = t; }

        public void SetNumeric(string baseText, string deltaText, Color deltaColor)
        {
            if (numericValueText) numericValueText.text = baseText;
            if (numericDeltaText)
            {
                numericDeltaText.gameObject.SetActive(!string.IsNullOrEmpty(deltaText));
                numericDeltaText.text = deltaText;
                numericDeltaText.color = deltaColor;
            }
            else
            {
                // Fallback: append colored rich text to the single field
                if (numericValueText)
                    numericValueText.text = string.IsNullOrEmpty(deltaText)
                        ? baseText
                        : $"{baseText} <color=#{ColorUtility.ToHtmlStringRGB(deltaColor)}>{deltaText}</color>";
            }
        }

        /// <summary>
        /// Draw bars where primary = 'from' (current), overlay = the difference toward 'to'.
        /// If allowNegativeDelta is false we show only positive (green) overlay.
        /// </summary>
        public void SetBars(float from01, float to01, Color primary, Color green, Color red, bool allowNegativeDelta = true)
        {
            from01 = Mathf.Clamp01(from01);
            to01   = Mathf.Clamp01(to01);

            if (primaryFillImage)
            {
                primaryFillImage.color = primary;
                StretchX(primaryFillImage.rectTransform, 0f, from01);
                primaryFillImage.gameObject.SetActive(from01 > 0f);
            }

            float min = Mathf.Min(from01, to01);
            float max = Mathf.Max(from01, to01);
            bool equal = Mathf.Abs(from01 - to01) < Epsilon;

            if (equal)
            {
                ShowDelta(positiveDeltaFillImage, false, 0f, 0f, green);
                ShowDelta(negativeDeltaFillImage, false, 0f, 0f, red);
            }
            else
            {
                bool improved = to01 > from01;
                if (improved)
                {
                    ShowDelta(positiveDeltaFillImage, true,  min, max, green);
                    ShowDelta(negativeDeltaFillImage, false, 0f,  0f,  red);
                }
                else
                {
                    ShowDelta(positiveDeltaFillImage, false, 0f, 0f, green);
                    ShowDelta(negativeDeltaFillImage, allowNegativeDelta, min, max, red);
                }
            }

            if (equippedMarkerImage)
            {
                var rt = equippedMarkerImage.rectTransform;
                float w = 0.0025f;
                StretchX(rt, Mathf.Clamp01(to01 - w * 0.5f), Mathf.Clamp01(to01 + w * 0.5f));
                equippedMarkerImage.gameObject.SetActive(true);
            }
        }

        private void ShowDelta(Image img, bool visible, float from, float to, Color color)
        {
            if (!img) return;
            img.gameObject.SetActive(visible);
            if (!visible) return;
            img.color = color;
            StretchX(img.rectTransform, from, to);
        }

        private static void StretchX(RectTransform rect, float x0, float x1)
        {
            if (!rect) return;
            var aMin = rect.anchorMin; aMin.x = x0;
            var aMax = rect.anchorMax; aMax.x = x1;
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        }
    }
}