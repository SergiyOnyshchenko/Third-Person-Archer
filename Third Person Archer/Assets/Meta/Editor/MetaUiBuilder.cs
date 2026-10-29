#if UNITY_EDITOR
using System.Collections.Generic;
using Meta.Economy;
using Meta.Weapons;
using Meta.Weapons.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the phase-5 meta UI into existing prefabs (idempotent: removes what it built before).
/// Map Screen: one path per zone (Campaign + Sniper levels), mission panel, gold chip, Play subtitle, Arsenal red dot.
/// Level Win: victory progress segment (class essence).
/// </summary>
public static class MetaUiBuilder
{
    private const string MapPrefab = "Assets/Scripts/UI New/Prefabs/Map Screen.prefab";
    private const string WinPrefab = "Assets/Prefabs/Level Flow/Level Win [State].prefab";
    private const string Kit = "Assets/UI/GUI PRO Kit - Fantasy RPG/ResourcesData/";
    private const string Art = "Assets/Meta/UI/Art/";

    private static readonly Color Ink = new Color(0.27f, 0.15f, 0.06f);
    private static readonly Color InkSoft = new Color(0.42f, 0.27f, 0.12f);

    // Path node positions per zone (map-center space, 1810x709 visible area): Campaign + Sniper levels, ~150 apart.
    private static readonly Vector2[][] ZonePoints =
    {
        new[] { new Vector2(-720, -50), new Vector2(-570, -40), new Vector2(-420, -70), new Vector2(-260, 30), new Vector2(-110, -40), new Vector2(30, -130), new Vector2(160, -40), new Vector2(310, -150), new Vector2(470, -120), new Vector2(500, 30), new Vector2(350, 40) },
        new[] { new Vector2(-780, 40), new Vector2(-640, -60), new Vector2(-480, -130), new Vector2(-330, -210), new Vector2(-180, -250), new Vector2(-30, -200), new Vector2(110, -150), new Vector2(230, -50), new Vector2(330, 80), new Vector2(360, 230), new Vector2(500, 140), new Vector2(600, 10), new Vector2(520, -120) },
        new[] { new Vector2(-800, -120), new Vector2(-660, -10), new Vector2(-500, -80), new Vector2(-350, -150), new Vector2(-200, -200), new Vector2(-40, -160), new Vector2(110, -160), new Vector2(250, -230), new Vector2(400, -150), new Vector2(450, -10), new Vector2(560, 100), new Vector2(640, -30), new Vector2(760, -120) },
    };

    [MenuItem("Tools/Meta/Build Phase 5 UI")]
    public static void BuildAll()
    {
        BuildMapScreen();
        BuildVictory();
        BuildArsenal();
        BuildHeroicPopup();
        AssetDatabase.SaveAssets();
        Debug.Log("MetaUiBuilder: phase 5 UI built.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Map screen
    // ═════════════════════════════════════════════════════════════════════════

    public static void BuildMapScreen()
    {
        var root = PrefabUtility.LoadPrefabContents(MapPrefab);
        try
        {
            foreach (var n in new[] { "Mission Panel", "Gold Chip" }) DestroyChild(root.transform, n);

            BuildGoldChip(root.transform);
            var panel = BuildMissionPanel(root.transform); // last: above the HUD

            for (int z = 0; z < 3; z++)
            {
                var map = root.transform.Find($"Maps/Mask/Map {z + 1}");
                if (map == null) continue;
                BuildCampaignPath(map, z, panel);

                foreach (var sel in map.GetComponentsInChildren<MissionSelectorButtonPresenter>(true))
                {
                    // Only the Boss landmark is left (Sniper levels are path nodes, the Guild Board is gone).
                    var so = new SerializedObject(sel);
                    so.FindProperty("_panel").objectReferenceValue = panel;
                    DestroyChild(sel.transform, "Badge");
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            BuildZoneBrowse(root.transform, panel);

            // Play button: "PLAY" + what it starts.
            var play = root.transform.Find("Buttons/Play Button");
            if (play != null)
            {
                var label = play.Find("Text_Play") as RectTransform;
                if (label != null) label.anchoredPosition = new Vector2(0, 14);
                DestroyChild(play, "Subtitle");
                var sub = Label(play, "Subtitle", "Level 1 · Bow", 28, Ink, TextAlignmentOptions.Center, outline: false);
                Place(sub.rectTransform, new Vector2(0, -36), new Vector2(420, 40));
                var view = play.GetComponent<PlayButtonView>();
                var so = new SerializedObject(view);
                so.FindProperty("_subtitle").objectReferenceValue = sub;

                // Power chip above the button: status dot + "damage / R".
                DestroyChild(play, "Power");
                var playRt = (RectTransform)play;
                var power = Chip(play, "Power", new Vector2(0, playRt.rect.height * 0.5f + 34), new Vector2(230, 56));
                var powerDot = Img(power, "Dot", Art + "map_path_dot.png", new Vector2(-82, 0), new Vector2(34, 34), MissionPreviewUtility.Green);
                powerDot.raycastTarget = false;
                var powerText = Label(power, "Text", "41 / 52", 30, Color.white, TextAlignmentOptions.Center, outline: true);
                Place(powerText.rectTransform, new Vector2(18, 0), new Vector2(170, 50));
                so.FindProperty("_powerRoot").objectReferenceValue = power.gameObject;
                so.FindProperty("_powerText").objectReferenceValue = powerText;
                so.FindProperty("_powerStatusGraphic").objectReferenceValue = powerDot;
                so.ApplyModifiedPropertiesWithoutUndo();

                var presenter = play.GetComponent<PlayMissionPresenter>();
                var pso = new SerializedObject(panel);
                pso.FindProperty("_playPresenter").objectReferenceValue = presenter;
                pso.ApplyModifiedPropertiesWithoutUndo();
            }

            // Arsenal button: red dot.
            var weapons = root.transform.Find("Buttons/Weapons Button");
            if (weapons != null)
            {
                DestroyChild(weapons, "Attention");
                var dotRt = Rect(weapons, "Attention", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-14, -14), new Vector2(54, 54));
                var dot = dotRt.gameObject.AddComponent<Image>();
                dot.sprite = Sprite(Art + "map_path_dot.png");
                dot.color = MissionPreviewUtility.Red;
                dot.raycastTarget = false;
                var excl = Label(dotRt, "Text", "!", 34, Color.white, TextAlignmentOptions.Center, outline: true);
                Stretch(excl.rectTransform, 0, 0);

                var badge = weapons.GetComponent<WeaponsAttentionBadge>() ?? weapons.gameObject.AddComponent<WeaponsAttentionBadge>();
                var so = new SerializedObject(badge);
                so.FindProperty("_dot").objectReferenceValue = dotRt.gameObject;
                so.ApplyModifiedPropertiesWithoutUndo();

                // Arsenal on one screen: open the weapon screen (class tabs) instead of the Loadout step.
                var open = weapons.GetComponent<UI.Widgets.OpenScreenButton>();
                if (open != null)
                {
                    var oso = new SerializedObject(open);
                    oso.FindProperty("_targetScreenId").stringValue = "";
                    oso.ApplyModifiedPropertiesWithoutUndo();
                }
                if (weapons.GetComponent<ArsenalOpenButton>() == null)
                    weapons.gameObject.AddComponent<ArsenalOpenButton>();
            }

            PrefabUtility.SaveAsPrefabAsset(root, MapPrefab);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void BuildCampaignPath(Transform map, int zone, MissionPanelView panel)
    {
        DestroyChild(map, "Campaign Path");

        var pathRt = Rect(map, "Campaign Path", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        pathRt.SetSiblingIndex(1); // above the map art, below the landmarks
        var view = pathRt.gameObject.AddComponent<CampaignPathView>();

        var points = Rect(pathRt, "Points", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        for (int i = 0; i < ZonePoints[zone].Length; i++)
            Rect(points, $"Point {i + 1}", Half, Half, ZonePoints[zone][i], new Vector2(20, 20));

        var dots = Rect(pathRt, "Dots", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var nodes = Rect(pathRt, "Nodes", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var dotRt = Rect(pathRt, "Dot Template", Half, Half, Vector2.zero, new Vector2(22, 22));
        var dot = dotRt.gameObject.AddComponent<Image>();
        dot.sprite = Sprite(Art + "map_path_dot.png");
        dot.raycastTarget = false;
        dotRt.gameObject.SetActive(false);

        var node = BuildNodeTemplate(pathRt);
        node.gameObject.SetActive(false);

        var boss = map.Find("Boss Button Variant") ?? map.Find("Boss Button Variant (1)");

        var so = new SerializedObject(view);
        so.FindProperty("_zoneIndex").intValue = zone;
        so.FindProperty("_pointsRoot").objectReferenceValue = points;
        so.FindProperty("_tailTarget").objectReferenceValue = boss as RectTransform;
        so.FindProperty("_nodeTemplate").objectReferenceValue = node;
        so.FindProperty("_dotTemplate").objectReferenceValue = dot;
        so.FindProperty("_dotsRoot").objectReferenceValue = dots;
        so.FindProperty("_nodesRoot").objectReferenceValue = nodes;
        so.FindProperty("_classIcons").objectReferenceValue = ClassIcons();
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Phase 6: zone arrows (existing Left/Right of Map Name View) + "HEROIC n/m" chip.</summary>
    private static void BuildZoneBrowse(Transform root, MissionPanelView panel)
    {
        var nameView = root.Find("Map Name View");
        if (nameView == null) return;

        DestroyChild(nameView, "Heroic Badge");
        var badge = Chip(nameView, "Heroic Badge", new Vector2(0, -78), new Vector2(380, 60));
        var badgeImage = badge.GetComponent<Image>();
        if (badgeImage != null) badgeImage.color = new Color(1f, 0.55f, 0.5f);
        var text = Label(badge, "Text", "HEROIC 0/7", 30, Color.white, TextAlignmentOptions.Center, outline: true);
        Stretch(text.rectTransform, 12, 4);
        badge.gameObject.SetActive(false);

        var view = nameView.GetComponent<ZoneBrowseView>() ?? nameView.gameObject.AddComponent<ZoneBrowseView>();
        var so = new SerializedObject(view);
        so.FindProperty("_left").objectReferenceValue = nameView.Find("Left")?.GetComponent<Button>();
        so.FindProperty("_right").objectReferenceValue = nameView.Find("Right")?.GetComponent<Button>();
        so.FindProperty("_heroicBadge").objectReferenceValue = badge.gameObject;
        so.FindProperty("_heroicText").objectReferenceValue = text;
        so.FindProperty("_mapsRoot").objectReferenceValue = root.Find("Maps/Mask") as RectTransform;
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private const string HeroicPopupPrefab = "Assets/Scripts/UI New/Prefabs/PopUps/Loop Transition Popup Screen.prefab";

    /// <summary>Phase 6: the old loop-transition popup now announces Heroic mode.</summary>
    [MenuItem("Tools/Meta/Build Phase 6 (Heroic)")]
    public static void BuildHeroic()
    {
        BuildHeroicPopup();
        BuildMapScreen();
        AssetDatabase.SaveAssets();
        Debug.Log("MetaUiBuilder: phase 6 (Heroic) UI built.");
    }

    private static void BuildHeroicPopup()
    {
        var root = PrefabUtility.LoadPrefabContents(HeroicPopupPrefab);
        try
        {
            var popup = root.GetComponentInChildren<LoopTransitionPopupController>(true);
            if (popup != null)
            {
                var so = new SerializedObject(popup);
                so.FindProperty("_titleString").stringValue = LoopTransitionPopupController.HeroicTitle;
                so.FindProperty("_bodyString").stringValue = LoopTransitionPopupController.HeroicBody;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, HeroicPopupPrefab);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static MapNodeView BuildNodeTemplate(Transform parent)
    {
        var rt = Rect(parent, "Node Template", Half, Half, Vector2.zero, new Vector2(118, 118));
        var hit = rt.gameObject.AddComponent<Image>();
        hit.color = new Color(1, 1, 1, 0); // raycast area
        var button = rt.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;

        var glow = Img(rt, "Glow", Art + "fx_soft_glow.png", new Vector2(0, 0), new Vector2(250, 250), new Color(1f, 0.8f, 0.35f, 0.85f));
        var ring = Img(rt, "Ring", Art + "fx_ring_glow.png", Vector2.zero, new Vector2(170, 170), new Color(1f, 0.92f, 0.6f, 1f));
        var medal = Img(rt, "Medallion", Art + "map_node_done.png", Vector2.zero, new Vector2(118, 118), Color.white);
        var number = Label(rt, "Number", "1", 46, Color.white, TextAlignmentOptions.Center, outline: true);
        Place(number.rectTransform, new Vector2(0, 2), new Vector2(100, 70));
        var icon = Img(rt, "Class Icon", null, new Vector2(0, 2), new Vector2(66, 66), Color.white);
        icon.preserveAspect = true;
        var check = Img(rt, "Check", Kit + "Sprites/Demo/Demo_Icon/icon_color_check_01_m.png", new Vector2(40, -38), new Vector2(50, 50), Color.white);
        check.preserveAspect = true;
        var lockImg = Img(rt, "Lock", Kit + "Sprites/Demo/Demo_Icon/icon_color_lock_s.png", new Vector2(38, -36), new Vector2(44, 44), Color.white);
        lockImg.preserveAspect = true;
        var status = Img(rt, "Status", Art + "map_path_dot.png", new Vector2(42, 42), new Vector2(34, 34), MissionPreviewUtility.Green);

        var view = rt.gameObject.AddComponent<MapNodeView>();
        var so = new SerializedObject(view);
        so.FindProperty("_button").objectReferenceValue = button;
        so.FindProperty("_medallion").objectReferenceValue = medal;
        so.FindProperty("_number").objectReferenceValue = number;
        so.FindProperty("_classIcon").objectReferenceValue = icon;
        so.FindProperty("_check").objectReferenceValue = check.gameObject;
        so.FindProperty("_lock").objectReferenceValue = lockImg.gameObject;
        so.FindProperty("_glow").objectReferenceValue = glow;
        so.FindProperty("_ring").objectReferenceValue = ring;
        so.FindProperty("_statusDot").objectReferenceValue = status;
        so.FindProperty("_completedSprite").objectReferenceValue = Sprite(Art + "map_node_done.png");
        so.FindProperty("_currentSprite").objectReferenceValue = Sprite(Art + "map_node_current.png");
        so.FindProperty("_lockedSprite").objectReferenceValue = Sprite(Art + "map_node_locked.png");
        so.FindProperty("_heroicCompletedSprite").objectReferenceValue = Sprite(Art + "map_node_heroic_done.png");
        so.FindProperty("_heroicCurrentSprite").objectReferenceValue = Sprite(Art + "map_node_heroic_current.png");
        so.ApplyModifiedPropertiesWithoutUndo();
        return view;
    }

    private static void BuildGoldChip(Transform root)
    {
        var chip = Chip(root, "Gold Chip", Vector2.zero, new Vector2(300, 92));
        chip.anchorMin = chip.anchorMax = new Vector2(1, 1);
        chip.anchoredPosition = new Vector2(-190, -64);

        var icon = Img(chip, "Icon", Kit + "Sprites/Component/Icon_ItemIcons_(Original)/icon_itemicon_coin.png", new Vector2(-104, 4), new Vector2(92, 92), Color.white);
        icon.preserveAspect = true;
        var amount = Label(chip, "Amount", "0", 44, Color.white, TextAlignmentOptions.Right, outline: true);
        Place(amount.rectTransform, new Vector2(30, 0), new Vector2(200, 70));

        var counter = chip.gameObject.AddComponent<CurrencyCounterView>();
        var so = new SerializedObject(counter);
        so.FindProperty("currency").enumValueIndex = (int)CurrencyType.Cash;
        so.FindProperty("amountText").objectReferenceValue = amount;
        so.FindProperty("iconImage").objectReferenceValue = icon;
        so.FindProperty("visuals").objectReferenceValue = CurrencyVisuals();
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static MissionPanelView BuildMissionPanel(Transform root)
    {
        var overlay = Rect(root, "Mission Panel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        overlay.SetAsLastSibling();
        var view = overlay.gameObject.AddComponent<MissionPanelView>();

        var content = Rect(overlay, "Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var dimRt = Rect(content, "Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var dim = dimRt.gameObject.AddComponent<Image>();
        dim.color = new Color(0.05f, 0.03f, 0.02f, 0.55f);
        var dimButton = dimRt.gameObject.AddComponent<Button>();
        dimButton.transition = Selectable.Transition.None;

        var panel = Rect(content, "Panel", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-40, -10), new Vector2(700, 980));
        panel.pivot = new Vector2(1, 0.5f);
        var bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = Sprite(Art + "ui_parchment_panel.png");
        bg.type = Image.Type.Sliced;

        // Header
        var ribbon = Img(panel, "Ribbon", Kit + "Sprites/Component/Label/label_ribbon_01.png", Vector2.zero, new Vector2(640, 128), Color.white);
        TopAnchor(ribbon.rectTransform, new Vector2(0, 20));
        var title = Label(ribbon.rectTransform, "Title", "LEVEL 1", 52, Color.white, TextAlignmentOptions.Center, outline: true);
        Stretch(title.rectTransform, 60, 18);
        title.rectTransform.offsetMin = new Vector2(60, 28);

        var close = Rect(panel, "Close", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-6, -6), new Vector2(92, 92));
        close.gameObject.AddComponent<Image>().sprite = Sprite(Kit + "Sprites/Component/Button/btn_circle_02.png");
        var closeButton = close.gameObject.AddComponent<Button>();
        Img(close, "Icon", Kit + "Sprites/Demo/Demo_Icon/icon_white_close.png", Vector2.zero, new Vector2(44, 44), Color.white);

        var subtitle = Label(panel, "Subtitle", "Zone · Mission", 32, InkSoft, TextAlignmentOptions.Center, outline: false);
        TopPlace(subtitle.rectTransform, -130, new Vector2(600, 46));

        // Weapon power
        var classIcon = Img(panel, "Class Icon", null, Vector2.zero, new Vector2(100, 100), Color.white);
        classIcon.preserveAspect = true;
        TopPlace(classIcon.rectTransform, -240, new Vector2(100, 100));
        classIcon.rectTransform.anchoredPosition = new Vector2(-230, -240);

        var classText = Label(panel, "Class", "BOW", 38, Ink, TextAlignmentOptions.Left, outline: false);
        TopPlace(classText.rectTransform, -196, new Vector2(400, 50));
        classText.rectTransform.anchoredPosition = new Vector2(70, -196);

        var track = Img(panel, "Power Bar", Art + "ui_bar_track.png", Vector2.zero, new Vector2(420, 50), Color.white);
        track.type = Image.Type.Sliced;
        TopPlace(track.rectTransform, -252, new Vector2(420, 50));
        track.rectTransform.anchoredPosition = new Vector2(70, -252);
        var fill = Img(track.rectTransform, "Fill", Art + "ui_bar_fill.png", Vector2.zero, Vector2.zero, MissionPreviewUtility.Green);
        Stretch(fill.rectTransform, 5, 5);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0.7f;
        var marker = Rect(track.rectTransform, "Min Marker", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6, 62));
        marker.gameObject.AddComponent<Image>().color = Ink;
        var powerText = Label(track.rectTransform, "Value", "32 / 40", 30, Color.white, TextAlignmentOptions.Center, outline: true);
        Stretch(powerText.rectTransform, 0, 0);
        var dmgCaption = Label(panel, "Power Caption", "POWER", 22, InkSoft, TextAlignmentOptions.Left, outline: false);
        TopPlace(dmgCaption.rectTransform, -290, new Vector2(420, 30));
        dmgCaption.rectTransform.anchoredPosition = new Vector2(70, -292);

        var status = Label(panel, "Status", "Ready! Your weapon is strong enough.", 29, Ink, TextAlignmentOptions.Center, outline: false);
        TopPlace(status.rectTransform, -366, new Vector2(600, 84));
        status.enableWordWrapping = true;

        var description = Label(panel, "Description", "", 30, Ink, TextAlignmentOptions.Center, outline: false);
        TopPlace(description.rectTransform, -500, new Vector2(580, 160));
        description.enableWordWrapping = true;

        // Reward
        var reward = Rect(panel, "Reward", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -712), new Vector2(600, 90));
        var rewardCaption = Label(reward, "Caption", "REWARD", 24, InkSoft, TextAlignmentOptions.Center, outline: false);
        Place(rewardCaption.rectTransform, new Vector2(0, 46), new Vector2(300, 30));
        var goldIcon = Img(reward, "Gold Icon", Kit + "Sprites/Component/Icon_ItemIcons_(Original)/icon_itemicon_coin.png", new Vector2(-250, -6), new Vector2(72, 72), Color.white);
        goldIcon.preserveAspect = true;
        var goldText = Label(reward, "Gold", "+100", 38, Color.white, TextAlignmentOptions.Left, outline: true);
        Place(goldText.rectTransform, new Vector2(-120, -6), new Vector2(180, 60));
        var essence = Rect(reward, "Essence", Half, Half, new Vector2(130, -6), new Vector2(340, 72));
        var essIcon = Img(essence, "Icon", null, new Vector2(-136, 0), new Vector2(64, 64), Color.white);
        essIcon.preserveAspect = true;
        var essText = Label(essence, "Text", "+6 Bow essence", 30, Color.white, TextAlignmentOptions.Left, outline: true);
        Place(essText.rectTransform, new Vector2(36, 0), new Vector2(270, 60));

        // Actions
        var upgrade = KitButton(panel, "Upgrade Button", "btn_rectangle_01_n_blue", "UPGRADE", new Vector2(-150, 86), new Vector2(280, 120), 38);
        var playBtn = KitButton(panel, "Play Button", "btn_rectangle_01_n_green", "PLAY", new Vector2(150, 86), new Vector2(300, 128), 50);
        var replay = KitButton(panel, "Replay Button", "btn_rectangle_01_n_yellow", "REPLAY", new Vector2(150, 86), new Vector2(300, 128), 44);
        foreach (var b in new[] { upgrade, playBtn, replay })
        {
            var r = (RectTransform)b.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0);
        }

        content.gameObject.SetActive(true);

        var so = new SerializedObject(view);
        so.FindProperty("_root").objectReferenceValue = content.gameObject;
        so.FindProperty("_dim").objectReferenceValue = dim;
        so.FindProperty("_dimButton").objectReferenceValue = dimButton;
        so.FindProperty("_panel").objectReferenceValue = panel;
        so.FindProperty("_closeButton").objectReferenceValue = closeButton;
        so.FindProperty("_titleText").objectReferenceValue = title;
        so.FindProperty("_subtitleText").objectReferenceValue = subtitle;
        so.FindProperty("_classIcon").objectReferenceValue = classIcon;
        so.FindProperty("_classText").objectReferenceValue = classText;
        so.FindProperty("_powerFill").objectReferenceValue = fill;
        so.FindProperty("_powerMinMarker").objectReferenceValue = marker;
        so.FindProperty("_powerText").objectReferenceValue = powerText;
        so.FindProperty("_statusText").objectReferenceValue = status;
        so.FindProperty("_descriptionText").objectReferenceValue = description;
        so.FindProperty("_rewardRoot").objectReferenceValue = reward.gameObject;
        so.FindProperty("_goldText").objectReferenceValue = goldText;
        so.FindProperty("_goldIcon").objectReferenceValue = goldIcon;
        so.FindProperty("_essenceRoot").objectReferenceValue = essence.gameObject;
        so.FindProperty("_essenceText").objectReferenceValue = essText;
        so.FindProperty("_essenceIcon").objectReferenceValue = essIcon;
        so.FindProperty("_playButton").objectReferenceValue = playBtn;
        so.FindProperty("_upgradeButton").objectReferenceValue = upgrade;
        so.FindProperty("_upgradeLabel").objectReferenceValue = upgrade.GetComponentInChildren<TMP_Text>(true);
        so.FindProperty("_replayButton").objectReferenceValue = replay;
        so.FindProperty("_classIcons").objectReferenceValue = ClassIcons();
        so.FindProperty("_currencyVisuals").objectReferenceValue = CurrencyVisuals();
        so.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Arsenal (loadout red dots, zone matchup line)
    // ═════════════════════════════════════════════════════════════════════════

    private const string LoadoutPrefab = "Assets/Scripts/UI New/Prefabs/Loadout Screen.prefab";
    private const string WeaponPrefab = "Assets/Scripts/UI New/Prefabs/Weapon Screen 1.prefab";

    public static void BuildArsenal()
    {
        var loadout = PrefabUtility.LoadPrefabContents(LoadoutPrefab);
        try
        {
            foreach (var slot in loadout.GetComponentsInChildren<LoadoutSlotView>(true))
            {
                DestroyChild(slot.transform, "Attention");
                var dotRt = Rect(slot.transform, "Attention", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-18, -18), new Vector2(50, 50));
                var dot = dotRt.gameObject.AddComponent<Image>();
                dot.sprite = Sprite(Art + "map_path_dot.png");
                dot.color = MissionPreviewUtility.Red;
                dot.raycastTarget = false;
                var excl = Label(dotRt, "Text", "!", 32, Color.white, TextAlignmentOptions.Center, outline: true);
                Stretch(excl.rectTransform, 0, 0);
                dotRt.gameObject.SetActive(false);

                var so = new SerializedObject(slot);
                so.FindProperty("_attentionDot").objectReferenceValue = dotRt.gameObject;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(loadout, LoadoutPrefab);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(loadout);
        }

        var weapon = PrefabUtility.LoadPrefabContents(WeaponPrefab);
        try
        {
            var screen = weapon.GetComponentInChildren<WeaponSelectionScreen>(true);
            if (screen != null)
            {
                var host = screen.transform;
                DestroyChild(host, "Matchup");
                // Over the 3D model, under the top bar (the model view sits between the Left and Right panels).
                var chip = Chip(host, "Matchup", Vector2.zero, new Vector2(840, 100));
                chip.anchorMin = chip.anchorMax = new Vector2(0.5f, 1);
                chip.anchoredPosition = new Vector2(0, -195);
                var rawImage = host.Find("RawImage");
                chip.SetSiblingIndex(rawImage != null ? rawImage.GetSiblingIndex() + 1 : host.childCount - 1);
                var text = Label(chip, "Text", "Japan · Level 6: Brawler 2 shots · 32 dmg = 1 shot", 28, Color.white, TextAlignmentOptions.Center, outline: true);
                Stretch(text.rectTransform, 20, 6);
                text.enableWordWrapping = true;

                var so = new SerializedObject(screen);
                so.FindProperty("_matchupRoot").objectReferenceValue = chip.gameObject;
                so.FindProperty("_matchupText").objectReferenceValue = text;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(weapon, WeaponPrefab);
            }
            else Debug.LogWarning("MetaUiBuilder: WeaponSelectionScreen not found in " + WeaponPrefab);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(weapon);
        }

        BuildArsenalTabs();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Tails: arsenal on one screen (class tabs)
    // ═════════════════════════════════════════════════════════════════════════

    private static readonly WeaponClass[] TabOrder =
        { WeaponClass.Crossbow, WeaponClass.Bow, WeaponClass.Shuriken, WeaponClass.Spear, WeaponClass.Boomerang };

    [MenuItem("Tools/Meta/Build Tails (Arsenal tabs, Map)")]
    public static void BuildTails()
    {
        BuildArsenalTabs();
        BuildMapScreen();
        AssetDatabase.SaveAssets();
        Debug.Log("MetaUiBuilder: arsenal tabs and map built.");
    }

    /// <summary>
    /// Class tabs in the weapon screen's top bar (the Loadout step is skipped: the map's Weapons button
    /// opens the weapon screen directly). Crossbow first, marked MAIN.
    /// </summary>
    private static void BuildArsenalTabs()
    {
        var root = PrefabUtility.LoadPrefabContents(WeaponPrefab);
        try
        {
            var screen = root.GetComponentInChildren<WeaponSelectionScreen>(true);
            var top = screen != null ? screen.transform.Find("Top") : null;
            if (top == null)
            {
                Debug.LogWarning("MetaUiBuilder: weapon screen top bar not found in " + WeaponPrefab);
                return;
            }

            DestroyChild(top, "Class Tabs");
            var strip = Rect(top, "Class Tabs", Half, Half, new Vector2(40, -2), new Vector2(1600, 110));
            var tabsView = strip.gameObject.AddComponent<ArsenalClassTabsView>();
            var icons = ClassIcons();

            const float w = 300, gap = 16;
            var tabs = new ArsenalClassTabView[TabOrder.Length];
            for (int i = 0; i < TabOrder.Length; i++)
            {
                var cls = TabOrder[i];
                float x = (i - (TabOrder.Length - 1) * 0.5f) * (w + gap);

                var rt = Rect(strip, "Tab " + cls, Half, Half, new Vector2(x, 0), new Vector2(w, 98));
                var bg = rt.gameObject.AddComponent<Image>();
                bg.sprite = Sprite(Art + "ui_wood_chip.png");
                bg.type = Image.Type.Sliced;
                var button = rt.gameObject.AddComponent<Button>();
                button.targetGraphic = bg;
                button.transition = Selectable.Transition.None;

                Sprite iconSprite = null;
                icons?.TryGet(cls, out iconSprite);
                var icon = Img(rt, "Icon", null, new Vector2(-98, 2), new Vector2(72, 72), Color.white);
                icon.sprite = iconSprite;
                icon.preserveAspect = true;

                var name = Label(rt, "Name", cls.ToString(), 30, Color.white, TextAlignmentOptions.Left, outline: true);
                Place(name.rectTransform, new Vector2(38, 20), new Vector2(190, 40));
                var level = Label(rt, "Level", "LV 1", 26, new Color(1f, 0.86f, 0.45f), TextAlignmentOptions.Left, outline: true);
                Place(level.rectTransform, new Vector2(38, -12), new Vector2(190, 34));

                var track = Img(rt, "Essence", Art + "ui_bar_track.png", new Vector2(38, -36), new Vector2(190, 16), Color.white);
                track.type = Image.Type.Sliced;
                var fill = Img(track.rectTransform, "Fill", Art + "ui_bar_fill.png", Vector2.zero, Vector2.zero, new Color(0.62f, 0.4f, 0.95f));
                Stretch(fill.rectTransform, 2, 2);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = 0.5f;

                var dotRt = Rect(rt, "Attention", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(44, 44));
                var dot = dotRt.gameObject.AddComponent<Image>();
                dot.sprite = Sprite(Art + "map_path_dot.png");
                dot.color = MissionPreviewUtility.Red;
                dot.raycastTarget = false;
                var excl = Label(dotRt, "Text", "!", 28, Color.white, TextAlignmentOptions.Center, outline: true);
                Stretch(excl.rectTransform, 0, 0);
                dotRt.gameObject.SetActive(false);

                var locked = Label(rt, "Locked", "LOCKED", 26, new Color(0.85f, 0.8f, 0.72f), TextAlignmentOptions.Left, outline: true);
                Place(locked.rectTransform, new Vector2(38, -14), new Vector2(190, 34));
                locked.gameObject.SetActive(false);

                if (cls == WeaponClass.Crossbow)
                {
                    var main = Chip(rt, "Main", new Vector2(98, -10), new Vector2(84, 30));
                    main.GetComponent<Image>().color = new Color(1f, 0.78f, 0.3f);
                    var mainText = Label(main, "Text", "MAIN", 20, Color.white, TextAlignmentOptions.Center, outline: true);
                    Stretch(mainText.rectTransform, 4, 0);
                }

                tabs[i] = new ArsenalClassTabView
                {
                    Class = cls, Button = button, Background = bg, Icon = icon, LevelText = level,
                    EssenceFill = fill, AttentionDot = dotRt.gameObject, LockedRoot = locked.gameObject,
                    NormalColor = new Color(0.78f, 0.74f, 0.7f), SelectedColor = new Color(1f, 0.9f, 0.55f),
                };
            }

            var so = new SerializedObject(tabsView);
            so.FindProperty("_screen").objectReferenceValue = screen;
            var arr = so.FindProperty("_tabs");
            arr.arraySize = tabs.Length;
            for (int i = 0; i < tabs.Length; i++)
            {
                var e = arr.GetArrayElementAtIndex(i);
                var t = tabs[i];
                e.FindPropertyRelative("Class").enumValueIndex = (int)t.Class;
                e.FindPropertyRelative("Button").objectReferenceValue = t.Button;
                e.FindPropertyRelative("Background").objectReferenceValue = t.Background;
                e.FindPropertyRelative("Icon").objectReferenceValue = t.Icon;
                e.FindPropertyRelative("LevelText").objectReferenceValue = t.LevelText;
                e.FindPropertyRelative("EssenceFill").objectReferenceValue = t.EssenceFill;
                e.FindPropertyRelative("AttentionDot").objectReferenceValue = t.AttentionDot;
                e.FindPropertyRelative("LockedRoot").objectReferenceValue = t.LockedRoot;
                e.FindPropertyRelative("NormalColor").colorValue = t.NormalColor;
                e.FindPropertyRelative("SelectedColor").colorValue = t.SelectedColor;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, WeaponPrefab);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Victory screen
    // ═════════════════════════════════════════════════════════════════════════

    public static void BuildVictory()
    {
        var root = PrefabUtility.LoadPrefabContents(WinPrefab);
        try
        {
            var victory = root.transform.Find("Victory Screen [SubState]");
            var layout = victory.Find("Victory Canvas/Layout");
            DestroyChild(layout, "Progress");

            var seg = Rect(layout, "Progress", new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(860, 200));
            var rewards = layout.Find("Rewards");
            seg.SetSiblingIndex(rewards.GetSiblingIndex() + 1);
            var le = seg.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 200;
            var group = seg.gameObject.AddComponent<CanvasGroup>();

            // Class essence block
            var classRoot = Rect(seg, "Class", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(760, 120));
            var chip = classRoot.gameObject.AddComponent<Image>();
            chip.sprite = Sprite(Art + "ui_wood_chip.png");
            chip.type = Image.Type.Sliced;
            var icon = Img(classRoot, "Icon", null, new Vector2(-318, 0), new Vector2(96, 96), Color.white);
            icon.preserveAspect = true;
            var title = Label(classRoot, "Title", "BOW · LV 5", 30, MissionPreviewUtility.Gold, TextAlignmentOptions.Left, outline: true);
            Place(title.rectTransform, new Vector2(-40, 32), new Vector2(460, 40));
            var track = Img(classRoot, "Bar", Art + "ui_bar_track.png", new Vector2(-40, -4), new Vector2(460, 40), Color.white);
            track.type = Image.Type.Sliced;
            var gain = Img(track.rectTransform, "Gain", Art + "ui_bar_fill.png", Vector2.zero, Vector2.zero, new Color(0.55f, 1f, 0.45f));
            Stretch(gain.rectTransform, 4, 4);
            gain.type = Image.Type.Filled; gain.fillMethod = Image.FillMethod.Horizontal; gain.fillAmount = 0.8f;
            var before = Img(track.rectTransform, "Fill", Art + "ui_bar_fill.png", Vector2.zero, Vector2.zero, new Color(0.62f, 0.4f, 0.95f));
            Stretch(before.rectTransform, 4, 4);
            before.type = Image.Type.Filled; before.fillMethod = Image.FillMethod.Horizontal; before.fillAmount = 0.5f;
            var value = Label(track.rectTransform, "Value", "34 / 40", 26, Color.white, TextAlignmentOptions.Center, outline: true);
            Stretch(value.rectTransform, 0, 0);
            var hint = Label(classRoot, "Hint", "6 more essence to LV 6", 24, Color.white, TextAlignmentOptions.Left, outline: true);
            Place(hint.rectTransform, new Vector2(-40, -40), new Vector2(460, 32));

            var stamp = Rect(classRoot, "Ready Stamp", Half, Half, new Vector2(300, 6), new Vector2(170, 110));
            var rays = Img(stamp, "Rays", Art + "fx_rays.png", Vector2.zero, new Vector2(300, 300), new Color(1f, 0.85f, 0.35f, 0.75f));
            var stampBg = Img(stamp, "Badge", Kit + "Sprites/Component/Button/btn_rectangle_01_n_green.png", Vector2.zero, new Vector2(170, 92), Color.white);
            stampBg.type = Image.Type.Sliced;
            var stampText = Label(stamp, "Text", "UPGRADE\nREADY!", 26, Color.white, TextAlignmentOptions.Center, outline: true);
            Place(stampText.rectTransform, Vector2.zero, new Vector2(170, 92));
            stamp.localRotation = Quaternion.Euler(0, 0, -8f);

            var view = seg.gameObject.AddComponent<VictoryProgressSegment>();
            var so = new SerializedObject(view);
            so.FindProperty("_rootGroup").objectReferenceValue = group;
            so.FindProperty("_classRoot").objectReferenceValue = classRoot.gameObject;
            so.FindProperty("_classIcon").objectReferenceValue = icon;
            so.FindProperty("_classTitle").objectReferenceValue = title;
            so.FindProperty("_essenceFill").objectReferenceValue = before;
            so.FindProperty("_essenceGainFill").objectReferenceValue = gain;
            so.FindProperty("_essenceText").objectReferenceValue = value;
            so.FindProperty("_classHint").objectReferenceValue = hint;
            so.FindProperty("_readyStamp").objectReferenceValue = stamp.gameObject;
            so.FindProperty("_readyRays").objectReferenceValue = rays.rectTransform;
            so.FindProperty("_classIcons").objectReferenceValue = ClassIcons();
            so.ApplyModifiedPropertiesWithoutUndo();

            var sub = victory.GetComponent<VictoryScreenSubState>();
            var sso = new SerializedObject(sub);
            sso.FindProperty("_progress").objectReferenceValue = view;
            sso.ApplyModifiedPropertiesWithoutUndo();

            // Make room: the stats (skull) row is smaller.
            var stats = layout.Find("Stats")?.GetComponent<LayoutElement>();
            if (stats != null) { stats.preferredHeight = 100; stats.minHeight = 0; }

            PrefabUtility.SaveAsPrefabAsset(root, WinPrefab);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════════

    private static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

    private static Sprite Sprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Debug.LogWarning("MetaUiBuilder: sprite not found " + path);
        return s;
    }

    private static WeaponClassIconLibrary ClassIcons() =>
        AssetDatabase.LoadAssetAtPath<WeaponClassIconLibrary>("Assets/Meta/Weapons/UI/WeaponClassIconLibrary.asset");

    private static CurrencyVisualLibrary CurrencyVisuals() =>
        AssetDatabase.LoadAssetAtPath<CurrencyVisualLibrary>("Assets/Meta/Economy/Data/CurrencyVisualLibrary.asset");

    private static void DestroyChild(Transform parent, string name)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            if (parent.GetChild(i).name == name)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    private static Image Img(Transform parent, string name, string spritePath, Vector2 pos, Vector2 size, Color color)
    {
        var rt = Rect(parent, name, Half, Half, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        if (spritePath != null) img.sprite = Sprite(spritePath);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Color color,
                                         TextAlignmentOptions align, bool outline)
    {
        var rt = Rect(parent, name, Half, Half, Vector2.zero, new Vector2(300, 60));
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Kit + (outline ? "Fonts/Alata-Regular-Outline 54 SDF.asset" : "Fonts/Alata-Regular SDF.asset"));
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        t.richText = true;
        return t;
    }

    private static RectTransform Chip(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var rt = Rect(parent, name, Half, Half, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Sprite(Art + "ui_wood_chip.png");
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        return rt;
    }

    private static Button KitButton(Transform parent, string name, string sprite, string label, Vector2 pos, Vector2 size, float fontSize)
    {
        var rt = Rect(parent, name, Half, Half, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Sprite(Kit + $"Sprites/Component/Button/{sprite}.png");
        img.type = Image.Type.Sliced;
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var t = Label(rt, "Text", label, fontSize, Color.white, TextAlignmentOptions.Center, outline: true);
        Stretch(t.rectTransform, 12, 10);
        t.rectTransform.offsetMin = new Vector2(12, 18);
        return b;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = Half;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void TopAnchor(RectTransform rt, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = pos;
    }

    private static void TopPlace(RectTransform rt, float y, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = size;
    }

    private static void ColumnPlace(RectTransform rt, float x0, float x1)
    {
        rt.anchorMin = new Vector2(x0, 0);
        rt.anchorMax = new Vector2(x1, 1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void Stretch(RectTransform rt, float x, float y)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(x, y);
        rt.offsetMax = new Vector2(-x, -y);
    }
}
#endif
