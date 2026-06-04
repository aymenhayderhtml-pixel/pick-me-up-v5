using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Roster scene — landscape two-panel layout (2340x1080).
/// Left panel: filter bar + vertically-scrolling hero grid (3 columns).
/// Right panel: detail view (placeholder until a hero is tapped).
/// </summary>
public class RosterView : MonoBehaviour
{
    // ── Colors ──────────────────────────────────────────────────────────
    private static readonly Color BgColor = new Color(0.039f, 0.047f, 0.078f);          // #0A0C14
    private static readonly Color CardFill = new Color(0.102f, 0.118f, 0.180f);          // #1A1E2E
    private static readonly Color AccentGold = new Color(0.784f, 0.659f, 0.294f);        // #C8A84B
    private static readonly Color SteelBlue = new Color(0.478f, 0.690f, 0.800f);         // #7AB0CC
    private static readonly Color Parchment = new Color(0.929f, 0.878f, 0.769f);         // #EDE0C4
    private static readonly Color TopBarBg = new Color(0.055f, 0.063f, 0.102f, 0.96f);
    private static readonly Color FilterActive = AccentGold;
    private static readonly Color FilterInactive = new Color(0.12f, 0.14f, 0.20f);
    private static readonly Color FilterBorderActive = AccentGold;
    private static readonly Color FilterBorderInactive = new Color(0.25f, 0.28f, 0.38f, 0.6f);
    private static readonly Color DetailBg = new Color(0.055f, 0.063f, 0.100f, 0.98f);
    private static readonly Color ButtonDismiss = new Color(0.55f, 0.15f, 0.15f);
    private static readonly Color ButtonBack = new Color(0.15f, 0.18f, 0.25f);
    private static readonly Color DividerColor = new Color(0.30f, 0.34f, 0.48f, 0.3f);
    private static readonly Color SeparatorGold = AccentGold;

    private const float SplitX = 0.71f; // left/right panel boundary

    // ── Runtime refs ────────────────────────────────────────────────────
    private IRosterService _roster;
    private Transform _gridContent;
    private readonly List<RosterHeroCard> _spawnedCards = new List<RosterHeroCard>();
    private List<HeroInstance> _allHeroes;
    private List<HeroInstance> _filtered;
    private string _activeFilter = "ALL";
    private readonly List<string> _filterClasses = new List<string>
    {
        "ALL", "NOVICE", "VANGUARD", "SCOUT", "MAGE", "BERSERKER", "ASSASSIN", "SUPPORT", "SPECIALIST"
    };
    private readonly Dictionary<string, Button> _filterButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Image> _filterBorders = new Dictionary<string, Image>();

    // Right panel refs
    private GameObject _placeholderObj;
    private GameObject _detailContent;
    private Image _detailPortrait;
    private TextMeshProUGUI _detailName;
    private TextMeshProUGUI _detailStars;
    private TextMeshProUGUI _detailClass;
    private RectTransform _moraleBarFill;
    private TextMeshProUGUI _detailMorale;
    private TextMeshProUGUI _detailTrait;
    private TextMeshProUGUI _detailStats;
    private TextMeshProUGUI _heroCountLabel;
    private GameObject _backDetailBtn;

    private HeroInstance _selectedHero;
    private HeroDefinition _selectedDef;

    // ════════════════════════════════════════════════════════════════════
    //  ENTRY POINT
    // ════════════════════════════════════════════════════════════════════

    private void Start()
    {
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        BuildUI();
        RefreshGrid();
    }

    // ════════════════════════════════════════════════════════════════════
    //  UI CONSTRUCTION
    // ════════════════════════════════════════════════════════════════════

    private void BuildUI()
    {
        // ── Full-screen dark background ─────────────────────────────────
        GameObject bg = CreateChild("Background", transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = BgColor;
        Stretch(bg);

        // ════════════════════════════════════════════════════════════════
        //  LEFT PANEL (0,0 → 0.72,1)
        // ════════════════════════════════════════════════════════════════

        GameObject leftBg = CreateChild("LeftPanelBg", transform);
        Image leftBgImg = leftBg.AddComponent<Image>();
        leftBgImg.color = BgColor;
        leftBgImg.raycastTarget = false;
        SetAnchors(leftBg, Vector2.zero, new Vector2(SplitX, 1f));

        // ════════════════════════════════════════════════════════════════
        //  HERO GRID (direct child of Canvas — offset below top bars)
        // ════════════════════════════════════════════════════════════════

        GameObject scrollObj = CreateChild("HeroGridScrollRect", transform);
        ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 40f;
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = new Vector2(SplitX, 1f);
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = new Vector2(0f, -140f);

        GameObject viewport = CreateChild("Viewport", scrollObj.transform);
        viewport.AddComponent<RectMask2D>();
        Stretch(viewport);
        sr.viewport = viewport.GetComponent<RectTransform>();

        GameObject content = CreateChild("Content", viewport.transform);
        GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(300f, 360f);
        glg.spacing = new Vector2(12f, 12f);
        glg.padding = new RectOffset(16, 16, 16, 16);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.childAlignment = TextAnchor.UpperLeft;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = Vector2.one;
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;
        sr.content = contentRt;

        _gridContent = content.transform;

        // ════════════════════════════════════════════════════════════════
        //  LEFT PANEL HEADER & FILTERS (drawn on top of ScrollRect)
        // ════════════════════════════════════════════════════════════════

        GameObject leftPanel = CreateChild("LeftPanel", transform);
        SetAnchors(leftPanel, Vector2.zero, new Vector2(SplitX, 1f));
        BuildLeftPanel(leftPanel.transform);

        // ════════════════════════════════════════════════════════════════
        //  VERTICAL SEPARATOR
        // ════════════════════════════════════════════════════════════════

        GameObject separator = CreateChild("Separator", transform);
        Image sepImg = separator.AddComponent<Image>();
        sepImg.color = new Color(SeparatorGold.r, SeparatorGold.g, SeparatorGold.b, 0.3f);
        sepImg.raycastTarget = false;
        RectTransform sepRt = separator.GetComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(SplitX, 0f);
        sepRt.anchorMax = new Vector2(SplitX, 1f);
        sepRt.offsetMin = new Vector2(-0.5f, 0f);
        sepRt.offsetMax = new Vector2(0.5f, 0f);

        // ════════════════════════════════════════════════════════════════
        //  RIGHT PANEL (0.72,0 → 1,1)
        // ════════════════════════════════════════════════════════════════

        GameObject rightPanel = CreateChild("RightPanel", transform);
        Image rpBg = rightPanel.AddComponent<Image>();
        rpBg.color = DetailBg;
        rpBg.raycastTarget = false;
        SetAnchors(rightPanel, new Vector2(SplitX, 0f), Vector2.one);

        BuildRightPanel(rightPanel.transform);
    }

    // ── LEFT PANEL ──────────────────────────────────────────────────────

    private void BuildLeftPanel(Transform parent)
    {
        // ── Top bar (title + back + count) ──────────────────────────────
        float topBarH = 80f / 1080f;

        GameObject topBar = CreateChild("TopBar", parent);
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = TopBarBg;
        topBarImg.raycastTarget = false;
        SetAnchors(topBar, new Vector2(0f, 1f - topBarH), Vector2.one);

        // Title
        GameObject titleObj = CreateChild("Title", topBar.transform);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "ROSTER";
        titleTmp.fontSize = 32;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = Parchment;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        titleTmp.raycastTarget = false;
        SetAnchors(titleObj, new Vector2(0.02f, 0f), new Vector2(0.18f, 1f));

        // Hero count
        GameObject countObj = CreateChild("HeroCount", topBar.transform);
        _heroCountLabel = countObj.AddComponent<TextMeshProUGUI>();
        _heroCountLabel.fontSize = 18;
        _heroCountLabel.color = SteelBlue;
        _heroCountLabel.alignment = TextAlignmentOptions.MidlineLeft;
        _heroCountLabel.raycastTarget = false;
        SetAnchors(countObj, new Vector2(0.18f, 0f), new Vector2(0.35f, 1f));

        // Back to Hub button
        GameObject backBtnObj = CreateChild("BackBtn", topBar.transform);
        Image backBtnImg = backBtnObj.AddComponent<Image>();
        backBtnImg.color = ButtonBack;
        Button backBtn = backBtnObj.AddComponent<Button>();
        backBtn.transition = Selectable.Transition.None;
        backBtn.targetGraphic = backBtnImg;
        backBtn.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        SetAnchors(backBtnObj, new Vector2(0.88f, 0.12f), new Vector2(0.99f, 0.88f));

        GameObject backLabel = CreateChild("BackLabel", backBtnObj.transform);
        TextMeshProUGUI backTmp = backLabel.AddComponent<TextMeshProUGUI>();
        backTmp.text = "HUB";
        backTmp.fontSize = 18;
        backTmp.fontStyle = FontStyles.Bold;
        backTmp.color = Parchment;
        backTmp.alignment = TextAlignmentOptions.Center;
        backTmp.raycastTarget = false;
        Stretch(backLabel);

        // ── Filter bar ──────────────────────────────────────────────────
        float filterH = 60f / 1080f;
        float filterTop = 1f - topBarH;

        GameObject filterBar = CreateChild("FilterBar", parent);
        Image filterBarImg = filterBar.AddComponent<Image>();
        filterBarImg.color = new Color(0.05f, 0.06f, 0.09f, 0.95f);
        filterBarImg.raycastTarget = false;
        SetAnchors(filterBar, new Vector2(0f, filterTop - filterH), new Vector2(1f, filterTop));

        // Horizontal scroll for filter buttons
        GameObject filterScroll = CreateChild("FilterScroll", filterBar.transform);
        ScrollRect filterSr = filterScroll.AddComponent<ScrollRect>();
        filterSr.horizontal = true;
        filterSr.vertical = false;
        filterSr.movementType = ScrollRect.MovementType.Elastic;
        Stretch(filterScroll);

        GameObject filterViewport = CreateChild("FilterViewport", filterScroll.transform);
        filterViewport.AddComponent<RectMask2D>();
        Stretch(filterViewport);
        filterSr.viewport = filterViewport.GetComponent<RectTransform>();

        GameObject filterContent = CreateChild("FilterContent", filterViewport.transform);
        HorizontalLayoutGroup hlg = filterContent.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6;
        hlg.padding = new RectOffset(8, 8, 4, 4);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        ContentSizeFitter filterFitter = filterContent.AddComponent<ContentSizeFitter>();
        filterFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        filterFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        RectTransform filterContentRt = filterContent.GetComponent<RectTransform>();
        filterContentRt.anchorMin = new Vector2(0f, 0f);
        filterContentRt.anchorMax = new Vector2(0f, 1f);
        filterContentRt.pivot = new Vector2(0f, 0.5f);
        filterContentRt.offsetMin = Vector2.zero;
        filterContentRt.offsetMax = Vector2.zero;
        filterSr.content = filterContentRt;

        foreach (string cls in _filterClasses)
        {
            CreateFilterButton(cls, filterContent.transform);
        }
    }

    // ── RIGHT PANEL ─────────────────────────────────────────────────────

    private void BuildRightPanel(Transform parent)
    {
        // ── Placeholder (visible at start) ──────────────────────────────
        _placeholderObj = CreateChild("Placeholder", parent);
        Stretch(_placeholderObj);

        GameObject phText = CreateChild("PlaceholderText", _placeholderObj.transform);
        TextMeshProUGUI phTmp = phText.AddComponent<TextMeshProUGUI>();
        phTmp.text = "SELECT A HERO";
        phTmp.fontSize = 28;
        phTmp.fontStyle = FontStyles.Bold;
        phTmp.color = Parchment;
        phTmp.alignment = TextAlignmentOptions.Center;
        phTmp.raycastTarget = false;
        Stretch(phText);

        // ── Detail content (hidden at start) ────────────────────────────
        _detailContent = CreateChild("DetailContent", parent);
        Stretch(_detailContent);

        // Portrait frame (top 35%)
        GameObject portraitFrame = CreateChild("PortraitFrame", _detailContent.transform);
        Image frameImg = portraitFrame.AddComponent<Image>();
        frameImg.color = CardFill;
        frameImg.raycastTarget = false;
        SetAnchors(portraitFrame, new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.97f));

        GameObject portraitObj = CreateChild("Portrait", portraitFrame.transform);
        _detailPortrait = portraitObj.AddComponent<Image>();
        _detailPortrait.preserveAspect = true;
        _detailPortrait.raycastTarget = false;
        RectTransform pRt = portraitObj.GetComponent<RectTransform>();
        pRt.anchorMin = Vector2.zero;
        pRt.anchorMax = Vector2.one;
        pRt.offsetMin = new Vector2(4f, 4f);
        pRt.offsetMax = new Vector2(-4f, -4f);

        // Hero name
        GameObject nameObj = CreateChild("HeroName", _detailContent.transform);
        _detailName = nameObj.AddComponent<TextMeshProUGUI>();
        _detailName.fontSize = 26;
        _detailName.fontStyle = FontStyles.Bold;
        _detailName.color = Parchment;
        _detailName.alignment = TextAlignmentOptions.Center;
        _detailName.raycastTarget = false;
        SetAnchors(nameObj, new Vector2(0.04f, 0.57f), new Vector2(0.96f, 0.62f));

        // Stars Row
        GameObject starsObj = CreateChild("StarsRow", _detailContent.transform);
        _detailStars = starsObj.AddComponent<TextMeshProUGUI>();
        _detailStars.fontSize = 20;
        _detailStars.color = AccentGold;
        _detailStars.alignment = TextAlignmentOptions.Center;
        _detailStars.raycastTarget = false;
        SetAnchors(starsObj, new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.57f));

        // Class Label
        GameObject classObj = CreateChild("ClassLabel", _detailContent.transform);
        _detailClass = classObj.AddComponent<TextMeshProUGUI>();
        _detailClass.fontSize = 18;
        _detailClass.color = SteelBlue;
        _detailClass.alignment = TextAlignmentOptions.Center;
        _detailClass.raycastTarget = false;
        SetAnchors(classObj, new Vector2(0.04f, 0.47f), new Vector2(0.96f, 0.52f));

        // Divider
        GameObject divider = CreateChild("Divider", _detailContent.transform);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = DividerColor;
        divImg.raycastTarget = false;
        SetAnchors(divider, new Vector2(0.12f, 0.455f), new Vector2(0.88f, 0.458f));

        // Stats block (ATK and DEF on line 1, HP on line 2, fontSize 20)
        GameObject statsObj = CreateChild("Stats", _detailContent.transform);
        _detailStats = statsObj.AddComponent<TextMeshProUGUI>();
        _detailStats.fontSize = 20;
        _detailStats.color = Parchment;
        _detailStats.alignment = TextAlignmentOptions.Center;
        _detailStats.raycastTarget = false;
        SetAnchors(statsObj, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.44f));

        // Morale Label
        GameObject moraleObj = CreateChild("Morale", _detailContent.transform);
        _detailMorale = moraleObj.AddComponent<TextMeshProUGUI>();
        _detailMorale.fontSize = 20;
        _detailMorale.color = SteelBlue;
        _detailMorale.alignment = TextAlignmentOptions.Center;
        _detailMorale.raycastTarget = false;
        SetAnchors(moraleObj, new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.33f));

        // Morale Bar Background (#1A1E2E)
        GameObject moraleBarBg = CreateChild("MoraleBarBg", _detailContent.transform);
        Image bgImg = moraleBarBg.AddComponent<Image>();
        bgImg.color = new Color(0.102f, 0.118f, 0.180f);
        bgImg.raycastTarget = false;
        SetAnchors(moraleBarBg, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.27f));

        // Morale Bar Fill (#3A8A4A)
        GameObject moraleBarFill = CreateChild("MoraleBarFill", moraleBarBg.transform);
        Image fillImg = moraleBarFill.AddComponent<Image>();
        fillImg.color = new Color(0.227f, 0.541f, 0.290f);
        fillImg.raycastTarget = false;
        _moraleBarFill = moraleBarFill.GetComponent<RectTransform>();
        _moraleBarFill.anchorMin = Vector2.zero;
        _moraleBarFill.anchorMax = new Vector2(1f, 1f);
        _moraleBarFill.offsetMin = Vector2.zero;
        _moraleBarFill.offsetMax = Vector2.zero;

        // Trait below morale bar
        GameObject traitObj = CreateChild("Trait", _detailContent.transform);
        _detailTrait = traitObj.AddComponent<TextMeshProUGUI>();
        _detailTrait.fontSize = 16;
        _detailTrait.color = SteelBlue;
        _detailTrait.alignment = TextAlignmentOptions.Center;
        _detailTrait.raycastTarget = false;
        SetAnchors(traitObj, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.23f));

        // ── Bottom buttons ──────────────────────────────────────────────

        // DISMISS (height 70px, fontSize 22)
        GameObject dismissObj = CreateChild("DismissBtn", _detailContent.transform);
        Image dismissImg = dismissObj.AddComponent<Image>();
        dismissImg.color = ButtonDismiss;
        Button dismissBtn = dismissObj.AddComponent<Button>();
        dismissBtn.transition = Selectable.Transition.None;
        dismissBtn.targetGraphic = dismissImg;
        dismissBtn.onClick.AddListener(OnDismiss);
        SetAnchors(dismissObj, new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.04f));
        RectTransform dismissRt = dismissObj.GetComponent<RectTransform>();
        dismissRt.pivot = new Vector2(0.5f, 0f);
        dismissRt.sizeDelta = new Vector2(0f, 70f);

        GameObject dismissLabel = CreateChild("Label", dismissObj.transform);
        TextMeshProUGUI dismissTmp = dismissLabel.AddComponent<TextMeshProUGUI>();
        dismissTmp.text = "DISMISS";
        dismissTmp.fontSize = 22;
        dismissTmp.fontStyle = FontStyles.Bold;
        dismissTmp.color = Parchment;
        dismissTmp.alignment = TextAlignmentOptions.Center;
        dismissTmp.raycastTarget = false;
        Stretch(dismissLabel);

        // BACK (deselect) — top-left corner of detail panel
        _backDetailBtn = CreateChild("BackDetailBtn", parent);
        _backDetailBtn.transform.SetAsFirstSibling();
        Image backDImg = _backDetailBtn.AddComponent<Image>();
        backDImg.color = ButtonBack;
        Button backDBtn = _backDetailBtn.AddComponent<Button>();
        backDBtn.transition = Selectable.Transition.None;
        backDBtn.targetGraphic = backDImg;
        backDBtn.onClick.AddListener(DeselectHero);
        RectTransform backDRt = _backDetailBtn.GetComponent<RectTransform>();
        backDRt.anchorMin = new Vector2(0f, 1f);
        backDRt.anchorMax = new Vector2(0f, 1f);
        backDRt.pivot = new Vector2(0f, 1f);
        backDRt.sizeDelta = new Vector2(110f, 45f);
        backDRt.anchoredPosition = new Vector2(8f, -8f);

        GameObject backDLabel = CreateChild("Label", _backDetailBtn.transform);
        TextMeshProUGUI backDTmp = backDLabel.AddComponent<TextMeshProUGUI>();
        backDTmp.text = "BACK";
        backDTmp.fontSize = 18;
        backDTmp.fontStyle = FontStyles.Bold;
        backDTmp.color = Parchment;
        backDTmp.alignment = TextAlignmentOptions.Center;
        backDTmp.raycastTarget = false;
        Stretch(backDLabel);

        _backDetailBtn.SetActive(false);

        // Start with placeholder visible, detail hidden
        _detailContent.SetActive(false);
        _placeholderObj.SetActive(true);
    }

    // ── Filter button factory ───────────────────────────────────────────

    private void CreateFilterButton(string className, Transform parent)
    {
        GameObject btnRoot = CreateChild("Filter_" + className, parent);
        LayoutElement le = btnRoot.AddComponent<LayoutElement>();
        le.minWidth = 120;
        le.preferredWidth = 120;
        le.minHeight = 40;

        Image borderImg = btnRoot.AddComponent<Image>();
        borderImg.color = className == "ALL" ? FilterBorderActive : FilterBorderInactive;

        GameObject inner = CreateChild("Inner", btnRoot.transform);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = className == "ALL" ? FilterActive : FilterInactive;
        innerImg.raycastTarget = false;
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(2f, 2f);
        innerRt.offsetMax = new Vector2(-2f, -2f);

        GameObject label = CreateChild("Label", inner.transform);
        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = className;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = className == "ALL" ? BgColor : Parchment;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        Stretch(label);

        Button btn = btnRoot.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.targetGraphic = borderImg;
        string captured = className;
        btn.onClick.AddListener(() => OnFilterClicked(captured));

        _filterButtons[className] = btn;
        _filterBorders[className] = borderImg;
    }

    // ════════════════════════════════════════════════════════════════════
    //  GRID LOGIC
    // ════════════════════════════════════════════════════════════════════

    private void RefreshGrid()
    {
        foreach (RosterHeroCard card in _spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        _spawnedCards.Clear();

        _allHeroes = _roster.GetAll();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (_activeFilter == "ALL")
        {
            _filtered = new List<HeroInstance>(_allHeroes);
        }
        else
        {
            _filtered = _allHeroes.Where(h =>
            {
                HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(h.HeroDefId);
                if (def == null) return false;
                string classStr = !string.IsNullOrEmpty(def.HeroClass)
                    ? def.HeroClass
                    : def.BaseClass.ToString();
                return string.Equals(classStr, _activeFilter, System.StringComparison.OrdinalIgnoreCase);
            }).ToList();
        }

        _filtered = _filtered.OrderByDescending(h => h.CurrentStarRank)
            .ThenBy(h =>
            {
                HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(h.HeroDefId);
                return HeroPresentationUtility.GetDisplayName(def, h.HeroDefId);
            }).ToList();

        // Destroy old cards
        foreach (RosterHeroCard card in _spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        _spawnedCards.Clear();

        // Spawn new cards
        foreach (HeroInstance hero in _filtered)
        {
            GameObject cardObj = new GameObject("HeroCard_" + hero.HeroDefId);
            cardObj.transform.SetParent(_gridContent, false);
            RosterHeroCard card = cardObj.AddComponent<RosterHeroCard>();
            card.Setup(hero, OnCardTapped);
            if (_selectedHero != null && hero.InstanceId == _selectedHero.InstanceId)
            {
                card.SetSelected(true);
            }
            _spawnedCards.Add(card);
        }

        if (_heroCountLabel != null)
        {
            _heroCountLabel.text = _filtered.Count + " / " + _allHeroes.Count;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  FILTER LOGIC
    // ════════════════════════════════════════════════════════════════════

    private void OnFilterClicked(string className)
    {
        _activeFilter = className;

        foreach (var kvp in _filterButtons)
        {
            bool active = kvp.Key == _activeFilter;
            Image border = _filterBorders[kvp.Key];
            border.color = active ? FilterBorderActive : FilterBorderInactive;

            Transform inner = kvp.Value.transform.Find("Inner");
            if (inner != null)
            {
                Image innerImg = inner.GetComponent<Image>();
                if (innerImg != null) innerImg.color = active ? FilterActive : FilterInactive;

                Transform labelT = inner.Find("Label");
                if (labelT != null)
                {
                    TextMeshProUGUI tmp = labelT.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.color = active ? BgColor : Parchment;
                }
            }
        }

        ApplyFilter();
    }

    // ════════════════════════════════════════════════════════════════════
    //  DETAIL / RIGHT PANEL
    // ════════════════════════════════════════════════════════════════════

    private void OnCardTapped(HeroInstance hero)
    {
        foreach (RosterHeroCard card in _spawnedCards)
        {
            if (card != null && card.Hero != null)
            {
                bool isTarget = card.Hero.InstanceId == hero.InstanceId;
                card.SetSelected(isTarget);
            }
        }

        if (_backDetailBtn != null) _backDetailBtn.SetActive(true);

        _selectedHero = hero;
        _selectedDef = HeroPresentationUtility.LoadHeroDefinition(hero.HeroDefId);
        PopulateDetail();
        _placeholderObj.SetActive(false);
        _detailContent.SetActive(true);
    }

    private void PopulateDetail()
    {
        if (_selectedHero == null) return;

        string heroName = HeroPresentationUtility.GetDisplayName(_selectedDef, _selectedHero.HeroDefId);
        _detailName.text = heroName.ToUpperInvariant();

        // Stars Row
        int stars = 1;
        if (_selectedHero != null) stars = _selectedHero.CurrentStarRank;
        else if (_selectedDef != null) stars = _selectedDef.BaseStarRank;

        Debug.Log("[RosterView] Hero star rank: " + stars + ", BaseStarRank: " + (_selectedDef != null ? _selectedDef.BaseStarRank.ToString() : "null"));
        _detailStars.text = string.Empty.PadRight(stars, '+');

        // Class Label
        string classLabel = _selectedDef != null
            ? HeroPresentationUtility.GetRoleLabel(_selectedDef)
            : "UNKNOWN";
        _detailClass.text = classLabel.ToUpperInvariant();

        // Morale Label
        _detailMorale.text = "Morale: " + _selectedHero.Morale + " / 100";

        // Morale Bar Fill
        if (_moraleBarFill != null)
        {
            float fillPct = Mathf.Clamp01(_selectedHero.Morale / 100f);
            _moraleBarFill.anchorMax = new Vector2(fillPct, 1f);
        }

        // Trait
        _detailTrait.text = "Trait: " + _selectedHero.Personality.ToString().ToUpperInvariant();
        _detailTrait.color = HeroColorUtility.GetTraitColor(_selectedHero.Personality);

        // Stats
        int atkVal = _selectedDef != null ? _selectedDef.BaseATK : 0;
        int defVal = _selectedDef != null ? _selectedDef.BaseDEF : 0;
        int hpVal = _selectedDef != null ? _selectedDef.BaseHP : 0;
        _detailStats.text =
            "ATK " + atkVal + "    DEF " + defVal + "\n" +
            "HP  " + hpVal;

        // Portrait
        _detailPortrait.sprite = null;
        _detailPortrait.color = GetRarityPlaceholderColor(_selectedHero.CurrentStarRank);

        if (_selectedDef != null && !string.IsNullOrEmpty(_selectedDef.PortraitSpritePath))
        {
            Sprite sp = Resources.Load<Sprite>(_selectedDef.PortraitSpritePath);
            if (sp != null)
            {
                _detailPortrait.sprite = sp;
                _detailPortrait.color = _selectedHero.IsAlive ? Color.white : new Color(0.35f, 0.35f, 0.35f);
            }
        }

        if (_detailPortrait.sprite == null && _selectedDef != null && _selectedDef.Portrait != null)
        {
            _detailPortrait.sprite = _selectedDef.Portrait;
            _detailPortrait.color = _selectedHero.IsAlive ? Color.white : new Color(0.35f, 0.35f, 0.35f);
        }
    }

    private void DeselectHero()
    {
        foreach (RosterHeroCard card in _spawnedCards)
        {
            if (card != null)
            {
                card.SetSelected(false);
            }
        }
        if (_backDetailBtn != null) _backDetailBtn.SetActive(false);
        _selectedHero = null;
        _selectedDef = null;
        _detailContent.SetActive(false);
        _placeholderObj.SetActive(true);
    }

    private void OnDismiss()
    {
        if (_selectedHero == null) return;

        _roster.Remove(_selectedHero.InstanceId);
        _selectedHero = null;
        _selectedDef = null;
        if (_backDetailBtn != null) _backDetailBtn.SetActive(false);
        _detailContent.SetActive(false);
        _placeholderObj.SetActive(true);
        _allHeroes = _roster.GetAll();
        ApplyFilter();
    }

    // ════════════════════════════════════════════════════════════════════
    //  UTILITY
    // ════════════════════════════════════════════════════════════════════

    private static GameObject CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(GameObject go, Vector2 min, Vector2 max)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Color GetRarityPlaceholderColor(int starRank)
    {
        return starRank switch
        {
            1 => new Color(0.290f, 0.290f, 0.353f),
            2 => new Color(0.227f, 0.353f, 0.290f),
            3 => new Color(0.165f, 0.290f, 0.416f),
            4 => new Color(0.353f, 0.227f, 0.478f),
            5 => new Color(0.478f, 0.290f, 0.165f),
            _ => new Color(0.290f, 0.290f, 0.353f),
        };
    }
}
