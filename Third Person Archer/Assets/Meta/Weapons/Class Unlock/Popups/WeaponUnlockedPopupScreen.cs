#nullable enable
using System.Linq;
using Meta.Weapons.UI;
using TMPro;
using UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons
{
    public sealed class WeaponUnlockedPopupScreen : MonoBehaviour, IReceivesArgs<WeaponUnlockedPopupArgs>
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _titleText = null!;
        [SerializeField] private TextMeshProUGUI _nameText = null!;
        [SerializeField] private Image _iconImage = null!;
        [SerializeField] private Button _closeButton = null!;
        [SerializeField] private Button _goToButton = null!;

        [Header("Data")]
        [SerializeField] private WeaponCatalog _weaponCatalog = null!;

        private WeaponUnlockedPopupArgs _args = null!;

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnClose);
            _goToButton.onClick.AddListener(OnGoTo);
        }

        public bool ValidateArgs(WeaponUnlockedPopupArgs args) => args != null && !string.IsNullOrEmpty(args.WeaponId);

        public void ApplyArgs(WeaponUnlockedPopupArgs args)
        {
            _args = args;

            var def = _weaponCatalog.All.FirstOrDefault(w => w != null && w.Id == args.WeaponId);

            _titleText.text = args.Title;
            _nameText.text = def != null ? def.DisplayName : args.WeaponId;

            // Requires WeaponDef.Icon (Sprite). If you don't have it, set icon null.
            _iconImage.sprite = def != null ? def.Icon : null;
        }

        private void OnClose()
        {
            var flow = ServiceLocator.Resolve<IWeaponUnlockPopupFlow>();
            flow.CloseAndContinue(this);
        }

        private void OnGoTo()
        {
            var flow = ServiceLocator.Resolve<IWeaponUnlockPopupFlow>();
            flow.GoToWeapon(this, _args.WeaponId);
        }
    }
}