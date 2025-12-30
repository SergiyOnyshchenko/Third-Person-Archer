using System;
using DG.Tweening;
using UnityEngine;
using TMPro;

public sealed class VictoryStatsSegment : MonoBehaviour, IVictorySegment
{
    [SerializeField] private CanvasGroup _rootGroup;
    [SerializeField] private TMP_Text _killsLabel;
    [SerializeField] private RectTransform _killsRoot;

    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private float _countDuration = 0.6f;
    [SerializeField] private float _punchScale = 1.1f;

    public event Action OnShown;

    private Tween _fadeTween;
    private Tween _countTween;
    private Tween _scaleTween;

    private int _targetKills;

    public void Show(VictoryContext context, FinalRewardBundle rewards)
    {
        gameObject.SetActive(true);

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 0f;
            _rootGroup.interactable = false;
            _rootGroup.blocksRaycasts = false;
        }

        _targetKills = context.enemiesKilled;

        if (_killsLabel != null)
            _killsLabel.text = "0";

        if (_killsRoot != null)
            _killsRoot.localScale = Vector3.one;

        _fadeTween?.Kill();
        _countTween?.Kill();
        _scaleTween?.Kill();

        if (_rootGroup != null)
        {
            _fadeTween = _rootGroup.DOFade(1f, _fadeDuration)
                .OnComplete(BeginCountAnimation);
        }
        else
        {
            BeginCountAnimation();
        }
    }

    private void BeginCountAnimation()
    {
        if (_killsLabel == null)
        {
            OnShown?.Invoke();
            return;
        }

        int value = 0;

        _countTween = DOTween.To(
                () => value,
                v =>
                {
                    value = v;
                    _killsLabel.text = $"{value}";
                },
                _targetKills,
                _countDuration
            )
            .OnComplete(() =>
            {
                if (_rootGroup != null)
                {
                    _rootGroup.interactable = true;
                    _rootGroup.blocksRaycasts = true;
                }

                OnShown?.Invoke();
            });

        if (_killsRoot != null)
        {
            _scaleTween = _killsRoot
                .DOPunchScale(Vector3.one * 0.1f, _countDuration, vibrato: 4)
                .OnComplete(() => _killsRoot.localScale = Vector3.one);
        }
    }

    public void Skip()
    {
        _fadeTween?.Kill();
        _countTween?.Kill();
        _scaleTween?.Kill();

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 1f;
            _rootGroup.interactable = true;
            _rootGroup.blocksRaycasts = true;
        }

        if (_killsLabel != null)
            _killsLabel.text = $"{_targetKills}";

        if (_killsRoot != null)
            _killsRoot.localScale = Vector3.one;

        OnShown?.Invoke();
    }
}