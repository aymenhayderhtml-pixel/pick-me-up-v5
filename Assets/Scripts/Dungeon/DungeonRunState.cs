using System;

[Serializable]
public class DungeonRunState
{
    public string DungeonId;
    public int CurrentWaveIndex;
    public int TotalWaves;
    public int EnemiesDefeatedInCurrentWave;
    public int TotalEnemiesInCurrentWave;
    public bool IsActive;
    public bool IsCleared;
    public bool IsFailed;
    public long StartTimeTicks;

    public DungeonRunState()
    {
        Reset();
    }

    public void Initialize(string dungeonId, int totalWaves)
    {
        DungeonId = dungeonId;
        CurrentWaveIndex = 0;
        TotalWaves = totalWaves;
        EnemiesDefeatedInCurrentWave = 0;
        TotalEnemiesInCurrentWave = 0;
        IsActive = true;
        IsCleared = false;
        IsFailed = false;
        StartTimeTicks = DateTime.UtcNow.Ticks;
    }

    public void AdvanceWave()
    {
        CurrentWaveIndex++;
        EnemiesDefeatedInCurrentWave = 0;
        TotalEnemiesInCurrentWave = 0;
    }

    public void RegisterEnemyKill()
    {
        EnemiesDefeatedInCurrentWave++;
    }

    public void FailRun()
    {
        IsActive = false;
        IsFailed = true;
    }

    public void ClearRun()
    {
        IsActive = false;
        IsCleared = true;
    }

    public void Reset()
    {
        DungeonId = string.Empty;
        CurrentWaveIndex = 0;
        TotalWaves = 0;
        EnemiesDefeatedInCurrentWave = 0;
        TotalEnemiesInCurrentWave = 0;
        IsActive = false;
        IsCleared = false;
        IsFailed = false;
        StartTimeTicks = 0;
    }
}
