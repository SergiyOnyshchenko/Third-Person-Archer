using UnityEngine;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// Panel that displays all stats for a weapon using individual WeaponStatView items.
    /// Completely dumb: just maps WeaponStats to views.
    /// </summary>
    public class WeaponStatsPanelView : MonoBehaviour
    {
        [Header("Per-Stat Views")]
        [SerializeField] private WeaponStatView _damageView;
        [SerializeField] private WeaponStatView _balanceView;
        [SerializeField] private WeaponStatView _distanceView;
        [SerializeField] private WeaponStatView _ammoView;
        [SerializeField] private WeaponStatView _reloadView;
        [SerializeField] private WeaponStatView _zoomView;

        private void Reset()
        {
            // Optional: auto-set labels in editor if you like
            _damageView?.SetLabel("Damage");
            _balanceView?.SetLabel("Balance");
            _distanceView?.SetLabel("Distance");
            _ammoView?.SetLabel("Ammo");
            _reloadView?.SetLabel("Reload");
            _zoomView?.SetLabel("Zoom");
        }

        /// <summary>
        /// Shows current stats (from WeaponStats) on all views.
        /// minStats/maxStats define slider ranges.
        /// </summary>
        public void SetCurrentStats(WeaponStats current, WeaponStats minStats, WeaponStats maxStats)
        {
            Reset();

            if (_damageView != null)
                _damageView.SetCurrent(current.Damage,     minStats.Damage,     maxStats.Damage);
            if (_balanceView != null)
                _balanceView.SetCurrent(current.Balance,   minStats.Balance,    maxStats.Balance);
            if (_distanceView != null)
                _distanceView.SetCurrent(current.Distance, minStats.Distance,   maxStats.Distance);
            if (_ammoView != null)
                _ammoView.SetCurrent(current.AmmoCount,    minStats.AmmoCount,  maxStats.AmmoCount);
            if (_reloadView != null)
                _reloadView.SetCurrent(current.ReloadTime, minStats.ReloadTime, maxStats.ReloadTime);
            if (_zoomView != null)
                _zoomView.SetCurrent(current.Zoom,         minStats.Zoom,       maxStats.Zoom);

            // When we set current stats, it's safe to hide previews.
            ClearPreview();
        }

        /// <summary>
        /// Shows upgrade preview for all stats (current -> next).
        /// If show == false, preview is hidden.
        /// </summary>
        public void SetUpgradePreview(WeaponStats current, WeaponStats next, bool show)
        {
            if (_damageView != null)
                _damageView.SetPreview(current.Damage,     next.Damage,     show);
            if (_balanceView != null)
                _balanceView.SetPreview(current.Balance,   next.Balance,    show);
            if (_distanceView != null)
                _distanceView.SetPreview(current.Distance, next.Distance,   show);
            if (_ammoView != null)
                _ammoView.SetPreview(current.AmmoCount,    next.AmmoCount,  show);
            if (_reloadView != null)
                _reloadView.SetPreview(current.ReloadTime, next.ReloadTime, show);
            if (_zoomView != null)
                _zoomView.SetPreview(current.Zoom,         next.Zoom,       show);
        }

        public void ClearPreview()
        {
            // Just reuse SetPreview with show=false
            var dummy = new WeaponStats();
            SetUpgradePreview(dummy, dummy, false);
        }
    }
}