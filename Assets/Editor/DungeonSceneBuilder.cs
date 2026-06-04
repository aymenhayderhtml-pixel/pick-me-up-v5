using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class DungeonSceneBuilder
{
    [MenuItem("Tools/Build Dungeon Scene")]
    public static void BuildScene()
    {
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Setup Main Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        camObj.tag = "MainCamera";

        // Setup Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2340);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background / stage bands
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 1f);

        GameObject topBand = new GameObject("TopBand", typeof(RectTransform), typeof(Image));
        topBand.transform.SetParent(canvasObj.transform, false);
        SetAnchors(topBand, new Vector2(0, 0.78f), new Vector2(1, 1f));
        topBand.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.13f, 0.95f);

        GameObject centerStage = new GameObject("CenterStage", typeof(RectTransform), typeof(Image));
        centerStage.transform.SetParent(canvasObj.transform, false);
        SetAnchors(centerStage, new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.78f));
        centerStage.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.10f, 0.92f);

        GameObject bottomBand = new GameObject("BottomBand", typeof(RectTransform), typeof(Image));
        bottomBand.transform.SetParent(canvasObj.transform, false);
        SetAnchors(bottomBand, new Vector2(0, 0f), new Vector2(1, 0.20f));
        bottomBand.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.98f);

        // Top Bar
        GameObject topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        topBar.transform.SetParent(canvasObj.transform, false);
        RectTransform topBarRt = topBar.GetComponent<RectTransform>();
        topBarRt.anchorMin = new Vector2(0, 0.82f);
        topBarRt.anchorMax = new Vector2(1, 1f);
        topBarRt.offsetMin = Vector2.zero;
        topBarRt.offsetMax = Vector2.zero;
        topBar.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.13f, 0.96f);

        // Wave Text
        GameObject waveTextObj = new GameObject("WaveText", typeof(RectTransform), typeof(TextMeshProUGUI));
        waveTextObj.transform.SetParent(topBar.transform, false);
        TextMeshProUGUI waveText = waveTextObj.GetComponent<TextMeshProUGUI>();
        waveText.text = "Wave 1 / 10";
        waveText.fontSize = 44;
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.color = Color.white;
        waveText.fontStyle = FontStyles.Bold;
        RectTransform waveRt = waveTextObj.GetComponent<RectTransform>();
        waveRt.anchorMin = new Vector2(0.22f, 0.60f);
        waveRt.anchorMax = new Vector2(0.96f, 0.92f);
        waveRt.offsetMin = Vector2.zero;
        waveRt.offsetMax = Vector2.zero;

        // Enemy Count Text
        GameObject countTextObj = new GameObject("EnemyCountText", typeof(RectTransform), typeof(TextMeshProUGUI));
        countTextObj.transform.SetParent(topBar.transform, false);
        TextMeshProUGUI countText = countTextObj.GetComponent<TextMeshProUGUI>();
        countText.text = "Enemies: 0";
        countText.fontSize = 28;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = new Color(0.80f, 0.85f, 0.95f, 1f);
        RectTransform countRt = countTextObj.GetComponent<RectTransform>();
        countRt.anchorMin = new Vector2(0.22f, 0.10f);
        countRt.anchorMax = new Vector2(0.96f, 0.55f);
        countRt.offsetMin = Vector2.zero;
        countRt.offsetMax = Vector2.zero;

        // Enemy Portrait (frame + image)
        GameObject portraitFrame = new GameObject("EnemyPortraitFrame", typeof(RectTransform), typeof(Image));
        portraitFrame.transform.SetParent(topBar.transform, false);
        Image frameImg = portraitFrame.GetComponent<Image>();
        frameImg.color = new Color(0.14f, 0.16f, 0.22f, 1f);
        RectTransform frameRt = portraitFrame.GetComponent<RectTransform>();
        frameRt.anchorMin = new Vector2(0.04f, 0.12f);
        frameRt.anchorMax = new Vector2(0.20f, 0.88f);
        frameRt.offsetMin = new Vector2(12, 12);
        frameRt.offsetMax = new Vector2(-12, -12);

        GameObject portraitObj = new GameObject("EnemyPortrait", typeof(RectTransform), typeof(Image));
        portraitObj.transform.SetParent(portraitFrame.transform, false);
        Image portraitImg = portraitObj.GetComponent<Image>();
        portraitImg.color = new Color(0.22f, 0.26f, 0.34f, 1f);
        portraitImg.preserveAspect = true;
        RectTransform portraitRt = portraitObj.GetComponent<RectTransform>();
        portraitRt.anchorMin = Vector2.zero;
        portraitRt.anchorMax = Vector2.one;
        portraitRt.offsetMin = new Vector2(6, 6);
        portraitRt.offsetMax = new Vector2(-6, -6);

        // Center detail staging (decorative title only - list and detail panels sit on top)
        GameObject stageLabel = new GameObject("StageLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        stageLabel.transform.SetParent(centerStage.transform, false);
        TextMeshProUGUI stageText = stageLabel.GetComponent<TextMeshProUGUI>();
        stageText.text = "AVAILABLE DUNGEONS";
        stageText.fontSize = 16;
        stageText.alignment = TextAlignmentOptions.Left;
        stageText.color = new Color(0.55f, 0.60f, 0.70f, 1f);
        stageText.fontStyle = FontStyles.Bold;
        SetAnchors(stageLabel, new Vector2(0.07f, 0.945f), new Vector2(0.34f, 0.985f));

        // Bottom Bar / Action Buttons (Back to Hub + Abandon)
        GameObject backBtnObj = new GameObject("BackToHubButton", typeof(RectTransform), typeof(Image), typeof(Button));
        backBtnObj.transform.SetParent(bottomBand.transform, false);
        Image backBtnImage = backBtnObj.GetComponent<Image>();
        backBtnImage.color = new Color(0.18f, 0.20f, 0.26f, 1f);
        Button backBtn = backBtnObj.GetComponent<Button>();
        RectTransform backBtnRt = backBtnObj.GetComponent<RectTransform>();
        backBtnRt.anchorMin = new Vector2(0.05f, 0.18f);
        backBtnRt.anchorMax = new Vector2(0.35f, 0.82f);
        backBtnRt.pivot = new Vector2(0.5f, 0);
        backBtnRt.offsetMin = Vector2.zero;
        backBtnRt.offsetMax = Vector2.zero;

        GameObject backTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        backTextObj.transform.SetParent(backBtnObj.transform, false);
        TextMeshProUGUI backText = backTextObj.GetComponent<TextMeshProUGUI>();
        backText.text = "BACK TO HUB";
        backText.fontSize = 28;
        backText.alignment = TextAlignmentOptions.Center;
        backText.color = Color.white;
        backText.fontStyle = FontStyles.Bold;
        RectTransform backTextRt = backTextObj.GetComponent<RectTransform>();
        backTextRt.anchorMin = Vector2.zero;
        backTextRt.anchorMax = Vector2.one;
        backTextRt.sizeDelta = Vector2.zero;

        GameObject btnObj = new GameObject("AbandonButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(bottomBand.transform, false);
        Image btnImage = btnObj.GetComponent<Image>();
        btnImage.color = new Color(0.45f, 0.12f, 0.12f, 1f);
        Button abandonBtn = btnObj.GetComponent<Button>();
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.65f, 0.18f);
        btnRt.anchorMax = new Vector2(0.95f, 0.82f);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;

        GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnText.text = "ABANDON";
        btnText.fontSize = 32;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        btnText.fontStyle = FontStyles.Bold;
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;

        // Panels
        GameObject victoryPanel = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image));
        victoryPanel.transform.SetParent(canvasObj.transform, false);
        victoryPanel.GetComponent<Image>().color = new Color(0, 0.8f, 0.15f, 0.75f);
        RectTransform vicRt = victoryPanel.GetComponent<RectTransform>();
        vicRt.anchorMin = Vector2.zero;
        vicRt.anchorMax = Vector2.one;
        vicRt.sizeDelta = Vector2.zero;
        
        GameObject vicTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        vicTextObj.transform.SetParent(victoryPanel.transform, false);
        TextMeshProUGUI vicText = vicTextObj.GetComponent<TextMeshProUGUI>();
        vicText.text = "VICTORY!";
        vicText.fontSize = 100;
        vicText.alignment = TextAlignmentOptions.Center;
        vicText.color = Color.white;
        RectTransform vicTextRt = vicTextObj.GetComponent<RectTransform>();
        vicTextRt.anchorMin = Vector2.zero;
        vicTextRt.anchorMax = Vector2.one;
        vicTextRt.sizeDelta = Vector2.zero;
        
        victoryPanel.SetActive(false);

        GameObject defeatPanel = new GameObject("DefeatPanel", typeof(RectTransform), typeof(Image));
        defeatPanel.transform.SetParent(canvasObj.transform, false);
        defeatPanel.GetComponent<Image>().color = new Color(0.85f, 0.1f, 0.1f, 0.75f);
        RectTransform defRt = defeatPanel.GetComponent<RectTransform>();
        defRt.anchorMin = Vector2.zero;
        defRt.anchorMax = Vector2.one;
        defRt.sizeDelta = Vector2.zero;

        GameObject defTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        defTextObj.transform.SetParent(defeatPanel.transform, false);
        TextMeshProUGUI defText = defTextObj.GetComponent<TextMeshProUGUI>();
        defText.text = "DEFEAT...";
        defText.fontSize = 100;
        defText.alignment = TextAlignmentOptions.Center;
        defText.color = Color.white;
        RectTransform defTextRt = defTextObj.GetComponent<RectTransform>();
        defTextRt.anchorMin = Vector2.zero;
        defTextRt.anchorMax = Vector2.one;
        defTextRt.sizeDelta = Vector2.zero;

        defeatPanel.SetActive(false);

        // Reward panel (sits below the victory banner; shows cleared-reward summary)
        GameObject rewardPanel = new GameObject("RewardPanel", typeof(RectTransform), typeof(Image));
        rewardPanel.transform.SetParent(canvasObj.transform, false);
        Image rewardPanelImg = rewardPanel.GetComponent<Image>();
        rewardPanelImg.color = new Color(0.05f, 0.06f, 0.10f, 0.92f);
        RectTransform rewardPanelRt = rewardPanel.GetComponent<RectTransform>();
        rewardPanelRt.anchorMin = new Vector2(0.04f, 0.22f);
        rewardPanelRt.anchorMax = new Vector2(0.96f, 0.78f);
        rewardPanelRt.offsetMin = Vector2.zero;
        rewardPanelRt.offsetMax = Vector2.zero;

        GameObject rewardSummary = CreateTMP("RewardSummary", "DUNGEON CLEARED", 36, rewardPanel.transform);
        SetAnchors(rewardSummary, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.97f));
        rewardSummary.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        rewardSummary.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        rewardSummary.GetComponent<TextMeshProUGUI>().color = new Color(0.30f, 0.90f, 0.45f, 1f);

        GameObject rewardListPanel = new GameObject("RewardList", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        rewardListPanel.transform.SetParent(rewardPanel.transform, false);
        Image rewardListImg = rewardListPanel.GetComponent<Image>();
        rewardListImg.color = new Color(0.08f, 0.10f, 0.14f, 0.85f);
        RectTransform rewardListRt = rewardListPanel.GetComponent<RectTransform>();
        rewardListRt.anchorMin = new Vector2(0.04f, 0.18f);
        rewardListRt.anchorMax = new Vector2(0.96f, 0.85f);
        rewardListRt.offsetMin = Vector2.zero;
        rewardListRt.offsetMax = Vector2.zero;
        VerticalLayoutGroup rewardListLayout = rewardListPanel.GetComponent<VerticalLayoutGroup>();
        rewardListLayout.spacing = 10f;
        rewardListLayout.padding = new RectOffset(16, 16, 16, 16);
        rewardListLayout.childForceExpandHeight = false;
        rewardListLayout.childForceExpandWidth = true;
        rewardListLayout.childAlignment = TextAnchor.UpperCenter;
        ContentSizeFitter rewardListFitter = rewardListPanel.AddComponent<ContentSizeFitter>();
        rewardListFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject rewardClaim = CreateButton("RewardClaimButton", "CLAIM", rewardPanel.transform, new Color(0.18f, 0.55f, 0.30f, 1f), 32);
        SetAnchors(rewardClaim, new Vector2(0.30f, 0.03f), new Vector2(0.70f, 0.15f));
        Button rewardClaimBtn = rewardClaim.GetComponent<Button>();

        rewardPanel.SetActive(false);

        // Controllers
        GameObject controllersObj = new GameObject("--Controllers--");

        // Dungeon Selection
        GameObject selectorObj = new GameObject("DungeonListView");
        selectorObj.transform.SetParent(controllersObj.transform);
        DungeonListView listView = selectorObj.AddComponent<DungeonListView>();

        GameObject listPanel = new GameObject("ListPanel", typeof(RectTransform), typeof(Image));
        listPanel.transform.SetParent(centerStage.transform, false);
        SetAnchors(listPanel, new Vector2(0.05f, 0.08f), new Vector2(0.36f, 0.94f));
        listPanel.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.13f, 0.92f);

        GameObject listScroll = new GameObject("ListContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listScroll.transform.SetParent(listPanel.transform, false);
        RectTransform listScrollRt = listScroll.GetComponent<RectTransform>();
        listScrollRt.anchorMin = new Vector2(0.05f, 0.05f);
        listScrollRt.anchorMax = new Vector2(0.95f, 0.95f);
        listScrollRt.pivot = new Vector2(0.5f, 1f);
        listScrollRt.sizeDelta = new Vector2(1f, 1.1f);
        listScrollRt.offsetMin = Vector2.zero;
        listScrollRt.offsetMax = Vector2.zero;
        var layout = listScroll.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        listScroll.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject detailPanel = new GameObject("DetailPanel", typeof(RectTransform), typeof(Image));
        detailPanel.transform.SetParent(centerStage.transform, false);
        SetAnchors(detailPanel, new Vector2(0.39f, 0.08f), new Vector2(0.95f, 0.94f));
        detailPanel.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.11f, 0.92f);

        GameObject detailTitle = CreateTMP("DetailTitle", "SELECT A DUNGEON", 34, detailPanel.transform);
        SetAnchors(detailTitle, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f));
        detailTitle.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject detailDesc = CreateTMP("DetailDesc", "Dungeon details will appear here.", 22, detailPanel.transform);
        SetAnchors(detailDesc, new Vector2(0.05f, 0.44f), new Vector2(0.95f, 0.74f));
        detailDesc.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        GameObject detailCost = CreateTMP("DetailCost", "SP --", 24, detailPanel.transform);
        SetAnchors(detailCost, new Vector2(0.05f, 0.25f), new Vector2(0.45f, 0.40f));

        GameObject detailReward = CreateTMP("DetailReward", "Rewards --", 24, detailPanel.transform);
        SetAnchors(detailReward, new Vector2(0.50f, 0.25f), new Vector2(0.95f, 0.40f));

        GameObject detailIcon = new GameObject("DetailIcon", typeof(RectTransform), typeof(Image));
        detailIcon.transform.SetParent(detailPanel.transform, false);
        SetAnchors(detailIcon, new Vector2(0.05f, 0.05f), new Vector2(0.25f, 0.22f));
        detailIcon.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.23f, 1f);

        GameObject startBtn = CreateButton("StartDungeonButton", "START", detailPanel.transform, new Color(0.15f, 0.45f, 0.25f, 1f), 24);
        SetAnchors(startBtn, new Vector2(0.55f, 0.05f), new Vector2(0.95f, 0.18f));

        SerializedObject listSo = new SerializedObject(listView);
        listSo.FindProperty("listContent").objectReferenceValue = listScroll.GetComponent<RectTransform>();
        listSo.FindProperty("titleText").objectReferenceValue = detailTitle.GetComponent<TextMeshProUGUI>();
        listSo.FindProperty("descriptionText").objectReferenceValue = detailDesc.GetComponent<TextMeshProUGUI>();
        listSo.FindProperty("costText").objectReferenceValue = detailCost.GetComponent<TextMeshProUGUI>();
        listSo.FindProperty("rewardText").objectReferenceValue = detailReward.GetComponent<TextMeshProUGUI>();
        listSo.FindProperty("iconImage").objectReferenceValue = detailIcon.GetComponent<Image>();
        listSo.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
        listSo.ApplyModifiedProperties();

        // Reward view (forwarded by the HUD) - declared here so it can reference list/detail panels
        GameObject rewardViewObj = new GameObject("DungeonRewardView");
        rewardViewObj.transform.SetParent(controllersObj.transform);
        DungeonRewardView rewardView = rewardViewObj.AddComponent<DungeonRewardView>();
        SerializedObject soRewardView = new SerializedObject(rewardView);
        soRewardView.FindProperty("rootPanel").objectReferenceValue = rewardPanel;
        soRewardView.FindProperty("itemListContent").objectReferenceValue = rewardListPanel.GetComponent<RectTransform>();
        soRewardView.FindProperty("summaryText").objectReferenceValue = rewardSummary.GetComponent<TextMeshProUGUI>();
        soRewardView.FindProperty("claimButton").objectReferenceValue = rewardClaimBtn;
        soRewardView.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        soRewardView.FindProperty("dungeonListPanel").objectReferenceValue = listPanel;
        soRewardView.FindProperty("dungeonDetailPanel").objectReferenceValue = detailPanel;
        soRewardView.ApplyModifiedProperties();

        // Dungeon HUD
        GameObject hudObj = new GameObject("DungeonHUD");
        hudObj.transform.SetParent(controllersObj.transform);
        DungeonHUD hud = hudObj.AddComponent<DungeonHUD>();

        SerializedObject soHud = new SerializedObject(hud);
        soHud.FindProperty("waveText").objectReferenceValue = waveText;
        soHud.FindProperty("enemyCountText").objectReferenceValue = countText;
        soHud.FindProperty("currentEnemyPortrait").objectReferenceValue = portraitImg;
        soHud.FindProperty("abandonButton").objectReferenceValue = abandonBtn;
        soHud.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        soHud.FindProperty("defeatPanel").objectReferenceValue = defeatPanel;
        soHud.FindProperty("rewardView").objectReferenceValue = rewardView;
        soHud.ApplyModifiedProperties();

        // Dungeon scene controller (back to hub)
        GameObject sceneCtrlObj = new GameObject("DungeonSceneController");
        sceneCtrlObj.transform.SetParent(controllersObj.transform);
        DungeonSceneController sceneCtrl = sceneCtrlObj.AddComponent<DungeonSceneController>();
        SerializedObject soCtrl = new SerializedObject(sceneCtrl);
        soCtrl.FindProperty("backToHubButton").objectReferenceValue = backBtn;
        soCtrl.ApplyModifiedProperties();

        // Spawn Manager
        GameObject spawnRoot = new GameObject("SpawnRoot");
        
        GameObject managerObj = new GameObject("DungeonSpawnManager");
        managerObj.transform.SetParent(controllersObj.transform);
        DungeonSpawnManager spawnManager = managerObj.AddComponent<DungeonSpawnManager>();

        SerializedObject soSpawn = new SerializedObject(spawnManager);
        soSpawn.FindProperty("spawnRoot").objectReferenceValue = spawnRoot.transform;
        soSpawn.FindProperty("spawnBounds").vector2Value = new Vector2(5f, 5f);

        // Wire default enemy prefab mapping
        GameObject defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/EnemyPrefabs/DefaultEnemy.prefab");
        if (defaultPrefab != null)
        {
            EnemyDataSO[] enemyAssets = Resources.LoadAll<EnemyDataSO>("Dungeons");
            SerializedProperty mappingList = soSpawn.FindProperty("prefabMappings");
            mappingList.ClearArray();
            for (int i = 0; i < enemyAssets.Length; i++)
            {
                if (enemyAssets[i] == null) continue;
                mappingList.InsertArrayElementAtIndex(i);
                SerializedProperty mapping = mappingList.GetArrayElementAtIndex(i);
                mapping.FindPropertyRelative("Data").objectReferenceValue = enemyAssets[i];
                mapping.FindPropertyRelative("Prefab").objectReferenceValue = defaultPrefab;
            }
        }

        soSpawn.ApplyModifiedProperties();

        // EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        string scenePath = "Assets/Scenes/Dungeon.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log("[DungeonSceneBuilder] Dungeon Scene created successfully at " + scenePath);
    }

    private static void SetAnchors(GameObject go, Vector2 min, Vector2 max)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        GameObject text = CreateTMP("Text", label, fontSize, go.transform).gameObject;
        SetAnchors(text, Vector2.zero, Vector2.one);
        return go;
    }
}
