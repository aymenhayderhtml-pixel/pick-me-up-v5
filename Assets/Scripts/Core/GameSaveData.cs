using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int Gold;
    public int Gems;
    public int AttributeStones;
    public int MemorialFragments;
    public List<HeroInstance> Heroes;
    public PityData Pity;

    public List<ItemInstance> Items = new List<ItemInstance>();
    public List<string> DiscoveredHeroIds = new List<string>();
    public List<string> MemorialEchoHeroIds = new List<string>();
    public Dictionary<string, int> DungeonProgress = new Dictionary<string, int>();
    public Dictionary<string, int> FacilityLevels = new Dictionary<string, int>();
    public List<FacilityUpgradeProgress> FacilityUpgradeQueue = new List<FacilityUpgradeProgress>();
    public int Stamina = 100;
    public long LastStaminaRegenTime;
    public long LastGoldCollectionTime;
    public long LastExpCollectionTime;
    public long LastHospitalProcessTime;
}

[Serializable]
public class PityData
{
    public int StandardSummonCount;
    public int PremiumSummonCount;
    public int RateUpSummonCount;
}
