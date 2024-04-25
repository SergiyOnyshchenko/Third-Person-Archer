using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CrossbowAnimator : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator _weaponAnimator;
    [SerializeField] private Animator _handsAnimator;
    [Header("Hand Points")]
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    private Animator[] _animators;
    public Transform LeftHand { get => _leftHand; }
    public Transform RightHand { get => _rightHand; }
    public UnityAction OnShooted;
    public UnityAction OnNewArowSeted;

    private void Awake()
    {
        _animators = new Animator[] { _weaponAnimator, _handsAnimator };
    }

    public void Shoot(UnityAction onShooted)
    {
        OnShooted = onShooted;
        SetTrigger("Shoot");
    }

    public void Reload(UnityAction onArrowSet)
    {
        OnNewArowSeted = onArrowSet;
        SetTrigger("Reload");
    }

    public void SendShootEvent()
    {
        OnShooted?.Invoke();
    }

    public void SendSetNewArrowEvent()
    {
        OnNewArowSeted?.Invoke();
    }

    private void SetTrigger(string name)
    {
        foreach (var animator in _animators)
            animator.SetTrigger(name);
    }
}
