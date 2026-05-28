using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupDungeonUI
{
    [MenuItem("Tools/Pick Me Up/Setup Dungeon UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        Canvas canvas = CreateCanvas("DungeonCanvas");

        GameObject topBar = CreatePanel("TopBar", canvas.transform, new Color(0.07f, 0.08f, 0.12f, 0.95f));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        GameObject backButton = CreateButton("BackButton", "< Back", topBar.transform, new Color(0.18f, 0.18f, 0.24f, 1f), 22);
        SetAnchors(backButton, new Vector2(0.02f, 0.15f), new Vector2(0.18f, 0.85f));

        GameObject titleText = CreateTMP("TitleText", "DUNGEONS", 28, topBar.transform);
        SetAnchors(titleText, new Vector2(0.25f, 0.15f), new Vector2(0.62f, 0.85f));

        GameObject staminaText = CreateTMP("StaminaText", "100/100", 26, topBar.transform);
        SetAnchors(staminaText, new Vector2(0.74f, 0.15f), new Vector2(0.98f, 0.85f));

        GameObject selectedDungeonText = CreateTMP("SelectedDungeonText", "SELECT A DUNGEON", 24, canvas.transform);
        SetAnchors(selectedDungeonText, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.88f));

        GameObject scheduleText = CreateTMP("ScheduleText", "Weekly Window: Always", 20, canvas.transform);
        SetAnchors(scheduleText, new Vector2(0.02f, 0.74f), new Vector2(0.98f, 0.80f));

        GameObject todayScheduleText = CreateTMP("TodayScheduleText", "TODAY: OPEN", 20, canvas.transform);
        SetAnchors(todayScheduleText, new Vector2(0.02f, 0.69f), new Vector2(0.98f, 0.75f));

        GameObject tacticalBar = CreatePanel("TacticalBar", canvas.transform, new Color(0.05f, 0.05f, 0.08f, 0.88f));
        SetAnchors(tacticalBar, new Vector2(0.02f, 0.61f), new Vector2(0.98f, 0.68f));

        GameObject tacticalSummaryText = CreateTMP("TacticalSummaryText", "TACTICAL STATION: No active commands.", 18, tacticalBar.transform);
        SetAnchors(tacticalSummaryText, new Vector2(0.02f, 0.10f), new Vector2(0.98f, 0.90f));

        GameObject tacticalButtons = CreatePanel("TacticalButtons", canvas.transform, Color.clear);
        SetAnchors(tacticalButtons, new Vector2(0.02f, 0.53f), new Vector2(0.98f, 0.60f));
        ConfigureTacticalRow(tacticalButtons);

        GameObject scanButton = CreateButton("ScanButton", "SCAN", tacticalButtons.transform, new Color(0.18f, 0.18f, 0.28f, 1f), 18);
        GameObject focusButton = CreateButton("FocusButton", "FOCUS", tacticalButtons.transform, new Color(0.18f, 0.18f, 0.28f, 1f), 18);
        GameObject rallyButton = CreateButton("RallyButton", "RALLY", tacticalButtons.transform, new Color(0.18f, 0.18f, 0.28f, 1f), 18);

        GameObject dungeonGrid = CreatePanel("DungeonGrid", canvas.transform, Color.clear);
        SetAnchors(dungeonGrid, new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.52f));
        ConfigureGrid(dungeonGrid);

        GameObject teamSlots = CreatePanel("TeamSlots", canvas.transform, Color.clear);
        SetAnchors(teamSlots, new Vector2(0.02f, 0.12f), new Vector2(0.98f, 0.33f));
        ConfigureRow(teamSlots);

        GameObject startRunButton = CreateButton("StartRunButton", "START RUN", canvas.transform, new Color(0.2f, 0.55f, 0.25f, 1f), 24);
        SetAnchors(startRunButton, new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.10f));
        GameObject startRunButtonText = startRunButton.transform.Find("Text").gameObject;

        GameObject resultsPanel = CreatePanel("ResultsPanel", canvas.transform, new Color(0f, 0f, 0f, 0.88f));
        SetAnchors(resultsPanel, new Vector2(0f, 0f), new Vector2(1f, 1f));
        resultsPanel.SetActive(false);

        GameObject resultsText = CreateTMP("ResultsText", "Dungeon Result", 28, resultsPanel.transform);
        SetAnchors(resultsText, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.7f));

        GameObject closeResultsButton = CreateButton("CloseResultsButton", "CONTINUE", resultsPanel.transform, new Color(0.22f, 0.22f, 0.28f, 1f), 22);
        SetAnchors(closeResultsButton, new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.33f));

        DungeonView view = canvas.gameObject.AddComponent<DungeonView>();
        SerializedObject so = new SerializedObject(view);
        so.FindProperty("backButton").objectReferenceValue = backButton.GetComponent<Button>();
        so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("staminaText").objectReferenceValue = staminaText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("selectedDungeonText").objectReferenceValue = selectedDungeonText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("scheduleText").objectReferenceValue = scheduleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("todayScheduleText").objectReferenceValue = todayScheduleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("tacticalSummaryText").objectReferenceValue = tacticalSummaryText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("scanButton").objectReferenceValue = scanButton.GetComponent<Button>();
        so.FindProperty("focusButton").objectReferenceValue = focusButton.GetComponent<Button>();
        so.FindProperty("rallyButton").objectReferenceValue = rallyButton.GetComponent<Button>();
        so.FindProperty("dungeonGridContent").objectReferenceValue = dungeonGrid.GetComponent<RectTransform>();
        so.FindProperty("teamSlotsContent").objectReferenceValue = teamSlots.GetComponent<RectTransform>();
        so.FindProperty("startRunButton").objectReferenceValue = startRunButton.GetComponent<Button>();
        so.FindProperty("startRunButtonText").objectReferenceValue = startRunButtonText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("resultsPanel").objectReferenceValue = resultsPanel;
        so.FindProperty("resultsText").objectReferenceValue = resultsText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("closeResultsButton").objectReferenceValue = closeResultsButton.GetComponent<Button>();
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

        grid.cellSize = new Vector2(280, 160);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
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

    private static void ConfigureRow(GameObject panel)
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
        layout.spacing = 12f;
        layout.padding = new RectOffset(10, 10, 10, 10);
    }

    private static void ConfigureTacticalRow(GameObject panel)
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
        layout.padding = new RectOffset(10, 10, 10, 10);
    }
}
