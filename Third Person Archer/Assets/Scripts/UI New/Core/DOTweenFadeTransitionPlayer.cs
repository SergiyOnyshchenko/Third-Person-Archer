#nullable enable
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace UI.Core
{
    /// <summary>Fade / None transitions backed by DOTween.</summary>
    public sealed class DOTweenFadeTransitionPlayer : ITransitionPlayer
    {
        public IEnumerator PlayEnter(ScreenView view)
        {
            switch (view.EnterTransition)
            {
                case UITransition.None:
                    InstantShow(view); yield break;

                case UITransition.Fade:
                    view.CanvasGroup.interactable = false;
                    view.CanvasGroup.blocksRaycasts = false;
                    view.CanvasGroup.alpha = 0f;
                    view.gameObject.SetActive(true);
                    var tIn = view.CanvasGroup.DOFade(1f, view.EnterDuration);
                    yield return tIn.WaitForCompletion();
                    view.CanvasGroup.interactable = true;
                    view.CanvasGroup.blocksRaycasts = true;
                    yield break;
            }
        }

        public IEnumerator PlayExit(ScreenView view)
        {
            switch (view.ExitTransition)
            {
                case UITransition.None:
                    InstantHide(view); yield break;

                case UITransition.Fade:
                    view.CanvasGroup.interactable = false;
                    view.CanvasGroup.blocksRaycasts = false;
                    var tOut = view.CanvasGroup.DOFade(0f, view.ExitDuration);
                    yield return tOut.WaitForCompletion();
                    view.gameObject.SetActive(false);
                    yield break;
            }
        }

        public void InstantShow(ScreenView view)
        {
            view.gameObject.SetActive(true);
            view.CanvasGroup.alpha = 1f;
            view.CanvasGroup.interactable = true;
            view.CanvasGroup.blocksRaycasts = true;
        }

        public void InstantHide(ScreenView view)
        {
            view.CanvasGroup.alpha = 0f;
            view.CanvasGroup.interactable = false;
            view.CanvasGroup.blocksRaycasts = false;
            view.gameObject.SetActive(false);
        }
    }
}