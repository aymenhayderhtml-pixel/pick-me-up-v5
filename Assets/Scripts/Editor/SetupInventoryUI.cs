using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupInventoryUI
{
    [MenuItem("Tools/Pick Me Up/Setup Inventory UI")]
    public static void Execute()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        Canvas canvas = CreateCanvas("InventoryCanvas");

        GameObject topBar = CreatePanel("TopBar", canvas.transform, new Color(0.07f, 0.08f, 0.12f, 0.95f));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        GameObject backButton = CreateButton("BackButton", "< Back", topBar.transform, new Color(0.18f, 0.18f, 0.24f, 1f), 22);
        SetAnchors(backButton, new Vector2(0.02f, 0.15f), new Vector2(0.16f, 0.85f));

        GameObject titleText = CreateTMP("TitleText", "INVENTORY", 28, topBar.transform);
        SetAnchors(titleText, new Vector2(0.2f, 0.15f), new Vector2(0.6f, 0.85f));

        GameObject itemCountText = CreateTMP("ItemCountText", "0 Items", 24, topBar.transform);
        SetAnchors(itemCountText, new Vector2(0.72f, 0.15f), new Vector2(0.98f, 0.85f));

        GameObject filterBar = CreatePanel("FilterBar", canvas.transform, Color.clear);
        SetAnchors(filterBar, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.87f));

        GameObject allTabButton = CreateButton("AllTabButton", "ALL", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 20);
        SetAnchors(allTabButton, new Vector2(0.0f, 0f), new Vector2(0.24f, 1f));

        GameObject equipmentTabButton = CreateButton("EquipmentTabButton", "EQUIP", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 20);
        SetAnchors(equipmentTabButton, new Vector2(0.25f, 0f), new Vector2(0.49f, 1f));

        GameObject consumablesTabButton = CreateButton("ConsumablesTabButton", "USE", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 20);
        SetAnchors(consumablesTabButton, new Vector2(0.50f, 0f), new Vector2(0.74f, 1f));

        GameObject materialsTabButton = CreateButton("MaterialsTabButton", "MAT", filterBar.transform, new Color(0.18f, 0.2f, 0.28f, 1f), 20);
        SetAnchors(materialsTabButton, new Vector2(0.75f, 0f), new Vector2(0.99f, 1f));

        GameObject itemGrid = CreatePanel("ItemGridContent", canvas.transform, Color.clear);
        SetAnchors(itemGrid, new Vector2(0.02f, 0.22f), new Vector2(0.52f, 0.79f));
        ConfigureGrid(itemGrid);

        GameObject detailPanel = CreatePanel("DetailPanel", canvas.transform, new Color(0f, 0f, 0f, 0.88f));
        SetAnchors(detailPanel, new Vector2(0.55f, 0.22f), new Vector2(0.98f, 0.79f));
        detailPanel.SetActive(false);

        GameObject detailName = CreateTMP("DetailNameText", "Item", 24, detailPanel.transform);
        SetAnchors(detailName, new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.95f));

        GameObject detailType = CreateTMP("DetailTypeText", "Type", 16, detailPanel.transform);
        SetAnchors(detailType, new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.78f));

        GameObject detailDesc = CreateTMP("DetailDescriptionText", "Description", 16, detailPanel.transform);
        SetAnchors(detailDesc, new Vector2(0.05f, 0.46f), new Vector2(0.95f, 0.68f));

        GameObject detailQuantity = CreateTMP("DetailQuantityText", "Quantity", 16, detailPanel.transform);
        SetAnchors(detailQuantity, new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.42f));

        GameObject detailEquipped = CreateTMP("DetailEquippedText", "Equipped", 16, detailPanel.transform);
        SetAnchors(detailEquipped, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.30f));

        GameObject detailStats = CreateTMP("DetailStatsText", "Stats", 16, detailPanel.transform);
        SetAnchors(detailStats, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.18f));

        GameObject actionBar = CreatePanel("ActionBar", canvas.transform, Color.clear);
        SetAnchors(actionBar, new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.18f));

        GameObject useButton = CreateButton("UseButton", "USE", actionBar.transform, new Color(0.18f, 0.55f, 0.25f, 1f), 18);
        SetAnchors(useButton, new Vector2(0.0f, 0f), new Vector2(0.24f, 1f));

        GameObject equipButton = CreateButton("EquipButton", "EQUIP", actionBar.transform, new Color(0.18f, 0.55f, 0.25f, 1f), 18);
        SetAnchors(equipButton, new Vector2(0.25f, 0f), new Vector2(0.49f, 1f));

        GameObject unequipButton = CreateButton("UnequipButton", "UNEQUIP", actionBar.transform, new Color(0.18f, 0.55f, 0.25f, 1f), 18);
        SetAnchors(unequipButton, new Vector2(0.50f, 0f), new Vector2(0.74f, 1f));

        GameObject sellButton = CreateButton("SellButton", "SELL", actionBar.transform, new Color(0.55f, 0.18f, 0.18f, 1f), 18);
        SetAnchors(sellButton, new Vector2(0.75f, 0f), new Vector2(0.99f, 1f));

        InventoryView view = canvas.gameObject.AddComponent<InventoryView>();
        SerializedObject so = new SerializedObject(view);
        so.FindProperty("backButton").objectReferenceValue = backButton.GetComponent<Button>();
        so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("itemCountText").objectReferenceValue = itemCountText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("allTabButton").objectReferenceValue = allTabButton.GetComponent<Button>();
        so.FindProperty("equipmentTabButton").objectReferenceValue = equipmentTabButton.GetComponent<Button>();
        so.FindProperty("consumablesTabButton").objectReferenceValue = consumablesTabButton.GetComponent<Button>();
        so.FindProperty("materialsTabButton").objectReferenceValue = materialsTabButton.GetComponent<Button>();
        so.FindProperty("useButton").objectReferenceValue = useButton.GetComponent<Button>();
        so.FindProperty("equipButton").objectReferenceValue = equipButton.GetComponent<Button>();
        so.FindProperty("unequipButton").objectReferenceValue = unequipButton.GetComponent<Button>();
        so.FindProperty("sellButton").objectReferenceValue = sellButton.GetComponent<Button>();
        so.FindProperty("itemGridContent").objectReferenceValue = itemGrid.GetComponent<RectTransform>();
        so.FindProperty("detailPanel").objectReferenceValue = detailPanel;
        so.FindProperty("detailNameText").objectReferenceValue = detailName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailTypeText").objectReferenceValue = detailType.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailDescriptionText").objectReferenceValue = detailDesc.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailQuantityText").objectReferenceValue = detailQuantity.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailEquippedText").objectReferenceValue = detailEquipped.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailStatsText").objectReferenceValue = detailStats.GetComponent<TextMeshProUGUI>();
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

        grid.cellSize = new Vector2(160, 180);
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
