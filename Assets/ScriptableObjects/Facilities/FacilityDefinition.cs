using System;
using UnityEngine;

public enum FacilityType
{
    Workshop,
    Square,
    Dorms,
    Crucible,
    TrainingHall,
    FlyingDock,
    MemorialHall,
    TacticalStation
}

[CreateAssetMenu(fileName = "NewFacilityDefinition", menuName = "PickMeUp/Facility Definition")]
public class FacilityDefinition : ScriptableObject
{
    public string FacilityId;
    public string DisplayName;
    public string Description;
    public FacilityType FacilityType;
    public string FacilityRole;
    [TextArea(2, 6)]
    public string FacilityEmotion;
    public int MaxLevel = 10;
    public int StartingLevel = 1;
    public int[] GoldCostPerLevel;
    public int[] GemCostPerLevel;
    public float[] BenefitPerLevel;
    public float UpgradeTimeBase = 60f;
    public float UpgradeTimeMultiplier = 1.5f;
    public Sprite FacilityIcon;
    public Color FacilityColor = Color.gray;

    public int GetUpgradeCost(int targetLevel)
    {
        if (GoldCostPerLevel == null || GoldCostPerLevel.Length == 0)
        {
            return 100 * targetLevel;
        }

        int index = Mathf.Clamp(targetLevel - 1, 0, GoldCostPerLevel.Length - 1);
        return GoldCostPerLevel[index];
    }

    public int GetGemCost(int targetLevel)
    {
        if (GemCostPerLevel == null || GemCostPerLevel.Length == 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(targetLevel - 1, 0, GemCostPerLevel.Length - 1);
        return GemCostPerLevel[index];
    }

    public float GetBenefitAtLevel(int level)
    {
        if (BenefitPerLevel == null || BenefitPerLevel.Length == 0)
        {
            return level * 10f;
        }

        int index = Mathf.Clamp(level - 1, 0, BenefitPerLevel.Length - 1);
        return BenefitPerLevel[index];
    }

    public float GetUpgradeTime(int targetLevel)
    {
        return UpgradeTimeBase * Mathf.Pow(UpgradeTimeMultiplier, targetLevel - 1);
    }

    public string GetBenefitDescription(int currentLevel)
    {
        float benefit = GetBenefitAtLevel(currentLevel);

        switch (FacilityType)
        {
            case FacilityType.Workshop:
                return benefit + " crafting output";
            case FacilityType.Square:
                return benefit + " morale influence";
            case FacilityType.Dorms:
                return benefit + " recovery speed";
            case FacilityType.Crucible:
                return benefit + " legacy conversion";
            case FacilityType.TrainingHall:
                return benefit + " EXP/hour";
            case FacilityType.FlyingDock:
                return benefit + " sortie efficiency";
            case FacilityType.MemorialHall:
                return benefit + " Echo resonance";
            case FacilityType.TacticalStation:
                return benefit + " command signals";
            default:
                return "Level " + currentLevel + " bonus";
        }
    }

    public string GetNextLevelBenefitDescription(int currentLevel)
    {
        if (currentLevel >= MaxLevel)
        {
            return "MAX LEVEL";
        }

        float currentBenefit = GetBenefitAtLevel(currentLevel);
        float nextBenefit = GetBenefitAtLevel(currentLevel + 1);
        float improvement = nextBenefit - currentBenefit;

        switch (FacilityType)
        {
            case FacilityType.Workshop:
                return "+" + improvement + " crafting output";
            case FacilityType.Square:
                return "+" + improvement + " morale influence";
            case FacilityType.Dorms:
                return "+" + improvement + " recovery speed";
            case FacilityType.Crucible:
                return "+" + improvement + " legacy conversion";
            case FacilityType.TrainingHall:
                return "+" + improvement + " EXP/hour";
            case FacilityType.FlyingDock:
                return "+" + improvement + " sortie efficiency";
            case FacilityType.MemorialHall:
                return "+" + improvement + " Echo resonance";
            case FacilityType.TacticalStation:
                return "+" + improvement + " command signals";
            default:
                return "+" + improvement;
        }
    }
}

[Serializable]
public class UpgradeCost
{
    public int Gold;
    public int Gems;
    public float TimeSeconds;
    public bool IsFree;

    public static UpgradeCost Free
    {
        get { return new UpgradeCost { IsFree = true }; }
    }
}

[Serializable]
public class FacilityUpgradeProgress
{
    public string FacilityId;
    public int TargetLevel;
    public long StartTimeTicks;
    public long CompletionTimeTicks;
    public bool IsCompleted;

    public DateTime GetStartTime()
    {
        return new DateTime(StartTimeTicks, DateTimeKind.Utc);
    }

    public DateTime GetCompletionTime()
    {
        return new DateTime(CompletionTimeTicks, DateTimeKind.Utc);
    }
}
