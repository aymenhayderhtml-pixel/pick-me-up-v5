using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemCard : MonoBehaviour
{
    public ItemInstance Item { get; private set; }
    public InventoryView ParentView { get; private set; }

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI newIndicatorText;
    [SerializeField] private TextMeshProUGUI equippedIndicatorText;

    private IInventoryService _inventoryService;
    private Button _button;

    private void Awake()
    {
        CacheReferences();
        ResolveService();
        CacheButton();
    }

    private void Start()
    {
        CacheReferences();
        ResolveService();
        CacheButton();
        RefreshDisplay();
    }

    public void SetupCard(ItemInstance item, InventoryView parentView)
    {
        Item = item;
        ParentView = parentView;
        CacheReferences();
        ResolveService();
        CacheButton();
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (Item == null || _inventoryService == null)
        {
            return;
        }

        ItemDefinition definition = _inventoryService.GetItemDefinition(Item.DefinitionId);
        if (definition == null)
        {
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = definition.Icon;
            itemIcon.enabled = definition.Icon != null;
        }

        if (rarityBorder != null)
        {
            rarityBorder.color = definition.Rarity.GetColor();
        }

        if (nameText != null)
        {
            nameText.text = definition.DisplayName;
            nameText.color = definition.Rarity.GetColor();
        }

        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(Item.Quantity > 1);
            quantityText.text = "x" + Item.Quantity;
        }

        if (newIndicatorText != null)
        {
            newIndicatorText.gameObject.SetActive(Item.IsNew);
            newIndicatorText.text = "NEW";
        }

        if (equippedIndicatorText != null)
        {
            equippedIndicatorText.gameObject.SetActive(Item.IsEquipped());
            equippedIndicatorText.text = "E";
        }
    }

    public void SetHighlight(bool highlighted)
    {
        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = highlighted ? new Color(0.3f, 0.5f, 0.8f, 1f) : new Color(0.15f, 0.15f, 0.2f, 0.9f);
        }
    }

    private void CacheReferences()
    {
        if (itemIcon == null)
        {
            Transform icon = transform.Find("ItemIcon");
            if (icon != null)
            {
                itemIcon = icon.GetComponent<Image>();
            }
        }

        if (rarityBorder == null)
        {
            Transform border = transform.Find("RarityBorder");
            if (border != null)
            {
                rarityBorder = border.GetComponent<Image>();
            }
        }

        if (nameText == null)
        {
            Transform name = transform.Find("Name");
            if (name != null)
            {
                nameText = name.GetComponent<TextMeshProUGUI>();
            }
        }

        if (quantityText == null)
        {
            Transform quantity = transform.Find("Quantity");
            if (quantity != null)
            {
                quantityText = quantity.GetComponent<TextMeshProUGUI>();
            }
        }

        if (newIndicatorText == null)
        {
            Transform indicator = transform.Find("NewIndicator");
            if (indicator != null)
            {
                newIndicatorText = indicator.GetComponent<TextMeshProUGUI>();
            }
        }

        if (equippedIndicatorText == null)
        {
            Transform equipped = transform.Find("EquippedIndicator");
            if (equipped != null)
            {
                equippedIndicatorText = equipped.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void ResolveService()
    {
        if (_inventoryService == null && ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IInventoryService>())
        {
            _inventoryService = ServiceRegistry.Instance.Resolve<IInventoryService>();
        }
    }

    private void CacheButton()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                _button = gameObject.AddComponent<Button>();
            }
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        if (ParentView != null && Item != null)
        {
            ParentView.OnItemClicked(Item);
        }
    }
}
