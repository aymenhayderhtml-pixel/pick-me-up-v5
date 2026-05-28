using System;
using System.Collections.Generic;

public interface ISynthesizerService
{
    SynthesisRecipe CalculateRecipe(HeroInstance baseHero, List<HeroInstance> materials);
    SynthesisResult ExecuteSynthesis(HeroInstance baseHero, List<HeroInstance> materials);
    bool CanSynthesize(HeroInstance baseHero, List<HeroInstance> materials);
    int GetMaxMaterialCount();
}

[Serializable]
public class SynthesisResult
{
    public bool Success;
    public string BaseHeroId;
    public string FailureReason;
    public int NewPromotionRank;
    public int NewStarRank;
    public int MoralePenaltyApplied;
    public List<string> ConsumedMaterialIds = new List<string>();
}

[Serializable]
public class SynthesisRecipe
{
    public float SuccessRate;
    public int MoralePenalty;
    public int StarRankIncrease;
    public int PromotionRankIncrease;
    public int RequiredGold;
}
