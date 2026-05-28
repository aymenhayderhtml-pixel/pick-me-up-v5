using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.Core;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.UI
{
    /// <summary>
    /// Main UI controller for the Inventory system.
    /// Handles displaying items, filtering, and interactions.
    /// </summary>
    public class InventoryView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform HeaderArea;
        public RectTransform FilterArea;
        public RectTransform ItemGridArea;
        public RectTransform FooterArea;

        [Header("UI Elements")]
        public Button BackButton;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI ItemCountText;

        [Header("Filter Tabs")]
        public Button AllTabButton;
        public Button EquipmentTabButton;
        public Button ConsumablesTabButton;
        public Button MaterialsTabButton;

        [Header("Action Buttons")]
        public Button UseButton;
        public Button EquipButton;
        public Button UnequipButton;
        public Button SellButton;

        [Header("Prefabs")]
        public RectTransform ItemCardPrefab;

        [Header("Colors")]
        public Color TabActiveColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        public Color TabInactiveColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        public Color SelectedItemColor = new Color(0.4f, 0.7f, 1f, 1f);
        public Color NewItemGlowColor = Color.yellow;

        // Services
        private IInventoryService _inventoryService;
        private IRosterService _rosterService;
        private ICurrencyService _currencyService;
        private IGameStateService _gameStateService;

        // State
        private ItemType _currentFilter = ItemType.None;
        private ItemRarity? _currentRarityFilter = null;
        private ItemInstance _selectedItem;
        private List<ItemInstance> _displayedItems = new List<ItemInstance>();
        private List<RectTransform> _itemCards = new List<RectTransform>();

        // UI State
        private RectTransform _itemDetailPanel;
        private GameObject _heroSelectorPanel;

        public void Initialize(
            IInventoryService inventoryService,
            IRosterService rosterService,
            ICurrencyService currencyService,
            IGameStateService gameStateService)
        {
            _inventoryService = inventoryService;
            _rosterService = rosterService;
            _currencyService = currencyService;
            _gameStateService = gameStateService;

            SetupUI();
            SetupEventListeners();
            RefreshUI();
        }

        private void SetupUI()
        {
            ClearExistingUI();

            // Create header
            CreateHeader();

            // Create filter tabs
            CreateFilterTabs();

            // Create item grid
            CreateItemGrid();

            // Create item detail panel (hidden initially)
            CreateItemDetailPanel();

            // Create footer with actions
            CreateFooter();
        }

        private void ClearExistingUI()
        {
            if (ContentArea != null)
            {
                foreach (Transform child in ContentArea)
                {
                    Destroy(child.gameObject);
                }
            }

            _itemCards.Clear();
            _selectedItem = null;
        }

        private void CreateHeader()
        {
            var headerContainer = new GameObject("HeaderContainer", typeof(RectTransform));
            headerContainer.transform.SetParent(HeaderArea, false);

            var headerRT = headerContainer.GetComponent<RectTransform>();
            headerRT.anchorMin = Vector2.zero;
            headerRT.anchorMax = Vector2.one;
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            // Back button
            var backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnObj.transform.SetParent(headerRT, false);
            var backBtnRT = backBtnObj.GetComponent<RectTransform>();
            backBtnRT.anchorMin = new Vector2(0, 0.5f);
            backBtnRT.anchorMax = new Vector2(0, 0.5f);
            backBtnRT.pivot = new Vector2(0, 0.5f);
            backBtnRT.anchoredPosition = new Vector2(30, 0);
            backBtnRT.sizeDelta = new Vector2(120, 60);

            var backBtnImage = backBtnObj.GetComponent<Image>();
            backBtnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            BackButton = backBtnObj.GetComponent<Button>();
            BackButton.onClick.AddListener(OnBackPressed);

            var backTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            var backText = backTextObj.GetComponent<TextMeshProUGUI>();
            backText.text = "< Back";
            backText.fontSize = 24;
            backText.color = Color.white;

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(headerRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.3f, 0.5f);
            titleRT.anchorMax = new Vector2(0.7f, 0.5f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            titleRT.sizeDelta = new Vector2(0, 50);

            TitleText = titleObj.GetComponent<TextMeshProUGUI>();
            TitleText.text = "Inventory";
            TitleText.fontSize = 32;
            TitleText.color = Color.white;
            TitleText.alignment = TextAlignmentOptions.Center;

            // Item count
            var countObj = new GameObject("ItemCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            countObj.transform.SetParent(headerRT, false);
            var countRT = countObj.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0.75f, 0.5f);
            countRT.anchorMax = new Vector2(1, 0.5f);
            countRT.pivot = new Vector2(1, 0.5f);
            countRT.anchoredPosition = new Vector2(-20, 0);
            countRT.sizeDelta = new Vector2(200, 40);

            ItemCountText = countObj.GetComponent<TextMeshProUGUI>();
            ItemCountText.text = "0 Items";
            ItemCountText.fontSize = 20;
            ItemCountText.color = Color.gray;
            ItemCountText.alignment = TextAlignmentOptions.Right;
        }

        private void CreateFilterTabs()
        {
            var filterContainer = new GameObject("FilterTabs", typeof(RectTransform));
            filterContainer.transform.SetParent(FilterArea, false);

            var filterRT = filterContainer.GetComponent<RectTransform>();
            filterRT.anchorMin = Vector2.zero;
            filterRT.anchorMax = Vector2.one;
            filterRT.offsetMin = Vector2.zero;
            filterRT.offsetMax = Vector2.zero;

            string[] tabNames = { "All", "Equipment", "Consumables", "Materials" };
            ItemType[] tabTypes = { ItemType.None, ItemType.Weapon, ItemType.Consumable, ItemType.Material };

            for (int i = 0; i < tabNames.Length; i++)
            {
                var tabObj = CreateTabButton(tabNames[i], i, tabTypes[i]);
                tabObj.transform.SetParent(filterRT, false);
            }
        }

        private RectTransform CreateTabButton(string name, int index, ItemType filterType)
        {
            var tabObj = new GameObject($"Tab_{name}", typeof(RectTransform), typeof(Image), typeof(Button));
            var tabRT = tabObj.GetComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(index * 0.25f, 0);
            tabRT.anchorMax = new Vector2((index + 1) * 0.25f, 1);
            tabRT.offsetMin = Vector2.zero;
            tabRT.offsetMax = Vector2.zero;

            var tabImage = tabObj.GetComponent<Image>();
            tabImage.color = index == 0 ? TabActiveColor : TabInactiveColor;

            var button = tabObj.GetComponent<Button>();
            int capturedIndex = index;
            ItemType capturedType = filterType;
            button.onClick.AddListener(() => OnTabClicked(capturedIndex, capturedType));

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(tabObj.transform, false);
            var textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 5);
            textRT.offsetMax = new Vector2(-10, -5);

            var text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = name;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return tabObj;
        }

        private void CreateItemGrid()
        {
            var gridContainer = new GameObject("ItemGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridContainer.transform.SetParent(ItemGridArea, false);

            var gridRT = gridContainer.GetComponent<RectTransform>();
            gridRT.anchorMin = Vector2.zero;
            gridRT.anchorMax = Vector2.one;
            gridRT.offsetMin = Vector2.zero;
            gridRT.offsetMax = Vector2.zero;

            var grid = gridContainer.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150, 170);
            grid.spacing = new Vector2(15, 15);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.padding = new RectOffset(20, 20, 20, 20);

            // Populate with items
            RefreshItemGrid();
        }

        private void RefreshItemGrid()
        {
            // Clear existing cards
            foreach (var card in _itemCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _itemCards.Clear();

            // Get filtered items
            _displayedItems.Clear();

            if (_currentFilter == ItemType.None)
            {
                _displayedItems = _inventoryService.GetAllItems();
            }
            else
            {
                _displayedItems = _inventoryService.GetItemsByType(_currentFilter);
            }

            // Filter by rarity if set
            if (_currentRarityFilter.HasValue)
            {
                _displayedItems = _displayedItems.FindAll(i =>
                    _inventoryService.GetItemDefinition(i.DefinitionId)?.Rarity == _currentRarityFilter.Value);
            }

            // Create item cards
            var grid = ItemGridArea.Find("ItemGrid")?.GetComponent<GridLayoutGroup>();
            if (grid == null) return;

            foreach (var item in _displayedItems)
            {
                var card = CreateItemCard(item, grid.transform);
                _itemCards.Add(card);
            }

            // Update count
            ItemCountText.text = $"{_displayedItems.Count} Items";
        }

        private RectTransform CreateItemCard(ItemInstance item, Transform parent)
        {
            var cardObj = Instantiate(ItemCardPrefab, parent, false);
            cardObj.name = $"ItemCard_{item.InstanceId}";

            var def = _inventoryService.GetItemDefinition(item.DefinitionId);
            if (def == null) return cardObj;

            // Card background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(cardObj, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // Rarity border
            var borderObj = new GameObject("RarityBorder", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(cardObj, false);
            var borderRT = borderObj.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(3, 3);
            borderRT.offsetMax = new Vector2(-3, -3);

            var borderImage = borderObj.GetComponent<Image>();
            borderImage.color = def.Rarity.GetColor();
            borderImage.fillCenter = false;

            // Icon
            if (def.Icon != null)
            {
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardObj, false);
                var iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.1f, 0.35f);
                iconRT.anchorMax = new Vector2(0.9f, 0.85f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;

                var iconImage = iconObj.GetComponent<Image>();
                iconImage.sprite = def.Icon;
                iconImage.preserveAspect = true;
            }

            // Name
            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(cardObj, false);
            var nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.15f);
            nameRT.anchorMax = new Vector2(0.95f, 0.32f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            var nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = def.DisplayName;
            nameText.fontSize = 14;
            nameText.color = def.Rarity.GetColor();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableWordWrapping = true;

            // Quantity (for stackable items)
            if (item.Quantity > 1)
            {
                var qtyObj = new GameObject("Quantity", typeof(RectTransform), typeof(TextMeshProUGUI));
                qtyObj.transform.SetParent(cardObj, false);
                var qtyRT = qtyObj.GetComponent<RectTransform>();
                qtyRT.anchorMin = new Vector2(0.7f, 0.02f);
                qtyRT.anchorMax = new Vector2(0.98f, 0.14f);
                qtyRT.offsetMin = Vector2.zero;
                qtyRT.offsetMax = Vector2.zero;

                var qtyText = qtyObj.GetComponent<TextMeshProUGUI>();
                qtyText.text = $"x{item.Quantity}";
                qtyText.fontSize = 14;
                qtyText.color = Color.white;
                qtyText.alignment = TextAlignmentOptions.Right;
            }

            // New indicator
            if (item.IsNew)
            {
                var newObj = new GameObject("NewIndicator", typeof(RectTransform), typeof(Image));
                newObj.transform.SetParent(cardObj, false);
                var newRT = newObj.GetComponent<RectTransform>();
                newRT.anchorMin = new Vector2(0.05f, 0.88f);
                newRT.anchorMax = new Vector2(0.35f, 0.98f);
                newRT.offsetMin = Vector2.zero;
                newRT.offsetMax = Vector2.zero;

                var newImage = newObj.GetComponent<Image>();
                newImage.color = NewItemGlowColor;

                var newTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                newTextObj.transform.SetParent(newObj.transform, false);
                var newText = newTextObj.GetComponent<TextMeshProUGUI>();
                newText.text = "NEW";
                newText.fontSize = 10;
                newText.color = Color.black;
                newText.alignment = TextAlignmentOptions.Center;
            }

            // Equipped indicator
            if (item.IsEquipped())
            {
                var equippedObj = new GameObject("EquippedIndicator", typeof(RectTransform), typeof(TextMeshProUGUI));
                equippedObj.transform.SetParent(cardObj, false);
                var equippedRT = equippedObj.GetComponent<RectTransform>();
                equippedRT.anchorMin = new Vector2(0.65f, 0.88f);
                equippedRT.anchorMax = new Vector2(0.95f, 0.98f);
                equippedRT.offsetMin = Vector2.zero;
                equippedRT.offsetMax = Vector2.zero;

                var equippedText = equippedObj.GetComponent<TextMeshProUGUI>();
                equippedText.text = "E";
                equippedText.fontSize = 12;
                equippedText.color = Color.cyan;
                equippedText.alignment = TextAlignmentOptions.Center;
            }

            // Add click handler
            var button = cardObj.gameObject.AddComponent<Button>();
            var cardData = cardObj.gameObject.AddComponent<InventoryItemCard>();
            cardData.Item = item;
            cardData.InventoryView = this;
            button.onClick.AddListener(() => OnItemClicked(item));

            return cardObj;
        }

        private void CreateItemDetailPanel()
        {
            _itemDetailPanel = new GameObject("ItemDetailPanel", typeof(RectTransform)).GetComponent<RectTransform>();
            _itemDetailPanel.transform.SetParent(Canvas.transform, false);
            _itemDetailPanel.anchorMin = new Vector2(0.1f, 0.15f);
            _itemDetailPanel.anchorMax = new Vector2(0.9f, 0.85f);
            _itemDetailPanel.offsetMin = Vector2.zero;
            _itemDetailPanel.offsetMax = Vector2.zero;
            _itemDetailPanel.gameObject.SetActive(false);

            // Background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(_itemDetailPanel, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        }

        private void CreateFooter()
        {
            var footerContainer = new GameObject("FooterContainer", typeof(RectTransform));
            footerContainer.transform.SetParent(FooterArea, false);

            var footerRT = footerContainer.GetComponent<RectTransform>();
            footerRT.anchorMin = Vector2.zero;
            footerRT.anchorMax = Vector2.one;
            footerRT.offsetMin = Vector2.zero;
            footerRT.offsetMax = Vector2.zero;

            // Action buttons row
            CreateActionButton(footerRT, "Use", 0, 0.25f, OnUsePressed);
            CreateActionButton(footerRT, "Equip", 0.275f, 0.5f, OnEquipPressed);
            CreateActionButton(footerRT, "Sell", 0.525f, 0.75f, OnSellPressed);
            CreateActionButton(footerRT, "Close", 0.775f, 1f, OnCloseDetailPressed);
        }

        private void CreateActionButton(RectTransform parent, string label, float xMin, float xMax, UnityEngine.Events.UnityAction onClick)
        {
            var btnObj = new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(xMin, 0.1f);
            btnRT.anchorMax = new Vector2(xMax, 0.9f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            var btnImage = btnObj.GetComponent<Image>();
            btnImage.color = new Color(0.25f, 0.25f, 0.35f, 0.9f);

            var button = btnObj.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            var textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(5, 5);
            textRT.offsetMax = new Vector2(-5, -5);

            var text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }

        private void SetupEventListeners()
        {
            if (BackButton != null)
            {
                BackButton.onClick.RemoveAllListeners();
                BackButton.onClick.AddListener(OnBackPressed);
            }
        }

        private void RefreshUI()
        {
            RefreshItemGrid();
            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            bool hasSelection = _selectedItem != null;
            bool isEquipable = false;
            bool isConsumable = false;

            if (_selectedItem != null)
            {
                var def = _inventoryService.GetItemDefinition(_selectedItem.DefinitionId);
                if (def != null)
                {
                    isEquipable = def.IsEquipable();
                    isConsumable = def.IsConsumable();
                }
            }
        }

        private void OnTabClicked(int index, ItemType filterType)
        {
            _currentFilter = filterType;
            _currentRarityFilter = null;

            // Update tab colors
            var filterContainer = FilterArea.Find("FilterTabs");
            if (filterContainer != null)
            {
                for (int i = 0; i < filterContainer.childCount; i++)
                {
                    var tab = filterContainer.GetChild(i);
                    var image = tab.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = i == index ? TabActiveColor : TabInactiveColor;
                    }
                }
            }

            RefreshItemGrid();
        }

        private void OnItemClicked(ItemInstance item)
        {
            _selectedItem = item;

            // Highlight selected card
            foreach (var card in _itemCards)
            {
                var border = card.Find("RarityBorder")?.GetComponent<Image>();
                if (border != null)
                {
                    var cardData = card.GetComponent<InventoryItemCard>();
                    if (cardData != null && cardData.Item.InstanceId == item.InstanceId)
                    {
                        border.color = SelectedItemColor;
                    }
                    else
                    {
                        var def = _inventoryService.GetItemDefinition(cardData?.Item?.DefinitionId);
                        if (def != null)
                        {
                            border.color = def.Rarity.GetColor();
                        }
                    }
                }
            }

            ShowItemDetail(item);
        }

        private void ShowItemDetail(ItemInstance item)
        {
            _itemDetailPanel.gameObject.SetActive(true);

            var def = _inventoryService.GetItemDefinition(item.DefinitionId);
            if (def == null) return;

            // Clear existing children
            foreach (Transform child in _itemDetailPanel)
            {
                if (child.name != "Background")
                    Destroy(child.gameObject);
            }

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(_itemDetailPanel, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.85f);
            titleRT.anchorMax = new Vector2(1, 0.95f);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = def.DisplayName;
            titleText.fontSize = 28;
            titleText.color = def.Rarity.GetColor();

            // Type and rarity
            var typeObj = new GameObject("Type", typeof(RectTransform), typeof(TextMeshProUGUI));
            typeObj.transform.SetParent(_itemDetailPanel, false);
            var typeRT = typeObj.GetComponent<RectTransform>();
            typeRT.anchorMin = new Vector2(0, 0.78f);
            typeRT.anchorMax = new Vector2(1, 0.84f);
            typeRT.offsetMin = new Vector2(20, 0);
            typeRT.offsetMax = new Vector2(-20, 0);

            var typeText = typeObj.GetComponent<TextMeshProUGUI>();
            typeText.text = $"{def.ItemType} - {def.Rarity}";
            typeText.fontSize = 18;
            typeText.color = Color.gray;

            // Description
            var descObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(_itemDetailPanel, false);
            var descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.5f);
            descRT.anchorMax = new Vector2(1, 0.76f);
            descRT.offsetMin = new Vector2(20, 0);
            descRT.offsetMax = new Vector2(-20, 0);

            var descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.text = def.Description;
            descText.fontSize = 16;
            descText.color = Color.white;

            // Stats (for equipment)
            if (def.IsEquipable())
            {
                CreateStatDisplay(_itemDetailPanel, "Stats", new Vector2(0, 0.2f), new Vector2(1, 0.48f), def);
            }

            // Quantity
            var qtyObj = new GameObject("Quantity", typeof(RectTransform), typeof(TextMeshProUGUI));
            qtyObj.transform.SetParent(_itemDetailPanel, false);
            var qtyRT = qtyObj.GetComponent<RectTransform>();
            qtyRT.anchorMin = new Vector2(0, 0.12f);
            qtyRT.anchorMax = new Vector2(1, 0.18f);
            qtyRT.offsetMin = new Vector2(20, 0);
            qtyRT.offsetMax = new Vector2(-20, 0);

            var qtyText = qtyObj.GetComponent<TextMeshProUGUI>();
            qtyText.text = item.Quantity > 1 ? $"Quantity: {item.Quantity}" : "";
            qtyText.fontSize = 16;
            qtyText.color = Color.white;

            // Equipped status
            if (item.IsEquipped())
            {
                var equippedObj = new GameObject("EquippedStatus", typeof(RectTransform), typeof(TextMeshProUGUI));
                equippedObj.transform.SetParent(_itemDetailPanel, false);
                var equippedRT = equippedObj.GetComponent<RectTransform>();
                equippedRT.anchorMin = new Vector2(0, 0.05f);
                equippedRT.anchorMax = new Vector2(1, 0.11f);
                equippedRT.offsetMin = new Vector2(20, 0);
                equippedRT.offsetMax = new Vector2(-20, 0);

                var equippedText = equippedObj.GetComponent<TextMeshProUGUI>();
                equippedText.text = $"Equipped to: {item.EquippedToHeroId}";
                equippedText.fontSize = 14;
                equippedText.color = Color.cyan;
            }

            // Sell value
            if (def.SellValue > 0)
            {
                var sellObj = new GameObject("SellValue", typeof(RectTransform), typeof(TextMeshProUGUI));
                sellObj.transform.SetParent(_itemDetailPanel, false);
                var sellRT = sellObj.GetComponent<RectTransform>();
                sellRT.anchorMin = new Vector2(0, 0);
                sellRT.anchorMax = new Vector2(1, 0.05f);
                sellRT.offsetMin = new Vector2(20, 0);
                sellRT.offsetMax = new Vector2(-20, 0);

                var sellText = sellObj.GetComponent<TextMeshProUGUI>();
                sellText.text = $"Sell Value: {def.SellValue} Gold";
                sellText.fontSize = 14;
                sellText.color = Color.yellow;
            }

            // Mark as seen
            _inventoryService.MarkAsSeen(item.InstanceId);
        }

        private void CreateStatDisplay(RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, ItemDefinition def)
        {
            var statsObj = new GameObject("StatsSection", typeof(RectTransform));
            statsObj.transform.SetParent(parent, false);
            var statsRT = statsObj.GetComponent<RectTransform>();
            statsRT.anchorMin = anchorMin;
            statsRT.anchorMax = anchorMax;
            statsRT.offsetMin = Vector2.zero;
            statsRT.offsetMax = Vector2.zero;

            float rarityMultiplier = GetRarityMultiplier(def.Rarity);
            string[] statNames = { "ATK", "DEF", "HP", "SPD" };
            int[] baseStats = { def.BaseAttack, def.BaseDefense, def.BaseHealth, def.BaseSpeed };

            for (int i = 0; i < statNames.Length; i++)
            {
                if (baseStats[i] > 0)
                {
                    var statObj = new GameObject($"Stat_{statNames[i]}", typeof(RectTransform), typeof(TextMeshProUGUI));
                    statObj.transform.SetParent(statsRT, false);
                    var statRT = statObj.GetComponent<RectTransform>();
                    statRT.anchorMin = new Vector2(0, 1f - (i + 1) * 0.25f);
                    statRT.anchorMax = new Vector2(1, 1f - i * 0.25f);
                    statRT.offsetMin = new Vector2(20, 0);
                    statRT.offsetMax = new Vector2(-20, 0);

                    var statText = statObj.GetComponent<TextMeshProUGUI>();
                    statText.text = $"{statNames[i]}: +{Mathf.CeilToInt(baseStats[i] * rarityMultiplier)}";
                    statText.fontSize = 16;
                    statText.color = Color.green;
                }
            }
        }

        private float GetRarityMultiplier(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return 1.0f;
                case ItemRarity.Uncommon: return 1.25f;
                case ItemRarity.Rare: return 1.5f;
                case ItemRarity.Epic: return 1.75f;
                case ItemRarity.Legendary: return 2.0f;
                default: return 1.0f;
            }
        }

        private void HideItemDetail()
        {
            _itemDetailPanel?.gameObject.SetActive(false);
            _selectedItem = null;

            // Reset card highlights
            foreach (var card in _itemCards)
            {
                var border = card.Find("RarityBorder")?.GetComponent<Image>();
                var cardData = card.GetComponent<InventoryItemCard>();
                if (border != null && cardData?.Item != null)
                {
                    var def = _inventoryService.GetItemDefinition(cardData.Item.DefinitionId);
                    if (def != null)
                    {
                        border.color = def.Rarity.GetColor();
                    }
                }
            }
        }

        private void OnUsePressed()
        {
            if (_selectedItem == null) return;

            var def = _inventoryService.GetItemDefinition(_selectedItem.DefinitionId);
            if (def == null || !def.IsConsumable()) return;

            // Show hero selector for consumables
            ShowHeroSelectorForItem(_selectedItem);
        }

        private void ShowHeroSelectorForItem(ItemInstance item)
        {
            // For simplicity, use first available hero
            var heroes = _rosterService.GetAllHeroes();
            if (heroes.Count > 0)
            {
                var result = _inventoryService.UseItem(item.InstanceId, heroes[0].InstanceId);
                Debug.Log(result.Message);
                RefreshUI();
            }
        }

        private void OnEquipPressed()
        {
            if (_selectedItem == null) return;

            var def = _inventoryService.GetItemDefinition(_selectedItem.DefinitionId);
            if (def == null || !def.IsEquipable()) return;

            // Show hero selector for equipment
            ShowHeroSelectorForEquipment(_selectedItem);
        }

        private void ShowHeroSelectorForEquipment(ItemInstance item)
        {
            var heroes = _rosterService.GetAllHeroes();
            if (heroes.Count > 0)
            {
                _inventoryService.EquipItem(item.InstanceId, heroes[0].InstanceId);
                Debug.Log($"Equipped {item.DefinitionId} to hero");
                RefreshUI();
            }
        }

        private void OnSellPressed()
        {
            if (_selectedItem == null) return;

            if (_inventoryService.SellItem(_selectedItem.InstanceId, 1))
            {
                Debug.Log($"Sold item for gold");
                HideItemDetail();
                RefreshUI();
            }
        }

        private void OnCloseDetailPressed()
        {
            HideItemDetail();
        }

        private void OnBackPressed()
        {
            _gameStateService.LoadScene("Hub");
        }

        private void OnEnable()
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// Component for item card click handling.
    /// </summary>
    public class InventoryItemCard : MonoBehaviour
    {
        public ItemInstance Item;
        public InventoryView InventoryView;
    }
}