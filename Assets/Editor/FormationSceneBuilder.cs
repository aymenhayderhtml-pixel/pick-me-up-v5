#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class FormationSceneBuilder : EditorWindow
{
    [MenuItem("PickMeUp/Build Formation Scene")]
    public static void ShowWindow()
    {
        GetWindow<FormationSceneBuilder>("Formation Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Formation Scene Builder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Build Formation Scene", GUILayout.Height(40)))
        {
            BuildScene();
        }
    }

    private static void BuildScene()
    {
        // Create root canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Title
        GameObject titleObj = CreateText(canvasObj.transform, "TITLE", "FORM YOUR TEAM", 48, TextAlignmentOptions.Center);
        SetAnchors(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-200f, -80f), new Vector2(200f, -20f));

        // Slot Container
        GameObject slotContainer = new GameObject("SlotContainer");
        slotContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform slotContRect = slotContainer.AddComponent<RectTransform>();
        slotContRect.anchorMin = new Vector2(0.5f, 0.65f);
        slotContRect.anchorMax = new Vector2(0.5f, 0.85f);
        slotContRect.offsetMin = new Vector2(-400f, 0f);
        slotContRect.offsetMax = new Vector2(400f, 0f);

        // Roster Container
        GameObject rosterContainer = new GameObject("RosterContainer");
        rosterContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform rosterRect = rosterContainer.AddComponent<RectTransform>();
        rosterRect.anchorMin = new Vector2(0.05f, 0.1f);
        rosterRect.anchorMax = new Vector2(0.55f, 0.6f);
        rosterRect.offsetMin = Vector2.zero;
        rosterRect.offsetMax = Vector2.zero;

        // Stats Panel
        GameObject statsPanel = new GameObject("StatsPanel");
        statsPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.6f, 0.4f);
        statsRect.anchorMax = new Vector2(0.95f, 0.6f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;

        // Buttons
        GameObject confirmBtn = CreateButton(canvasObj.transform, "CONFIRM", "START DUNGEON");
        SetAnchors(confirmBtn.GetComponent<RectTransform>(), new Vector2(0.7f, 0.15f), new Vector2(0.9f, 0.25f));

        GameObject clearBtn = CreateButton(canvasObj.transform, "CLEAR", "CLEAR ALL");
        SetAnchors(clearBtn.GetComponent<RectTransform>(), new Vector2(0.7f, 0.05f), new Vector2(0.9f, 0.13f));

        GameObject backBtn = CreateButton(canvasObj.transform, "BACK", "BACK TO HUB");
        SetAnchors(backBtn.GetComponent<RectTransform>(), new Vector2(0.05f, 0.02f), new Vector2(0.25f, 0.08f));

        // FormationView component
        FormationView view = canvasObj.AddComponent<FormationView>();
        SerializedObject so = new SerializedObject(view);

        // Slot prefab
        GameObject slotPrefab = CreateSlotPrefab();
        so.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        so.FindProperty("slotContainer").objectReferenceValue = slotContRect;

        // Roster card prefab
        GameObject cardPrefab = CreateRosterCardPrefab();
        so.FindProperty("rosterCardPrefab").objectReferenceValue = cardPrefab;
        so.FindProperty("rosterListContainer").objectReferenceValue = rosterRect;

        // Stats texts
        so.FindProperty("totalHpText").objectReferenceValue = CreateStatText(statsPanel.transform, "HP", "HP: 0").GetComponent<TextMeshProUGUI>();
        so.FindProperty("totalAtkText").objectReferenceValue = CreateStatText(statsPanel.transform, "ATK", "ATK: 0").GetComponent<TextMeshProUGUI>();
        so.FindProperty("totalDefText").objectReferenceValue = CreateStatText(statsPanel.transform, "DEF", "DEF: 0").GetComponent<TextMeshProUGUI>();
        so.FindProperty("selectedCountText").objectReferenceValue = CreateStatText(statsPanel.transform, "COUNT", "0 / 5").GetComponent<TextMeshProUGUI>();

        // Buttons
        so.FindProperty("confirmButton").objectReferenceValue = confirmBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("clearButton").objectReferenceValue = clearBtn.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<UnityEngine.UI.Button>();

        so.ApplyModifiedProperties();

        // Save scene
        string path = "Assets/Scenes/Formation.unity";
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene), path);

        Debug.Log("[FormationSceneBuilder] Scene saved to " + path);
    }

    private static GameObject CreateSlotPrefab()
    {
        GameObject prefab = new GameObject("SlotPrefab");
        RectTransform rect = prefab.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(140f, 160f);

        UnityEngine.UI.Image img = prefab.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        UnityEngine.UI.Button btn = prefab.AddComponent<UnityEngine.UI.Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
        btn.colors = cb;

        // Portrait
        GameObject portrait = new GameObject("Portrait");
        portrait.transform.SetParent(prefab.transform, false);
        RectTransform pRect = portrait.AddComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.1f, 0.3f);
        pRect.anchorMax = new Vector2(0.9f, 0.9f);
        pRect.offsetMin = Vector2.zero;
        pRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image pImg = portrait.AddComponent<UnityEngine.UI.Image>();
        pImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Name
        GameObject nameObj = CreateText(prefab.transform, "Name", "Empty", 18, TextAlignmentOptions.Center);
        RectTransform nRect = nameObj.GetComponent<RectTransform>();
        nRect.anchorMin = new Vector2(0f, 0.05f);
        nRect.anchorMax = new Vector2(1f, 0.28f);
        nRect.offsetMin = Vector2.zero;
        nRect.offsetMax = Vector2.zero;

        // Row label
        GameObject rowObj = CreateText(prefab.transform, "RowLabel", "FRONT", 14, TextAlignmentOptions.Center);
        RectTransform rRect = rowObj.GetComponent<RectTransform>();
        rRect.anchorMin = new Vector2(0f, 0.88f);
        rRect.anchorMax = new Vector2(1f, 1f);
        rRect.offsetMin = Vector2.zero;
        rRect.offsetMax = Vector2.zero;
        TextMeshProUGUI rowTmp = rowObj.GetComponent<TextMeshProUGUI>();
        rowTmp.color = new Color(0.6f, 0.6f, 0.7f, 1f);

        return prefab;
    }

    private static GameObject CreateRosterCardPrefab()
    {
        GameObject prefab = new GameObject("RosterCardPrefab");
        RectTransform rect = prefab.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 150f);

        UnityEngine.UI.Image img = prefab.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        UnityEngine.UI.Button btn = prefab.AddComponent<UnityEngine.UI.Button>();

        // Portrait
        GameObject portrait = new GameObject("Portrait");
        portrait.transform.SetParent(prefab.transform, false);
        RectTransform pRect = portrait.AddComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.1f, 0.4f);
        pRect.anchorMax = new Vector2(0.9f, 0.9f);
        pRect.offsetMin = Vector2.zero;
        pRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image pImg = portrait.AddComponent<UnityEngine.UI.Image>();
        pImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Name
        GameObject nameObj = CreateText(prefab.transform, "Name", "Name", 16, TextAlignmentOptions.Center);
        SetAnchors(nameObj.GetComponent<RectTransform>(), new Vector2(0f, 0.22f), new Vector2(1f, 0.38f));

        // Class
        GameObject classObj = CreateText(prefab.transform, "Class", "Class", 14, TextAlignmentOptions.Center);
        SetAnchors(classObj.GetComponent<RectTransform>(), new Vector2(0f, 0.08f), new Vector2(1f, 0.22f));

        // Stars
        GameObject starsObj = CreateText(prefab.transform, "Stars", "★", 14, TextAlignmentOptions.Center);
        SetAnchors(starsObj.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.08f));

        return prefab;
    }

    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, 60f);

        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.25f, 0.35f, 0.55f, 1f);

        UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.4f, 0.5f, 0.7f, 1f);
        btn.colors = cb;

        GameObject textObj = CreateText(btnObj.transform, "Text", label, 20, TextAlignmentOptions.Center);
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        return btnObj;
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        return obj;
    }

    private static GameObject CreateStatText(Transform parent, string name, string text)
    {
        GameObject obj = CreateText(parent, name, text, 22, TextAlignmentOptions.Left);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 30f);
        return obj;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
