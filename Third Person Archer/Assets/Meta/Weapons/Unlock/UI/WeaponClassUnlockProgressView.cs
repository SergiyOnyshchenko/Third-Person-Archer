using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if MOREMOUNTAINS_FEEL
using MoreMountains.Feedbacks;
#endif

namespace Meta.Weapons.Unlocks.UI
{
    public sealed class WeaponClassUnlockProgressView : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image classIcon;
        [SerializeField] private TMP_Text classTitle;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Image progressFill;
        [SerializeField] private GameObject unlockedBadge;

#if MOREMOUNTAINS_FEEL
        [Header("Optional Feedback")]
        [SerializeField] private MMF_Player tickFeedback;
        [SerializeField] private MMF_Player unlockedFeedback;
#endif

        private void Awake()
        {
            if (progressBar != null)
            {
                progressBar.minValue = 0f;
                progressBar.maxValue = 1f;
                progressBar.wholeNumbers = false;
            }
            SetVisible(false);
        }

        public void SetHeader(Sprite icon, string title, Color accent, Color fillColor)
        {
            if (classIcon != null) classIcon.sprite = icon;
            if (classTitle != null) classTitle.text = title;
            if (progressFill != null) progressFill.color = fillColor;
        }

        public void SetProgressInstant(int current, int required, bool unlocked)
        {
            if (required <= 0) required = 1;
            float pct = Mathf.Clamp01((float)current / required);
            if (progressBar != null) progressBar.value = pct;
            if (progressLabel != null) progressLabel.text = $"{current}/{required}";
            if (unlockedBadge != null) unlockedBadge.SetActive(unlocked);
        }

        public void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            // Important: do NOT deactivate the GameObject here, as it may also
            // host the presenter component running coroutines. Deactivating it
            // would kill those coroutines and could leave the state machine stuck.
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

        public System.Collections.IEnumerator AnimateProgress(int fromCur, int toCur, int required, float seconds)
        {
            if (required <= 0) required = 1;

            float start = Mathf.Clamp01((float)fromCur / required);
            float end   = Mathf.Clamp01((float)toCur   / required);

            if (progressBar != null) progressBar.value = start;
            if (progressLabel != null) progressLabel.text = $"{fromCur}/{required}";

            if (seconds <= 0.0001f || Mathf.Approximately(start, end))
            {
                if (progressBar != null) progressBar.value = end;
                if (progressLabel != null) progressLabel.text = $"{toCur}/{required}";
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / seconds; // works when timescale is 0
                float v = Mathf.SmoothStep(start, end, Mathf.Clamp01(t));
                if (progressBar != null) progressBar.value = v;

                int shown = Mathf.RoundToInt(Mathf.Lerp(fromCur, toCur, Mathf.Clamp01(t)));
                if (progressLabel != null) progressLabel.text = $"{shown}/{required}";

                yield return null;
            }

            if (progressBar != null) progressBar.value = end;
            if (progressLabel != null) progressLabel.text = $"{toCur}/{required}";
        }
    }
}