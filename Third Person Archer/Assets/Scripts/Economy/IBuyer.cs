using UnityEngine;

public interface IBuyer
{
    Transform Root { get; }
    bool CanPurchase(IProduct product);
    void MakePurchase(IProduct product);
}
