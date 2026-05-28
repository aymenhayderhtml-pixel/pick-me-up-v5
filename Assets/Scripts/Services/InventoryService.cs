using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryService : IInventoryService
{
    private static readonly string[] ResourcePaths = { "Items", "ScriptableObjects/Items" };

    private readonly GameStateService _gameState;
    private readonly IRosterService _roster;
    private readonly ICurrencyService _currency;
    private readonly Dictionary<string, ItemDefinition> _definitionCache = new Dictionary<string, ItemDefinition>();
    private List<ItemDefinition> _definitions;

    public InventoryService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _definitions = LoadDefinitions();
        CacheDefinitions();
    }

    public List<ItemInstance> GetAllItems()
    {
        EnsureState();
        return new List<ItemInstance>(_gameState.Data.Items);
    }

    public List<ItemInstance> GetItemsByType(ItemType type)
    {
        return GetAllItems().FindAll(item =>
        {
            ItemDefinition def = GetItemDefinition(item.DefinitionId);
            return def != null && def.ItemType == type;
        });
    }

    public List<ItemInstance> GetItemsByRarity(ItemRarity rarity)
    {
        return GetAllItems().FindAll(item =>
        {
            ItemDefinition def = GetItemDefinition(item.DefinitionId);
            return def != null && def.Rarity == rarity;
        });
    }

    public ItemInstance GetItem(string instanceId)
    {
        return GetAllItems().Find(item => item.InstanceId == instanceId);
    }

    public ItemDefinition GetItemDefinition(string definitionId)
    {
        if (string.IsNullOrEmpty(definitionId))
        {
            return null;
        }

        ItemDefinition def;
        if (_definitionCache.TryGetValue(definitionId, out def))
        {
            return def;
        }

        def = _definitions.Find(d => d != null && d.ItemId == definitionId);
        if (def != null)
        {
            _definitionCache[definitionId] = def;
        }

        return def;
    }

    public string CreateItemInstance(string definitionId, int quantity = 1)
    {
        ItemDefinition def = GetItemDefinition(definitionId);
        if (def == null)
        {
            return null;
        }

        ItemInstance item = new ItemInstance(definitionId)
        {
            Quantity = Mathf.Max(1, quantity),
            MaxStackSize = def.MaxStackSize
        };

        ApplyDefinitionBonuses(item, def);
        _gameState.Data.Items.Add(item);
        _gameState.Save();
        return item.InstanceId;
    }

    public bool AddItem(ItemInstance item)
    {
        if (item == null)
        {
            return false;
        }

        EnsureState();
        ItemDefinition def = GetItemDefinition(item.DefinitionId);
        if (def != null)
        {
            item.MaxStackSize = def.MaxStackSize;
            ApplyDefinitionBonuses(item, def);
        }

        var stack = _gameState.Data.Items.Find(existing => existing.DefinitionId == item.DefinitionId && existing.IsStackable());
        if (stack != null && stack.TryAddQuantity(item.Quantity))
        {
            _gameState.Save();
            return true;
        }

        _gameState.Data.Items.Add(item.Clone());
        _gameState.Save();
        return true;
    }

    public bool RemoveItem(string instanceId, int quantity = 1)
    {
        EnsureState();
        ItemInstance item = GetItem(instanceId);
        if (item == null)
        {
            return false;
        }

        if (quantity <= 0 || item.Quantity <= quantity)
        {
            _gameState.Data.Items.RemoveAll(entry => entry.InstanceId == instanceId);
            _gameState.Save();
            return true;
        }

        item.Quantity -= quantity;
        _gameState.Save();
        return true;
    }

    public UseItemResult UseItem(string instanceId, string heroId = null)
    {
        ItemInstance item = GetItem(instanceId);
        ItemDefinition def = item != null ? GetItemDefinition(item.DefinitionId) : null;
        if (item == null || def == null || !def.IsConsumable())
        {
            return new UseItemResult { Success = false, Message = "Item cannot be used." };
        }

        HeroInstance hero = null;
        if (!string.IsNullOrEmpty(heroId))
        {
            hero = _roster.GetAll().Find(h => h.InstanceId == heroId);
        }

        if (hero == null)
        {
            hero = _roster.GetAlive().FirstOrDefault();
        }

        if (hero != null && def.MoraleRestoreAmount != 0)
        {
            hero.Morale = Mathf.Clamp(hero.Morale + def.MoraleRestoreAmount, 0, 100);
        }

        if (hero != null && def.HealAmount > 0)
        {
            // No HP system in the current core model, so treat heal items as morale support.
            hero.Morale = Mathf.Clamp(hero.Morale + Mathf.Max(1, def.HealAmount / 10), 0, 100);
        }

        RemoveItem(instanceId, 1);
        _gameState.Save();

        return new UseItemResult
        {
            Success = true,
            Message = "Item used.",
            HealthRestored = def.HealAmount,
            MoraleRestored = def.MoraleRestoreAmount,
            ConsumedItemId = instanceId
        };
    }

    public bool EquipItem(string itemInstanceId, string heroInstanceId)
    {
        ItemInstance item = GetItem(itemInstanceId);
        ItemDefinition def = item != null ? GetItemDefinition(item.DefinitionId) : null;
        HeroInstance hero = _roster.GetAll().Find(h => h.InstanceId == heroInstanceId);
        if (item == null || def == null || hero == null || !def.IsEquipable())
        {
            return false;
        }

        UnequipSlot(hero, def.GetEquipmentSlot());
        if (!string.IsNullOrEmpty(item.EquippedToHeroId))
        {
            UnequipItem(item.InstanceId);
        }

        item.EquippedToHeroId = hero.InstanceId;
        SetHeroEquipmentSlot(hero, def.GetEquipmentSlot(), item.InstanceId);
        _gameState.Save();
        return true;
    }

    public bool UnequipItem(string itemInstanceId)
    {
        ItemInstance item = GetItem(itemInstanceId);
        if (item == null)
        {
            return false;
        }

        HeroInstance hero = _roster.GetAll().Find(h => h.InstanceId == item.EquippedToHeroId);
        if (hero != null)
        {
            ClearHeroEquipmentSlot(hero, itemInstanceId);
        }

        item.Unequip();
        _gameState.Save();
        return true;
    }

    public List<ItemInstance> GetEquippedItems(string heroInstanceId)
    {
        return GetAllItems().FindAll(item => item.EquippedToHeroId == heroInstanceId);
    }

    public ItemStatBonus GetHeroEquipmentBonus(string heroInstanceId)
    {
        ItemStatBonus bonus = new ItemStatBonus();
        foreach (ItemInstance item in GetEquippedItems(heroInstanceId))
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
        ItemInstance item = GetItem(instanceId);
        ItemDefinition def = item != null ? GetItemDefinition(item.DefinitionId) : null;
        if (item == null || def == null)
        {
            return false;
        }

        int sellQty = Mathf.Clamp(quantity, 1, item.Quantity);
        int gold = Mathf.Max(0, def.SellValue * sellQty);
        _currency.AddGold(gold);
        RemoveItem(instanceId, sellQty);
        return true;
    }

    public int GetItemCount(string definitionId)
    {
        return GetAllItems().Where(item => item.DefinitionId == definitionId).Sum(item => item.Quantity);
    }

    public bool HasItem(string definitionId, int quantity = 1)
    {
        return GetItemCount(definitionId) >= quantity;
    }

    public void MarkAsSeen(string instanceId)
    {
        ItemInstance item = GetItem(instanceId);
        if (item == null)
        {
            return;
        }

        item.IsNew = false;
        _gameState.Save();
    }

    public void ClearAllNewFlags()
    {
        foreach (ItemInstance item in GetAllItems())
        {
            item.IsNew = false;
        }

        _gameState.Save();
    }

    private void ApplyDefinitionBonuses(ItemInstance item, ItemDefinition def)
    {
        if (item == null || def == null)
        {
            return;
        }

        float rarityMultiplier = GetRarityMultiplier(def.Rarity);
        item.BonusHealth = Mathf.CeilToInt(def.BaseHealth * rarityMultiplier);
        item.BonusAttack = Mathf.CeilToInt(def.BaseAttack * rarityMultiplier);
        item.BonusDefense = Mathf.CeilToInt(def.BaseDefense * rarityMultiplier);
        item.BonusSpeed = Mathf.CeilToInt(def.BaseSpeed * rarityMultiplier);
        item.BonusCritRate = def.BaseCritRate * rarityMultiplier;
        item.BonusCritDamage = def.BaseCritDamage * rarityMultiplier;
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

    private void EnsureState()
    {
        if (_gameState.Data.Items == null)
        {
            _gameState.Data.Items = new List<ItemInstance>();
        }
    }

    private List<ItemDefinition> LoadDefinitions()
    {
        foreach (string path in ResourcePaths)
        {
            ItemDefinition[] loaded = Resources.LoadAll<ItemDefinition>(path);
            if (loaded != null && loaded.Length > 0)
            {
                return new List<ItemDefinition>(loaded);
            }
        }

        return new List<ItemDefinition>();
    }

    private void CacheDefinitions()
    {
        foreach (ItemDefinition def in _definitions)
        {
            if (def != null && !_definitionCache.ContainsKey(def.ItemId))
            {
                _definitionCache.Add(def.ItemId, def);
            }
        }
    }

    private void SetHeroEquipmentSlot(HeroInstance hero, string slot, string itemId)
    {
        if (hero == null)
        {
            return;
        }

        switch (slot)
        {
            case "Weapon":
                hero.WeaponId = itemId;
                break;
            case "Armor":
                hero.ArmorId = itemId;
                break;
            case "Accessory":
                hero.AccessoryId = itemId;
                break;
        }
    }

    private void ClearHeroEquipmentSlot(HeroInstance hero, string itemId)
    {
        if (hero == null)
        {
            return;
        }

        if (hero.WeaponId == itemId)
        {
            hero.WeaponId = string.Empty;
        }

        if (hero.ArmorId == itemId)
        {
            hero.ArmorId = string.Empty;
        }

        if (hero.AccessoryId == itemId)
        {
            hero.AccessoryId = string.Empty;
        }
    }

    private void UnequipSlot(HeroInstance hero, string slot)
    {
        if (hero == null)
        {
            return;
        }

        string itemId = null;
        switch (slot)
        {
            case "Weapon":
                itemId = hero.WeaponId;
                hero.WeaponId = string.Empty;
                break;
            case "Armor":
                itemId = hero.ArmorId;
                hero.ArmorId = string.Empty;
                break;
            case "Accessory":
                itemId = hero.AccessoryId;
                hero.AccessoryId = string.Empty;
                break;
        }

        if (!string.IsNullOrEmpty(itemId))
        {
            ItemInstance oldItem = GetItem(itemId);
            if (oldItem != null)
            {
                oldItem.Unequip();
            }
        }
    }
}
