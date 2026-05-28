using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class CreateRosterHeroCardPrefab
{
    private static readonly Color GOLD_COLOR = new Color(0.95f, 0.75f, 0.1f, 1f);
    private static readonly Color DEAD_OVERLAY_COLOR = new Color(0f, 0f, 0f, 0.5f);
    private static readonly Color RED_COLOR = new Color(0.8f, 0f, 0f, 1f);

    [MenuItem("Tools/Pick Me Up/Create Roster Hero Card Prefab")]
    public static void Execute()
    {
        // Create directories if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "UI");
        }

        // Build the prefab hierarchy
        GameObject rootGo = BuildPrefabHierarchy();

        // Save as prefab asset
        string path = "Assets/Resources/UI/RosterHeroCardPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, path);

        if (prefab != null)
        {
            Debug.Log($"RosterHeroCardPrefab created successfully at: {path}");
            Selection.activeObject = prefab;
        }
        else
        {
            Debug.LogError($"Failed to create prefab at: {path}");
        }

        // Cleanup temporary GameObject
        Object.DestroyImmediate(rootGo);
    }

    private static GameObject BuildPrefabHierarchy()
    {
        // Root GameObject
        GameObject rootGo = new GameObject("RosterHeroCardPrefab");
        RectTransform rootRT = rootGo.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(380, 560);

        // Background image (will be colored at runtime based on rarity)
        Image bgImage = rootGo.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.35f, 1f); // Default gray placeholder

        // Add RosterHeroCard component
        RosterHeroCard rosterCard = rootGo.AddComponent<RosterHeroCard>();

        // 1. Portrait
        GameObject portrait = CreatePanel("Portrait", rootGo.transform, Color.white);
        RectTransform portraitRT = portrait.GetComponent<RectTransform>();
        portraitRT.anchorMin = new Vector2(0.5f, 1f);
        portraitRT.anchorMax = new Vector2(0.5f, 1f);
        portraitRT.offsetMin = new Vector2(-150, -320);
        portraitRT.offsetMax = new Vector2(150, -20);
        portraitRT.sizeDelta = Vector2.zero;
        portraitRT.pivot = new Vector2(0.5f, 1f);
        // Image already added by CreatePanel

        // 2. DeadOverlay
        GameObject deadOverlay = CreatePanel("DeadOverlay", rootGo.transform, DEAD_OVERLAY_COLOR);
        RectTransform deadRT = deadOverlay.GetComponent<RectTransform>();
        deadRT.anchorMin = Vector2.zero;
        deadRT.anchorMax = Vector2.one;
        deadRT.offsetMin = Vector2.zero;
        deadRT.offsetMax = Vector2.zero;
        deadOverlay.SetActive(false);

        // "DEAD" text inside DeadOverlay
        GameObject deadText = CreateTMP("DEAD", "DEAD", 48, deadOverlay.transform);
        RectTransform deadTextRT = deadText.GetComponent<RectTransform>();
        deadTextRT.anchorMin = Vector2.zero;
        deadTextRT.anchorMax = Vector2.one;
        deadTextRT.offsetMin = Vector2.zero;
        deadTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI deadTMP = deadText.GetComponent<TextMeshProUGUI>();
        deadTMP.color = RED_COLOR;
        deadTMP.fontStyle = FontStyles.Bold;

        // 3. NameLabel
        GameObject nameLabel = CreateTMP("NameLabel", "Hero Name", 28, rootGo.transform);
        RectTransform nameRT = nameLabel.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 1f);
        nameRT.anchorMax = new Vector2(1, 1f);
        nameRT.offsetMin = new Vector2(10, -480);
        nameRT.offsetMax = new Vector2(-10, -430);
        nameRT.sizeDelta = new Vector2(-20, 50);
        nameRT.pivot = new Vector2(0.5f, 1f);
        TextMeshProUGUI nameTMP = nameLabel.GetComponent<TextMeshProUGUI>();
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.textWrappingMode = TextWrappingModes.Normal;

        // 4. StarLabel
        GameObject starLabel = CreateTMP("StarLabel", "★★★★★", 22, rootGo.transform);
        RectTransform starRT = starLabel.GetComponent<RectTransform>();
        starRT.anchorMin = new Vector2(0, 1f);
        starRT.anchorMax = new Vector2(1, 1f);
        starRT.offsetMin = new Vector2(10, -540);
        starRT.offsetMax = new Vector2(-10, -495);
        starRT.sizeDelta = new Vector2(-20, 45);
        starRT.pivot = new Vector2(0.5f, 1f);
        TextMeshProUGUI starTMP = starLabel.GetComponent<TextMeshProUGUI>();
        starTMP.color = GOLD_COLOR;
        starTMP.alignment = TextAlignmentOptions.Center;

        // 5. TraitBadge
        GameObject traitBadge = CreatePanel("TraitBadge", rootGo.transform, new Color(0.3f, 0.5f, 0.8f, 1f));
        RectTransform traitRT = traitBadge.GetComponent<RectTransform>();
        traitRT.anchorMin = new Vector2(1f, 1f);
        traitRT.anchorMax = new Vector2(1f, 1f);
        traitRT.offsetMin = new Vector2(-110, -56);
        traitRT.offsetMax = new Vector2(-20, -20);
        traitRT.sizeDelta = Vector2.zero;
        traitRT.pivot = Vector2.one;

        // Trait text inside badge
        GameObject traitText = CreateTMP("TraitText", "Trait", 18, traitBadge.transform);
        RectTransform traitTextRT = traitText.GetComponent<RectTransform>();
        traitTextRT.anchorMin = Vector2.zero;
        traitTextRT.anchorMax = Vector2.one;
        traitTextRT.offsetMin = Vector2.zero;
        traitTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI traitTMP = traitText.GetComponent<TextMeshProUGUI>();
        traitTMP.color = Color.white;
        traitTMP.alignment = TextAlignmentOptions.Center;

        return rootGo;
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