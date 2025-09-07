using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Meta.Weapons.Unlocks;

public class WeaponClassUnlockController : MonoBehaviour
{
    [SerializeField] private WeaponClassUnlockConfig unlockConfig;
    private IWeaponClassUnlockService _unlockService;

    private void Awake()
    {
        var repo = new WeaponClassUnlockRepository(); 
        _unlockService = new WeaponClassUnlockService(unlockConfig, repo);
    }

    public void OnMissionCompleted(string missionId)
    {
        var deltas = _unlockService.RegisterMissionComplete(missionId);
        if (deltas.Count == 0) return;

        // Example: show a compact progress panel
        foreach (var d in deltas)
        {
            Debug.Log($"Progress: {d.After.Class} {d.After.Current}/{d.After.Required}" +
                      (d.NewlyUnlocked ? "  (UNLOCKED!)" : ""));
            // TODO: Drive your UI view here (progress bar + 'Unlocked!' badge)
        }

        // Optional: hint next locked class overall
        var next = _unlockService.GetNextLockedClass();
        if (next.HasValue)
        {
            var p = _unlockService.GetProgress(next.Value);
            Debug.Log($"Next to unlock: {next.Value}  {p.Current}/{p.Required}");
            // TODO: e.g., show a “Next: Crossbow 2/3” footer in your summary
        }
    }
}