using System;

public interface IVictorySegment
{
    /// <summary>Called when this segment should start showing + animating.</summary>
    void Show(VictoryContext context, FinalRewardBundle rewards);

    /// <summary>Skip current animation and jump to final state.</summary>
    void Skip();

    /// <summary>Raised when this segment is fully finished.</summary>
    event Action OnShown;
}