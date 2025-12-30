using System;
using System.Collections.Generic;
using DG.Tweening;
using Meta.Economy;
using UnityEngine;

public sealed class VictoryRewardsSegment : MonoBehaviour, IVictorySegment
{
    [SerializeField] private CanvasGroup _rootGroup;

    [Header("Rows")]
    [SerializeField] private Transform _rowsRoot;
    [SerializeField] private CurrencyAmountRowView _rowPrefab;

    [Header("Animation")]
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private float _countDuration = 0.6f;

    public event Action OnShown;

    private Tween _fadeTween;
    private Tween _countTween;

    private readonly List<CurrencyAmountRowView> _rows = new();

    // Target values for current animation state (base by default)
    private readonly List<CurrencyRewardEntry> _targets = new();

    public void Show(VictoryContext context, FinalRewardBundle rewards)
    {
        gameObject.SetActive(true);

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 0f;
            _rootGroup.interactable = false;
            _rootGroup.blocksRaycasts = false;
        }

        BuildTargetsFromBundleBase(rewards);
        RebuildRows(initialAmount: 0);

        _fadeTween?.Kill();
        _countTween?.Kill();

        if (_rootGroup != null)
        {
            _fadeTween = _rootGroup.DOFade(1f, _fadeDuration)
                .OnComplete(BeginCount);
        }
        else
        {
            BeginCount();
        }
    }

    public void ApplyFinalValues(FinalRewardBundle rewards)
    {
        // Update targets to final values (after multiplier)
        BuildTargetsFromBundleFinal(rewards);

        // Update instantly (you can animate later if you want)
        // Keep same row order (Cash first, then tokens)
        ApplyTargetsToExistingRows();
    }

    private void BeginCount()
    {
        float t = 0f;

        _countTween = DOTween.To(
            () => t,
            v =>
            {
                t = v;
                for (int i = 0; i < _rows.Count && i < _targets.Count; i++)
                {
                    int value = Mathf.RoundToInt(Mathf.Lerp(0f, _targets[i].amount, t));
                    _rows[i].SetAmount(value);
                }
            },
            1f,
            _countDuration
        ).OnComplete(() =>
        {
            if (_rootGroup != null)
            {
                _rootGroup.interactable = true;
                _rootGroup.blocksRaycasts = true;
            }
            OnShown?.Invoke();
        });
    }

    private void BuildTargetsFromBundleBase(FinalRewardBundle rewards)
    {
        _targets.Clear();

        // Cash row first
        _targets.Add(new CurrencyRewardEntry(CurrencyType.Cash, Mathf.Max(0, rewards.baseCash)));

        // Then all token currencies (already currency-typed)
        if (rewards.baseCurrencies != null)
        {
            for (int i = 0; i < rewards.baseCurrencies.Count; i++)
            {
                var e = rewards.baseCurrencies[i];
                if (e.currency == CurrencyType.Cash) continue;
                if (e.amount <= 0) continue;
                _targets.Add(e);
            }
        }
    }

    private void BuildTargetsFromBundleFinal(FinalRewardBundle rewards)
    {
        _targets.Clear();

        _targets.Add(new CurrencyRewardEntry(CurrencyType.Cash, Mathf.Max(0, rewards.finalCash)));

        if (rewards.finalCurrencies != null)
        {
            for (int i = 0; i < rewards.finalCurrencies.Count; i++)
            {
                var e = rewards.finalCurrencies[i];
                if (e.currency == CurrencyType.Cash) continue;
                if (e.amount <= 0) continue;
                _targets.Add(e);
            }
        }
    }

    private void RebuildRows(int initialAmount)
    {
        ClearRows();

        if (_rowsRoot == null || _rowPrefab == null)
            return;

        for (int i = 0; i < _targets.Count; i++)
        {
            var e = _targets[i];
            var row = Instantiate(_rowPrefab, _rowsRoot);
            row.Setup(e.currency, initialAmount);
            _rows.Add(row);
        }
    }

    private void ApplyTargetsToExistingRows()
    {
        // If count mismatched (e.g. zero tokens then later some appear), rebuild safely.
        if (_rows.Count != _targets.Count)
        {
            RebuildRows(initialAmount: 0);
        }

        for (int i = 0; i < _rows.Count && i < _targets.Count; i++)
        {
            _rows[i].SetAmount(_targets[i].amount);
        }
    }

    private void ClearRows()
    {
        for (int i = 0; i < _rows.Count; i++)
            if (_rows[i] != null)
                Destroy(_rows[i].gameObject);

        _rows.Clear();
    }

    public void Skip()
    {
        _fadeTween?.Kill();
        _countTween?.Kill();

        if (_rootGroup != null)
        {
            _rootGroup.alpha = 1f;
            _rootGroup.interactable = true;
            _rootGroup.blocksRaycasts = true;
        }

        // Snap to target values
        ApplyTargetsToExistingRows();

        OnShown?.Invoke();
    }
}