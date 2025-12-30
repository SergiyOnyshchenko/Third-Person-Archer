using UnityEngine;

public sealed class LoopProgressService : ILoopProgress
{
    private readonly MetaLoopProgressData _data;
    private readonly int _maxBalancedLoopIndex;

    public LoopProgressService(MetaLoopProgressData data, int maxBalancedLoopIndex = 2)
    {
        _data = data;
        _maxBalancedLoopIndex = Mathf.Max(0, maxBalancedLoopIndex);
    }

    public int CurrentLoopIndex => _data != null ? _data.CurrentLoopIndex : 0;

    public int GetBalanceLoopIndex()
    {
        int loop = CurrentLoopIndex;
        return Mathf.Clamp(loop, 0, _maxBalancedLoopIndex);
    }
}