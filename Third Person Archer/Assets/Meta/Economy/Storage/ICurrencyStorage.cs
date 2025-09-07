namespace Meta.Economy
{
    /// <summary>
    /// Persistence port for currency values; adapter pattern for PlayerPrefs/SaveSystem/etc.
    /// </summary>
    public interface ICurrencyStorage
    {
        int Load(CurrencyType type, int defaultValue);
        void Save(CurrencyType type, int value);
        void DeleteAll();
    }
}