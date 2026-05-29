#nullable enable
using System.Collections.Generic;
using System.Linq;
using Meta.Weapons.UI;
using UI.Core;
using UnityEngine;

namespace Meta.Weapons
{
    public sealed class WeaponUnlockPopupsController : MonoBehaviour, IWeaponUnlockPopupFlow
    {
        private const string SaveKey = "weapon_unlock_popups_progress_v1";

        [Header("Data")]
        [SerializeField] private WeaponCatalog _weaponCatalog = null!;

        [Header("Popup Screen Ids")]
        [SerializeField] private string _weaponUnlockedPopupId = "weapon_unlocked_popup";
        [SerializeField] private string _weaponClassUnlockedPopupId = "weapon_class_unlocked_popup";

        [Header("Navigation")]
        [SerializeField] private string _weaponSelectionScreenId = "WeaponSelection";

        // Public access for screens/presenters that need it
        public int CurrentCompanyLevel => _classUnlocks?.CurrentCompanyLevel ?? 1;
        public IWeaponClassUnlockService ClassUnlocks => _classUnlocks!;

        private IUINavigator _nav = null!;
        private IWeaponClassUnlockService _classUnlocks = null!;
        private WeaponUnlockPopupsProgress _progress = new();

        private readonly Queue<object> _pendingArgs = new();
        private bool _popupActive;

        private void Awake()
        {
        }

        private void Start()
        {
            _nav = ServiceLocator.Resolve<IUINavigator>();
            _classUnlocks = ServiceLocator.Resolve<IWeaponClassUnlockService>();
            ServiceLocator.Register<IWeaponUnlockPopupFlow>(this);
            _progress = SaveSystem.Load(SaveKey, new WeaponUnlockPopupsProgress());

            // Build the pending queue without showing anything yet.
            if (_classUnlocks is WeaponClassUnlockService concrete)
                concrete.Refresh();
            _pendingArgs.Clear();
            _popupActive = false;
            BuildQueue();

            if (_pendingArgs.Count == 0)
                return;

            // Determine priority from the first pending item.
            int priority = _pendingArgs.Peek() is WeaponClassUnlockedPopupArgs
                ? StartupPopupPriority.WeaponClassUnlock
                : StartupPopupPriority.WeaponUnlock;

            if (ServiceLocator.TryResolve<IStartupPopupCoordinator>(out var coord))
            {
                // Submit a single request; the coordinator calls ShowNextIfAny when it is
                // time to start the weapon unlock flow.  Subsequent popups in the flow are
                // shown by CloseAndContinue → ShowNextIfAny without coordinator involvement,
                // which is safe because all higher-priority requests have already been shown
                // by the time WeaponClassUnlock / WeaponUnlock requests are reached.
                coord.Submit(new StartupPopupRequest
                {
                    Priority         = priority,
                    LogicalId        = "weapon_unlock_flow",
                    CustomShowAction = ShowNextIfAny
                    // No OnBeforeShow: mark-as-shown happens inside ShowNextIfAny.
                });
            }
            else
            {
                // Fallback: no coordinator in scene — show directly (original behaviour).
                ShowNextIfAny();
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IWeaponUnlockPopupFlow>();
        }

        /// <summary>
        /// Rebuild the pending queue and show the first popup immediately (no coordinator).
        /// Called externally when weapon state changes mid-session (e.g. after a purchase).
        /// </summary>
        public void RebuildAndStart()
        {
            if (_classUnlocks is WeaponClassUnlockService concrete)
                concrete.Refresh();

            _pendingArgs.Clear();
            _popupActive = false;

            BuildQueue();
            ShowNextIfAny();
        }

        private void BuildQueue()
        {
            EnqueueClassPopupIfNew(WeaponClass.Spear);
            EnqueueClassPopupIfNew(WeaponClass.Boomerang);

            int companyLevel = _classUnlocks.CurrentCompanyLevel;

            foreach (var def in _weaponCatalog.All)
            {
                if (def == null) continue;
                if (def.UnlockAfterCampaignLevel <= 0) continue;
                if (def.UnlockAfterCampaignLevel > companyLevel) continue;
                if (!_classUnlocks.IsClassUnlocked(def.Class)) continue;
                if (_progress.shownWeaponIds.Contains(def.Id)) continue;

                _pendingArgs.Enqueue(new WeaponUnlockedPopupArgs
                {
                    Title    = "New Weapon Unlocked",
                    WeaponId = def.Id
                });
            }
        }

        private void EnqueueClassPopupIfNew(WeaponClass cls)
        {
            if (!_classUnlocks.IsClassUnlocked(cls)) return;

            int v = (int)cls;
            if (_progress.shownWeaponClasses.Contains(v)) return;

            _pendingArgs.Enqueue(new WeaponClassUnlockedPopupArgs
            {
                Title       = "New Weapon Class Unlocked",
                WeaponClass = cls,
                BodyText    = GetClassIntroBody(cls)
            });
        }

        private static string? GetClassIntroBody(WeaponClass cls) => cls switch
        {
            WeaponClass.Spear =>
                "The Spear is a high-damage weapon built for power — it hits hard and is effective against tough enemies.\n\n" +
                "Upcoming Campaign missions will check your Spear damage. Upgrade your Spear to keep advancing.\n\n" +
                "Spear Tokens can be earned from Campaign missions and Contracts.",
            WeaponClass.Boomerang =>
                "The Boomerang is a returning weapon — it launches, arcs back, and can hit multiple targets in one throw.\n\n" +
                "Upcoming Campaign missions will check your Boomerang damage. Upgrade your Boomerang to keep advancing.\n\n" +
                "Boomerang Tokens can be earned from Campaign missions and Contracts.",
            _ => null
        };

        private void ShowNextIfAny()
        {
            if (_popupActive) return;
            if (_pendingArgs.Count <= 0) return;

            var args = _pendingArgs.Dequeue();

            if (args is WeaponUnlockedPopupArgs w)
            {
                if (!_progress.shownWeaponIds.Contains(w.WeaponId))
                    _progress.shownWeaponIds.Add(w.WeaponId);
            }
            else if (args is WeaponClassUnlockedPopupArgs c)
            {
                int v = (int)c.WeaponClass;
                if (!_progress.shownWeaponClasses.Contains(v))
                    _progress.shownWeaponClasses.Add(v);
            }

            SaveSystem.Save(SaveKey, _progress);

            _popupActive = true;

            if (args is WeaponUnlockedPopupArgs)
                _nav.ShowPopup(_weaponUnlockedPopupId, args);
            else
                _nav.ShowPopup(_weaponClassUnlockedPopupId, args);
        }

        // ---- IWeaponUnlockPopupFlow ----

        public void CloseAndContinue(object popupInstance)
        {
            DestroyPopupInstance(popupInstance);
            _popupActive = false;
            ShowNextIfAny();
        }

        public void GoToWeaponClass(object popupInstance, WeaponClass weaponClass)
        {
            DestroyPopupInstance(popupInstance);
            _popupActive = false;
            _pendingArgs.Clear();
            _nav.Open(_weaponSelectionScreenId, new WeaponSelectionArgs(weaponClass, CurrentCompanyLevel), reuseCached: true);
        }

        public void GoToWeapon(object popupInstance, string weaponId)
        {
            DestroyPopupInstance(popupInstance);
            _popupActive = false;
            _pendingArgs.Clear();

            var def = _weaponCatalog.All.FirstOrDefault(w => w != null && w.Id == weaponId);
            if (def == null) return;

            _nav.Open(_weaponSelectionScreenId, new WeaponSelectionArgs(def.Class, CurrentCompanyLevel, def.Id), reuseCached: true);
        }

        private static void DestroyPopupInstance(object popupInstance)
        {
            if (popupInstance == null) return;

            if (popupInstance is Component c)
            {
                Object.Destroy(c.gameObject);
                return;
            }

            if (popupInstance is ScreenView sv)
            {
                Object.Destroy(sv.gameObject);
                return;
            }

            if (popupInstance is GameObject go)
            {
                Object.Destroy(go);
                return;
            }

            if (popupInstance is Object uo)
                Object.Destroy(uo);
        }
    }
}
