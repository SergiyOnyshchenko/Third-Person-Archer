using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Meta.Energy.UI
{
    /// <summary>
    /// Top-HUD energy bar view.
    /// Shows "current/max" and, when not full, shows a mm:ss countdown to the next +1.
    /// The "+" button invokes a UnityEvent so you can open your refill popup.
    /// </summary>
    [AddComponentMenu("Meta/Energy/Energy Bar View")]
    public sealed class EnergyBarView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _slider;            // optional, for visual fill
        [SerializeField] private TMP_Text _valueText;       // "current/max"
        [SerializeField] private TMP_Text _timerText;       // "mm:ss" or "Full"
        [SerializeField] private Button _plusButton;        // opens refill popup (event below)

        [Header("Display")]
        [SerializeField] private bool _hideTimerWhenFull = false;
        [SerializeField] private string _fullLabel = "Full";

        [Header("Actions")]
        [SerializeField] private UnityEvent _onPlusClicked;

        // Cache the service we subscribe to
        private EnergyService _service;
        private bool _subscribed;

        private void Awake()
        {
            // Optional: wire button click
            if (_plusButton != null)
                _plusButton.onClick.AddListener(OnPlusClicked);
        }

        private void OnEnable()
        {
            TryBindService();

            // If service isn't ready yet (e.g., bootstrap scene still loading), try again next frame
            if (_service == null)
                StartCoroutine(CoTryBindNextFrames());
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private System.Collections.IEnumerator CoTryBindNextFrames()
        {
            // Retry for a few frames in case the singleton appears shortly after this view
            for (int i = 0; i < 30 && _service == null; i++)
            {
                yield return null;
                TryBindService();
            }
        }

        private void TryBindService()
        {
            if (_service != null) return;

            var runner = EnergyServiceRunner.Instance;
            if (runner == null || runner.Service == null) return;

            _service = runner.Service;
            Subscribe();
            // Initial paint
            PaintEnergy(_service.CurrentEnergy, _service.MaxEnergy);
            PaintTimer(_service.SecondsToNextPoint, _service.IsFull);
        }

        private void Subscribe()
        {
            if (_service == null || _subscribed) return;

            _service.OnEnergyChanged += HandleEnergyChanged;
            _service.OnSecondsToNextChanged += HandleSecondsToNextChanged;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (_service == null || !_subscribed) return;

            _service.OnEnergyChanged -= HandleEnergyChanged;
            _service.OnSecondsToNextChanged -= HandleSecondsToNextChanged;
            _subscribed = false;
        }

        private void HandleEnergyChanged(int current, int max)
        {
            PaintEnergy(current, max);

            // When energy hits full, ensure timer reflects "Full" (or hides).
            if (current >= max)
                PaintTimer(0, true);
        }

        private void HandleSecondsToNextChanged(int seconds)
        {
            var isFull = _service != null && _service.IsFull;
            PaintTimer(seconds, isFull);
        }

        private void PaintEnergy(int current, int max)
        {
            // Clamp defensively
            if (current < 0) current = 0;
            if (max <= 0) max = 1;
            if (current > max) current = max;

            if (_valueText != null)
                _valueText.text = $"{current}/{max}";

            if (_slider != null)
            {
                _slider.minValue = 0f;
                _slider.maxValue = max;
                _slider.value = current;
            }
        }

        private void PaintTimer(int seconds, bool isFull)
        {
            if (_timerText == null) return;

            if (isFull)
            {
                if (_hideTimerWhenFull)
                {
                    _timerText.gameObject.SetActive(false);
                }
                else
                {
                    _timerText.gameObject.SetActive(true);
                    _timerText.text = _fullLabel;
                }
                return;
            }

            _timerText.gameObject.SetActive(true);
            if (seconds < 0) seconds = 0;

            // mm:ss format
            int minutes = seconds / 60;
            int sec = seconds % 60;
            _timerText.text = $"{minutes:00}:{sec:00}";
        }

        private void OnPlusClicked()
        {
            // Always allow opening the popup, even when full (buttons inside can be disabled)
            _onPlusClicked?.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // Auto-assign common references if placed under a typical hierarchy.
            if (_slider == null) _slider = GetComponentInChildren<Slider>(true);
            if (_valueText == null) _valueText = GetComponentInChildren<TMP_Text>(true);
            if (_plusButton == null) _plusButton = GetComponentInChildren<Button>(true);

            // Try to find a second TMP_Text as timer if both are present under same GO
            if (_timerText == null)
            {
                var texts = GetComponentsInChildren<TMP_Text>(true);
                if (texts != null && texts.Length > 1)
                {
                    // Heuristic: the second text is often the timer
                    _timerText = texts[1];
                }
            }
        }
#endif
    }
}