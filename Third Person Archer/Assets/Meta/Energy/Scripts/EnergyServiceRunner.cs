using UnityEngine;

namespace Meta.Energy
{
    public sealed class EnergyServiceRunner : MonoBehaviour
    {
        public static EnergyServiceRunner Instance { get; private set; }

        [SerializeField] private EnergyConfig _config;

        public EnergyService Service { get; private set; }

        private ITimeProvider _timeProvider;

        private void Awake()
        {
            // Enforce singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_config == null)
            {
                Debug.LogError("[Energy] Missing EnergyConfig on EnergyServiceRunner singleton.");
                enabled = false;
                return;
            }

            _timeProvider = new LocalTimeProvider();
            Service = new EnergyService(_config, _timeProvider);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Service == null) return;
            if (hasFocus) Service.RecalculateFromNow();
        }

        private void Update()
        {
            if (Service == null) return;
            Service.TickForeground(Time.deltaTime);
        }

        private void OnApplicationPause(bool pause)
        {
            if (Service == null) return;
            if (pause) Service.ForceSave();
            else Service.RecalculateFromNow();
        }

        private void OnApplicationQuit()
        {
            if (Service == null) return;
            Service.ForceSave();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
