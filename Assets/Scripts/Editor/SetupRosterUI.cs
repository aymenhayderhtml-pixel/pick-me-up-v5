using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class SetupRosterUI
{
    [MenuItem("Tools/Pick Me Up/Setup Roster UI")]
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGo;
        if (canvas == null)
        {
            canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGo = canvas.gameObject;
        }

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2340);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        GameObject bgPanel = FindOrCreateChild(canvasGo.transform, "Background", () =>
        {
            GameObject go = CreatePanel("Background", canvasGo.transform, new Color(0.039f, 0.039f, 0.078f, 1f));
            Stretch(go);
            return go;
        });
        bgPanel.transform.SetAsFirstSibling();

        RosterView rosterView = canvasGo.GetComponent<RosterView>();
        if (rosterView == null)
            rosterView = canvasGo.AddComponent<RosterView>();

        // ── TopBar ────────────────────────────────────────────────────────
        GameObject topBar = FindOrCreateChild(canvasGo.transform, "TopBar", () =>
            CreatePanel("TopBar", canvasGo.transform, new Color(0.059f, 0.059f, 0.098f, 1f)));
        SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1));
        topBar.GetComponent<RectTransform>().offsetMin = new Vector2(0, -180);
        topBar.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

        GameObject backBtn = FindOrCreateChild(topBar.transform, "BackBtn", () =>
            CreateButton("BackBtn", "←", topBar.transform, new Color(0.2f, 0.2f, 0.2f, 1f), 48));
        SetAnchors(backBtn, new Vector2(0, 0), new Vector2(0, 1));
        backBtn.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20);
        backBtn.GetComponent<RectTransform>().offsetMax = new Vector2(120, -20);

        GameObject goldLabel = FindOrCreateChild(topBar.transform, "GoldLabel", () =>
            CreateTMP("GoldLabel", "Gold: 0", 32, topBar.transform));
        SetAnchors(goldLabel, new Vector2(0.25f, 0), new Vector2(0.45f, 1));
        goldLabel.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
        goldLabel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);

        GameObject gemsLabel = FindOrCreateChild(topBar.transform, "GemsLabel", () =>
            CreateTMP("GemsLabel", "Gems: 0", 32, topBar.transform));
        SetAnchors(gemsLabel, new Vector2(0.45f, 0), new Vector2(0.65f, 1));
        gemsLabel.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
        gemsLabel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);

        GameObject stonesLabel = FindOrCreateChild(topBar.transform, "StonesLabel", () =>
            CreateTMP("StonesLabel", "Stones: 0", 32, topBar.transform));
        SetAnchors(stonesLabel, new Vector2(0.65f, 0), new Vector2(0.85f, 1));
        stonesLabel.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
        stonesLabel.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);

        // ── FilterBar ─────────────────────────────────────────────────────
        GameObject filterBar = FindOrCreateChild(canvasGo.transform, "FilterBar", () =>
            CreatePanel("FilterBar", canvasGo.transform, new Color(0.078f, 0.078f, 0.122f, 1f)));
        SetAnchors(filterBar, new Vector2(0, 1), new Vector2(0.45f, 1));
        filterBar.GetComponent<RectTransform>().offsetMin = new Vector2(0, -280);
        filterBar.GetComponent<RectTransform>().offsetMax = new Vector2(0, -180);

        string[] classOptions = System.Enum.GetNames(typeof(HeroClass));
        TMP_Dropdown classDropdown = FindOrCreateComponent<TMP_Dropdown>(filterBar.transform, "ClassDropdown", () =>
            CreateTMPDropdown("ClassDropdown", filterBar.transform, classOptions));
        if (classDropdown != null)
        {
            SetAnchors(classDropdown.gameObject, new Vector2(0, 0.5f), new Vector2(0.28f, 1f));
            classDropdown.GetComponent<RectTransform>().offsetMin = new Vector2(10, 5);
            classDropdown.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);
            ApplyDarkDropdownStyle(classDropdown);
        }

        Toggle aliveToggle = FindOrCreateComponent<Toggle>(filterBar.transform, "AliveToggle", () =>
            CreateTMPToggle("AliveToggle", "Alive", filterBar.transform));
        if (aliveToggle != null)
        {
            SetAnchors(aliveToggle.gameObject, new Vector2(0.32f, 0.5f), new Vector2(0.52f, 1f));
            aliveToggle.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
            aliveToggle.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);
            FixToggleDark(aliveToggle);
        }

        string[] sortOptions = new string[] { "Name", "Class", "Level", "Stars", "Morale" };
        TMP_Dropdown sortDropdown = FindOrCreateComponent<TMP_Dropdown>(filterBar.transform, "SortDropdown", () =>
            CreateTMPDropdown("SortDropdown", filterBar.transform, sortOptions));
        if (sortDropdown != null)
        {
            SetAnchors(sortDropdown.gameObject, new Vector2(0.56f, 0.5f), new Vector2(0.76f, 1f));
            sortDropdown.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
            sortDropdown.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);
            ApplyDarkDropdownStyle(sortDropdown);
        }

        GameObject heroCount = FindOrCreateChild(filterBar.transform, "HeroCount", () =>
            CreateTMP("HeroCount", "Heroes: 0", 24, filterBar.transform));
        SetAnchors(heroCount, new Vector2(0, 0), new Vector2(1, 0.5f));
        heroCount.GetComponent<RectTransform>().offsetMin = new Vector2(10, 5);
        heroCount.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);

        // ── Hero Grid ScrollRect ───────────────────────────────────────────
        GameObject scrollRectGo = FindOrCreateChild(canvasGo.transform, "HeroGridScrollRect", () =>
        {
            var sr = CreateUIViaMenu<ScrollRect>("GameObject/UI/Scroll View", "HeroGridScrollRect", canvasGo.transform);
            return sr != null ? sr.gameObject : null;
        });

        ScrollRect scrollRect = null;
        Transform content = null;
        if (scrollRectGo != null)
        {
            scrollRect = scrollRectGo.GetComponent<ScrollRect>();
            SetAnchors(scrollRectGo, new Vector2(0, 0), new Vector2(0.45f, 1));
            scrollRectGo.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            scrollRectGo.GetComponent<RectTransform>().offsetMax = new Vector2(0, -280);

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            Transform viewport = scrollRectGo.transform.Find("Viewport");
            if (viewport != null)
            {
                Image vpImg = viewport.GetComponent<Image>();
                if (vpImg == null) vpImg = viewport.gameObject.AddComponent<Image>();
                vpImg.color = new Color(0.05f, 0.05f, 0.08f, 0.6f);

                Mask mask = viewport.GetComponent<Mask>();
                if (mask == null) mask = viewport.gameObject.AddComponent<Mask>();

                scrollRect.viewport = viewport.GetComponent<RectTransform>();
            }

            content = scrollRectGo.transform.Find("Viewport/Content");
            if (content != null)
            {
                GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
                if (grid == null) grid = content.gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(380, 560);
                grid.spacing = new Vector2(30, 30);
                grid.padding = new RectOffset(25, 25, 25, 25);
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                grid.childAlignment = TextAnchor.UpperCenter;
                grid.constraint = GridLayoutGroup.Constraint.Flexible;

                ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
                if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                scrollRect.content = content.GetComponent<RectTransform>();
            }

            Transform hScrollbar = scrollRectGo.transform.Find("Scrollbar Horizontal");
            if (hScrollbar != null) Object.DestroyImmediate(hScrollbar.gameObject);
            scrollRect.horizontalScrollbar = null;

            Transform vScrollbar = scrollRectGo.transform.Find("Scrollbar Vertical");
            if (vScrollbar != null)
            {
                scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
                FixScrollbarDark(vScrollbar);
            }
        }

        // ── Detail Panel ───────────────────────────────────────────────────
        GameObject detailPanel = FindOrCreateChild(canvasGo.transform, "DetailPanel", () =>
            CreatePanel("DetailPanel", canvasGo.transform, new Color(0.102f, 0.102f, 0.180f, 0.95f)));
        SetAnchors(detailPanel, new Vector2(0.45f, 0), new Vector2(1, 1));
        detailPanel.SetActive(false);

        GameObject closeBtn = FindOrCreateChild(detailPanel.transform, "CloseBtn", () =>
            CreateButton("CloseBtn", "X", detailPanel.transform, new Color(0.8f, 0.2f, 0.2f, 1f), 36));
        SetAnchors(closeBtn, new Vector2(0.88f, 0.94f), new Vector2(1, 1));
        closeBtn.GetComponent<RectTransform>().offsetMin = new Vector2(0, 10);
        closeBtn.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);

        GameObject portrait = FindOrCreateChild(detailPanel.transform, "Portrait", () =>
            CreatePanel("Portrait", detailPanel.transform, Color.white));
        SetAnchors(portrait, new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.92f));

        GameObject detailName = FindOrCreateChild(detailPanel.transform, "Name", () =>
        {
            GameObject go = CreateTMP("Name", "HERO NAME", 46, detailPanel.transform);
            go.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            return go;
        });
        SetAnchors(detailName, new Vector2(0.1f, 0.54f), new Vector2(0.9f, 0.60f));

        GameObject detailClass = FindOrCreateChild(detailPanel.transform, "Class", () =>
            CreateTMP("Class", "CLASS", 32, detailPanel.transform));
        SetAnchors(detailClass, new Vector2(0.1f, 0.49f), new Vector2(0.9f, 0.54f));

        GameObject detailStars = FindOrCreateChild(detailPanel.transform, "Stars", () =>
            CreateTMP("Stars", "★★★★★", 32, detailPanel.transform));
        SetAnchors(detailStars, new Vector2(0.1f, 0.44f), new Vector2(0.9f, 0.49f));

        GameObject detailEquipment = FindOrCreateChild(detailPanel.transform, "Equipment", () =>
        {
            GameObject go = CreateTMP("Equipment", "Weapon: None\nArmor: None\nAccessory: None\nStatus: Unlocked", 24, detailPanel.transform);
            go.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            return go;
        });
        SetAnchors(detailEquipment, new Vector2(0.1f, 0.30f), new Vector2(0.9f, 0.44f));

        GameObject traitBadgeBg = FindOrCreateChild(detailPanel.transform, "TraitBadgeBg", () =>
            CreatePanel("TraitBadgeBg", detailPanel.transform, new Color(0.3f, 0.3f, 0.5f, 1f)));
        SetAnchors(traitBadgeBg, new Vector2(0.25f, 0.46f), new Vector2(0.75f, 0.52f));

        GameObject traitText = FindOrCreateChild(traitBadgeBg.transform, "TraitText", () =>
            CreateTMP("TraitText", "TRAIT", 28, traitBadgeBg.transform));
        Stretch(traitText);

        GameObject moraleLabel = FindOrCreateChild(detailPanel.transform, "MoraleLabel", () =>
            CreateTMP("MoraleLabel", "Morale:", 28, detailPanel.transform));
        SetAnchors(moraleLabel, new Vector2(0.1f, 0.22f), new Vector2(0.35f, 0.28f));

        GameObject moraleSliderGo = FindOrCreateChild(detailPanel.transform, "MoraleSlider", () =>
        {
            var s = CreateUIViaMenu<Slider>("GameObject/UI/Slider", "MoraleSlider", detailPanel.transform);
            return s != null ? s.gameObject : null;
        });
        Slider moraleSlider = null;
        if (moraleSliderGo != null)
        {
            moraleSlider = moraleSliderGo.GetComponent<Slider>();
            SetAnchors(moraleSliderGo, new Vector2(0.38f, 0.22f), new Vector2(0.9f, 0.28f));
            FixSliderDark(moraleSlider);
        }

        GameObject potentialText = FindOrCreateChild(detailPanel.transform, "Potential", () =>
        {
            GameObject go = CreateTMP("Potential", "Flavor text describing hero potential...", 24, detailPanel.transform);
            go.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            return go;
        });
        SetAnchors(potentialText, new Vector2(0.1f, 0.10f), new Vector2(0.9f, 0.20f));

        GameObject promoteBtn = FindOrCreateChild(detailPanel.transform, "PromoteBtn", () =>
            CreateButton("PromoteBtn", "PROMOTE", detailPanel.transform, new Color(0.2f, 0.667f, 0.2f, 1f), 36));
        SetAnchors(promoteBtn, new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.09f));

        GameObject promoteCost = FindOrCreateChild(detailPanel.transform, "Cost", () =>
            CreateTMP("Cost", "X Stones", 28, detailPanel.transform));
        SetAnchors(promoteCost, new Vector2(0.1f, 0), new Vector2(0.9f, 0.03f));

        GameObject synthesizeBtn = FindOrCreateChild(detailPanel.transform, "SynthesizeBtn", () =>
            CreateButton("SynthesizeBtn", "SYNTHESIZE", detailPanel.transform, new Color(0.8f, 0.2f, 0.2f, 1f), 36));
        SetAnchors(synthesizeBtn, new Vector2(0.1f, 0), new Vector2(0.9f, 0.02f));

        // ── Synth Warning Panel ────────────────────────────────────────────
        GameObject synthPanel = FindOrCreateChild(canvasGo.transform, "SynthWarningPanel", () =>
            CreatePanel("SynthWarningPanel", canvasGo.transform, new Color(0, 0, 0, 0.9f)));
        SetAnchors(synthPanel, new Vector2(0, 0), new Vector2(1, 1));
        synthPanel.SetActive(false);

        GameObject warningText = FindOrCreateChild(synthPanel.transform, "WarningText", () =>
        {
            GameObject go = CreateTMP("WarningText", "WARNING: Synthesis kills the hero permanently. Proceed?", 40, synthPanel.transform);
            go.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            return go;
        });
        SetAnchors(warningText, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f));

        GameObject confirmBtn = FindOrCreateChild(synthPanel.transform, "ConfirmBtn", () =>
            CreateButton("ConfirmBtn", "CONFIRM", synthPanel.transform, new Color(0.8f, 0.2f, 0.2f, 1f), 36));
        SetAnchors(confirmBtn, new Vector2(0.2f, 0.35f), new Vector2(0.45f, 0.45f));

        GameObject cancelBtn = FindOrCreateChild(synthPanel.transform, "CancelBtn", () =>
            CreateButton("CancelBtn", "CANCEL", synthPanel.transform, new Color(0.4f, 0.4f, 0.4f, 1f), 36));
        SetAnchors(cancelBtn, new Vector2(0.55f, 0.35f), new Vector2(0.8f, 0.45f));

        // ── Wire RosterView via SerializedObject ───────────────────────────
        SerializedObject so = new SerializedObject(rosterView);

        so.FindProperty("goldLabel").objectReferenceValue = goldLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("gemsLabel").objectReferenceValue = gemsLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("stonesLabel").objectReferenceValue = stonesLabel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("heroCountLabel").objectReferenceValue = heroCount.GetComponent<TextMeshProUGUI>();
        so.FindProperty("backBtn").objectReferenceValue = backBtn.GetComponent<Button>();
        so.FindProperty("classDropdown").objectReferenceValue = classDropdown;
        so.FindProperty("aliveDeadToggle").objectReferenceValue = aliveToggle;
        so.FindProperty("sortDropdown").objectReferenceValue = sortDropdown;

        if (content != null)
            so.FindProperty("gridContent").objectReferenceValue = content;

        so.FindProperty("detailPanel").objectReferenceValue = detailPanel;
        so.FindProperty("detailPanelRT").objectReferenceValue = detailPanel.GetComponent<RectTransform>();
        so.FindProperty("detailPortrait").objectReferenceValue = portrait.GetComponent<Image>();
        so.FindProperty("detailName").objectReferenceValue = detailName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailClass").objectReferenceValue = detailClass.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailTraitBadgeText").objectReferenceValue = traitText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailTraitBadgeBg").objectReferenceValue = traitBadgeBg.GetComponent<Image>();
        so.FindProperty("detailMoraleBar").objectReferenceValue = moraleSlider;
        so.FindProperty("detailPotentialText").objectReferenceValue = potentialText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailEquipmentText").objectReferenceValue = detailEquipment.GetComponent<TextMeshProUGUI>();
        so.FindProperty("detailStars").objectReferenceValue = detailStars.GetComponent<TextMeshProUGUI>();
        so.FindProperty("promoteBtn").objectReferenceValue = promoteBtn.GetComponent<Button>();
        so.FindProperty("promoteCostText").objectReferenceValue = promoteCost.GetComponent<TextMeshProUGUI>();
        so.FindProperty("synthesizeBtn").objectReferenceValue = synthesizeBtn.GetComponent<Button>();
        so.FindProperty("detailCloseBtn").objectReferenceValue = closeBtn.GetComponent<Button>();
        so.FindProperty("synthWarningPanel").objectReferenceValue = synthPanel;
        so.FindProperty("synthWarningText").objectReferenceValue = warningText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("synthConfirmBtn").objectReferenceValue = confirmBtn.GetComponent<Button>();
        so.FindProperty("synthCancelBtn").objectReferenceValue = cancelBtn.GetComponent<Button>();

        GameObject heroCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/RosterHeroCardPrefab.prefab");
        if (heroCardPrefab != null)
            so.FindProperty("heroCardPrefab").objectReferenceValue = heroCardPrefab;

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(canvasGo);
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[SetupRosterUI] Roster UI setup complete.");
    }

    #region FindOrCreate helpers

    private static GameObject FindOrCreateChild(Transform parent, string name, System.Func<GameObject> factory)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
                return parent.GetChild(i).gameObject;
        }
        GameObject go = factory();
        if (go != null) go.name = name;
        return go;
    }

    private static T FindOrCreateComponent<T>(Transform parent, string name, System.Func<T> factory) where T : Component
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
            {
                T comp = parent.GetChild(i).GetComponent<T>();
                if (comp != null) return comp;
            }
        }
        return factory();
    }

    private static T CreateUIViaMenu<T>(string menuPath, string name, Transform parent) where T : Component
    {
        var prevSelection = Selection.activeGameObject;
        Selection.activeGameObject = parent.gameObject;

        var before = new HashSet<T>(Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        EditorApplication.ExecuteMenuItem(menuPath);

        T result = null;
        GameObject resultGo = null;

        if (Selection.activeGameObject != null && Selection.activeGameObject != parent.gameObject)
        {
            result = Selection.activeGameObject.GetComponent<T>();
            if (result != null) resultGo = Selection.activeGameObject;
        }

        if (result == null)
        {
            var after = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var comp in after)
            {
                if (!before.Contains(comp))
                {
                    result = comp;
                    resultGo = comp.gameObject;
                    break;
                }
            }
        }

        if (resultGo != null)
        {
            resultGo.name = name;
            if (resultGo.transform.parent != parent)
                resultGo.transform.SetParent(parent, false);
        }
        else
        {
            Debug.LogWarning($"[SetupRosterUI] Failed to create {typeof(T).Name} via menu: {menuPath}");
        }

        Selection.activeGameObject = prevSelection;
        return result;
    }

    private static TMP_Dropdown CreateTMPDropdown(string name, Transform parent, string[] options)
    {
        var dd = CreateUIViaMenu<TMP_Dropdown>("GameObject/UI/Dropdown - TextMeshPro", name, parent);
        if (dd != null)
        {
            dd.options.Clear();
            foreach (var opt in options)
                dd.options.Add(new TMP_Dropdown.OptionData(opt));
        }
        return dd;
    }

    private static Toggle CreateTMPToggle(string name, string label, Transform parent)
    {
        var toggle = CreateUIViaMenu<Toggle>("GameObject/UI/Toggle", name, parent);
        if (toggle != null)
        {
            var labelTmp = toggle.GetComponentInChildren<TextMeshProUGUI>(true);
            if (labelTmp != null)
            {
                labelTmp.text = label;
                labelTmp.color = Color.white;
            }
            else
            {
                var legacyText = toggle.GetComponentInChildren<Text>(true);
                if (legacyText != null)
                {
                    Object.DestroyImmediate(legacyText.gameObject);
                    GameObject newLabel = CreateTMP("Label", label, 28, toggle.transform);
                    Stretch(newLabel);
                }
            }
        }
        return toggle;
    }

    #endregion

    #region Dark styling helpers

    private static void ApplyDarkDropdownStyle(TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;

        GameObject dropdownGo = dropdown.gameObject;

        var template = dropdown.transform.Find("Template");
        if (template != null)
        {
            FixAllImages(template.gameObject);
        }

        FixDropdownCaption(dropdownGo);

        var bg = dropdownGo.transform.Find("Background");
        if (bg != null)
        {
            var img = bg.GetComponent<Image>();
            if (img != null) img.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        }

        var arrow = dropdownGo.transform.Find("Arrow");
        if (arrow != null)
        {
            var img = arrow.GetComponent<Image>();
            if (img != null) img.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        }
    }

    private static void FixAllImages(GameObject root)
    {
        var imgs = root.GetComponentsInChildren<Image>(true);
        foreach (var img in imgs)
        {
            if (img.gameObject.name.Contains("Item Background"))
                img.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            else if (img.gameObject.name.Contains("Checkmark"))
                img.color = new Color(0.95f, 0.75f, 0.1f, 1f);
            else if (img.gameObject.name.Contains("Viewport"))
                img.color = new Color(0.05f, 0.05f, 0.1f, 1f);
            else if (img.gameObject.name.Contains("Handle"))
                img.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            else if (img.gameObject.name.Contains("Scrollbar") && !img.gameObject.name.Contains("Handle"))
                img.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            else if (img.gameObject.name.Contains("Template") || img.gameObject.name.Contains("Content"))
                img.color = new Color(0.08f, 0.08f, 0.12f, 1f);
        }

        var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in texts)
            tmp.color = Color.white;
    }

    private static void FixDropdownCaption(GameObject dropdownGo)
    {
        var bg = dropdownGo.transform.Find("Background") ?? dropdownGo.transform.Find("Template")?.parent?.Find("Background");
        if (bg != null)
        {
            var img = bg.GetComponent<Image>();
            if (img != null) img.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        }

        var label = dropdownGo.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.color = Color.white;
    }

    private static void FixToggleDark(Toggle toggle)
    {
        if (toggle == null) return;

        var bg = toggle.transform.Find("Background");
        if (bg != null)
        {
            var img = bg.GetComponent<Image>();
            if (img != null) img.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var checkmark = bg.Find("Checkmark");
            if (checkmark != null)
            {
                var checkImg = checkmark.GetComponent<Image>();
                if (checkImg != null) checkImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            }
        }

        var label = toggle.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null) label.color = Color.white;
    }

    private static void FixSliderDark(Slider slider)
    {
        if (slider == null) return;

        var images = slider.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name == "Background")
                img.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            else if (img.gameObject.name == "Fill")
                img.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            else if (img.gameObject.name == "Handle")
                img.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        }
    }

    private static void FixScrollbarDark(Transform scrollbar)
    {
        if (scrollbar == null) return;
        var images = scrollbar.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name.Contains("Handle"))
                img.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            else if (img.gameObject.name.Contains("Scrollbar") && !img.gameObject.name.Contains("Handle"))
                img.color = new Color(0.08f, 0.08f, 0.12f, 1f);
        }
    }

    #endregion

    #region Core UI helpers

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
