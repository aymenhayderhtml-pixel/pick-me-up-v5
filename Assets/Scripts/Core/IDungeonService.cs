using System;
using System.Collections.Generic;

public interface IDungeonService
{
    int GetTacticalSignalLimit();
    int GetQueuedTacticalSignalCount();
    string GetTacticalSummary();
    bool HasTacticalSignal(TacticalSignal signal);
    bool ToggleTacticalSignal(TacticalSignal signal);
    void ClearTacticalSignals();
    List<DungeonDefinition> GetAllDungeons();
    List<DungeonDefinition> GetAvailableDungeonsToday();
    DungeonDefinition GetDungeon(string dungeonId);
    int GetDungeonProgress(string dungeonId);
    int GetStamina();
    int GetMaxStamina();
    bool CanRunDungeon(string dungeonId, int staminaCost);
    DungeonRunResult RunDungeon(string dungeonId, List<string> heroInstanceIds);
    FloorData GenerateFloor(string dungeonId, int floorNumber);
    int CalculateTeamPower(List<string> heroInstanceIds);
    void RechargeStamina(int amount);
    void ProcessStaminaRegeneration();
    void ResetProgress();
}

[Serializable]
public class DungeonRunResult
{
    public bool Success;
    public string DungeonId;
    public int FloorCleared;
    public List<string> EarnedItemIds = new List<string>();
    public int GoldEarned;
    public int ExpEarned;
    public int StonesEarned;
    public List<string> AffectedHeroIds = new List<string>();
    public List<HeroMoraleEffect> MoraleEffects = new List<HeroMoraleEffect>();
    public List<string> TacticalSignals = new List<string>();
    public string TacticalSummary;
    public string FailureReason;
}

[Serializable]
public class HeroMoraleEffect
{
    public string HeroInstanceId;
    public int MoraleChange;
    public string Reason;
}

[Serializable]
public enum TacticalSignal
{
    Scan,
    Focus,
    Rally
}
