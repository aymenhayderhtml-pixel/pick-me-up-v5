using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.Core;

namespace PickMeUp.Game.UI
{
    /// <summary>
    /// Represents a single item card in the inventory grid.
    /// Handles display and interaction for individual items.
    /// </summary>
    public class InventoryItemCard : MonoBehaviour
    {
        [Header("UI References")]
        public Image ItemIcon;
        public Image RarityBorder;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI QuantityText;
        public TextMeshProUGUI NewIndicator;
        public TextMeshProUGUI EquippedIndicator;
        public GameObject StatBonusContainer;

        // External references
        public InventoryView ParentView { get; set; }

        // Data
        private ItemInstance _item;
        private IInventoryService _inventoryService;

        public ItemInstance Item
        {
            get => _item;
            set
            {
                _item = value;
                if (_item != null)
                {
                    UpdateDisplay();
                }
            }
        }

        private void Awake()
        {
            _inventoryService = ServiceLocator.Get<IInventoryService>();
        }

        public void Initialize()
        {
            // Setup click handler
            var button = gameObject.GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnCardClicked);

            if (_item != null)
            {
                UpdateDisplay();
            }
        }

        public void UpdateDisplay()
        {
            if (_item == null || _inventoryService == null)
                return;

            var definition = _inventoryService.GetItemDefinition(_item.DefinitionId);
            if (definition == null)
                return;

            // Set icon
            if (ItemIcon != null && definition.Icon != null)
            {
                ItemIcon.sprite = definition.Icon;
                ItemIcon.gameObject.SetActive(true);
            }
            else if (ItemIcon != null)
            {
                ItemIcon.gameObject.SetActive(false);
            }

            // Set rarity border color
            if (RarityBorder != null)
            {
                RarityBorder.color = definition.Rarity.GetColor();
            }

            // Set name
            if (NameText != null)
            {
                NameText.text = definition.DisplayName;
                NameText.color = definition.Rarity.GetColor();
            }

            // Set quantity (only show if > 1)
            if (QuantityText != null)
            {
                if (_item.Quantity > 1)
                {
                    QuantityText.text = $"x{_item.Quantity}";
                    QuantityText.gameObject.SetActive(true);
                }
                else
                {
                    QuantityText.gameObject.SetActive(false);
                }
            }

            // Show NEW indicator
            if (NewIndicator != null)
            {
                NewIndicator.gameObject.SetActive(_item.IsNew);
            }

            // Show equipped indicator
            if (EquippedIndicator != null)
            {
                if (_item.IsEquipped())
                {
                    EquippedIndicator.text = "E";
                    EquippedIndicator.gameObject.SetActive(true);
                }
                else
                {
                    EquippedIndicator.gameObject.SetActive(false);
                }
            }
        }

        public void SetHighlight(bool highlighted)
        {
            var background = GetComponent<Image>();
            if (background != null)
            {
                background.color = highlighted
                    ? new Color(0.3f, 0.5f, 0.8f, 1f)
                    : new Color(0.15f, 0.15f, 0.2f, 0.9f);
            }
        }

        private void OnCardClicked()
        {
            ParentView?.OnItemClicked(_item);
        }

        private void Start()
        {
            Initialize();
        }
    }
}