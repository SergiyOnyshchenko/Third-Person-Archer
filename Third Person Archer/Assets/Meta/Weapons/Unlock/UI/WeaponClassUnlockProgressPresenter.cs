using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.Unlocks.UI
{
    public sealed class WeaponClassUnlockProgressPresenter : MonoBehaviour
    {
        [SerializeField] private WeaponClassPresentationConfig presentation;
        [SerializeField] private WeaponClassUnlockProgressView view;

        [Header("Timings (unscaled time)")]
        [SerializeField, Min(0f)] private float appearSeconds = 0.1f;
        [SerializeField, Min(0f)] private float progressAnimSeconds = 0.6f;
        [SerializeField, Min(0f)] private float holdSeconds = 1.0f;
        [SerializeField, Min(0f)] private float disappearSeconds = 0.1f;

        private readonly Queue<ClassProgressDelta> _queue = new();
        private Coroutine _runner;
        private bool _running;

        public bool IsRunning => _running;
        public event System.Action Completed;

        public void EnqueueResults(IReadOnlyList<ClassProgressDelta> deltas)
        {
            if (deltas == null || deltas.Count == 0)
            {
                // nothing to show; make sure any listener can continue
                if (!_running) Completed?.Invoke();
                return;
            }

            foreach (var d in deltas) _queue.Enqueue(d);

            if (!_running)
            {
                // ensure this presenter is enabled so coroutines run
                enabled = true;
                gameObject.SetActive(true);

                _runner = StartCoroutine(RunQueue());
            }
        }

        private IEnumerator RunQueue()
        {
            _running = true;
            try
            {
                if (view != null) view.SetVisible(false);

                while (_queue.Count > 0)
                {
                    var d = _queue.Dequeue();

                    // Header
                    WeaponClassPresentationConfig.Entry e;
                    if (presentation == null || !presentation.TryGet(d.After.Class, out e))
                    {
                        e = new WeaponClassPresentationConfig.Entry
                        {
                            Class = d.After.Class,
                            DisplayName = d.After.Class.ToString(),
                            Icon = null,
                            Accent = Color.white,
                            ProgressColor = new Color(0.2f, 0.8f, 0.2f)
                        };
                    }

                    if (view != null)
                    {
                        view.SetHeader(e.Icon, e.DisplayName, e.Accent, e.ProgressColor);
                        view.SetProgressInstant(d.Before.Current, d.Before.Required, d.Before.Unlocked);
                        view.SetVisible(true);
                    }

                    if (view != null && appearSeconds > 0f) yield return new WaitForSecondsRealtime(appearSeconds);

                    // Animate only if something actually changed
                    if (d.After.Current != d.Before.Current || d.After.Unlocked != d.Before.Unlocked)
                    {
                        if (view != null)
                        {
                            view.PlayTick();
                            yield return StartCoroutine(view.AnimateProgress(
                                d.Before.Current, d.After.Current, d.After.Required, progressAnimSeconds));
                        }
                    }

                    if (d.NewlyUnlocked)
                    {
                        if (view != null)
                        {
                            view.SetProgressInstant(d.After.Required, d.After.Required, true);
                            view.PlayUnlocked();
                        }
                    }

                    if (view != null && holdSeconds > 0f) yield return new WaitForSecondsRealtime(holdSeconds);
                    if (view != null && disappearSeconds > 0f) yield return new WaitForSecondsRealtime(disappearSeconds);

                    if (view != null) view.SetVisible(false);
                }
            }
            finally
            {
                _running = false;
                if (view != null) view.SetVisible(false);
                Completed?.Invoke();
            }
        }
    }
}
