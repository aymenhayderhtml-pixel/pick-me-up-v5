using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotView : MonoBehaviour
{
    private GameObject weaponSlot;
    private GameObject armorSlot;
    private GameObject accessorySlot;

    private IInventoryService inventoryService;
    private HeroInstance currentHero;

    private void Awake()
    {
        // Find slots by name under EquipmentSection sibling
        Transform parent = transform.Find("EquipmentSection");
        if (parent == null)
        {
            // Fallback: scan children
            parent = transform;
        }
        weaponSlot    = FindSlot(parent, "Slot_Weapon");
        armorSlot     = FindSlot(parent, "Slot_Armor");
        accessorySlot = FindSlot(parent, "Slot_Accessory");
    }

    private GameObject FindSlot(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) return t.gameObject;
        // Search deeper
        foreach (Transform child in parent)
        {
            t = child.Find(name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    public void Setup(HeroInstance hero)
    {
        currentHero = hero;
        inventoryService = ServiceRegistry.Instance?.Resolve<IInventoryService>();
        Refresh();
    }

    public void Refresh()
    {
        if (currentHero == null) return;

        ClearSlot(weaponSlot);
        ClearSlot(armorSlot);
        ClearSlot(accessorySlot);

        if (inventoryService == null) return;

        // Weapon
        if (!string.IsNullOrEmpty(currentHero.WeaponId))
        {
            ItemInstance item = inventoryService.GetItem(currentHero.WeaponId);
            ItemDefinition def = item != null ? inventoryService.GetItemDefinition(item.DefinitionId) : null;
            PopulateSlot(weaponSlot, item, def);
        }

        // Armor
        if (!string.IsNullOrEmpty(currentHero.ArmorId))
        {
            ItemInstance item = inventoryService.GetItem(currentHero.ArmorId);
            ItemDefinition def = item != null ? inventoryService.GetItemDefinition(item.DefinitionId) : null;
            PopulateSlot(armorSlot, item, def);
        }

        // Accessory
        if (!string.IsNullOrEmpty(currentHero.AccessoryId))
        {
            ItemInstance item = inventoryService.GetItem(currentHero.AccessoryId);
            ItemDefinition def = item != null ? inventoryService.GetItemDefinition(item.DefinitionId) : null;
            PopulateSlot(accessorySlot, item, def);
        }
    }

    private void PopulateSlot(GameObject slotRoot, ItemInstance item, ItemDefinition def)
    {
        if (slotRoot == null) return;

        // Find or create icon
        Transform iconT = slotRoot.transform.Find("Icon");
        Image icon = iconT != null ? iconT.GetComponent<Image>() : null;

        Transform nameT = slotRoot.transform.Find("Name");
        TextMeshProUGUI nameText = nameT != null ? nameT.GetComponent<TextMeshProUGUI>() : null;

        Transform statsT = slotRoot.transform.Find("Stats");
        TextMeshProUGUI statsText = statsT != null ? statsT.GetComponent<TextMeshProUGUI>() : null;

        if (icon != null)
        {
            icon.sprite = def != null ? def.Icon : null;
            icon.color = def != null ? def.RarityColor : Color.gray;
        }

        if (nameText != null)
            nameText.text = def != null ? def.DisplayName : "Unknown";

        if (statsText != null && item != null)
        {
            statsText.text = "";
            if (item.BonusAttack > 0) statsText.text += "ATK+" + item.BonusAttack + " ";
            if (item.BonusDefense > 0) statsText.text += "DEF+" + item.BonusDefense + " ";
            if (item.BonusHealth > 0) statsText.text += "HP+" + item.BonusHealth + " ";
            if (item.BonusSpeed > 0) statsText.text += "SPD+" + item.BonusSpeed;
        }
    }

    private void ClearSlot(GameObject slotRoot)
    {
        if (slotRoot == null) return;

        Transform iconT = slotRoot.transform.Find("Icon");
        Image icon = iconT != null ? iconT.GetComponent<Image>() : null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        }

        Transform nameT = slotRoot.transform.Find("Name");
        TextMeshProUGUI nameText = nameT != null ? nameT.GetComponent<TextMeshProUGUI>() : null;
        if (nameText != null) nameText.text = "Empty";

        Transform statsT = slotRoot.transform.Find("Stats");
        TextMeshProUGUI statsText = statsT != null ? statsT.GetComponent<TextMeshProUGUI>() : null;
        if (statsText != null) statsText.text = "";
    }
}
