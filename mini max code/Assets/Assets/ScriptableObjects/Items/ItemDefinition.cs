using System;
using UnityEngine;

namespace PickMeUp.Game.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject definition for an item type in the game.
    /// Contains the template data for all items of this type.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "PickMeUp/Item Definition")]
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
        public float EffectDuration; // in seconds

        [Header("Visual")]
        public Sprite Icon;
        public string IconPath;
        public Color RarityColor = Color.white;

        [Header("Crafting (for materials)")]
        public bool IsCraftable;
        public RecipeData CraftingRecipe;

        [Header("Sell Value")]
        public int SellValue; // in gold

        /// <summary>
        /// Creates the stat bonuses for an item instance.
        /// </summary>
        public static Core.ItemInstance CreateStatBonuses(ItemDefinition definition)
        {
            var instance = new Core.ItemInstance
            {
                DefinitionId = definition.ItemId,
                MaxStackSize = definition.MaxStackSize
            };

            // Apply stat bonuses based on rarity
            float rarityMultiplier = GetRarityMultiplier(definition.Rarity);

            instance.BonusHealth = Mathf.CeilToInt(definition.BaseHealth * rarityMultiplier);
            instance.BonusAttack = Mathf.CeilToInt(definition.BaseAttack * rarityMultiplier);
            instance.BonusDefense = Mathf.CeilToInt(definition.BaseDefense * rarityMultiplier);
            instance.BonusSpeed = Mathf.CeilToInt(definition.BaseSpeed * rarityMultiplier);
            instance.BonusCritRate = definition.BaseCritRate * rarityMultiplier;
            instance.BonusCritDamage = definition.BaseCritDamage * rarityMultiplier;

            return instance;
        }

        private static float GetRarityMultiplier(ItemRarity rarity)
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

        /// <summary>
        /// Checks if this item can be equipped to heroes.
        /// </summary>
        public bool IsEquipable()
        {
            return ItemType == ItemType.Weapon ||
                   ItemType == ItemType.Armor ||
                   ItemType == ItemType.Accessory;
        }

        /// <summary>
        /// Checks if this item is a consumable.
        /// </summary>
        public bool IsConsumable()
        {
            return ItemType == ItemType.Consumable ||
                   ItemType == ItemType.HealPotion ||
                   ItemType == ItemType.MoraleBooster;
        }

        /// <summary>
        /// Gets the slot type this equipment occupies.
        /// </summary>
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
    }

    /// <summary>
    /// Crafting recipe data for items.
    /// </summary>
    [Serializable]
    public class RecipeData
    {
        public string[] RequiredItemIds;
        public int[] RequiredQuantities;
        public int GoldCost;
        public float CraftTime; // in seconds
    }
}