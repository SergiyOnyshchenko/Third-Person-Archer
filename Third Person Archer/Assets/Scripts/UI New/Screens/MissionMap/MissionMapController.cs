#nullable enable
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    [RequireComponent(typeof(ScreenView))]
    public sealed class MissionMapController : MonoBehaviour, IReceivesArgs<MissionMapArgs>
    {
        [SerializeField] private Text _title = null!;
        [SerializeField] private Button _openConfirm = null!;

        private int _zoneId;
        private bool _highlight;

        private void Awake()
        {
            _openConfirm.onClick.AddListener(() =>
            {
                var nav = ServiceLocator.Resolve<IUINavigator>();
                nav.ShowModal("ConfirmDialog", new ConfirmDialogArgs
                {
                    Title = "Start Mission?",
                    Message = $"Start mission in Zone {_zoneId}?",
                    ConfirmText = "Start",
                    CancelText = "Cancel",
                    OnConfirm = () =>
                    {
                        nav.ShowOverlay("LoadingOverlay");
                        // Simulate finish:
                        Invoke(nameof(HideLoading), 1.5f);
                    }
                });
            });
        }

        void HideLoading()
        {
            ServiceLocator.Resolve<IUINavigator>().HideOverlay("LoadingOverlay");
        }

        public bool ValidateArgs(MissionMapArgs args) => args.ZoneId > 0;
        public void ApplyArgs(MissionMapArgs args)
        {
            _zoneId = args.ZoneId;
            _highlight = args.HighlightAvailable;
            if (_title) _title.text = $"Mission Map – Zone {_zoneId}" + (_highlight ? " ★" : "");
        }
    }
}