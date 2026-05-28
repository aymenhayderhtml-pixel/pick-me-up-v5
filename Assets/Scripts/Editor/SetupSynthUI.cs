using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupSynthUI
{
    [MenuItem("Tools/Pick Me Up/Setup Synth UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        Canvas canvas = CreateCanvas("SynthCanvas");

        GameObject topBar = CreatePanel("TopBar", canvas.transform, new Color(0.07f, 0.08f, 0.12f, 0.95f));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        GameObject backButton = CreateButton("BackButton", "< Back", topBar.transform, new Color(0.18f, 0.18f, 0.24f, 1f), 22);
        SetAnchors(backButton, new Vector2(0.02f, 0.15f), new Vector2(0.16f, 0.85f));

        GameObject titleText = CreateTMP("TitleText", "SYNTHESIS LAB", 28, topBar.transform);
        SetAnchors(titleText, new Vector2(0.22f, 0.15f), new Vector2(0.64f, 0.85f));

        GameObject successRateText = CreateTMP("SuccessRateText", "Select Heroes", 24, topBar.transform);
        SetAnchors(successRateText, new Vector2(0.70f, 0.15f), new Vector2(0.98f, 0.85f));

        GameObject baseArea = CreatePanel("BaseHeroArea", canvas.transform, Color.clear);
        SetAnchors(baseArea, new Vector2(0.02f, 0.56f), new Vector2(0.48f, 0.86f));
        ConfigureGrid(baseArea, 2, new Vector2(180, 100));

        GameObject materialArea = CreatePanel("MaterialArea", canvas.transform, Color.clear);
        SetAnchors(materialArea, new Vector2(0.52f, 0.56f), new Vector2(0.98f, 0.86f));
        ConfigureGrid(materialArea, 2, new Vector2(180, 100));

        GameObject previewArea = CreatePanel("PreviewArea", canvas.transform, new Color(0f, 0f, 0f, 0.85f));
        SetAnchors(previewArea, new Vector2(0.02f, 0.18f), new Vector2(0.98f, 0.54f));

        GameObject synthButton = CreateButton("SynthesizeButton", "SYNTHESIZE", canvas.transform, new Color(0.18f, 0.55f, 0.25f, 1f), 24);
        SetAnchors(synthButton, new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.14f));

        SynthView view = canvas.gameObject.AddComponent<SynthView>();
        SerializedObject so = new SerializedObject(view);
        so.FindProperty("backButton").objectReferenceValue = backButton.GetComponent<Button>();
        so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("successRateText").objectReferenceValue = successRateText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("baseHeroArea").objectReferenceValue = baseArea.GetComponent<RectTransform>();
        so.FindProperty("materialArea").objectReferenceValue = materialArea.GetComponent<RectTransform>();
        so.FindProperty("previewArea").objectReferenceValue = previewArea.GetComponent<RectTransform>();
        so.FindProperty("synthButton").objectReferenceValue = synthButton.GetComponent<Button>();
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

    private static void ConfigureGrid(GameObject panel, int columns, Vector2 cellSize)
    {
        GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = panel.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = cellSize;
        grid.spacing = new Vector2(12, 12);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
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
