using System;

public interface IDungeonRewardService
{
    event Action<DungeonRewardResult> OnRewardGranted;
    event Action<DungeonRewardResult> OnRewardPreview;

    DungeonRewardResult BuildReward(DungeonDataSO dungeon);
    DungeonRewardResult ApplyReward(DungeonDataSO dungeon);
    void ApplyReward(DungeonRewardResult result);
}
