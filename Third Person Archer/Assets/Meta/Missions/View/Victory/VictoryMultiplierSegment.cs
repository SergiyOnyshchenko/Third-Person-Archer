using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public struct MultiplierSegment
{
    [Range(0f, 1f)] public float start;
    [Range(0f, 1f)] public float end;
    public float multiplier;
}

public interface IAdsService
{
    void ShowRewarded(string placementId, Action<bool> onComplete);
}

public sealed class VictoryMultiplierSegment : MonoBehaviour, IVictorySegment
{
    [Header("Roots")]
    [SerializeField] private CanvasGroup _rootGroup;
    [SerializeField] private RectTransform _minigameRoot;
    [SerializeField] private RectTransform _summaryRoot;

    [Header("Arrow")]
    [SerializeField] private RectTransform _barRect;
    [SerializeField] private RectTransform _arrowRect;
    [SerializeField] private float _baseSpeed = 0.6f;
    [SerializeField] private AnimationCurve _speedCurve; // 0..1

    [Header("Segments")]
    [SerializeField] private List<MultiplierSegment> _segments;

    [Header("Buttons")]
    [SerializeField] private Button _multiplyButton;

    [Header("Summary (this is the multiplier label)")]
    [SerializeField] private TMP_Text _summaryText; // "Multiply 2x"

    [Header("Ads")]
    [SerializeField] private MonoBehaviour _adsServiceBehaviour;
    [SerializeField] private string _adsPlacementId = "reward_multiplier";

    [SerializeField] private float _fadeDuration = 0.3f;

    public event Action OnShown;
    public event Action<float> OnMultiplierDecided;

    private IAdsService AdsService => _adsServiceBehaviour as IAdsService;

    public bool IsResolved => _multiplierChosen;
    public float ChosenMultiplier => _chosenMultiplier;

    private float _position;
    private int _direction = 1;
    private bool _running;
    private bool _multiplierChosen;
    private float _chosenMultiplier = 1f;

    private Tween _fadeTween;

    private void Awake()
    {
        if (_multiplyButton != null)
        {
            _multiplyButton.onClick.RemoveAllListeners();
            _multiplyButton.onClick.AddListener(OnMultiplyClicked);
        }
    }

    public void Show(VictoryContext context, FinalRewardBundle rewards)
    {
        gameObject.SetActive(true);

        _multiplierChosen = false;
        _chosenMultiplier = 1f;
        _position = 0f;
        _direction = 1;
        _running = false;

        _fadeTween?.Kill();

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 0f;
            _rootGroup.interactable = false;
            _rootGroup.blocksRaycasts = false;
        }

        // Minigame visible, summary hidden (so multiplier label not visible)
        if (_minigameRoot != null) _minigameRoot.gameObject.SetActive(true);
        if (_summaryRoot != null) _summaryRoot.gameObject.SetActive(false);

        UpdateArrowVisual();

        if (_rootGroup != null)
        {
            _fadeTween = _rootGroup.DOFade(1f, _fadeDuration)
                .OnComplete(() =>
                {
                    _rootGroup.interactable = true;
                    _rootGroup.blocksRaycasts = true;

                    _running = true; // start arrow
                    OnShown?.Invoke(); // IMPORTANT: this allows Continue button to appear
                });
        }
        else
        {
            _running = true;
            OnShown?.Invoke();
        }
    }

    private void Update()
    {
        if (!_running || _multiplierChosen || _arrowRect == null || _barRect == null)
            return;

        float distFromCenter = Mathf.Abs((_position - 0.5f) * 2f); // 0 at center, 1 at edges
        float speedFactor = _speedCurve != null
            ? _speedCurve.Evaluate(1f - distFromCenter)
            : 1f + (1f - distFromCenter);

        _position += _direction * _baseSpeed * speedFactor * Time.deltaTime;

        if (_position >= 1f)
        {
            _position = 1f;
            _direction = -1;
        }
        else if (_position <= 0f)
        {
            _position = 0f;
            _direction = 1;
        }

        UpdateArrowVisual();
    }

    private void UpdateArrowVisual()
    {
        if (_barRect == null || _arrowRect == null)
            return;

        float barWidth = _barRect.rect.width;
        float localX = Mathf.Lerp(-barWidth / 2f, barWidth / 2f, _position);

        var pos = _arrowRect.localPosition;
        pos.x = localX;
        _arrowRect.localPosition = pos;
    }

    private void OnMultiplyClicked()
    {
        if (_multiplierChosen) return;

        // Always tied to ad
        if (AdsService != null)
            AdsService.ShowRewarded(_adsPlacementId, OnAdCompleted);
        else
            OnAdCompleted(true); // no ads service: treat as success for now
    }

    private void OnAdCompleted(bool success)
    {
        if (_multiplierChosen) return;

        if (!success)
        {
            // Ad failed: cancel multiplier, keep 1x
            CompleteMinigame(1f);
            return;
        }

        _running = false;
        float multiplier = EvaluateMultiplier(_position);
        CompleteMinigame(multiplier);
    }

    /// <summary>
    /// Called by VictoryScreenSubState when user presses Continue while minigame is still active.
    /// No ad => no multiplier => 1x.
    /// </summary>
    public void ResolveAsNoMultiplier()
    {
        if (_multiplierChosen) return;

        _running = false;
        CompleteMinigame(1f);
    }

    private float EvaluateMultiplier(float pos)
    {
        if (_segments == null || _segments.Count == 0)
            return 1f;

        foreach (var seg in _segments)
        {
            if (pos >= seg.start && pos < seg.end)
                return seg.multiplier;
        }

        return 1f;
    }

    private void CompleteMinigame(float multiplier)
    {
        _multiplierChosen = true;
        _chosenMultiplier = multiplier;

        // Minigame hidden, summary visible (now multiplier label can be seen)
        if (_minigameRoot != null) _minigameRoot.gameObject.SetActive(false);
        if (_summaryRoot != null) _summaryRoot.gameObject.SetActive(true);

        if (_summaryText != null)
            _summaryText.text = $"Multiply {multiplier:0.#}x";

        OnMultiplierDecided?.Invoke(multiplier);
    }

    public void Skip()
    {
        // Segment-level skip (e.g. debug) behaves like Continue-skip: 1x
        ResolveAsNoMultiplier();

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 1f;
            _rootGroup.interactable = true;
            _rootGroup.blocksRaycasts = true;
        }

        OnShown?.Invoke();
    }
}