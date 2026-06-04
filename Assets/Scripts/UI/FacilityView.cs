using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
//  FacilityView  –  dark-fantasy HQ management screen
//  Procedurally built; no Inspector wiring required.
// ─────────────────────────────────────────────────────────────────────────────
public class FacilityView : MonoBehaviour
{
    // ── colours ──────────────────────────────────────────────────────────────
    private static readonly Color ColBg          = Hex("#0B0D1A");
    private static readonly Color ColPanel       = Hex("#111525");
    private static readonly Color ColCard        = Hex("#151929");
    private static readonly Color ColCardBorder  = Hex("#1E2440");
    private static readonly Color ColGold        = Hex("#C8A84B");
    private static readonly Color ColGoldDim     = Hex("#7A6530");
    private static readonly Color ColGem         = Hex("#5B9EE0");
    private static readonly Color ColMorale      = Hex("#3AB87A");
    private static readonly Color ColMoraleLow   = Hex("#E05050");
    private static readonly Color ColText        = Hex("#D8DCF0");
    private static readonly Color ColSubText     = Hex("#8890B0");
    private static readonly Color ColUpgrade     = Hex("#2A6A3A");
    private static readonly Color ColUpgradeHi   = Hex("#3A8A4A");
    private static readonly Color ColLocked      = Hex("#1A1D2E");
    private static readonly Color ColLockedText  = Hex("#3A3E55");
    private static readonly Color ColSelectedGlow= Hex("#C8A84B");
    private static readonly Color ColDivider     = Hex("#1E2440");
    private static readonly Color ColHeaderBg    = Hex("#0D1020");
    private static readonly Color ColDetailBg    = Hex("#0F1322");

    // ── layout constants ──────────────────────────────────────────────────────
    private const float HeaderH   = 110f;
    private const float MapRight  = 0.60f;   // left 60% = map, right 40% = detail
    private const float CardW     = 220f;
    private const float CardH     = 160f;
    private const float CardPad   = 14f;
    private const float DetailPad = 20f;

    // ── runtime state ─────────────────────────────────────────────────────────
    private IFacilityService  _facilityService;
    private ICurrencyService  _currencyService;
    private IRosterService    _rosterService;

    private RectTransform     _mapArea;
    private RectTransform     _detailPanel;
    private bool              _detailVisible = false;
    private string            _selectedId    = null;

    private readonly List<FacilityCardWidget> _cards = new List<FacilityCardWidget>();

    // detail panel widgets
    private Image             _detailBg;
    private TextMeshProUGUI   _detailName;
    private TextMeshProUGUI   _detailRole;
    private TextMeshProUGUI   _detailEmotion;
    private TextMeshProUGUI   _detailLevel;
    private Image             _levelBarFill;
    private TextMeshProUGUI   _detailBenefit;
    private TextMeshProUGUI   _detailNextBenefit;
    private TextMeshProUGUI   _detailCost;
    private TextMeshProUGUI   _detailWarning;
    private Button            _upgradeButton;
    private TextMeshProUGUI   _upgradeBtnText;

    // dock sub-panel
    private GameObject        _dockSubPanel;
    private TextMeshProUGUI   _dockStatus;
    private Button            _btnRecon;
    private Button            _btnSupply;
    private Button            _btnExtraction;
    private Button            _btnLaunch;

    // header widgets
    private TextMeshProUGUI   _goldLabel;
    private TextMeshProUGUI   _gemsLabel;
    private TextMeshProUGUI   _moraleLabel;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Start()
    {
        _facilityService = ServiceRegistry.Instance.Resolve<IFacilityService>();
        _currencyService = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _rosterService   = ServiceRegistry.Instance.Resolve<IRosterService>();

        BuildUI();
        PopulateMap();
        RefreshHeader();
        SetDetailVisible(false);
    }

    private void OnEnable()
    {
        if (_currencyService != null) RefreshHeader();
    }

    // ── UI construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        // root canvas background
        Canvas canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1080, 2340);
        scaler.matchWidthOrHeight   = 0.5f;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

        // full-screen bg
        RectTransform root = GetOrAddRectTransform(gameObject);
        Stretch(root);
        AddImage(root, ColBg);

        BuildHeader(root);
        BuildMapAndDetail(root);
    }

    private void BuildHeader(RectTransform root)
    {
        GameObject header = NewGO("Header", root);
        RectTransform hrt = header.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0, 1);
        hrt.anchorMax = new Vector2(1, 1);
        hrt.pivot     = new Vector2(0.5f, 1);
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;
        hrt.sizeDelta = new Vector2(0, HeaderH);
        AddImage(hrt, ColHeaderBg);

        // separator line at bottom of header
        GameObject sep = NewGO("HeaderSep", hrt);
        RectTransform sepRt = sep.GetComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(0, 0);
        sepRt.anchorMax = new Vector2(1, 0);
        sepRt.pivot     = new Vector2(0.5f, 0);
        sepRt.sizeDelta = new Vector2(0, 2);
        sepRt.anchoredPosition = Vector2.zero;
        AddImage(sepRt, ColGold);

        // BACK button
        Button back = MakeButton("Back", "BACK", 18, hrt,
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f));
        RectTransform backRt = back.GetComponent<RectTransform>();
        backRt.sizeDelta         = new Vector2(120, 54);
        backRt.anchoredPosition  = new Vector2(70, 0);
        StyleButton(back, ColCardBorder, ColGold);
        back.onClick.AddListener(() => SceneManager.LoadScene("Hub"));

        // Title label centred
        GameObject titleGo = NewGO("Title", hrt);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.3f, 0);
        titleRt.anchorMax = new Vector2(0.7f, 1);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
        TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
        title.text      = "HEADQUARTERS";
        title.fontSize  = 28;
        title.fontStyle = FontStyles.Bold;
        title.color     = ColGold;
        title.alignment = TextAlignmentOptions.Center;

        // currency row – right side
        float iconX = 1080 - 20;
        _moraleLabel = MakeCurrencyLabel(hrt, "Morale", ref iconX, ColMorale);
        _gemsLabel   = MakeCurrencyLabel(hrt, "Gems",   ref iconX, ColGem);
        _goldLabel   = MakeCurrencyLabel(hrt, "Gold",   ref iconX, ColGold);
    }

    // Returns a TMP label placed from the right side of the header
    private TextMeshProUGUI MakeCurrencyLabel(RectTransform parent, string name, ref float rightEdgeX, Color col)
    {
        float w = 180f;
        float h = 44f;
        GameObject go = NewGO("Curr_" + name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot     = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(-(1080 - rightEdgeX + 10), 0);
        rightEdgeX -= (w + 8);

        // small coloured background pill
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(col.r * 0.2f, col.g * 0.2f, col.b * 0.2f, 0.6f);

        // prefix label
        GameObject prefGo = NewGO("Prefix", rt);
        RectTransform prefRt = prefGo.GetComponent<RectTransform>();
        prefRt.anchorMin = new Vector2(0, 0);
        prefRt.anchorMax = new Vector2(0.38f, 1);
        prefRt.offsetMin = prefRt.offsetMax = Vector2.zero;
        TextMeshProUGUI pref = prefGo.AddComponent<TextMeshProUGUI>();
        pref.text      = name.ToUpper()[0].ToString();
        pref.fontSize  = 18;
        pref.color     = col;
        pref.alignment = TextAlignmentOptions.Center;
        pref.fontStyle = FontStyles.Bold;

        // value label
        GameObject valGo = NewGO("Value", rt);
        RectTransform valRt = valGo.GetComponent<RectTransform>();
        valRt.anchorMin = new Vector2(0.38f, 0);
        valRt.anchorMax = new Vector2(1, 1);
        valRt.offsetMin = new Vector2(4, 0);
        valRt.offsetMax = Vector2.zero;
        TextMeshProUGUI val = valGo.AddComponent<TextMeshProUGUI>();
        val.text      = "0";
        val.fontSize  = 18;
        val.color     = ColText;
        val.alignment = TextAlignmentOptions.Left;

        return val;
    }

    private void BuildMapAndDetail(RectTransform root)
    {
        // ── MAP AREA (left 60%) ───────────────────────────────────────────────
        GameObject mapGo = NewGO("MapArea", root);
        _mapArea = mapGo.GetComponent<RectTransform>();
        _mapArea.anchorMin = new Vector2(0, 0);
        _mapArea.anchorMax = new Vector2(MapRight, 1);
        _mapArea.offsetMin = new Vector2(0, 0);
        _mapArea.offsetMax = new Vector2(0, -HeaderH);

        // ── DETAIL PANEL (right 40%) ─────────────────────────────────────────
        GameObject detailGo = NewGO("DetailPanel", root);
        _detailPanel = detailGo.GetComponent<RectTransform>();
        _detailPanel.anchorMin = new Vector2(MapRight, 0);
        _detailPanel.anchorMax = new Vector2(1, 1);
        _detailPanel.offsetMin = Vector2.zero;
        _detailPanel.offsetMax = new Vector2(0, -HeaderH);
        AddImage(_detailPanel, ColDetailBg);

        // vertical separator
        GameObject vsep = NewGO("VSep", root);
        RectTransform vsepRt = vsep.GetComponent<RectTransform>();
        vsepRt.anchorMin = new Vector2(MapRight, 0);
        vsepRt.anchorMax = new Vector2(MapRight, 1);
        vsepRt.offsetMin = new Vector2(-1, -HeaderH);
        vsepRt.offsetMax = new Vector2(1, -HeaderH);
        AddImage(vsepRt, ColDivider);

        BuildDetailPanel(_detailPanel);
    }

    private void BuildDetailPanel(RectTransform parent)
    {
        float p = DetailPad;

        // "tap a building" placeholder
        GameObject placeholder = NewGO("Placeholder", parent);
        RectTransform phRt = placeholder.GetComponent<RectTransform>();
        phRt.anchorMin = new Vector2(0, 0.35f);
        phRt.anchorMax = new Vector2(1, 0.65f);
        phRt.offsetMin = phRt.offsetMax = Vector2.zero;
        TextMeshProUGUI phTxt = placeholder.AddComponent<TextMeshProUGUI>();
        phTxt.text      = "Tap a building\nto inspect it";
        phTxt.fontSize  = 20;
        phTxt.color     = ColSubText;
        phTxt.alignment = TextAlignmentOptions.Center;
        phTxt.name      = "DetailPlaceholder";

        // ── content root (hidden until selection) ────────────────────────────
        GameObject content = NewGO("DetailContent", parent);
        RectTransform cRt = content.GetComponent<RectTransform>();
        Stretch(cRt);
        content.name = "DetailContent";

        float yOff = -p;

        // facility name
        _detailName = MakeLabel("DetName", content.transform, 26, FontStyles.Bold, ColGold, TextAlignmentOptions.Center);
        PositionLabel(_detailName.rectTransform, 0, yOff, 0, 40); yOff -= 44;

        // role badge
        _detailRole = MakeLabel("DetRole", content.transform, 14, FontStyles.Normal, ColSubText, TextAlignmentOptions.Center);
        PositionLabel(_detailRole.rectTransform, 0, yOff, 0, 26); yOff -= 30;

        // divider
        MakeDivider(content.transform, yOff); yOff -= 18;

        // level progress row
        _detailLevel = MakeLabel("DetLevel", content.transform, 18, FontStyles.Bold, ColText, TextAlignmentOptions.Left);
        PositionLabel(_detailLevel.rectTransform, p, yOff, -p, 28); yOff -= 32;

        // level bar bg
        GameObject barBg = NewGO("LvBarBg", content.transform);
        RectTransform barBgRt = barBg.GetComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0, 1);
        barBgRt.anchorMax = new Vector2(1, 1);
        barBgRt.pivot     = new Vector2(0, 1);
        barBgRt.offsetMin = new Vector2(p, 0);
        barBgRt.offsetMax = new Vector2(-p, 0);
        barBgRt.sizeDelta = new Vector2(0, 10);
        barBgRt.anchoredPosition = new Vector2(p, yOff);
        AddImage(barBgRt, ColCardBorder);

        // fill
        GameObject barFill = NewGO("LvBarFill", barBg.transform);
        RectTransform fillRt = barFill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(0.5f, 1);
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        _levelBarFill = barFill.AddComponent<Image>();
        _levelBarFill.color = ColGold;
        yOff -= 18;

        // benefit row
        _detailBenefit = MakeLabel("DetBenefit", content.transform, 16, FontStyles.Normal, ColText, TextAlignmentOptions.Left);
        PositionLabel(_detailBenefit.rectTransform, p, yOff, -p, 22); yOff -= 26;

        _detailNextBenefit = MakeLabel("DetNextBen", content.transform, 14, FontStyles.Normal, ColUpgradeHi, TextAlignmentOptions.Left);
        PositionLabel(_detailNextBenefit.rectTransform, p, yOff, -p, 20); yOff -= 26;

        // divider
        MakeDivider(content.transform, yOff); yOff -= 18;

        // emotion / flavour
        _detailEmotion = MakeLabel("DetEmotion", content.transform, 14, FontStyles.Italic, ColSubText, TextAlignmentOptions.Left);
        _detailEmotion.textWrappingMode = TextWrappingModes.Normal;
        PositionLabel(_detailEmotion.rectTransform, p, yOff, -p, 60); yOff -= 68;

        // divider
        MakeDivider(content.transform, yOff); yOff -= 18;

        // cost label
        _detailCost = MakeLabel("DetCost", content.transform, 16, FontStyles.Normal, ColGold, TextAlignmentOptions.Left);
        PositionLabel(_detailCost.rectTransform, p, yOff, -p, 26); yOff -= 30;

        // warning / status
        _detailWarning = MakeLabel("DetWarning", content.transform, 13, FontStyles.Normal, ColSubText, TextAlignmentOptions.Left);
        _detailWarning.textWrappingMode = TextWrappingModes.Normal;
        PositionLabel(_detailWarning.rectTransform, p, yOff, -p, 40); yOff -= 48;

        // UPGRADE button – fixed to bottom of panel
        GameObject upgGo = NewGO("UpgradeBtn", content.transform);
        RectTransform upgRt = upgGo.GetComponent<RectTransform>();
        upgRt.anchorMin = new Vector2(0, 0);
        upgRt.anchorMax = new Vector2(1, 0);
        upgRt.pivot     = new Vector2(0.5f, 0);
        upgRt.offsetMin = new Vector2(p, p);
        upgRt.offsetMax = new Vector2(-p, p + 70);
        _upgradeButton  = upgGo.AddComponent<Button>();
        Image upgImg    = upgGo.AddComponent<Image>();
        upgImg.color    = ColUpgrade;

        // button border highlight
        GameObject upgBorder = NewGO("UpgBorder", upgGo.transform);
        RectTransform ubRt = upgBorder.GetComponent<RectTransform>();
        ubRt.anchorMin = new Vector2(0, 0);
        ubRt.anchorMax = new Vector2(1, 0);
        ubRt.pivot     = new Vector2(0.5f, 0);
        ubRt.sizeDelta = new Vector2(0, 3);
        ubRt.anchoredPosition = Vector2.zero;
        AddImage(ubRt, ColUpgradeHi);

        _upgradeBtnText = MakeLabel("UpgText", upgGo.transform, 22, FontStyles.Bold, ColText, TextAlignmentOptions.Center);
        Stretch(_upgradeBtnText.rectTransform);
        _upgradeBtnText.text = "UPGRADE";
        _upgradeButton.onClick.AddListener(OnUpgradePressed);

        // ── DOCK sub-panel ────────────────────────────────────────────────────
        BuildDockSubPanel(content.transform, yOff);

        // hide content by default; placeholder is shown
        content.SetActive(false);
        content.name = "DetailContent";
        placeholder.name = "DetailPlaceholder";
    }

    private void BuildDockSubPanel(Transform parent, float yStart)
    {
        _dockSubPanel = NewGO("DockPanel", parent);
        RectTransform dpRt = _dockSubPanel.GetComponent<RectTransform>();
        dpRt.anchorMin = new Vector2(0, 0.30f);
        dpRt.anchorMax = new Vector2(1, 0.62f);
        dpRt.offsetMin = new Vector2(DetailPad, 0);
        dpRt.offsetMax = new Vector2(-DetailPad, 0);
        AddImage(dpRt, Hex("#0D1020"));

        float p = 10f;
        float y = -p;

        _dockStatus = MakeLabel("DockStatus", _dockSubPanel.transform, 15, FontStyles.Bold, ColSubText, TextAlignmentOptions.Center);
        PositionLabel(_dockStatus.rectTransform, 0, y, 0, 24); y -= 28;

        _btnRecon      = MakeSortieButton("Recon",     DockSortieType.Recon,      _dockSubPanel.transform, y); y -= 54;
        _btnSupply     = MakeSortieButton("Supply",    DockSortieType.Supply,     _dockSubPanel.transform, y); y -= 54;
        _btnExtraction = MakeSortieButton("Extraction",DockSortieType.Extraction, _dockSubPanel.transform, y); y -= 60;

        // Launch button
        GameObject lGo = NewGO("LaunchBtn", _dockSubPanel.transform);
        RectTransform lRt = lGo.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0, 1);
        lRt.anchorMax = new Vector2(1, 1);
        lRt.pivot     = new Vector2(0.5f, 1);
        lRt.offsetMin = new Vector2(p, 0);
        lRt.offsetMax = new Vector2(-p, 0);
        lRt.sizeDelta = new Vector2(0, 50);
        lRt.anchoredPosition = new Vector2(0, y);
        _btnLaunch = lGo.AddComponent<Button>();
        AddImage(lRt, Hex("#1A4A2A"));
        TextMeshProUGUI lt = MakeLabel("LText", lGo.transform, 18, FontStyles.Bold, ColUpgradeHi, TextAlignmentOptions.Center);
        Stretch(lt.rectTransform);
        lt.text = "LAUNCH SORTIE";
        _btnLaunch.onClick.AddListener(LaunchSortie);

        _dockSubPanel.SetActive(false);
    }

    private Button MakeSortieButton(string label, DockSortieType type, Transform parent, float y)
    {
        GameObject go = NewGO("Sort_" + label, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(DetailPad, 0);
        rt.offsetMax = new Vector2(-DetailPad, 0);
        rt.sizeDelta = new Vector2(0, 46);
        rt.anchoredPosition = new Vector2(0, y);
        Button btn = go.AddComponent<Button>();
        AddImage(rt, ColCardBorder);
        TextMeshProUGUI txt = MakeLabel("T", go.transform, 16, FontStyles.Normal, ColText, TextAlignmentOptions.Center);
        Stretch(txt.rectTransform);
        txt.text = label.ToUpperInvariant();
        btn.onClick.AddListener(() => {
            _facilityService?.ToggleSortieType(type);
            RefreshDockPanel();
        });
        return btn;
    }

    // ── Map population ────────────────────────────────────────────────────────
    private void PopulateMap()
    {
        // Destroy any previously built scroll root first
        foreach (Transform child in _mapArea)
        {
            Destroy(child.gameObject);
        }
        _cards.Clear();

        if (_facilityService == null) return;

        List<FacilityDefinition> defs = _facilityService.GetAllFacilities();

        // Layout: 2-column grid, centred in map area, with row groupings
        // Row layout (top to bottom): Workshop | Square   /  Dorms | Training Hall  /  Memorial Hall | Crucible  /  Flying Dock | Tactical Station
        string[][] rowLayout = new string[][]
        {
            new string[] { "workshop",        "square"           },
            new string[] { "dorms",           "training_hall"    },
            new string[] { "memorial_hall",   "crucible"         },
            new string[] { "flying_dock",     "tactical_station" }
        };

        // Map area root – use a ScrollRect so many cards don't get clipped
        GameObject scrollGo = NewGO("MapScroll", _mapArea);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        Stretch(scrollRt);
        ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        GameObject vpGo = NewGO("MapViewport", scrollRt);
        RectTransform vpRt = vpGo.GetComponent<RectTransform>();
        Stretch(vpRt);
        Mask vpMask = vpGo.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;
        Image vpImg = vpGo.AddComponent<Image>();
        vpImg.color = Color.clear;
        scroll.viewport = vpRt;

        GameObject contentGo = NewGO("MapContent", vpRt);
        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot     = new Vector2(0.5f, 1);
        contentRt.offsetMin = contentRt.offsetMax = Vector2.zero;
        scroll.content = contentRt;

        // title label inside map
        GameObject mapTitle = NewGO("MapTitle", contentRt);
        RectTransform mtRt = mapTitle.GetComponent<RectTransform>();
        mtRt.anchorMin = new Vector2(0, 1);
        mtRt.anchorMax = new Vector2(1, 1);
        mtRt.pivot     = new Vector2(0.5f, 1);
        mtRt.sizeDelta = new Vector2(0, 52);
        mtRt.anchoredPosition = Vector2.zero;
        TextMeshProUGUI mtTxt = mapTitle.AddComponent<TextMeshProUGUI>();
        mtTxt.text      = "COMMAND BASE";
        mtTxt.fontSize  = 20;
        mtTxt.fontStyle = FontStyles.Bold;
        mtTxt.color     = ColGoldDim;
        mtTxt.alignment = TextAlignmentOptions.Center;

        float startY  = -60f;   // below the title label
        float rowH    = CardH + CardPad * 2;
        int   totalRows = rowLayout.Length;
        float totalContentH = startY + (totalRows * rowH) + CardPad;
        contentRt.sizeDelta = new Vector2(0, Mathf.Abs(totalContentH) + 40);

        for (int r = 0; r < rowLayout.Length; r++)
        {
            string[] row = rowLayout[r];
            int cols = row.Length;
            float rowTop = startY - r * rowH;

            for (int c = 0; c < cols; c++)
            {
                string fid = row[c];
                FacilityDefinition def = defs.Find(d => d != null && d.FacilityId == fid);
                if (def == null) continue;

                int level = _facilityService.GetFacilityLevel(fid);
                bool locked = level <= 0;

                FacilityCardWidget widget = BuildFacilityCard(contentRt, def, level, locked, c, cols, rowTop);
                _cards.Add(widget);
            }
        }
    }

    private FacilityCardWidget BuildFacilityCard(RectTransform parent, FacilityDefinition def,
        int level, bool locked, int col, int totalCols, float rowTop)
    {
        GameObject cardGo = NewGO("Card_" + def.FacilityId, parent);
        RectTransform cardRt = cardGo.GetComponent<RectTransform>();

        // position: anchor top-left of parent, offset by column
        cardRt.anchorMin = new Vector2(0.5f, 1);
        cardRt.anchorMax = new Vector2(0.5f, 1);
        cardRt.pivot     = new Vector2(0.5f, 1);

        // x: spread columns horizontally with padding
        float totalW  = totalCols * CardW + (totalCols - 1) * CardPad;
        float startX  = -totalW / 2f + CardW / 2f;
        float xPos    = startX + col * (CardW + CardPad);
        cardRt.sizeDelta        = new Vector2(CardW, CardH);
        cardRt.anchoredPosition = new Vector2(xPos, rowTop);

        // card background
        Color baseCol = locked ? ColLocked : new Color(def.FacilityColor.r * 0.18f, def.FacilityColor.g * 0.18f, def.FacilityColor.b * 0.18f, 1f);
        Image cardImg = cardGo.AddComponent<Image>();
        cardImg.color = baseCol;

        // border Image child (for selection highlight)
        GameObject borderGo = NewGO("Border", cardRt);
        RectTransform borderRt = borderGo.GetComponent<RectTransform>();
        Stretch(borderRt);
        borderRt.offsetMin = new Vector2(-3, -3);
        borderRt.offsetMax = new Vector2(3, 3);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = ColCardBorder;
        borderGo.transform.SetSiblingIndex(0); // draw behind content

        // top accent bar using facility colour
        GameObject accentGo = NewGO("Accent", cardRt);
        RectTransform accentRt = accentGo.GetComponent<RectTransform>();
        accentRt.anchorMin = new Vector2(0, 1);
        accentRt.anchorMax = new Vector2(1, 1);
        accentRt.pivot     = new Vector2(0.5f, 1);
        accentRt.offsetMin = Vector2.zero;
        accentRt.offsetMax = Vector2.zero;
        accentRt.sizeDelta = new Vector2(0, 4);
        AddImage(accentRt, locked ? ColLockedText : def.FacilityColor);

        if (!locked)
        {
            // facility name
            TextMeshProUGUI nameLabel = MakeLabel("N", cardRt, 17, FontStyles.Bold,
                ColText, TextAlignmentOptions.Center);
            nameLabel.rectTransform.anchorMin = new Vector2(0, 0.60f);
            nameLabel.rectTransform.anchorMax = new Vector2(1, 0.92f);
            nameLabel.rectTransform.offsetMin = new Vector2(6, 0);
            nameLabel.rectTransform.offsetMax = new Vector2(-6, 0);
            nameLabel.text = def.DisplayName.ToUpperInvariant();

            // role badge
            TextMeshProUGUI roleLabel = MakeLabel("R", cardRt, 12, FontStyles.Normal,
                new Color(def.FacilityColor.r * 1.4f, def.FacilityColor.g * 1.4f, def.FacilityColor.b * 1.4f, 1f),
                TextAlignmentOptions.Center);
            roleLabel.rectTransform.anchorMin = new Vector2(0, 0.42f);
            roleLabel.rectTransform.anchorMax = new Vector2(1, 0.60f);
            roleLabel.rectTransform.offsetMin = new Vector2(6, 0);
            roleLabel.rectTransform.offsetMax = new Vector2(-6, 0);
            roleLabel.text = _facilityService.GetFacilityRole(def.FacilityId);

            // level indicator bar bg
            GameObject lvBgGo = NewGO("LvBg", cardRt);
            RectTransform lvBgRt = lvBgGo.GetComponent<RectTransform>();
            lvBgRt.anchorMin = new Vector2(0.05f, 0.22f);
            lvBgRt.anchorMax = new Vector2(0.95f, 0.28f);
            lvBgRt.offsetMin = lvBgRt.offsetMax = Vector2.zero;
            AddImage(lvBgRt, Hex("#1A1E2E"));

            // level fill
            GameObject lvFillGo = NewGO("LvFill", lvBgGo.transform);
            RectTransform lvFillRt = lvFillGo.GetComponent<RectTransform>();
            lvFillRt.anchorMin = new Vector2(0, 0);
            lvFillRt.anchorMax = new Vector2(Mathf.Clamp01((float)level / 10f), 1);
            lvFillRt.offsetMin = lvFillRt.offsetMax = Vector2.zero;
            Image fillImg = lvFillGo.AddComponent<Image>();
            fillImg.color = def.FacilityColor;

            // level text
            TextMeshProUGUI lvLabel = MakeLabel("L", cardRt, 14, FontStyles.Normal,
                ColGold, TextAlignmentOptions.Center);
            lvLabel.rectTransform.anchorMin = new Vector2(0, 0.04f);
            lvLabel.rectTransform.anchorMax = new Vector2(1, 0.20f);
            lvLabel.rectTransform.offsetMin = Vector2.zero;
            lvLabel.rectTransform.offsetMax = Vector2.zero;
            lvLabel.text = "Lv. " + level;
        }
        else
        {
            // locked overlay
            TextMeshProUGUI lockedLabel = MakeLabel("Locked", cardRt, 16, FontStyles.Bold,
                ColLockedText, TextAlignmentOptions.Center);
            Stretch(lockedLabel.rectTransform);
            lockedLabel.text = def.DisplayName.ToUpperInvariant() + "\n[LOCKED]";
        }

        // button component
        Button btn = cardGo.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = locked ? Color.white : new Color(1.15f, 1.10f, 1.05f);
        cb.pressedColor     = new Color(0.88f, 0.88f, 0.88f);
        btn.colors = cb;

        string capturedId = def.FacilityId;
        if (!locked)
        {
            btn.onClick.AddListener(() => SelectFacility(capturedId));
        }

        return new FacilityCardWidget
        {
            Root       = cardGo,
            BorderImg  = borderImg,
            FacilityId = def.FacilityId,
            Locked     = locked
        };
    }

    // ── Selection ─────────────────────────────────────────────────────────────
    private void SelectFacility(string facilityId)
    {
        // deselect old
        if (!string.IsNullOrEmpty(_selectedId))
        {
            FacilityCardWidget old = _cards.Find(c => c.FacilityId == _selectedId);
            old?.SetSelected(false);
        }

        _selectedId = facilityId;

        // highlight new
        FacilityCardWidget widget = _cards.Find(c => c.FacilityId == facilityId);
        widget?.SetSelected(true);

        RefreshDetailPanel(facilityId);
        SetDetailVisible(true);
    }

    private void RefreshDetailPanel(string facilityId)
    {
        if (_facilityService == null) return;

        FacilityDefinition def = _facilityService.GetFacility(facilityId);
        if (def == null) return;

        int level      = _facilityService.GetFacilityLevel(facilityId);
        bool atMax     = level >= def.MaxLevel;
        UpgradeCost cost = _facilityService.GetUpgradeCost(facilityId);
        bool canUpgrade  = _facilityService.CanUpgrade(facilityId);

        if (_detailName  != null) _detailName.text  = def.DisplayName.ToUpperInvariant();
        if (_detailRole  != null) _detailRole.text  = _facilityService.GetFacilityRole(facilityId)
                                                       + " | " + (def.FacilityType == FacilityType.Crucible
                                                       || def.FacilityType == FacilityType.MemorialHall
                                                       || def.FacilityType == FacilityType.TacticalStation
                                                       ? "SHADOW" : "COMMAND");
        if (_detailEmotion != null) _detailEmotion.text = _facilityService.GetFacilityEmotion(facilityId);
        if (_detailLevel   != null) _detailLevel.text   = "Level  " + level + " / " + def.MaxLevel;

        if (_levelBarFill != null)
        {
            RectTransform fillRt = _levelBarFill.rectTransform;
            fillRt.anchorMax = new Vector2(Mathf.Clamp01((float)level / def.MaxLevel), 1);
        }

        if (_detailBenefit     != null) _detailBenefit.text     = "Now:  " + def.GetBenefitDescription(level);
        if (_detailNextBenefit != null) _detailNextBenefit.text = atMax ? "MAX LEVEL" : "Next: " + def.GetNextLevelBenefitDescription(level);

        if (_detailCost != null)
        {
            if (atMax)
                _detailCost.text = "Fully upgraded";
            else
                _detailCost.text = "Cost: " + cost.Gold.ToString("N0") + " Gold"
                                   + (cost.Gems > 0 ? " + " + cost.Gems.ToString("N0") + " Gems" : "");
        }

        if (_detailWarning != null)
        {
            _detailWarning.text = _facilityService.IsShadowFacility(facilityId)
                ? "Shadow facilities change the roster permanently. Use with intent."
                : "Command facilities support daily growth and long-term stability.";
            _detailWarning.color = _facilityService.IsShadowFacility(facilityId) ? Hex("#C05050") : ColSubText;
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.interactable = canUpgrade;
            if (_upgradeBtnText != null)
                _upgradeBtnText.text = atMax ? "MAX LEVEL" : (canUpgrade ? "UPGRADE" : "INSUFFICIENT FUNDS");

            Image upgImg = _upgradeButton.GetComponent<Image>();
            if (upgImg != null)
                upgImg.color = canUpgrade ? ColUpgrade : ColCardBorder;
        }

        // dock panel
        bool isDock = def.FacilityType == FacilityType.FlyingDock;
        if (_dockSubPanel != null) _dockSubPanel.SetActive(isDock);
        if (isDock) RefreshDockPanel();
    }

    private void SetDetailVisible(bool visible)
    {
        _detailVisible = visible;
        if (_detailPanel == null) return;

        // find content/placeholder by name in children
        Transform content     = _detailPanel.Find("DetailContent");
        Transform placeholder = _detailPanel.Find("DetailPlaceholder");
        if (content     != null) content.gameObject.SetActive(visible);
        if (placeholder != null) placeholder.gameObject.SetActive(!visible);
    }

    // ── Upgrade ───────────────────────────────────────────────────────────────
    private void OnUpgradePressed()
    {
        if (string.IsNullOrEmpty(_selectedId)) return;
        if (_facilityService == null) return;

        bool ok = _facilityService.StartUpgrade(_selectedId);
        if (ok)
        {
            RefreshHeader();
            RefreshDetailPanel(_selectedId);
            // rebuild the card's level bar
            PopulateMap();
            // re-select so highlight is restored
            FacilityCardWidget w = _cards.Find(c => c.FacilityId == _selectedId);
            w?.SetSelected(true);
        }
        else if (_detailWarning != null)
        {
            _detailWarning.text  = "Upgrade failed — check your resources.";
            _detailWarning.color = ColMoraleLow;
        }
    }

    // ── Header refresh ────────────────────────────────────────────────────────
    private void RefreshHeader()
    {
        if (_currencyService == null) return;
        if (_goldLabel   != null) _goldLabel.text   = _currencyService.GetGold().ToString("N0");
        if (_gemsLabel   != null) _gemsLabel.text   = _currencyService.GetGems().ToString("N0");
        if (_moraleLabel != null)
        {
            int morale = GetAverageMorale();
            _moraleLabel.text  = morale.ToString();
            _moraleLabel.color = morale < 30 ? ColMoraleLow : morale < 60 ? ColGold : ColMorale;
        }
    }

    private int GetAverageMorale()
    {
        if (_rosterService == null) return 0;
        List<HeroInstance> alive = _rosterService.GetAlive();
        if (alive == null || alive.Count == 0) return 0;
        return Mathf.RoundToInt((float)alive.Average(h => h.Morale));
    }

    // ── Dock helpers ──────────────────────────────────────────────────────────
    private void RefreshDockPanel()
    {
        if (_dockSubPanel == null || _facilityService == null) return;
        DockSortieType queued = _facilityService.GetQueuedSortieType();
        if (_dockStatus != null) _dockStatus.text = "Queued: " + queued.ToString().ToUpperInvariant();
        SetSortieButtonState(_btnRecon,      DockSortieType.Recon,      queued);
        SetSortieButtonState(_btnSupply,     DockSortieType.Supply,     queued);
        SetSortieButtonState(_btnExtraction, DockSortieType.Extraction, queued);
    }

    private void SetSortieButtonState(Button btn, DockSortieType type, DockSortieType queued)
    {
        if (btn == null) return;
        bool active = queued == type;
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = active ? ColUpgrade : ColCardBorder;
        TextMeshProUGUI lbl = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (lbl != null) lbl.color = active ? ColUpgradeHi : ColText;
    }

    private void LaunchSortie()
    {
        if (_facilityService == null) return;
        if (_facilityService.LaunchSortie(out DockSortieResult result))
        {
            if (_detailWarning != null) { _detailWarning.text = result.Summary; _detailWarning.color = ColMorale; }
            RefreshHeader();
            RefreshDockPanel();
        }
        else if (_detailWarning != null)
        {
            _detailWarning.text  = result.Summary;
            _detailWarning.color = ColMoraleLow;
        }
    }

    // ── Low-level helpers ─────────────────────────────────────────────────────
    private static GameObject NewGO(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject NewGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static RectTransform GetOrAddRectTransform(GameObject go)
    {
        return go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Image AddImage(RectTransform rt, Color col)
    {
        Image img = rt.gameObject.GetComponent<Image>() ?? rt.gameObject.AddComponent<Image>();
        img.color = col;
        return img;
    }

    private static Button MakeButton(string name, string label, float fontSize, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Button btn = go.GetComponent<Button>();
        TextMeshProUGUI txt = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI))
                              .GetComponent<TextMeshProUGUI>();
        txt.transform.SetParent(go.transform, false);
        Stretch(txt.rectTransform);
        txt.text      = label;
        txt.fontSize  = fontSize;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color     = Color.white;
        return btn;
    }

    private static void StyleButton(Button btn, Color bgCol, Color textCol)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = bgCol;
        TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = textCol;
    }

    private static TextMeshProUGUI MakeLabel(string name, Transform parent, float size,
        FontStyles style, Color col, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = col;
        tmp.alignment = align;
        return tmp;
    }

    // anchoredPosition-based label placement from top-left
    private static void PositionLabel(RectTransform rt, float padLeft, float yFromTop, float padRight, float height)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0, 1);
        rt.offsetMin = new Vector2(padLeft,  0);
        rt.offsetMax = new Vector2(padRight, 0);
        rt.sizeDelta = new Vector2(0, height);
        rt.anchoredPosition = new Vector2(padLeft, yFromTop);
    }

    private static void MakeDivider(Transform parent, float yFromTop)
    {
        GameObject go = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 1);
        rt.anchorMax = new Vector2(0.98f, 1);
        rt.pivot     = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(0, yFromTop);
        go.GetComponent<Image>().color = ColDivider;
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}

// ── Helper widget wrapper ─────────────────────────────────────────────────────
public class FacilityCardWidget
{
    public GameObject Root;
    public Image      BorderImg;
    public string     FacilityId;
    public bool       Locked;

    public void SetSelected(bool selected)
    {
        if (BorderImg == null) return;
        BorderImg.color = selected
            ? new Color(0.784f, 0.659f, 0.294f, 1f)   // #C8A84B
            : new Color(0.118f, 0.133f, 0.188f, 1f);  // #1E2240
    }
}
