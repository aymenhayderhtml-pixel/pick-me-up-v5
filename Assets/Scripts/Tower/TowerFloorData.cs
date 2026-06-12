using System;

[Serializable]
public class TowerFloorData
{
    public int FloorNumber;
    public int EnemyLevel;
    public int EnemyCount;
    public bool IsBossFloor;
    public int StaminaCost;
    public int GoldReward;
    public int ExpReward;
    public string[] PossibleEnemyIds;
    public string BossEnemyId;
}
