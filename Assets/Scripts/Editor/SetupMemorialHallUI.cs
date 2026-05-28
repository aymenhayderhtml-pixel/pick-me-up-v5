using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupMemorialHallUI
{
    [MenuItem("Tools/Pick Me Up/Setup Memorial Hall UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        Canvas canvas = CreateCanvas("MemorialHallCanvas");

        GameObject topBar = CreatePanel("TopBar", canvas.transform, new Color(0.07f, 0.08f, 0.12f, 0.95f));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        GameObject backButton = CreateButton("BackButton", "< Back", topBar.transform, new Color(0.18f, 0.18f, 0.24f, 1f), 22);
        SetAnchors(backButton, new Vector2(0.02f, 0.15f), new Vector2(0.16f, 0.85f));

        GameObject titleText = CreateTMP("TitleText", "MEMORIAL HALL", 28, topBar.transform);
        SetAnchors(titleText, new Vector2(0.2f, 0.15f), new Vector2(0.6f, 0.85f));

        GameObject completionText = CreateTMP("CompletionText", "0/0", 24, topBar.transform);
        SetAnchors(completionText, new Vector2(0.72f, 0.15f), new Vector2(0.98f, 0.85f));

        GameObject summaryBar = CreatePanel("SummaryBar", canvas.transform, new Color(0.05f, 0.05f, 0.08f, 0.85f));
        SetAnchors(summaryBar, new Vector2(0.02f, 0.73f), new Vector2(0.98f, 0.79f));

        GameObject echoSummaryText = CreateTMP("EchoSummaryText", "Echoes: 0", 18, summaryBar.transform);
        SetAnchors(echoSummaryText, new Vector2(0.02f, 0.1f), new Vector2(0.40f, 0.9f));

        GameObject legacySummaryText = CreateTMP("LegacySummaryText", "Legacy is dormant.", 18, summaryBar.transform);
        SetAnchors(legacySummaryText, new Vector2(0.42f, 0.1f), new Vector2(0.98f, 0.9f));

        GameObject filterBar = CreatePanel("FilterBar", canvas.transform, Color.clear);
        SetAnchors(filterBar, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.87f));

        GameObject allButton = CreateButton("AllButton", "ALL", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 18);
        SetAnchors(allButton, new Vector2(0f, 0f), new Vector2(0.32f, 1f));

        GameObject discoveredButton = CreateButton("DiscoveredButton", "FOUND", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 18);
        SetAnchors(discoveredButton, new Vector2(0.34f, 0f), new Vector2(0.66f, 1f));

        GameObject undiscoveredButton = CreateButton("UndiscoveredButton", "HIDDEN", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 18);
        SetAnchors(undiscoveredButton, new Vector2(0.68f, 0f), new Vector2(1f, 1f));

        GameObject grid = CreatePanel("HeroGridContent", canvas.transform, Color.clear);
        SetAnchors(grid, new Vector2(0.02f, 0.22f), new Vector2(0.52f, 0.79f));
        ConfigureGrid(grid);

        GameObject detailPanel = CreatePanel("DetailPanel", canvas.transform, new Color(0f, 0f, 0f, 0.88f));
        SetAnchors(detailPanel, new Vector2(0.55f, 0.22f), new Vector2(0.98f, 0.79f));
        detailPanel.SetActive(false);

        GameObject portrait = CreatePanel("Portrait", detailPanel.transform, new Color(1f, 1f, 1f, 0.08f));
        SetAnchors(portrait, new Vector2(0.08f, 0.58f), new Vector2(0.40f, 0.92f));

        GameObject detailName = CreateTMP("DetailNameText", "Hero", 26, detailPanel.transform);
        SetAnchors(detailName, new Vector2(0.45f, 0.82f), new Vector2(0.95f, 0.94f));

        GameObject detailClass = CreateTMP("DetailClassText", "Class", 18, detailPanel.transform);
        SetAnchors(detailClass, new Vector2(0.45f, 0.74f), new Vector2(0.95f, 0.80f));

        GameObject detailStars = CreateTMP("DetailStarsText", "*****", 18, detailPanel.transform);
        SetAnchors(detailStars, new Vector2(0.45f, 0.66f), new Vector2(0.95f, 0.72f));

        GameObject detailLore = CreateTMP("DetailLoreText", "Lore", 16, detailPanel.transform);
        SetAnchors(detailLore, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.56f));

        GameObject detailCount = CreateTMP("DetailCountText", "Acquired", 16, detailPanel.transform);
        SetAnchors(detailCount, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.28f));

        GameObject detailEchoText = CreateTMP("DetailEchoText", "Echo Resonance: DORMANT", 16, detailPanel.transform);
        SetAnchors(detailEchoText, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.22f));

        GameObject detailLegacyText = CreateTMP("DetailLegacyText", "No memorial record yet.", 16, detailPanel.transform);
        SetAnchors(detailLegacyText, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.13f));

        GameObject detailCloseButton = CreateButton("DetailCloseButton", "CLOSE", detailPanel.transform, new Color(0.18f, 0.18f, 0.24f, 1f), 18);
        SetAnchors(detailCloseButton, new Vector2(0.76f, 0.90f), new Vector2(0.95f, 0.98f));

        System.Type viewType = System.Type.GetType("MemorialHallView, Assembly-CSharp");
        Component view = canvas.gameObject.AddComponent(viewType);
        SerializedObject so = new SerializedObject(view);
        so.FindProperty("backButton").objectReferenceValue = backButton.GetComponent<Button>();
        so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("completionText").objectReferenceValue = completionText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("echoSummaryText").objectReferenceValue = echoSummaryText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("legacySummaryText").objectReferenceValue = legacySummaryText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("allButton").objectReferenceValue = allButton.GetComponent<Button>();
        so.FindProperty("discoveredButton").objectReferenceValue = discoveredButton.GetComponent<Button>();
        so.FindProperty("undiscoveredButton").objectReferenceValue = undiscoveredButton.GetComponent<Button>();
        so.FindProperty("heroGridContent").objectReferenceValue = grid.GetComponent<RectTransform>();
        so.FindProperty("detailPanel").objectReferenceValue = detailPanel;
        so.FindProperty("detailPortrait").objectReferenceValue = portrait.GetComponent<Image>();
        so.FindProperty("detailNameText").objectReferenceValue = detailName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailClassText").objectReferenceValue = detailClass.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailStarsText").objectReferenceValue = detailStars.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailLoreText").objectReferenceValue = detailLore.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailCountText").objectReferenceValue = detailCount.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailEchoText").objectReferenceValue = detailEchoText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailLegacyText").objectReferenceValue = detailLegacyText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailCloseButton").objectReferenceValue = detailCloseButton.GetComponent<Button>();
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
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

        grid.cellSize = new Vector2(160, 220);
        grid.spacing = new Vector2(16, 16);
        grid.padding = new RectOffset(12, 12, 12, 12);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = panel.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }
}
