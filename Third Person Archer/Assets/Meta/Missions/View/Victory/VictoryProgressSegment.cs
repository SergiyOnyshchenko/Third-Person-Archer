using System;
using DG.Tweening;
using Meta.Economy;
using Meta.Weapons;
using Meta.Weapons.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "What this win brought me": essence of the played class filling toward its next level
/// (with an "Upgrade ready!" stamp).
/// Wallet values are read before the payout, so the bar shows before → after this mission.
/// </summary>
public sealed class VictoryProgressSegment : MonoBehaviour, IVictorySegment
{
    [SerializeField] private CanvasGroup _rootGroup;

    [Header("Class essence")]
    [SerializeField] private GameObject _classRoot;
    [SerializeField] private Image _classIcon;
    [SerializeField] private TMP_Text _classTitle;
    [SerializeField] private Image _essenceFill;
    [SerializeField] private Image _essenceGainFill;
    [SerializeField] private TMP_Text _essenceText;
    [SerializeField] private TMP_Text _classHint;
    [SerializeField] private GameObject _readyStamp;
    [SerializeField] private RectTransform _readyRays;

    [Header("Look")]
    [SerializeField] private WeaponClassIconLibrary _classIcons;
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private float _fillDuration = 0.8f;

    public event Action OnShown;

    private DG.Tweening.Sequence _seq;
    private Tween _raysTween;

    private WeaponClass _class;
    private bool _hasClass;
    private int _tokensBefore, _cashBefore;
    private int _costTokens, _costCash, _classLevel;
    private bool _atCap;

    public void Show(VictoryContext context, FinalRewardBundle rewards)
    {
        gameObject.SetActive(true);
        if (_rootGroup != null) _rootGroup.alpha = 0f;

        var runtime = GameplayRuntime.Instance;
        var ctx = runtime != null ? runtime.Context : null;

        _seq?.Kill();
        _seq = DOTween.Sequence().SetLink(gameObject);
        if (_rootGroup != null) _seq.Append(_rootGroup.DOFade(1f, _fadeDuration));

        SetupClass(ctx, runtime != null ? runtime.Balance : null, rewards);

        // Let the bars fill before the next segment (multiplier) starts.
        _seq.AppendInterval(_fillDuration * 0.6f);
        _seq.OnComplete(() => OnShown?.Invoke());
    }

    public void Skip()
    {
        _seq?.Complete();
    }

    /// <summary>Re-fills the essence bar after the reward multiplier.</summary>
    public void ApplyFinalValues(FinalRewardBundle rewards)
    {
        if (!_hasClass) return;
        RenderClass(rewards, animate: true, delay: 0f);
    }

    // ── Class essence ────────────────────────────────────────────────────────

    private void SetupClass(MissionContext ctx, BalanceConfig balance, FinalRewardBundle rewards)
    {
        _hasClass = false;
        if (_classRoot != null) _classRoot.SetActive(false);
        if (_readyStamp != null) _readyStamp.SetActive(false);
        _raysTween?.Kill();

        var wallet = Economy.Wallet;
        if (ctx == null || !ctx.IsValid || balance == null || balance.WeaponCatalog == null || wallet == null) return;

        _class = ctx.SelectedType == MissionType.Boss ? WeaponClass.Crossbow : ctx.RequiredWeaponClass;

        var repo = new WeaponRepository();
        var progression = new ClassProgressionService(repo, wallet, new EquipmentService(repo), new StatsService(), balance.WeaponCatalog);
        var upgrades = new UpgradeService(progression, wallet, balance.WeaponCatalog);

        if (progression.GetPowerDef(_class) == null) return;

        _classLevel = progression.GetClassLevel(_class);
        _atCap = !upgrades.TryGetUpgradeCost(_class, out _costCash, out _costTokens);
        _tokensBefore = wallet.Get(WeaponCurrencyUtility.GetTokenCurrency(_class));
        _cashBefore = wallet.Get(CurrencyType.Cash);
        _hasClass = true;

        if (_classRoot != null) _classRoot.SetActive(true);
        if (_classIcon != null && _classIcons != null && _classIcons.TryGet(_class, out var icon)) _classIcon.sprite = icon;
        if (_classTitle != null) _classTitle.text = $"{_class.ToString().ToUpperInvariant()}  ·  LV {_classLevel + 1}";

        RenderClass(rewards, animate: true, delay: _fadeDuration);
    }

    private void RenderClass(FinalRewardBundle rewards, bool animate, float delay)
    {
        var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(_class);
        int gained = 0;
        if (rewards?.finalCurrencies != null)
            foreach (var e in rewards.finalCurrencies)
                if (e.currency == tokenCurrency) gained += e.amount;

        int cashAfter = _cashBefore + (rewards != null ? rewards.finalCash : 0);
        int tokensAfter = _tokensBefore + gained;

        float before01 = _atCap || _costTokens <= 0 ? 1f : Mathf.Clamp01(_tokensBefore / (float)_costTokens);
        float after01 = _atCap || _costTokens <= 0 ? 1f : Mathf.Clamp01(tokensAfter / (float)_costTokens);

        bool ready = !_atCap && tokensAfter >= _costTokens && cashAfter >= _costCash;

        if (_essenceText != null)
            _essenceText.text = _atCap ? "Tier maxed" : $"{tokensAfter} / {_costTokens}";

        if (_classHint != null)
            _classHint.text = _atCap
                ? "Level cap of this tier: get the next tier in the Arsenal!"
                : ready
                    ? $"<color={MissionPreviewUtility.Hex(MissionPreviewUtility.Green)}>Upgrade ready!</color> LV {_classLevel + 1} → {_classLevel + 2}"
                    : tokensAfter < _costTokens
                        ? $"{_costTokens - tokensAfter} more essence to LV {_classLevel + 2}"
                        : $"{_costCash - cashAfter} more gold to LV {_classLevel + 2}";

        if (_essenceFill != null) _essenceFill.fillAmount = before01;
        if (_essenceGainFill != null)
        {
            _essenceGainFill.DOKill();
            if (!animate) _essenceGainFill.fillAmount = after01;
            else
            {
                // A multiplier re-render grows from the current fill, the first one from "before".
                if (delay > 0f) _essenceGainFill.fillAmount = before01;
                _essenceGainFill.DOFillAmount(after01, _fillDuration).SetDelay(delay).SetEase(Ease.OutCubic)
                    .SetLink(_essenceGainFill.gameObject);
            }
        }

        if (_readyStamp == null) return;

        bool wasShown = _readyStamp.activeSelf;
        _readyStamp.transform.DOKill();
        if (!ready)
        {
            _readyStamp.SetActive(false);
            return;
        }
        if (wasShown) return;

        var stamp = _readyStamp.transform;
        DOTween.Sequence().SetLink(_readyStamp)
            .AppendInterval(delay + _fillDuration)
            .AppendCallback(() =>
            {
                _readyStamp.SetActive(true);
                stamp.localScale = Vector3.one * 2.2f;
                StartRays();
            })
            .Append(stamp.DOScale(1f, 0.35f).SetEase(Ease.OutBack, 2f))
            .Append(stamp.DOPunchRotation(new Vector3(0, 0, 8f), 0.3f, 8, 0.5f));
    }

    private void StartRays()
    {
        if (_readyRays == null) return;
        _raysTween?.Kill();
        _readyRays.localRotation = Quaternion.identity;
        _raysTween = _readyRays.DOLocalRotate(new Vector3(0, 0, -360f), 12f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1).SetLink(_readyRays.gameObject);
    }

}
