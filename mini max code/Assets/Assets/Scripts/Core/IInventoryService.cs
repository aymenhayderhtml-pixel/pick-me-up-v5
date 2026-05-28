using System;
using System.Collections.Generic;

namespace PickMeUp.Game.Core
{
    /// <summary>
    /// Interface for inventory service that manages player items.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Gets all items in the inventory.
        /// </summary>
        List<ItemInstance> GetAllItems();

        /// <summary>
        /// Gets items filtered by type.
        /// </summary>
        List<ItemInstance> GetItemsByType(ItemType type);

        /// <summary>
        /// Gets items filtered by rarity.
        /// </summary>
        List<ItemInstance> GetItemsByRarity(ItemRarity rarity);

        /// <summary>
        /// Gets a specific item instance by ID.
        /// </summary>
        ItemInstance GetItem(string instanceId);

        /// <summary>
        /// Gets the definition for an item type.
        /// </summary>
        ScriptableObjects.ItemDefinition GetItemDefinition(string definitionId);

        /// <summary>
        /// Creates a new item instance and adds it to inventory.
        /// Returns the instance ID of the created item.
        /// </summary>
        string CreateItemInstance(string definitionId, int quantity = 1);

        /// <summary>
        /// Adds an existing item instance to inventory.
        /// Handles stacking if possible.
        /// </summary>
        bool AddItem(ItemInstance item);

        /// <summary>
        /// Removes an item from inventory.
        /// </summary>
        bool RemoveItem(string instanceId, int quantity = 1);

        /// <summary>
        /// Uses a consumable item.
        /// </summary>
        UseItemResult UseItem(string instanceId, string heroId = null);

        /// <summary>
        /// Equips an item to a hero.
        /// </summary>
        bool EquipItem(string itemInstanceId, string heroInstanceId);

        /// <summary>
        /// Unequips an item from a hero.
        /// </summary>
        bool UnequipItem(string itemInstanceId);

        /// <summary>
        /// Gets all equipped items for a hero.
        /// </summary>
        List<ItemInstance> GetEquippedItems(string heroInstanceId);

        /// <summary>
        /// Gets total stat bonuses for a hero from equipment.
        /// </summary>
        ItemStatBonus GetHeroEquipmentBonus(string heroInstanceId);

        /// <summary>
        /// Sells an item for gold.
        /// </summary>
        bool SellItem(string instanceId, int quantity = 1);

        /// <summary>
        /// Gets item count by definition ID.
        /// </summary>
        int GetItemCount(string definitionId);

        /// <summary>
        /// Checks if player has enough of a specific item.
        /// </summary>
        bool HasItem(string definitionId, int quantity = 1);

        /// <summary>
        /// Marks an item as no longer new.
        /// </summary>
        void MarkAsSeen(string instanceId);

        /// <summary>
        /// Clears all new flags.
        /// </summary>
        void ClearAllNewFlags();
    }

    /// <summary>
    /// Result of using an item.
    /// </summary>
    [Serializable]
    public class UseItemResult
    {
        public bool Success;
        public string Message;
        public int HealthRestored;
        public int MoraleRestored;
        public string ConsumedItemId;
    }

    /// <summary>
    /// Combined stat bonuses from equipment.
    /// </summary>
    [Serializable]
    public class ItemStatBonus
    {
        public int TotalHealth;
        public int TotalAttack;
        public int TotalDefense;
        public int TotalSpeed;
        public float TotalCritRate;
        public float TotalCritDamage;
        public List<string> EquippedItemIds;

        public ItemStatBonus()
        {
            EquippedItemIds = new List<string>();
        }

        public void Clear()
        {
            TotalHealth = 0;
            TotalAttack = 0;
            TotalDefense = 0;
            TotalSpeed = 0;
            TotalCritRate = 0f;
            TotalCritDamage = 0f;
            EquippedItemIds.Clear();
        }
    }
}