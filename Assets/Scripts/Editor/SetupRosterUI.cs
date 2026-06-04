using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class SetupRosterUI
{
    // ── Design tokens (2400×1080 landscape reference) ─────────────────────
    private static readonly Color ColorDarkNavy     = new Color(0.04f, 0.04f, 0.07f);
    private static readonly Color ColorTopBar       = new Color(0.06f, 0.06f, 0.10f, 0.98f);
    private static readonly Color ColorFilterBar    = new Color(0.08f, 0.08f, 0.12f, 0.95f);
    private static readonly Color ColorGridBg      = new Color(0.03f, 0.03f, 0.06f);
    private static readonly Color ColorDetailBg    = new Color(0.08f, 0.08f, 0.14f, 0.98f);
    private static readonly Color ColorGold        = new Color(0.95f, 0.75f, 0.10f);
    private static readonly Color ColorGreen       = new Color(0.22f, 0.70f, 0.30f);
    private static readonly Color ColorRed         = new Color(0.80f, 0.20f, 0.20f);
    private static readonly Color ColorPromote     = new Color(0.20f, 0.60f, 0.20f);
    private static readonly Color ColorSelection     = new Color(0.20f, 0.50f, 0.90f, 0.30f);

    [MenuItem("Tools/Pick Me Up/Setup Roster UI")]
    public static void Execute()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGo;
        if (canvas == null)
        {
            canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGo = canvas.gameObject;
        }

        // ═══════ 2400×1080 LANDSCAPE CANVAS ═══════
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2400, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        // Background
        GameObject bgPanel = FindOrCreateChild(canvasGo.transform, "Background", () =>
        {
            GameObject go = CreatePanel("Background", canvasGo.transform, ColorDarkNavy);
            Stretch(go);
            return go;
        });
        bgPanel.transform.SetAsFirstSibling();

        RosterView rosterView = canvasGo.GetComponent<RosterView>();
        if (rosterView == null)
            rosterView = canvasGo.AddComponent<RosterView>();

        // ────────────────────────────────────────────────────────────
        //  TOP RESOURCE BAR  — full width, 120px height
        // ────────────────────────────────────────────────────────────
        GameObject topBar = FindOrCreateChild(canvasGo.transform, "TopBar", () =>
            CreatePanel("TopBar", canvasGo.transform, ColorTopBar));
        SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1));
        SetOffsets(topBar, 0, -130, 0, 0);

        // Back button
        GameObject backBtnG = FindOrCreateChild(topBar.transform, "BackBtn", () =>
            CreateButton("BackBtn", "<", topBar.transform, new Color(0.15f, 0.15f, 0.20f), 64));
        SetAnchors(backBtnG, new Vector2(0, 0), new Vector2(0, 1));
        SetOffsets(backBtnG, 16, 16, 130, -16);

        // Gold
        GameObject goldLabel = FindOrCreateChild(topBar.transform, "GoldLabel", () =>
            CreateTMP("GoldLabel", "Gold: 0", 52, topBar.transform));
        SetAnchors(goldLabel, new Vector2(0.20f, 0), new Vector2(0.40f, 1));
        SetOffsets(goldLabel, 10, 10, -10, -10);

        // Gems
        GameObject gemsLabel = FindOrCreateChild(topBar.transform, "GemsLabel", () =>
            CreateTMP("GemsLabel", "Gems: 0", 52, topBar.transform));
        SetAnchors(gemsLabel, new Vector2(0.40f, 0), new Vector2(0.60f, 1));
        SetOffsets(gemsLabel, 10, 10, -10, -10);

        // Stones
        GameObject stonesLabel = FindOrCreateChild(topBar.transform, "StonesLabel", () =>
            CreateTMP("StonesLabel", "Stones: 0", 52, topBar.transform));
        SetAnchors(stonesLabel, new Vector2(0.60f, 0), new Vector2(0.80f, 1));
        SetOffsets(stonesLabel, 10, 10, -10, -10);

        // ────────────────────────────────────────────────────────────
        //  FILTER BAR  — below top bar, 80px height
        // ────────────────────────────────────────────────────────────
        GameObject filterBar = FindOrCreateChild(canvasGo.transform, "FilterBar", () =>
            CreatePanel("FilterBar", canvasGo.transform, ColorFilterBar));
        SetAnchors(filterBar, new Vector2(0, 1), new Vector2(0.70f, 1));
        SetOffsets(filterBar, 0, -210, 0, -130);

        List<string> classOptionsList = new List<string> { "All" };
        classOptionsList.AddRange(System.Enum.GetNames(typeof(HeroClass)));
        string[] classOptions = classOptionsList.ToArray();
        TMP_Dropdown classDropdown = FindOrCreateComponent<TMP_Dropdown>(filterBar.transform, "ClassDropdown", () =>
        {
            var dd = CreateUIViaMenu<TMP_Dropdown>("GameObject/UI/Dropdown - TextMeshPro", "ClassDropdown", filterBar.transform);
            if (dd != null) { dd.options.Clear(); foreach (var o in classOptions) dd.options.Add(new TMP_Dropdown.OptionData(o)); dd.RefreshShownValue(); }
            return dd;
        });
        if (classDropdown != null)
        {
            SetAnchors(classDropdown.gameObject, new Vector2(0, 0.1f), new Vector2(0.28f, 0.9f));
            SetOffsets(classDropdown.gameObject, 12, 6, -6, -6);
            ApplyDarkDropdownStyle(classDropdown);
        }

        Toggle aliveToggle = FindOrCreateComponent<Toggle>(filterBar.transform, "AliveToggle", () =>
            CreateTMPToggle("AliveToggle", "Alive", filterBar.transform));
        if (aliveToggle != null)
        {
            SetAnchors(aliveToggle.gameObject, new Vector2(0.30f, 0.1f), new Vector2(0.48f, 0.9f));
            SetOffsets(aliveToggle.gameObject, 6, 6, -6, -6);
            FixToggleDark(aliveToggle);
        }

        TMP_Dropdown sortDropdown = FindOrCreateComponent<TMP_Dropdown>(filterBar.transform, "SortDropdown", () =>
        {
            var dd = CreateUIViaMenu<TMP_Dropdown>("GameObject/UI/Dropdown - TextMeshPro", "SortDropdown", filterBar.transform);
            if (dd != null) { dd.options.Clear(); var sOpts = new[] { "Morale", "Rarity", "Date", "Name" }; foreach (var o in sOpts) dd.options.Add(new TMP_Dropdown.OptionData(o)); dd.RefreshShownValue(); }
            return dd;
        });
        if (sortDropdown != null)
        {
            SetAnchors(sortDropdown.gameObject, new Vector2(0.50f, 0.1f), new Vector2(0.78f, 0.9f));
            SetOffsets(sortDropdown.gameObject, 6, 6, -6, -6);
            ApplyDarkDropdownStyle(sortDropdown);
        }

        GameObject heroCount = FindOrCreateChild(filterBar.transform, "HeroCount", () =>
            CreateTMP("HeroCount", "Heroes: 0", 32, filterBar.transform));
        SetAnchors(heroCount, new Vector2(0.80f, 0), new Vector2(1, 1));
        SetOffsets(heroCount, 8, 4, -8, -4);

        // ────────────────────────────────────────────────────────────
        //  HERO GRID  — 70% width
        //  CLEAN: destroy orphaned duplicate content from previous runs
        // ────────────────────────────────────────────────────────────
        // Remove stale grid scroll rects and their children
        var existingGrids = new List<GameObject>();
        for (int i = 0; i < canvasGo.transform.childCount; i++)
        {
            GameObject c = canvasGo.transform.GetChild(i).gameObject;
            if (c.name.StartsWith("HeroGridScrollRect"))
            {
                // Only keep the FIRST one; destroy the rest
                if (existingGrids.Count == 0)
                {
                    // Clean its internals
                    var toDelete = new List<GameObject>();
                    for (int j = 0; j < c.transform.childCount; j++)
                    {
                        GameObject child = c.transform.GetChild(j).gameObject;
                        if (child.name == "Viewport" || child.name == "Scrollbar Horizontal" || child.name == "Scrollbar Vertical")
                        {
                            // Check if this is a duplicate by counting children with same name
                            int count = 0;
                            for (int k = 0; k < c.transform.childCount; k++)
                                if (c.transform.GetChild(k).name == child.name) count++;
                            if (count > 1) toDelete.Add(child);
                        }
                    }
                    foreach (var d in toDelete) Object.DestroyImmediate(d);
                    existingGrids.Add(c);
                }
                else
                {
                    Object.DestroyImmediate(c);
                }
            }
        }

        GameObject scrollRectGo = FindOrCreateChild(canvasGo.transform, "HeroGridScrollRect", () =>
        {
            var go = new GameObject("HeroGridScrollRect", typeof(RectTransform), typeof(ScrollRect));
            return go;
        });

        // Clear old viewport/content duplicates inside
        var killList = new List<GameObject>();
        foreach (Transform child in scrollRectGo.transform)
            if (child.name == "Viewport" || child.name == "Scrollbar Vertical" || child.name == "Scrollbar Horizontal")
                killList.Add(child.gameObject);
        foreach (var k in killList) Object.DestroyImmediate(k);
        RectTransform srRt = scrollRectGo.GetComponent<RectTransform>();
        srRt.anchorMin = new Vector2(0, 0);
        srRt.anchorMax = new Vector2(0.70f, 1);
        srRt.offsetMin = new Vector2(0, 0);
        srRt.offsetMax = new Vector2(0, -210);

        ScrollRect scrollRect = scrollRectGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 60f;

        // Viewport — no image, just mask to clip content
        GameObject viewportG = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportG.transform.SetParent(scrollRectGo.transform, false);
        RectTransform vpRt = viewportG.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        Image vpImg = viewportG.GetComponent<Image>();
        vpImg.color = Color.clear;
        vpImg.raycastTarget = false;
        Mask vpMask = viewportG.GetComponent<Mask>();
        vpMask.showMaskGraphic = false;
        scrollRect.viewport = vpRt;

        // Content with GridLayoutGroup
        GameObject contentG = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        contentG.transform.SetParent(viewportG.transform, false);
        RectTransform contentRt = contentG.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = Vector2.zero;
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        GridLayoutGroup grid = contentG.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(360, 420);
        grid.spacing = new Vector2(16, 16);
        grid.padding = new RectOffset(16, 16, 16, 16);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        ContentSizeFitter fitter = contentG.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scrollRect.content = contentRt;

        // No horizontal scrollbar
        scrollRect.horizontalScrollbar = null;
        scrollRect.verticalScrollbar = null;

        // ────────────────────────────────────────────────────────────
        //  DETAIL PANEL  — 30% width, right side
        // ────────────────────────────────────────────────────────────
        GameObject detailPanel = FindOrCreateChild(canvasGo.transform, "DetailPanel", () =>
            CreatePanel("DetailPanel", canvasGo.transform, ColorDetailBg));
        SetAnchors(detailPanel, new Vector2(0.70f, 0), new Vector2(1, 1));
        SetOffsets(detailPanel, 0, 0, 0, 0);
        detailPanel.SetActive(false);

        // Close button
        GameObject closeBtnG = FindOrCreateChild(detailPanel.transform, "CloseBtn", () =>
            CreateButton("CloseBtn", "X", detailPanel.transform, new Color(0.15f, 0.15f, 0.20f), 48));
        SetAnchors(closeBtnG, new Vector2(1, 1), new Vector2(1, 1));
        SetOffsets(closeBtnG, -100, -20, -100, -20);

        // Portrait — large, top of detail panel
        GameObject portraitG = FindOrCreateChild(detailPanel.transform, "Portrait", () =>
            CreatePanel("Portrait", detailPanel.transform, new Color(0.10f, 0.10f, 0.16f)));
        SetAnchors(portraitG, new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.92f));

        // Name
        GameObject detailNameG = FindOrCreateChild(detailPanel.transform, "Name", () =>
        {
            var go = CreateTMP("Name", "HERO NAME", 56, detailPanel.transform);
            go.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            go.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            return go;
        });
        SetAnchors(detailNameG, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.50f));

        // Class
        GameObject detailClassG = FindOrCreateChild(detailPanel.transform, "Class", () =>
            CreateTMP("Class", "CLASS | BADGE", 32, detailPanel.transform));
        SetAnchors(detailClassG, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.42f));
        detailClassG.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Stars
        GameObject detailStarsG = FindOrCreateChild(detailPanel.transform, "Stars", () =>
            CreateTMP("Stars", "[*][*]", 36, detailPanel.transform));
        SetAnchors(detailStarsG, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.36f));
        detailStarsG.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Trait badge
        GameObject traitBadgeG = FindOrCreateChild(detailPanel.transform, "TraitBadgeBg", () =>
            CreatePanel("TraitBadgeBg", detailPanel.transform, new Color(0.30f, 0.30f, 0.50f)));
        SetAnchors(traitBadgeG, new Vector2(0.08f, 0.26f), new Vector2(0.45f, 0.30f));
        GameObject traitTextG = FindOrCreateChild(traitBadgeG.transform, "TraitText", () =>
            CreateTMP("TraitText", "BRAVE", 22, traitBadgeG.transform));
        Stretch(traitTextG);

        // Morale
        GameObject moraleLabelG = FindOrCreateChild(detailPanel.transform, "MoraleLabel", () =>
            CreateTMP("MoraleLabel", "MORALE", 24, detailPanel.transform));
        SetAnchors(moraleLabelG, new Vector2(0.08f, 0.22f), new Vector2(0.36f, 0.26f));
        moraleLabelG.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        GameObject moraleSliderG = new GameObject("MoraleSlider", typeof(RectTransform), typeof(Slider));
        moraleSliderG.transform.SetParent(detailPanel.transform, false);
        RectTransform msRt = moraleSliderG.GetComponent<RectTransform>();
        msRt.anchorMin = new Vector2(0.38f, 0.22f);
        msRt.anchorMax = new Vector2(0.92f, 0.26f);
        msRt.offsetMin = Vector2.zero;
        msRt.offsetMax = Vector2.zero;
        Slider moraleSlider = moraleSliderG.GetComponent<Slider>();
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)).transform;
        fillArea.SetParent(moraleSliderG.transform, false);
        fillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fillArea.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).transform;
        fill.SetParent(fillArea, false);
        fill.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fill.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        fill.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        fill.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = ColorGreen;
        moraleSlider.fillRect = fill.GetComponent<RectTransform>();
        var sliderBg = new GameObject("Background", typeof(RectTransform), typeof(Image)).transform;
        sliderBg.SetParent(moraleSliderG.transform, false);
        sliderBg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        sliderBg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        sliderBg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        sliderBg.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        sliderBg.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f);
        moraleSlider.handleRect = null;

        // Equipment
        GameObject equipG = FindOrCreateChild(detailPanel.transform, "Equipment", () =>
        {
            var go = CreateTMP("Equipment", "Weapon: None\nArmor: None\nAccessory: None\nLock: Unlocked", 24, detailPanel.transform);
            go.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            go.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            return go;
        });
        SetAnchors(equipG, new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.21f));

        // Potential text
        GameObject potentialG = FindOrCreateChild(detailPanel.transform, "Potential", () =>
        {
            var go = CreateTMP("Potential", "\"Potential description\"", 20, detailPanel.transform);
            go.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            go.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            return go;
        });
        SetAnchors(potentialG, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.10f));

        // Promote button — full width, 90px
        GameObject promoteBtnG = FindOrCreateChild(detailPanel.transform, "PromoteBtn", () =>
            CreateButton("PromoteBtn", "PROMOTE", detailPanel.transform, ColorPromote, 40));
        SetAnchors(promoteBtnG, new Vector2(0.06f, 0.01f), new Vector2(0.94f, 0.04f));
        promoteBtnG.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Promote cost
        GameObject promoteCostG = FindOrCreateChild(detailPanel.transform, "Cost", () =>
            CreateTMP("Cost", "Cost: 0 Stones", 22, detailPanel.transform));
        SetAnchors(promoteCostG, new Vector2(0.06f, 0), new Vector2(0.94f, 0.01f));
        promoteCostG.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Synthesize button — full width, 90px
        GameObject synthBtnG = FindOrCreateChild(detailPanel.transform, "SynthesizeBtn", () =>
            CreateButton("SynthesizeBtn", "SYNTHESIZE", detailPanel.transform, ColorRed, 40));
        SetAnchors(synthBtnG, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.09f));
        synthBtnG.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // ── Synth Warning Panel ────────────────────────────────────────────
        GameObject synthPanel = FindOrCreateChild(canvasGo.transform, "SynthWarningPanel", () =>
            CreatePanel("SynthWarningPanel", canvasGo.transform, new Color(0, 0, 0, 0.9f)));
        SetAnchors(synthPanel, new Vector2(0, 0), new Vector2(1, 1));
        synthPanel.SetActive(false);

        GameObject warningText = FindOrCreateChild(synthPanel.transform, "WarningText", () =>
        {
            var go = CreateTMP("WarningText", "WARNING: Synthesis consumes the hero permanently. Proceed?", 48, synthPanel.transform);
            go.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            return go;
        });
        SetAnchors(warningText, new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.72f));

        GameObject confirmBtnG = FindOrCreateChild(synthPanel.transform, "ConfirmBtn", () =>
            CreateButton("ConfirmBtn", "CONFIRM", synthPanel.transform, ColorRed, 44));
        SetAnchors(confirmBtnG, new Vector2(0.25f, 0.35f), new Vector2(0.45f, 0.45f));

        GameObject cancelBtnG = FindOrCreateChild(synthPanel.transform, "CancelBtn", () =>
            CreateButton("CancelBtn", "CANCEL", synthPanel.transform, new Color(0.35f, 0.35f, 0.35f), 44));
        SetAnchors(cancelBtnG, new Vector2(0.55f, 0.35f), new Vector2(0.75f, 0.45f));

        // ── Wire RosterView ────────────────────────────────────────────────
        SerializedObject so = new SerializedObject(rosterView);

        so.FindProperty("goldLabel").objectReferenceValue = goldLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("gemsLabel").objectReferenceValue = gemsLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("stonesLabel").objectReferenceValue = stonesLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("heroCountLabel").objectReferenceValue = heroCount.GetComponent<TextMeshProUGUI>();
        so.FindProperty("backBtn").objectReferenceValue = backBtnG.GetComponent<Button>();
        so.FindProperty("classDropdown").objectReferenceValue = classDropdown;
        so.FindProperty("aliveDeadToggle").objectReferenceValue = aliveToggle;
        so.FindProperty("sortDropdown").objectReferenceValue = sortDropdown;
        so.FindProperty("gridContent").objectReferenceValue = contentRt;
        so.FindProperty("detailPanel").objectReferenceValue = detailPanel;
        so.FindProperty("detailPanelRT").objectReferenceValue = detailPanel.GetComponent<RectTransform>();
        so.FindProperty("detailPortrait").objectReferenceValue = portraitG.GetComponent<Image>();
        so.FindProperty("detailName").objectReferenceValue = detailNameG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailClass").objectReferenceValue = detailClassG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailTraitBadgeText").objectReferenceValue = traitTextG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailTraitBadgeBg").objectReferenceValue = traitBadgeG.GetComponent<Image>();
        so.FindProperty("detailMoraleBar").objectReferenceValue = moraleSlider;
        so.FindProperty("detailPotentialText").objectReferenceValue = potentialG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailEquipmentText").objectReferenceValue = equipG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailStars").objectReferenceValue = detailStarsG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("promoteBtn").objectReferenceValue = promoteBtnG.GetComponent<Button>();
        so.FindProperty("promoteCostText").objectReferenceValue = promoteCostG.GetComponent<TextMeshProUGUI>();
        so.FindProperty("synthesizeBtn").objectReferenceValue = synthBtnG.GetComponent<Button>();
        so.FindProperty("detailCloseBtn").objectReferenceValue = closeBtnG.GetComponent<Button>();
        so.FindProperty("synthWarningPanel").objectReferenceValue = synthPanel;
        so.FindProperty("synthWarningText").objectReferenceValue = warningText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("synthConfirmBtn").objectReferenceValue = confirmBtnG.GetComponent<Button>();
        so.FindProperty("synthCancelBtn").objectReferenceValue = cancelBtnG.GetComponent<Button>();

        GameObject heroCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/RosterHeroCardPrefab.prefab");
        if (heroCardPrefab != null)
            so.FindProperty("heroCardPrefab").objectReferenceValue = heroCardPrefab;

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(canvasGo);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupRosterUI] Roster UI complete (2400x1080 landscape).");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    private static GameObject FindOrCreateChild(Transform parent, string name, System.Func<GameObject> factory)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
                return parent.GetChild(i).gameObject;
        }
        GameObject go = factory();
        if (go != null) go.name = name;
        return go;
    }

    private static T FindOrCreateComponent<T>(Transform parent, string name, System.Func<T> factory) where T : Component
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
            {
                T comp = parent.GetChild(i).GetComponent<T>();
                if (comp != null) return comp;
            }
        }
        return factory();
    }

    private static TMP_Dropdown CreateTMPDropdown(string name, Transform parent, string[] options)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var dd = go.AddComponent<TMP_Dropdown>();
        // Add required child label
        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lRt = labelGo.GetComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = Vector2.one;
        lRt.offsetMin = new Vector2(8, 2);
        lRt.offsetMax = new Vector2(-8, -2);
        dd.captionText = labelGo.GetComponent<TextMeshProUGUI>();

        // Template (for dropdown list)
        var templateGo = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask), typeof(Canvas));
        templateGo.transform.SetParent(go.transform, false);
        templateGo.SetActive(false);
        RectTransform tRt = templateGo.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0);
        tRt.anchorMax = new Vector2(1, 0);
        tRt.pivot = new Vector2(0.5f, 1);
        tRt.sizeDelta = new Vector2(0, 360);
        tRt.offsetMin = new Vector2(0, 0);
        tRt.offsetMax = new Vector2(0, 360);
        templateGo.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f);
        dd.template = templateGo.GetComponent<RectTransform>();

        // Viewport inside template
        var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(templateGo.transform, false);
        RectTransform vpRt2 = vpGo.GetComponent<RectTransform>();
        vpRt2.anchorMin = Vector2.zero;
        vpRt2.anchorMax = Vector2.one;
        vpRt2.offsetMin = Vector2.zero;
        vpRt2.offsetMax = Vector2.zero;
        vpGo.GetComponent<Image>().color = Color.clear;
        var scrollRect2 = templateGo.GetComponent<ScrollRect>();
        scrollRect2.viewport = vpRt2;
        scrollRect2.horizontal = false;
        scrollRect2.vertical = true;

        // Content inside viewport
        var contentGo2 = new GameObject("Content", typeof(RectTransform), typeof(ToggleGroup));
        contentGo2.transform.SetParent(vpGo.transform, false);
        RectTransform cRt2 = contentGo2.GetComponent<RectTransform>();
        cRt2.anchorMin = new Vector2(0, 1);
        cRt2.anchorMax = new Vector2(1, 1);
        cRt2.pivot = new Vector2(0.5f, 1);
        cRt2.offsetMin = Vector2.zero;
        cRt2.offsetMax = Vector2.zero;
        scrollRect2.content = cRt2;

        dd.options.Clear();
        foreach (var opt in options)
            dd.options.Add(new TMP_Dropdown.OptionData(opt));

        // Create items
        foreach (var opt in options)
        {
            var itemGo = new GameObject("Item " + opt, typeof(RectTransform), typeof(Toggle), typeof(Image));
            itemGo.transform.SetParent(contentGo2.transform, false);
            RectTransform iRt = itemGo.GetComponent<RectTransform>();
            iRt.sizeDelta = new Vector2(0, 40);
            iRt.anchorMin = new Vector2(0, 1);
            iRt.anchorMax = new Vector2(1, 1);
            iRt.pivot = new Vector2(0.5f, 1);
            var itemText = new GameObject("ItemLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            itemText.transform.SetParent(itemGo.transform, false);
            RectTransform itRt = itemText.GetComponent<RectTransform>();
            itRt.anchorMin = Vector2.zero;
            itRt.anchorMax = Vector2.one;
            itRt.offsetMin = new Vector2(8, 2);
            itRt.offsetMax = new Vector2(-8, -2);
            var tmp = itemText.GetComponent<TextMeshProUGUI>();
            tmp.text = opt;
            tmp.color = Color.white;
            tmp.fontSize = 28;
            var toggle = itemGo.GetComponent<Toggle>();
            toggle.targetGraphic = itemGo.GetComponent<Image>();
            toggle.graphic = itemText.GetComponent<TextMeshProUGUI>();
            toggle.group = contentGo2.GetComponent<ToggleGroup>();
            dd.options.Add(new TMP_Dropdown.OptionData(opt));
        }

        dd.RefreshShownValue();
        return dd;
    }

    private static Toggle CreateTMPToggle(string name, string label, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Toggle), typeof(Image));
        go.transform.SetParent(parent, false);
        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.18f);

        var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGo.transform.SetParent(go.transform, false);
        RectTransform ckRt = checkGo.GetComponent<RectTransform>();
        ckRt.anchorMin = new Vector2(0.15f, 0.15f);
        ckRt.anchorMax = new Vector2(0.40f, 0.85f);
        ckRt.offsetMin = Vector2.zero;
        ckRt.offsetMax = Vector2.zero;
        var checkImg = checkGo.GetComponent<Image>();
        checkImg.color = ColorGreen;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        RectTransform lRt2 = labelGo.GetComponent<RectTransform>();
        lRt2.anchorMin = new Vector2(0.45f, 0);
        lRt2.anchorMax = new Vector2(1, 1);
        lRt2.offsetMin = Vector2.zero;
        lRt2.offsetMax = Vector2.zero;
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;

        var toggle = go.GetComponent<Toggle>();
        toggle.graphic = checkImg;
        toggle.targetGraphic = bg;
        toggle.isOn = true;
        return toggle;
    }

    private static void ApplyDarkDropdownStyle(TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;
        var bg = dropdown.transform.Find("Background");
        if (bg != null && bg.GetComponent<Image>() != null)
            bg.GetComponent<Image>().color = new Color(0.10f, 0.10f, 0.15f);
    }

    private static void FixToggleDark(Toggle toggle)
    {
        if (toggle == null) return;
        var bg = toggle.GetComponent<Image>();
        if (bg != null) bg.color = new Color(0.12f, 0.12f, 0.18f);
        var check = toggle.transform.Find("Checkmark");
        if (check != null)
        {
            var img = check.GetComponent<Image>();
            if (img != null) img.color = ColorGreen;
        }
    }

    private static T CreateUIViaMenu<T>(string menuPath, string name, Transform parent) where T : Component
    {
        var prevSelection = Selection.activeGameObject;
        Selection.activeGameObject = parent.gameObject;

        var before = new HashSet<T>(Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        EditorApplication.ExecuteMenuItem(menuPath);

        T result = null;
        GameObject resultGo = null;

        if (Selection.activeGameObject != null && Selection.activeGameObject != parent.gameObject)
        {
            result = Selection.activeGameObject.GetComponent<T>();
            if (result != null) resultGo = Selection.activeGameObject;
        }

        if (result == null)
        {
            var after = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var comp in after)
            {
                if (!before.Contains(comp))
                {
                    result = comp;
                    resultGo = comp.gameObject;
                    break;
                }
            }
        }

        if (resultGo != null)
        {
            resultGo.name = name;
            if (resultGo.transform.parent != parent)
                resultGo.transform.SetParent(parent, false);
        }
        else Debug.LogWarning($"[SetupRosterUI] Failed to create {typeof(T).Name} via menu: {menuPath}");

        Selection.activeGameObject = prevSelection;
        return result;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static GameObject CreateTMP(string name, string text, float fontSize, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    private static GameObject CreateButton(string name, string label, Transform parent, Color color, float fontSize)
    {
        GameObject go = CreatePanel(name, parent, color);
        go.AddComponent<Button>();
        GameObject textGo = CreateTMP("Text", label, fontSize, go.transform);
        Stretch(textGo);
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

    private static void SetOffsets(GameObject go, float left, float top, float right, float bottom)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(right, top);
    }
}
