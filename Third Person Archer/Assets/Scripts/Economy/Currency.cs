using UnityEngine;
using System;

public abstract class Currency : ScriptableObject
{
    [field: Header("Settings")]
    [field: SerializeField] protected int InitialAmount { get; private set; }
    [field: SerializeField] protected string SaveKey { get; private set; }
    [field: Header("View")]
    [field: SerializeField] public Sprite Icon { get; private set; }
    public abstract CurrencyType Type { get; }
    public int Amount { get; private set; }
   
    public event Action OnCurrencyUpdated;

    public void Save()
    {
        PlayerPrefs.SetInt(SaveKey, Amount);
    }

    public void Load()
    {
        Amount = PlayerPrefs.GetInt(SaveKey, InitialAmount);
        OnCurrencyUpdated?.Invoke();
    }

    public void Add(int amount)
    {
        if (amount < 0)
        {
            throw new System.ArgumentException("Amount cannot be negative.");
        }
        Amount += amount;
        OnCurrencyUpdated?.Invoke();
        Save();
    }

    public bool Subtract(int amount)
    {
        if (amount < 0)
        {
            throw new System.ArgumentException("Amount cannot be negative.");
        }
        if (amount > Amount)
        {
            return false; 
        }
        Amount -= amount;
        OnCurrencyUpdated?.Invoke();
        Save();
        return true;
    }
}