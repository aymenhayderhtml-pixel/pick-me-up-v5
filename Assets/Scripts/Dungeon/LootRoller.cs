using System.Collections.Generic;
using UnityEngine;

public static class LootRoller
{
    public static DungeonRewardResult Roll(DungeonDataSO dungeon, System.Random rng = null)
    {
        DungeonRewardResult result = new DungeonRewardResult();
        if (dungeon == null)
        {
            return result;
        }

        result.Gold = Mathf.Max(0, dungeon.BaseGoldReward);
        result.Exp = Mathf.Max(0, dungeon.BaseExpReward);

        DungeonLootTableSO table = dungeon.TypedLootTable;
        if (table == null || table.Entries == null || table.Entries.Count == 0)
        {
            return result;
        }

        if (rng == null)
        {
            rng = new System.Random();
        }

        // First pass: collect guaranteed items so a clear always shows something.
        for (int i = 0; i < table.Entries.Count; i++)
        {
            DungeonLootTableSO.LootEntry entry = table.Entries[i];
            if (entry == null || !entry.IsValid || !entry.Guaranteed)
            {
                continue;
            }

            AddRolledItem(result, entry, rng);
        }

        // Second pass: roll N times against the weighted table.
        int totalWeight = 0;
        for (int i = 0; i < table.Entries.Count; i++)
        {
            DungeonLootTableSO.LootEntry entry = table.Entries[i];
            if (entry != null && entry.IsValid)
            {
                totalWeight += entry.Weight;
            }
        }

        if (totalWeight <= 0)
        {
            return result;
        }

        for (int roll = 0; roll < table.Rolls; roll++)
        {
            float dropRoll = (float)rng.NextDouble();
            if (dropRoll > (1f + table.BonusDropChance))
            {
                continue;
            }

            int pick = rng.Next(0, totalWeight);
            DungeonLootTableSO.LootEntry chosen = null;
            for (int i = 0; i < table.Entries.Count; i++)
            {
                DungeonLootTableSO.LootEntry entry = table.Entries[i];
                if (entry == null || !entry.IsValid)
                {
                    continue;
                }

                pick -= entry.Weight;
                if (pick < 0)
                {
                    chosen = entry;
                    break;
                }
            }

            if (chosen == null)
            {
                continue;
            }

            if (!chosen.Guaranteed)
            {
                float chanceRoll = (float)rng.NextDouble();
                if (chanceRoll > Mathf.Clamp01(chosen.DropChance + table.BonusDropChance))
                {
                    continue;
                }
            }

            AddRolledItem(result, chosen, rng);
        }

        return result;
    }

    private static void AddRolledItem(DungeonRewardResult result, DungeonLootTableSO.LootEntry entry, System.Random rng)
    {
        int minQ = Mathf.Max(1, entry.MinQuantity);
        int maxQ = Mathf.Max(minQ, entry.MaxQuantity);
        int qty = minQ == maxQ ? minQ : rng.Next(minQ, maxQ + 1);

        DungeonRewardItem existing = result.Items.Find(i => i.ItemId == entry.ItemId);
        if (existing != null)
        {
            existing.Quantity += qty;
        }
        else
        {
            result.Items.Add(new DungeonRewardItem
            {
                ItemId = entry.ItemId,
                DisplayName = string.IsNullOrEmpty(entry.FallbackName) ? entry.ItemId : entry.FallbackName,
                Quantity = qty
            });
        }
    }

    public static DungeonRewardResult RollBoss(DungeonDataSO dungeon, int floorLevel = 1)
    {
        DungeonRewardResult result = Roll(dungeon);

        if (dungeon == null || dungeon.TypedLootTable == null) return result;

        DungeonLootTableSO table = dungeon.TypedLootTable;

        // Boss bonus
        result.Gold += table.BossGoldBonus;
        result.Exp += table.BossExpBonus;

        // Boss loot with rarity scaling
        System.Random rng = new System.Random();
        foreach (DungeonLootTableSO.LootEntry entry in table.BossLoot)
        {
            if (entry == null) continue;
            if (!((float)rng.NextDouble() <= entry.DropChance)) continue;

            float rarityRoll = (float)rng.NextDouble();
            int qty = entry.MinQuantity + rng.Next(0, entry.MaxQuantity - entry.MinQuantity + 1);

            result.Items.Add(new DungeonRewardItem
            {
                ItemId = entry.ItemId,
                DisplayName = string.IsNullOrEmpty(entry.FallbackName) ? entry.ItemId : entry.FallbackName,
                Quantity = qty
            });
        }

        return result;
    }

}
