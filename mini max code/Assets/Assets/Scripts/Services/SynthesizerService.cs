using System;
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Game.Core;

namespace PickMeUp.Game.Services
{
    /// <summary>
    /// Service that handles hero synthesis logic.
    /// </summary>
    public class SynthesizerService : ISynthesizerService
    {
        private readonly ISaveLoadService _saveLoadService;
        private readonly IRosterService _rosterService;
        private readonly ICurrencyService _currencyService;

        private const int MaxMaterialCount = 3;
        private const int BaseSynthesisGoldCost = 1000;
        private const float BaseSuccessRate = 0.8f;
        private const float MaterialBonusRate = 0.05f; // Per matching star rank
        private const float StatBoostPerMaterial = 0.1f; // 10% boost per material
        private const int StarRankIncreaseThreshold = 3; // Need 3+ same-star materials for star increase
        private const int MoralePenalty = 15; // 15% morale penalty for base hero

        public SynthesizerService(
            ISaveLoadService saveLoadService,
            IRosterService rosterService,
            ICurrencyService currencyService)
        {
            _saveLoadService = saveLoadService;
            _rosterService = rosterService;
            _currencyService = currencyService;
        }

        public SynthesisRecipe CalculateRecipe(HeroInstance baseHero, List<HeroInstance> materials)
        {
            var recipe = new SynthesisRecipe
            {
                SuccessRate = BaseSuccessRate,
                StatMultiplier = 1f,
                MoralePenalty = MoralePenalty,
                StarRankIncrease = 0,
                RequiredGold = CalculateGoldCost(materials)
            };

            if (baseHero == null || materials.Count == 0)
            {
                return recipe;
            }

            // Calculate success rate bonus from material quality
            float bonusRate = 0f;
            int matchingStarCount = 0;

            foreach (var material in materials)
            {
                if (material.StarRank == baseHero.StarRank)
                {
                    matchingStarCount++;
                    bonusRate += MaterialBonusRate;
                }
                else if (material.StarRank > baseHero.StarRank)
                {
                    // Higher star materials are even better
                    bonusRate += MaterialBonusRate * 1.5f;
                }
            }

            recipe.SuccessRate = Mathf.Min(0.98f, BaseSuccessRate + bonusRate);

            // Calculate stat multiplier
            recipe.StatMultiplier = 1f + (materials.Count * StatBoostPerMaterial);

            // Add bonus for matching star ranks
            if (matchingStarCount >= 2)
            {
                recipe.StatMultiplier += 0.05f; // Extra 5% for 2+ matching
            }

            // Check for star rank increase
            if (matchingStarCount >= StarRankIncreaseThreshold)
            {
                recipe.StarRankIncrease = 1;
            }

            // Higher level materials give slightly more benefit
            float avgLevel = 0f;
            foreach (var material in materials)
            {
                avgLevel += material.Level;
            }
            avgLevel /= materials.Count;

            if (avgLevel > baseHero.Level)
            {
                recipe.StatMultiplier += 0.02f; // Bonus for higher level materials
            }

            return recipe;
        }

        public bool CanSynthesize(HeroInstance baseHero, List<HeroInstance> materials)
        {
            if (baseHero == null || materials.Count == 0)
                return false;

            if (materials.Count > MaxMaterialCount)
                return false;

            // Can't use same hero as both base and material
            if (materials.Contains(baseHero))
                return false;

            // Need enough gold
            int goldCost = CalculateGoldCost(materials);
            if (_currencyService.GetGold() < goldCost)
                return false;

            // Base hero must not be in active dungeon run (simplified check)
            return true;
        }

        public SynthesisResult ExecuteSynthesis(HeroInstance baseHero, List<HeroInstance> materials)
        {
            var result = new SynthesisResult
            {
                Success = false,
                BaseHeroId = baseHero.InstanceId,
                ConsumedMaterialIds = new List<string>()
            };

            // Validate
            if (!CanSynthesize(baseHero, materials))
            {
                result.FailureReason = "Cannot perform synthesis with current selection";
                return result;
            }

            // Deduct gold
            int goldCost = CalculateGoldCost(materials);
            if (!_currencyService.SpendGold(goldCost))
            {
                result.FailureReason = "Not enough gold";
                return result;
            }

            // Calculate recipe
            var recipe = CalculateRecipe(baseHero, materials);

            // Roll for success
            float roll = UnityEngine.Random.value;
            bool success = roll <= recipe.SuccessRate;

            if (success)
            {
                // Apply stat boost to base hero
                int oldAttack = baseHero.Attack;
                int oldDefense = baseHero.Defense;
                int oldMaxHealth = baseHero.MaxHealth;

                baseHero.Attack = Mathf.CeilToInt(baseHero.Attack * recipe.StatMultiplier);
                baseHero.Defense = Mathf.CeilToInt(baseHero.Defense * recipe.StatMultiplier);
                baseHero.MaxHealth = Mathf.CeilToInt(baseHero.MaxHealth * recipe.StatMultiplier);
                baseHero.CurrentHealth = baseHero.MaxHealth; // Full heal

                // Apply star rank increase
                if (recipe.StarRankIncrease > 0)
                {
                    baseHero.StarRank += recipe.StarRankIncrease;
                }

                // Apply morale penalty
                int moraleLoss = Mathf.CeilToInt(baseHero.CurrentMorale * (recipe.MoralePenalty / 100f));
                baseHero.CurrentMorale = Mathf.Max(0, baseHero.CurrentMorale - moraleLoss);

                // Update hero
                _rosterService.UpdateHero(baseHero);

                result.Success = true;
                result.NewAttack = baseHero.Attack;
                result.NewDefense = baseHero.Defense;
                result.NewMaxHealth = baseHero.MaxHealth;
                result.NewLevel = baseHero.Level;
                result.NewStarRank = baseHero.StarRank;
                result.MoralePenaltyApplied = moraleLoss;
            }
            else
            {
                // Failure: base hero takes extra morale damage
                int moraleLoss = Mathf.CeilToInt(baseHero.CurrentMorale * 0.3f); // 30% on failure
                baseHero.CurrentMorale = Mathf.Max(0, baseHero.CurrentMorale - moraleLoss);
                _rosterService.UpdateHero(baseHero);

                result.FailureReason = $"Synthesis failed! (Roll: {roll:F2}, Required: {recipe.SuccessRate:F2})";
                result.MoralePenaltyApplied = moraleLoss;
            }

            // Consume materials
            foreach (var material in materials)
            {
                result.ConsumedMaterialIds.Add(material.InstanceId);
                _rosterService.RemoveHero(material.InstanceId);
            }

            // Save progress
            _saveLoadService.SaveGame(_saveLoadService.LoadGame());

            return result;
        }

        public int GetMaxMaterialCount()
        {
            return MaxMaterialCount;
        }

        private int CalculateGoldCost(List<HeroInstance> materials)
        {
            int baseCost = BaseSynthesisGoldCost;

            // Each material adds cost
            foreach (var material in materials)
            {
                baseCost += 500 + (material.StarRank * 200);
            }

            return baseCost;
        }
    }
}