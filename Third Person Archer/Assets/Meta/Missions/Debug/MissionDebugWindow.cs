using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MissionDebugWindow : MonoBehaviour
{
    [Header("Toggle")]
    public KeyCode toggleKeyPrimary = KeyCode.BackQuote;
    public KeyCode toggleKeyAlt = KeyCode.F1;
    [Range(0.05f, 0.5f)] public float threeFingerTapWindow = 0.2f;

    [Header("Refs")]
    public MissionProgressData progressData;   // assign in Inspector
    public Canvas targetCanvas;                // optional (auto-created if null)

    [Header("Behavior")]
    public bool openOnStart = false;           // 👈 new

    [Header("Style")]
    public int headerFontSize = 32;
    public int rowFontSize = 26;
    public float windowPadding = 20f;

    private RectTransform _window;
    private RectTransform _content;
    private GameObject _blocker;
    private bool _built, _visible;
    private int _tapCount; private float _tapWindowStart;

    void Start()
    {
        EnsureEventSystem();
        BuildUI();

        if (openOnStart) Show();
        else Hide();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKeyPrimary) || Input.GetKeyDown(toggleKeyAlt)) Toggle();
        HandleThreeFingerTap();
    }

    private void HandleThreeFingerTap()
    {
        if (Input.touchCount == 0) { _tapCount = 0; return; }
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            float now = Time.unscaledTime;
            if (_tapCount == 0) _tapWindowStart = now;

            if (now - _tapWindowStart <= threeFingerTapWindow)
            {
                _tapCount++;
                if (_tapCount >= 3)
                {
                    Toggle();
                    _tapCount = 0;
                    _tapWindowStart = 0;
                    break;
                }
            }
            else { _tapCount = 1; _tapWindowStart = now; }
        }
    }

    public void Toggle()
    {
        if (!_built) BuildUI();
        if (_visible) Hide(); else Show();
    }

    private void Show()
    {
        _window.gameObject.SetActive(true);
        _blocker.SetActive(true);
        _visible = true;
        RebuildList();
    }

    private void Hide()
    {
        if (_window) _window.gameObject.SetActive(false);
        if (_blocker) _blocker.SetActive(false);
        _visible = false;
    }

    private void BuildUI()
    {
        if (_built) return;

        Canvas canvas = targetCanvas ?? FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("MissionDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Screen blocker
        _blocker = new GameObject("Blocker", typeof(Image), typeof(Button));
        _blocker.transform.SetParent(canvas.transform, false);
        var blk = _blocker.GetComponent<RectTransform>();
        blk.anchorMin = Vector2.zero; blk.anchorMax = Vector2.one;
        blk.offsetMin = Vector2.zero; blk.offsetMax = Vector2.zero;
        _blocker.GetComponent<Image>().color = new Color(0,0,0,0.35f);
        _blocker.GetComponent<Button>().onClick.AddListener(Hide);

        // Window
        var window = new GameObject("MissionDebugWindow", typeof(Image));
        window.transform.SetParent(canvas.transform, false);
        _window = window.GetComponent<RectTransform>();
        _window.anchorMin = new Vector2(0.07f, 0.07f);
        _window.anchorMax = new Vector2(0.93f, 0.93f);
        _window.offsetMin = Vector2.zero; _window.offsetMax = Vector2.zero;
        window.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        var vlg = window.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset((int)windowPadding, (int)windowPadding, (int)windowPadding, (int)windowPadding);
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childForceExpandHeight = false;    // 👈 don’t force equal expansion

        // Title (fixed height)
        var title = CreateTMP("Mission Debug Window", headerFontSize, TextAlignmentOptions.Left);
        title.transform.SetParent(window.transform, false);
        var titleLE = title.gameObject.AddComponent<LayoutElement>();
        titleLE.preferredHeight = Mathf.Max(48f, headerFontSize * 1.8f); // 👈 only occupies this height
        titleLE.flexibleHeight = 0;

        // ScrollView (fills remaining height)
        var scrollRoot = new GameObject("ScrollView", typeof(ScrollRect), typeof(LayoutElement));
        scrollRoot.transform.SetParent(window.transform, false);
        var scrollLE = scrollRoot.GetComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;      // 👈 take all remaining space
        scrollLE.minHeight = 0;

        var scrollRT = (RectTransform)scrollRoot.transform;
        scrollRT.anchorMin = Vector2.zero; scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = Vector2.zero; scrollRT.offsetMax = Vector2.zero;

        // Viewport
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollRoot.transform, false);
        var vpRT = (RectTransform)viewport.transform;
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;

        // Content (top-anchored, preferred height)
        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        _content = content.GetComponent<RectTransform>();
        _content.anchorMin = new Vector2(0f, 1f);
        _content.anchorMax = new Vector2(1f, 1f);
        _content.pivot = new Vector2(0.5f, 1f);
        _content.anchoredPosition = Vector2.zero;
        _content.sizeDelta = new Vector2(0, 0);

        var contentVLG = content.GetComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 6;
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        contentVLG.childAlignment = TextAnchor.UpperLeft;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = scrollRoot.GetComponent<ScrollRect>();
        scroll.viewport = vpRT;
        scroll.content = _content;
        scroll.vertical = true;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.inertia = true;
        scroll.scrollSensitivity = 40f;

        _built = true;
    }

    private void RebuildList()
    {
        for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);

        if (!progressData) { AddInfo("No MissionProgressData assigned!"); return; }
        var zones = progressData.AllZones;
        if (zones == null || zones.Count == 0) { AddInfo("No zones found!"); return; }

        int counter = 1; // global running number

        foreach (var zone in zones)
        {
            var zoneLabel = CreateTMP($"— {zone.Name}", rowFontSize + 2, TextAlignmentOptions.Left);
            zoneLabel.fontStyle = FontStyles.Bold;
            zoneLabel.color = Color.white;
            zoneLabel.transform.SetParent(_content, false);

            foreach (var seg in zone.Segments)
            {
                foreach (var mission in seg.Missions)
                {
                    var row = new GameObject($"Mission_{mission.Name}", typeof(Image), typeof(Button), typeof(LayoutElement));
                    row.transform.SetParent(_content, false);
                    var rowImg = row.GetComponent<Image>();
                    rowImg.color = new Color(1,1,1,0.06f);

                    var le = row.GetComponent<LayoutElement>();
                    le.minHeight = 60;
                    le.flexibleWidth = 1;

                    var text = CreateTMP($"#{counter:000}  [{mission.MissionType}] {mission.Name}", rowFontSize, TextAlignmentOptions.Left);
                    text.enableWordWrapping = false;
                    text.overflowMode = TextOverflowModes.Ellipsis;
                    text.transform.SetParent(row.transform, false);

                    var tRT = text.rectTransform;
                    tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
                    tRT.offsetMin = new Vector2(16, 6); tRT.offsetMax = new Vector2(-16, -6);

                    row.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (ScenesLoader.Instance != null) ScenesLoader.Instance.LoadMission(mission);
                        else Debug.LogError("ScenesLoader.Instance is null!");
                    });

                    counter++;
                }
            }
        }
    }

    private TextMeshProUGUI CreateTMP(string text, int size, TextAlignmentOptions align)
    {
        var go = new GameObject("TMP", typeof(TextMeshProUGUI));
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    private void AddInfo(string msg)
    {
        var t = CreateTMP(msg, rowFontSize, TextAlignmentOptions.Left);
        t.color = Color.yellow;
        t.transform.SetParent(_content, false);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(es);
    }
}