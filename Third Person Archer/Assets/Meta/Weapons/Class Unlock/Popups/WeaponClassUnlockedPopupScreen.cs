#nullable enable
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Meta.Weapons.UI;

namespace Meta.Weapons
{
    public sealed class WeaponClassUnlockedPopupScreen : MonoBehaviour, IReceivesArgs<WeaponClassUnlockedPopupArgs>
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _titleText = null!;
        [SerializeField] private TextMeshProUGUI _nameText = null!;
        [SerializeField] private Image _iconImage = null!;
        [SerializeField] private Button _closeButton = null!;
        [SerializeField] private Button _goToButton = null!;

        [Header("Visuals")]
        [SerializeField] private WeaponClassIconLibrary _classIcons = null!;

        private WeaponClassUnlockedPopupArgs _args = null!;

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnClose);
            _goToButton.onClick.AddListener(OnGoTo);
        }

        public bool ValidateArgs(WeaponClassUnlockedPopupArgs args) => args != null;

        public void ApplyArgs(WeaponClassUnlockedPopupArgs args)
        {
            _args = args;

            _titleText.text = args.Title;
            _nameText.text = args.WeaponClass.ToString();

            _iconImage.sprite = (_classIcons != null && _classIcons.TryGet(args.WeaponClass, out var icon))
                ? icon
                : null;
        }

        private void OnClose()
        {
            var flow = ServiceLocator.Resolve<IWeaponUnlockPopupFlow>();
            flow.CloseAndContinue(this);
        }

        private void OnGoTo()
        {
            var flow = ServiceLocator.Resolve<IWeaponUnlockPopupFlow>();
            flow.GoToWeaponClass(this, _args.WeaponClass);
        }
    }
}