using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyRoster", menuName = "PickMeUp/Dungeon/Enemy Roster")]
public class EnemyRoster : ScriptableObject
{
    [Header("Regular Enemies")]
    public EnemyDataSO GoblinGrunt;
    public EnemyDataSO GoblinArcher;
    public EnemyDataSO OrcWarrior;
    public EnemyDataSO SkeletonMage;
    public EnemyDataSO DarkKnight;

    [Header("Bosses")]
    public EnemyDataSO GoblinChieftain;
    public EnemyDataSO OrcWarlord;
    public EnemyDataSO LichKing;
    public EnemyDataSO DragonWyrm;

    public EnemyDataSO GetEnemyForDungeon(string dungeonId, int waveIndex, int totalWaves)
    {
        bool isBossWave = waveIndex == totalWaves - 1;

        if (isBossWave)
        {
            return GetBossForDungeon(dungeonId);
        }

        float progress = (float)waveIndex / totalWaves;

        if (progress < 0.3f)
            return GoblinGrunt;
        else if (progress < 0.5f)
            return GoblinArcher;
        else if (progress < 0.7f)
            return OrcWarrior;
        else if (progress < 0.85f)
            return SkeletonMage;
        else
            return DarkKnight;
    }

    private EnemyDataSO GetBossForDungeon(string dungeonId)
    {
        switch (dungeonId)
        {
            case "dungeon_isralta_mine":
                return GoblinChieftain;
            case "dungeon_kendert_forest":
                return OrcWarlord;
            case "dungeon_singmirel_plateau":
                return LichKing;
            case "dungeon_main_tower":
                return DragonWyrm;
            default:
                return GoblinChieftain;
        }
    }
}
