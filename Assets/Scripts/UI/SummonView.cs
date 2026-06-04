using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// MOEBIUS SUMMON — Ritual Invocation System (Redesigned)
///
/// Features:
/// - Procedural UI generation via BuildFullHierarchy
/// - RarityVisualConfig for configurable colors/glow/particles
/// - SummonAnimationController for double-tap skip with rarity gate
/// - InfoPanel replacing "ISEL" with Featured Unit + Drop Rates
/// - Typography hierarchy: hero name 52px Bold, class 36px, buttons 36px Bold
/// - INVOKE x10 visually dominant over INVOKE x1
/// - Radial gradient background
/// </summary>
public class SummonView : MonoBehaviour
{
    // ── Rarity config (scriptable, not hardcoded) ─────────────────────────
    [Header("Config (Loaded from Resources)")]
    [SerializeField] private string rarityConfigPath = "Configs/RarityVisualConfig";
    [SerializeField] private string featuredUnitConfigPath = "Configs/FeaturedUnitConfig";

    private RarityVisualConfig _rarityConfig;
    private FeaturedUnitConfig _featuredConfig;

    // ── Rarity helpers (backward compat) ─────────────────────────────────
    private static readonly Color[] RarityColors = new Color[]
    {
        new Color(0.35f, 0.35f, 0.38f), // 1* grey-void
        new Color(0.30f, 0.69f, 0.31f), // 2* green
        new Color(0.18f, 0.59f, 0.95f), // 3* blue
        new Color(0.61f, 0.15f, 0.69f), // 4* purple
        new Color(1f, 0.76f, 0.03f),    // 5* gold
    };

    private static readonly Color[] TabColors =
    {
        new Color(0.25f, 0.18f, 0.05f, 1f),
        new Color(0.12f, 0.10f, 0.28f, 1f),
        new Color(0.08f, 0.16f, 0.10f, 1f),
    };

    private static readonly Color[] TabActiveColors =
    {
        new Color(0.65f, 0.50f, 0.10f, 1f),
        new Color(0.35f, 0.20f, 0.65f, 1f),
        new Color(0.15f, 0.45f, 0.20f, 1f),
    };

    // ── Design Tokens ────────────────────────────────────────────────────
    private static readonly Color ColorBgCenter = new Color(0.08f, 0.10f, 0.15f, 1f);    // dark navy
    private static readonly Color ColorBgEdge   = new Color(0.05f, 0.06f, 0.10f, 1f);    // deeper navy
    private static readonly Color ColorPanelBg  = new Color(0.08f, 0.10f, 0.15f, 0.92f); // navy @ 92%
    private static readonly Color ColorPrimary  = new Color(0.20f, 0.28f, 0.50f, 1f);    // navy-blue
    private static readonly Color ColorPrimaryDark = new Color(0.12f, 0.18f, 0.35f, 1f); // deep navy
    private static readonly Color ColorAccentGold = new Color(1f, 0.76f, 0.03f, 1f);    // #ffc107
    private static readonly Color ColorTextSecondary = new Color(0.70f, 0.61f, 0.86f);  // #b39ddb

    // ── Isel dialogue (kept for flavor) ──────────────────────────────────
    private const string IselGreeting = "The circle is ready, Master.";
    private const string IselSummonStart = "Initiating summon...";

    private static readonly string[] IselLines1Star = { "Unremarkable.", "They will serve.", "Adequate." };
    private static readonly string[] IselLines3Star = { "Capable.", "Potential detected.", "3-star resonance." };
    private static readonly string[] IselLines4StarPlus = { "This one is different.", "The Circle chose well.", "Remarkable." };

    private static readonly string[] TabDialogue =
    {
        "Standard pool.\nGold cost applies.",
        "Higher resonance.\nGem cost applies.",
        "Equipment: pending."
    };

    // ── Scene objects ────────────────────────────────────────────────────
    private GameObject _summoningCircle;
    private GameObject _effectsLayer;
    private GameObject _bottomPanel;
    private GameObject _iselPanel;
    private GameObject _infoPanel;
    private GameObject _topBar;
    private GameObject _skipModal;
    private GameObject _idleStateRoot; // New: idle circle + text

    private Image _dimOverlay;
    private Image _ritualCircleImage;
    private Image _environmentGlow;
    private Image _silhouetteImage;
    private Image _materializeImage;
    private Image _crackOverlay;

    private TextMeshProUGUI _iselDialogueTMP;
    private TextMeshProUGUI _statusResultTMP;
    private TextMeshProUGUI _invokeCostLabel;
    private TextMeshProUGUI _goldDisplay;
    private TextMeshProUGUI _gemsDisplay;
    private TextMeshProUGUI _heroNameTMP;
    private TextMeshProUGUI _heroClassTMP;
    private TextMeshProUGUI _rarityLabelTMP;

    private Button[] _tabButtons;
    private Image[]  _tabImages;
    private Button _invokeSingleBtn;
    private Button _invokeTenBtn;
    private Button _backBtn;
    private Image  _invokeSingleImg;
    private Image  _invokeTenImg;
    private Image  _invokeTenBorder;

    private SummonAnimationController _animController;

    private IGachaService    _gacha;
    private ICurrencyService _currency;

    // ── Pity Progress ────────────────────────────────────────────────────
    [Header("Pity UI (assign in Inspector)")]
    [SerializeField] private Image pityFillImage;
    [SerializeField] private TextMeshProUGUI pityText;

    // ── State ────────────────────────────────────────────────────────────
    private bool _isAnimating;
    private bool _doubleTapSkip;
    private float _lastTapTime;
    private int  _activeTab;
    private int  _sessionTotalSummons;
    private int  _sessionFiveStars;
    private float _speedMultiplier = 1f;

    // ── Delays ───────────────────────────────────────────────────────────
    private const float PrepDuration           = 0.4f;
    private const float TensionDelay           = 0.8f;
    private const float ActivateDuration       = 0.8f;
    private const float DistortionDuration     = 0.6f;
    private const float MaterializeDuration    = 1.2f;
    private const float CardRevealDuration     = 0.5f;
    private const float CompleteFadeDuration   = 0.8f;
    private const float TapSkipWindow          = 0.3f;

    private const float CircleScaleNormal   = 1.0f;
    private const float CircleScaleDominant = 1.8f;

    // ═══════════════════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════

    private void Start()
    {
        _gacha    = ServiceRegistry.Instance.Resolve<IGachaService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();

        // Load configs
        _rarityConfig = Resources.Load<RarityVisualConfig>(rarityConfigPath);
        if (_rarityConfig == null)
            Debug.LogWarning("[SummonView] RarityVisualConfig not found at " + rarityConfigPath);

        _featuredConfig = Resources.Load<FeaturedUnitConfig>(featuredUnitConfigPath);
        if (_featuredConfig == null)
            Debug.LogWarning("[SummonView] FeaturedUnitConfig not found at " + featuredUnitConfigPath);

        BuildFullHierarchy();
        WireButtons();
        ShowIselDialogue(IselGreeting);
        RefreshUI();
        StartCoroutine(CircleIdlePulse());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  BUILD HIERARCHY — Full redesign
    // ═══════════════════════════════════════════════════════════════════════
    //
    //  Canvas
    //   ├── Background (radial gradient: #1a0a2e center → #0d0518 edge)
    //   ├── SummoningCircle (CENTER, dominant - 1.8x)
    //   ├── EffectsLayer (glow, crack, silhouette, materialize)
    //   ├── InfoPanel (right — replaces ISEL: FeaturedUnit + DropRates)
    //   ├── SkipConfirmationModal (optional, for 4★+ skip gates)
    //   ├── BottomPanel (tabs + invoke buttons ONLY)
    //   └── TopBar (back + gold + gems + pity)

    private void BuildFullHierarchy()
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        Transform root = rootCanvas != null ? rootCanvas.transform : transform;
        RemoveGeneratedChildren(root);

        // ── Background (radial feel via two-layer gradient) ──
        GameObject bg = CreatePanel("Background", root, Vector2.zero, Vector2.one);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = ColorBgCenter;

        // Inner radial fade overlay
        GameObject bgInner = new GameObject("BgInnerGlow", typeof(RectTransform), typeof(Image));
        bgInner.transform.SetParent(bg.transform, false);
        RectTransform bgInnerRT = bgInner.GetComponent<RectTransform>();
        bgInnerRT.anchorMin = new Vector2(0.15f, 0.05f);
        bgInnerRT.anchorMax = new Vector2(0.85f, 0.85f);
        bgInnerRT.offsetMin = Vector2.zero;
        bgInnerRT.offsetMax = Vector2.zero;
        Image bgInnerImg = bgInner.GetComponent<Image>();
        bgInnerImg.color = new Color(0.15f, 0.06f, 0.25f, 0.6f);

        // Dim overlay
        _dimOverlay = CreateFullRectImage("DimOverlay", bg.transform);
        _dimOverlay.color = new Color(0f, 0f, 0f, 0f);

        // ── SummoningCircle — BIG, dominant ──
        _summoningCircle = new GameObject("SummoningCircle", typeof(RectTransform), typeof(Image));
        _summoningCircle.transform.SetParent(root, false);
        RectTransform circleRT = _summoningCircle.GetComponent<RectTransform>();
        float cSize = 0.76f;
        circleRT.anchorMin = new Vector2((1f - cSize) * 0.5f + 0.05f, 0.08f);
        circleRT.anchorMax = new Vector2((1f + cSize) * 0.5f - 0.05f, 0.85f);
        circleRT.offsetMin = Vector2.zero;
        circleRT.offsetMax = Vector2.zero;
        _ritualCircleImage = _summoningCircle.GetComponent<Image>();
        _ritualCircleImage.color = new Color(0.30f, 0.20f, 0.55f, 0.05f);

        // ── EffectsLayer ──
        _effectsLayer = new GameObject("EffectsLayer", typeof(RectTransform), typeof(Image));
        _effectsLayer.transform.SetParent(root, false);
        SetAnchorsRect(_effectsLayer.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        _environmentGlow = _effectsLayer.GetComponent<Image>();
        _environmentGlow.color = new Color(0.30f, 0.20f, 0.55f, 0f);

        _crackOverlay = CreateFullRectImage("CrackOverlay", _effectsLayer.transform);
        _crackOverlay.color = new Color(0f, 0f, 0f, 0f);

        // Silhouette
        GameObject silGO = new GameObject("Silhouette", typeof(RectTransform), typeof(Image));
        silGO.transform.SetParent(_effectsLayer.transform, false);
        RectTransform silRT = silGO.GetComponent<RectTransform>();
        silRT.anchorMin = new Vector2(0.30f, 0.15f);
        silRT.anchorMax = new Vector2(0.70f, 0.75f);
        silRT.offsetMin = Vector2.zero;
        silRT.offsetMax = Vector2.zero;
        _silhouetteImage = silGO.GetComponent<Image>();
        _silhouetteImage.color = new Color(0.05f, 0.02f, 0.12f, 0f);

        // Materialize image (character)
        GameObject matGO = new GameObject("MaterializeImage", typeof(RectTransform), typeof(Image));
        matGO.transform.SetParent(_effectsLayer.transform, false);
        RectTransform matRT = matGO.GetComponent<RectTransform>();
        matRT.anchorMin = new Vector2(0.22f, 0.10f);
        matRT.anchorMax = new Vector2(0.78f, 0.80f);
        matRT.offsetMin = Vector2.zero;
        matRT.offsetMax = Vector2.zero;
        _materializeImage = matGO.GetComponent<Image>();
        _materializeImage.color = new Color(1f, 1f, 1f, 0f);

        // ── InfoPanel (replaces ISEL) — Featured Unit + Drop Rates ──
        BuildInfoPanel(root);

        // ── SkipConfirmationModal ──
        BuildSkipModal(root);

        // ── BottomPanel ──
        BuildBottomPanel(root);

        // ── TopBar ──
        BuildTopBar(root);

        // ── Status result text ──
        BuildStatusText(root);

        // ── Wire up SummonAnimationController ──
        _animController = GetComponent<SummonAnimationController>();
        if (_animController == null)
        {
            _animController = gameObject.AddComponent<SummonAnimationController>();
            // Wire modal references
            var modalField = typeof(SummonAnimationController).GetField("skipConfirmationModal",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (modalField != null && _skipModal != null)
                modalField.SetValue(_animController, _skipModal);

            var textField = typeof(SummonAnimationController).GetField("skipModalText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (textField != null)
            {
                TextMeshProUGUI modalText = _skipModal?.GetComponentInChildren<TextMeshProUGUI>();
                if (modalText != null)
                    textField.SetValue(_animController, modalText);
            }
        }
    }

    // ── NEW: InfoPanel — Featured Unit + Drop Rates ──────────────────────
    private void BuildInfoPanel(Transform root)
    {
        _infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        _infoPanel.transform.SetParent(root, false);
        RectTransform r = _infoPanel.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.68f, 0.32f);
        r.anchorMax = new Vector2(0.98f, 0.88f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        _infoPanel.GetComponent<Image>().color = ColorPanelBg;

        // Border
        GameObject border = new GameObject("Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(_infoPanel.transform, false);
        SetAnchorsRect(border.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        border.GetComponent<RectTransform>().offsetMin = new Vector2(2f, 2f);
        border.GetComponent<RectTransform>().offsetMax = new Vector2(-2f, -2f);
        border.GetComponent<Image>().color = new Color(0.29f, 0.08f, 0.55f, 0.55f); // #4a148c border

        // ── Featured Unit Panel (top half) ──
        GameObject featuredGO = new GameObject("FeaturedUnitPanel", typeof(RectTransform), typeof(Image));
        featuredGO.transform.SetParent(_infoPanel.transform, false);
        RectTransform ftRT = featuredGO.GetComponent<RectTransform>();
        ftRT.anchorMin = new Vector2(0.03f, 0.50f);
        ftRT.anchorMax = new Vector2(0.97f, 0.97f);
        ftRT.offsetMin = Vector2.zero;
        ftRT.offsetMax = Vector2.zero;
        featuredGO.GetComponent<Image>().color = new Color(0.15f, 0.06f, 0.25f, 0.4f);

        // Header: "★ FEATURED UNIT" (24px Bold, design token)
        GameObject ftHeader = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        ftHeader.transform.SetParent(featuredGO.transform, false);
        SetAnchorsRect(ftHeader.GetComponent<RectTransform>(), new Vector2(0.04f, 0.80f), new Vector2(0.96f, 0.95f));
        TextMeshProUGUI ftHeaderTMP = ftHeader.GetComponent<TextMeshProUGUI>();
        ftHeaderTMP.text = "★ FEATURED UNIT";
        ftHeaderTMP.fontSize = 24f;  // Increased from 20f
        ftHeaderTMP.color = ColorAccentGold;
        ftHeaderTMP.fontStyle = FontStyles.Bold;
        ftHeaderTMP.alignment = TextAlignmentOptions.Center;

        // Hero portrait placeholder
        GameObject ftPortrait = new GameObject("PortraitPlaceholder", typeof(RectTransform), typeof(Image));
        ftPortrait.transform.SetParent(featuredGO.transform, false);
        RectTransform ftPrtRT = ftPortrait.GetComponent<RectTransform>();
        ftPrtRT.anchorMin = new Vector2(0.25f, 0.35f);
        ftPrtRT.anchorMax = new Vector2(0.75f, 0.78f);
        ftPrtRT.offsetMin = Vector2.zero;
        ftPrtRT.offsetMax = Vector2.zero;
        ftPortrait.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.20f, 0.80f);

        // Portrait text hint
        GameObject ftHint = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
        ftHint.transform.SetParent(ftPortrait.transform, false);
        SetAnchorsRect(ftHint.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        TextMeshProUGUI ftHintTMP = ftHint.GetComponent<TextMeshProUGUI>();
        ftHintTMP.text = _featuredConfig != null ? _featuredConfig.featuredUnit.heroName.ToUpper() : "BELQUIST";
        ftHintTMP.fontSize = 24f;
        ftHintTMP.color = ColorTextSecondary;
        ftHintTMP.alignment = TextAlignmentOptions.Center;
        ftHintTMP.fontStyle = FontStyles.Bold;

        // Hero name (36px Bold, design token)
        GameObject ftName = new GameObject("HeroName", typeof(RectTransform), typeof(TextMeshProUGUI));
        ftName.transform.SetParent(featuredGO.transform, false);
        SetAnchorsRect(ftName.GetComponent<RectTransform>(), new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.30f));
        TextMeshProUGUI ftNameTMP = ftName.GetComponent<TextMeshProUGUI>();
        ftNameTMP.text = _featuredConfig != null ? _featuredConfig.featuredUnit.heroName.ToUpper() : "BELQUIST";
        ftNameTMP.fontSize = 36f;  // Increased from 16f
        ftNameTMP.color = Color.white;
        ftNameTMP.alignment = TextAlignmentOptions.Center;
        ftNameTMP.fontStyle = FontStyles.Bold;

        // Subtitle (20px SemiBold, design token)
        GameObject ftSub = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        ftSub.transform.SetParent(featuredGO.transform, false);
        SetAnchorsRect(ftSub.GetComponent<RectTransform>(), new Vector2(0.04f, 0.15f), new Vector2(0.96f, 0.24f));
        TextMeshProUGUI ftSubTMP = ftSub.GetComponent<TextMeshProUGUI>();
        ftSubTMP.text = _featuredConfig != null ? _featuredConfig.featuredUnit.subtitle : "2x drop chance this week";
        ftSubTMP.fontSize = 20f;  // Increased from 14f
        ftSubTMP.color = ColorAccentGold;
        ftSubTMP.alignment = TextAlignmentOptions.Center;
        ftSubTMP.fontStyle = FontStyles.Bold;

        // ── Isel Dialogue line (24px Medium, design token) ──
        GameObject dialogueGO = new GameObject("IselDialogue", typeof(RectTransform), typeof(TextMeshProUGUI));
        dialogueGO.transform.SetParent(_infoPanel.transform, false);
        SetAnchorsRect(dialogueGO.GetComponent<RectTransform>(), new Vector2(0.04f, 0.47f), new Vector2(0.96f, 0.52f));
        _iselDialogueTMP = dialogueGO.GetComponent<TextMeshProUGUI>();
        _iselDialogueTMP.text = "";
        _iselDialogueTMP.fontSize = 24f;  // Increased from 17f
        _iselDialogueTMP.color = new Color(0.82f, 0.77f, 0.91f);  // #d1c4e9
        _iselDialogueTMP.alignment = TextAlignmentOptions.Center;
        _iselDialogueTMP.fontStyle = FontStyles.Bold;

        // ── Divider ──
        GameObject divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divider.transform.SetParent(_infoPanel.transform, false);
        SetAnchorsRect(divider.GetComponent<RectTransform>(), new Vector2(0.03f, 0.44f), new Vector2(0.97f, 0.46f));
        divider.GetComponent<Image>().color = new Color(0.50f, 0.30f, 0.70f, 0.30f);

        // ── Drop Rates Panel (bottom half) ──
        GameObject ratesGO = new GameObject("DropRatesPanel", typeof(RectTransform), typeof(Image));
        ratesGO.transform.SetParent(_infoPanel.transform, false);
        RectTransform rtRT = ratesGO.GetComponent<RectTransform>();
        rtRT.anchorMin = new Vector2(0.03f, 0.02f);
        rtRT.anchorMax = new Vector2(0.97f, 0.42f);
        rtRT.offsetMin = Vector2.zero;
        rtRT.offsetMax = Vector2.zero;
        ratesGO.GetComponent<Image>().color = new Color(0.08f, 0.04f, 0.14f, 0.5f);

        // Header: "DROP RATES" (24px Bold, design token)
        GameObject drHeader = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        drHeader.transform.SetParent(ratesGO.transform, false);
        SetAnchorsRect(drHeader.GetComponent<RectTransform>(), new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.95f));
        TextMeshProUGUI drHeaderTMP = drHeader.GetComponent<TextMeshProUGUI>();
        drHeaderTMP.text = "DROP RATES";
        drHeaderTMP.fontSize = 24f;  // Increased from 18f
        drHeaderTMP.color = Color.white;
        drHeaderTMP.fontStyle = FontStyles.Bold;
        drHeaderTMP.alignment = TextAlignmentOptions.Center;

        // Build drop rate bars from config or defaults
        BuildDropRateBars(ratesGO);
    }

    private void BuildDropRateBars(GameObject parent)
    {
        FeaturedUnitConfig.DropRateEntry[] entries;
        if (_featuredConfig != null && _featuredConfig.dropRates != null && _featuredConfig.dropRates.Length > 0)
            entries = _featuredConfig.dropRates;
        else
        {
            entries = new FeaturedUnitConfig.DropRateEntry[]
            {
                new FeaturedUnitConfig.DropRateEntry { label = "Common (1-2)", percentage = 60f, barColor = new Color(0.62f, 0.62f, 0.62f) },
                new FeaturedUnitConfig.DropRateEntry { label = "Rare (3)", percentage = 30f, barColor = new Color(0.18f, 0.59f, 0.95f) },
                new FeaturedUnitConfig.DropRateEntry { label = "Epic (4)", percentage = 8f, barColor = new Color(0.61f, 0.15f, 0.69f) },
                new FeaturedUnitConfig.DropRateEntry { label = "Legendary (5+)", percentage = 2f, barColor = new Color(1f, 0.76f, 0.03f) }
            };
        }

        float total = 0f;
        foreach (var e in entries) total += e.percentage;
        float yStart = 0.70f;
        float yStep = 0.20f;

        for (int i = 0; i < entries.Length; i++)
        {
            int idx = i;
            float yMin = yStart - (idx + 1) * yStep;
            float yMax = yStart - idx * yStep - 0.02f;

            // Row container (Horizontal Layout for proper alignment)
            GameObject row = new GameObject("RateRow_" + idx, typeof(RectTransform));
            row.transform.SetParent(parent.transform, false);
            SetAnchorsRect(row.GetComponent<RectTransform>(), new Vector2(0.05f, yMin), new Vector2(0.98f, yMax));

            // Label (left side, fixed width via anchor)
            GameObject lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(row.transform, false);
            SetAnchorsRect(lbl.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0.55f, 1f));
            TextMeshProUGUI lblTMP = lbl.GetComponent<TextMeshProUGUI>();
            lblTMP.text = entries[idx].label;
            lblTMP.fontSize = 22f;  // Increased from 12f
            lblTMP.color = new Color(0.80f, 0.75f, 0.90f);
            lblTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Bar container (stretches between label and percent)
            GameObject barContainer = new GameObject("BarContainer", typeof(RectTransform), typeof(Image));
            barContainer.transform.SetParent(row.transform, false);
            SetAnchorsRect(barContainer.GetComponent<RectTransform>(), new Vector2(0.56f, 0.15f), new Vector2(0.82f, 0.85f));
            Image barBg = barContainer.GetComponent<Image>();
            barBg.color = new Color(0.10f, 0.04f, 0.18f, 0.5f);

            // Bar fill (left-anchored, width driven by percentage)
            GameObject barFill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barContainer.transform, false);
            RectTransform barFillRT = barFill.GetComponent<RectTransform>();
            barFillRT.anchorMin = new Vector2(0f, 0f);
            barFillRT.anchorMax = new Vector2(0f, 1f); // Left anchored
            barFillRT.pivot = new Vector2(0f, 0.5f);
            float normW = entries[idx].percentage / Mathf.Max(total, 0.01f);
            barFillRT.sizeDelta = new Vector2(normW * 220f, 0f); // Fill width proportional to percentage
            barFill.GetComponent<Image>().color = entries[idx].barColor;

            // Percentage text (right side)
            GameObject pctLbl = new GameObject("Percent", typeof(RectTransform), typeof(TextMeshProUGUI));
            pctLbl.transform.SetParent(row.transform, false);
            SetAnchorsRect(pctLbl.GetComponent<RectTransform>(), new Vector2(0.84f, 0f), new Vector2(1f, 1f));
            TextMeshProUGUI pctTMP = pctLbl.GetComponent<TextMeshProUGUI>();
            pctTMP.text = $"{entries[idx].percentage}%";
            pctTMP.fontSize = 22f;  // Increased from 12f
            pctTMP.color = entries[idx].barColor;
            pctTMP.alignment = TextAlignmentOptions.MidlineRight;
            pctTMP.fontStyle = FontStyles.Bold;
        }
    }

    // ── NEW: Skip Confirmation Modal ──────────────────────────────────────
    private void BuildSkipModal(Transform root)
    {
        _skipModal = new GameObject("SkipConfirmationModal", typeof(RectTransform), typeof(Image));
        _skipModal.transform.SetParent(root, false);
        SetAnchorsRect(_skipModal.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        _skipModal.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.70f);
        _skipModal.SetActive(false);

        // Modal panel
        GameObject modalPanel = new GameObject("ModalPanel", typeof(RectTransform), typeof(Image));
        modalPanel.transform.SetParent(_skipModal.transform, false);
        RectTransform mpRT = modalPanel.GetComponent<RectTransform>();
        mpRT.anchorMin = new Vector2(0.15f, 0.35f);
        mpRT.anchorMax = new Vector2(0.85f, 0.65f);
        mpRT.offsetMin = Vector2.zero;
        mpRT.offsetMax = Vector2.zero;
        modalPanel.GetComponent<Image>().color = new Color(0.08f, 0.04f, 0.14f, 0.96f);

        // Border
        GameObject mpBorder = new GameObject("Border", typeof(RectTransform), typeof(Image));
        mpBorder.transform.SetParent(modalPanel.transform, false);
        SetAnchorsRect(mpBorder.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        mpBorder.GetComponent<RectTransform>().offsetMin = new Vector2(2f, 2f);
        mpBorder.GetComponent<RectTransform>().offsetMax = new Vector2(-2f, -2f);
        mpBorder.GetComponent<Image>().color = new Color(0.61f, 0.15f, 0.69f, 0.70f);

        // Text
        GameObject modalText = new GameObject("ModalText", typeof(RectTransform), typeof(TextMeshProUGUI));
        modalText.transform.SetParent(modalPanel.transform, false);
        SetAnchorsRect(modalText.GetComponent<RectTransform>(), new Vector2(0.05f, 0.50f), new Vector2(0.95f, 0.95f));
        TextMeshProUGUI mtTMP = modalText.GetComponent<TextMeshProUGUI>();
        mtTMP.text = "A 4★ hero appeared! Watch the celebration?";
        mtTMP.fontSize = 24f;
        mtTMP.color = ColorAccentGold;
        mtTMP.alignment = TextAlignmentOptions.Center;
        mtTMP.fontStyle = FontStyles.Bold;

        // Watch button (cancel skip)
        GameObject watchBtn = new GameObject("WatchButton", typeof(RectTransform), typeof(Image), typeof(Button));
        watchBtn.transform.SetParent(modalPanel.transform, false);
        SetAnchorsRect(watchBtn.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.42f));
        watchBtn.GetComponent<Image>().color = ColorPrimaryDark;
        GameObject watchLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        watchLbl.transform.SetParent(watchBtn.transform, false);
        SetAnchorsRect(watchLbl.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        TextMeshProUGUI watchTMP = watchLbl.GetComponent<TextMeshProUGUI>();
        watchTMP.text = "Watch Celebration";
        watchTMP.fontSize = 22f;
        watchTMP.color = ColorAccentGold;
        watchTMP.alignment = TextAlignmentOptions.Center;
        watchTMP.fontStyle = FontStyles.Bold;
        watchBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (_animController != null) _animController.CancelSkip();
        });

        // Skip anyway button
        GameObject skipBtn = new GameObject("SkipButton", typeof(RectTransform), typeof(Image), typeof(Button));
        skipBtn.transform.SetParent(modalPanel.transform, false);
        SetAnchorsRect(skipBtn.GetComponent<RectTransform>(), new Vector2(0.52f, 0.08f), new Vector2(0.95f, 0.42f));
        skipBtn.GetComponent<Image>().color = new Color(0.20f, 0.12f, 0.38f, 1f);
        GameObject skipLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        skipLbl.transform.SetParent(skipBtn.transform, false);
        SetAnchorsRect(skipLbl.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        TextMeshProUGUI skipTMP = skipLbl.GetComponent<TextMeshProUGUI>();
        skipTMP.text = "Skip Anyway";
        skipTMP.fontSize = 22f;
        skipTMP.color = Color.white;
        skipTMP.alignment = TextAlignmentOptions.Center;
        skipTMP.fontStyle = FontStyles.Bold;
        skipBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (_animController != null) _animController.ConfirmSkip();
        });
    }

    // ── BottomPanel ──────────────────────────────────────────────────────
    private void BuildBottomPanel(Transform root)
    {
        _bottomPanel = new GameObject("BottomPanel", typeof(RectTransform), typeof(Image));
        _bottomPanel.transform.SetParent(root, false);
        RectTransform r = _bottomPanel.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0f);
        r.anchorMax = new Vector2(1f, 0.30f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        _bottomPanel.GetComponent<Image>().color = new Color(0.04f, 0.02f, 0.08f, 0.95f);

        // Subtitle
        GameObject subGO = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subGO.transform.SetParent(_bottomPanel.transform, false);
        SetAnchorsRect(subGO.GetComponent<RectTransform>(), new Vector2(0.03f, 0.72f), new Vector2(0.97f, 0.90f));
        TextMeshProUGUI subTMP = subGO.GetComponent<TextMeshProUGUI>();
        subTMP.text = "Drawing individuals from parallel worlds";
        subTMP.fontSize = 18f;
        subTMP.color = ColorTextSecondary;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.fontStyle = FontStyles.Italic;

        // Tabs
        BuildTabs(_bottomPanel.transform);


        // Buttons
        BuildInvokeButtons(_bottomPanel.transform);
    }

    // ── Tabs ──────────────────────────────────────────────────────────────
    private void BuildTabs(Transform parent)
    {
        string[] labels = { "STANDARD", "ADVANCED", "EQUIP" };
        _tabButtons = new Button[3];
        _tabImages  = new Image[3];

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            GameObject go = new GameObject("Tab_" + labels[i], typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            float xMin = 0.03f + i * 0.32f;
            rt.anchorMin = new Vector2(xMin, 0.54f);
            rt.anchorMax = new Vector2(xMin + 0.30f, 0.70f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _tabImages[i]  = go.GetComponent<Image>();
            _tabImages[i].color = i == 0 ? TabActiveColors[i] : TabColors[i];
            _tabButtons[i] = go.GetComponent<Button>();
            _tabButtons[i].onClick.AddListener(() => SwitchTab(idx));

            GameObject lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(go.transform, false);
            SetAnchorsRect(lbl.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            TextMeshProUGUI tmp = lbl.GetComponent<TextMeshProUGUI>();
            tmp.text = labels[i];
            tmp.fontSize = 20f;   // Increased from 14
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    // ── Invoke Buttons ────────────────────────────────────────────────────
    private void BuildInvokeButtons(Transform parent)
    {
        // INVOKE x10 (PRIMARY — larger, gold border, gradient background)
        GameObject tGO = new GameObject("InvokeTen", typeof(RectTransform), typeof(Image), typeof(Button));
        tGO.transform.SetParent(parent, false);
        RectTransform tRT = tGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.50f, 0.06f);
        tRT.anchorMax = new Vector2(0.97f, 0.38f);
        tRT.offsetMin = Vector2.zero;
        tRT.offsetMax = Vector2.zero;
        _invokeTenImg = tGO.GetComponent<Image>();
        _invokeTenImg.color = ColorPrimary;
        _invokeTenBtn = tGO.GetComponent<Button>();

        // Gold border
        GameObject tenBorder = new GameObject("Border", typeof(RectTransform), typeof(Image));
        tenBorder.transform.SetParent(tGO.transform, false);
        SetAnchorsRect(tenBorder.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        tenBorder.GetComponent<RectTransform>().offsetMin = new Vector2(3f, 3f);
        tenBorder.GetComponent<RectTransform>().offsetMax = new Vector2(-3f, -3f);
        _invokeTenBorder = tenBorder.GetComponent<Image>();
        _invokeTenBorder.color = ColorAccentGold;

        GameObject tLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        tLbl.transform.SetParent(tGO.transform, false);
        SetAnchorsRect(tLbl.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        TextMeshProUGUI tTMP = tLbl.GetComponent<TextMeshProUGUI>();
        tTMP.text = "INVOKE x10\n<color=#ffd700><size=22>1,000 Gold</size></color>";
        tTMP.fontSize = 36f;     // Primary button size (design token)
        tTMP.color = Color.white;
        tTMP.fontStyle = FontStyles.Bold;
        tTMP.alignment = TextAlignmentOptions.Center;

        // INVOKE x1 (SECONDARY — smaller, solid)
        GameObject sGO = new GameObject("InvokeSingle", typeof(RectTransform), typeof(Image), typeof(Button));
        sGO.transform.SetParent(parent, false);
        RectTransform sRT = sGO.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0.02f, 0.06f);
        sRT.anchorMax = new Vector2(0.46f, 0.38f);
        sRT.offsetMin = Vector2.zero;
        sRT.offsetMax = Vector2.zero;
        _invokeSingleImg = sGO.GetComponent<Image>();
        _invokeSingleImg.color = new Color(0.19f, 0.11f, 0.57f, 1f); // #311b92
        _invokeSingleBtn = sGO.GetComponent<Button>();

        // Border
        GameObject sBorder = new GameObject("Border", typeof(RectTransform), typeof(Image));
        sBorder.transform.SetParent(sGO.transform, false);
        SetAnchorsRect(sBorder.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        sBorder.GetComponent<RectTransform>().offsetMin = new Vector2(2f, 2f);
        sBorder.GetComponent<RectTransform>().offsetMax = new Vector2(-2f, -2f);
        sBorder.GetComponent<Image>().color = new Color(0.49f, 0.30f, 1f, 1f); // #7c4dff

        GameObject sLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        sLbl.transform.SetParent(sGO.transform, false);
        SetAnchorsRect(sLbl.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        TextMeshProUGUI sTMP = sLbl.GetComponent<TextMeshProUGUI>();
        sTMP.text = "INVOKE x1\n<color=#b39ddb><size=20>200 Gold</size></color>";
        sTMP.fontSize = 28f;     // Secondary button size
        sTMP.color = Color.white;
        sTMP.fontStyle = FontStyles.Bold;
        sTMP.alignment = TextAlignmentOptions.Center;

        _invokeSingleBtn.onClick.AddListener(OnInvokeSingle);
        _invokeTenBtn.onClick.AddListener(OnInvokeTen);
    }

    // ── TopBar ────────────────────────────────────────────────────────────
    private void BuildTopBar(Transform root)
    {
        _topBar = new GameObject("TopBar", typeof(RectTransform));
        _topBar.transform.SetParent(root, false);
        RectTransform r = _topBar.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0.94f);
        r.anchorMax = new Vector2(1f, 1f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        // Back button
        GameObject backGO = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        backGO.transform.SetParent(_topBar.transform, false);
        SetAnchorsRect(backGO.GetComponent<RectTransform>(), new Vector2(0.01f, 0.05f), new Vector2(0.10f, 0.95f));
        backGO.GetComponent<Image>().color = new Color(0.15f, 0.10f, 0.25f, 0.80f);
        _backBtn = backGO.GetComponent<Button>();

        GameObject backLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        backLbl.transform.SetParent(backGO.transform, false);
        SetAnchorsRect(backLbl.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        TextMeshProUGUI bTMP = backLbl.GetComponent<TextMeshProUGUI>();
        bTMP.text = "← BACK";
        bTMP.fontSize = 24f;
        bTMP.color = ColorTextSecondary;
        bTMP.alignment = TextAlignmentOptions.Center;
        bTMP.fontStyle = FontStyles.Bold;

        // Gold display (design token: 24px Bold)
        GameObject goldGO = new GameObject("GoldDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
        goldGO.transform.SetParent(_topBar.transform, false);
        SetAnchorsRect(goldGO.GetComponent<RectTransform>(), new Vector2(0.50f, 0.05f), new Vector2(0.75f, 0.95f));
        _goldDisplay = goldGO.GetComponent<TextMeshProUGUI>();
        _goldDisplay.text = "Gold: 0";
        _goldDisplay.fontSize = 24f;
        _goldDisplay.color = new Color(1f, 0.84f, 0f, 1f); // #FFD700
        _goldDisplay.alignment = TextAlignmentOptions.MidlineRight;
        _goldDisplay.fontStyle = FontStyles.Bold;

        // Gems display (design token: 24px Bold)
        GameObject gemsGO = new GameObject("GemsDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
        gemsGO.transform.SetParent(_topBar.transform, false);
        SetAnchorsRect(gemsGO.GetComponent<RectTransform>(), new Vector2(0.78f, 0.05f), new Vector2(0.98f, 0.95f));
        _gemsDisplay = gemsGO.GetComponent<TextMeshProUGUI>();
        _gemsDisplay.text = "Gems: 0";
        _gemsDisplay.fontSize = 24f;
        _gemsDisplay.color = new Color(0.88f, 0.69f, 1f, 1f); // #E0B0FF
        _gemsDisplay.alignment = TextAlignmentOptions.MidlineRight;
        _gemsDisplay.fontStyle = FontStyles.Bold;

        // Pity
        GameObject pityGO = new GameObject("PityDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
        pityGO.transform.SetParent(_topBar.transform, false);
        SetAnchorsRect(pityGO.GetComponent<RectTransform>(), new Vector2(0.15f, 0.05f), new Vector2(0.45f, 0.95f));
        TextMeshProUGUI pityTMP = pityGO.GetComponent<TextMeshProUGUI>();
        pityTMP.text = "Pity: 0";
        pityTMP.fontSize = 20f;
        pityTMP.color = ColorTextSecondary;
        pityTMP.alignment = TextAlignmentOptions.MidlineLeft;
        pityTMP.fontStyle = FontStyles.Bold;
    }

    // ── Status Text ──────────────────────────────────────────────────────
    private void BuildStatusText(Transform root)
    {
        GameObject go = new GameObject("StatusResult", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(root, false);
        SetAnchorsRect(go.GetComponent<RectTransform>(), new Vector2(0.05f, 0.60f), new Vector2(0.65f, 0.80f));
        _statusResultTMP = go.GetComponent<TextMeshProUGUI>();
        _statusResultTMP.text = "";
        _statusResultTMP.fontSize = 52f;   // Hero name size (design token)
        _statusResultTMP.color = Color.white;
        _statusResultTMP.alignment = TextAlignmentOptions.Left;
        _statusResultTMP.fontStyle = FontStyles.Bold;
        go.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  REMOVE DUPLICATES
    // ═══════════════════════════════════════════════════════════════════════

    private void RemoveGeneratedChildren(Transform root)
    {
        string[] names = new string[]
        {
            "Background", "SummoningCircle", "EffectsLayer", "BottomPanel", "IselPanel", "InfoPanel",
            "TopBar", "DimOverlay", "StatusResult", "MagicCircle", "EnvironmentGlow", "Silhouette",
            "MaterializeImage", "InvokePanel", "BondLabel", "RitualLayer", "CrackOverlay",
            "CharacterSpawnPoint", "TotalCounter", "FiveStarCounter", "StandardPanel", "PremiumPanel",
            "BannerTabs", "CardRevealPanel", "BackBtn", "SkipConfirmationModal"
        };
        List<Transform> remove = new List<Transform>();
        foreach (string n in names)
            foreach (Transform c in root)
                if (c.name == n) remove.Add(c);
        foreach (Transform t in remove)
        {
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  WIRE BUTTONS
    // ═══════════════════════════════════════════════════════════════════════

    private void WireButtons()
    {
        if (_backBtn != null)
            _backBtn.onClick.AddListener(OnBack);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  TAB SWITCHING
    // ═══════════════════════════════════════════════════════════════════════

    private void SwitchTab(int idx)
    {
        _activeTab = idx;
        for (int i = 0; i < _tabImages.Length; i++)
            if (_tabImages[i] != null)
                _tabImages[i].color = i == idx ? TabActiveColors[i] : TabColors[i];

        // Update button cost texts
        if (_invokeTenBtn != null)
        {
            var btnTMP = _invokeTenBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTMP != null)
            {
                string costText = idx switch { 0 => "1,000 Gold", 1 => "100 Gems", _ => "—" };
                btnTMP.text = $"INVOKE x10\n<color=#ffd700><size=22>{costText}</size></color>";
            }
        }
        if (_invokeSingleBtn != null)
        {
            var btnTMP = _invokeSingleBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTMP != null)
            {
                string costText = idx switch { 0 => "200 Gold", 1 => "20 Gems", _ => "—" };
                btnTMP.text = $"INVOKE x1\n<color=#b39ddb><size=20>{costText}</size></color>";
            }
        }

        Color a1 = idx switch
        {
            0 => new Color(0.20f, 0.12f, 0.38f, 1f),
            1 => new Color(0.12f, 0.10f, 0.28f, 1f),
            _ => new Color(0.08f, 0.16f, 0.10f, 1f)
        };
        Color a2 = idx switch
        {
            0 => ColorPrimary,
            1 => new Color(0.18f, 0.14f, 0.38f, 1f),
            _ => new Color(0.10f, 0.22f, 0.14f, 1f)
        };
        if (_invokeSingleImg != null) _invokeSingleImg.color = a1;
        if (_invokeTenImg    != null) _invokeTenImg.color    = a2;

        ShowIselDialogue(TabDialogue[idx]);
        RefreshUI();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  BUTTON HANDLERS
    // ═══════════════════════════════════════════════════════════════════════

    private void OnInvokeSingle()
    {
        if (_isAnimating) return;
        if (_activeTab == 2) { ShowIselDialogue("Equipment: pending."); return; }
        HeroInstance hero = _activeTab == 0 ? _gacha.SummonStandard() : _gacha.SummonPremium();
        if (hero == null) { ShowAffordError(_activeTab == 0 ? "Gold insufficient." : "Gems insufficient."); return; }
        _sessionTotalSummons++;
        if (hero.CurrentStarRank >= 5) _sessionFiveStars++;
        StartCoroutine(InvokeSummonSingle(hero));
    }

    private void OnInvokeTen()
    {
        if (_isAnimating) return;
        if (_activeTab == 2) { ShowIselDialogue("Equipment: pending."); return; }
        List<HeroInstance> heroes = _activeTab == 0 ? _gacha.SummonStandardTen() : _gacha.SummonPremiumTen();
        if (heroes == null) { ShowAffordError(_activeTab == 0 ? "Gold insufficient." : "Gems insufficient."); return; }
        _sessionTotalSummons += heroes.Count;
        _sessionFiveStars    += heroes.Count(h => h.CurrentStarRank >= 5);
        StartCoroutine(InvokeSummonTen(heroes));
    }

    private void OnBack()
    {
        if (_isAnimating) return;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  INVOKE FLOW
    // ═══════════════════════════════════════════════════════════════════════

    private IEnumerator InvokeSummonSingle(HeroInstance hero)
    {
        _isAnimating = true;
        _doubleTapSkip = false;
        _speedMultiplier = 1f;
        SetButtonsInteractable(false);
        HideStatusUI();

        if (_animController != null)
            _animController.StartSummonAnimation(hero.CurrentStarRank);

        yield return StartCoroutine(PhasePrep(hero.CurrentStarRank));
        ShowIselDialogue(IselSummonStart);
        yield return StartCoroutine(PhaseActivateCircle(hero.CurrentStarRank));
        yield return StartCoroutine(PhaseDistortion(hero.CurrentStarRank));
        yield return StartCoroutine(PhaseTensionPause(hero.CurrentStarRank));
        yield return StartCoroutine(PhaseMaterialization(hero));
        yield return StartCoroutine(PhaseCardReveal(hero));
        yield return StartCoroutine(IselReaction(hero.CurrentStarRank));
        yield return StartCoroutine(PhaseComplete());

        if (_animController != null) _animController.EndAnimation();
        RefreshUI();
        _isAnimating = false;
        SetButtonsInteractable(true);
    }

    private IEnumerator InvokeSummonTen(List<HeroInstance> heroes)
    {
        _isAnimating = true;
        _doubleTapSkip = false;
        _speedMultiplier = 1f;
        SetButtonsInteractable(false);
        HideStatusUI();

        if (_animController != null)
            _animController.StartSummonAnimation(heroes[0].CurrentStarRank);

        yield return StartCoroutine(PhasePrep(heroes[0].CurrentStarRank));
        ShowIselDialogue(IselSummonStart);

        for (int i = 0; i < heroes.Count; i++)
        {
            HeroInstance h = heroes[i];
            HideStatusUI();
            yield return StartCoroutine(PhaseActivateCircle(h.CurrentStarRank));
            yield return StartCoroutine(PhaseDistortion(h.CurrentStarRank));
            yield return StartCoroutine(PhaseTensionPause(h.CurrentStarRank));
            yield return StartCoroutine(PhaseMaterialization(h));
            if (i == heroes.Count - 1)
            {
                yield return StartCoroutine(PhaseCardReveal(h));
                yield return StartCoroutine(IselReaction(h.CurrentStarRank));
            }
            else
            {
                ShowIselDialogue(h.CurrentStarRank <= 2 ? "Unremarkable." : h.CurrentStarRank == 3 ? "Capable." : "This one is different.");
                yield return new WaitForSeconds(0.35f);
            }
            yield return StartCoroutine(PhaseQuickClose());
        }

        yield return StartCoroutine(PhaseComplete());
        if (_animController != null) _animController.EndAnimation();
        RefreshUI();
        _isAnimating = false;
        SetButtonsInteractable(true);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PHASES
    // ═══════════════════════════════════════════════════════════════════════

    private IEnumerator PhasePrep(int starRank)
    {
        float d = PrepDuration / _speedMultiplier;
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = false;

        float elapsed = 0f;
        while (elapsed < d)
        {
            if (CheckSkip()) yield break;
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0.30f, elapsed / d);
            yield return null;
        }
        group.alpha = 0.30f;
        _ritualCircleImage.gameObject.SetActive(true);
    }

    private IEnumerator PhaseActivateCircle(int starRank)
    {
        Color rc = GetRarityColor(starRank);
        bool high = starRank >= 4;
        float d = (high ? ActivateDuration * 1.3f : ActivateDuration) / _speedMultiplier;

        _summoningCircle.transform.localScale = Vector3.one * 1.5f;

        float elapsed = 0f;
        while (elapsed < d)
        {
            if (CheckSkip()) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / d;

            float ca = Mathf.SmoothStep(0f, GetCircleGlowIntensity(starRank), t);
            _ritualCircleImage.color = new Color(rc.r, rc.g, rc.b, ca);

            float ga = high ? Mathf.SmoothStep(0f, GetEnvGlowIntensity(starRank), t) : Mathf.SmoothStep(0f, GetEnvGlowIntensity(starRank), t);
            _environmentGlow.color = new Color(rc.r * 0.5f, rc.g * 0.5f, rc.b * 0.5f, ga);

            float s = Mathf.SmoothStep(1.5f, CircleScaleDominant, t);
            _summoningCircle.transform.localScale = new Vector3(s, s, 1f);

            SetImageAlpha(_dimOverlay, Mathf.SmoothStep(0f, 0.15f, t));
            yield return null;
        }
        _summoningCircle.transform.localScale = Vector3.one * CircleScaleDominant;
    }

    private IEnumerator PhaseDistortion(int starRank)
    {
        Color rc = GetRarityColor(starRank);
        bool low  = starRank <= 2;
        bool mid  = starRank == 3;
        bool high = starRank >= 4;
        float d = (high ? DistortionDuration * 1.4f : DistortionDuration) / _speedMultiplier;

        float elapsed = 0f;
        while (elapsed < d)
        {
            if (CheckSkip()) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / d;

            float silA;
            if (low)
                silA = Mathf.SmoothStep(0f, 0.65f, t) * (0.7f + 0.3f * Mathf.Sin(elapsed * 24f));
            else if (mid)
                silA = Mathf.SmoothStep(0f, 0.70f, t);
            else
                silA = Mathf.SmoothStep(0f, 0.85f, t);

            _silhouetteImage.color = new Color(rc.r * 0.20f, rc.g * 0.15f, rc.b * 0.30f, Mathf.Clamp01(silA));

            float ck = low ? Mathf.SmoothStep(0f, 0.30f, t) * (0.5f + 0.5f * Mathf.Sin(elapsed * 30f)) : Mathf.SmoothStep(0f, 0.15f, t);
            SetImageAlpha(_crackOverlay, ck);

            SetImageAlpha(_dimOverlay, 0.15f + (0.10f * Mathf.Sin(elapsed * 2f)));
            yield return null;
        }
    }

    private IEnumerator PhaseTensionPause(int starRank)
    {
        bool high = starRank >= 4;
        float tension = high ? TensionDelay * 1.3f : TensionDelay;
        tension /= _speedMultiplier;

        float elapsed = 0f;
        while (elapsed < tension)
        {
            if (CheckSkip() && !high) yield break;
            if (CheckSkip() && high)
                elapsed += Time.deltaTime * 0.5f;
            elapsed += Time.deltaTime;

            float pulse = Mathf.Sin(elapsed * 3f) * 0.5f + 0.5f;
            SetImageAlpha(_dimOverlay, 0.15f + pulse * 0.10f);

            Color c = _ritualCircleImage.color;
            c.a = 0.55f + pulse * 0.15f;
            _ritualCircleImage.color = c;
            yield return null;
        }
    }

    private IEnumerator PhaseMaterialization(HeroInstance hero)
    {
        HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(hero.HeroDefId);
        if (def != null && !string.IsNullOrEmpty(def.PortraitSpritePath))
        {
            Sprite sp = Resources.Load<Sprite>(def.PortraitSpritePath);
            if (sp != null) _materializeImage.sprite = sp;
        }

        Color rc = GetRarityColor(hero.CurrentStarRank);
        bool high = hero.CurrentStarRank >= 4;
        float d = (high ? MaterializeDuration * 1.3f : MaterializeDuration) / _speedMultiplier;

        float elapsed = 0f;
        while (elapsed < d)
        {
            if (CheckSkip() && !high) yield break;
            if (CheckSkip() && high) { elapsed += Time.deltaTime * 2f; }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / d);

            SetImageAlpha(_silhouetteImage, Mathf.SmoothStep(0.70f, 0f, t));

            float ma = Mathf.SmoothStep(0f, 1f, t);
            _materializeImage.color = new Color(1f, 1f, 1f, ma);

            float sway = Mathf.Sin(elapsed * 1.5f) * 3f;
            _materializeImage.transform.localRotation = Quaternion.Euler(0f, 0f, sway);

            float ga = high ? Mathf.SmoothStep(0.3f, GetEnvGlowIntensity(hero.CurrentStarRank), t) : Mathf.SmoothStep(0.2f, GetEnvGlowIntensity(hero.CurrentStarRank), t);
            _environmentGlow.color = new Color(rc.r, rc.g, rc.b, ga);

            if (high && t > 0.4f)
            {
                float si = Mathf.Lerp(12f, 0f, (t - 0.4f) / 0.6f);
                _effectsLayer.transform.localPosition = new Vector3(Random.Range(-si, si), Random.Range(-si, si), 0f);
            }
            yield return null;
        }
        _effectsLayer.transform.localPosition = Vector3.zero;
        _materializeImage.transform.localRotation = Quaternion.identity;
    }

    private IEnumerator PhaseCardReveal(HeroInstance hero)
    {
        HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(hero.HeroDefId);
        string name = def != null ? HeroPresentationUtility.GetDisplayName(def, hero.HeroDefId) : hero.HeroDefId;
        string role = def != null ? HeroPresentationUtility.GetRoleLabel(def) : "UNKNOWN";

        if (_statusResultTMP != null)
        {
            _statusResultTMP.gameObject.SetActive(true);
            _statusResultTMP.text = "";
            _statusResultTMP.color = GetRarityColor(hero.CurrentStarRank);
            _statusResultTMP.fontSize = 52f; // Hero name — design token
        }

        float d = CardRevealDuration / _speedMultiplier;
        // MODIFIED: Replaced \u2605 with [N] format to fix font missing glyph warning
        string full = $"{name.ToUpper()}  |  {role}\n[{hero.CurrentStarRank}] {hero.CurrentStarRank}-star resonance.";
        float elapsed = 0f;
        while (elapsed < d)
        {
            if (CheckSkip()) { _statusResultTMP.text = full; yield break; }
            elapsed += Time.deltaTime;
            int cc = Mathf.RoundToInt(Mathf.Lerp(0, full.Length, elapsed / d));
            if (_statusResultTMP != null)
                _statusResultTMP.text = full.Substring(0, Mathf.Clamp(cc, 0, full.Length));
            yield return null;
        }
        if (_statusResultTMP != null) _statusResultTMP.text = full;
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator IselReaction(int starRank)
    {
        if (starRank >= 4)
        {
            yield return new WaitForSeconds(0.5f / _speedMultiplier);
            ShowIselDialogue(IselLines4StarPlus[Random.Range(0, IselLines4StarPlus.Length)]);
        }
        else if (starRank == 3)
            ShowIselDialogue(IselLines3Star[Random.Range(0, IselLines3Star.Length)]);
        else
            ShowIselDialogue(IselLines1Star[Random.Range(0, IselLines1Star.Length)]);
        yield return new WaitForSeconds(0.3f / _speedMultiplier);
    }

    private IEnumerator PhaseComplete()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            float d = CompleteFadeDuration / _speedMultiplier;
            float elapsed = 0f;
            while (elapsed < d)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(0.30f, 1f, elapsed / d);
                yield return null;
            }
            group.alpha = 1f;
            group.interactable = true;
        }
        yield return StartCoroutine(CloseRitual());
    }

    private IEnumerator PhaseQuickClose()
    {
        float d = 0.3f / _speedMultiplier;
        float elapsed = 0f;
        while (elapsed < d)
        {
            if (CheckSkip()) break;
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / d);
            SetImageAlpha(_ritualCircleImage,   Mathf.SmoothStep(0f, 0.65f, t));
            SetImageAlpha(_materializeImage,    Mathf.SmoothStep(0f, 1f,    t));
            SetImageAlpha(_environmentGlow,     Mathf.SmoothStep(0f, 0.6f,  t));
            SetImageAlpha(_silhouetteImage,      0f);
            yield return null;
        }
        ResetImages();
    }

    private IEnumerator CloseRitual()
    {
        float d = 0.4f / _speedMultiplier;
        float elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / d);
            SetImageAlpha(_dimOverlay,           Mathf.SmoothStep(0f, 0.25f, t));
            SetImageAlpha(_ritualCircleImage,    Mathf.SmoothStep(0f, 0.65f, t));
            SetImageAlpha(_materializeImage,     Mathf.SmoothStep(0f, 1f,    t));
            SetImageAlpha(_environmentGlow,      Mathf.SmoothStep(0f, 0.6f,  t));
            SetImageAlpha(_silhouetteImage,       0f);
            SetImageAlpha(_crackOverlay,          0f);
            yield return null;
        }
        ResetImages();
        SetImageAlpha(_dimOverlay, 0f);
    }

    private void ResetImages()
    {
        _ritualCircleImage.color    = new Color(_ritualCircleImage.color.r, _ritualCircleImage.color.g, _ritualCircleImage.color.b, 0f);
        _materializeImage.color     = new Color(1f, 1f, 1f, 0f);
        _environmentGlow.color      = new Color(_environmentGlow.color.r, _environmentGlow.color.g, _environmentGlow.color.b, 0f);
        SetImageAlpha(_silhouetteImage, 0f);
        SetImageAlpha(_crackOverlay, 0f);
        _effectsLayer.transform.localPosition = Vector3.zero;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SKIP SYSTEM (Double-tap → skip; 4★+ → confirmation)
    // ═══════════════════════════════════════════════════════════════════════

    private void Update()
    {
        if (!_isAnimating) return;

        bool tapped = Input.GetMouseButtonDown(0);

        // Also support touch on mobile
        if (!tapped && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            tapped = true;

        if (tapped)
        {
            float now = Time.time;
            if (now - _lastTapTime < TapSkipWindow)
            {
                // Double tap — skip immediately
                _doubleTapSkip = true;
                _speedMultiplier = 99f; // fast-forward everything
            }
            else
            {
                // Single tap — speed up only
                if (_speedMultiplier < 2f)
                    _speedMultiplier = 2f;
            }
            _lastTapTime = now;
        }
    }

    private bool CheckSkip()
    {
        if (_doubleTapSkip)
        {
            _doubleTapSkip = false;
            return true;
        }
        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  IDLE PULSE — circle breathes
    // ═══════════════════════════════════════════════════════════════════════

    private IEnumerator CircleIdlePulse()
    {
        while (true)
        {
            if (!_isAnimating)
            {
                float pulse = Mathf.Sin(Time.time * 1.2f) * 0.03f + 0.05f;
                Color c = _ritualCircleImage.color;
                float target = Mathf.Lerp(0.03f, 0.08f, pulse);
                c.a = Mathf.Lerp(c.a, target, Time.deltaTime * 2f);
                _ritualCircleImage.color = c;
            }
            yield return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ISEL DIALOGUE
    // ═══════════════════════════════════════════════════════════════════════

    private void ShowIselDialogue(string text)
    {
        if (_iselDialogueTMP != null)
            _iselDialogueTMP.text = text;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  RARITY HELPERS (Config-backed)
    // ═══════════════════════════════════════════════════════════════════════

    private Color GetRarityColor(int starRank)
    {
        if (_rarityConfig != null)
        {
            var data = _rarityConfig.GetRarityDataClamped(starRank);
            return data.circleGlowColor;
        }
        int idx = Mathf.Clamp(starRank - 1, 0, RarityColors.Length - 1);
        return RarityColors[idx];
    }

    private float GetCircleGlowIntensity(int starRank)
    {
        if (_rarityConfig != null)
            return _rarityConfig.GetRarityDataClamped(starRank).circleGlowIntensity;
        return starRank >= 4 ? 0.75f : 0.45f;
    }

    private float GetEnvGlowIntensity(int starRank)
    {
        if (_rarityConfig != null)
            return _rarityConfig.GetRarityDataClamped(starRank).environmentGlowIntensity;
        return starRank >= 4 ? 0.50f : 0.25f;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UI HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    public void RefreshUI()
    {
        if (_gacha == null || _currency == null) return;
        PityData pity = _gacha.GetPity();
        if (_goldDisplay != null) _goldDisplay.text = $"Gold: {_currency.GetGold():N0}";
        if (_gemsDisplay != null) _gemsDisplay.text = $"Gems: {_currency.GetGems():N0}";
        UpdatePityDisplay(pity);
    }

    private void UpdatePityDisplay(PityData pity)
    {
        // Determine current pity and max threshold based on active tab
        int currentCount;
        int maxCount;
        if (_activeTab == 0) // Standard
        {
            currentCount = pity.StandardSummonCount;
            maxCount = 300;
        }
        else if (_activeTab == 1) // Premium
        {
            currentCount = pity.PremiumSummonCount;
            maxCount = 200;
        }
        else // Equip (not implemented, show 0)
        {
            currentCount = 0;
            maxCount = 1;
        }
        if (pityFillImage != null)
            pityFillImage.fillAmount = currentCount / (float)Mathf.Max(maxCount, 1);
        if (pityText != null)
            pityText.text = $"PITY {currentCount}/{maxCount}";
        // Also update the top-bar pity text if it exists
        Transform topBarTransform = _topBar?.transform;
        if (topBarTransform != null)
        {
            foreach (Transform child in topBarTransform)
            {
                TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null && tmp.text.StartsWith("Pity:"))
                {
                    tmp.text = $"Pity: {currentCount}/{maxCount}";
                    break;
                }
            }
        }
    }

    private void HideStatusUI()
    {
        if (_statusResultTMP != null) { _statusResultTMP.gameObject.SetActive(false); _statusResultTMP.text = ""; }
    }

    private void SetButtonsInteractable(bool val)
    {
        if (_invokeSingleBtn != null) _invokeSingleBtn.interactable = val;
        if (_invokeTenBtn    != null) _invokeTenBtn.interactable    = val;
        if (_backBtn         != null) _backBtn.interactable         = val;
        if (_tabButtons != null)
            foreach (Button b in _tabButtons)
                if (b != null) b.interactable = val;
    }

    // ── Toast ─────────────────────────────────────────────────────────────

    private GameObject _toastGo;
    private Coroutine  _toastCoroutine;

    private void ShowAffordError(string msg)
    {
        if (_toastGo == null) CreateToast();
        _toastGo.SetActive(true);
        TextMeshProUGUI tmp = _toastGo.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = msg;
        if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
        _toastCoroutine = StartCoroutine(FadeToast());
    }

    private void CreateToast()
    {
        _toastGo = new GameObject("AffordToast");
        _toastGo.transform.SetParent(transform, false);
        RectTransform rt = _toastGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject bg = new GameObject("ToastBg");
        bg.transform.SetParent(_toastGo.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.1f, 0.7f);
        bgRT.anchorMax = new Vector2(0.9f, 0.8f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.04f, 0.14f, 0.96f);

        GameObject textGo = new GameObject("ToastText");
        textGo.transform.SetParent(bg.transform, false);
        RectTransform textRT = textGo.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 40f;
        tmp.color = new Color(0.95f, 0.55f, 0.55f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        _toastGo.SetActive(false);
    }

    private IEnumerator FadeToast()
    {
        yield return new WaitForSeconds(2.5f);
        float fd = 0.4f;
        float el = 0f;
        CanvasGroup cg = _toastGo.GetComponent<CanvasGroup>();
        if (cg == null) cg = _toastGo.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        while (el < fd) { el += Time.deltaTime; cg.alpha = 1f - (el / fd); yield return null; }
        cg.alpha = 0f; _toastGo.SetActive(false); cg.alpha = 1f;
    }

    // ── Utilities ─────────────────────────────────────────────────────────

    private void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }

    private Image CreateFullRectImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go.GetComponent<Image>();
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 min, Vector2 max)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    private void SetAnchorsRect(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
