using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupTowerUI
{
    // Color definitions
    private static readonly Color BG_COLOR = new Color(0.04f, 0.04f, 0.08f, 1f);
    private static readonly Color TOPBAR_COLOR = new Color(0.06f, 0.06f, 0.1f, 1f);
    private static readonly Color BTN_DARK_COLOR = new Color(0.2f, 0.2f, 0.2f, 1f);
    private static readonly Color BTN_PURPLE_COLOR = new Color(0.29f, 0.13f, 0.5f, 1f);
    private static readonly Color GOLD_COLOR = new Color(0.95f, 0.75f, 0.1f, 1f);
    private static readonly Color GRAY_COLOR = new Color(0.53f, 0.53f, 0.53f, 1f);
    private static readonly Color DARK_GRAY_COLOR = new Color(0.4f, 0.4f, 0.4f, 1f);
    private static readonly Color PANEL_COLOR = new Color(0.08f, 0.08f, 0.12f, 1f);

    [MenuItem("Tools/Pick Me Up/Setup Tower UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        // Create canvas
        GameObject canvasGo = new GameObject("TowerCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2340);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreatePanel("Background", canvasGo.transform, BG_COLOR);
        Stretch(bg);

        // Build UI hierarchy
        BuildTopBar(canvasGo.transform, out GameObject backBtnGo, out GameObject floorLabelGo);
        BuildCenterArea(canvasGo.transform, out GameObject currentFloorTitleGo, out GameObject currentFloorNumberGo, out GameObject highestFloorLabelGo, out GameObject startRunBtnGo);
        BuildBottomInfo(canvasGo.transform);

        // Add TowerView — must be on canvas root for ServiceRegistry
        TowerView towerView = canvasGo.AddComponent<TowerView>();

        // Wire via SerializedObject
        var so = new SerializedObject(towerView);

        so.FindProperty("backBtn").objectReferenceValue = backBtnGo.GetComponent<Button>();
        so.FindProperty("floorLabel").objectReferenceValue = floorLabelGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("currentFloorTitle").objectReferenceValue = currentFloorTitleGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("currentFloorNumber").objectReferenceValue = currentFloorNumberGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("highestFloorLabel").objectReferenceValue = highestFloorLabelGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("startRunBtn").objectReferenceValue = startRunBtnGo.GetComponent<Button>();
        so.FindProperty("startRunBtnText").objectReferenceValue = startRunBtnGo.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGo;

        Debug.Log("[SetupTowerUI] Tower UI built successfully.");
    }

    private static void BuildTopBar(Transform parent, out GameObject backBtn, out GameObject floorLabel)
    {
        GameObject topBar = CreatePanel("TopBar", parent, TOPBAR_COLOR);
        SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1));
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchoredPosition = new Vector2(0, -90);
        topBarRT.sizeDelta = new Vector2(0, 180);

        backBtn = CreateButton("BackBtn", "\u2190", topBar.transform, BTN_DARK_COLOR, 36);
        SetAnchors(backBtn, new Vector2(0, 0), new Vector2(0.15f, 1));

        GameObject title = CreateTMP("Title", "TOWER", 40, topBar.transform);
        SetAnchors(title, new Vector2(0.35f, 0), new Vector2(0.65f, 1));
        TextMeshProUGUI titleTMP = title.GetComponent<TextMeshProUGUI>();
        titleTMP.fontStyle = FontStyles.Bold;

        floorLabel = CreateTMP("FloorLabel", "Floor \u2014", 32, topBar.transform);
        SetAnchors(floorLabel, new Vector2(0.7f, 0), new Vector2(0.95f, 1));
        TextMeshProUGUI floorTMP = floorLabel.GetComponent<TextMeshProUGUI>();
        floorTMP.color = GOLD_COLOR;
        floorTMP.alignment = TextAlignmentOptions.Right;
    }

    private static void BuildCenterArea(Transform parent, out GameObject currentFloorTitle, out GameObject currentFloorNumber, out GameObject highestFloorLabel, out GameObject startRunBtn)
    {
        // Center container
        GameObject centerArea = CreatePanel("CenterArea", parent, new Color(0, 0, 0, 0));
        SetAnchors(centerArea, new Vector2(0, 0), new Vector2(1, 1));
        RectTransform centerRT = centerArea.GetComponent<RectTransform>();
        centerRT.offsetMin = new Vector2(50, 300);
        centerRT.offsetMax = new Vector2(-50, -250);

        currentFloorTitle = CreateTMP("CurrentFloorTitle", "TOWER STATUS", 48, centerArea.transform);
        SetAnchors(currentFloorTitle, new Vector2(0, 0.8f), new Vector2(1, 1));
        TextMeshProUGUI currentTitleTMP = currentFloorTitle.GetComponent<TextMeshProUGUI>();
        currentTitleTMP.alignment = TextAlignmentOptions.Center;

        currentFloorNumber = CreateTMP("CurrentFloorNumber", "\u2014", 120, centerArea.transform);
        SetAnchors(currentFloorNumber, new Vector2(0, 0.55f), new Vector2(1, 0.8f));
        TextMeshProUGUI currentFloorTMP = currentFloorNumber.GetComponent<TextMeshProUGUI>();
        currentFloorTMP.color = GOLD_COLOR;
        currentFloorTMP.fontStyle = FontStyles.Bold;
        currentFloorTMP.alignment = TextAlignmentOptions.Center;

        highestFloorLabel = CreateTMP("HighestFloorLabel", "Highest: Floor 0", 32, centerArea.transform);
        SetAnchors(highestFloorLabel, new Vector2(0, 0.4f), new Vector2(1, 0.55f));
        TextMeshProUGUI highestFloorTMP = highestFloorLabel.GetComponent<TextMeshProUGUI>();
        highestFloorTMP.color = GRAY_COLOR;
        highestFloorTMP.alignment = TextAlignmentOptions.Center;

        startRunBtn = CreateButton("StartRunBtn", "ENTER TOWER", centerArea.transform, BTN_PURPLE_COLOR, 40);
        SetAnchors(startRunBtn, new Vector2(0.1f, 0), new Vector2(0.9f, 0.3f));
    }

    private static void BuildBottomInfo(Transform parent)
    {
        GameObject bottomInfo = CreatePanel("BottomInfo", parent, new Color(0, 0, 0, 0));
        SetAnchors(bottomInfo, new Vector2(0, 0), new Vector2(1, 0.1f));

        GameObject infoText = CreateTMP("InfoText", "Climb the tower, defeat enemies on each floor.\nFailing a floor resets your progress.", 24, bottomInfo.transform);
        Stretch(infoText);
        TextMeshProUGUI infoTMP = infoText.GetComponent<TextMeshProUGUI>();
        infoTMP.color = DARK_GRAY_COLOR;
        infoTMP.alignment = TextAlignmentOptions.Center;
    }

    #region Helper Methods

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

    #endregion
}
