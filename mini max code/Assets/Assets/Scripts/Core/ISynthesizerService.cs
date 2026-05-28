using System;
using System.Collections.Generic;
using PickMeUp.Game.Core;

namespace PickMeUp.Game.Services
{
    /// <summary>
    /// Result of a synthesis operation.
    /// </summary>
    [Serializable]
    public class SynthesisResult
    {
        public bool Success;
        public string BaseHeroId;
        public string FailureReason;

        // New stats after synthesis
        public int NewAttack;
        public int NewDefense;
        public int NewMaxHealth;
        public int NewLevel;
        public int NewStarRank;

        // Effects
        public int MoralePenaltyApplied;
        public List<string> ConsumedMaterialIds;
    }

    /// <summary>
    /// Recipe for synthesis calculation.
    /// </summary>
    [Serializable]
    public class SynthesisRecipe
    {
        public float SuccessRate;
        public float StatMultiplier;
        public int MoralePenalty;
        public int StarRankIncrease;
        public int RequiredGold;
    }

    /// <summary>
    /// Interface for the synthesizer service.
    /// </summary>
    public interface ISynthesizerService
    {
        /// <summary>
        /// Calculates the synthesis recipe based on base hero and materials.
        /// </summary>
        SynthesisRecipe CalculateRecipe(HeroInstance baseHero, List<HeroInstance> materials);

        /// <summary>
        /// Executes the synthesis operation.
        /// </summary>
        SynthesisResult ExecuteSynthesis(HeroInstance baseHero, List<HeroInstance> materials);

        /// <summary>
        /// Checks if synthesis is possible.
        /// </summary>
        bool CanSynthesize(HeroInstance baseHero, List<HeroInstance> materials);

        /// <summary>
        /// Gets the maximum number of material heroes allowed.
        /// </summary>
        int GetMaxMaterialCount();
    }
}