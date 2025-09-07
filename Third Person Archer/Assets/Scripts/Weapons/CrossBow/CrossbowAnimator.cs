using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UI.HUD;

public class CrossbowAnimator : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator _weaponAnimator;
    [SerializeField] private Animator _handsAnimator;

    [Header("Hand Points")]
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;

    [Header("Reload Settings")]
    [SerializeField] private string _reloadStateName = "Reload"; // name of reload state in Animator
    [SerializeField] private int _reloadLayer = 0;               // layer index of reload animation
    private float _defaultReloadDuration;
    private float _currentReloadDuration;

    private Animator[] _animators;
    public Transform LeftHand => _leftHand;
    public Transform RightHand => _rightHand;

    public UnityAction OnShooted;
    public UnityAction OnNewArowSeted;
    public UnityAction OnReloaded;

    private void Awake()
    {
        _animators = new Animator[] { _weaponAnimator, _handsAnimator };

        // Cache default reload animation duration
        if (_weaponAnimator != null)
        {
            var clipInfo = _weaponAnimator.runtimeAnimatorController.animationClips;
            foreach (var clip in clipInfo)
            {
                if (clip.name == _reloadStateName)
                {
                    _defaultReloadDuration = clip.length;
                    break;
                }
            }
        }
    }

    private void OnEnable()
    {
        foreach (var animator in _animators)
            animator.SetFloat("ReloadMult", 1f); // default speed
    }

    public void Shoot(UnityAction onShooted)
    {
        OnShooted = onShooted;
        SetTrigger("Shoot");
    }

    public void Reload(UnityAction onArrowSet)
    {
        OnNewArowSeted = onArrowSet;

        // START: notify UI with known duration (fallback to default if needed)
        float duration = _currentReloadDuration > 0f ? _currentReloadDuration :
                         (_defaultReloadDuration > 0f ? _defaultReloadDuration : 0.01f);
        ReloadSignals.Start(duration);

        SetTrigger("Reload");
    }

    public void SendReloadEvent()
    {
        OnReloaded?.Invoke();
        ReloadSignals.End(); 
    }

    public void SetReloadDuration(float desiredDuration)
    {
        if (_defaultReloadDuration <= 0f)
        {
            Debug.LogWarning("Default reload duration not set or not found!");
            return;
        }

        float mult = _defaultReloadDuration / desiredDuration;
        _currentReloadDuration = desiredDuration; 

        foreach (var animator in _animators)
            animator.SetFloat("ReloadMult", mult);
    }

    public void SendShootEvent() => OnShooted?.Invoke();
    public void SendSetNewArrowEvent() => OnNewArowSeted?.Invoke();

    private void SetTrigger(string name)
    {
        foreach (var animator in _animators)
            animator.SetTrigger(name);
    }
}