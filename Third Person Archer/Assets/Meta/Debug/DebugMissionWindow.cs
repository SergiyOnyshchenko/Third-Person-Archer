using System.Collections.Generic;
using Meta.Weapons;
using Meta.Economy;
using UnityEngine;

public sealed class DebugMissionWindow : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] private bool _show = false;

    [Header("Scaling (landscape)")]
    [SerializeField] private Vector2 _designResolution = new Vector2(1920, 1080);
    [SerializeField] private float _minScale = 1.2f;
    [SerializeField] private float _maxScale = 2.6f;

    [Header("Safe Margins (unscaled pixels)")]
    [SerializeField] private float _margin = 24f;
    [SerializeField] private float _topMargin = 24f;

    [Header("UI Size (unscaled)")]
    [SerializeField] private int _baseFontSize = 34;
    [SerializeField] private int _titleFontAdd = 10;
    [SerializeField] private float _buttonHeight = 72f;
    [SerializeField] private float _toggleHeight = 78f;
    [SerializeField] private float _rowSpacing = 12f;
    [SerializeField] private float _toggleGap = 16f;

    [Header("Scrollbars (unscaled)")]
    [SerializeField] private float _scrollbarSize = 46f;

    [Header("Colors")]
    [SerializeField] private Color _backgroundColor = new Color(0.10f, 0.10f, 0.10f, 1f);
    [SerializeField] private Color _panelColor = new Color(0.16f, 0.16f, 0.16f, 1f);

    [Header("Debug Options")]
    [SerializeField] private bool _easyDebugMode = true;

    [Header("Economy Debug")]
    [SerializeField] private int _addCashAmount = 100;
    [SerializeField] private int _addTokenAmount = 5;
    [SerializeField] private bool _economyFoldout = false;
    // Cached heights (unscaled) so scrollview can fill remaining space.
    [SerializeField] private float _economyRowHeight = 68f; // optional (just for spacing)

    public bool IsOpen => _show;

    public void Open()
    {
        ResetFoldouts();
        _show = true;
    }
    
    public void Close() => _show = false;
    public void Toggle() => _show = !_show;

    private Vector2 _scroll;
    private readonly Dictionary<int, bool> _zoneFoldout = new();
    private bool _campaignFoldout = false;
    private bool _sniperFoldout = false;
    private bool _bossFoldout = false;

    private GUIStyle _titleStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _panelStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _foldoutButtonStyle;
    private GUIStyle _smallStatusStyle;

    private Texture2D _bgTex;
    private Texture2D _panelTex;

    private void OnDestroy()
    {
        if (_bgTex != null) Destroy(_bgTex);
        if (_panelTex != null) Destroy(_panelTex);
    }

    private void OnGUI()
    {
        if (!_show)
            return;

        EnsureTextures();
        EnsureStyles();
        EnsureBigScrollbars();

        float scale = ComputeScale();

        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float w = Screen.width / scale;
        float h = Screen.height / scale;

        GUI.DrawTexture(new Rect(0, 0, w, h), _bgTex, ScaleMode.StretchToFill);

        Rect area = new Rect(_margin, _topMargin, w - _margin * 2f, h - _topMargin - _margin);
        GUI.DrawTexture(area, _panelTex, ScaleMode.StretchToFill);

        GUILayout.BeginArea(area, _panelStyle);
        DrawContent();
        GUILayout.EndArea();

        GUI.matrix = oldMatrix;
    }

    private float ComputeScale()
    {
        float sx = Screen.width / _designResolution.x;
        float sy = Screen.height / _designResolution.y;
        return Mathf.Clamp(Mathf.Min(sx, sy), _minScale, _maxScale);
    }

    private void EnsureTextures()
    {
        if (_bgTex == null)
            _bgTex = CreateSolidTex(_backgroundColor);

        if (_panelTex == null)
            _panelTex = CreateSolidTex(_panelColor);
    }

    private static Texture2D CreateSolidTex(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Point;
        return t;
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
            return;

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(18, 18, 18, 18)
        };

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = _baseFontSize + _titleFontAdd,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = _baseFontSize,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        };

        _smallStatusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(18, _baseFontSize - 8),
            alignment = TextAnchor.MiddleLeft
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = _baseFontSize,
            fixedHeight = _buttonHeight
        };

        _foldoutButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = _baseFontSize + 2,
            fixedHeight = _buttonHeight
        };
    }

    private void EnsureBigScrollbars()
    {
        GUI.skin.verticalScrollbar.fixedWidth = _scrollbarSize;
        GUI.skin.verticalScrollbarThumb.fixedWidth = _scrollbarSize;

        GUI.skin.horizontalScrollbar.fixedHeight = _scrollbarSize;
        GUI.skin.horizontalScrollbarThumb.fixedHeight = _scrollbarSize;

        GUI.skin.verticalScrollbarThumb.padding = new RectOffset(6, 6, 6, 6);
        GUI.skin.horizontalScrollbarThumb.padding = new RectOffset(6, 6, 6, 6);
    }

    private void DrawContent()
    {
        var rt = MainMenuRuntime.Instance;
        var services = rt != null ? rt.Services : null;

        // Top bar
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("DEBUG MISSIONS", _titleStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("CLOSE", _buttonStyle, GUILayout.Width(240), GUILayout.Height(_buttonHeight)))
            _show = false;
        GUILayout.EndHorizontal();

        GUILayout.Space(_rowSpacing);

        // We want: Economy + toggles + info fixed on top, and ONLY missions list scrolls.
        // So we measure remaining height and make the missions scrollview fill it.
        Rect remaining = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        // We will draw the top fixed header in a vertical layout, then compute how much height is left.
        // In IMGUI, easiest approach: draw header first, then start a scrollview with GUILayout.ExpandHeight(true).

        // ===== Economy foldout (collapsible) =====
        _economyFoldout = DrawFoldout("ECONOMY DEBUG", _economyFoldout, fullWidth: true);

        if (_economyFoldout)
        {
            DrawEconomyDebug();
            GUILayout.Space(_rowSpacing);
        }

        // ===== Debug options =====
        _easyDebugMode = DrawBigCenteredToggle("Easy Debug Mode", _easyDebugMode);

        GUILayout.Space(_rowSpacing);

        if (services == null)
        {
            GUILayout.Label("MainMenuRuntime.Services is null.", _labelStyle);
            return;
        }

        var progressData = services.ProgressData;
        var launchRequest = services.LaunchRequest;
        var loopData = services.LoopData;
        var catalog = services.Catalog;
        var weaponRequirement = services.WeaponRequirement;

        if (progressData == null || launchRequest == null || loopData == null || catalog == null || weaponRequirement == null)
        {
            GUILayout.Label("Missing required services on MainMenuServices:", _labelStyle);
            GUILayout.Label($"ProgressData={(progressData != null)} LaunchRequest={(launchRequest != null)} LoopData={(loopData != null)}", _labelStyle);
            GUILayout.Label($"Catalog={(catalog != null)} WeaponRequirement={(weaponRequirement != null)}", _labelStyle);
            return;
        }

        var zones = progressData.AllZones;
        if (zones == null || zones.Count == 0)
        {
            GUILayout.Label("No zones in MissionProgressData.", _labelStyle);
            return;
        }

        GUILayout.Label($"LoopIndex={loopData.CurrentLoopIndex} | Debug: Rewards ON, Progress OFF, Required Weapon Class ON", _labelStyle);

        GUILayout.Space(_rowSpacing);

        // ===== Missions list scroll fills the remaining space =====
        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

        for (int zi = 0; zi < zones.Count; zi++)
        {
            var z = zones[zi];
            if (z == null) continue;

            if (!_zoneFoldout.ContainsKey(zi))
                _zoneFoldout[zi] = false;

            _zoneFoldout[zi] = DrawFoldout($"ZONE {z.ID} (index={zi})", _zoneFoldout[zi], fullWidth: true);
            if (!_zoneFoldout[zi])
                continue;

            GUILayout.Space(_rowSpacing * 0.5f);

            DrawZoneSection(z, zi, catalog, weaponRequirement, launchRequest, loopData);

            GUILayout.Space(_rowSpacing * 1.2f);
        }

        GUILayout.EndScrollView();
    }

    private void DrawEconomyDebug()
    {
        // Economy is installed via EconomyInstaller. We access it through the static Economy.Wallet.
        // If your project uses a different access point, replace these calls.
        IWallet wallet = Economy.Wallet;

        GUILayout.Label("ECONOMY DEBUG", _titleStyle);

        if (wallet == null)
        {
            GUILayout.Label("Economy.Wallet is null (Economy not initialized in this scene).", _labelStyle);
            return;
        }

        DrawCurrencyRow(wallet, CurrencyType.Cash, "Coins", _addCashAmount);
        DrawCurrencyRow(wallet, CurrencyType.BowToken, "BowToken", _addTokenAmount);
        DrawCurrencyRow(wallet, CurrencyType.CrossbowToken, "CrossbowToken", _addTokenAmount);
        DrawCurrencyRow(wallet, CurrencyType.SpearToken, "SpearToken", _addTokenAmount);
        DrawCurrencyRow(wallet, CurrencyType.ShurikenToken, "ShurikenToken", _addTokenAmount);
        DrawCurrencyRow(wallet, CurrencyType.BoomerangToken, "BoomerangToken", _addTokenAmount);
    }

    private void DrawCurrencyRow(IWallet wallet, CurrencyType type, string label, int addAmount)
    {
        long amount = wallet.Get(type);

        GUILayout.BeginHorizontal();

        GUILayout.Label($"{label}: {amount}", _labelStyle, GUILayout.ExpandWidth(true));

        string btn = addAmount >= 0 ? $"+{addAmount}" : $"{addAmount}";
        if (GUILayout.Button(btn, _buttonStyle, GUILayout.Width(180), GUILayout.Height(_buttonHeight)))
        {
            wallet.Add(type, addAmount);
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(_rowSpacing * 0.25f);
    }

    private bool DrawBigCenteredToggle(string label, bool value)
    {
        Rect row = GUILayoutUtility.GetRect(0, _toggleHeight, GUILayout.ExpandWidth(true), GUILayout.Height(_toggleHeight));

        Rect box = new Rect(row.x, row.y, _toggleHeight, _toggleHeight);
        Rect labelRect = new Rect(box.xMax + _toggleGap, row.y, row.width - _toggleHeight - _toggleGap, _toggleHeight);

        string check = value ? "✓" : "";
        if (GUI.Button(box, check, _buttonStyle))
            value = !value;

        var centeredLabel = new GUIStyle(_labelStyle) { alignment = TextAnchor.MiddleCenter };
        if (GUI.Button(labelRect, label, centeredLabel))
            value = !value;

        return value;
    }

    private void DrawZoneSection(
        ZoneData zone,
        int zoneIndex,
        IMissionCatalogService catalog,
        IWeaponRequirementService weaponRequirement,
        MissionLaunchRequest launchRequest,
        MetaLoopProgressData loopData)
    {
        _campaignFoldout = DrawFoldout("CAMPAIGN", _campaignFoldout, fullWidth: false);
        if (_campaignFoldout)
            DrawSegmentButtons(zone, zoneIndex, MissionType.Campaign, catalog, weaponRequirement, launchRequest, loopData);

        GUILayout.Space(_rowSpacing * 0.4f);

        _sniperFoldout = DrawFoldout("SNIPER", _sniperFoldout, fullWidth: false);
        if (_sniperFoldout)
            DrawSegmentButtons(zone, zoneIndex, MissionType.Sniper, catalog, weaponRequirement, launchRequest, loopData);

        GUILayout.Space(_rowSpacing * 0.4f);

        _bossFoldout = DrawFoldout("BOSS", _bossFoldout, fullWidth: false);
        if (_bossFoldout)
            DrawSegmentButtons(zone, zoneIndex, MissionType.Boss, catalog, weaponRequirement, launchRequest, loopData);
    }

    private void DrawSegmentButtons(
        ZoneData zone,
        int zoneIndex,
        MissionType type,
        IMissionCatalogService catalog,
        IWeaponRequirementService weaponRequirement,
        MissionLaunchRequest launchRequest,
        MetaLoopProgressData loopData)
    {
        var seg = zone.GetSegmentByType(type);
        if (seg == null || seg.Missions == null || seg.Missions.Count == 0)
        {
            GUILayout.Label("  (no missions)", _labelStyle);
            return;
        }

        GUILayout.Space(_rowSpacing * 0.3f);

        for (int i = 0; i < seg.Missions.Count; i++)
        {
            var mission = seg.Missions[i];
            if (mission == null) continue;

            string scenePath = (mission.Scene != null) ? mission.Scene.ScenePath : "";
            bool hasScene = !string.IsNullOrEmpty(scenePath);

            GUILayout.BeginHorizontal();

            GUI.enabled = hasScene;

            string btnLabel = $"PLAY {type} [{i}]  {mission.name}";
            if (GUILayout.Button(btnLabel, _buttonStyle, GUILayout.Height(_buttonHeight)))
                LaunchDebugMission(zoneIndex, type, mission, catalog, weaponRequirement, launchRequest, loopData);

            GUI.enabled = true;

            GUILayout.Label(hasScene ? "OK" : "MISSING SCENE", _smallStatusStyle, GUILayout.Width(260));

            GUILayout.EndHorizontal();

            GUILayout.Space(_rowSpacing * 0.35f);
        }
    }

    private void LaunchDebugMission(
        int zoneIndex,
        MissionType type,
        MissionData mission,
        IMissionCatalogService catalog,
        IWeaponRequirementService weaponRequirement,
        MissionLaunchRequest launchRequest,
        MetaLoopProgressData loopData)
    {
        if (mission == null || mission.Scene == null || string.IsNullOrEmpty(mission.Scene.ScenePath))
            return;

        int loopIndex = loopData != null ? loopData.CurrentLoopIndex : 0;
        int balanceLoopIndex = Mathf.Clamp(loopIndex, 0, 2);

        int globalIndex = catalog.GetGlobalCampaignIndexOrMinusOne(mission);

        WeaponClass required = weaponRequirement.GetRequiredWeaponClass(mission, loopIndex);

        bool easyApplies = _easyDebugMode && type == MissionType.Campaign;

        launchRequest.Set(
            mode: type,
            missionToLoad: mission,
            requiredWeaponClass: required,
            zoneIndex: zoneIndex,
            globalCampaignIndex: globalIndex,
            loopIndex: loopIndex,
            balanceLoopIndex: balanceLoopIndex,
            isDebugRun: true,
            easyDebugMode: easyApplies
        );

        _show = false;
        ScenesLoader.Instance.LoadScene(mission.Scene.ScenePath);
    }

    private bool DrawFoldout(string label, bool value, bool fullWidth)
    {
        string txt = (value ? "▼ " : "► ") + label;

        if (fullWidth)
            return GUILayout.Toggle(value, txt, _foldoutButtonStyle, GUILayout.Height(_buttonHeight));

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        bool newValue = GUILayout.Toggle(value, txt, _foldoutButtonStyle, GUILayout.Height(_buttonHeight));
        GUILayout.EndHorizontal();
        return newValue;
    }

    private void ResetFoldouts()
    {
        _economyFoldout = false;

        _campaignFoldout = false;
        _sniperFoldout = false;
        _bossFoldout = false;

        _zoneFoldout.Clear();
    }
}