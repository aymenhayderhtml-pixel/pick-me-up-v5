using System;
using System.Collections.Generic;

public interface IInventoryService
{
    List<ItemInstance> GetAllItems();
    List<ItemInstance> GetItemsByType(ItemType type);
    List<ItemInstance> GetItemsByRarity(ItemRarity rarity);
    ItemInstance GetItem(string instanceId);
    ItemDefinition GetItemDefinition(string definitionId);
    string CreateItemInstance(string definitionId, int quantity = 1);
    bool AddItem(ItemInstance item);
    bool RemoveItem(string instanceId, int quantity = 1);
    UseItemResult UseItem(string instanceId, string heroId = null);
    bool EquipItem(string itemInstanceId, string heroInstanceId);
    bool UnequipItem(string itemInstanceId);
    List<ItemInstance> GetEquippedItems(string heroInstanceId);
    ItemStatBonus GetHeroEquipmentBonus(string heroInstanceId);
    bool SellItem(string instanceId, int quantity = 1);
    int GetItemCount(string definitionId);
    bool HasItem(string definitionId, int quantity = 1);
    void MarkAsSeen(string instanceId);
    void ClearAllNewFlags();
}

[Serializable]
public class UseItemResult
{
    public bool Success;
    public string Message;
    public int HealthRestored;
    public int MoraleRestored;
    public string ConsumedItemId;
}

[Serializable]
public class ItemStatBonus
{
    public int TotalHealth;
    public int TotalAttack;
    public int TotalDefense;
    public int TotalSpeed;
    public float TotalCritRate;
    public float TotalCritDamage;
    public List<string> EquippedItemIds = new List<string>();

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
