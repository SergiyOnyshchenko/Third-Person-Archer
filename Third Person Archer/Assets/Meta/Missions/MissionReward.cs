using Meta.Weapons;

public sealed class MissionReward
{
    public int Money { get; }
    public int[] Tokens { get; } // index = (int)WeaponClass

    public MissionReward(int money, int[] tokens)
    {
        Money = money;
        Tokens = tokens;
    }

    public int GetTokens(WeaponClass weaponClass)
    {
        if (Tokens == null) return 0;
        int idx = (int)weaponClass;
        return (idx >= 0 && idx < Tokens.Length) ? Tokens[idx] : 0;
    }
}