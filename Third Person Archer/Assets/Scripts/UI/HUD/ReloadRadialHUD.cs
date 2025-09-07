using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    [DefaultExecutionOrder(10_000)]
    public class ReloadRadialHUD : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _radialFill; 

        [Header("Behavior")]
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _hideWhenDone = true;

        private Coroutine _routine;

        private void OnEnable()
        {
            ReloadSignals.OnReloadStarted += HandleStart;
            ReloadSignals.OnReloadEnded   += HandleEnd;
            HideImmediate();
        }

        private void OnDisable()
        {
            ReloadSignals.OnReloadStarted -= HandleStart;
            ReloadSignals.OnReloadEnded   -= HandleEnd;
        }

        private void HandleStart(float duration)
        {
            if (duration <= 0f)
            {
                // Instant reload → flash full then hide
                ShowImmediate();
                SetFill(0f);
                if (_hideWhenDone) HideImmediate();
                return;
            }

            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Animate(duration));
        }

        private void HandleEnd()
        {
            if (_routine != null) { StopCoroutine(_routine); _routine = null; }
            if (_hideWhenDone) HideImmediate();
            else SetFill(0f);
        }

        private IEnumerator Animate(float duration)
        {
            ShowImmediate();
            float start = _useUnscaledTime ? Time.unscaledTime : Time.time;
            float end   = start + duration;

            while (true)
            {
                float now = _useUnscaledTime ? Time.unscaledTime : Time.time;
                float t = Mathf.InverseLerp(end, start, now); // 1→0
                SetFill(Mathf.Clamp01(t));
                if (now >= end) break;
                yield return null;
            }

            // Done
            SetFill(0f);
            if (_hideWhenDone) HideImmediate();
            _routine = null;
        }

        private void SetFill(float value)
        {
            if (_radialFill) _radialFill.fillAmount = value;
        }

        private void ShowImmediate()
        {
            if (_group)
            {
                _group.alpha = 1f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
        }

        private void HideImmediate()
        {
            if (_group)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
        }
    }
}