using UnityEngine;
using Meta.Weapons.UI.Selection;

namespace Meta.Weapons.UI.Upgrade
{
    public class WeaponUpgradeScreenController : MonoBehaviour
    {
        [Header("View and Configs")]
        [SerializeField] private WeaponUpgradeScreenView view;
        [SerializeField] private UIStatNormalizationConfig normalization;
        [SerializeField] private UIStyleConfig style;
        [SerializeField] private WeaponUpgradeFocusMap focusMap;

        [Header("Backend Composition")]
        [SerializeField] private WeaponsInitializer backend; // exposes repository, equipmentService, upgradeService, statsService
        [SerializeField] private WeaponDef[] weaponCatalog;

        private WeaponUpgradeScreenPresenter presenter;

        private void Awake()
        {
            presenter = new WeaponUpgradeScreenPresenter(
                view,
                normalization,
                style,
                backend.WeaponRepository,
                backend.UpgradeService,
                backend.EquipmentService,
                backend.StatsService,
                Meta.Economy.Economy.Wallet, // replace with your implementation
                new SystemTimeProvider(),
                weaponCatalog,
                focusMap,
                playSuccessFeedback: () => { /* optional: trigger Feel/VFX */ }
            );

            presenter.OnRequestReturnToSelection += () =>
            {
                // Hide this screen and show the selection screen (your navigation system)
            };
        }

        private void Start() => presenter.Show();
        private void Update() => presenter.Tick();
    }
}
