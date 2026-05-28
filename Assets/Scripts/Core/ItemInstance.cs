using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public string InstanceId;
    public string DefinitionId;
    public int Quantity = 1;
    public int MaxStackSize = 99;
    public long AcquiredTimestampTicks;
    public bool IsNew = true;
    public string EquippedToHeroId;
    public int BonusHealth;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public float BonusCritRate;
    public float BonusCritDamage;

    public ItemInstance()
    {
        InstanceId = Guid.NewGuid().ToString();
        AcquiredTimestampTicks = DateTime.UtcNow.Ticks;
    }

    public ItemInstance(string definitionId) : this()
    {
        DefinitionId = definitionId;
    }

    public bool IsStackable()
    {
        return Quantity < MaxStackSize;
    }

    public bool TryAddQuantity(int amount)
    {
        if (Quantity + amount > MaxStackSize)
        {
            return false;
        }

        Quantity += amount;
        return true;
    }

    public bool TryRemoveQuantity(int amount)
    {
        if (Quantity < amount)
        {
            return false;
        }

        Quantity -= amount;
        return true;
    }

    public bool IsEquipped()
    {
        return !string.IsNullOrEmpty(EquippedToHeroId);
    }

    public void Unequip()
    {
        EquippedToHeroId = string.Empty;
    }

    public DateTime GetAcquiredDateTime()
    {
        return new DateTime(AcquiredTimestampTicks, DateTimeKind.Utc);
    }

    public ItemInstance Clone()
    {
        return new ItemInstance
        {
            InstanceId = Guid.NewGuid().ToString(),
            DefinitionId = DefinitionId,
            Quantity = Quantity,
            MaxStackSize = MaxStackSize,
            AcquiredTimestampTicks = DateTime.UtcNow.Ticks,
            IsNew = true,
            EquippedToHeroId = string.Empty,
            BonusHealth = BonusHealth,
            BonusAttack = BonusAttack,
            BonusDefense = BonusDefense,
            BonusSpeed = BonusSpeed,
            BonusCritRate = BonusCritRate,
            BonusCritDamage = BonusCritDamage
        };
    }
}

public enum ItemType
{
    None,
    Weapon,
    Armor,
    Accessory,
    Consumable,
    HealPotion,
    MoraleBooster,
    Material,
    UpgradeStone,
    EvolutionShard,
    ArtifactShard,
    ExpScroll,
    KeyItem,
    Currency
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public static class ItemRarityExtensions
{
    public static Color GetColor(this ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f);
            case ItemRarity.Uncommon:
                return new Color(0.3f, 0.9f, 0.3f);
            case ItemRarity.Rare:
                return new Color(0.3f, 0.5f, 0.9f);
            case ItemRarity.Epic:
                return new Color(0.7f, 0.3f, 0.9f);
            case ItemRarity.Legendary:
                return new Color(1f, 0.6f, 0.1f);
            default:
                return Color.white;
        }
    }
}
