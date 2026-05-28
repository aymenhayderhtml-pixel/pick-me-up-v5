using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupFacilityUI
{
    [MenuItem("Tools/Pick Me Up/Setup Facility UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        Canvas canvas = GetOrCreateCanvas("FacilityCanvas");
        CleanupDuplicateCanvases("FacilityCanvas", canvas);

        GameObject topBar = CreatePanel("TopBar", canvas.transform, new Color(0.07f, 0.08f, 0.12f, 0.95f));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        GameObject backButton = CreateButton("BackButton", "< Back", topBar.transform, new Color(0.18f, 0.18f, 0.24f, 1f), 22);
        SetAnchors(backButton, new Vector2(0.02f, 0.15f), new Vector2(0.18f, 0.85f));

        GameObject titleText = CreateTMP("TitleText", "FACILITIES", 28, topBar.transform);
        SetAnchors(titleText, new Vector2(0.25f, 0.15f), new Vector2(0.55f, 0.85f));

        GameObject goldText = CreateTMP("GoldText", "0", 24, topBar.transform);
        SetAnchors(goldText, new Vector2(0.58f, 0.15f), new Vector2(0.78f, 0.85f));

        GameObject gemsText = CreateTMP("GemsText", "0", 24, topBar.transform);
        SetAnchors(gemsText, new Vector2(0.78f, 0.15f), new Vector2(0.98f, 0.85f));

        GameObject fragmentsText = CreateTMP("FragmentsText", "0", 20, topBar.transform);
        SetAnchors(fragmentsText, new Vector2(0.58f, 0.00f), new Vector2(0.78f, 0.12f));

        GameObject moraleText = CreateTMP("MoraleText", "Morale 50/100", 20, topBar.transform);
        SetAnchors(moraleText, new Vector2(0.78f, 0.00f), new Vector2(0.98f, 0.12f));

        GameObject tabBar = CreatePanel("TabBar", canvas.transform, Color.clear);
        SetAnchors(tabBar, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.87f));

        GameObject commandTabButton = CreateButton("CommandTabButton", "COMMAND", tabBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 18);
        SetAnchors(commandTabButton, new Vector2(0f, 0f), new Vector2(0.48f, 1f));

        GameObject shadowTabButton = CreateButton("ShadowTabButton", "SHADOW", tabBar.transform, new Color(0.25f, 0.15f, 0.18f, 1f), 18);
        SetAnchors(shadowTabButton, new Vector2(0.52f, 0f), new Vector2(1f, 1f));

        GameObject list = CreatePanel("FacilityGrid", canvas.transform, Color.clear);
        SetAnchors(list, new Vector2(0.02f, 0.20f), new Vector2(0.98f, 0.79f));
        ConfigureGrid(list);

        GameObject detailPanel = CreatePanel("DetailPanel", canvas.transform, new Color(0f, 0f, 0f, 0.85f));
        SetAnchors(detailPanel, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.72f));
        detailPanel.SetActive(false);

        GameObject detailName = CreateTMP("DetailName", "Facility", 28, detailPanel.transform);
        SetAnchors(detailName, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.95f));

        GameObject detailRole = CreateTMP("DetailRole", "Role", 18, detailPanel.transform);
        SetAnchors(detailRole, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.81f));

        GameObject detailDescription = CreateTMP("DetailDescription", "Description", 18, detailPanel.transform);
        SetAnchors(detailDescription, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.72f));

        GameObject detailLevel = CreateTMP("DetailLevel", "Level", 20, detailPanel.transform);
        SetAnchors(detailLevel, new Vector2(0.08f, 0.46f), new Vector2(0.92f, 0.56f));

        GameObject detailBenefit = CreateTMP("DetailBenefit", "Benefit", 20, detailPanel.transform);
        SetAnchors(detailBenefit, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.44f));

        GameObject detailEmotion = CreateTMP("DetailEmotion", "Emotion", 16, detailPanel.transform);
        SetAnchors(detailEmotion, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.32f));

        GameObject detailCost = CreateTMP("DetailCost", "Cost", 18, detailPanel.transform);
        SetAnchors(detailCost, new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.22f));

        GameObject detailWarning = CreateTMP("DetailWarning", "Warning", 16, detailPanel.transform);
        SetAnchors(detailWarning, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.14f));

        GameObject upgradeButton = CreateButton("UpgradeButton", "COMMIT UPGRADE", detailPanel.transform, new Color(0.18f, 0.55f, 0.25f, 1f), 22);
        SetAnchors(upgradeButton, new Vector2(0.25f, 0.00f), new Vector2(0.75f, 0.06f));

        GameObject dockPanel = CreatePanel("DockPanel", detailPanel.transform, new Color(0.08f, 0.06f, 0.10f, 0.92f));
        SetAnchors(dockPanel, new Vector2(0.08f, 0.00f), new Vector2(0.92f, 0.26f));
        dockPanel.SetActive(false);

        GameObject dockStatus = CreateTMP("DockStatus", "Queued Sortie: RECON", 16, dockPanel.transform);
        SetAnchors(dockStatus, new Vector2(0.04f, 0.74f), new Vector2(0.96f, 0.96f));

        GameObject dockButtons = CreatePanel("DockButtons", dockPanel.transform, Color.clear);
        SetAnchors(dockButtons, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.70f));
        ConfigureDockRow(dockButtons);

        GameObject reconButton = CreateButton("ReconButton", "RECON", dockButtons.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 16);
        GameObject supplyButton = CreateButton("SupplyButton", "SUPPLY", dockButtons.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 16);
        GameObject extractionButton = CreateButton("ExtractionButton", "EXTRACTION", dockButtons.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 16);
        GameObject launchSortieButton = CreateButton("LaunchSortieButton", "LAUNCH", dockPanel.transform, new Color(0.55f, 0.2f, 0.2f, 1f), 18);
        SetAnchors(launchSortieButton, new Vector2(0.25f, 0.00f), new Vector2(0.75f, 0.08f));

        FacilityView view = canvas.gameObject.AddComponent<FacilityView>();
        SerializedObject so = new SerializedObject(view);
        so.FindProperty("backButton").objectReferenceValue = backButton.GetComponent<Button>();
        so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("goldText").objectReferenceValue = goldText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("gemsText").objectReferenceValue = gemsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("memorialFragmentsText").objectReferenceValue = fragmentsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("moraleText").objectReferenceValue = moraleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("facilityGridContent").objectReferenceValue = list.GetComponent<RectTransform>();
        so.FindProperty("commandTabButton").objectReferenceValue = commandTabButton.GetComponent<Button>();
        so.FindProperty("shadowTabButton").objectReferenceValue = shadowTabButton.GetComponent<Button>();
        so.FindProperty("detailPanel").objectReferenceValue = detailPanel;
        so.FindProperty("detailNameText").objectReferenceValue = detailName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailRoleText").objectReferenceValue = detailRole.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailDescriptionText").objectReferenceValue = detailDescription.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailLevelText").objectReferenceValue = detailLevel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailBenefitText").objectReferenceValue = detailBenefit.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailEmotionText").objectReferenceValue = detailEmotion.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailCostText").objectReferenceValue = detailCost.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailWarningText").objectReferenceValue = detailWarning.GetComponent<TextMeshProUGUI>();
        so.FindProperty("upgradeButton").objectReferenceValue = upgradeButton.GetComponent<Button>();
        so.FindProperty("dockPanel").objectReferenceValue = dockPanel;
        so.FindProperty("dockStatusText").objectReferenceValue = dockStatus.GetComponent<TextMeshProUGUI>();
        so.FindProperty("reconButton").objectReferenceValue = reconButton.GetComponent<Button>();
        so.FindProperty("supplyButton").objectReferenceValue = supplyButton.GetComponent<Button>();
        so.FindProperty("extractionButton").objectReferenceValue = extractionButton.GetComponent<Button>();
        so.FindProperty("launchSortieButton").objectReferenceValue = launchSortieButton.GetComponent<Button>();
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static Canvas GetOrCreateCanvas(string name)
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas existing in canvases)
        {
            if (existing != null && existing.gameObject.name == name)
            {
                return existing;
            }
        }

        return CreateCanvas(name);
    }

    private static void CleanupDuplicateCanvases(string name, Canvas keep)
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas existing in canvases)
        {
            if (existing == null || existing == keep)
            {
                continue;
            }

            if (existing.gameObject.name == name)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2340);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
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
        GameObject text = CreateTMP("Text", label, fontSize, go.transform);
        Stretch(text);
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

    private static void ConfigureGrid(GameObject panel)
    {
        GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = panel.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = new Vector2(320, 150);
        grid.spacing = new Vector2(18, 18);
        grid.padding = new RectOffset(16, 16, 16, 16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = panel.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private static void ConfigureDockRow(GameObject panel)
    {
        HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = panel.AddComponent<HorizontalLayoutGroup>();
        }

        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.spacing = 10f;
        layout.padding = new RectOffset(4, 4, 4, 4);
    }
}
