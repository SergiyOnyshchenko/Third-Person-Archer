using System.Collections;
using System.Collections.Generic;
using Meta.Economy;
using Meta.Weapons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class VictoryScreenSubState : SubState
{
    [Header("Root")]
    [SerializeField] private GameObject _root;          // A1: root GO
    [SerializeField] private CanvasGroup _rootGroup;    // optional

    [Header("Segments")]
    [SerializeField] private VictoryHeaderSegment _header;
    [SerializeField] private VictoryStatsSegment _stats;
    [SerializeField] private VictoryRewardsSegment _rewards;
    [SerializeField] private VictoryMultiplierSegment _multiplier;

    [Header("Continue")]
    [SerializeField] private Button _continueButton;

    [Min(0f)]
    [SerializeField] private float _finishDelay = 0.5f;

    [Header("Events")]
    public UnityEvent OnVictoryScreenFinished;

    private readonly List<IVictorySegment> _segments = new();
    private int _segmentIndex = -1;

    private VictoryContext _context;
    private FinalRewardBundle _bundle;

    private Coroutine _finishRoutine;
    private bool _payoutApplied;

    public override void Enter()
    {
        base.Enter();

        ShowRoot(true);
        BuildSequence();

        if (_continueButton != null)
        {
            _continueButton.onClick.RemoveAllListeners();
            _continueButton.onClick.AddListener(OnContinueClicked);
            _continueButton.gameObject.SetActive(false); // shown after segments end
            _continueButton.interactable = true;
        }

        var missionCtx = GameplayRuntime.Instance.Context;
        var progress = GameplayRuntime.Instance != null ? GameplayRuntime.Instance.ProgressData : null;

        _context = VictoryContextFactory.FromMissionContext(missionCtx, progress);
        if (_context == null)
        {
            Debug.LogError("VictoryScreenSubState: context is null.");
            return;
        }

        _bundle = ComputeRewardsPreview(missionCtx, GameplayRuntime.Instance.Rewards);

        if (_multiplier != null)
        {
            _multiplier.OnMultiplierDecided -= OnMultiplierDecided;
            _multiplier.OnMultiplierDecided += OnMultiplierDecided;
        }

        _payoutApplied = false;

        _segmentIndex = -1;
        MoveNext();
    }

    public override void Exit()
    {
        if (_multiplier != null)
            _multiplier.OnMultiplierDecided -= OnMultiplierDecided;

        if (_finishRoutine != null)
        {
            StopCoroutine(_finishRoutine);
            _finishRoutine = null;
        }

        base.Exit();
    }

    private void BuildSequence()
    {
        _segments.Clear();
        if (_header != null) _segments.Add(_header);
        if (_stats != null) _segments.Add(_stats);
        if (_rewards != null) _segments.Add(_rewards);
        if (_multiplier != null) _segments.Add(_multiplier);

        for (int i = 0; i < _segments.Count; i++)
        {
            _segments[i].OnShown -= MoveNext;
            _segments[i].OnShown += MoveNext;
        }
    }

    private void MoveNext()
    {
        _segmentIndex++;

        if (_segmentIndex >= _segments.Count)
        {
            if (_continueButton != null)
                _continueButton.gameObject.SetActive(true);
            return;
        }

        _segments[_segmentIndex].Show(_context, _bundle);
    }

    private static FinalRewardBundle ComputeRewardsPreview(
        MissionContext ctx,
        IMissionRewardService rewardService)
    {
        if (ctx == null || rewardService == null)
            return new FinalRewardBundle();

        MissionReward reward = rewardService.Calculate(ctx);

        var bundle = new FinalRewardBundle
        {
            baseCash = reward.Money,
            multiplier = 1f
        };

        if (reward.Tokens != null)
        {
            for (int i = 0; i < reward.Tokens.Length; i++)
            {
                int amount = reward.Tokens[i];
                if (amount <= 0)
                    continue;

                var weaponClass = (WeaponClass)i;
                CurrencyType currency = WeaponCurrencyUtility.GetTokenCurrency(weaponClass);
                bundle.baseCurrencies.Add(new CurrencyRewardEntry(currency, amount));
            }
        }

        bundle.finalCash = bundle.baseCash;
        bundle.finalCurrencies = new List<CurrencyRewardEntry>(bundle.baseCurrencies);

        return bundle;
    }

    private void OnMultiplierDecided(float multiplier)
    {
        _bundle.multiplier = multiplier;

        _bundle.finalCash = Mathf.RoundToInt(_bundle.baseCash * multiplier);

        _bundle.finalCurrencies.Clear();
        foreach (var e in _bundle.baseCurrencies)
        {
            _bundle.finalCurrencies.Add(
                new CurrencyRewardEntry(
                    e.currency,
                    Mathf.RoundToInt(e.amount * multiplier)));
        }

        if (_rewards != null)
            _rewards.ApplyFinalValues(_bundle);
    }

    private void OnContinueClicked()
    {
        if (_continueButton != null)
            _continueButton.interactable = false;

        if (_multiplier != null && !_multiplier.IsResolved)
        {
            _multiplier.ResolveAsNoMultiplier();
            // OnMultiplierDecided(1f) will be invoked and will update _bundle.final*
        }

        // Apply payout once
        if (!_payoutApplied)
        {
            _payoutApplied = true;
            ApplyPayout(_bundle);
        }

        if (_finishRoutine != null)
            StopCoroutine(_finishRoutine);

        _finishRoutine = StartCoroutine(FinishAfterDelay());
    }

    private IEnumerator FinishAfterDelay()
    {
        if (_finishDelay > 0f)
            yield return new WaitForSeconds(_finishDelay);

        OnVictoryScreenFinished?.Invoke();
    }

    private static void ApplyPayout(FinalRewardBundle rewards)
    {
        if (Economy.Wallet == null)
        {
            Debug.LogError("Victory payout: Economy.Wallet is null. Did you install EconomyInstaller?");
            return;
        }

        if (rewards.finalCash > 0)
            Economy.Wallet.Add(CurrencyType.Cash, rewards.finalCash);

        if (rewards.finalCurrencies != null)
        {
            foreach (var e in rewards.finalCurrencies)
            {
                if (e.currency == CurrencyType.Cash) continue;
                if (e.amount <= 0) continue;
                Economy.Wallet.Add(e.currency, e.amount);
            }
        }
    }

    private void ShowRoot(bool show)
    {
        if (_root != null)
            _root.SetActive(show);

        if (_rootGroup != null)
        {
            _rootGroup.alpha = show ? 1f : 0f;
            _rootGroup.interactable = show;
            _rootGroup.blocksRaycasts = show;
        }
    }
}