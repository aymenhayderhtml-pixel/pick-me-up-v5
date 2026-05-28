using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SynthesizerService : ISynthesizerService
{
    private readonly GameStateService _gameState;
    private readonly IRosterService _roster;
    private readonly ICurrencyService _currency;

    public SynthesizerService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
    }

    public SynthesisRecipe CalculateRecipe(HeroInstance baseHero, List<HeroInstance> materials)
    {
        int materialCount = materials != null ? materials.Count : 0;
        return new SynthesisRecipe
        {
            SuccessRate = Mathf.Clamp(50f + materialCount * 10f + (baseHero != null ? baseHero.PromotionRank * 5f : 0f), 5f, 95f),
            MoralePenalty = Mathf.Clamp(5 + materialCount * 2, 5, 30),
            StarRankIncrease = materialCount > 0 ? 1 : 0,
            PromotionRankIncrease = Mathf.Clamp(materialCount, 0, 3),
            RequiredGold = Mathf.Max(0, materialCount * 250)
        };
    }

    public SynthesisResult ExecuteSynthesis(HeroInstance baseHero, List<HeroInstance> materials)
    {
        if (!CanSynthesize(baseHero, materials))
        {
            return new SynthesisResult
            {
                Success = false,
                BaseHeroId = baseHero != null ? baseHero.InstanceId : string.Empty,
                FailureReason = "Invalid synthesis setup."
            };
        }

        SynthesisRecipe recipe = CalculateRecipe(baseHero, materials);
        if (_currency.GetGold() < recipe.RequiredGold)
        {
            return new SynthesisResult
            {
                Success = false,
                BaseHeroId = baseHero.InstanceId,
                FailureReason = "Not enough gold."
            };
        }

        _currency.SpendGold(recipe.RequiredGold);

        var consumedIds = new List<string>();
        foreach (HeroInstance material in materials)
        {
            if (material == null)
            {
                continue;
            }

            consumedIds.Add(material.InstanceId);
            _roster.Remove(material.InstanceId);
        }

        baseHero.PromotionRank = Mathf.Clamp(baseHero.PromotionRank + recipe.PromotionRankIncrease, 0, 5);
        if (recipe.StarRankIncrease > 0)
        {
            baseHero.CurrentStarRank = Mathf.Clamp(baseHero.CurrentStarRank + recipe.StarRankIncrease, 1, 5);
        }

        foreach (HeroInstance hero in _roster.GetAlive())
        {
            hero.Morale = Mathf.Max(0, hero.Morale - 1);
        }

        _gameState.Save();

        return new SynthesisResult
        {
            Success = true,
            BaseHeroId = baseHero.InstanceId,
            NewPromotionRank = baseHero.PromotionRank,
            NewStarRank = baseHero.CurrentStarRank,
            MoralePenaltyApplied = recipe.MoralePenalty,
            ConsumedMaterialIds = consumedIds
        };
    }

    public bool CanSynthesize(HeroInstance baseHero, List<HeroInstance> materials)
    {
        if (baseHero == null || materials == null)
        {
            return false;
        }

        if (materials.Count == 0 || materials.Count > GetMaxMaterialCount())
        {
            return false;
        }

        return materials.All(material => material != null && material.InstanceId != baseHero.InstanceId);
    }

    public int GetMaxMaterialCount()
    {
        return 3;
    }
}
