using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Procedurally builds the full Summon scene UI.
/// Run from: Tools > Pick Me Up > Setup Summon UI
///
/// IMPORTANT: Open the Summon scene BEFORE running this tool.
///
/// Creates:
///   SummonCanvas
///   ├── Background (dark panel)
///   ├── TopBar
///   │   ├── GoldLabel
///   │   └── GemsLabel
///   ├── BannerTabs
///   │   ├── StandardTab
///   │   └── PremiumTab
///   ├── BannerPanel
///   │   ├── StandardPanel
///   │   │   ├── PityStandardLabel
///   │   │   ├── SummonStandardBtn  (x1 — 1,000 Gold)
///   │   │   └── SummonStandardTenBtn (x10 — 9,000 Gold)
///   │   └── PremiumPanel
///   │       ├── PityPremiumLabel
///   │       ├── SummonPremiumBtn   (x1 — 300 Gems)
///   │       └── SummonPremiumTenBtn (x10 — 2,700 Gems)
///   ├── CardRevealPanel
///   │   ├── CrackOverlay
///   │   ├── CardRoot
///   │   │   ├── CardFrame
///   │   │   ├── CardPortrait
///   │   │   ├── NameBanner
///   │   │   │   ├── CardNameLabel
///   │   │   │   └── CardClassLabel
///   │   │   └── StarsContainer
///   │   └── TapToContinueLabel
///   └── BackBtn
/// </summary>
public static class SetupSummonUI
{
    // ── Colors ─────────────────────────────────────────────────────────────
    private static readonly Color BgColor        = new Color(0.04f, 0.04f, 0.08f, 1f);
    private static readonly Color PanelColor      = new Color(0.08f, 0.08f, 0.14f, 1f);
    private static readonly Color AccentGold      = new Color(0.85f, 0.65f, 0.10f, 1f);
    private static readonly Color AccentPurple    = new Color(0.55f, 0.20f, 0.80f, 1f);
    private static readonly Color ButtonStandard  = new Color(0.15f, 0.35f, 0.15f, 1f);
    private static readonly Color ButtonPremium   = new Color(0.30f, 0.10f, 0.50f, 1f);
    private static readonly Color ButtonBack      = new Color(0.20f, 0.20f, 0.20f, 1f);
    private static readonly Color CrackColor      = new Color(0.95f, 0.75f, 0.10f, 0f);

    [MenuItem("Tools/Pick Me Up/Setup Summon UI")]
    public static void CreateSummonUI()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        // ── Root canvas ────────────────────────────────────────────────────
        GameObject canvasGo = new GameObject("SummonCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2340);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Background ─────────────────────────────────────────────────────
        GameObject bg = CreatePanel("Background", canvasGo.transform, BgColor);
        Stretch(bg);

        // ── Top bar ────────────────────────────────────────────────────────
        GameObject topBar = CreatePanel("TopBar", canvasGo.transform, new Color(0.06f, 0.06f, 0.10f, 1f));
        SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1));
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchoredPosition = new Vector2(0, -90);
        topBarRT.sizeDelta = new Vector2(0, 180);

        GameObject goldLabel = CreateTMP("GoldLabel", "Gold: 0", 36, topBar.transform);
        SetAnchors(goldLabel, Vector2.zero, new Vector2(0.5f, 1f));
        goldLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        PadLeft(goldLabel, 30);

        GameObject gemsLabel = CreateTMP("GemsLabel", "Gems: 0", 36, topBar.transform);
        SetAnchors(gemsLabel, new Vector2(0.5f, 0), Vector2.one);
        gemsLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineRight;
        PadRight(gemsLabel, 30);

        // ── Banner panels ──────────────────────────────────────────────────
        // Standard Panel
        GameObject standardPanel = CreatePanel("StandardPanel", canvasGo.transform, PanelColor);
        SetAnchors(standardPanel, Vector2.zero, Vector2.one);
        RectTransform spRT = standardPanel.GetComponent<RectTransform>();
        spRT.offsetMin = new Vector2(40, 420);
        spRT.offsetMax = new Vector2(-40, -220);

        GameObject pityStd = CreateTMP("PityStandardLabel", "Pity: 0 / 180", 30, standardPanel.transform);
        SetAnchors(pityStd, new Vector2(0, 1), new Vector2(1, 1));
        pityStd.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 70);
        pityStd.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
        pityStd.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        pityStd.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);

        GameObject stdSingle = CreateButton("SummonStandardBtn", "✦  SUMMON  ×1\n1,000 Gold", standardPanel.transform, ButtonStandard, 36);
        SetAnchors(stdSingle, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.62f));

        GameObject stdTen = CreateButton("SummonStandardTenBtn", "✦  SUMMON  ×10\n9,000 Gold", standardPanel.transform, AccentGold, 36);
        SetAnchors(stdTen, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.32f));
        stdTen.GetComponent<Image>().color = new Color(0.70f, 0.50f, 0.05f, 1f);

        // Premium Panel (hidden by default — tab system can be wired later)
        GameObject premiumPanel = CreatePanel("PremiumPanel", canvasGo.transform, PanelColor);
        SetAnchors(premiumPanel, Vector2.zero, Vector2.one);
        RectTransform ppRT = premiumPanel.GetComponent<RectTransform>();
        ppRT.offsetMin = new Vector2(40, 420);
        ppRT.offsetMax = new Vector2(-40, -220);
        premiumPanel.SetActive(false); // hidden until tab is implemented

        GameObject pityPrem = CreateTMP("PityPremiumLabel", "Pity: 0 / 100", 30, premiumPanel.transform);
        SetAnchors(pityPrem, new Vector2(0, 1), new Vector2(1, 1));
        pityPrem.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 70);
        pityPrem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
        pityPrem.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        pityPrem.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);

        GameObject premSingle = CreateButton("SummonPremiumBtn", "◆  SUMMON  ×1\n300 Gems", premiumPanel.transform, ButtonPremium, 36);
        SetAnchors(premSingle, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.62f));

        GameObject premTen = CreateButton("SummonPremiumTenBtn", "◆  SUMMON  ×10\n2,700 Gems", premiumPanel.transform, AccentPurple, 36);
        SetAnchors(premTen, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.32f));

        // ── Tab buttons ────────────────────────────────────────────────────
        GameObject tabBar = new GameObject("BannerTabs");
        tabBar.transform.SetParent(canvasGo.transform, false);
        tabBar.AddComponent<RectTransform>();
        SetAnchors(tabBar, new Vector2(0, 1), new Vector2(1, 1));
        RectTransform tabRT = tabBar.GetComponent<RectTransform>();
        tabRT.anchoredPosition = new Vector2(0, -215);
        tabRT.sizeDelta = new Vector2(0, 100);

        HorizontalLayoutGroup tabHLG = tabBar.AddComponent<HorizontalLayoutGroup>();
        tabHLG.childForceExpandWidth = true;
        tabHLG.childForceExpandHeight = true;
        tabHLG.spacing = 4;
        tabHLG.padding = new RectOffset(40, 40, 0, 0);

        GameObject stdTab = CreateButton("StandardTab", "STANDARD", tabBar.transform, ButtonStandard, 28);
        GameObject premTab = CreateButton("PremiumTab", "PREMIUM",  tabBar.transform, ButtonPremium, 28);

        // Tab toggles panels
        stdTab.GetComponent<Button>().onClick.AddListener(() => {
            standardPanel.SetActive(true);
            premiumPanel.SetActive(false);
        });
        premTab.GetComponent<Button>().onClick.AddListener(() => {
            standardPanel.SetActive(false);
            premiumPanel.SetActive(true);
        });

        // ── Card reveal panel ──────────────────────────────────────────────
        GameObject cardRevealPanel = CreatePanel("CardRevealPanel", canvasGo.transform, new Color(0, 0, 0, 0.90f));
        Stretch(cardRevealPanel);
        cardRevealPanel.SetActive(false);

        // Crack overlay (full screen flash)
        GameObject crackOverlayGo = CreatePanel("CrackOverlay", cardRevealPanel.transform, CrackColor);
        Stretch(crackOverlayGo);
        crackOverlayGo.SetActive(false);

        // Card root (centered)
        GameObject cardRoot = new GameObject("CardRoot");
        cardRoot.transform.SetParent(cardRevealPanel.transform, false);
        RectTransform cardRootRT = cardRoot.AddComponent<RectTransform>();
        cardRootRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRootRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRootRT.sizeDelta = new Vector2(600, 900);
        cardRootRT.anchoredPosition = Vector2.zero;
        cardRoot.SetActive(false);

        // Card frame
        GameObject cardFrameGo = CreatePanel("CardFrame", cardRoot.transform, new Color(0.25f, 0.20f, 0.10f, 1f));
        Stretch(cardFrameGo);

        // Card portrait
        GameObject cardPortraitGo = CreatePanel("CardPortrait", cardRoot.transform, Color.black);
        SetAnchors(cardPortraitGo, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.90f));

        // Name banner
        GameObject nameBanner = CreatePanel("NameBanner", cardRoot.transform, new Color(0, 0, 0, 0.75f));
        SetAnchors(nameBanner, new Vector2(0, 0.08f), new Vector2(1, 0.20f));

        GameObject cardName = CreateTMP("CardNameLabel", "◄  HERO NAME  ►", 42, nameBanner.transform);
        Stretch(cardName);
        TextMeshProUGUI cardNameTMP = cardName.GetComponent<TextMeshProUGUI>();
        cardNameTMP.alignment = TextAlignmentOptions.Center;
        cardNameTMP.fontStyle = FontStyles.Bold;
        cardNameTMP.color = Color.white;

        GameObject cardClass = CreateTMP("CardClassLabel", "CLASS", 28, nameBanner.transform);
        SetAnchors(cardClass, new Vector2(0, 0), new Vector2(1, 0.35f));
        cardClass.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        cardClass.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        // Stars container
        GameObject starsContainer = new GameObject("StarsContainer");
        starsContainer.transform.SetParent(cardRoot.transform, false);
        RectTransform starsRT = starsContainer.AddComponent<RectTransform>();
        starsRT.anchorMin = new Vector2(0.1f, 0.01f);
        starsRT.anchorMax = new Vector2(0.9f, 0.09f);
        starsRT.offsetMin = Vector2.zero;
        starsRT.offsetMax = Vector2.zero;
        HorizontalLayoutGroup starsHLG = starsContainer.AddComponent<HorizontalLayoutGroup>();
        starsHLG.childAlignment = TextAnchor.MiddleCenter;
        starsHLG.spacing = 8;
        starsHLG.childForceExpandWidth = false;
        starsHLG.childForceExpandHeight = false;

        // Tap to continue label
        GameObject tapLabel = CreateTMP("TapToContinueLabel", "tap to continue", 28, cardRevealPanel.transform);
        SetAnchors(tapLabel, new Vector2(0, 0), new Vector2(1, 0));
        tapLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 80);
        tapLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        tapLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        tapLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f);

        // ── Back button ────────────────────────────────────────────────────
        GameObject backBtnGo = CreateButton("BackBtn", "←  BACK", canvasGo.transform, ButtonBack, 32);
        SetAnchors(backBtnGo, new Vector2(0, 0), new Vector2(0.35f, 0));
        RectTransform backRT = backBtnGo.GetComponent<RectTransform>();
        backRT.sizeDelta = new Vector2(0, 130);
        backRT.anchoredPosition = new Vector2(0, 70);

        // ── Wire SummonView ────────────────────────────────────────────────
        SummonView summonView = canvasGo.AddComponent<SummonView>();
        SerializedObject so = new SerializedObject(summonView);

        so.FindProperty("summonStandardBtn").objectReferenceValue    = stdSingle.GetComponent<Button>();
        so.FindProperty("summonStandardTenBtn").objectReferenceValue = stdTen.GetComponent<Button>();
        so.FindProperty("summonPremiumBtn").objectReferenceValue     = premSingle.GetComponent<Button>();
        so.FindProperty("summonPremiumTenBtn").objectReferenceValue  = premTen.GetComponent<Button>();
        so.FindProperty("backBtn").objectReferenceValue              = backBtnGo.GetComponent<Button>();

        so.FindProperty("goldCostLabel").objectReferenceValue    = goldLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("gemsCostLabel").objectReferenceValue    = gemsLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("pityStandardLabel").objectReferenceValue = pityStd.GetComponent<TextMeshProUGUI>();
        so.FindProperty("pityPremiumLabel").objectReferenceValue  = pityPrem.GetComponent<TextMeshProUGUI>();

        so.FindProperty("cardRevealPanel").objectReferenceValue = cardRevealPanel;
        so.FindProperty("cardPortrait").objectReferenceValue    = cardPortraitGo.GetComponent<Image>();
        so.FindProperty("cardFrame").objectReferenceValue       = cardFrameGo.GetComponent<Image>();
        so.FindProperty("cardNameLabel").objectReferenceValue   = cardNameTMP;
        so.FindProperty("cardClassLabel").objectReferenceValue  = cardClass.GetComponent<TextMeshProUGUI>();
        so.FindProperty("starsContainer").objectReferenceValue  = starsContainer.transform;
        so.FindProperty("crackOverlay").objectReferenceValue    = crackOverlayGo.GetComponent<Image>();
        so.FindProperty("cardRoot").objectReferenceValue        = cardRoot;

        so.ApplyModifiedProperties();

        // ── Save scene ─────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupSummonUI] Summon UI built successfully. Wire starIconPrefab manually if using star sprites.");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static GameObject CreateTMP(string name, string text, float fontSize, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return go;
    }

    private static GameObject CreateButton(string name, string label, Transform parent, Color color, float fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<Button>();

        GameObject textGo = CreateTMP("Text", label, fontSize, go.transform);
        Stretch(textGo);
        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

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

    private static void PadLeft(GameObject go, float pad)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(pad, rt.offsetMin.y);
    }

    private static void PadRight(GameObject go, float pad)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.offsetMax = new Vector2(-pad, rt.offsetMax.y);
    }
}
