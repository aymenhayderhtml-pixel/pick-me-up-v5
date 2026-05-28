using System;
using System.Collections.Generic;

namespace PickMeUp.Game.Core
{
    /// <summary>
    /// Interface for facility service that handles upgradeable buildings.
    /// </summary>
    public interface IFacilityService
    {
        /// <summary>
        /// Gets all facility definitions.
        /// </summary>
        List<FacilityDefinition> GetAllFacilities();

        /// <summary>
        /// Gets a specific facility definition by ID.
        /// </summary>
        FacilityDefinition GetFacility(string facilityId);

        /// <summary>
        /// Gets the current level of a facility.
        /// </summary>
        int GetFacilityLevel(string facilityId);

        /// <summary>
        /// Gets the upgrade cost for the next level.
        /// </summary>
        UpgradeCost GetUpgradeCost(string facilityId);

        /// <summary>
        /// Checks if a facility can be upgraded.
        /// </summary>
        bool CanUpgrade(string facilityId);

        /// <summary>
        /// Starts upgrading a facility.
        /// </summary>
        bool StartUpgrade(string facilityId);

        /// <summary>
        /// Completes an upgrade immediately (with gems).
        /// </summary>
        bool InstantUpgrade(string facilityId);

        /// <summary>
        /// Gets the benefit value at the facility's current level.
        /// </summary>
        float GetCurrentBenefit(string facilityId);

        /// <summary>
        /// Gets the benefit value at the next level.
        /// </summary>
        float GetNextLevelBenefit(string facilityId);

        /// <summary>
        /// Processes passive generation (called periodically).
        /// </summary>
        void ProcessPassiveGeneration();

        /// <summary>
        /// Gets accumulated gold from gold mine.
        /// </summary>
        int CollectGold();

        /// <summary>
        /// Gets accumulated EXP from training ground.
        /// </summary>
        int CollectExp();

        /// <summary>
        /// Processes morale recovery in hospital.
        /// </summary>
        void ProcessHospitalRecovery();

        /// <summary>
        /// Applies morale recovery to a specific hero.
        /// </summary>
        void RecoverHeroMorale(string heroInstanceId, int amount);

        /// <summary>
        /// Resets all facility progress.
        /// </summary>
        void ResetProgress();
    }

    /// <summary>
    /// Represents the cost of upgrading a facility.
    /// </summary>
    [Serializable]
    public class UpgradeCost
    {
        public int Gold;
        public int Gems;
        public float TimeSeconds;
        public bool IsFree;

        public static UpgradeCost Free => new UpgradeCost { IsFree = true };
    }

    /// <summary>
    /// Progress data for a facility upgrade.
    /// </summary>
    [Serializable]
    public class FacilityUpgradeProgress
    {
        public string FacilityId;
        public int TargetLevel;
        public DateTime StartTime;
        public DateTime CompletionTime;
        public bool IsCompleted;
    }
}