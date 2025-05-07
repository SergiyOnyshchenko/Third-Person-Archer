using UnityEngine;

public class AddCurrencyRewardSubstate : SubState
{
    [SerializeField] private Price _reward;

    public override void Enter()
    {
        base.Enter();
        AddReward();
    }

    public void AddReward()
    {
        Economy.Instance.AddToCurrency(_reward.Currency, _reward.Value);
    }
}
