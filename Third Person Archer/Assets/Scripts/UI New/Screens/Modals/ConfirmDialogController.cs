#nullable enable
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    [RequireComponent(typeof(ScreenView))]
    public sealed class ConfirmDialogController : MonoBehaviour, IReceivesArgs<ConfirmDialogArgs>
    {
        [SerializeField] private Text _title = null!;
        [SerializeField] private Text _message = null!;
        [SerializeField] private Button _confirm = null!;
        [SerializeField] private Button _cancel = null!;

        private ConfirmDialogArgs _args;

        private void Awake()
        {
            _confirm.onClick.AddListener(() =>
            {
                _args.OnConfirm?.Invoke();
                ServiceLocator.Resolve<IUINavigator>().CloseTopModal();
            });
            _cancel.onClick.AddListener(() =>
            {
                _args.OnCancel?.Invoke();
                ServiceLocator.Resolve<IUINavigator>().CloseTopModal();
            });
        }

        public bool ValidateArgs(ConfirmDialogArgs args) => true;
        public void ApplyArgs(ConfirmDialogArgs args)
        {
            _args = args;
            if (_title)   _title.text = args.Title;
            if (_message) _message.text = args.Message;
            if (_confirm) _confirm.GetComponentInChildren<Text>().text = args.ConfirmText ?? args.ConfirmText;
            if (_cancel)  _cancel.GetComponentInChildren<Text>().text  = args.CancelText;
        }
    }
}