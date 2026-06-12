using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonService : IDungeonService
{
    private readonly GameStateService _gameState;
    private readonly ICurrencyService _currency;
    private readonly IRosterService _roster;
    private readonly IDungeonRewardService _rewardService;
    private List<DungeonDataSO> _dungeons;
    private DungeonDataSO _activeDungeonData;

    public event Action<DungeonDataSO> OnDungeonStarted;
    public event Action<EnemyWaveSO> OnWaveStarted;
    public event Action OnWaveCleared;
    public event Action OnDungeonCleared;
    public event Action OnDungeonFailed;
    public event Action<DungeonRewardResult> OnRewardGranted;

    public DungeonRunState CurrentRunState { get; private set; }
    public bool IsInDungeon => CurrentRunState != null && CurrentRunState.IsActive;

    public DungeonService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        if (ServiceRegistry.Instance.HasService<IDungeonRewardService>())
        {
            _rewardService = ServiceRegistry.Instance.Resolve<IDungeonRewardService>();
        }
        CurrentRunState = new DungeonRunState();
        _dungeons = LoadDungeons();
    }

    public List<DungeonDataSO> GetAllDungeons()
    {
        return new List<DungeonDataSO>(_dungeons);
    }

    public DungeonDataSO GetDungeon(string dungeonId)
    {
        return _dungeons.Find(d => d != null && d.Id == dungeonId);
    }

    public bool CanAttemptDungeon(string dungeonId, int currentStamina, int currentLevel)
    {
        DungeonDataSO dungeon = GetDungeon(dungeonId);
        return dungeon != null && dungeon.CanAttempt(currentStamina, currentLevel);
    }

    public void StartDungeon(DungeonDataSO dungeonData)
    {
        if (dungeonData == null || dungeonData.TotalWaves <= 0)
        {
            Debug.LogError("[DungeonService] Cannot start dungeon: missing data or waves.");
            return;
        }

        // Stamina cost
        int staminaCost = GetStaminaCost(dungeonData);
        if (_gameState.Data.Stamina < staminaCost)
        {
            Debug.LogWarning("[DungeonService] Not enough stamina. Need " + staminaCost);
            return;
        }
        _gameState.Data.Stamina -= staminaCost;

        _activeDungeonData = dungeonData;
        CurrentRunState.Initialize(dungeonData.Id, dungeonData.TotalWaves);
        OnDungeonStarted?.Invoke(dungeonData);
        StartCurrentWave();
    }

    public void RestoreRun(DungeonRunState savedState, DungeonDataSO dungeonData)
    {
        if (savedState == null || dungeonData == null)
        {
            return;
        }

        _activeDungeonData = dungeonData;
        CurrentRunState = savedState;

        if (IsInDungeon)
        {
            StartCurrentWave();
        }
    }

    public void RegisterEnemyKill()
    {
        if (!IsInDungeon)
        {
            return;
        }

        CurrentRunState.RegisterEnemyKill();

        if (CurrentRunState.EnemiesDefeatedInCurrentWave >= CurrentRunState.TotalEnemiesInCurrentWave)
        {
            OnWaveCleared?.Invoke();
            CurrentRunState.AdvanceWave();
            StartCurrentWave();
        }
    }

    public void FailDungeon()
    {
        if (!IsInDungeon)
        {
            return;
        }

        CurrentRunState.FailRun();
        OnDungeonFailed?.Invoke();
    }

    public void AbandonDungeon()
    {
        if (!IsInDungeon)
        {
            return;
        }

        CurrentRunState.Reset();
        _activeDungeonData = null;
    }

    private void StartCurrentWave()
    {
        if (_activeDungeonData == null)
        {
            return;
        }

        if (CurrentRunState.CurrentWaveIndex >= _activeDungeonData.TotalWaves)
        {
            ClearDungeon();
            return;
        }

        EnemyWaveSO waveData = _activeDungeonData.Waves[CurrentRunState.CurrentWaveIndex];
        CurrentRunState.TotalEnemiesInCurrentWave = waveData != null ? waveData.TotalEnemyCount : 0;
        CurrentRunState.EnemiesDefeatedInCurrentWave = 0;
        OnWaveStarted?.Invoke(waveData);
    }

    private void ClearDungeon()
    {
        CurrentRunState.ClearRun();
        OnDungeonCleared?.Invoke();

        if (_rewardService != null && _activeDungeonData != null)
        {
            DungeonRewardResult result = _rewardService.BuildReward(_activeDungeonData);
            _rewardService.ApplyReward(result);
            OnRewardGranted?.Invoke(result);
        }
    }

    private int GetStaminaCost(DungeonDataSO dungeonData)
    {
        if (dungeonData == null) return 10;
        int cost = dungeonData.StaminaCost > 0 ? dungeonData.StaminaCost : 10;
        IFacilityService facilityService = ServiceRegistry.Instance?.Resolve<IFacilityService>();
        if (facilityService != null)
        {
            int dormsLevel = facilityService.GetFacilityLevel("dorms");
            cost = Mathf.Max(1, cost - dormsLevel);
        }
        return cost;
    }

    private List<DungeonDataSO> LoadDungeons()
    {
        DungeonDataSO[] loaded = Resources.LoadAll<DungeonDataSO>("Dungeons");
        if (loaded != null && loaded.Length > 0)
        {
            return new List<DungeonDataSO>(loaded);
        }

        return new List<DungeonDataSO>();
    }
}
