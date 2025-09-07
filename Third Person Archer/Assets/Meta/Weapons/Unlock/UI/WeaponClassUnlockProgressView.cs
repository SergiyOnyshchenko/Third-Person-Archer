// Assets/Scripts/Meta/Weapons/Unlocks/UI/WeaponClassUnlockProgressView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if MOREMOUNTAINS_FEEL
using MoreMountains.Feedbacks;
#endif

namespace Meta.Weapons.Unlocks.UI
{
    /// <summary>Popup that shows class icon/name and animated progress (before→after). No logic here.</summary>
    public sealed class WeaponClassUnlockProgressView : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image classIcon;
        [SerializeField] private TMP_Text classTitle;
        [SerializeField] private Slider progressBar;             // 0..1
        [SerializeField] private TMP_Text progressLabel;         // "2/3"
        [SerializeField] private Image progressFill;             // optional; if set, colored by config
        [SerializeField] private GameObject unlockedBadge;       // shown when unlocked

#if MOREMOUNTAINS_FEEL
        [Header("Optional Feedback")]
        [SerializeField] private MMF_Player tickFeedback;        // each step / gain
        [SerializeField] private MMF_Player unlockedFeedback;    // when unlocked
#endif

        public void SetHeader(Sprite icon, string title, Color accent, Color fillColor)
        {
            if (classIcon != null) classIcon.sprite = icon;
            if (classTitle != null) classTitle.text = title;
            if (progressFill != null) progressFill.color = fillColor;
        }

        public void SetProgressInstant(int current, int required, bool unlocked)
        {
            float pct = required > 0 ? Mathf.Clamp01((float)current / required) : 1f;
            if (progressBar != null) progressBar.value = pct;
            if (progressLabel != null) progressLabel.text = $"{current}/{required}";
            if (unlockedBadge != null) unlockedBadge.SetActive(unlocked);
        }

        public void SetVisible(bool visible)
        {
            if (canvasGroup == null) { gameObject.SetActive(visible); return; }
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            gameObject.SetActive(visible); // keep simple
        }

        public void PlayTick()
        {
#if MOREMOUNTAINS_FEEL
            tickFeedback?.PlayFeedbacks();
#endif
        }

        public void PlayUnlocked()
        {
#if MOREMOUNTAINS_FEEL
            unlockedFeedback?.PlayFeedbacks();
#endif
        }

        // Smooth bar update over t seconds
        public System.Collections.IEnumerator AnimateProgress(int fromCur, int toCur, int required, float seconds)
        {
            if (required <= 0) required = 1;
            float start = Mathf.Clamp01((float)fromCur / required);
            float end   = Mathf.Clamp01((float)toCur   / required);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, seconds);
                float v = Mathf.SmoothStep(start, end, t);
                if (progressBar != null) progressBar.value = v;

                int shown = Mathf.RoundToInt(Mathf.Lerp(fromCur, toCur, t));
                if (progressLabel != null) progressLabel.text = $"{shown}/{required}";

                yield return null;
            }

            // snap
            if (progressBar != null) progressBar.value = end;
            if (progressLabel != null) progressLabel.text = $"{toCur}/{required}";
        }
    }
}

