using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupHubUI
{
    [MenuItem("Tools/Pick Me Up/Setup Hub UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        Canvas canvas = CreateCanvas("HubCanvas");

        GameObject topBar = FindOrCreateChild(canvas.transform, "TopBar", () => CreatePanel("TopBar", canvas.transform, new Color(0.08f, 0.08f, 0.12f, 0.95f)));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        GameObject goldLabel = FindOrCreateChild(topBar.transform, "GoldLabel", () => CreateTMP("GoldLabel", "Gold: 0", 28, topBar.transform));
        SetAnchors(goldLabel, new Vector2(0.02f, 0.15f), new Vector2(0.24f, 0.85f));

        GameObject gemsLabel = FindOrCreateChild(topBar.transform, "GemsLabel", () => CreateTMP("GemsLabel", "Gems: 0", 28, topBar.transform));
        SetAnchors(gemsLabel, new Vector2(0.24f, 0.15f), new Vector2(0.46f, 0.85f));

        GameObject stonesLabel = FindOrCreateChild(topBar.transform, "StonesLabel", () => CreateTMP("StonesLabel", "Stones: 0", 28, topBar.transform));
        SetAnchors(stonesLabel, new Vector2(0.46f, 0.15f), new Vector2(0.68f, 0.85f));

        GameObject staminaLabel = FindOrCreateChild(topBar.transform, "StaminaLabel", () => CreateTMP("StaminaLabel", "Stamina: 100/100", 28, topBar.transform));
        SetAnchors(staminaLabel, new Vector2(0.68f, 0.15f), new Vector2(0.98f, 0.85f));

        GameObject centerArea = FindOrCreateChild(canvas.transform, "CenterArea", () => CreatePanel("CenterArea", canvas.transform, new Color(0.04f, 0.05f, 0.08f, 1f)));
        SetAnchors(centerArea, new Vector2(0, 0.20f), new Vector2(1, 0.88f));

        GameObject bottomDock = FindOrCreateChild(canvas.transform, "BottomDock", () => CreatePanel("BottomDock", canvas.transform, new Color(0.06f, 0.06f, 0.1f, 0.98f)));
        SetAnchors(bottomDock, new Vector2(0, 0), new Vector2(1, 0.20f));

        GameObject topRow = FindOrCreateChild(bottomDock.transform, "TopRow", () => CreatePanel("TopRow", bottomDock.transform, Color.clear));
        SetAnchors(topRow, new Vector2(0.02f, 0.52f), new Vector2(0.98f, 0.98f));
        ConfigureRow(topRow);

        GameObject bottomRow = FindOrCreateChild(bottomDock.transform, "BottomRow", () => CreatePanel("BottomRow", bottomDock.transform, Color.clear));
        SetAnchors(bottomRow, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.50f));
        ConfigureRow(bottomRow);

        GameObject rosterBtn = FindOrCreateChild(topRow.transform, "RosterBtn", () => CreateButton("RosterBtn", "ROSTER", topRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 24));
        GameObject synthBtn = FindOrCreateChild(topRow.transform, "SynthBtn", () => CreateButton("SynthBtn", "SYNTH", topRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 24));
        GameObject trainBtn = FindOrCreateChild(topRow.transform, "TrainBtn", () => CreateButton("TrainBtn", "FACILITIES", topRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 22));
        GameObject towerBtn = FindOrCreateChild(topRow.transform, "TowerBtn", () => CreateButton("TowerBtn", "TOWER", topRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 24));

        GameObject summonBtn = FindOrCreateChild(bottomRow.transform, "SummonBtn", () => CreateButton("SummonBtn", "SUMMON", bottomRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 24));
        GameObject dungeonBtn = FindOrCreateChild(bottomRow.transform, "DungeonBtn", () => CreateButton("DungeonBtn", "DUNGEON", bottomRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 24));
        GameObject inventoryBtn = FindOrCreateChild(bottomRow.transform, "InventoryBtn", () => CreateButton("InventoryBtn", "INVENTORY", bottomRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 22));
        GameObject memorialBtn = FindOrCreateChild(bottomRow.transform, "MemorialBtn", () => CreateButton("MemorialBtn", "MEMORIAL", bottomRow.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 22));

        HubView hubView = canvas.gameObject.GetComponent<HubView>();
        if (hubView == null)
        {
            hubView = canvas.gameObject.AddComponent<HubView>();
        }

        SerializedObject so = new SerializedObject(hubView);
        so.FindProperty("goldLabel").objectReferenceValue = goldLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("gemsLabel").objectReferenceValue = gemsLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("stonesLabel").objectReferenceValue = stonesLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("staminaLabel").objectReferenceValue = staminaLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("rosterBtn").objectReferenceValue = rosterBtn.GetComponent<Button>();
        so.FindProperty("synthBtn").objectReferenceValue = synthBtn.GetComponent<Button>();
        so.FindProperty("trainBtn").objectReferenceValue = trainBtn.GetComponent<Button>();
        so.FindProperty("towerBtn").objectReferenceValue = towerBtn.GetComponent<Button>();
        so.FindProperty("summonBtn").objectReferenceValue = summonBtn.GetComponent<Button>();
        so.FindProperty("dungeonBtn").objectReferenceValue = dungeonBtn.GetComponent<Button>();
        so.FindProperty("inventoryBtn").objectReferenceValue = inventoryBtn.GetComponent<Button>();
        so.FindProperty("memorialBtn").objectReferenceValue = memorialBtn.GetComponent<Button>();
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

    private static GameObject FindOrCreateChild(Transform parent, string name, System.Func<GameObject> factory)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
            {
                return parent.GetChild(i).gameObject;
            }
        }

        GameObject created = factory();
        created.name = name;
        return created;
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

    private static void ConfigureRow(GameObject row)
    {
        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = row.AddComponent<HorizontalLayoutGroup>();
        }

        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.spacing = 14f;
        layout.padding = new RectOffset(10, 10, 10, 10);
    }
}
