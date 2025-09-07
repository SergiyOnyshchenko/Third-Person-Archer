using UnityEngine;

namespace Meta.Economy
{
    /// <summary>Default, lightweight storage. Safe and simple.</summary>
    public sealed class CurrencyStoragePlayerPrefs : ICurrencyStorage
    {
        private string Key(CurrencyType t) => $"wallet_{t}";

        public int Load(CurrencyType type, int defaultValue)
        {
            return PlayerPrefs.GetInt(Key(type), defaultValue);
        }

        public void Save(CurrencyType type, int value)
        {
            PlayerPrefs.SetInt(Key(type), value);
            PlayerPrefs.Save();
        }

        public void DeleteAll()
        {
            foreach (CurrencyType t in System.Enum.GetValues(typeof(CurrencyType)))
                PlayerPrefs.DeleteKey(Key(t));
            PlayerPrefs.Save();
        }
    }
}