// NOTE: Use this if you want to persist via your custom SaveSystem:
//  - In Project Settings > Player > Scripting Define Symbols add: ECONOMY_USE_SAVESYSTEM
//  - Ensure a static SaveSystem with Save<T>(string, T) and Load<T>(string, T) exists.

#if ECONOMY_USE_SAVESYSTEM
namespace Meta.Economy
{
    public sealed class CurrencyStorageSaveSystem : ICurrencyStorage
    {
        private string FileName(CurrencyType t) => $"wallet_{t}.json";

        public int Load(CurrencyType type, int defaultValue)
        {
            return SaveSystem.Load<int>(FileName(type), defaultValue);
        }

        public void Save(CurrencyType type, int value)
        {
            SaveSystem.Save(FileName(type), value);
        }

        public void DeleteAll()
        {
            foreach (CurrencyType t in System.Enum.GetValues(typeof(CurrencyType)))
                SaveSystem.Save(FileName(t), 0);
        }
    }
}
#endif
