using UnityEngine;
using Meta.Weapons.Unlocks;

public sealed class UnlockProgressTransition : StateTransition
{
    [Header("Config")]
    [SerializeField] private WeaponClassUnlockConfig unlockConfig;// optional; implements ICompletedMissionProvider

    [Header("UI")]
    [SerializeField] private UnlockProgressSubState progressSubState;

    private IWeaponClassUnlockService _service;

    public interface ICompletedMissionProvider { string GetCompletedMissionId(); }

    public override void Enter()
    {
        base.Enter();

        // Get mission id
        var id = ResolveMissionId();
        if (string.IsNullOrEmpty(id)) { DoTransition(); return; }

        // Build service
        _service ??= new WeaponClassUnlockService(unlockConfig, new WeaponClassUnlockRepository());

        // Compute deltas
        var deltas = _service.RegisterMissionComplete(id);

        // If nothing changed -> move on
        if (deltas == null || deltas.Count == 0) { DoTransition(); return; }

        // Get substate
        if (progressSubState == null)
            progressSubState = GetComponentInChildren<UnlockProgressSubState>(true);

        if (progressSubState == null) { DoTransition(); return; }

        // Subscribe for completion
        progressSubState.Completed += OnProgressCompleted;

        // IMPORTANT: Prepare (deferred start). Substate will play on Enter().
        progressSubState.Prepare(deltas);
    }

    public override void Exit()
    {
        if (progressSubState != null)
            progressSubState.Completed -= OnProgressCompleted;
        base.Exit();
    }

    private void OnProgressCompleted() => DoTransition();

    private string ResolveMissionId()
    {
        if (MissionContext.Instance != null && MissionContext.Instance.MissionData != null)
            return MissionContext.Instance.MissionData.ID;

        return "";
    }
}