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
        [SerializeField] private string _weaponUnlockedPopupId = "WeaponUnlockedPopup";
        [SerializeField] private string _weaponClassUnlockedPopupId = "WeaponClassUnlockedPopup";

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

            // IMPORTANT: created & registered by WeaponsInitializer
            _classUnlocks = ServiceLocator.Resolve<IWeaponClassUnlockService>();

            // Register only the popup flow (UI concern)
            ServiceLocator.Register<IWeaponUnlockPopupFlow>(this);

            _progress = SaveSystem.Load(SaveKey, new WeaponUnlockPopupsProgress());

            RebuildAndStart();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IWeaponUnlockPopupFlow>();
        }

        public void RebuildAndStart()
        {
            // If your concrete unlock service supports Refresh(), it should be refreshed by mission completion.
            // BUT we can be defensive here:
            if (_classUnlocks is WeaponClassUnlockService concrete)
                concrete.Refresh();

            _pendingArgs.Clear();
            _popupActive = false;

            BuildQueue();
            ShowNextIfAny();
        }

        private void BuildQueue()
        {
            // 1) Class popups: only for classes that are not start-unlocked
            EnqueueClassPopupIfNew(WeaponClass.Spear);
            EnqueueClassPopupIfNew(WeaponClass.Boomerang);

            // 2) Weapon popups: only if UnlockAfterCampaignLevel > 0 (your rule)
            int companyLevel = _classUnlocks.CurrentCompanyLevel;

            foreach (var def in _weaponCatalog.All)
            {
                if (def == null) continue;

                if (def.UnlockAfterCampaignLevel <= 0) continue;              // default weapons => no popup
                if (def.UnlockAfterCampaignLevel > companyLevel) continue;    // not yet reached

                // Only if its class is unlocked (so we don't show hidden content)
                if (!_classUnlocks.IsClassUnlocked(def.Class)) continue;

                if (_progress.shownWeaponIds.Contains(def.Id)) continue;

                _pendingArgs.Enqueue(new WeaponUnlockedPopupArgs
                {
                    Title = "New Weapon Unlocked",
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
                Title = "New Weapon Class Unlocked",
                WeaponClass = cls
            });
        }

        private void ShowNextIfAny()
        {
            if (_popupActive) return;
            if (_pendingArgs.Count <= 0) return;

            var args = _pendingArgs.Dequeue();

            // Mark-as-shown when we actually show it (guarantees “once”)
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

            // UX choice: navigating should stop popup spam.
            _pendingArgs.Clear();

            _nav.Open(_weaponSelectionScreenId, new WeaponSelectionArgs(weaponClass, CurrentCompanyLevel), reuseCached: true);
        }

        public void GoToWeapon(object popupInstance, string weaponId)
        {
            DestroyPopupInstance(popupInstance);
            _popupActive = false;

            _pendingArgs.Clear();

            var def = _weaponCatalog.All.FirstOrDefault(w => w != null && w.Id == weaponId);
            if (def == null)
                return;

            _nav.Open(_weaponSelectionScreenId, new WeaponSelectionArgs(def.Class, CurrentCompanyLevel, def.Id), reuseCached: true);
        }

        private static void DestroyPopupInstance(object popupInstance)
        {
            if (popupInstance == null) return;

            // If caller passed a MonoBehaviour (like "this"), destroy its GameObject.
            if (popupInstance is Component c)
            {
                Object.Destroy(c.gameObject);
                return;
            }

            // If caller passed a ScreenView, destroy its GameObject too (covers your UI framework).
            if (popupInstance is ScreenView sv)
            {
                Object.Destroy(sv.gameObject);
                return;
            }

            // If caller passed the GameObject directly.
            if (popupInstance is GameObject go)
            {
                Object.Destroy(go);
                return;
            }

            // Fallback: destroy UnityEngine.Object (best effort).
            if (popupInstance is Object uo)
            {
                Object.Destroy(uo);
            }
        }
    }
}