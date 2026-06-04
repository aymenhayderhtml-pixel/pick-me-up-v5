using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
//  FacilityView  –  dark military sci-fi HQ  |  Landscape 2340 × 1080
// ─────────────────────────────────────────────────────────────────────────────
public class FacilityView : MonoBehaviour
{
    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color C_BgDeep      = Hex("#050a08");
    static readonly Color C_Card        = Hex("#0D1120");
    static readonly Color C_CardBdr     = Hex("#1B223D");
    static readonly Color C_Gold        = Hex("#C8A84B");
    static readonly Color C_GoldDim     = Hex("#5A4A25");
    static readonly Color C_Gem         = Hex("#4A8ECF");
    static readonly Color C_Green       = Hex("#2EA869");
    static readonly Color C_GreenDk     = Hex("#13452B");
    static readonly Color C_Red         = Hex("#CF3E3E");
    static readonly Color C_Text        = Hex("#E2E6F5");
    static readonly Color C_Sub         = Hex("#7882A4");
    static readonly Color C_Locked      = Hex("#0A0C16");
    static readonly Color C_LckTxt      = Hex("#2C3044");
    static readonly Color C_HdrBg       = Hex("#06080E");
    static readonly Color C_MorBg       = Hex("#080A12");
    static readonly Color C_DetBgTop    = Hex("#0d1a12"); 
    static readonly Color C_DetBgBot    = Hex("#080f0a"); 
    static readonly Color C_UpgBtn      = Hex("#c9a227");
    static readonly Color C_InfoBtn     = Hex("#0E1428");
    static readonly Color C_Divider     = Hex("#141A30");
    static readonly Color C_TealActive  = Hex("#2a8a8a"); // Muted teal
    static readonly Color C_GrayDk      = Hex("#1a1a1a"); // Dim inactive paths
    
    // Strict Banner Color System
    static readonly Color B_Combat      = Hex("#1a4d2a");
    static readonly Color B_Support     = Hex("#1a3a4d");
    static readonly Color B_Special     = Hex("#4d1a1a");
    static readonly Color B_Locked      = Hex("#2a2a2a");

    // ── Layout Constants ──────────────────────────────────────────────────────
    const float HdrH    = 92f;     
    const float MorBarH = 48f;     
    const float DetFrac = 0.42f;   // Increased right detail panel width slightly for readability (42% width)
    const float DP      = 24f;     

    // ── Services ──────────────────────────────────────────────────────────────
    IFacilityService _fac;
    ICurrencyService _cur;
    IRosterService   _ros;

    // ── UI Roots ──────────────────────────────────────────────────────────────
    RectTransform _mapRoot;
    RectTransform _detRoot;

    // Detail-panel widgets
    TextMeshProUGUI _dName, _dLevel, _dDesc;
    TextMeshProUGUI _dCurBonusLabel, _dCurBonusValue;
    TextMeshProUGUI _dNxtBonusLabel, _dNxtBonusValue;
    TextMeshProUGUI _dCostTxt;
    Button          _btnUpg;
    TextMeshProUGUI _btnUpgTxt;
    TextMeshProUGUI _btnUpgErr; 
    Image           _detArtBg;

    // Morale bar
    Image           _morFill;
    TextMeshProUGUI _morTxt;

    // Header currency labels
    TextMeshProUGUI _lblGold, _lblGems, _lblMor;

    // Additional Detail Widgets
    TextMeshProUGUI _dPower, _dUnlocks, _dLore;

    // ── State ─────────────────────────────────────────────────────────────────
    string _selId;
    readonly List<FacilityBuildingCard>    _bldCards = new List<FacilityBuildingCard>();
    readonly Dictionary<string, Coroutine> _pulses   = new Dictionary<string, Coroutine>();
    readonly List<PathSegmentWidget>       _paths     = new List<PathSegmentWidget>();
    Coroutine _upgBtnPulse;

    // ─────────────────────────────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        _fac = ServiceRegistry.Instance.Resolve<IFacilityService>();
        _cur = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _ros = ServiceRegistry.Instance.Resolve<IRosterService>();

        BuildUI();
        PopulateCitadelMap();
        RefreshHeader();
        RefreshMoraleBar();

        // Default selection: Workshop / Command Center
        if (_fac?.GetFacility("workshop") != null)
            SelectBuilding("workshop");
    }

    void OnEnable()
    {
        if (_cur != null) { RefreshHeader(); RefreshMoraleBar(); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BUILD UI
    // ─────────────────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2340, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

        var root = GetOrAdd<RectTransform>(gameObject);
        Stretch(root);
        AddImg(root, C_BgDeep);

        BuildAtmosphere(root);
        BuildHeader(root);
        BuildMoraleBar(root);
        BuildColumns(root);
    }

    void BuildAtmosphere(RectTransform root)
    {
        // Background texture layer: radial gradient + faint scanline pattern (3% opacity)
        var bgGo = NewGO("CitadelBackground", root);
        var bgRt = bgGo.GetComponent<RectTransform>();
        Stretch(bgRt);
        var bgImg = bgGo.AddComponent<Image>();
        
        Texture2D tex = new Texture2D(64, 64);
        Color centerC = Hex("#0d1a12");
        Color edgeC   = Hex("#050a08");
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = (x - 32f) / 32f;
                float dy = (y - 32f) / 32f;
                float dist = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                Color c = Color.Lerp(centerC, edgeC, dist);
                
                // Add a very faint scanline pattern (horizontal grid lines every 4 pixels at 3% opacity overlay)
                if (y % 4 == 0)
                {
                    c = Color.Lerp(c, Color.white, 0.03f);
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        bgImg.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        bgImg.raycastTarget = false;

        // Ground Fog
        AddImg(MkRect("Atm_Fog", root, 0f, 0f, 0.62f, 0.40f), new Color(0.03f, 0.04f, 0.08f, 0.40f)).raycastTarget = false;
    }

    void BuildHeader(RectTransform root)
    {
        var hdr = MkRect("Header", root, 0, 1, 1, 1);
        hdr.pivot = new Vector2(0.5f, 1);
        hdr.sizeDelta = new Vector2(0, HdrH);
        hdr.anchoredPosition = Vector2.zero;
        AddImg(hdr, C_HdrBg);

        var sep = MkRect("HdrSep", hdr, 0, 0, 1, 0);
        sep.pivot = new Vector2(0.5f, 0); sep.sizeDelta = new Vector2(0, 2);
        AddImg(sep, C_Gold);

        // BACK button with Arrow icon + Text (<- BACK)
        var backGo = NewGO("Back", hdr);
        var backRt = backGo.GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0, 0.5f); backRt.anchorMax = new Vector2(0, 0.5f);
        backRt.pivot = new Vector2(0, 0.5f);
        backRt.sizeDelta = new Vector2(140, 50);
        backRt.anchoredPosition = new Vector2(16, 0);
        AddImg(backRt, Hex("#121422"));
        var bkBdr = MkRect("Bdr", backRt, 0, 0, 1, 0);
        bkBdr.pivot = new Vector2(0.5f, 0); bkBdr.sizeDelta = new Vector2(0, 2);
        AddImg(bkBdr, C_GoldDim);
        var backBtn = backGo.AddComponent<Button>();
        var backTxt = MkLbl("T", backRt, 15, FontStyles.Bold, C_Gold, TextAlignmentOptions.Center);
        Stretch(backTxt.rectTransform); backTxt.text = "<- BACK";
        backBtn.onClick.AddListener(() => SceneManager.LoadScene("Hub"));

        // Title
        var title = MkLbl("Title", hdr, 26, FontStyles.Bold, C_Gold, TextAlignmentOptions.Center);
        var trt = title.rectTransform;
        trt.anchorMin = new Vector2(0.12f, 0.08f);
        trt.anchorMax = new Vector2(0.48f, 0.92f);
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        title.text = "CITADEL HEADQUARTERS";

        // Resource panels: separate units with 4px gaps
        _lblGold = MakeSeparatedCurrencyChip(hdr, "• GOLD", C_Gold,  0.49f, 0.62f);
        _lblGems = MakeSeparatedCurrencyChip(hdr, "• GEMS", C_Gem,   0.622f, 0.75f);
        _lblMor  = MakeSeparatedCurrencyChip(hdr, "• MORALE", C_Green, 0.752f, 0.88f);
    }

    TextMeshProUGUI MakeSeparatedCurrencyChip(RectTransform parent, string title, Color col, float x0, float x1)
    {
        var chip = MkRect("Chip_" + title, parent, x0, 0.15f, x1, 0.85f);
        AddImg(chip, new Color(col.r * 0.08f, col.g * 0.08f, col.b * 0.08f, 0.90f));

        var iconBdr = MkRect("Bdr", chip, 0, 0, 1, 1);
        iconBdr.offsetMin = new Vector2(-1, -1); iconBdr.offsetMax = new Vector2(1, 1);
        AddImg(iconBdr, new Color(col.r * 0.4f, col.g * 0.4f, col.b * 0.4f, 0.5f)).raycastTarget = false;

        var ic = MkLbl("Lbl", chip, 11, FontStyles.Bold, col, TextAlignmentOptions.Center);
        ic.rectTransform.anchorMin = new Vector2(0, 0);
        ic.rectTransform.anchorMax = new Vector2(0.38f, 1);
        ic.rectTransform.offsetMin = ic.rectTransform.offsetMax = Vector2.zero;
        ic.text = title;

        var val = MkLbl("Val", chip, 15, FontStyles.Bold, C_Text, TextAlignmentOptions.Left);
        val.rectTransform.anchorMin = new Vector2(0.38f, 0);
        val.rectTransform.anchorMax = new Vector2(1, 1);
        val.rectTransform.offsetMin = new Vector2(6, 0);
        val.rectTransform.offsetMax = Vector2.zero;
        val.text = "0";
        return val;
    }

    void BuildMoraleBar(RectTransform root)
    {
        var bar = MkRect("MoraleBar", root, 0, 1, 1, 1);
        bar.pivot = new Vector2(0.5f, 1);
        bar.sizeDelta = new Vector2(0, MorBarH);
        bar.anchoredPosition = new Vector2(0, -HdrH);
        AddImg(bar, C_MorBg);

        // Increased left padding on "FORTRESS MORALE" label
        var lbl = MkLbl("Lbl", bar, 12, FontStyles.Bold, C_Sub, TextAlignmentOptions.Left);
        lbl.rectTransform.anchorMin = new Vector2(0, 0);
        lbl.rectTransform.anchorMax = new Vector2(0.15f, 1);
        lbl.rectTransform.offsetMin = new Vector2(24, 0); // 24px left padding
        lbl.rectTransform.offsetMax = Vector2.zero;
        lbl.text = "FORTRESS MORALE";

        // Bar start offset increased to accommodate label padding
        var segArea = MkRect("Segs", bar, 0.17f, 0.35f, 0.72f, 0.65f);
        AddImg(segArea, C_LckTxt); 

        var fillRt = MkRect("Fill", segArea, 0f, 0f, 0f, 1f); 
        _morFill = AddImg(fillRt, C_Green);

        _morTxt = MkLbl("Val", bar, 13, FontStyles.Bold, C_Text, TextAlignmentOptions.Right);
        _morTxt.rectTransform.anchorMin = new Vector2(0.72f, 0);
        _morTxt.rectTransform.anchorMax = new Vector2(1, 1);
        _morTxt.rectTransform.offsetMin = Vector2.zero;
        _morTxt.rectTransform.offsetMax = new Vector2(-24, 0); // increased right padding
        _morTxt.text = "0 / 100";
    }

    void BuildColumns(RectTransform root)
    {
        float yOff = -(HdrH + MorBarH);

        // Map area (left 62 %)
        var mapGo = NewGO("MapArea", root);
        _mapRoot = mapGo.GetComponent<RectTransform>();
        _mapRoot.anchorMin = new Vector2(0, 0);
        _mapRoot.anchorMax = new Vector2(1f - DetFrac, 1);
        _mapRoot.offsetMin = new Vector2(0, 0);
        _mapRoot.offsetMax = new Vector2(0, yOff);

        // Detail panel (right 38 %) - Gradient background setup
        var detGo = NewGO("Detail", root);
        _detRoot = detGo.GetComponent<RectTransform>();
        _detRoot.anchorMin = new Vector2(1f - DetFrac, 0);
        _detRoot.anchorMax = new Vector2(1, 1);
        _detRoot.offsetMin = Vector2.zero;
        _detRoot.offsetMax = new Vector2(0, yOff);
        
        var detImg = _detRoot.gameObject.AddComponent<Image>();
        Texture2D detGrad = new Texture2D(1, 16);
        for (int i = 0; i < 16; i++)
        {
            detGrad.SetPixel(0, i, Color.Lerp(C_DetBgBot, C_DetBgTop, (float)i / 15f));
        }
        detGrad.Apply();
        detImg.sprite = Sprite.Create(detGrad, new Rect(0, 0, 1, 16), new Vector2(0.5f, 0.5f));

        // Subtle left edge glow: 1px vertical line in #1a3a2a
        var leftGlow = MkRect("LeftGlow", _detRoot, 0f, 0f, 0f, 1f);
        leftGlow.pivot = new Vector2(0f, 0.5f);
        leftGlow.sizeDelta = new Vector2(1f, 0f);
        AddImg(leftGlow, Hex("#1a3a2a"));

        BuildDetailPanel(_detRoot);
    }

    void BuildDetailPanel(RectTransform panel)
    {
        // Art zone
        var artGo = NewGO("ArtArea", panel);
        _detArtBg = artGo.AddComponent<Image>();
        _detArtBg.color = C_Card;
        var artRt = artGo.GetComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0, 0.65f);
        artRt.anchorMax = new Vector2(1, 1);
        artRt.offsetMin = artRt.offsetMax = Vector2.zero;
        BuildArtSilhouette(artRt, C_Gold);
        
        var fade = MkRect("ArtFade", artRt, 0, 0, 1, 0.35f);
        AddImg(fade, new Color(C_DetBgBot.r, C_DetBgBot.g, C_DetBgBot.b, 0.95f));

        // Content (adjusted with 24px padding inside)
        var cGo = NewGO("DetailContent", panel);
        var cRt = cGo.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 0);
        cRt.anchorMax = new Vector2(1, 0.65f);
        cRt.offsetMin = cRt.offsetMax = Vector2.zero;

        float y = -DP;
        // Bolder / larger command title
        _dName  = PosLbl("Name",  cRt, 31, FontStyles.Bold,   C_Gold, TextAlignmentOptions.Left, DP, y, -DP, 38); y -= 42;
        // Muted gray level on own line (enlarged)
        _dLevel = PosLbl("Level", cRt, 16, FontStyles.Normal,  C_Sub,  TextAlignmentOptions.Left, DP, y, -DP, 22); y -= 26;
        _dDesc  = PosLbl("Desc",  cRt, 14, FontStyles.Italic,  C_Sub,  TextAlignmentOptions.Left, DP, y, -DP, 44); y -= 50;
        _dDesc.textWrappingMode = TextWrappingModes.Normal;
        _dDesc.lineSpacing = 20f; // Add spacing on multi-line text

        MkDivider(cRt, y); y -= 12;

        // Side-by-side columns
        var colContainer = MkRect("BonusCols", cRt, 0f, 1f, 1f, 1f);
        colContainer.pivot = new Vector2(0.5f, 1f);
        colContainer.sizeDelta = new Vector2(0f, 70f); // Sized up container slightly
        colContainer.anchoredPosition = new Vector2(0f, y);
        y -= 80;

        var col1 = MkRect("Col1", colContainer, 0.05f, 0f, 0.48f, 1f);
        _dCurBonusLabel = MkLbl("CurH", col1, 12, FontStyles.Bold, C_Sub, TextAlignmentOptions.Left); // Sized up 20%
        _dCurBonusLabel.rectTransform.anchorMin = new Vector2(0, 0.6f);
        _dCurBonusLabel.rectTransform.anchorMax = new Vector2(1, 1);
        _dCurBonusLabel.rectTransform.offsetMin = _dCurBonusLabel.rectTransform.offsetMax = Vector2.zero;
        _dCurBonusLabel.text = "CURRENT BONUS";

        _dCurBonusValue = MkLbl("CurV", col1, 17, FontStyles.Normal, C_Text, TextAlignmentOptions.Left); // Sized up 30%
        _dCurBonusValue.rectTransform.anchorMin = new Vector2(0, 0f);
        _dCurBonusValue.rectTransform.anchorMax = new Vector2(1, 0.6f);
        _dCurBonusValue.rectTransform.offsetMin = _dCurBonusValue.rectTransform.offsetMax = Vector2.zero;

        var col2 = MkRect("Col2", colContainer, 0.52f, 0f, 0.95f, 1f);
        _dNxtBonusLabel = MkLbl("NxtH", col2, 12, FontStyles.Bold, C_Sub, TextAlignmentOptions.Left); // Sized up 20%
        _dNxtBonusLabel.rectTransform.anchorMin = new Vector2(0, 0.6f);
        _dNxtBonusLabel.rectTransform.anchorMax = new Vector2(1, 1);
        _dNxtBonusLabel.rectTransform.offsetMin = _dNxtBonusLabel.rectTransform.offsetMax = Vector2.zero;
        _dNxtBonusLabel.text = "UPGRADE BENEFIT";

        _dNxtBonusValue = MkLbl("NxtV", col2, 17, FontStyles.Normal, C_Text, TextAlignmentOptions.Left); // Sized up 30%
        _dNxtBonusValue.rectTransform.anchorMin = new Vector2(0, 0f);
        _dNxtBonusValue.rectTransform.anchorMax = new Vector2(1, 0.6f);
        _dNxtBonusValue.rectTransform.offsetMin = _dNxtBonusValue.rectTransform.offsetMax = Vector2.zero;

        MkDivider(cRt, y); y -= 12;

        var powHdr = PosLbl("PowH", cRt, 13, FontStyles.Bold, C_Gold, TextAlignmentOptions.Left, DP, y, -DP, 18); y -= 20; // Sized up 20%
        powHdr.text = "FACILITY POWER RATING";
        _dPower = PosLbl("PowV", cRt, 17, FontStyles.Bold, C_Text, TextAlignmentOptions.Left, DP + 8, y, -DP, 22); y -= 26; // Sized up 30%

        MkDivider(cRt, y); y -= 12;

        var unlHdr = PosLbl("UnlH", cRt, 13, FontStyles.Bold, C_Gem, TextAlignmentOptions.Left, DP, y, -DP, 18); y -= 20; // Sized up 20%
        unlHdr.text = "CITADEL SYSTEM UNLOCKS";
        _dUnlocks = PosLbl("UnlV", cRt, 17, FontStyles.Normal, C_Text, TextAlignmentOptions.Left, DP + 8, y, -DP, 22); y -= 26; // Sized up 30%

        MkDivider(cRt, y); y -= 12;
        _dCostTxt = PosLbl("Cost", cRt, 18, FontStyles.Bold, C_Gold, TextAlignmentOptions.Left, DP, y, -DP, 26); y -= 32;

        // Upgrade button: flat gold, dark text, 2px darker gold bottom border
        var upgGo = NewGO("UpgBtn", panel);
        var upgRt = upgGo.GetComponent<RectTransform>();
        upgRt.anchorMin = new Vector2(0, 0); upgRt.anchorMax = new Vector2(1, 0);
        upgRt.pivot = new Vector2(0.5f, 0);
        upgRt.offsetMin = new Vector2(DP, DP + 58f + 20f); 
        upgRt.offsetMax = new Vector2(-DP, DP + 58f + 20f + 64f);
        _btnUpg = upgGo.AddComponent<Button>();
        
        var btnImg = upgRt.gameObject.AddComponent<Image>();
        btnImg.color = C_UpgBtn; // Flat Gold #c9a227

        // 2px darker gold bottom border for depth
        var bottomBdr = MkRect("BottomBdr", upgRt, 0f, 0f, 1f, 0f);
        bottomBdr.pivot = new Vector2(0.5f, 0f);
        bottomBdr.sizeDelta = new Vector2(0f, 2f);
        AddImg(bottomBdr, Hex("#a07d1a"));

        _btnUpgTxt = MkLbl("T", upgGo.transform, 20, FontStyles.Bold, Hex("#0a0a0a"), TextAlignmentOptions.Center);
        Stretch(_btnUpgTxt.rectTransform); _btnUpgTxt.text = "EXECUTE UPGRADE";
        _btnUpg.onClick.AddListener(OnUpgradePressed);

        _btnUpgErr = MkLbl("Err", panel, 12, FontStyles.Bold, C_Red, TextAlignmentOptions.Center);
        var errRt = _btnUpgErr.rectTransform;
        errRt.anchorMin = new Vector2(0f, 0f); errRt.anchorMax = new Vector2(1f, 0f);
        errRt.pivot = new Vector2(0.5f, 0f);
        errRt.offsetMin = new Vector2(DP, DP + 56f);
        errRt.offsetMax = new Vector2(-DP, DP + 56f + 20f);
        _btnUpgErr.text = "";

        // Secondary Button: tactical info button styled as active secondary
        var infoGo = NewGO("InfoBtn", panel);
        var infoRt = infoGo.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 0); infoRt.anchorMax = new Vector2(1, 0);
        infoRt.pivot = new Vector2(0.5f, 0);
        infoRt.offsetMin = new Vector2(DP, DP);
        infoRt.offsetMax = new Vector2(-DP, DP + 48f);
        infoGo.AddComponent<Button>();
        
        var infoImg = infoRt.gameObject.AddComponent<Image>();
        infoImg.color = C_InfoBtn;
        
        // 1px border #2a3a4a
        var infoBdr = MkRect("Bdr", infoRt, 0f, 0f, 1f, 1f);
        infoBdr.offsetMin = new Vector2(-1, -1); infoBdr.offsetMax = new Vector2(1, 1);
        AddImg(infoBdr, Hex("#2a3a4a")).raycastTarget = false;
        infoBdr.SetSiblingIndex(0);

        var infoTxt = MkLbl("T", infoGo.transform, 15, FontStyles.Bold, Hex("#a0aec0"), TextAlignmentOptions.Center);
        Stretch(infoTxt.rectTransform); infoTxt.text = "TACTICAL DETAILS / ASSIGN HEROES";

        // Placeholder
        var ph = MkLbl("DetailPlaceholder", panel, 16, FontStyles.Italic, C_Sub, TextAlignmentOptions.Center);
        ph.rectTransform.anchorMin = new Vector2(0, 0.3f);
        ph.rectTransform.anchorMax = new Vector2(1, 0.7f);
        ph.rectTransform.offsetMin = ph.rectTransform.offsetMax = Vector2.zero;
        ph.text = "Select a facility from the Citadel Map\nto inspect upgrading operations.";

        artGo.SetActive(false);
        cGo.SetActive(false);
    }

    void BuildArtSilhouette(RectTransform art, Color tint)
    {
        Color dk  = new Color(tint.r * 0.15f, tint.g * 0.15f, tint.b * 0.15f, 0.90f);
        Color md  = new Color(tint.r * 0.25f, tint.g * 0.25f, tint.b * 0.25f, 0.80f);
        Color glw = new Color(tint.r * 0.18f, tint.g * 0.18f, tint.b * 0.18f, 0.50f);

        AddImg(MkRect("CT",  art, 0.35f, 0.10f, 0.65f, 0.85f), dk);
        AddImg(MkRect("CTT", art, 0.40f, 0.80f, 0.60f, 0.98f), md);
        AddImg(MkRect("LT",  art, 0.12f, 0.20f, 0.35f, 0.70f), new Color(dk.r * 0.8f, dk.g * 0.8f, dk.b * 0.8f, 0.85f));
        AddImg(MkRect("RT",  art, 0.65f, 0.20f, 0.88f, 0.70f), new Color(dk.r * 0.8f, dk.g * 0.8f, dk.b * 0.8f, 0.85f));
        AddImg(MkRect("GL",  art, 0.22f, 0f, 0.78f, 0.25f), glw);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CITADEL MAP POPULATION (ORGANIC/TACTICAL CITY-VIEW PATTERN)
    // ─────────────────────────────────────────────────────────────────────────
    void PopulateCitadelMap()
    {
        foreach (Transform ch in _mapRoot) Destroy(ch.gameObject);
        _bldCards.Clear();
        _paths.Clear();
        StopAllCoroutines();
        _pulses.Clear();
        _upgBtnPulse = null;

        if (_fac == null) return;
        var defs = _fac.GetAllFacilities();

        Vector2 posWorkshop = new Vector2(0.50f, 0.83f);
        Vector2 posMemorial = new Vector2(0.20f, 0.62f);
        Vector2 posCrucible = new Vector2(0.80f, 0.62f);
        Vector2 posTactical = new Vector2(0.50f, 0.44f);
        Vector2 posDorms    = new Vector2(0.20f, 0.26f);
        Vector2 posTraining = new Vector2(0.80f, 0.26f);
        Vector2 posSquare   = new Vector2(0.50f, 0.10f);
        Vector2 posFlying   = new Vector2(0.84f, 0.44f);

        // Draw connections first (rendered behind card layers as background connectors)
        DrawCitadelPaths(posWorkshop, posMemorial, posCrucible, posTactical, posDorms, posTraining, posSquare, posFlying);

        // Now place building cards
        PlaceTowerCard(defs, "workshop",         posWorkshop.x, posWorkshop.y, "COMMAND CENTER", true);
        PlaceTowerCard(defs, "memorial_hall",    posMemorial.x, posMemorial.y, "MEMORIAL HALL", false);
        PlaceTowerCard(defs, "crucible",         posCrucible.x, posCrucible.y, "CRUCIBLE", false);
        PlaceTowerCard(defs, "tactical_station", posTactical.x, posTactical.y, "TACTICAL STATION", false);
        PlaceTowerCard(defs, "dorms",            posDorms.x,    posDorms.y,    "DORMS", false);
        PlaceTowerCard(defs, "training_hall",    posTraining.x, posTraining.y, "TRAINING HALL", false);
        PlaceTowerCard(defs, "square",           posSquare.x,   posSquare.y,   "THE SQUARE", false);
        PlaceTowerCard(defs, "flying_dock",      posFlying.x,   posFlying.y,   "FLYING DOCK", false);

        // Locked outposts
        PlaceLockedTower("Smithy",        "UNLOCK FLOOR 15", 0.12f, 0.44f);
        PlaceLockedTower("Barracks",      "UNLOCK FLOOR 20", 0.50f, 0.64f);

        // Ambient particles
        StartCoroutine(SpawnParticles(_mapRoot));
    }

    void DrawCitadelPaths(
        Vector2 CC, Vector2 Mem, Vector2 Cruc, Vector2 Tac, 
        Vector2 Dor, Vector2 Train, Vector2 Sq, Vector2 Fly)
    {
        // Active links (active dependencies - mute teal #2a8a8a)
        DrawCitadelConnection(CC, Mem, "workshop", "memorial_hall", isActive: true);
        DrawCitadelConnection(CC, Cruc, "workshop", "crucible", isActive: true);
        DrawCitadelConnection(CC, Tac, "workshop", "tactical_station", isActive: true);
        DrawCitadelConnection(Mem, Tac, "memorial_hall", "tactical_station", isActive: true);
        DrawCitadelConnection(Cruc, Tac, "crucible", "tactical_station", isActive: true);
        DrawCitadelConnection(Tac, Dor, "tactical_station", "dorms", isActive: true);
        DrawCitadelConnection(Tac, Train, "tactical_station", "training_hall", isActive: true);
        DrawCitadelConnection(Dor, Sq, "dorms", "square", isActive: true);
        DrawCitadelConnection(Train, Sq, "training_hall", "square", isActive: true);
        DrawCitadelConnection(Cruc, Fly, "crucible", "flying_dock", isActive: true);
        DrawCitadelConnection(Tac, Fly, "tactical_station", "flying_dock", isActive: true);

        // Locked dependencies (dark gray #1a1a1a)
        DrawCitadelConnection(Mem, new Vector2(0.12f, 0.44f), "memorial_hall", "smithy", isActive: false);
        DrawCitadelConnection(Tac, new Vector2(0.50f, 0.64f), "tactical_station", "barracks", isActive: false);
    }

    void DrawCitadelConnection(Vector2 p1, Vector2 p2, string fromId, string toId, bool isActive)
    {
        float mapW = 1450.8f;
        float mapH = 944f;

        Vector2 startPx = new Vector2(p1.x * mapW, p1.y * mapH);
        Vector2 endPx   = new Vector2(p2.x * mapW, p2.y * mapH);

        if (Mathf.Abs(startPx.x - endPx.x) > 10f && Mathf.Abs(startPx.y - endPx.y) > 10f)
        {
            Vector2 elbowPx = new Vector2(endPx.x, startPx.y);
            DrawSegment(startPx, elbowPx, fromId, toId, isActive);
            DrawSegment(elbowPx, endPx, fromId, toId, isActive);
        }
        else
        {
            DrawSegment(startPx, endPx, fromId, toId, isActive);
        }
    }

    void DrawSegment(Vector2 from, Vector2 to, string fromId, string toId, bool isActive)
    {
        var lineGo = new GameObject("PathSeg", typeof(RectTransform), typeof(Image));
        lineGo.transform.SetParent(_mapRoot, false);
        lineGo.transform.SetAsFirstSibling(); // force behind cards
        var rt = lineGo.GetComponent<RectTransform>();

        Vector2 middle = Vector2.Lerp(from, to, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = middle;

        float dist = Vector2.Distance(from, to);
        bool isHorizontal = Mathf.Abs(from.y - to.y) < 2f;
        rt.sizeDelta = isHorizontal ? new Vector2(dist, 4f) : new Vector2(4f, dist);

        var img = lineGo.GetComponent<Image>();
        img.color = isActive ? C_TealActive : C_GrayDk;

        _paths.Add(new PathSegmentWidget
        {
            FromId = fromId, ToId = toId, Img = img, IsActive = isActive
        });
    }

    void PlaceTowerCard(List<FacilityDefinition> defs, string fid, float x, float y, string fallbackName, bool isCC)
    {
        var def = defs.Find(d => d?.FacilityId == fid);
        if (def == null) return;

        int level   = _fac.GetFacilityLevel(fid);
        bool canUpg = _fac.CanUpgrade(fid);

        var go = NewGO("Bld_" + fid, _mapRoot);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(x, y);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(210, 160);

        Color baseCol = new Color(def.FacilityColor.r * 0.08f, def.FacilityColor.g * 0.08f, def.FacilityColor.b * 0.08f, 1f);
        var cardImg = go.AddComponent<Image>();
        cardImg.color = baseCol;

        // 1px dark drop shadow #000000 at 40% opacity at bottom edge
        var shadowGo = NewGO("Shadow", rt);
        var shadowRt = shadowGo.GetComponent<RectTransform>();
        Stretch(shadowRt);
        shadowRt.offsetMin = new Vector2(0f, -4f); shadowRt.offsetMax = new Vector2(0f, -4f);
        shadowGo.transform.SetSiblingIndex(0);
        AddImg(shadowRt, new Color(0f, 0f, 0f, 0.40f));

        var glowGo = NewGO("Glow", rt);
        Stretch(glowGo.GetComponent<RectTransform>());
        glowGo.GetComponent<RectTransform>().offsetMin = new Vector2(-2, -2);
        glowGo.GetComponent<RectTransform>().offsetMax = new Vector2(2, 2);
        glowGo.transform.SetSiblingIndex(1);
        var glowImg = glowGo.AddComponent<Image>();
        glowImg.color = isCC ? C_Gold : new Color(C_CardBdr.r, C_CardBdr.g, C_CardBdr.b, 0.65f);

        // Tower Graphic
        var art = MkRect("TowerArt", rt, 0.05f, 0.35f, 0.95f, 0.95f);
        // Thumbnail brightened by 15% to increase visibility against dark background
        Color artBase = new Color(def.FacilityColor.r * 0.14f, def.FacilityColor.g * 0.14f, def.FacilityColor.b * 0.14f, 1f);
        AddImg(art, artBase);
        BuildMiniSilhouette(art, def.FacilityColor * 1.15f);
        var artFade = MkRect("Fade", art, 0, 0, 1, 0.30f);
        AddImg(artFade, new Color(baseCol.r, baseCol.g, baseCol.b, 0.85f));

        var spire = MkRect("SpireHighlight", art, 0.47f, 0.88f, 0.53f, 0.98f);
        AddImg(spire, def.FacilityColor);

        // Card banner tint based on strict color logic
        Color bannerColor = B_Support;
        if (fid == "workshop" || fid == "tactical_station" || fid == "training_hall" || fid == "square") bannerColor = B_Combat;
        else if (fid == "crucible") bannerColor = B_Special;

        var topBnd = MkRect("Bnd", rt, 0f, 0.95f, 1f, 1f);
        AddImg(topBnd, bannerColor);

        // Name Tag
        var nameLbl = MkLbl("Name", rt, 12f, FontStyles.Bold, C_Text, TextAlignmentOptions.Center);
        nameLbl.rectTransform.anchorMin = new Vector2(0f, 0.28f);
        nameLbl.rectTransform.anchorMax = new Vector2(1f, 0.44f);
        nameLbl.rectTransform.offsetMin = nameLbl.rectTransform.offsetMax = Vector2.zero;
        nameLbl.text = isCC ? "COMMAND CENTER" : def.DisplayName.ToUpperInvariant();

        var lvBadge = MkRect("LvPill", rt, 0.25f, 0.04f, 0.75f, 0.24f);
        AddImg(lvBadge, Hex("#1a1a1a"));
        var lvLbl = MkLbl("Lv", lvBadge, 10f, FontStyles.Bold, C_Text, TextAlignmentOptions.Center);
        Stretch(lvLbl.rectTransform);
        lvLbl.text = "LEVEL " + level;

        if (canUpg)
        {
            var notification = MkRect("UpgN", rt, 0.80f, 0.80f, 0.98f, 0.98f);
            AddImg(notification, C_Green);
            var nTxt = MkLbl("T", notification, 10f, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            Stretch(nTxt.rectTransform); nTxt.text = "+";
            if (!isCC) glowImg.color = C_Green;
        }

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        var cb = btn.colors;
        cb.highlightedColor = new Color(1.10f, 1.05f, 1.00f);
        cb.pressedColor     = new Color(0.85f, 0.85f, 0.85f);
        btn.colors = cb;
        string captId = fid;
        btn.onClick.AddListener(() => SelectBuilding(captId));

        var card = new FacilityBuildingCard
        {
            FacilityId = fid, Root = go, GlowImg = glowImg, CardImg = cardImg, CanUpgrade = canUpg
        };
        _bldCards.Add(card);
        if (canUpg) StartPulse(card);
    }

    void PlaceLockedTower(string name, string unlockReq, float x, float y)
    {
        var go = NewGO("Locked_" + name, _mapRoot);
        var rt = go.GetComponent<RectTransform>();
        
        rt.anchorMin = rt.anchorMax = new Vector2(x, y);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(210, 160);

        // Entire locked card feel ghosted: 40% opacity, desaturated, gray banner
        Color lockedBg = C_Locked;
        lockedBg.a = 0.40f;
        AddImg(rt, lockedBg);

        var shadowGo = NewGO("Shadow", rt);
        var shadowRt = shadowGo.GetComponent<RectTransform>();
        Stretch(shadowRt);
        shadowRt.offsetMin = new Vector2(0f, -4f); shadowRt.offsetMax = new Vector2(0f, -4f);
        shadowGo.transform.SetSiblingIndex(0);
        AddImg(shadowRt, new Color(0f, 0f, 0f, 0.20f)); // dimmed shadow

        // Padlock icon: gray at 50% opacity, no shine or glow
        var art = MkRect("PadlockArt", rt, 0.05f, 0.35f, 0.95f, 0.95f);
        AddImg(art, new Color(0.08f, 0.09f, 0.14f, 0.40f));
        
        Color padColor = Hex("#4a5568");
        padColor.a = 0.50f;
        var shackle = MkRect("Shackle", art, 0.40f, 0.55f, 0.60f, 0.80f);
        AddImg(shackle, padColor);
        var shackleCut = MkRect("Cut", shackle, 0.20f, 0f, 0.80f, 0.70f);
        AddImg(shackleCut, new Color(0.08f, 0.09f, 0.14f, 1f));

        var lockBody = MkRect("Body", art, 0.33f, 0.25f, 0.67f, 0.58f);
        AddImg(lockBody, padColor);

        var topBnd = MkRect("Bnd", rt, 0f, 0.95f, 1f, 1f);
        AddImg(topBnd, B_Locked); // strict gray banner #2a2a2a

        var nameLbl = MkLbl("Name", rt, 11f, FontStyles.Bold, C_LckTxt, TextAlignmentOptions.Center);
        nameLbl.rectTransform.anchorMin = new Vector2(0f, 0.05f);
        nameLbl.rectTransform.anchorMax = new Vector2(1f, 0.25f);
        nameLbl.rectTransform.offsetMin = nameLbl.rectTransform.offsetMax = Vector2.zero;
        nameLbl.text = name.ToUpperInvariant();

        var trigger = go.AddComponent<EventTrigger>();
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => ShowTooltip(name, unlockReq, rt.anchoredPosition));
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => HideTooltip());
        trigger.triggers.Add(entryExit);
    }

    void ShowTooltip(string name, string req, Vector2 pos)
    {
        var tt = GameObject.Find("CitadelTooltip") ?? NewGO("CitadelTooltip", _mapRoot);
        tt.name = "CitadelTooltip";
        var rt = tt.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(220, 80);
        rt.anchoredPosition = pos + new Vector2(0, 95f);

        var bg = GetOrAdd<Image>(tt);
        bg.color = Hex("#0c0f16");
        
        var txt = tt.GetComponentInChildren<TextMeshProUGUI>() ?? MkLbl("T", tt.transform, 11, FontStyles.Bold, C_Gold, TextAlignmentOptions.Center);
        Stretch(txt.rectTransform);
        txt.text = name.ToUpper() + "\nRequires: " + req;
        tt.SetActive(true);
    }

    void HideTooltip()
    {
        var tt = GameObject.Find("CitadelTooltip");
        if (tt != null) tt.SetActive(false);
    }

    void BuildMiniSilhouette(RectTransform art, Color facilityColor)
    {
        float r = Mathf.Clamp01(facilityColor.r * 1.5f);
        float g = Mathf.Clamp01(facilityColor.g * 1.5f);
        float b = Mathf.Clamp01(facilityColor.b * 1.5f);
        Color dk  = new Color(r * 0.18f, g * 0.18f, b * 0.18f, 0.90f);
        Color brt = new Color(r * 0.35f, g * 0.35f, b * 0.35f, 0.70f);
        Color glw = new Color(r * 0.22f, g * 0.22f, b * 0.22f, 0.50f);

        AddImg(MkRect("CT",  art, 0.38f, 0.10f, 0.62f, 0.88f), dk);
        AddImg(MkRect("CTT", art, 0.43f, 0.80f, 0.57f, 0.98f), brt);
        AddImg(MkRect("LT",  art, 0.18f, 0.18f, 0.38f, 0.72f), new Color(dk.r * 0.8f, dk.g * 0.8f, dk.b * 0.8f, 0.85f));
        AddImg(MkRect("RT",  art, 0.62f, 0.18f, 0.82f, 0.72f), new Color(dk.r * 0.8f, dk.g * 0.8f, dk.b * 0.8f, 0.85f));
        AddImg(MkRect("GL",  art, 0.28f, 0f, 0.72f, 0.28f), glw);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SELECTION & DETAIL PANEL HANDLING
    // ─────────────────────────────────────────────────────────────────────────
    void SelectBuilding(string fid)
    {
        if (!string.IsNullOrEmpty(_selId))
        {
            var old = _bldCards.Find(c => c.FacilityId == _selId);
            old?.SetSelected(false, _fac?.CanUpgrade(_selId) ?? false);
        }
        _selId = fid;
        
        foreach (var card in _bldCards)
        {
            if (card.Root != null)
            {
                card.Root.transform.localScale = (card.FacilityId == fid) ? Vector3.one : (Vector3.one * 0.95f);
            }
        }

        var sel = _bldCards.Find(c => c.FacilityId == fid);
        sel?.SetSelected(true, false);
        
        // Dynamic subtle pulse glow on paths connected to the currently selected card
        foreach (var p in _paths)
        {
            if (p.Img == null) continue;
            if (p.FromId == fid || p.ToId == fid)
            {
                p.Img.color = C_TealActive;
                if (p.PulseCoroutine != null) StopCoroutine(p.PulseCoroutine);
                p.PulseCoroutine = StartCoroutine(PulseActivePath(p.Img));
            }
            else
            {
                if (p.PulseCoroutine != null) StopCoroutine(p.PulseCoroutine);
                p.Img.color = p.IsActive ? C_TealActive : C_GrayDk;
            }
        }

        RefreshDetail(fid);
        SetDetailVisible(true);
    }

    IEnumerator PulseActivePath(Image img)
    {
        while (img != null)
        {
            float pingPong = Mathf.PingPong(Time.time * 2f, 1f);
            img.color = Color.Lerp(C_TealActive * 0.7f, C_TealActive * 1.3f, pingPong);
            yield return null;
        }
    }

    void RefreshDetail(string fid)
    {
        if (_fac == null) return;
        var def = _fac.GetFacility(fid);
        if (def == null) return;

        int level   = _fac.GetFacilityLevel(fid);
        bool atMax  = level >= def.MaxLevel;
        bool canUpg = _fac.CanUpgrade(fid);
        var cost    = _fac.GetUpgradeCost(fid);

        if (_dName  != null) _dName.text  = fid == "workshop" ? "COMMAND CENTER" : def.DisplayName.ToUpperInvariant();
        if (_dLevel != null) _dLevel.text = atMax
            ? "Lv. " + level + "  [MAX REGISTRATION]"
            : "Lv. " + level + " -> Lv. " + (level + 1);
        if (_dDesc  != null) { _dDesc.text  = _fac.GetFacilityEmotion(fid); _dDesc.color = C_Sub; }
        
        if (_dCurBonusValue != null) _dCurBonusValue.text = def.GetBenefitDescription(level);
        if (_dNxtBonusValue != null)
        {
            _dNxtBonusValue.text = "<color=#2EA869>" + def.GetNextLevelBenefitDescription(level) + "</color>";
        }

        if (_dCostTxt  != null) _dCostTxt.text  = atMax
            ? "Fortress expansion complete."
            : "REQUIREMENT: " + cost.Gold.ToString("N0") + " Gold"
              + (cost.Gems > 0 ? " + " + cost.Gems + " Gems" : "");

        if (_dPower != null)
        {
            int powerRating = level * 350 + (fid == "workshop" ? 500 : 150);
            _dPower.text = powerRating + " PWR (" + (canUpg ? "+" + 350 : "MAX") + ")";
        }

        if (_dUnlocks != null)
        {
            _dUnlocks.text = atMax 
                ? "All systems functional."
                : "Lv. " + (level + 1) + " Unlocks: Advanced " + def.DisplayName + " tier";
        }

        if (_detArtBg != null)
            _detArtBg.color = new Color(
                def.FacilityColor.r * 0.12f,
                def.FacilityColor.g * 0.12f,
                def.FacilityColor.b * 0.12f, 1f);

        if (_btnUpg != null)
        {
            _btnUpg.interactable = canUpg;
            var bi = _btnUpg.GetComponent<Image>();
            
            if (_upgBtnPulse != null) StopCoroutine(_upgBtnPulse);
            
            if (canUpg)
            {
                _btnUpgTxt.text = "EXECUTE UPGRADE";
                _btnUpgTxt.color = Hex("#0a0a0a");
                _btnUpgErr.text = ""; 
                if (bi != null) bi.color = C_UpgBtn; // Flat Gold #c9a227
            }
            else
            {
                _btnUpgTxt.text = atMax ? "MAX EXPANSION" : "EXECUTE UPGRADE";
                _btnUpgTxt.color = Hex("#666666");
                if (bi != null) bi.color = new Color(0.15f, 0.15f, 0.15f, 1.0f);
                _btnUpgErr.text = atMax ? "" : "INSUFFICIENT FUNDS";
            }
        }
    }

    string GetCitadelLore(string fid)
    {
        switch (fid)
        {
            case "workshop":
                return "The core engine powering the floating base's internal grid. Contains synthesizers and heavy forge tools.";
            case "memorial_hall":
                return "A quiet shrine displaying echoes of ancient legends. Tracks the achievements of your summoned roster.";
            case "crucible":
                return "A glowing laboratory dedicated to synthesizing raw power and combining hero essences.";
            case "tactical_station":
                return "The operational bridge. Deciphers enemy patrol routes and tracks fortress threat level indices.";
            case "dorms":
                return "Sleeping quarters constructed to shield weary warriors from dimensional radiation.";
            case "training_hall":
                return "Equipped with automated dummy simulators. Accelerates basic combat drills for novices.";
            case "square":
                return "A temporal dimensional portal connecting the fortress to spatial rifts and anomalies.";
            case "flying_dock":
                return "The harbor for airships departing to outer world expeditions and raiding campaigns.";
            default:
                return "A dark stone monolith built from ruins recovered during early floor clear expeditions.";
        }
    }

    void SetDetailVisible(bool visible)
    {
        if (_detRoot == null) return;
        foreach (Transform ch in _detRoot)
        {
            if (ch.name == "DetailContent")    ch.gameObject.SetActive(visible);
            if (ch.name == "DetailPlaceholder") ch.gameObject.SetActive(!visible);
            if (ch.name == "ArtArea")          ch.gameObject.SetActive(visible);
        }
    }

    void OnUpgradePressed()
    {
        if (string.IsNullOrEmpty(_selId) || _fac == null) return;
        bool ok = _fac.StartUpgrade(_selId);
        if (ok)
        {
            RefreshHeader();
            RefreshMoraleBar();
            RefreshDetail(_selId);
            PopulateCitadelMap();
            var card = _bldCards.Find(c => c.FacilityId == _selId);
            card?.SetSelected(true, false);
            SetDetailVisible(true);
        }
        else if (_btnUpgErr != null)
        {
            _btnUpgErr.text = "INSUFFICIENT FUNDS";
        }
    }

    void RefreshHeader()
    {
        if (_cur == null) return;
        if (_lblGold != null) _lblGold.text = _cur.GetGold().ToString("N0");
        if (_lblGems != null) _lblGems.text = _cur.GetGems().ToString("N0");
        if (_lblMor  != null)
        {
            int m = GetAvgMorale();
            _lblMor.text  = m.ToString() + " / 100";
            _lblMor.color = m < 30 ? C_Red : m < 60 ? C_Gold : C_Green;
        }
    }

    void RefreshMoraleBar()
    {
        if (_morFill == null) return;
        int morale = GetAvgMorale();
        float t = Mathf.Clamp01(morale / 100f);
        _morFill.rectTransform.anchorMax = new Vector2(t, 1f);
        _morFill.color = Color.Lerp(C_Red, C_Green, t);
        if (_morTxt != null) _morTxt.text = morale + " / 100";
    }

    int GetAvgMorale()
    {
        if (_ros == null) return 0;
        var alive = _ros.GetAlive();
        if (alive == null || alive.Count == 0) return 0;
        return Mathf.RoundToInt((float)alive.Average(h => h.Morale));
    }

    void StartPulse(FacilityBuildingCard card)
    {
        if (card?.GlowImg == null) return;
        if (_pulses.TryGetValue(card.FacilityId, out var old) && old != null) StopCoroutine(old);
        _pulses[card.FacilityId] = StartCoroutine(PulseGlow(card.GlowImg, C_Green, 0.20f, 0.80f, 0.8f));
    }

    IEnumerator PulseGlow(Image img, Color col, float minA, float maxA, float speed)
    {
        float t = 0f;
        while (img != null)
        {
            float alpha = Mathf.Lerp(minA, maxA, (Mathf.Sin(t * speed * Mathf.PI * 2f) + 1f) * 0.5f);
            img.color = new Color(col.r, col.g, col.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator SpawnParticles(RectTransform mapRoot)
    {
        float[]  xs   = { 0.10f, 0.22f, 0.38f, 0.50f, 0.62f, 0.78f, 0.90f };
        Color[]  cols = {
            new Color(C_Gold.r,  C_Gold.g,  C_Gold.b,  0.22f),
            new Color(0.28f, 0.45f, 0.85f, 0.20f),
            new Color(C_Green.r, C_Green.g, C_Green.b, 0.18f)
        };
        int idx = 0;

        while (mapRoot != null)
        {
            yield return new WaitForSeconds(1.10f);
            if (mapRoot == null) yield break;

            var pGo = new GameObject("P", typeof(RectTransform), typeof(Image));
            pGo.transform.SetParent(mapRoot, false);
            var pRt = pGo.GetComponent<RectTransform>();
            float xFrac = xs[Random.Range(0, xs.Length)];
            pRt.anchorMin = new Vector2(xFrac, 0);
            pRt.anchorMax = new Vector2(xFrac, 0);
            pRt.pivot     = new Vector2(0.5f, 0.5f);
            float sz = Random.Range(4f, 8f);
            pRt.sizeDelta        = new Vector2(sz, sz);
            pRt.anchoredPosition = new Vector2(0, Random.Range(20f, 600f));
            pGo.GetComponent<Image>().color = cols[idx % cols.Length];
            idx++;

            StartCoroutine(FloatParticle(pGo, pRt, pGo.GetComponent<Image>()));
        }
    }

    IEnumerator FloatParticle(GameObject go, RectTransform rt, Image img)
    {
        float dur     = Random.Range(4f, 7f);
        float elapsed = 0f;
        float startY  = rt != null ? rt.anchoredPosition.y : 0f;
        float rise    = Random.Range(100f, 250f);
        Color startC  = img != null ? img.color : Color.clear;

        while (elapsed < dur && go != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            if (rt  != null) rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY + t * rise);
            if (img != null) img.color           = new Color(startC.r, startC.g, startC.b, startC.a * (1f - t));
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    static GameObject NewGO(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
    static GameObject NewGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
    static RectTransform MkRect(string name, RectTransform parent, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }
    static T GetOrAdd<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    static Image AddImg(RectTransform rt, Color col)
    {
        var img = rt.gameObject.GetComponent<Image>() ?? rt.gameObject.AddComponent<Image>();
        img.color = col; return img;
    }
    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
    static TextMeshProUGUI MkLbl(string name, RectTransform parent, float sz, FontStyles style, Color col, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = sz; tmp.fontStyle = style; tmp.color = col; tmp.alignment = align;
        return tmp;
    }
    static TextMeshProUGUI MkLbl(string name, Transform parent, float sz, FontStyles style, Color col, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = sz; tmp.fontStyle = style; tmp.color = col; tmp.alignment = align;
        return tmp;
    }
    static TextMeshProUGUI PosLbl(string name, RectTransform parent, float sz, FontStyles style, Color col, TextAlignmentOptions align, float padL, float yFromTop, float padR, float h)
    {
        var lbl = MkLbl(name, parent, sz, style, col, align);
        var rt  = lbl.rectTransform;
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0, 1);
        rt.offsetMin = new Vector2(padL, 0); rt.offsetMax = new Vector2(padR, 0);
        rt.sizeDelta = new Vector2(0, h);
        rt.anchoredPosition = new Vector2(padL, yFromTop);
        return lbl;
    }
    static void MkDivider(RectTransform parent, float yFromTop)
    {
        var go = new GameObject("Div", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.03f, 1); rt.anchorMax = new Vector2(0.97f, 1);
        rt.pivot = new Vector2(0.5f, 1); rt.sizeDelta = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(0, yFromTop);
        go.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.6f);
    }
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }
}

// ─────────────────────────────────────────────────────────────────────────────
public class PathSegmentWidget
{
    public string FromId;
    public string ToId;
    public Image Img;
    public bool IsActive;
    public Coroutine PulseCoroutine;
}

// ─────────────────────────────────────────────────────────────────────────────
public class FacilityBuildingCard
{
    public string     FacilityId;
    public GameObject Root;
    public Image      GlowImg;
    public Image      CardImg;
    public bool       CanUpgrade;

    public void SetSelected(bool selected, bool upgradeReady)
    {
        if (GlowImg == null) return;
        if (selected)
            GlowImg.color = new Color(0.784f, 0.659f, 0.294f, 1.00f);  // gold glow
        else if (upgradeReady)
            GlowImg.color = new Color(0.227f, 0.722f, 0.478f, 0.55f);  // green
        else
            GlowImg.color = new Color(0.118f, 0.149f, 0.271f, 0.70f);  // dark border
    }
}
