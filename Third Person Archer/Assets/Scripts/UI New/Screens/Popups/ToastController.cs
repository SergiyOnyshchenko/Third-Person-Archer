#nullable enable
using System.Collections;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Screens
{
    [RequireComponent(typeof(ScreenView))]
    public sealed class ToastController : MonoBehaviour, IReceivesArgs<ToastArgs>
    {
        [SerializeField] private TextMeshProUGUI _text = null!;
        [SerializeField] private float _defaultDuration = 2f;
        private float _duration;

        private void OnEnable() => StartCoroutine(AutoClose());

        private IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(_duration > 0 ? _duration : _defaultDuration);
            Destroy(gameObject);
        }

        public bool ValidateArgs(ToastArgs args) => !string.IsNullOrWhiteSpace(args.Message);
        public void ApplyArgs(ToastArgs args)
        {
            if (_text) _text.text = args.Message;
            _duration = args.Duration;
        }
    }
}