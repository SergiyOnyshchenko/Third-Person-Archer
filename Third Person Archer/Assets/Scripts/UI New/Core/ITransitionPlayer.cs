#nullable enable
using System.Collections;
using UnityEngine;

namespace UI.Core
{
    public interface ITransitionPlayer
    {
        IEnumerator PlayEnter(ScreenView view);
        IEnumerator PlayExit(ScreenView view);
        void InstantShow(ScreenView view);
        void InstantHide(ScreenView view);
    }
}