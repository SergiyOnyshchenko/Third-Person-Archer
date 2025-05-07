using UnityEngine;

public interface IProduct
{
    Price Price { get; }
    bool IsPurchased { get; }
    void Purchase(IBuyer buyer);
}
