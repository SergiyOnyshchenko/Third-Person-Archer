using UnityEngine;

public sealed class CompleteMissionOutcomeSubState : SubState
{
    [SerializeField] private MissionOutcome _outcome = MissionOutcome.Completed;

    public override void Enter()
    {
        base.Enter();
        
        if (GameplayRuntime.Instance == null)
        {
            Debug.LogError("CompleteMissionOutcomeSubState: GameplayRuntime.Instance is null.");
            return;
        }

        var result = GameplayRuntime.Instance.CompleteMission(_outcome);
    }
}