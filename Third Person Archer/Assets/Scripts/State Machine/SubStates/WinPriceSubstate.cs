using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Meta.Economy;

public class WinPriceSubstate : SubState
{
    [SerializeField] private WinCoinsView _winCoinsView;
    [SerializeField] private Button _adButton;
    private int _adMultiplier = 3;
    private int _coinsPrice = 0;

    public override void Enter()
    {
        base.Enter();
        _adButton.onClick.AddListener(AdMultiplyPrice);

        _coinsPrice = EnemyManager.Instance.GetDeadEnemiesCoins();
        _winCoinsView.Init(_coinsPrice);
    }

    public override void Exit()
    {
        Economy.Wallet.Add(CurrencyType.Cash, _coinsPrice);
        _adButton.onClick.RemoveListener(AdMultiplyPrice);
        base.Exit();
    }

    private void AdMultiplyPrice()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool value) =>
        {
            if (value)
            {
                _adButton.gameObject.SetActive(false);
                _coinsPrice *= _adMultiplier;
                _winCoinsView.Init(_coinsPrice);
            }
        });
    }
}
