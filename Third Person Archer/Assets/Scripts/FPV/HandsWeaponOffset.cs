using System.Collections;
using System.Collections.Generic;
using Actor;
using Meta.Weapons;
using UnityEngine;

public class HandsWeaponOffset : MonoBehaviour, IActorIniter
{
    [System.Serializable]
    private struct Offset
    {
        public WeaponClass Class;
        public Vector3 Position;
        public Vector3 Rotation;
    }

    [SerializeField] private Transform _target;
    [Space]
    [SerializeField] private Offset[] _offsets;
    [SerializeField] private Vector3 _defaultPosition;
    [SerializeField] private Vector3 _defaultRotation;
    private EquippedWeaponDef _equippedWeapon;

    public void InitActor(ActorController actor)
    {
        if (_target == null)
            _target = transform;

        if (actor.TryGetProperty(out _equippedWeapon))
        {
            ApplyOffset();
            _equippedWeapon.OnPropertyChanged += ApplyOffset;
        }
    }

    private void OnDestroy()
    {
        if (_equippedWeapon != null)
            _equippedWeapon.OnPropertyChanged -= ApplyOffset;
    }

    public void ApplyOffset()
    {
        if (_equippedWeapon == null)
        {
            ApplyDefaultOffset();
            return;
        }

        if (_equippedWeapon.Value == null)
        {
            ApplyDefaultOffset();
            return;
        }

        ApplyOffset(_equippedWeapon.Value.Class);
    }

    public void ApplyOffset(WeaponClass weapon)
    {
        foreach (var offset in _offsets)
        {
            if (offset.Class == weapon)
            {
                ApplyOffset(offset.Position, offset.Rotation);
                return;
            }
        }

        ApplyDefaultOffset();
    }

    public void ApplyDefaultOffset()
    {
        ApplyOffset(_defaultPosition, _defaultRotation);
    }

    public void ApplyOffset(Vector3 position, Vector3 rotation)
    {
        _target.localPosition = position;
        _target.localEulerAngles = rotation;
    }
}
