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

        GameObject topBar = FindOrCreateChild(canvas.transform, "TopBar", () => CreatePanel("TopBar", canvas.transform, new Color(0.05f, 0.06f, 0.10f, 0.98f)));
        SetAnchors(topBar, new Vector2(0, 0.88f), new Vector2(1, 1));

        // Player Identity Panel (avatar, name, level, XP bar)
        GameObject playerCard = FindOrCreateChild(topBar.transform, "PlayerIdentity", () => CreatePanel("PlayerIdentity", topBar.transform, Color.clear));
        SetAnchors(playerCard, new Vector2(0.02f, 0.05f), new Vector2(0.48f, 0.95f));

        GameObject avatar = FindOrCreateChild(playerCard.transform, "Avatar", () => CreatePanel("Avatar", playerCard.transform, new Color(0.12f, 0.18f, 0.35f, 1f)));
        SetAnchors(avatar, new Vector2(0f, 0.10f), new Vector2(0.18f, 0.90f));

        GameObject nameLabel = FindOrCreateChild(playerCard.transform, "PlayerName", () => CreateTMP("PlayerName", "Loki (Master)", 26, playerCard.transform));
        SetAnchors(nameLabel, new Vector2(0.20f, 0.50f), new Vector2(0.60f, 0.90f));
        nameLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        nameLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject lvLabel = FindOrCreateChild(playerCard.transform, "PlayerLevel", () => CreateTMP("PlayerLevel", "Lv.14", 22, playerCard.transform));
        SetAnchors(lvLabel, new Vector2(0.62f, 0.50f), new Vector2(0.98f, 0.90f));
        lvLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        lvLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.76f, 0.03f, 1f); // Gold

        GameObject xpBg = FindOrCreateChild(playerCard.transform, "XPBarBg", () => CreatePanel("XPBarBg", playerCard.transform, new Color(0.05f, 0.05f, 0.08f, 1f)));
        SetAnchors(xpBg, new Vector2(0.20f, 0.15f), new Vector2(0.98f, 0.35f));

        GameObject xpFill = FindOrCreateChild(xpBg.transform, "XPBarFill", () => CreatePanel("XPBarFill", xpBg.transform, new Color(0f, 0.7f, 1f, 1f)));
        SetAnchors(xpFill, new Vector2(0f, 0f), new Vector2(0.65f, 1f)); // 65% XP

        // Currency Displays (Shifted to the right side of TopBar, color coded)
        GameObject goldLabel = FindOrCreateChild(topBar.transform, "GoldLabel", () => CreateTMP("GoldLabel", "Gold: 0", 24, topBar.transform));
        SetAnchors(goldLabel, new Vector2(0.50f, 0.15f), new Vector2(0.62f, 0.85f));
        goldLabel.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.84f, 0f, 1f); // Gold yellow
        goldLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject gemsLabel = FindOrCreateChild(topBar.transform, "GemsLabel", () => CreateTMP("GemsLabel", "Gems: 0", 24, topBar.transform));
        SetAnchors(gemsLabel, new Vector2(0.62f, 0.15f), new Vector2(0.74f, 0.85f));
        gemsLabel.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.90f, 1f, 1f); // Gems cyan
        gemsLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject stonesLabel = FindOrCreateChild(topBar.transform, "StonesLabel", () => CreateTMP("StonesLabel", "Stones: 0", 24, topBar.transform));
        SetAnchors(stonesLabel, new Vector2(0.74f, 0.15f), new Vector2(0.86f, 0.85f));
        stonesLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.82f, 0.77f, 0.91f); // Stones lavender
        stonesLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject staminaLabel = FindOrCreateChild(topBar.transform, "StaminaLabel", () => CreateTMP("StaminaLabel", "Stamina: 100/100", 24, topBar.transform));
        SetAnchors(staminaLabel, new Vector2(0.86f, 0.15f), new Vector2(0.98f, 0.85f));
        staminaLabel.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0.90f, 0.46f, 1f); // Stamina green
        staminaLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Background / void fill: CenterArea and events
        GameObject centerArea = FindOrCreateChild(canvas.transform, "CenterArea", () => CreatePanel("CenterArea", canvas.transform, new Color(0.08f, 0.10f, 0.15f, 1f)));
        SetAnchors(centerArea, new Vector2(0, 0.20f), new Vector2(1, 0.88f));

        GameObject carousel = FindOrCreateChild(centerArea.transform, "EventCarousel", () => CreatePanel("EventCarousel", centerArea.transform, new Color(0.05f, 0.06f, 0.10f, 0.6f)));
        SetAnchors(carousel, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.90f));

        GameObject bannerTitle = FindOrCreateChild(carousel.transform, "BannerTitle", () => CreateTMP("BannerTitle", "EVENT: CODES OF TAONI", 36, carousel.transform));
        SetAnchors(bannerTitle, new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.90f));
        bannerTitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.76f, 0.03f, 1f); // Gold accent
        bannerTitle.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject bannerDesc = FindOrCreateChild(carousel.transform, "BannerDesc", () => CreateTMP("BannerDesc", "Flesh out facility upgrades, explore Towers,\nand prepare your Heroes for the spatial crack.\n\nRate up event is currently live in the summoning room!", 26, carousel.transform));
        SetAnchors(bannerDesc, new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.70f));
        bannerDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.70f, 0.61f, 0.86f); // secondary lavender
        bannerDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        GameObject bottomDock = FindOrCreateChild(canvas.transform, "BottomDock", () => CreatePanel("BottomDock", canvas.transform, new Color(0.05f, 0.06f, 0.10f, 0.98f)));
        SetAnchors(bottomDock, new Vector2(0, 0), new Vector2(1, 0.20f));

        GameObject topRow = FindOrCreateChild(bottomDock.transform, "TopRow", () => CreatePanel("TopRow", bottomDock.transform, Color.clear));
        SetAnchors(topRow, new Vector2(0.02f, 0.52f), new Vector2(0.98f, 0.98f));
        ConfigureRow(topRow);

        GameObject bottomRow = FindOrCreateChild(bottomDock.transform, "BottomRow", () => CreatePanel("BottomRow", bottomDock.transform, Color.clear));
        SetAnchors(bottomRow, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.50f));
        ConfigureRow(bottomRow);

        // Grid Button styling and priority differences
        Color normalBtnColor = new Color(0.12f, 0.18f, 0.35f, 1f); // deep navy
        Color priorityBtnColor = new Color(0.35f, 0.20f, 0.65f, 1f); // Royal purple for SUMMON
        Color priorityDungeonColor = new Color(0.20f, 0.35f, 0.65f, 1f); // Royal blue for DUNGEON

        GameObject rosterBtn = FindOrCreateChild(topRow.transform, "RosterBtn", () => CreateButton("RosterBtn", "ROSTER", topRow.transform, normalBtnColor, 24));
        GameObject synthBtn = FindOrCreateChild(topRow.transform, "SynthBtn", () => CreateButton("SynthBtn", "SYNTH", topRow.transform, normalBtnColor, 24));
        GameObject trainBtn = FindOrCreateChild(topRow.transform, "TrainBtn", () => CreateButton("TrainBtn", "FACILITIES", topRow.transform, normalBtnColor, 22));
        GameObject towerBtn = FindOrCreateChild(topRow.transform, "TowerBtn", () => CreateButton("TowerBtn", "TOWER", topRow.transform, normalBtnColor, 24));

        GameObject summonBtn = FindOrCreateChild(bottomRow.transform, "SummonBtn", () => CreateButton("SummonBtn", "SUMMON", bottomRow.transform, priorityBtnColor, 24));
        GameObject dungeonBtn = FindOrCreateChild(bottomRow.transform, "DungeonBtn", () => CreateButton("DungeonBtn", "DUNGEON", bottomRow.transform, priorityDungeonColor, 24));
        GameObject inventoryBtn = FindOrCreateChild(bottomRow.transform, "InventoryBtn", () => CreateButton("InventoryBtn", "INVENTORY", bottomRow.transform, normalBtnColor, 22));
        GameObject memorialBtn = FindOrCreateChild(bottomRow.transform, "MemorialBtn", () => CreateButton("MemorialBtn", "MEMORIAL", bottomRow.transform, normalBtnColor, 22));

        // Notification Badges (SUMMON & MEMORIAL)
        GameObject summonBadge = FindOrCreateChild(summonBtn.transform, "NotificationBadge", () => CreatePanel("NotificationBadge", summonBtn.transform, new Color(0.85f, 0.15f, 0.15f, 1f)));
        SetAnchors(summonBadge, new Vector2(0.85f, 0.70f), new Vector2(0.97f, 0.95f));

        GameObject memorialBadge = FindOrCreateChild(memorialBtn.transform, "NotificationBadge", () => CreatePanel("NotificationBadge", memorialBtn.transform, new Color(0.85f, 0.15f, 0.15f, 1f)));
        SetAnchors(memorialBadge, new Vector2(0.85f, 0.70f), new Vector2(0.97f, 0.95f));

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
        scaler.referenceResolution = new Vector2(2400, 1080);
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
