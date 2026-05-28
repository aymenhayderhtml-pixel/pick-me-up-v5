using System;
using UnityEngine;

namespace PickMeUp.Game.Core
{
    /// <summary>
    /// Serializable representation of an item instance in the player's inventory.
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        [Header("Identity")]
        public string InstanceId;
        public string DefinitionId;

        [Header("Stack Info")]
        public int Quantity = 1;
        public int MaxStackSize = 99;

        [Header("Metadata")]
        public DateTime AcquiredTime;
        public bool IsNew = true;

        [Header("Equipment (if applicable)")]
        public string EquippedToHeroId;

        // Stat bonuses from equipment
        public int BonusHealth;
        public int BonusAttack;
        public int BonusDefense;
        public int BonusSpeed;
        public float BonusCritRate;
        public float BonusCritDamage;

        public ItemInstance()
        {
            InstanceId = Guid.NewGuid().ToString();
            AcquiredTime = DateTime.Now;
        }

        public ItemInstance(string definitionId) : this()
        {
            DefinitionId = definitionId;
        }

        /// <summary>
        /// Checks if this item is stackable with other items of the same type.
        /// </summary>
        public bool IsStackable()
        {
            return Quantity < MaxStackSize;
        }

        /// <summary>
        /// Tries to add quantity to this stack.
        /// </summary>
        public bool TryAddQuantity(int amount)
        {
            if (Quantity + amount <= MaxStackSize)
            {
                Quantity += amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes quantity from this stack.
        /// </summary>
        public bool TryRemoveQuantity(int amount)
        {
            if (Quantity >= amount)
            {
                Quantity -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if this item is currently equipped.
        /// </summary>
        public bool IsEquipped()
        {
            return !string.IsNullOrEmpty(EquippedToHeroId);
        }

        /// <summary>
        /// Unequips this item from any hero.
        /// </summary>
        public void Unequip()
        {
            EquippedToHeroId = null;
        }

        /// <summary>
        /// Creates a copy of this item instance.
        /// </summary>
        public ItemInstance Clone()
        {
            return new ItemInstance
            {
                InstanceId = Guid.NewGuid().ToString(),
                DefinitionId = DefinitionId,
                Quantity = Quantity,
                MaxStackSize = MaxStackSize,
                AcquiredTime = DateTime.Now,
                IsNew = true,
                EquippedToHeroId = null,
                BonusHealth = BonusHealth,
                BonusAttack = BonusAttack,
                BonusDefense = BonusDefense,
                BonusSpeed = BonusSpeed,
                BonusCritRate = BonusCritRate,
                BonusCritDamage = BonusCritDamage
            };
        }
    }

    /// <summary>
    /// Types of items in the game.
    /// </summary>
    public enum ItemType
    {
        None,
        // Equipment
        Weapon,
        Armor,
        Accessory,
        // Consumables
        Consumable,
        HealPotion,
        MoraleBooster,
        // Materials
        Material,
        UpgradeStone,
        EvolutionShard,
        ArtifactShard,
        ExpScroll,
        // Miscellaneous
        KeyItem,
        Currency
    }

    /// <summary>
    /// Rarity levels for items.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Extension methods for ItemRarity.
    /// </summary>
    public static class ItemRarityExtensions
    {
        public static Color GetColor(this ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // Gray
                case ItemRarity.Uncommon:
                    return new Color(0.3f, 0.9f, 0.3f); // Green
                case ItemRarity.Rare:
                    return new Color(0.3f, 0.5f, 0.9f); // Blue
                case ItemRarity.Epic:
                    return new Color(0.7f, 0.3f, 0.9f); // Purple
                case ItemRarity.Legendary:
                    return new Color(1f, 0.6f, 0.1f); // Orange/Gold
                default:
                    return Color.white;
            }
        }

        public static string GetDisplayName(this ItemRarity rarity)
        {
            return rarity.ToString();
        }
    }
}