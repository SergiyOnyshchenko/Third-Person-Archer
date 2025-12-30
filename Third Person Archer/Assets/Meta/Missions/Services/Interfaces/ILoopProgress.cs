public interface ILoopProgress
{
    int CurrentLoopIndex { get; }

    /// <summary>Loop index clamped to supported balance range (0..2 if you balance only 3 loops).</summary>
    int GetBalanceLoopIndex();
}