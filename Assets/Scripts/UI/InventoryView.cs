using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private Button allTabButton;
    [SerializeField] private Button equipmentTabButton;
    [SerializeField] private Button consumablesTabButton;
    [SerializeField] private Button materialsTabButton;
    [SerializeField] private Button useButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private RectTransform itemGridContent;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailTypeText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    [SerializeField] private TextMeshProUGUI detailQuantityText;
    [SerializeField] private TextMeshProUGUI detailEquippedText;
    [SerializeField] private TextMeshProUGUI detailStatsText;

    private IInventoryService _inventoryService;
    private IRosterService _rosterService;
    private ICurrencyService _currencyService;
    private readonly List<InventoryItemCard> _spawnedCards = new List<InventoryItemCard>();
    private ItemType _currentFilter = ItemType.None;
    private ItemInstance _selectedItem;

    private void Start()
    {
        _inventoryService = ServiceRegistry.Instance.Resolve<IInventoryService>();
        _rosterService = ServiceRegistry.Instance.Resolve<IRosterService>();
        _currencyService = ServiceRegistry.Instance.Resolve<ICurrencyService>();

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        }

        if (allTabButton != null) allTabButton.onClick.AddListener(() => SetFilter(ItemType.None));
        if (equipmentTabButton != null) equipmentTabButton.onClick.AddListener(() => SetFilter(ItemType.Weapon));
        if (consumablesTabButton != null) consumablesTabButton.onClick.AddListener(() => SetFilter(ItemType.Consumable));
        if (materialsTabButton != null) materialsTabButton.onClick.AddListener(() => SetFilter(ItemType.Material));
        if (useButton != null) useButton.onClick.AddListener(OnUsePressed);
        if (equipButton != null) equipButton.onClick.AddListener(OnEquipPressed);
        if (unequipButton != null) unequipButton.onClick.AddListener(OnUnequipPressed);
        if (sellButton != null) sellButton.onClick.AddListener(OnSellPressed);

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        if (_inventoryService == null)
        {
            return;
        }

        RefreshUI();
    }

    private void SetFilter(ItemType type)
    {
        _currentFilter = type;
        RefreshUI();
    }

    public void OnItemClicked(ItemInstance item)
    {
        _selectedItem = item;
        PopulateDetail();
        RefreshHighlights();
    }

    private void RefreshUI()
    {
        if (titleText != null)
        {
            titleText.text = "INVENTORY";
        }

        BuildGrid();
        UpdateCount();
        UpdateActionButtons();
    }

    private void BuildGrid()
    {
        if (itemGridContent == null || _inventoryService == null)
        {
            return;
        }

        foreach (Transform child in itemGridContent)
        {
            Destroy(child.gameObject);
        }

        _spawnedCards.Clear();

        List<ItemInstance> items;
        if (_currentFilter == ItemType.None)
        {
            items = _inventoryService.GetAllItems();
        }
        else if (_currentFilter == ItemType.Weapon)
        {
            items = _inventoryService.GetAllItems().Where(item =>
            {
                ItemDefinition def = _inventoryService.GetItemDefinition(item.DefinitionId);
                return def != null && def.IsEquipable();
            }).ToList();
        }
        else
        {
            items = _inventoryService.GetItemsByType(_currentFilter);
        }

        foreach (ItemInstance item in items)
        {
            GameObject card = CreateItemCard(item);
            card.transform.SetParent(itemGridContent, false);
            InventoryItemCard cardComponent = card.GetComponent<InventoryItemCard>();
            cardComponent.SetupCard(item, this);
            _spawnedCards.Add(cardComponent);
        }
    }

    private GameObject CreateItemCard(ItemInstance item)
    {
        GameObject card = new GameObject("ItemCard_" + item.InstanceId, typeof(RectTransform), typeof(Image), typeof(Button), typeof(InventoryItemCard));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 180);
        card.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        CreateTMP("Name", "Item", 14, card.transform, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.8f));
        CreateTMP("Quantity", string.Empty, 12, card.transform, new Vector2(0.65f, 0.02f), new Vector2(0.95f, 0.15f));
        CreateTMP("NewIndicator", "NEW", 10, card.transform, new Vector2(0.05f, 0.85f), new Vector2(0.35f, 0.98f));
        CreateTMP("EquippedIndicator", "E", 12, card.transform, new Vector2(0.72f, 0.85f), new Vector2(0.95f, 0.98f));
        GameObject icon = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(card.transform, false);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.18f);
        iconRT.anchorMax = new Vector2(0.9f, 0.5f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        GameObject border = new GameObject("RarityBorder", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(card.transform, false);
        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(4, 4);
        borderRT.offsetMax = new Vector2(-4, -4);
        border.GetComponent<Image>().color = Color.gray;

        return card;
    }

    private void PopulateDetail()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(_selectedItem != null);
        }

        if (_selectedItem == null)
        {
            return;
        }

        ItemDefinition def = _inventoryService.GetItemDefinition(_selectedItem.DefinitionId);
        if (def == null)
        {
            return;
        }

        if (detailNameText != null)
        {
            detailNameText.text = def.DisplayName;
        }

        if (detailTypeText != null)
        {
            detailTypeText.text = def.ItemType + " / " + def.Rarity;
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = def.Description;
        }

        if (detailQuantityText != null)
        {
            detailQuantityText.text = "Quantity: " + _selectedItem.Quantity;
        }

        if (detailEquippedText != null)
        {
            detailEquippedText.text = _selectedItem.IsEquipped() ? "Equipped to: " + _selectedItem.EquippedToHeroId : "Not equipped";
        }

        if (detailStatsText != null)
        {
            detailStatsText.text = "ATK +" + _selectedItem.BonusAttack + "  DEF +" + _selectedItem.BonusDefense + "  HP +" + _selectedItem.BonusHealth;
        }
    }

    private void RefreshHighlights()
    {
        foreach (InventoryItemCard card in _spawnedCards)
        {
            card.SetHighlight(card.Item != null && _selectedItem != null && card.Item.InstanceId == _selectedItem.InstanceId);
        }
    }

    private void UpdateCount()
    {
        if (itemCountText != null && _inventoryService != null)
        {
            itemCountText.text = _inventoryService.GetAllItems().Count + " Items";
        }
    }

    private void UpdateActionButtons()
    {
        bool hasSelection = _selectedItem != null;
        if (useButton != null) useButton.interactable = hasSelection;
        if (equipButton != null) equipButton.interactable = hasSelection;
        if (unequipButton != null) unequipButton.interactable = hasSelection;
        if (sellButton != null) sellButton.interactable = hasSelection;
    }

    private void OnUsePressed()
    {
        if (_selectedItem == null)
        {
            return;
        }

        _inventoryService.UseItem(_selectedItem.InstanceId, FindFirstHeroId());
        RefreshUI();
    }

    private void OnEquipPressed()
    {
        if (_selectedItem == null)
        {
            return;
        }

        _inventoryService.EquipItem(_selectedItem.InstanceId, FindFirstHeroId());
        RefreshUI();
    }

    private void OnUnequipPressed()
    {
        if (_selectedItem == null)
        {
            return;
        }

        _inventoryService.UnequipItem(_selectedItem.InstanceId);
        RefreshUI();
    }

    private void OnSellPressed()
    {
        if (_selectedItem == null)
        {
            return;
        }

        _inventoryService.SellItem(_selectedItem.InstanceId, 1);
        _selectedItem = null;
        RefreshUI();
    }

    private string FindFirstHeroId()
    {
        HeroInstance hero = _rosterService.GetAlive().FirstOrDefault();
        return hero != null ? hero.InstanceId : string.Empty;
    }

    private GameObject CreateTMP(string name, string text, float fontSize, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
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
        return go;
    }
}
