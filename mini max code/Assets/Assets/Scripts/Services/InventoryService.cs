using System;
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Game.Core;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.Services
{
    /// <summary>
    /// Service that handles inventory management including items, equipment, and consumption.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly ISaveLoadService _saveLoadService;
        private readonly IRosterService _rosterService;
        private readonly ICurrencyService _currencyService;

        private List<ItemInstance> _inventory;
        private Dictionary<string, ItemDefinition> _itemDefinitions;
        private const string ResourcesPath = "ScriptableObjects/Items";

        public InventoryService(
            ISaveLoadService saveLoadService,
            IRosterService rosterService,
            ICurrencyService currencyService)
        {
            _saveLoadService = saveLoadService;
            _rosterService = rosterService;
            _currencyService = currencyService;

            _inventory = new List<ItemInstance>();
            _itemDefinitions = new Dictionary<string, ItemDefinition>();

            LoadInventory();
            LoadItemDefinitions();
        }

        private void LoadInventory()
        {
            var saveData = _saveLoadService.LoadGame();

            _inventory.Clear();
            if (saveData.Items != null)
            {
                foreach (var item in saveData.Items)
                {
                    _inventory.Add(item);
                }
            }
        }

        private void LoadItemDefinitions()
        {
            _itemDefinitions.Clear();

            // Load all item definitions from resources
            var definitions = Resources.LoadAll<ItemDefinition>(ResourcesPath);

            foreach (var def in definitions)
            {
                if (!string.IsNullOrEmpty(def.ItemId))
                {
                    _itemDefinitions[def.ItemId] = def;
                }
            }

            // Ensure default items exist
            EnsureDefaultItems();
        }

        private void EnsureDefaultItems()
        {
            // Add default consumables if not present
            CreateDefaultItem("heal_potion_small", "Small Health Potion", ItemType.HealPotion, 1);
            CreateDefaultItem("heal_potion_large", "Large Health Potion", ItemType.HealPotion, 5);
            CreateDefaultItem("morale_booster", "Morale Booster", ItemType.MoraleBooster, 3);

            // Add default materials
            CreateDefaultItem("upgrade_stone", "Upgrade Stone", ItemType.UpgradeStone, 10);
            CreateDefaultItem("evolution_shard", "Evolution Shard", ItemType.EvolutionShard, 5);

            // Add dungeon loot items
            CreateDefaultItem("artifact_shard", "Artifact Shard", ItemType.ArtifactShard, 20);
            CreateDefaultItem("exp_scroll", "EXP Scroll", ItemType.ExpScroll, 15);
        }

        private void CreateDefaultItem(string id, string name, ItemType type, int stackSize)
        {
            if (!_itemDefinitions.ContainsKey(id))
            {
                var itemDef = ScriptableObject.CreateInstance<ItemDefinition>();
                itemDef.ItemId = id;
                itemDef.DisplayName = name;
                itemDef.ItemType = type;
                itemDef.MaxStackSize = stackSize;
                itemDef.Description = $"A {name.ToLower()} for various uses.";

                _itemDefinitions[id] = itemDef;
            }
        }

        private void SaveInventory()
        {
            var saveData = _saveLoadService.LoadGame();
            saveData.Items = _inventory;
            _saveLoadService.SaveGame(saveData);
        }

        public List<ItemInstance> GetAllItems()
        {
            return new List<ItemInstance>(_inventory);
        }

        public List<ItemInstance> GetItemsByType(ItemType type)
        {
            return _inventory.FindAll(item =>
            {
                var def = GetItemDefinition(item.DefinitionId);
                return def != null && def.ItemType == type;
            });
        }

        public List<ItemInstance> GetItemsByRarity(ItemRarity rarity)
        {
            return _inventory.FindAll(item =>
            {
                var def = GetItemDefinition(item.DefinitionId);
                return def != null && def.Rarity == rarity;
            });
        }

        public ItemInstance GetItem(string instanceId)
        {
            return _inventory.Find(item => item.InstanceId == instanceId);
        }

        public ItemDefinition GetItemDefinition(string definitionId)
        {
            return _itemDefinitions.ContainsKey(definitionId) ? _itemDefinitions[definitionId] : null;
        }

        public string CreateItemInstance(string definitionId, int quantity = 1)
        {
            if (!_itemDefinitions.ContainsKey(definitionId))
            {
                Debug.LogWarning($"Item definition not found: {definitionId}");
                return null;
            }

            var def = _itemDefinitions[definitionId];
            var item = new ItemInstance(definitionId)
            {
                Quantity = Mathf.Min(quantity, def.MaxStackSize),
                MaxStackSize = def.MaxStackSize
            };

            // Apply stat bonuses for equipment
            if (def.IsEquipable())
            {
                var bonuses = ItemDefinition.CreateStatBonuses(def);
                item.BonusHealth = bonuses.BonusHealth;
                item.BonusAttack = bonuses.BonusAttack;
                item.BonusDefense = bonuses.BonusDefense;
                item.BonusSpeed = bonuses.BonusSpeed;
                item.BonusCritRate = bonuses.BonusCritRate;
                item.BonusCritDamage = bonuses.BonusCritDamage;
            }

            _inventory.Add(item);
            SaveInventory();

            return item.InstanceId;
        }

        public bool AddItem(ItemInstance item)
        {
            if (item == null)
                return false;

            // Try to stack with existing items
            var existing = _inventory.Find(i =>
                i.DefinitionId == item.DefinitionId &&
                i.Quantity < i.MaxStackSize &&
                string.IsNullOrEmpty(i.EquippedToHeroId));

            if (existing != null)
            {
                int spaceAvailable = existing.MaxStackSize - existing.Quantity;
                int toAdd = Mathf.Min(item.Quantity, spaceAvailable);

                existing.Quantity += toAdd;
                item.Quantity -= toAdd;

                if (item.Quantity > 0)
                {
                    // Add remaining as new stack
                    item.InstanceId = Guid.NewGuid().ToString();
                    _inventory.Add(item);
                }
            }
            else
            {
                _inventory.Add(item);
            }

            SaveInventory();
            return true;
        }

        public bool RemoveItem(string instanceId, int quantity = 1)
        {
            var item = GetItem(instanceId);
            if (item == null)
                return false;

            if (item.IsEquipped())
            {
                Debug.LogWarning("Cannot remove equipped item. Unequip first.");
                return false;
            }

            if (item.Quantity > quantity)
            {
                item.Quantity -= quantity;
            }
            else
            {
                _inventory.Remove(item);
            }

            SaveInventory();
            return true;
        }

        public UseItemResult UseItem(string instanceId, string heroId = null)
        {
            var result = new UseItemResult();
            var item = GetItem(instanceId);

            if (item == null)
            {
                result.Success = false;
                result.Message = "Item not found.";
                return result;
            }

            var def = GetItemDefinition(item.DefinitionId);
            if (def == null || !def.IsConsumable())
            {
                result.Success = false;
                result.Message = "Item cannot be used.";
                return result;
            }

            // Handle different consumable types
            switch (def.ItemType)
            {
                case ItemType.HealPotion:
                    result = UseHealPotion(item, def, heroId);
                    break;

                case ItemType.MoraleBooster:
                    result = UseMoraleBooster(item, def, heroId);
                    break;

                case ItemType.ExpScroll:
                    result = UseExpScroll(item, def, heroId);
                    break;

                default:
                    result.Success = false;
                    result.Message = "Unknown consumable type.";
                    return result;
            }

            if (result.Success)
            {
                RemoveItem(instanceId, 1);
            }

            return result;
        }

        private UseItemResult UseHealPotion(ItemInstance item, ItemDefinition def, string heroId)
        {
            var result = new UseItemResult
            {
                Success = false
            };

            if (string.IsNullOrEmpty(heroId))
            {
                result.Message = "Please select a hero.";
                return result;
            }

            var hero = _rosterService.GetHero(heroId);
            if (hero == null)
            {
                result.Message = "Hero not found.";
                return result;
            }

            int healAmount = def.HealAmount;
            int actualHeal = Mathf.Min(healAmount, hero.MaxHealth - hero.CurrentHealth);

            hero.CurrentHealth = Mathf.Min(hero.MaxHealth, hero.CurrentHealth + healAmount);
            _rosterService.UpdateHero(hero);

            result.Success = true;
            result.HealthRestored = actualHeal;
            result.Message = $"{hero.HeroName} recovered {actualHeal} HP.";
            result.ConsumedItemId = item.InstanceId;

            return result;
        }

        private UseItemResult UseMoraleBooster(ItemInstance item, ItemDefinition def, string heroId)
        {
            var result = new UseItemResult
            {
                Success = false
            };

            if (string.IsNullOrEmpty(heroId))
            {
                result.Message = "Please select a hero.";
                return result;
            }

            var hero = _rosterService.GetHero(heroId);
            if (hero == null)
            {
                result.Message = "Hero not found.";
                return result;
            }

            int moraleRestore = def.MoraleRestoreAmount;
            int actualRestore = Mathf.Min(moraleRestore, 100 - hero.CurrentMorale);

            hero.CurrentMorale = Mathf.Min(100, hero.CurrentMorale + moraleRestore);
            _rosterService.UpdateHero(hero);

            result.Success = true;
            result.MoraleRestored = actualRestore;
            result.Message = $"{hero.HeroName}'s morale increased by {actualRestore}.";
            result.ConsumedItemId = item.InstanceId;

            return result;
        }

        private UseItemResult UseExpScroll(ItemInstance item, ItemDefinition def, string heroId)
        {
            var result = new UseItemResult
            {
                Success = false
            };

            if (string.IsNullOrEmpty(heroId))
            {
                result.Message = "Please select a hero.";
                return result;
            }

            var hero = _rosterService.GetHero(heroId);
            if (hero == null)
            {
                result.Message = "Hero not found.";
                return result;
            }

            // EXP scrolls give flat EXP (simplified system)
            int expGained = def.HealAmount; // Reusing HealAmount for EXP amount
            hero.CurrentExp += expGained;

            // Check for level up
            while (hero.CurrentExp >= hero.RequiredExp)
            {
                hero.CurrentExp -= hero.RequiredExp;
                hero.Level++;
                hero.Attack += 5;
                hero.Defense += 3;
                hero.MaxHealth += 20;
                hero.CurrentHealth = hero.MaxHealth; // Full heal on level up
                hero.RequiredExp = CalculateRequiredExp(hero.Level);
            }

            _rosterService.UpdateHero(hero);

            result.Success = true;
            result.Message = $"{hero.HeroName} gained {expGained} EXP!";
            result.ConsumedItemId = item.InstanceId;

            return result;
        }

        private int CalculateRequiredExp(int level)
        {
            return 100 + (level * 50);
        }

        public bool EquipItem(string itemInstanceId, string heroInstanceId)
        {
            var item = GetItem(itemInstanceId);
            if (item == null)
            {
                Debug.LogWarning("Item not found.");
                return false;
            }

            var def = GetItemDefinition(item.DefinitionId);
            if (def == null || !def.IsEquipable())
            {
                Debug.LogWarning("Item is not equipable.");
                return false;
            }

            var hero = _rosterService.GetHero(heroInstanceId);
            if (hero == null)
            {
                Debug.LogWarning("Hero not found.");
                return false;
            }

            // Unequip current item in that slot if any
            string slot = def.GetEquipmentSlot();
            string currentItemId = GetEquippedItemInSlot(heroInstanceId, slot);

            if (!string.IsNullOrEmpty(currentItemId))
            {
                UnequipItem(currentItemId);
            }

            // Equip new item
            item.EquippedToHeroId = heroInstanceId;

            // Update hero's equipment references
            switch (def.ItemType)
            {
                case ItemType.Weapon:
                    hero.WeaponId = itemInstanceId;
                    break;
                case ItemType.Armor:
                    hero.ArmorId = itemInstanceId;
                    break;
                case ItemType.Accessory:
                    hero.AccessoryId = itemInstanceId;
                    break;
            }

            _rosterService.UpdateHero(hero);
            SaveInventory();

            return true;
        }

        public bool UnequipItem(string itemInstanceId)
        {
            var item = GetItem(itemInstanceId);
            if (item == null)
            {
                return false;
            }

            if (!item.IsEquipped())
            {
                return true; // Already unequipped
            }

            string heroId = item.EquippedToHeroId;
            var hero = _rosterService.GetHero(heroId);
            if (hero == null)
            {
                Debug.LogWarning("Hero not found for equipped item.");
                return false;
            }

            var def = GetItemDefinition(item.DefinitionId);
            if (def != null)
            {
                // Clear hero's equipment reference
                switch (def.ItemType)
                {
                    case ItemType.Weapon:
                        hero.WeaponId = null;
                        break;
                    case ItemType.Armor:
                        hero.ArmorId = null;
                        break;
                    case ItemType.Accessory:
                        hero.AccessoryId = null;
                        break;
                }

                _rosterService.UpdateHero(hero);
            }

            item.EquippedToHeroId = null;
            SaveInventory();

            return true;
        }

        public List<ItemInstance> GetEquippedItems(string heroInstanceId)
        {
            var equipped = new List<ItemInstance>();

            foreach (var item in _inventory)
            {
                if (item.EquippedToHeroId == heroInstanceId)
                {
                    equipped.Add(item);
                }
            }

            return equipped;
        }

        private string GetEquippedItemInSlot(string heroInstanceId, string slot)
        {
            switch (slot)
            {
                case "Weapon":
                    var hero = _rosterService.GetHero(heroInstanceId);
                    return hero?.WeaponId;
                case "Armor":
                    return _inventory.Find(i => i.EquippedToHeroId == heroInstanceId &&
                        GetItemDefinition(i.DefinitionId)?.ItemType == ItemType.Armor)?.InstanceId;
                case "Accessory":
                    return _inventory.Find(i => i.EquippedToHeroId == heroInstanceId &&
                        GetItemDefinition(i.DefinitionId)?.ItemType == ItemType.Accessory)?.InstanceId;
                default:
                    return null;
            }
        }

        public ItemStatBonus GetHeroEquipmentBonus(string heroInstanceId)
        {
            var bonus = new ItemStatBonus();
            var equipped = GetEquippedItems(heroInstanceId);

            foreach (var item in equipped)
            {
                bonus.TotalHealth += item.BonusHealth;
                bonus.TotalAttack += item.BonusAttack;
                bonus.TotalDefense += item.BonusDefense;
                bonus.TotalSpeed += item.BonusSpeed;
                bonus.TotalCritRate += item.BonusCritRate;
                bonus.TotalCritDamage += item.BonusCritDamage;
                bonus.EquippedItemIds.Add(item.InstanceId);
            }

            return bonus;
        }

        public bool SellItem(string instanceId, int quantity = 1)
        {
            var item = GetItem(instanceId);
            if (item == null)
            {
                return false;
            }

            if (item.IsEquipped())
            {
                Debug.LogWarning("Cannot sell equipped item.");
                return false;
            }

            var def = GetItemDefinition(item.DefinitionId);
            if (def == null)
            {
                return false;
            }

            int sellValue = def.SellValue * quantity;

            if (RemoveItem(instanceId, quantity))
            {
                _currencyService.AddGold(sellValue);
                return true;
            }

            return false;
        }

        public int GetItemCount(string definitionId)
        {
            int total = 0;
            foreach (var item in _inventory)
            {
                if (item.DefinitionId == definitionId && !item.IsEquipped())
                {
                    total += item.Quantity;
                }
            }
            return total;
        }

        public bool HasItem(string definitionId, int quantity = 1)
        {
            return GetItemCount(definitionId) >= quantity;
        }

        public void MarkAsSeen(string instanceId)
        {
            var item = GetItem(instanceId);
            if (item != null)
            {
                item.IsNew = false;
                SaveInventory();
            }
        }

        public void ClearAllNewFlags()
        {
            foreach (var item in _inventory)
            {
                item.IsNew = false;
            }
            SaveInventory();
        }
    }
}