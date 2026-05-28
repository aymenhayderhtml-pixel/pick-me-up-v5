using System;
using UnityEngine;

namespace PickMeUp.Game.ScriptableObjects
{
    /// <summary>
    /// Defines a type of facility/building in the game.
    /// </summary>
    public enum FacilityType
    {
        TrainingGround,
        Hospital,
        AlchemyLab,
        GoldMine
    }

    /// <summary>
    /// ScriptableObject definition for a facility building.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFacility", menuName = "PickMeUp/Facility Definition")]
    public class FacilityDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string FacilityId;
        public string DisplayName;
        public string Description;
        public FacilityType FacilityType;

        [Header("Levels")]
        public int MaxLevel = 10;
        public int StartingLevel = 1;

        [Header("Upgrade Costs (per level)")]
        public int[] GoldCostPerLevel;
        public int[] GemCostPerLevel;

        [Header("Benefit Values (per level)")]
        public float[] BenefitPerLevel;

        [Header("Timers (in seconds)")]
        public float UpgradeTimeBase = 60f;
        public float UpgradeTimeMultiplier = 1.5f;

        [Header("Visual")]
        public Sprite FacilityIcon;
        public Color FacilityColor = Color.gray;

        /// <summary>
        /// Gets the upgrade cost for a specific level.
        /// </summary>
        public int GetUpgradeCost(int targetLevel)
        {
            if (GoldCostPerLevel == null || GoldCostPerLevel.Length == 0)
                return 100 * targetLevel;

            int index = Mathf.Clamp(targetLevel - 1, 0, GoldCostPerLevel.Length - 1);
            return GoldCostPerLevel[index];
        }

        /// <summary>
        /// Gets the gem cost for upgrade (for higher levels).
        /// </summary>
        public int GetGemCost(int targetLevel)
        {
            if (GemCostPerLevel == null || GemCostPerLevel.Length == 0)
                return 0;

            int index = Mathf.Clamp(targetLevel - 1, 0, GemCostPerLevel.Length - 1);
            return GemCostPerLevel[index];
        }

        /// <summary>
        /// Gets the benefit value at a specific level.
        /// </summary>
        public float GetBenefitAtLevel(int level)
        {
            if (BenefitPerLevel == null || BenefitPerLevel.Length == 0)
                return level * 10f;

            int index = Mathf.Clamp(level - 1, 0, BenefitPerLevel.Length - 1);
            return BenefitPerLevel[index];
        }

        /// <summary>
        /// Gets the upgrade time for a specific level.
        /// </summary>
        public float GetUpgradeTime(int targetLevel)
        {
            return UpgradeTimeBase * Mathf.Pow(UpgradeTimeMultiplier, targetLevel - 1);
        }

        /// <summary>
        /// Gets a description of the current benefit.
        /// </summary>
        public string GetBenefitDescription(int currentLevel)
        {
            float benefit = GetBenefitAtLevel(currentLevel);

            switch (FacilityType)
            {
                case FacilityType.TrainingGround:
                    return $"{benefit} EXP/hour";

                case FacilityType.Hospital:
                    return $"{benefit}% faster morale recovery";

                case FacilityType.AlchemyLab:
                    return $"{benefit}% better conversion rates";

                case FacilityType.GoldMine:
                    return $"{benefit} Gold/hour";

                default:
                    return $"Level {currentLevel} bonus";
            }
        }

        /// <summary>
        /// Gets a description of the next level benefit.
        /// </summary>
        public string GetNextLevelBenefitDescription(int currentLevel)
        {
            if (currentLevel >= MaxLevel)
                return "MAX LEVEL";

            float currentBenefit = GetBenefitAtLevel(currentLevel);
            float nextBenefit = GetBenefitAtLevel(currentLevel + 1);
            float improvement = nextBenefit - currentBenefit;

            switch (FacilityType)
            {
                case FacilityType.TrainingGround:
                    return $"+{improvement} EXP/hour";

                case FacilityType.Hospital:
                    return $"+{improvement}% recovery speed";

                case FacilityType.AlchemyLab:
                    return $"+{improvement}% efficiency";

                case FacilityType.GoldMine:
                    return $"+{improvement} Gold/hour";

                default:
                    return $"+{improvement}";
            }
        }
    }
}