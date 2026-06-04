using System;
using UnityEngine;

public class DungeonRewardService : IDungeonRewardService
{
    private readonly ICurrencyService _currency;
    private readonly IInventoryService _inventory;

    public event Action<DungeonRewardResult> OnRewardGranted;
    public event Action<DungeonRewardResult> OnRewardPreview;

    public DungeonRewardService()
    {
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _inventory = ServiceRegistry.Instance.Resolve<IInventoryService>();
    }

    public DungeonRewardResult BuildReward(DungeonDataSO dungeon)
    {
        DungeonRewardResult result = LootRoller.Roll(dungeon);
        OnRewardPreview?.Invoke(result);
        return result;
    }

    public DungeonRewardResult ApplyReward(DungeonDataSO dungeon)
    {
        DungeonRewardResult result = LootRoller.Roll(dungeon);
        ApplyReward(result);
        return result;
    }

    public void ApplyReward(DungeonRewardResult result)
    {
        if (result == null || result.IsEmpty)
        {
            OnRewardGranted?.Invoke(result);
            return;
        }

        if (result.Gold > 0)
        {
            _currency.AddGold(result.Gold);
        }

        if (result.Exp > 0)
        {
            _currency.AddDungeonExp(result.Exp);
        }

        if (result.Items != null)
        {
            for (int i = 0; i < result.Items.Count; i++)
            {
                DungeonRewardItem item = result.Items[i];
                if (item == null || string.IsNullOrEmpty(item.ItemId) || item.Quantity <= 0)
                {
                    continue;
                }

                _inventory.CreateItemInstance(item.ItemId, item.Quantity);
            }
        }

        OnRewardGranted?.Invoke(result);
    }
}
