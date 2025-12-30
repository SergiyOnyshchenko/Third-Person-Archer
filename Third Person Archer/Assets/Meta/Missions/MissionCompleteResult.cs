public sealed class MissionCompleteResult
{
    public bool Applied { get; }
    public MissionReward Reward { get; }

    public MissionCompleteResult(bool applied, MissionReward reward)
    {
        Applied = applied;
        Reward = reward;
    }
}