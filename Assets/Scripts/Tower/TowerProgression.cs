using UnityEngine;

public static class TowerProgression
{
    public static TowerFloorData GetFloorData(int floorNumber)
    {
        bool isBoss = floorNumber % 10 == 0;

        int baseLevel = 1 + (floorNumber - 1) * 2;
        int enemyCount = isBoss ? 1 : Mathf.Min(5, 2 + floorNumber / 5);

        return new TowerFloorData
        {
            FloorNumber = floorNumber,
            EnemyLevel = baseLevel,
            EnemyCount = enemyCount,
            IsBossFloor = isBoss,
            StaminaCost = 5 + floorNumber / 10,
            GoldReward = 50 + floorNumber * 10 + (isBoss ? 200 : 0),
            ExpReward = 25 + floorNumber * 5 + (isBoss ? 100 : 0),
            PossibleEnemyIds = GetEnemyPoolForFloor(floorNumber),
            BossEnemyId = isBoss ? GetBossForFloor(floorNumber) : null
        };
    }

    private static string[] GetEnemyPoolForFloor(int floor)
    {
        if (floor < 10)
            return new[] { "enemy_goblin_grunt", "enemy_goblin_archer" };
        else if (floor < 25)
            return new[] { "enemy_goblin_grunt", "enemy_goblin_archer", "enemy_orc_warrior" };
        else if (floor < 50)
            return new[] { "enemy_orc_warrior", "enemy_skeleton_mage", "enemy_dark_knight" };
        else
            return new[] { "enemy_skeleton_mage", "enemy_dark_knight", "enemy_lich_king" };
    }

    private static string GetBossForFloor(int floor)
    {
        if (floor < 20) return "enemy_goblin_chieftain";
        if (floor < 40) return "enemy_orc_warlord";
        if (floor < 70) return "enemy_lich_king";
        return "enemy_dragon_wyrm";
    }
}
