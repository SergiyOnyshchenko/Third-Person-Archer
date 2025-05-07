using UnityEngine;

[CreateAssetMenu(fileName = "Price", menuName = "Economy/Price")]
public class Price : ScriptableObject
{
    [SerializeField] private int _value;
    [SerializeField] private CurrencyType _currency;
    public int Value { get => _value; }
    public CurrencyType Currency { get => _currency; }
}
