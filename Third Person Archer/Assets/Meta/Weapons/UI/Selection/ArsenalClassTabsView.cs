using System;
using DG.Tweening;
using Meta.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Weapons.UI
{
    /// <summary>
    /// Arsenal on one screen: a tab per class on the weapon screen (level, essence to the next level,
    /// red dot when something can be bought). Tapping a tab switches the screen to that class.
    /// </summary>
    public sealed class ArsenalClassTabsView : MonoBehaviour
    {
        [SerializeField] private WeaponSelectionScreen _screen;
        [SerializeField] private ArsenalClassTabView[] _tabs = Array.Empty<ArsenalClassTabView>();

        private IClassProgressionService _progression;

        private void Awake()
        {
            foreach (var tab in _tabs)
            {
                if (tab == null) continue;
                var cls = tab.Class;
                tab.Bind(() => OnTabClicked(cls));
            }
        }

        private void OnEnable()
        {
            if (_screen != null) _screen.OnClassShown += OnClassShown;
            if (Meta.Economy.Economy.Wallet != null) Meta.Economy.Economy.Wallet.BalanceChanged += OnBalanceChanged;

            _progression = WeaponsInitializer.Instance != null ? WeaponsInitializer.Instance.Progression : null;
            if (_progression != null) _progression.OnProgressChanged += OnProgressChanged;

            Refresh();
        }

        private void OnDisable()
        {
            if (_screen != null) _screen.OnClassShown -= OnClassShown;
            if (Meta.Economy.Economy.Wallet != null) Meta.Economy.Economy.Wallet.BalanceChanged -= OnBalanceChanged;
            if (_progression != null) _progression.OnProgressChanged -= OnProgressChanged;
        }

        private void OnClassShown(WeaponClass cls) => Refresh();
        private void OnBalanceChanged(CurrencyType type, int balance, int delta) => Refresh();
        private void OnProgressChanged(WeaponClass cls) => Refresh();

        private void OnTabClicked(WeaponClass cls)
        {
            if (_screen == null || cls == _screen.CurrentClass || !ArsenalAttention.IsClassUnlocked(cls)) return;
            _screen.ShowForClass(cls, _screen.CampaignLevel);
        }

        public void Refresh()
        {
            var weapons = WeaponsInitializer.Instance;
            if (weapons == null || weapons.Progression == null || weapons.UpgradeService == null) return;

            var wallet = Meta.Economy.Economy.Wallet;
            var selected = _screen != null ? _screen.CurrentClass : WeaponClass.Bow;

            foreach (var tab in _tabs)
            {
                if (tab == null) continue;
                var cls = tab.Class;

                if (!ArsenalAttention.IsClassUnlocked(cls))
                {
                    tab.SetLocked();
                    continue;
                }

                int level = weapons.Progression.GetClassLevel(cls);
                float essence01 = 1f;
                if (weapons.UpgradeService.TryGetUpgradeCost(cls, out _, out int tokens) && tokens > 0 && wallet != null)
                    essence01 = Mathf.Clamp01(wallet.Get(WeaponCurrencyUtility.GetTokenCurrency(cls)) / (float)tokens);

                tab.SetData(level, essence01, ArsenalAttention.HasSomethingToBuy(cls), cls == selected);
            }
        }
    }

    [Serializable]
    public sealed class ArsenalClassTabView
    {
        public WeaponClass Class;
        public Button Button;
        public Image Background;
        public Image Icon;
        public TextMeshProUGUI LevelText;
        public Image EssenceFill;
        public GameObject AttentionDot;
        public GameObject LockedRoot;

        [Tooltip("Background tint: normal / selected.")]
        public Color NormalColor = new Color(0.82f, 0.72f, 0.58f);
        public Color SelectedColor = new Color(1f, 0.84f, 0.38f);

        private Tween _pulse;

        public void Bind(Action onClick)
        {
            if (Button == null) return;
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(() => onClick());
        }

        public void SetData(int level, float essence01, bool attention, bool selected)
        {
            if (LockedRoot != null) LockedRoot.SetActive(false);
            if (Button != null) Button.interactable = true;
            if (Icon != null) Icon.color = Color.white;
            if (LevelText != null) LevelText.text = $"LV {level + 1}"; // shown 1-based, like the level bar
            if (EssenceFill != null)
            {
                EssenceFill.gameObject.SetActive(true);
                EssenceFill.fillAmount = essence01;
            }
            if (Background != null) Background.color = selected ? SelectedColor : NormalColor;
            if (Button != null) Button.transform.localScale = selected ? Vector3.one * 1.06f : Vector3.one;

            SetAttention(attention);
        }

        public void SetLocked()
        {
            if (LockedRoot != null) LockedRoot.SetActive(true);
            if (Button != null)
            {
                Button.interactable = false;
                Button.transform.localScale = Vector3.one;
            }
            if (Icon != null) Icon.color = new Color(0.25f, 0.25f, 0.25f, 0.8f);
            if (LevelText != null) LevelText.text = "";
            if (EssenceFill != null) EssenceFill.gameObject.SetActive(false);
            if (Background != null) Background.color = NormalColor * 0.6f;
            SetAttention(false);
        }

        private void SetAttention(bool show)
        {
            if (AttentionDot == null) return;

            bool was = AttentionDot.activeSelf;
            AttentionDot.SetActive(show);
            if (show == was) return;

            _pulse?.Kill();
            AttentionDot.transform.localScale = Vector3.one;
            if (show)
                _pulse = AttentionDot.transform.DOScale(1.25f, 0.5f).SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine).SetLink(AttentionDot);
        }
    }
}
