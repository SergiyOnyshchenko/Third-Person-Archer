#nullable enable
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    [RequireComponent(typeof(ScreenView))]
    public sealed class LoadingOverlayController : MonoBehaviour
    {
        [SerializeField] private Text _label = null!;

        private void OnEnable()
        {
            if (_label) _label.text = "Loading...";
        }
    }
}
