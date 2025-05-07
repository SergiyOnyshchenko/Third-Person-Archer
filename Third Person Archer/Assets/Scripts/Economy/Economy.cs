using UnityEngine;

public class Economy : MonoBehaviour
{
    [SerializeField] private Currency[] _currencies;

    public static Economy Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        foreach (var currency in _currencies)
        {
            currency.Load();
        }
    }

    void OnApplicationQuit()
    {
        foreach (var currency in _currencies)
        {
            currency.Save();
        }
    }

    public Currency GetCurrency(CurrencyType type)
    {
        foreach (var currency in _currencies)
        {
            if (currency.Type == type)
            {
                return currency;
            }
        }

        //Debug.LogError($"Currency \"{type}\" not found.");
        return null;
    }

    public void AddToCurrency(CurrencyType type, int amount)
    {
        Currency currency = GetCurrency(type);
        if (currency != null)
        {
            currency.Add(amount);
            //Debug.Log($"Added {amount} to \"{type}\". New balance: {currency.Amount}");
        }
    }

    public bool SubtractFromCurrency(CurrencyType type, int amount)
    {
        Currency currency = GetCurrency(type);
        if (currency != null)
        {
            bool success = currency.Subtract(amount);
            if (success)
            {
                //Debug.Log($"Subtracted {amount} from \"{type}\". New balance: {currency.Amount}");
            }
            else
            {
                //Debug.LogWarning($"Not enough \"{type}\" to subtract {amount}. Current balance: {currency.Amount}");
            }
            return success;
        }
        return false;
    }

    public int GetCurrencyAmount(CurrencyType type)
    {
        Currency currency = GetCurrency(type);
        return currency != null ? currency.Amount : 0;
    }
}
