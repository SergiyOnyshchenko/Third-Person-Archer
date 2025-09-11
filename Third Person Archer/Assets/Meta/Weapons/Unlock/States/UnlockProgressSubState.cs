using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons.Unlocks;
using Meta.Weapons.Unlocks.UI;

public sealed class UnlockProgressSubState : SubState
{
    [Header("UI Root (Canvas or Panel to toggle)")]
    [SerializeField] private GameObject uiRoot; // <- stays inactive before/after; active only while substate runs

    [Header("Playback")]
    [SerializeField] private WeaponClassUnlockProgressPresenter presenter;
    [SerializeField] private WeaponClassUnlockProgressView view;

    public event System.Action Completed;

    private bool _playing;
    private IReadOnlyList<ClassProgressDelta> _pending; // set by Transition before Enter()

    public override void Enter()
    {
        base.Enter();

        if (uiRoot != null) uiRoot.SetActive(true);
        //if (view != null) view.SetVisible(false);

        if (presenter != null) presenter.Completed += OnPresenterCompleted;

        // If transition already gave us deltas, kick off now.
        if (_pending != null)
        {
            if (_pending.Count == 0) { _pending = null; Complete(); return; }

            _playing = true;

            // Ensure the presenter component can run coroutines
            if (presenter != null)
            {
                presenter.enabled = true;
                presenter.gameObject.SetActive(true);
                presenter.EnqueueResults(_pending);
            }
            _pending = null;
        }
    }

    public override void Exit()
    {
        if (presenter != null) presenter.Completed -= OnPresenterCompleted;

        _playing = false;
        _pending = null;

        if (view != null) view.SetVisible(false);
        if (uiRoot != null) uiRoot.SetActive(false);    // <-- keep UI inactive after state

        base.Exit();
    }

    /// <summary>
    /// Called by the Transition. Safe to call before Enter():
    /// we defer playback until Enter() to avoid subscription timing issues.
    /// </summary>
    public void Prepare(IReadOnlyList<ClassProgressDelta> deltas)
    {
        // If UI/presenter is not ready yet, just stash and Enter() will play them.
        _pending = deltas ?? new List<ClassProgressDelta>(0);

        // If the substate is already entered (rare), play immediately.
        if (isActiveAndEnabled && _pending.Count > 0 && presenter != null)
        {
            _playing = true;
            presenter.enabled = true;
            presenter.gameObject.SetActive(true);
            presenter.EnqueueResults(_pending);
            _pending = null;
        }
        else if (isActiveAndEnabled && _pending.Count == 0)
        {
            _pending = null;
            Complete();
        }
    }

    private void OnPresenterCompleted()
    {
        if (!_playing) return;
        _playing = false;
        Complete();
    }

    private void Complete()
    {
        // Let the Transition advance
        Completed?.Invoke();
    }
}