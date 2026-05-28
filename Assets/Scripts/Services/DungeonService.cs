using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonService : IDungeonService
{
    private const int MaxStamina = 100;
    private static readonly string[] ResourcePaths = { "Dungeons", "ScriptableObjects/Dungeons" };

    private readonly GameStateService _gameState;
    private readonly ICurrencyService _currency;
    private readonly IRosterService _roster;
    private readonly IFacilityService _facility;
    private List<DungeonDefinition> _definitions;
    private readonly List<TacticalSignal> _queuedSignals = new List<TacticalSignal>();

    public DungeonService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        _facility = ServiceRegistry.Instance.Resolve<IFacilityService>();
        _definitions = LoadDefinitions();
    }

    public int GetTacticalSignalLimit()
    {
        int level = 0;
        if (_facility != null)
        {
            FacilityDefinition station = _facility.GetFacility("tactical_station");
            if (station != null)
            {
                level = _facility.GetFacilityLevel("tactical_station");
            }
        }

        return Mathf.Clamp(1 + (level / 3), 1, 4);
    }

    public int GetQueuedTacticalSignalCount()
    {
        return _queuedSignals.Count;
    }

    public string GetTacticalSummary()
    {
        if (_queuedSignals.Count == 0)
        {
            return "No active commands. The Master is waiting.";
        }

        List<string> labels = new List<string>();
        foreach (TacticalSignal signal in _queuedSignals)
        {
            labels.Add(signal.ToString().ToUpperInvariant());
        }

        return "Commands: " + string.Join(", ", labels);
    }

    public bool HasTacticalSignal(TacticalSignal signal)
    {
        return _queuedSignals.Contains(signal);
    }

    public bool ToggleTacticalSignal(TacticalSignal signal)
    {
        if (_queuedSignals.Contains(signal))
        {
            _queuedSignals.Remove(signal);
            return true;
        }

        if (_queuedSignals.Count >= GetTacticalSignalLimit())
        {
            return false;
        }

        _queuedSignals.Add(signal);
        return true;
    }

    public void ClearTacticalSignals()
    {
        _queuedSignals.Clear();
    }

    public List<DungeonDefinition> GetAllDungeons()
    {
        return new List<DungeonDefinition>(_definitions);
    }

    public List<DungeonDefinition> GetAvailableDungeonsToday()
    {
        DayOfWeek today = DateTime.UtcNow.DayOfWeek;
        return _definitions.Where(def => IsDungeonAvailableOn(def, today)).ToList();
    }

    public DungeonDefinition GetDungeon(string dungeonId)
    {
        return _definitions.Find(def => def != null && def.DungeonId == dungeonId);
    }

    public int GetDungeonProgress(string dungeonId)
    {
        EnsureState();
        int progress;
        if (_gameState.Data.DungeonProgress.TryGetValue(dungeonId, out progress))
        {
            return progress;
        }

        return 0;
    }

    public int GetStamina()
    {
        EnsureState();
        return Mathf.Clamp(_gameState.Data.Stamina, 0, MaxStamina);
    }

    public int GetMaxStamina()
    {
        return MaxStamina;
    }

    public bool CanRunDungeon(string dungeonId, int staminaCost)
    {
        return IsDungeonAvailableOn(GetDungeon(dungeonId), DateTime.UtcNow.DayOfWeek) && GetStamina() >= staminaCost;
    }

    public DungeonRunResult RunDungeon(string dungeonId, List<string> heroInstanceIds)
    {
        DungeonDefinition dungeon = GetDungeon(dungeonId);
        if (dungeon == null)
        {
            return new DungeonRunResult { Success = false, DungeonId = dungeonId, FailureReason = "Dungeon not found." };
        }

        if (!IsDungeonAvailableOn(dungeon, DateTime.UtcNow.DayOfWeek))
        {
            return new DungeonRunResult { Success = false, DungeonId = dungeonId, FailureReason = "This dungeon is not available today." };
        }

        int staminaCost = Mathf.Max(1, dungeon.BaseStaminaCost);
        int tacticalDiscount = _queuedSignals.Contains(TacticalSignal.Rally) ? 2 : 0;
        staminaCost = Mathf.Max(1, staminaCost - tacticalDiscount);
        if (GetStamina() < staminaCost)
        {
            return new DungeonRunResult { Success = false, DungeonId = dungeonId, FailureReason = "Not enough stamina." };
        }

        if (heroInstanceIds == null || heroInstanceIds.Count < dungeon.MinHeroCount)
        {
            return new DungeonRunResult { Success = false, DungeonId = dungeonId, FailureReason = "Not enough heroes selected." };
        }

        _gameState.Data.Stamina = Mathf.Max(0, _gameState.Data.Stamina - staminaCost);

        int nextProgress = GetDungeonProgress(dungeonId) + 1;
        _gameState.Data.DungeonProgress[dungeonId] = nextProgress;

        float rewardMultiplier = 1f + nextProgress * dungeon.RewardScaling;
        if (_queuedSignals.Contains(TacticalSignal.Focus))
        {
            rewardMultiplier += 0.20f;
        }

        if (_queuedSignals.Contains(TacticalSignal.Scan))
        {
            rewardMultiplier += 0.10f;
        }

        if (_queuedSignals.Contains(TacticalSignal.Rally))
        {
            rewardMultiplier += 0.05f;
        }

        int goldEarned = Mathf.RoundToInt(dungeon.BaseGoldReward * rewardMultiplier);
        int expEarned = Mathf.RoundToInt(dungeon.BaseExpReward * rewardMultiplier);
        int stonesEarned = Mathf.RoundToInt(dungeon.BaseStonesReward * rewardMultiplier);

        _currency.AddGold(goldEarned);
        _currency.AddAttributeStones(stonesEarned);

        var result = new DungeonRunResult
        {
            Success = true,
            DungeonId = dungeonId,
            FloorCleared = nextProgress,
            GoldEarned = goldEarned,
            ExpEarned = expEarned,
            StonesEarned = stonesEarned,
            AffectedHeroIds = new List<string>(heroInstanceIds)
        };

        foreach (string heroId in heroInstanceIds)
        {
            int moraleChange = _queuedSignals.Contains(TacticalSignal.Rally) ? -1 : -2;
            result.MoraleEffects.Add(new HeroMoraleEffect
            {
                HeroInstanceId = heroId,
                MoraleChange = moraleChange,
                Reason = _queuedSignals.Contains(TacticalSignal.Rally) ? "Rally support" : "Dungeon fatigue"
            });
        }

        result.TacticalSignals = _queuedSignals.Select(signal => signal.ToString()).ToList();
        result.TacticalSummary = GetTacticalSummary();

        _gameState.Save();
        ClearTacticalSignals();
        return result;
    }

    public FloorData GenerateFloor(string dungeonId, int floorNumber)
    {
        DungeonDefinition dungeon = GetDungeon(dungeonId);
        if (dungeon == null)
        {
            return null;
        }

        int scaledGold = Mathf.RoundToInt(dungeon.BaseGoldReward * (1f + floorNumber * dungeon.RewardScaling));
        int scaledExp = Mathf.RoundToInt(dungeon.BaseExpReward * (1f + floorNumber * dungeon.RewardScaling));
        int scaledStones = Mathf.RoundToInt(dungeon.BaseStonesReward * (1f + floorNumber * dungeon.RewardScaling));

        return new FloorData
        {
            DungeonId = dungeonId,
            FloorNumber = floorNumber,
            RecommendedPower = Mathf.RoundToInt(floorNumber * 100f),
            GoldReward = scaledGold,
            ExpReward = scaledExp,
            StonesReward = scaledStones
        };
    }

    public int CalculateTeamPower(List<string> heroInstanceIds)
    {
        if (heroInstanceIds == null || heroInstanceIds.Count == 0)
        {
            return 0;
        }

        int power = 0;
        var heroes = _roster.GetAll();
        foreach (string id in heroInstanceIds)
        {
            HeroInstance hero = heroes.Find(h => h.InstanceId == id);
            if (hero != null)
            {
                power += hero.CurrentStarRank * 100 + Mathf.RoundToInt(hero.Morale * 0.5f);
            }
        }

        return power;
    }

    public void RechargeStamina(int amount)
    {
        EnsureState();
        _gameState.Data.Stamina = Mathf.Clamp(_gameState.Data.Stamina + Mathf.Max(0, amount), 0, MaxStamina);
        _gameState.Save();
    }

    public void ProcessStaminaRegeneration()
    {
        EnsureState();
        long nowTicks = DateTime.UtcNow.Ticks;
        if (_gameState.Data.LastStaminaRegenTime <= 0)
        {
            _gameState.Data.LastStaminaRegenTime = nowTicks;
            return;
        }

        TimeSpan elapsed = new DateTime(nowTicks, DateTimeKind.Utc) - new DateTime(_gameState.Data.LastStaminaRegenTime, DateTimeKind.Utc);
        if (elapsed.TotalMinutes < 5.0)
        {
            return;
        }

        int regen = Mathf.FloorToInt((float)(elapsed.TotalMinutes / 5.0));
        if (regen <= 0)
        {
            return;
        }

        int before = _gameState.Data.Stamina;
        _gameState.Data.Stamina = Mathf.Clamp(_gameState.Data.Stamina + regen, 0, MaxStamina);
        _gameState.Data.LastStaminaRegenTime = nowTicks;

        if (_gameState.Data.Stamina != before)
        {
            _gameState.Save();
        }
    }

    public void ResetProgress()
    {
        EnsureState();
        _gameState.Data.DungeonProgress.Clear();
        _gameState.Data.Stamina = MaxStamina;
        _gameState.Data.LastStaminaRegenTime = DateTime.UtcNow.Ticks;
        _gameState.Save();
    }

    private List<DungeonDefinition> LoadDefinitions()
    {
        foreach (string path in ResourcePaths)
        {
            DungeonDefinition[] loaded = Resources.LoadAll<DungeonDefinition>(path);
            if (loaded != null && loaded.Length > 0)
            {
                return new List<DungeonDefinition>(loaded);
            }
        }

        return new List<DungeonDefinition>();
    }

    private void EnsureState()
    {
        if (_gameState.Data.DungeonProgress == null)
        {
            _gameState.Data.DungeonProgress = new Dictionary<string, int>();
        }
    }

    private bool IsDungeonAvailableOn(DungeonDefinition dungeon, DayOfWeek day)
    {
        if (dungeon == null || dungeon.AvailableDays == null || dungeon.AvailableDays.Count == 0)
        {
            return true;
        }

        return dungeon.AvailableDays.Contains(day);
    }
}
