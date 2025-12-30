#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Meta.Weapons;
using UnityEditor;
using UnityEngine;

public sealed class WeaponDefAutoGeneratorWindow : EditorWindow
{
    [Header("Inputs")]
    [SerializeField] private MissionProgressData _missionProgressData;
    [SerializeField] private BalanceConfig _balanceConfig;
    [SerializeField] private WeaponCatalog _weaponCatalog;

    [Header("Damage (anchored to gate)")]
    [Tooltip("BaseDamage = GateRequiredDamage * BaseFactor")]
    [SerializeField, Range(0.1f, 1.5f)] private float _baseDamageFactor = 0.85f;

    [Tooltip("MaxDamage = GateRequiredDamage * MaxFactor (non-starter weapons)")]
    [SerializeField, Range(1.0f, 3.0f)] private float _maxDamageFactor = 1.25f;

    [Tooltip("IMPORTANT: If your GateModule uses DamageCap, keeping this ON prevents weapon damage from plateauing at the cap.")]
    [SerializeField] private bool _ignoreGateDamageCapForWeaponGeneration = true;

    [Header("Starter weapon damage (UnlockAfterCampaignLevel==0)")]
    [Tooltip("If enabled, starter weapon floors are derived from BalanceConfig class damage multiplier: Floor = StarterFloorBase * ClassDamageMultiplier.")]
    [SerializeField] private bool _autoStarterFloorsFromBalance = true;

    [Tooltip("Base floor used for starter weapons before applying class multiplier (if Auto is enabled).")]
    [SerializeField] private float _starterFloorBase = 10f;

    [Tooltip("Manual per-class floors (used only if Auto is disabled).")]
    [SerializeField] private float[] _starterDamageFloorPerClass = { 10f, 10f, 10f, 10f, 10f };

    [Tooltip("If enabled, starter MaxDamage is computed from BaseDamage (instead of gate).")]
    [SerializeField] private bool _starterUseSeparateMaxRule = true;

    [Tooltip("Starter: MaxDamage = BaseDamage * StarterMaxMultiplier + StarterMaxAdd.")]
    [SerializeField, Range(1f, 5f)] private float _starterMaxMultiplier = 2.0f;

    [Tooltip("Starter: MaxDamage = BaseDamage * Mult + Add. (Add is applied after multiplier)")]
    [SerializeField] private float _starterMaxAdd = 0f;

    [Header("Purchase prices (deterministic)")]
    [Tooltip("Purchase cash = CashBase + CashPerLevel * UnlockAfterCampaignLevel. Starter weapons forced to 0.")]
    [SerializeField] private int _purchaseCashBase = 200;

    [SerializeField] private int _purchaseCashPerLevel = 35;

    [Tooltip("Purchase tokens = TokensBase + TokensPerLevel * UnlockAfterCampaignLevel. Starter weapons forced to 0.")]
    [SerializeField] private int _purchaseTokensBase = 2;

    [SerializeField] private int _purchaseTokensPerLevel = 1;

    [Header("Upgrade prices (per weapon, based on rewards at unlock)")]
    [Tooltip("Share of per-mission income that should go into upgrades (rest is for purchases).")]
    [SerializeField, Range(0f, 1f)] private float _upgradeBudgetShare = 0.65f;

    [Tooltip("Upgrade start cost = AvgCost * this factor.")]
    [SerializeField, Range(0.2f, 1.0f)] private float _upgradeStartFactor = 0.7f;

    [Tooltip("Upgrade end cost = AvgCost * this factor.")]
    [SerializeField, Range(1.0f, 2.5f)] private float _upgradeEndFactor = 1.4f;

    [SerializeField] private AnimationCurve _upgradePriceCurve01 = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool _allowAdUpgrade = true;

    [Header("Filtering")]
    [SerializeField] private bool _onlySelectedWeaponDefs = false;

    [Header("Debug")]
    [SerializeField] private bool _logDamageInputs = false;

    [MenuItem("Tools/Balance/Weapon Def Auto Generator")]
    public static void Open() => GetWindow<WeaponDefAutoGeneratorWindow>("WeaponDef Generator");

    private void OnGUI()
    {
        EditorGUILayout.Space(6);

        _missionProgressData = (MissionProgressData)EditorGUILayout.ObjectField("MissionProgressData", _missionProgressData, typeof(MissionProgressData), false);
        _balanceConfig = (BalanceConfig)EditorGUILayout.ObjectField("BalanceConfig", _balanceConfig, typeof(BalanceConfig), false);
        _weaponCatalog = (WeaponCatalog)EditorGUILayout.ObjectField("WeaponCatalog", _weaponCatalog, typeof(WeaponCatalog), false);

        EditorGUILayout.Space(10);
        DrawHeader("Damage");
        _baseDamageFactor = EditorGUILayout.Slider("Base Damage Factor", _baseDamageFactor, 0.1f, 1.5f);
        _maxDamageFactor = EditorGUILayout.Slider("Max Damage Factor (non-starters)", _maxDamageFactor, 1.0f, 3.0f);
        _ignoreGateDamageCapForWeaponGeneration = EditorGUILayout.ToggleLeft("Ignore Gate DamageCap for weapon generation", _ignoreGateDamageCapForWeaponGeneration);

        EditorGUILayout.Space(8);
        DrawHeader("Starter weapon damage");
        _autoStarterFloorsFromBalance = EditorGUILayout.ToggleLeft("Auto starter floors from BalanceConfig class multiplier", _autoStarterFloorsFromBalance);

        using (new EditorGUI.DisabledScope(!_autoStarterFloorsFromBalance))
        {
            _starterFloorBase = EditorGUILayout.FloatField("Starter Floor Base", _starterFloorBase);
            if (_starterFloorBase < 0f) _starterFloorBase = 0f;
        }

        using (new EditorGUI.DisabledScope(_autoStarterFloorsFromBalance))
        {
            EditorGUILayout.LabelField("Manual Starter Damage Floors (per WeaponClass)", EditorStyles.miniBoldLabel);
            EnsureArraySize(ref _starterDamageFloorPerClass, Enum.GetValues(typeof(WeaponClass)).Length, 10f);
            for (int i = 0; i < _starterDamageFloorPerClass.Length; i++)
            {
                _starterDamageFloorPerClass[i] = EditorGUILayout.FloatField(((WeaponClass)i).ToString(), _starterDamageFloorPerClass[i]);
                if (_starterDamageFloorPerClass[i] < 0f) _starterDamageFloorPerClass[i] = 0f;
            }
        }

        EditorGUILayout.Space(6);
        _starterUseSeparateMaxRule = EditorGUILayout.ToggleLeft("Starter uses separate MaxDamage rule", _starterUseSeparateMaxRule);
        using (new EditorGUI.DisabledScope(!_starterUseSeparateMaxRule))
        {
            _starterMaxMultiplier = EditorGUILayout.Slider("Starter Max Multiplier", _starterMaxMultiplier, 1f, 5f);
            _starterMaxAdd = EditorGUILayout.FloatField("Starter Max Add", _starterMaxAdd);
            if (_starterMaxAdd < 0f) _starterMaxAdd = 0f;
        }

        EditorGUILayout.Space(10);
        DrawHeader("Purchase prices");
        _purchaseCashBase = EditorGUILayout.IntField("Cash Base", _purchaseCashBase);
        _purchaseCashPerLevel = EditorGUILayout.IntField("Cash Per Unlock Level", _purchaseCashPerLevel);
        _purchaseTokensBase = EditorGUILayout.IntField("Tokens Base", _purchaseTokensBase);
        _purchaseTokensPerLevel = EditorGUILayout.IntField("Tokens Per Unlock Level", _purchaseTokensPerLevel);

        EditorGUILayout.Space(10);
        DrawHeader("Upgrade prices (per weapon from rewards at unlock)");
        _upgradeBudgetShare = EditorGUILayout.Slider("Upgrade Budget Share", _upgradeBudgetShare, 0f, 1f);
        _upgradeStartFactor = EditorGUILayout.Slider("Upgrade Start Factor", _upgradeStartFactor, 0.2f, 1.0f);
        _upgradeEndFactor = EditorGUILayout.Slider("Upgrade End Factor", _upgradeEndFactor, 1.0f, 2.5f);
        _upgradePriceCurve01 = EditorGUILayout.CurveField("Upgrade Price Curve 0..1", _upgradePriceCurve01);
        _allowAdUpgrade = EditorGUILayout.Toggle("Allow Ad Upgrade", _allowAdUpgrade);

        EditorGUILayout.Space(10);
        _onlySelectedWeaponDefs = EditorGUILayout.ToggleLeft("Only Selected WeaponDef assets", _onlySelectedWeaponDefs);

        EditorGUILayout.Space(6);
        _logDamageInputs = EditorGUILayout.ToggleLeft("Log damage inputs (debug)", _logDamageInputs);

        EditorGUILayout.Space(16);

        using (new EditorGUI.DisabledScope(_missionProgressData == null || _balanceConfig == null || _weaponCatalog == null))
        {
            if (GUILayout.Button("GENERATE", GUILayout.Height(40)))
                Generate();
        }

        if (_missionProgressData == null || _balanceConfig == null || _weaponCatalog == null)
            EditorGUILayout.HelpBox("Assign MissionProgressData, BalanceConfig, and WeaponCatalog.", MessageType.Info);
    }

    private static void DrawHeader(string title) => EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

    private void Generate()
    {
        try
        {
            ValidateAndGenerate();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WeaponDef generator crashed:\n{ex}");
        }
    }

    private void ValidateAndGenerate()
    {
        var defs = _weaponCatalog.All?.Where(d => d != null).ToList();
        if (defs == null || defs.Count == 0)
        {
            Debug.LogError("WeaponDef generator: WeaponCatalog.All is empty.");
            return;
        }

        if (_onlySelectedWeaponDefs)
        {
            var selected = new HashSet<WeaponDef>(Selection.objects.OfType<WeaponDef>());
            defs = defs.Where(d => selected.Contains(d)).ToList();
            if (defs.Count == 0)
            {
                Debug.LogWarning("WeaponDef generator: Only Selected enabled, but no WeaponDef assets are selected.");
                return;
            }
        }

        BuildCampaignLoopMap(_missionProgressData, out int campaignsPerLoop, out int[] zoneStarts, out int[] zoneCounts);
        if (campaignsPerLoop <= 0)
        {
            Debug.LogError("WeaponDef generator: campaignsPerLoop == 0. Check ZoneData campaign segments.");
            return;
        }

        Debug.Log($"WeaponDef generator: Starting. Weapons={defs.Count}, campaignsPerLoop={campaignsPerLoop}");

        int changed = 0;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Generate WeaponDef Balance");

        foreach (var def in defs)
        {
            if (def == null) continue;

            bool isStarter = def.UnlockAfterCampaignLevel <= 0;

            int unlockLevel = Mathf.Max(0, def.UnlockAfterCampaignLevel);
            int globalIndexTotal = isStarter ? 0 : Mathf.Max(0, unlockLevel - 1);

            int loopIndex = globalIndexTotal / campaignsPerLoop;
            int indexInLoop = globalIndexTotal % campaignsPerLoop;
            int zoneIndex = ResolveZoneIndex(indexInLoop, zoneStarts, zoneCounts);

            // THIS IS THE FIX:
            // - Use UNCAPPPED gate for weapon generation (so weapons don't plateau at DamageCap).
            // - Still include loop multiplier so later loops scale.
            float gateForWeapon;
            if (_ignoreGateDamageCapForWeaponGeneration)
                gateForWeapon = GetRequiredDamageForCampaign_Uncapped(_balanceConfig, def.Class, globalIndexTotal, loopIndex);
            else
                gateForWeapon = _balanceConfig.GetRequiredDamageForCampaign(def.Class, globalIndexTotal, loopIndex);

            float floor = isStarter ? GetStarterFloor(def.Class) : 0f;

            float baseDamage = Mathf.Max(floor, gateForWeapon * _baseDamageFactor);

            float maxDamage;
            if (isStarter && _starterUseSeparateMaxRule)
                maxDamage = baseDamage * Mathf.Max(1f, _starterMaxMultiplier) + Mathf.Max(0f, _starterMaxAdd);
            else
                maxDamage = Mathf.Max(baseDamage, gateForWeapon * _maxDamageFactor);

            baseDamage = RoundDamageNice(baseDamage);
            maxDamage = RoundDamageNice(maxDamage);
            if (maxDamage < baseDamage) maxDamage = baseDamage;

            if (_logDamageInputs)
            {
                float gateCapped = _balanceConfig.GetRequiredDamageForCampaign(def.Class, globalIndexTotal, loopIndex);
                Debug.Log(
                    $"[GEN] {def.name} | Class={def.Class} | Unlock={def.UnlockAfterCampaignLevel} | GlobalIdx={globalIndexTotal} | " +
                    $"Loop={loopIndex} | InLoop={indexInLoop} | Zone={zoneIndex} | GateUsed={( _ignoreGateDamageCapForWeaponGeneration ? "UNCAPPED" : "CAPPED")}={gateForWeapon:F2} | " +
                    $"GateCapped={gateCapped:F2} | Base={baseDamage:F2} | Max={maxDamage:F2}",
                    def);
            }

            int purchaseCash, purchaseTokens;
            if (isStarter)
            {
                purchaseCash = 0;
                purchaseTokens = 0;
            }
            else
            {
                purchaseCash = NiceRoundMoney(_purchaseCashBase + _purchaseCashPerLevel * unlockLevel);
                purchaseTokens = NiceRoundTokens(_purchaseTokensBase + _purchaseTokensPerLevel * unlockLevel);
            }

            // Upgrade prices anchor to rewards at same "position in loop"
            var reward = _balanceConfig.GetMissionReward(MissionType.Campaign, indexInLoop, def.Class, loopIndex);

            int cashPerMission = Mathf.Max(0, reward.Cash);
            int tokensPerMission = ExtractTokenForClass(reward.TokensPerType, def.Class);

            float avgUpgradeCash = cashPerMission * _upgradeBudgetShare;
            float avgUpgradeTokens = tokensPerMission * _upgradeBudgetShare;

            int upCashStart = NiceRoundMoney(Mathf.Max(10f, avgUpgradeCash * _upgradeStartFactor));
            int upCashEnd = NiceRoundMoney(Mathf.Max(10f, avgUpgradeCash * _upgradeEndFactor));
            if (upCashEnd < upCashStart) upCashEnd = upCashStart;

            int upTokStart = NiceRoundTokens(Mathf.Max(0f, avgUpgradeTokens * _upgradeStartFactor));
            int upTokEnd = NiceRoundTokens(Mathf.Max(0f, avgUpgradeTokens * _upgradeEndFactor));
            if (upTokEnd < upTokStart) upTokEnd = upTokStart;

            bool didChange = ApplyToWeaponDef(
                def,
                baseDamage,
                maxDamage,
                purchaseCash,
                purchaseTokens,
                upCashStart,
                upCashEnd,
                upTokStart,
                upTokEnd,
                _upgradePriceCurve01,
                _allowAdUpgrade);

            if (didChange) changed++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"WeaponDef generator: Done. Changed weapons: {changed}/{defs.Count}");
    }

    // ------------------------------------------------------------
    // Gate (UNCAPPED) computation via reflection
    // ------------------------------------------------------------
    private static float GetRequiredDamageForCampaign_Uncapped(BalanceConfig cfg, WeaponClass cls, int globalIndex, int loopIndex)
    {
        if (cfg == null) return 0f;

        try
        {
            // BalanceConfig has: [SerializeField] private GateModule _gates;
            var gatesField = typeof(BalanceConfig).GetField("_gates", BindingFlags.Instance | BindingFlags.NonPublic);
            if (gatesField == null) return cfg.GetRequiredDamageForCampaign(cls, globalIndex, loopIndex);

            object gates = gatesField.GetValue(cfg);
            if (gates == null) return cfg.GetRequiredDamageForCampaign(cls, globalIndex, loopIndex);

            // GateModule has: [SerializeField] private WeaponGateProfile[] _weaponGateProfiles;
            var profilesField = gates.GetType().GetField("_weaponGateProfiles", BindingFlags.Instance | BindingFlags.NonPublic);
            if (profilesField == null) return cfg.GetRequiredDamageForCampaign(cls, globalIndex, loopIndex);

            var profiles = profilesField.GetValue(gates) as Array;
            if (profiles == null) return cfg.GetRequiredDamageForCampaign(cls, globalIndex, loopIndex);

            object profile = null;
            foreach (var p in profiles)
            {
                if (p == null) continue;
                var classField = p.GetType().GetField("Class", BindingFlags.Instance | BindingFlags.Public);
                if (classField == null) continue;

                var pClass = (WeaponClass)classField.GetValue(p);
                if (pClass == cls)
                {
                    profile = p;
                    break;
                }
            }

            if (profile == null) return cfg.GetRequiredDamageForCampaign(cls, globalIndex, loopIndex);

            float baseDamage = ReadFloat(profile, "BaseDamage", 0f);
            float missionGrowth = ReadFloat(profile, "MissionGrowth", 0f);
            var curve = ReadCurve(profile, "AdditionalCurve");

            float value = baseDamage + missionGrowth * Mathf.Max(0, globalIndex);

            if (curve != null)
                value *= Mathf.Max(0.01f, curve.Evaluate(globalIndex));

            // IMPORTANT: do NOT apply DamageCap here.

            // Apply loop multiplier (same way BalanceConfig wrapper does)
            float loopMult = cfg.GetLoopMultipliers(loopIndex).GateDamage;
            value *= loopMult;

            return Mathf.Max(0f, value);
        }
        catch
        {
            // fallback
            return cfg.GetRequiredDamageForCampaign(cls, globalIndex, loopIndex);
        }
    }

    private static float ReadFloat(object obj, string fieldName, float fallback)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        if (f == null) return fallback;
        var v = f.GetValue(obj);
        return v is float ff ? ff : fallback;
    }

    private static AnimationCurve ReadCurve(object obj, string fieldName)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        if (f == null) return null;
        return f.GetValue(obj) as AnimationCurve;
    }

    // ------------------------------------------------------------
    // Starter floor
    // ------------------------------------------------------------
    private float GetStarterFloor(WeaponClass cls)
    {
        if (_autoStarterFloorsFromBalance && _balanceConfig != null)
        {
            float mult = 1f;
            try { mult = Mathf.Max(0.01f, _balanceConfig.GetWeaponClassDamageMultiplier(cls)); }
            catch { mult = 1f; }

            float v = Mathf.Max(0f, _starterFloorBase) * mult;
            return RoundDamageNice(v);
        }

        EnsureArraySize(ref _starterDamageFloorPerClass, Enum.GetValues(typeof(WeaponClass)).Length, 10f);
        int idx = (int)cls;
        if (idx < 0 || idx >= _starterDamageFloorPerClass.Length) return RoundDamageNice(_starterFloorBase);
        return RoundDamageNice(Mathf.Max(0f, _starterDamageFloorPerClass[idx]));
    }

    private static int ExtractTokenForClass(int[] tokensPerType, WeaponClass wc)
    {
        if (tokensPerType == null) return 0;
        int idx = (int)wc;
        if (idx < 0 || idx >= tokensPerType.Length) return 0;
        return Mathf.Max(0, tokensPerType[idx]);
    }

    // ------------------------------------------------------------
    // Apply changes
    // ------------------------------------------------------------
    private static bool ApplyToWeaponDef(
        WeaponDef def,
        float baseDamage,
        float maxDamage,
        int purchaseCash,
        int purchaseTokens,
        int upgradeCashStart,
        int upgradeCashEnd,
        int upgradeTokensStart,
        int upgradeTokensEnd,
        AnimationCurve priceCurve01,
        bool canUpgradeWithAd)
    {
        if (def == null) return false;

        EnsureUpgradeProfileInstance(def);

        var so = new SerializedObject(def);
        so.Update();

        bool changed = false;

        changed |= SetFloat(so, "_baseStats.Damage", baseDamage);
        changed |= SetFloat(so, "_maxStats.Damage", maxDamage);

        changed |= SetInt(so, "_purchaseCash", purchaseCash);
        changed |= SetInt(so, "_purchaseTokens", purchaseTokens);

        var profileProp = so.FindProperty("_upgradePriceProfile");
        if (profileProp != null)
        {
            changed |= SetInt(profileProp, "_startUpgradeCash", upgradeCashStart);
            changed |= SetInt(profileProp, "_maxUpgradeCash", upgradeCashEnd);
            changed |= SetInt(profileProp, "_startUpgradeTokens", upgradeTokensStart);
            changed |= SetInt(profileProp, "_maxUpgradeTokens", upgradeTokensEnd);

            var curveProp = profileProp.FindPropertyRelative("_priceCurve01");
            if (curveProp != null)
            {
                if (!CurvesEqual(curveProp.animationCurveValue, priceCurve01))
                {
                    curveProp.animationCurveValue = priceCurve01;
                    changed = true;
                }
            }

            var adProp = profileProp.FindPropertyRelative("_canUpgradeWithAd");
            if (adProp != null && adProp.boolValue != canUpgradeWithAd)
            {
                adProp.boolValue = canUpgradeWithAd;
                changed = true;
            }
        }
        else
        {
            Debug.LogWarning($"WeaponDef generator: Can't find _upgradePriceProfile on {def.name}.", def);
        }

        if (!changed)
            return false;

        Undo.RecordObject(def, "Generate WeaponDef Balance");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(def);
        return true;
    }

    private static void EnsureUpgradeProfileInstance(WeaponDef def)
    {
        var field = typeof(WeaponDef).GetField("_upgradePriceProfile", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null) return;

        var current = field.GetValue(def) as UpgradePriceProfile;
        if (current != null) return;

        field.SetValue(def, new UpgradePriceProfile());
        EditorUtility.SetDirty(def);
    }

    // ------------------------------------------------------------
    // Campaign loop mapping
    // ------------------------------------------------------------
    private static void BuildCampaignLoopMap(MissionProgressData mpd, out int campaignsPerLoop, out int[] zoneStarts, out int[] zoneCounts)
    {
        var zones = mpd != null ? mpd.AllZones : null;
        zones ??= new List<ZoneData>();

        zoneStarts = new int[zones.Count];
        zoneCounts = new int[zones.Count];

        int running = 0;
        for (int i = 0; i < zones.Count; i++)
        {
            zoneStarts[i] = running;
            int c = CountCampaignMissionsInZone(zones[i]);
            zoneCounts[i] = c;
            running += c;
        }

        campaignsPerLoop = running;
    }

    private static int CountCampaignMissionsInZone(ZoneData zone)
    {
        if (zone == null) return 0;
        var seg = zone.GetSegmentByType(MissionType.Campaign);
        if (seg == null) return 0;
        return seg.Missions != null ? seg.Missions.Count : 0;
    }

    private static int ResolveZoneIndex(int indexInLoop, int[] zoneStarts, int[] zoneCounts)
    {
        for (int i = 0; i < zoneStarts.Length; i++)
        {
            int start = zoneStarts[i];
            int count = i < zoneCounts.Length ? zoneCounts[i] : 0;
            if (indexInLoop >= start && indexInLoop < start + count)
                return i;
        }
        return Mathf.Max(0, zoneStarts.Length - 1);
    }

    // ------------------------------------------------------------
    // Serialized helpers
    // ------------------------------------------------------------
    private static bool SetFloat(SerializedObject so, string path, float value)
    {
        var p = so.FindProperty(path);
        if (p == null)
        {
            Debug.LogWarning($"WeaponDef generator: Missing property '{path}'.");
            return false;
        }

        if (Mathf.Approximately(p.floatValue, value))
            return false;

        p.floatValue = value;
        return true;
    }

    private static bool SetInt(SerializedObject so, string path, int value)
    {
        var p = so.FindProperty(path);
        if (p == null)
        {
            Debug.LogWarning($"WeaponDef generator: Missing property '{path}'.");
            return false;
        }

        value = Mathf.Max(0, value);
        if (p.intValue == value)
            return false;

        p.intValue = value;
        return true;
    }

    private static bool SetInt(SerializedProperty root, string relativeName, int value)
    {
        var p = root.FindPropertyRelative(relativeName);
        if (p == null) return false;

        value = Mathf.Max(0, value);
        if (p.intValue == value)
            return false;

        p.intValue = value;
        return true;
    }

    private static bool CurvesEqual(AnimationCurve a, AnimationCurve b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        if (a.length != b.length) return false;

        for (int i = 0; i < a.length; i++)
        {
            var ka = a.keys[i];
            var kb = b.keys[i];
            if (!Mathf.Approximately(ka.time, kb.time)) return false;
            if (!Mathf.Approximately(ka.value, kb.value)) return false;
            if (!Mathf.Approximately(ka.inTangent, kb.inTangent)) return false;
            if (!Mathf.Approximately(ka.outTangent, kb.outTangent)) return false;
        }
        return true;
    }

    // ------------------------------------------------------------
    // Rounding
    // ------------------------------------------------------------
    private static float RoundDamageNice(float value)
    {
        if (value <= 0f) return 0f;

        float abs = Mathf.Abs(value);
        float step =
            abs < 10f ? 0.5f :
            abs < 50f ? 1f :
            abs < 200f ? 5f :
            abs < 1000f ? 10f :
            25f;

        return Mathf.Round(value / step) * step;
    }

    private static int NiceRoundMoney(float value)
    {
        value = Mathf.Max(10f, value);

        float abs = Mathf.Abs(value);
        int step =
            abs < 100f ? 10 :
            abs < 500f ? 25 :
            abs < 2000f ? 50 :
            abs < 10000f ? 100 :
            abs < 50000f ? 250 :
            500;

        int rounded = Mathf.RoundToInt(value / step) * step;
        return Mathf.Max(10, rounded);
    }

    private static int NiceRoundTokens(float value)
    {
        value = Mathf.Max(0f, value);
        return Mathf.RoundToInt(value);
    }

    private static void EnsureArraySize<T>(ref T[] arr, int size, T fill)
    {
        if (arr != null && arr.Length == size) return;

        var newArr = new T[size];
        for (int i = 0; i < size; i++) newArr[i] = fill;

        if (arr != null)
        {
            for (int i = 0; i < Mathf.Min(arr.Length, newArr.Length); i++)
                newArr[i] = arr[i];
        }

        arr = newArr;
    }
}
#endif