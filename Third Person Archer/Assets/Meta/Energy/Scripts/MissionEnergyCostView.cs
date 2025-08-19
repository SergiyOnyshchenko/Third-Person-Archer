using TMPro;
using UnityEngine;

namespace Meta.Energy.UI
{
    /// <summary>
    /// Shows how much energy a given mission type costs.
    /// If the cost is 0 (free), the view hides itself.
    /// </summary>
    [AddComponentMenu("Meta/Energy/Mission Energy Cost View")]
    public sealed class MissionEnergyCostView : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MissionType _missionType;
        [SerializeField] private bool _hideWhenZero = true;
        [SerializeField] private string _format = "{0}"; // e.g. "{0}" or "{0} ⚡"

        [Header("UI")]
        [SerializeField] private TMP_Text _costText; // displays the cost

        private EnergyService _service;

        private void OnEnable()
        {
            TryBindService();

            // If service isn't ready yet (bootstrap order), retry for a few frames
            if (_service == null) StartCoroutine(CoTryBindNextFrames());
        }

        private System.Collections.IEnumerator CoTryBindNextFrames()
        {
            for (int i = 0; i < 30 && _service == null; i++)
            {
                yield return null;
                TryBindService();
            }
        }

        private void TryBindService()
        {
            if (_service != null) return;

            var runner = EnergyServiceRunner.Instance; // singleton
            if (runner == null || runner.Service == null) return;

            _service = runner.Service;
            Refresh();
        }

        /// <summary>Manually refresh the view (useful if you change MissionType at runtime).</summary>
        public void Refresh()
        {
            if (_service == null) return;

            int cost = _service.GetCostFor(_missionType); // per-type cost (0 means free)
            if (_hideWhenZero && cost <= 0)
            {
                gameObject.SetActive(false); // hide as requested
                return;
            }

            if (_costText != null)
                _costText.text = string.Format(_format, Mathf.Max(0, cost));
        }

        /// <summary>Change the mission type and update the display.</summary>
        public void SetMissionType(MissionType newType)
        {
            _missionType = newType;
            Refresh();
        }
    }
}
