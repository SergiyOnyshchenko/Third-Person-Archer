using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;

public class HeadShootFeedback : MonoBehaviour
{
    [SerializeField] private Hitbox _hitbox;
    [SerializeField] private Transform _scaleBone;
    public UnityEvent OnHeadshoot = new UnityEvent();

    private void OnEnable()
    {
        _hitbox.OnDamagedEvent.AddListener(DoHeadshoot);
    }

    private void OnDisable()
    {
        _hitbox.OnDamagedEvent.RemoveListener(DoHeadshoot);
    }

    private void DoHeadshoot(int damage)
    {
        _scaleBone.localScale = Vector3.zero;
    }
}
