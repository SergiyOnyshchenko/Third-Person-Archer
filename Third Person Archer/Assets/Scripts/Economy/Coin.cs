using UnityEngine;

[CreateAssetMenu(fileName = "Coin", menuName = "Economy/Coin")]
public class Coin : Currency
{
    public override CurrencyType Type => CurrencyType.Coin;
}
