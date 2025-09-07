using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.Unlocks.UI
{
    /// <summary>
    /// Drives the progress popup(s) after mission completion.
    /// Feed it with the deltas returned by IWeaponClassUnlockService.RegisterMissionComplete(missionId).
    /// It will show each delta sequentially with a short animation.
    /// </summary>
    public sealed class WeaponClassUnlockProgressPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponClassPresentationConfig presentation;
        [SerializeField] private WeaponClassUnlockProgressView view;

        [Header("Timings")]
        [SerializeField, Min(0f)] private float appearSeconds = 0.15f;
        [SerializeField, Min(0f)] private float progressAnimSeconds = 0.6f;
        [SerializeField, Min(0f)] private float holdSeconds = 1.0f;
        [SerializeField, Min(0f)] private float disappearSeconds = 0.15f;

        private readonly Queue<ClassProgressDelta> _queue = new();

        private bool _running;

        public void EnqueueResults(IReadOnlyList<ClassProgressDelta> deltas)
        {
            if (deltas == null || deltas.Count == 0) return;
            foreach (var d in deltas) _queue.Enqueue(d);
            if (!_running) StartCoroutine(RunQueue());
        }

        private IEnumerator RunQueue()
        {
            _running = true;
            view.SetVisible(false);

            while (_queue.Count > 0)
            {
                var d = _queue.Dequeue();

                // Header & colors
                if (!presentation.TryGet(d.After.Class, out var e))
                {
                    // Fallback if no presentation entry
                    e = new WeaponClassPresentationConfig.Entry
                    {
                        Class = d.After.Class,
                        DisplayName = d.After.Class.ToString(),
                        Icon = null,
                        Accent = Color.white,
                        ProgressColor = new Color(0.2f, 0.8f, 0.2f)
                    };
                }

                view.SetHeader(e.Icon, e.DisplayName, e.Accent, e.ProgressColor);

                // Start state (before)
                view.SetProgressInstant(d.Before.Current, d.Before.Required, d.Before.Unlocked);
                view.SetVisible(true);

                // (optional) simple fade in
                if (appearSeconds > 0f) yield return new WaitForSecondsRealtime(appearSeconds);

                // Animate progress
                if (d.After.Current != d.Before.Current || d.After.Unlocked != d.Before.Unlocked)
                {
                    view.PlayTick();
                    yield return view.AnimateProgress(d.Before.Current, d.After.Current, d.After.Required, progressAnimSeconds);
                }

                if (d.NewlyUnlocked)
                {
                    view.SetProgressInstant(d.After.Required, d.After.Required, true);
                    view.PlayUnlocked();
                }

                yield return new WaitForSecondsRealtime(holdSeconds);

                // Fade out/hide
                if (disappearSeconds > 0f) yield return new WaitForSecondsRealtime(disappearSeconds);
                view.SetVisible(false);
            }

            _running = false;
        }
    }
}
