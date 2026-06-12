using UnityEngine;

public static class MemorialHallReward
{
    public static void GrantDiscoveryReward(string heroDefId, ICurrencyService currency, IInventoryService inventory)
    {
        if (currency == null) return;

        currency.AddGold(50);
        currency.AddDungeonExp(25);

        Debug.Log("[MemorialHall] Discovery reward granted for " + heroDefId);
    }

    public static void GrantEchoReward(string heroDefId, ICurrencyService currency)
    {
        if (currency == null) return;

        currency.AddGold(10);

        Debug.Log("[MemorialHall] Echo reward granted for " + heroDefId);
    }
}
