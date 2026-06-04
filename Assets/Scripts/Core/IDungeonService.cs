using System;
using System.Collections.Generic;

public interface IDungeonService
{
    event Action<DungeonDataSO> OnDungeonStarted;
    event Action<EnemyWaveSO> OnWaveStarted;
    event Action OnWaveCleared;
    event Action OnDungeonCleared;
    event Action OnDungeonFailed;
    event Action<DungeonRewardResult> OnRewardGranted;

    DungeonRunState CurrentRunState { get; }
    bool IsInDungeon { get; }

    List<DungeonDataSO> GetAllDungeons();
    DungeonDataSO GetDungeon(string dungeonId);
    bool CanAttemptDungeon(string dungeonId, int currentStamina, int currentLevel);

    void StartDungeon(DungeonDataSO dungeonData);
    void RestoreRun(DungeonRunState savedState, DungeonDataSO dungeonData);
    void RegisterEnemyKill();
    void FailDungeon();
    void AbandonDungeon();
}
