using System;
using DG.Tweening;
using Meta.Economy;
using Meta.Weapons;
using UI.Core;
using UnityEngine;

/// <summary>
/// Red dot on the Arsenal button: some unlocked class can afford an upgrade or buy its next tier.
/// Replaces the chain of "new weapon" popups (D12: unlocks are shown on the map, not in popups).
/// </summary>
public sealed class WeaponsAttentionBadge : MonoBehaviour
{
    [SerializeField] private GameObject _dot;

    private Tween _pulse;
    private IClassProgressionService _progression;

    private void OnEnable()
    {
        if (Economy.Wallet != null) Economy.Wallet.BalanceChanged += OnBalanceChanged;

        var weapons = WeaponsInitializer.Instance;
        _progression = weapons != null ? weapons.Progression : null;
        if (_progression != null) _progression.OnProgressChanged += OnProgressChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (Economy.Wallet != null) Economy.Wallet.BalanceChanged -= OnBalanceChanged;
        if (_progression != null) _progression.OnProgressChanged -= OnProgressChanged;
        _pulse?.Kill();
    }

    private void OnBalanceChanged(CurrencyType type, int balance, int delta) => Refresh();
    private void OnProgressChanged(WeaponClass cls) => Refresh();

    public void Refresh()
    {
        if (_dot == null) return;

        bool show = HasSomethingToDo();
        _dot.SetActive(show);

        _pulse?.Kill();
        if (show)
        {
            _dot.transform.localScale = Vector3.one;
            _pulse = _dot.transform.DOScale(1.25f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(_dot);
        }
    }

    private static bool HasSomethingToDo() => Meta.Weapons.UI.ArsenalAttention.AnyClassHasSomethingToBuy();
}
