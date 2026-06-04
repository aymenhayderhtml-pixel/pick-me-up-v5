using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class CreateRosterHeroCardPrefab
{
    [MenuItem("Tools/Pick Me Up/Create Roster Hero Card Prefab")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/UI"))
            AssetDatabase.CreateFolder("Assets/Resources", "UI");

        GameObject rootGo = BuildPrefabHierarchy();
        string path = "Assets/Resources/UI/RosterHeroCardPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, path);

        if (prefab != null)
        {
            Debug.Log($"RosterHeroCardPrefab created at: {path}");
            Selection.activeObject = prefab;
        }
        else Debug.LogError($"Failed to create prefab at: {path}");

        Object.DestroyImmediate(rootGo);
    }

    private static GameObject BuildPrefabHierarchy()
    {
        // ── Root 360×420 ─────────────────────────────────────────────
        GameObject rootGo = new GameObject("RosterHeroCardPrefab");
        RectTransform rootRT = rootGo.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(360, 420);

        // Card background — clearly visible so cards stand out individually
        Image cardBg = rootGo.AddComponent<Image>();
        cardBg.color = new Color(0.35f, 0.40f, 0.50f);
        cardBg.raycastTarget = true;

        // RosterHeroCard component
        RosterHeroCard rosterCard = rootGo.AddComponent<RosterHeroCard>();

        // Button on the root for clickability
        Button btn = rootGo.AddComponent<Button>();
        btn.targetGraphic = cardBg;

        // ── Frame (rarity border) ─────────────────────────────────────
        GameObject frameG = new GameObject("Frame", typeof(RectTransform), typeof(Image));
        frameG.transform.SetParent(rootGo.transform, false);
        RectTransform fRt = frameG.GetComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.offsetMin = new Vector2(3, 3);
        fRt.offsetMax = new Vector2(-3, -3);
        Image frameImg = frameG.GetComponent<Image>();
        frameImg.color = new Color(0.55f, 0.55f, 0.55f, 0.70f);
        frameImg.raycastTarget = false;

        // ── Portrait area ─────────────────────────────────────────────
        GameObject portraitG = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portraitG.transform.SetParent(rootGo.transform, false);
        RectTransform pRt = portraitG.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.06f, 0.34f);
        pRt.anchorMax = new Vector2(0.94f, 0.94f);
        pRt.offsetMin = new Vector2(4, 4);
        pRt.offsetMax = new Vector2(-4, -4);
        Image portraitImg = portraitG.GetComponent<Image>();
        portraitImg.color = new Color(0.55f, 0.60f, 0.70f);
        portraitImg.raycastTarget = false;

        // ── Dead overlay (hidden) ─────────────────────────────────────
        GameObject deadG = new GameObject("DeadOverlay", typeof(RectTransform), typeof(Image));
        deadG.transform.SetParent(rootGo.transform, false);
        RectTransform ddRt = deadG.GetComponent<RectTransform>();
        ddRt.anchorMin = Vector2.zero;
        ddRt.anchorMax = Vector2.one;
        ddRt.offsetMin = Vector2.zero;
        ddRt.offsetMax = Vector2.zero;
        Image deadImg = deadG.GetComponent<Image>();
        deadImg.color = new Color(0f, 0f, 0f, 0.50f);
        deadImg.raycastTarget = false;
        deadG.SetActive(false);

        // DEAD label inside overlay
        GameObject deadLabelG = new GameObject("DEAD", typeof(RectTransform), typeof(TextMeshProUGUI));
        deadLabelG.transform.SetParent(deadG.transform, false);
        RectTransform dlRt = deadLabelG.GetComponent<RectTransform>();
        dlRt.anchorMin = Vector2.zero;
        dlRt.anchorMax = Vector2.one;
        dlRt.offsetMin = Vector2.zero;
        dlRt.offsetMax = Vector2.zero;
        TextMeshProUGUI deadTmp = deadLabelG.GetComponent<TextMeshProUGUI>();
        deadTmp.text = "DEAD";
        deadTmp.fontSize = 48;
        deadTmp.color = new Color(0.8f, 0f, 0f);
        deadTmp.alignment = TextAlignmentOptions.Center;
        deadTmp.fontStyle = FontStyles.Bold;

        // ── Trait badge (top-right) ───────────────────────────────────
        GameObject traitG = new GameObject("TraitBadge", typeof(RectTransform), typeof(Image));
        traitG.transform.SetParent(rootGo.transform, false);
        RectTransform trRt = traitG.GetComponent<RectTransform>();
        trRt.anchorMin = new Vector2(1, 1);
        trRt.anchorMax = new Vector2(1, 1);
        trRt.pivot = Vector2.one;
        trRt.offsetMin = new Vector2(-56, -34);
        trRt.offsetMax = new Vector2(-6, -6);
        Image traitImg = traitG.GetComponent<Image>();
        traitImg.color = new Color(0.30f, 0.50f, 0.80f);
        traitImg.raycastTarget = false;

        GameObject traitTextG = new GameObject("TraitText", typeof(RectTransform), typeof(TextMeshProUGUI));
        traitTextG.transform.SetParent(traitG.transform, false);
        RectTransform ttRt = traitTextG.GetComponent<RectTransform>();
        ttRt.anchorMin = Vector2.zero;
        ttRt.anchorMax = Vector2.one;
        ttRt.offsetMin = Vector2.zero;
        ttRt.offsetMax = Vector2.zero;
        TextMeshProUGUI traitTmp = traitTextG.GetComponent<TextMeshProUGUI>();
        traitTmp.text = "B";
        traitTmp.fontSize = 28;
        traitTmp.color = Color.white;
        traitTmp.alignment = TextAlignmentOptions.Center;

        // ── Name label (bottom-left) ──────────────────────────────────
        GameObject nameG = new GameObject("NameLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameG.transform.SetParent(rootGo.transform, false);
        RectTransform nrRt = nameG.GetComponent<RectTransform>();
        nrRt.anchorMin = new Vector2(0.06f, 0.04f);
        nrRt.anchorMax = new Vector2(0.94f, 0.32f);
        nrRt.offsetMin = Vector2.zero;
        nrRt.offsetMax = Vector2.zero;
        TextMeshProUGUI nameTmp = nameG.GetComponent<TextMeshProUGUI>();
        nameTmp.text = "HERO";
        nameTmp.fontSize = 26;
        nameTmp.color = Color.white;
        nameTmp.alignment = TextAlignmentOptions.BottomLeft;
        nameTmp.textWrappingMode = TextWrappingModes.Normal;

        // ── Stars container (between portrait and name) ───────────────
        GameObject starsG = new GameObject("StarsContainer", typeof(RectTransform));
        starsG.transform.SetParent(rootGo.transform, false);
        RectTransform srRt2 = starsG.GetComponent<RectTransform>();
        srRt2.anchorMin = new Vector2(0.06f, 0.28f);
        srRt2.anchorMax = new Vector2(0.94f, 0.34f);
        srRt2.offsetMin = new Vector2(0, 2);
        srRt2.offsetMax = new Vector2(0, -2);

        // Add a star text as fallback (also set at runtime)
        GameObject starTextG = new GameObject("StarLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        starTextG.transform.SetParent(starsG.transform, false);
        RectTransform stRt = starTextG.GetComponent<RectTransform>();
        stRt.anchorMin = Vector2.zero;
        stRt.anchorMax = Vector2.one;
        stRt.offsetMin = Vector2.zero;
        stRt.offsetMax = Vector2.zero;
        TextMeshProUGUI starTmp = starTextG.GetComponent<TextMeshProUGUI>();
        starTmp.text = "[*][*]";
        starTmp.fontSize = 20;
        starTmp.color = new Color(0.95f, 0.75f, 0.10f);
        starTmp.alignment = TextAlignmentOptions.Left;

        // ── Selection border (hidden) ─────────────────────────────────
        GameObject borderG = new GameObject("SelectionBorder", typeof(RectTransform), typeof(Image));
        borderG.transform.SetParent(rootGo.transform, false);
        RectTransform brRt = borderG.GetComponent<RectTransform>();
        brRt.anchorMin = Vector2.zero;
        brRt.anchorMax = Vector2.one;
        brRt.offsetMin = Vector2.zero;
        brRt.offsetMax = Vector2.zero;
        Image borderImg = borderG.GetComponent<Image>();
        borderImg.color = new Color(0.20f, 0.50f, 0.90f, 0.35f);
        borderImg.raycastTarget = false;
        borderG.SetActive(false);

        return rootGo;
    }
}
