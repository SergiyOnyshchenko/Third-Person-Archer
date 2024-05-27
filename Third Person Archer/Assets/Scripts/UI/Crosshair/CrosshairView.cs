using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairView : MonoBehaviour, IActorIniter
{
    [SerializeField] private Transform _scalePart;
    [Space]
    [SerializeField] private float _minScale = 1;
    [SerializeField] private float _maxScale = 1.5f;
    private WeaponPull _weaponPull;
    private SpringFloat _springFloat;

    private void Start()
    {
        _springFloat = new SpringFloat(25f, 0.3f, _minScale);
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out WeaponPull weaponPull))
            _weaponPull = weaponPull;
    }

    private void FixedUpdate()
    {
        float scale = Mathf.Lerp(_minScale, _maxScale, _weaponPull.Value);
        _springFloat.UpdateValue(scale);
        _scalePart.localScale = new Vector3(_springFloat.Value, _springFloat.Value, _springFloat.Value);
    }
}
