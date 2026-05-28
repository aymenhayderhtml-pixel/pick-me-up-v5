using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDefinition", menuName = "PickMeUp/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string ItemId;
    public string DisplayName;
    public string Description;
    public ItemType ItemType;
    public ItemRarity Rarity = ItemRarity.Common;

    [Header("Stack Info")]
    public int MaxStackSize = 99;

    [Header("Stats (for equipment)")]
    public int BaseHealth;
    public int BaseAttack;
    public int BaseDefense;
    public int BaseSpeed;
    public float BaseCritRate;
    public float BaseCritDamage;

    [Header("Effect (for consumables)")]
    public int HealAmount;
    public int MoraleRestoreAmount;
    public float EffectDuration;

    [Header("Visual")]
    public Sprite Icon;
    public string IconPath;
    public Color RarityColor = Color.white;

    [Header("Crafting (for materials)")]
    public bool IsCraftable;
    public RecipeData CraftingRecipe;

    [Header("Sell Value")]
    public int SellValue;

    public ItemInstance CreateInstance(int quantity = 1)
    {
        ItemInstance instance = new ItemInstance(ItemId)
        {
            Quantity = Mathf.Max(1, quantity),
            MaxStackSize = MaxStackSize
        };

        float rarityMultiplier = GetRarityMultiplier(Rarity);
        instance.BonusHealth = Mathf.CeilToInt(BaseHealth * rarityMultiplier);
        instance.BonusAttack = Mathf.CeilToInt(BaseAttack * rarityMultiplier);
        instance.BonusDefense = Mathf.CeilToInt(BaseDefense * rarityMultiplier);
        instance.BonusSpeed = Mathf.CeilToInt(BaseSpeed * rarityMultiplier);
        instance.BonusCritRate = BaseCritRate * rarityMultiplier;
        instance.BonusCritDamage = BaseCritDamage * rarityMultiplier;
        return instance;
    }

    public bool IsEquipable()
    {
        return ItemType == ItemType.Weapon || ItemType == ItemType.Armor || ItemType == ItemType.Accessory;
    }

    public bool IsConsumable()
    {
        return ItemType == ItemType.Consumable || ItemType == ItemType.HealPotion || ItemType == ItemType.MoraleBooster;
    }

    public string GetEquipmentSlot()
    {
        switch (ItemType)
        {
            case ItemType.Weapon:
                return "Weapon";
            case ItemType.Armor:
                return "Armor";
            case ItemType.Accessory:
                return "Accessory";
            default:
                return null;
        }
    }

    private float GetRarityMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return 1.0f;
            case ItemRarity.Uncommon:
                return 1.25f;
            case ItemRarity.Rare:
                return 1.5f;
            case ItemRarity.Epic:
                return 1.75f;
            case ItemRarity.Legendary:
                return 2.0f;
            default:
                return 1.0f;
        }
    }
}

[Serializable]
public class RecipeData
{
    public string[] RequiredItemIds;
    public int[] RequiredQuantities;
    public int GoldCost;
    public float CraftTime;
}
