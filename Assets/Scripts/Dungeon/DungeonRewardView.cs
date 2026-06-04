using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonRewardView : MonoBehaviour
{
    [Header("Container")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private RectTransform itemListContent;
    [SerializeField] private TextMeshProUGUI summaryText;

    [Header("Buttons")]
    [SerializeField] private Button claimButton;

    [Header("Return Targets")]
    [Tooltip("Victory banner that the reward view will hide when the player claims.")]
    [SerializeField] private GameObject victoryPanel;
    [Tooltip("Dungeon list panel that will be shown again when the player claims.")]
    [SerializeField] private GameObject dungeonListPanel;
    [Tooltip("Dungeon detail panel that will be shown again when the player claims.")]
    [SerializeField] private GameObject dungeonDetailPanel;

    [Header("Style")]
    [SerializeField] private Color currencyRowColor = new Color(0.18f, 0.20f, 0.26f, 0.95f);
    [SerializeField] private Color itemRowColor = new Color(0.13f, 0.15f, 0.20f, 0.95f);
    [SerializeField] private float rowHeight = 110f;
    [SerializeField] private float rowSpacing = 10f;
    [SerializeField] private int rowFontSize = 28;

    private IInventoryService _inventoryService;
    private readonly List<GameObject> spawnedRows = new List<GameObject>();
    private string _activeDungeonName;

    private void Awake()
    {
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IInventoryService>())
        {
            _inventoryService = ServiceRegistry.Instance.Resolve<IInventoryService>();
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnClaimClicked);
            claimButton.onClick.AddListener(OnClaimClicked);
        }

        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

    public void HandleRewardGranted(DungeonRewardResult result)
    {
        if (result == null)
        {
            return;
        }

        ClearRows();

        if (summaryText != null)
        {
            string header = "DUNGEON CLEARED";
            if (!string.IsNullOrEmpty(_activeDungeonName))
            {
                header += "\n<size=24>" + _activeDungeonName + "</size>";
            }
            summaryText.text = header;
        }

        if (result.Gold > 0)
        {
            CreateCurrencyRow("GOLD", "+ " + result.Gold);
        }

        if (result.Exp > 0)
        {
            CreateCurrencyRow("EXP", "+ " + result.Exp);
        }

        if (result.Items != null)
        {
            for (int i = 0; i < result.Items.Count; i++)
            {
                CreateItemRow(result.Items[i]);
            }
        }

        if (rootPanel != null)
        {
            rootPanel.SetActive(true);
        }
    }

    private void CreateCurrencyRow(string label, string value)
    {
        GameObject row = CreateRowBase(label + "Row", currencyRowColor);
        CreateRowText(row, label, 0.05f, 0.55f, FontStyles.Bold);
        CreateRowText(row, value, 0.55f, 0.95f, FontStyles.Normal, TextAlignmentOptions.Right);
    }

    private void CreateItemRow(DungeonRewardItem item)
    {
        if (item == null)
        {
            return;
        }

        GameObject row = CreateRowBase(item.ItemId + "Row", itemRowColor);

        string resolvedName = item.ItemId;
        ItemDefinition def = _inventoryService != null ? _inventoryService.GetItemDefinition(item.ItemId) : null;
        if (def != null && !string.IsNullOrEmpty(def.DisplayName))
        {
            resolvedName = def.DisplayName;
        }
        else if (!string.IsNullOrEmpty(item.DisplayName) && item.DisplayName != item.ItemId)
        {
            resolvedName = item.DisplayName;
        }

        CreateRowText(row, resolvedName, 0.05f, 0.70f, FontStyles.Bold);
        CreateRowText(row, "x" + item.Quantity, 0.70f, 0.95f, FontStyles.Normal, TextAlignmentOptions.Right);
    }

    private GameObject CreateRowBase(string name, Color color)
    {
        GameObject row = new GameObject(name, typeof(RectTransform), typeof(Image));
        row.transform.SetParent(itemListContent, false);
        Image img = row.GetComponent<Image>();
        img.color = color;

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight = rowHeight;
        le.preferredHeight = rowHeight;
        le.flexibleHeight = 0f;

        spawnedRows.Add(row);
        return row;
    }

    private void CreateRowText(GameObject parent, string text, float anchorXMin, float anchorXMax, FontStyles style, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorXMin, 0.05f);
        rt.anchorMax = new Vector2(anchorXMax, 0.95f);
        rt.offsetMin = new Vector2(12, 0);
        rt.offsetMax = new Vector2(-12, 0);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = rowFontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void OnClaimClicked()
    {
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IDungeonService>())
        {
            IDungeonService dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
            if (dungeonService.IsInDungeon)
            {
                dungeonService.AbandonDungeon();
            }
        }

        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        if (dungeonListPanel != null)
        {
            dungeonListPanel.SetActive(true);
        }
        if (dungeonDetailPanel != null)
        {
            dungeonDetailPanel.SetActive(true);
        }

        _activeDungeonName = string.Empty;
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
            {
                Destroy(spawnedRows[i]);
            }
        }
        spawnedRows.Clear();
    }
}
