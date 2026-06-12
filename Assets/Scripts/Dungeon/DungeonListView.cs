using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonListView : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform listContent;

    [Header("Detail UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button startButton;

    private readonly List<Button> spawnedButtons = new List<Button>();
    private IDungeonService dungeonService;
    private DungeonDataSO selectedDungeon;

    public DungeonDataSO SelectedDungeon => selectedDungeon;

    private void Start()
    {
        if (ServiceRegistry.Instance == null || !ServiceRegistry.Instance.HasService<IDungeonService>())
        {
            return;
        }

        dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartClicked);
            startButton.onClick.AddListener(OnStartClicked);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (dungeonService == null)
        {
            return;
        }

        ClearButtons();

        List<DungeonDataSO> dungeons = dungeonService.GetAllDungeons();
        if (dungeons == null || dungeons.Count == 0)
        {
            SetDetails(null);
            return;
        }

        foreach (DungeonDataSO dungeon in dungeons.Where(d => d != null))
        {
            Button button = CreateButton(dungeon);
            spawnedButtons.Add(button);
        }

        if (selectedDungeon == null || dungeonService.GetDungeon(selectedDungeon.Id) == null)
        {
            selectedDungeon = dungeons[0];
        }

        SetDetails(selectedDungeon);
    }

    private Button CreateButton(DungeonDataSO dungeon)
    {
        GameObject buttonObj = new GameObject(dungeon.DisplayName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(listContent, false);
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 180);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);

        Image bg = buttonObj.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.16f, 0.96f);

        // Left icon slot
        GameObject iconFrame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
        iconFrame.transform.SetParent(buttonObj.transform, false);
        Image frameImg = iconFrame.GetComponent<Image>();
        frameImg.color = new Color(0.22f, 0.22f, 0.28f, 1f);
        RectTransform frameRt = iconFrame.GetComponent<RectTransform>();
        frameRt.anchorMin = new Vector2(0.03f, 0.10f);
        frameRt.anchorMax = new Vector2(0.21f, 0.90f);
        frameRt.offsetMin = Vector2.zero;
        frameRt.offsetMax = Vector2.zero;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(iconFrame.transform, false);
        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.color = new Color(0.30f, 0.32f, 0.40f, 1f);
        iconImg.preserveAspect = true;
        if (dungeon.Icon != null)
        {
            iconImg.sprite = dungeon.Icon;
        }
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = Vector2.zero;
        iconRt.anchorMax = Vector2.one;
        iconRt.offsetMin = new Vector2(6, 6);
        iconRt.offsetMax = new Vector2(-6, -6);

        Button button = buttonObj.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        DungeonDataSO captured = dungeon;
        button.onClick.AddListener(() =>
        {
            selectedDungeon = captured;
            SetDetails(captured);
        });

        TextMeshProUGUI name = CreateTMP("Name", dungeon.DisplayName, 26f, buttonObj.transform, new Vector2(0.24f, 0.55f), new Vector2(0.97f, 0.92f));
        name.alignment = TextAlignmentOptions.Left;
        name.fontStyle = FontStyles.Bold;

        TextMeshProUGUI meta = CreateTMP("Meta",
            "SP " + dungeon.StaminaCost + "  |  Waves " + dungeon.TotalWaves,
            16f, buttonObj.transform, new Vector2(0.24f, 0.18f), new Vector2(0.97f, 0.52f));
        meta.alignment = TextAlignmentOptions.Left;
        meta.color = new Color(0.78f, 0.82f, 0.92f, 1f);

        TextMeshProUGUI hint = CreateTMP("Hint", "TAP TO INSPECT", 12f, buttonObj.transform, new Vector2(0.24f, 0.05f), new Vector2(0.97f, 0.20f));
        hint.alignment = TextAlignmentOptions.Left;
        hint.color = new Color(0.55f, 0.60f, 0.70f, 1f);
        hint.fontStyle = FontStyles.Italic;

        return button;
    }

    private void SetDetails(DungeonDataSO dungeon)
    {
        if (titleText != null)
        {
            titleText.text = dungeon != null ? dungeon.DisplayName.ToUpperInvariant() : "NO DUNGEON";
        }

        if (descriptionText != null)
        {
            descriptionText.text = dungeon != null ? dungeon.Description : "No dungeon assets found yet.";
        }

        if (costText != null)
        {
            costText.text = dungeon != null ? "SP " + dungeon.StaminaCost : "--";
        }

        if (rewardText != null)
        {
            rewardText.text = dungeon != null ? "Gold +" + dungeon.BaseGoldReward + "  EXP +" + dungeon.BaseExpReward : "--";
        }

        if (iconImage != null)
        {
            iconImage.sprite = dungeon != null ? dungeon.Icon : null;
            iconImage.gameObject.SetActive(dungeon != null && dungeon.Icon != null);
        }

        if (startButton != null)
        {
            startButton.interactable = dungeon != null;
        }
    }

    private void OnStartClicked()
    {
        if (selectedDungeon == null || dungeonService == null)
        {
            return;
        }

        dungeonService.StartDungeon(selectedDungeon);
    }



    private void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    private TextMeshProUGUI CreateTMP(string name, string text, float fontSize, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return tmp;
    }
}
