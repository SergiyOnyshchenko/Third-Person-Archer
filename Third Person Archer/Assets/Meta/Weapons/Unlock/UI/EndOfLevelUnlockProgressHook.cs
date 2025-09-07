using UnityEngine;

namespace Meta.Weapons.Unlocks.UI
{
    /// <summary>
    /// Thin adapter to call the unlock service and forward the resulting deltas into the popup presenter.
    /// </summary>
    public sealed class EndOfLevelUnlockProgressHook : MonoBehaviour
    {
        [SerializeField] private WeaponClassUnlockProgressPresenter presenter;
        [SerializeField] private WeaponClassUnlockConfig config;

        // Swap this with your DI bootstrapping. Repository uses your SaveSystem.
        private IWeaponClassUnlockService _service;

        private void Awake()
        {
            var repo = new WeaponClassUnlockRepository(); // uses SaveSystem.Save/Load
            _service = new WeaponClassUnlockService(config, repo);
        }

        /// <summary>Call this when a mission is actually completed.</summary>
        public void OnMissionCompleted(string missionId)
        {
            var deltas = _service.RegisterMissionComplete(missionId);
            presenter.EnqueueResults(deltas);
        }
    }
}