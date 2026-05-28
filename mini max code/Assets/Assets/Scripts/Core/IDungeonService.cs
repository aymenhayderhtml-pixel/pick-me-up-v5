using System;
using System.Collections.Generic;

namespace PickMeUp.Game.Core
{
    /// <summary>
    /// Interface for dungeon service that handles dungeon exploration logic.
    /// </summary>
    public interface IDungeonService
    {
        /// <summary>
        /// Gets all available dungeon definitions.
        /// </summary>
        List<DungeonDefinition> GetAllDungeons();

        /// <summary>
        /// Gets a specific dungeon by ID.
        /// </summary>
        DungeonDefinition GetDungeon(string dungeonId);

        /// <summary>
        /// Gets the current progress for a dungeon.
        /// </summary>
        int GetDungeonProgress(string dungeonId);

        /// <summary>
        /// Gets the current stamina value.
        /// </summary>
        int GetStamina();

        /// <summary>
        /// Gets the maximum stamina value.
        /// </summary>
        int GetMaxStamina();

        /// <summary>
        /// Checks if a dungeon run is possible with the given stamina cost.
        /// </summary>
        bool CanRunDungeon(string dungeonId, int staminaCost);

        /// <summary>
        /// Runs a dungeon with the selected team.
        /// Returns the results of the run including rewards.
        /// </summary>
        DungeonRunResult RunDungeon(string dungeonId, List<string> heroInstanceIds);

        /// <summary>
        /// Generates floor data for a given dungeon and floor number.
        /// </summary>
        FloorData GenerateFloor(string dungeonId, int floorNumber);

        /// <summary>
        /// Calculates the power level of a team of heroes.
        /// </summary>
        int CalculateTeamPower(List<string> heroInstanceIds);

        /// <summary>
        /// Recharges stamina (called periodically or with items).
        /// </summary>
        void RechargeStamina(int amount);

        /// <summary>
        /// Checks if stamina should be restored and applies it.
        /// </summary>
        void ProcessStaminaRegeneration();

        /// <summary>
        /// Resets dungeon progress (for new game).
        /// </summary>
        void ResetProgress();
    }

    /// <summary>
    /// Result of a dungeon run containing rewards and status.
    /// </summary>
    [Serializable]
    public class DungeonRunResult
    {
        public bool Success;
        public string DungeonId;
        public int FloorCleared;
        public List<string> EarnedItemIds;
        public int GoldEarned;
        public int ExpEarned;
        public int StonesEarned;
        public List<string> AffectedHeroIds;
        public List<HeroMoraleEffect> MoraleEffects;
        public string FailureReason;
    }

    /// <summary>
    /// Morale effect applied to a hero after dungeon run.
    /// </summary>
    [Serializable]
    public class HeroMoraleEffect
    {
        public string HeroInstanceId;
        public int MoraleChange;
        public string Reason;
    }
}