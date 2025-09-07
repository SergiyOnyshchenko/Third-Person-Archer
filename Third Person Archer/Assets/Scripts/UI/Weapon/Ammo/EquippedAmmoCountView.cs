using System.Linq;
using UnityEngine;
using Actor;
using Meta.Weapons;

public class EquippedAmmoCountView : MonoBehaviour, IActorIniter
{
    private IAmmoCountView[] _allViews;
    private EquippedWeaponDef _equippedWeapon;

    public void InitActor(ActorController actor)
    {
        _allViews = GetComponentsInChildren<MonoBehaviour>(true)
            .OfType<IAmmoCountView>()
            .ToArray();
        
        SetAllActive(false);

        if (actor.TryGetProperty(out _equippedWeapon))
        {
            _equippedWeapon.OnPropertyChanged += OnEquippedWeaponChanged;
            OnEquippedWeaponChanged();
        }
        else
        {
            SetAllActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_equippedWeapon != null)
            _equippedWeapon.OnPropertyChanged -= OnEquippedWeaponChanged;
    }

    private void OnEquippedWeaponChanged()
    {
        if (_equippedWeapon == null || _equippedWeapon.Value == null)
        {
            SetAllActive(false);
            return;
        }

        var newWeaponClass = _equippedWeapon.Value.Class;

        foreach (var view in _allViews)
            view.gameObject.SetActive(newWeaponClass == view.WeaponType);
    }

    private void SetAllActive(bool active)
    {
        if (_allViews == null) return;
        foreach (var view in _allViews)
            view.gameObject.SetActive(active);
    }
}