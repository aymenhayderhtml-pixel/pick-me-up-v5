using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FacilityService : IFacilityService
{
    private static readonly string[] ResourcePaths = { "Facilities", "ScriptableObjects/Facilities" };

    private readonly GameStateService _gameState;
    private readonly ICurrencyService _currency;
    private readonly IRosterService _roster;
    private List<FacilityDefinition> _definitions;
    private DockSortieType? _queuedSortieType;

    public FacilityService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        _definitions = LoadDefinitions();
        if (_definitions == null || _definitions.Count == 0)
        {
            _definitions = CreateFallbackDefinitions();
        }
    }

    public List<FacilityDefinition> GetAllFacilities()
    {
        return new List<FacilityDefinition>(_definitions);
    }

    public FacilityDefinition GetFacility(string facilityId)
    {
        return _definitions.Find(def => def != null && def.FacilityId == facilityId);
    }

    public int GetFacilityLevel(string facilityId)
    {
        EnsureState();
        FacilityDefinition def = GetFacility(facilityId);
        int defaultLevel = def != null ? Mathf.Max(1, def.StartingLevel) : 0;
        int level;
        if (_gameState.Data.FacilityLevels.TryGetValue(facilityId, out level))
        {
            return level;
        }

        return defaultLevel;
    }

    public UpgradeCost GetUpgradeCost(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return new UpgradeCost { Gold = 0, Gems = 0, TimeSeconds = 0f };
        }

        int nextLevel = GetFacilityLevel(facilityId) + 1;
        return new UpgradeCost
        {
            Gold = def.GetUpgradeCost(nextLevel),
            Gems = def.GetGemCost(nextLevel),
            TimeSeconds = def.GetUpgradeTime(nextLevel),
            IsFree = false
        };
    }

    public bool CanUpgrade(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return false;
        }

        int currentLevel = GetFacilityLevel(facilityId);
        if (currentLevel >= def.MaxLevel)
        {
            return false;
        }

        UpgradeCost cost = GetUpgradeCost(facilityId);
        return _currency.GetGold() >= cost.Gold && _currency.GetGems() >= cost.Gems;
    }

    public bool StartUpgrade(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null || !CanUpgrade(facilityId))
        {
            return false;
        }

        UpgradeCost cost = GetUpgradeCost(facilityId);
        if (!_currency.SpendGold(cost.Gold))
        {
            return false;
        }

        if (cost.Gems > 0 && !_currency.SpendGems(cost.Gems))
        {
            _currency.AddGold(cost.Gold);
            return false;
        }

        int nextLevel = GetFacilityLevel(facilityId) + 1;
        _gameState.Data.FacilityLevels[facilityId] = nextLevel;
        _gameState.Save();
        return true;
    }

    public bool InstantUpgrade(string facilityId)
    {
        return StartUpgrade(facilityId);
    }

    public float GetCurrentBenefit(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return 0f;
        }

        return def.GetBenefitAtLevel(GetFacilityLevel(facilityId));
    }

    public float GetNextLevelBenefit(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return 0f;
        }

        return def.GetBenefitAtLevel(GetFacilityLevel(facilityId) + 1);
    }

    public string GetFacilityRole(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return "UNKNOWN";
        }

        if (!string.IsNullOrEmpty(def.FacilityRole))
        {
            return def.FacilityRole;
        }

        return GetDefaultRole(def.FacilityType);
    }

    public string GetFacilityEmotion(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return "A quiet room with no story yet.";
        }

        if (!string.IsNullOrEmpty(def.FacilityEmotion))
        {
            return def.FacilityEmotion;
        }

        return GetDefaultEmotion(def.FacilityType);
    }

    public bool IsShadowFacility(string facilityId)
    {
        FacilityDefinition def = GetFacility(facilityId);
        if (def == null)
        {
            return false;
        }

        return def.FacilityType == FacilityType.Crucible || def.FacilityType == FacilityType.MemorialHall || def.FacilityType == FacilityType.TacticalStation;
    }

    public DockSortieType GetQueuedSortieType()
    {
        return _queuedSortieType ?? DockSortieType.Recon;
    }

    public bool ToggleSortieType(DockSortieType sortieType)
    {
        if (_queuedSortieType.HasValue && _queuedSortieType.Value == sortieType)
        {
            _queuedSortieType = null;
            return true;
        }

        _queuedSortieType = sortieType;
        return true;
    }

    public bool LaunchSortie(out DockSortieResult result)
    {
        result = new DockSortieResult
        {
            SortieType = GetQueuedSortieType(),
            Success = false,
            Summary = "No sortie plan has been committed."
        };

        FacilityDefinition dock = GetFacility("flying_dock");
        int dockLevel = dock != null ? GetFacilityLevel(dock.FacilityId) : 1;
        int moraleBonus = Mathf.Clamp(dockLevel / 2, 1, 5);

        switch (GetQueuedSortieType())
        {
            case DockSortieType.Recon:
                if (!_currency.SpendGold(500))
                {
                    result.Summary = "Not enough Gold for reconnaissance.";
                    return false;
                }

                result.Success = true;
                result.GoldSpent = 500;
                result.MoraleChange = moraleBonus;
                result.Summary = "Recon sortie complete. Dungeon scouting intelligence improves future planning.";
                BoostMorale(moraleBonus);
                _gameState.Data.LastGoldCollectionTime = DateTime.UtcNow.Ticks;
                break;
            case DockSortieType.Supply:
                if (!_currency.SpendGems(1))
                {
                    result.Summary = "Not enough Gems for supply sortie.";
                    return false;
                }

                result.Success = true;
                result.GemsSpent = 1;
                result.StaminaRestored = 10 + dockLevel;
                result.MoraleChange = 2;
                result.Summary = "Supply sortie complete. Stamina restored and the roster steadied.";
                _gameState.Data.Stamina = Mathf.Clamp(_gameState.Data.Stamina + result.StaminaRestored, 0, 100);
                BoostMorale(2);
                break;
            case DockSortieType.Extraction:
                if (_currency.GetGold() < 1000 || _currency.GetAttributeStones() < 5)
                {
                    result.Summary = "Extraction sortie requires 1,000 Gold and 5 Stones.";
                    return false;
                }

                _currency.SpendGold(1000);
                _currency.SpendAttributeStones(5);
                result.Success = true;
                result.GoldSpent = 1000;
                result.AttributeStonesSpent = 5;
                result.MemorialFragmentsGained = 3 + dockLevel;
                result.MoraleChange = -1;
                result.Summary = "Extraction sortie complete. Memorial Fragments recovered from the field.";
                _currency.AddMemorialFragments(result.MemorialFragmentsGained);
                BoostMorale(-1);
                break;
        }

        _queuedSortieType = null;
        _gameState.Save();
        return true;
    }

    private void BoostMorale(int amount)
    {
        foreach (HeroInstance hero in _roster.GetAlive())
        {
            if (hero != null)
            {
                hero.Morale = Mathf.Clamp(hero.Morale + amount, 0, 100);
            }
        }
    }

    public void ProcessPassiveGeneration()
    {
        EnsureState();
        int goldIncome = 0;
        int expIncome = 0;
        int fragmentIncome = 0;
        int moraleRecovery = 0;

        foreach (FacilityDefinition facility in _definitions)
        {
            if (facility == null)
            {
                continue;
            }

            int level = GetFacilityLevel(facility.FacilityId);
            if (level <= 0)
            {
                continue;
            }

            switch (facility.FacilityType)
            {
                case FacilityType.Workshop:
                    goldIncome += Mathf.RoundToInt(facility.GetBenefitAtLevel(level) * 0.25f);
                    break;
                case FacilityType.Square:
                    moraleRecovery += Mathf.RoundToInt(facility.GetBenefitAtLevel(level));
                    break;
                case FacilityType.Dorms:
                    moraleRecovery += Mathf.RoundToInt(facility.GetBenefitAtLevel(level) * 0.5f);
                    break;
                case FacilityType.Crucible:
                    fragmentIncome += Mathf.RoundToInt(facility.GetBenefitAtLevel(level));
                    break;
                case FacilityType.TrainingHall:
                    goldIncome += Mathf.RoundToInt(facility.GetBenefitAtLevel(level));
                    break;
                case FacilityType.FlyingDock:
                    expIncome += Mathf.RoundToInt(facility.GetBenefitAtLevel(level));
                    break;
                case FacilityType.MemorialHall:
                    fragmentIncome += Mathf.RoundToInt(facility.GetBenefitAtLevel(level) * 0.5f);
                    break;
                case FacilityType.TacticalStation:
                    expIncome += Mathf.RoundToInt(facility.GetBenefitAtLevel(level));
                    break;
            }
        }

        if (goldIncome > 0)
        {
            _currency.AddGold(goldIncome);
            _gameState.Data.LastGoldCollectionTime = DateTime.UtcNow.Ticks;
        }

        if (expIncome > 0)
        {
            _gameState.Data.LastExpCollectionTime = DateTime.UtcNow.Ticks;
        }

        if (fragmentIncome > 0)
        {
            _currency.AddMemorialFragments(fragmentIncome);
        }

        if (moraleRecovery > 0)
        {
            ProcessHospitalRecovery();
        }

        _gameState.Save();
    }

    public int CollectGold()
    {
        int amount = 0;
        foreach (FacilityDefinition facility in _definitions)
        {
            if (facility != null && (facility.FacilityType == FacilityType.Workshop || facility.FacilityType == FacilityType.FlyingDock))
            {
                amount += Mathf.RoundToInt(facility.GetBenefitAtLevel(GetFacilityLevel(facility.FacilityId)));
            }
        }

        if (amount > 0)
        {
            _currency.AddGold(amount);
        }

        return amount;
    }

    public int CollectExp()
    {
        int amount = 0;
        foreach (FacilityDefinition facility in _definitions)
        {
            if (facility != null && (facility.FacilityType == FacilityType.TrainingHall || facility.FacilityType == FacilityType.TacticalStation))
            {
                amount += Mathf.RoundToInt(facility.GetBenefitAtLevel(GetFacilityLevel(facility.FacilityId)));
            }
        }

        _gameState.Data.LastExpCollectionTime = DateTime.UtcNow.Ticks;
        _gameState.Save();
        return amount;
    }

    public int CollectMemorialFragments()
    {
        int amount = 0;
        foreach (FacilityDefinition facility in _definitions)
        {
            if (facility == null)
            {
                continue;
            }

            if (facility.FacilityType == FacilityType.Crucible || facility.FacilityType == FacilityType.MemorialHall)
            {
                amount += Mathf.RoundToInt(facility.GetBenefitAtLevel(GetFacilityLevel(facility.FacilityId)));
            }
        }

        if (amount > 0)
        {
            _currency.AddMemorialFragments(amount);
        }

        return amount;
    }

    public void ProcessHospitalRecovery()
    {
        foreach (HeroInstance hero in _roster.GetAlive())
        {
            if (hero != null)
            {
                hero.Morale = Mathf.Min(100, hero.Morale + 1);
            }
        }

        _gameState.Data.LastHospitalProcessTime = DateTime.UtcNow.Ticks;
        _gameState.Save();
    }

    public void RecoverHeroMorale(string heroInstanceId, int amount)
    {
        HeroInstance hero = _roster.GetAll().Find(h => h.InstanceId == heroInstanceId);
        if (hero == null)
        {
            return;
        }

        hero.Morale = Mathf.Clamp(hero.Morale + amount, 0, 100);
        _gameState.Save();
    }

    public void ResetProgress()
    {
        EnsureState();
        _gameState.Data.FacilityLevels.Clear();
        _gameState.Data.FacilityUpgradeQueue.Clear();
        _gameState.Data.LastGoldCollectionTime = 0;
        _gameState.Data.LastExpCollectionTime = 0;
        _gameState.Data.LastHospitalProcessTime = 0;
        _gameState.Save();
    }

    private List<FacilityDefinition> LoadDefinitions()
    {
        foreach (string path in ResourcePaths)
        {
            FacilityDefinition[] loaded = Resources.LoadAll<FacilityDefinition>(path);
            if (loaded != null && loaded.Length > 0)
            {
                return new List<FacilityDefinition>(loaded);
            }
        }

        return new List<FacilityDefinition>();
    }

    private List<FacilityDefinition> CreateFallbackDefinitions()
    {
        List<FacilityDefinition> defs = new List<FacilityDefinition>();

        defs.Add(CreateFacility("workshop", "Workshop", FacilityType.Workshop, "CRAFTING", "The room where scrap becomes purpose.", new Color(0.75f, 0.55f, 0.2f)));
        defs.Add(CreateFacility("square", "Square", FacilityType.Square, "MORALE", "A gathering place where the Master’s voice can steady the roster.", new Color(0.55f, 0.45f, 0.25f)));
        defs.Add(CreateFacility("dorms", "Dorms", FacilityType.Dorms, "RECOVERY", "Quiet rooms where the wounded recover their will.", new Color(0.35f, 0.5f, 0.65f)));
        defs.Add(CreateFacility("crucible", "Crucible", FacilityType.Crucible, "SACRIFICE", "A dark chamber where choice becomes irreversible power.", new Color(0.65f, 0.2f, 0.25f)));
        defs.Add(CreateFacility("training_hall", "Training Hall", FacilityType.TrainingHall, "GROWTH", "A hall of drills, trials, and hard-earned preparation.", new Color(0.25f, 0.6f, 0.75f)));
        defs.Add(CreateFacility("flying_dock", "Flying Dock", FacilityType.FlyingDock, "EXPEDITION", "The launch point for sortie planning and deployment.", new Color(0.35f, 0.55f, 0.75f)));
        defs.Add(CreateFacility("memorial_hall", "Memorial Hall", FacilityType.MemorialHall, "LEGACY", "The dead remain present here as Echoes.", new Color(0.45f, 0.35f, 0.55f)));
        defs.Add(CreateFacility("tactical_station", "Tactical Station", FacilityType.TacticalStation, "COMMAND", "The Master reaches into battle from this room.", new Color(0.8f, 0.3f, 0.3f)));

        return defs;
    }

    private FacilityDefinition CreateFacility(string id, string displayName, FacilityType type, string role, string emotion, Color color)
    {
        FacilityDefinition def = ScriptableObject.CreateInstance<FacilityDefinition>();
        def.FacilityId = id;
        def.DisplayName = displayName;
        def.Description = emotion;
        def.FacilityType = type;
        def.FacilityRole = role;
        def.FacilityEmotion = emotion;
        def.MaxLevel = 10;
        def.StartingLevel = 1;
        def.FacilityColor = color;
        def.GoldCostPerLevel = BuildGoldCurve(type);
        def.GemCostPerLevel = BuildGemCurve(type);
        def.BenefitPerLevel = BuildBenefitCurve(type);
        def.UpgradeTimeBase = 60f;
        def.UpgradeTimeMultiplier = 1.25f;
        return def;
    }

    private int[] BuildGoldCurve(FacilityType type)
    {
        int baseCost = type == FacilityType.Crucible || type == FacilityType.MemorialHall ? 600 : 250;
        int[] values = new int[10];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = baseCost + (i * baseCost / 2);
        }

        return values;
    }

    private int[] BuildGemCurve(FacilityType type)
    {
        int[] values = new int[10];
        int premiumStart = (type == FacilityType.FlyingDock || type == FacilityType.TacticalStation) ? 1 : 0;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = i < 3 ? premiumStart : 0;
        }

        return values;
    }

    private float[] BuildBenefitCurve(FacilityType type)
    {
        float[] values = new float[10];
        float start = type switch
        {
            FacilityType.Workshop => 10f,
            FacilityType.Square => 5f,
            FacilityType.Dorms => 8f,
            FacilityType.Crucible => 6f,
            FacilityType.TrainingHall => 12f,
            FacilityType.FlyingDock => 7f,
            FacilityType.MemorialHall => 4f,
            FacilityType.TacticalStation => 3f,
            _ => 5f
        };

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = start + (i * start * 0.5f);
        }

        return values;
    }

    private void EnsureState()
    {
        if (_gameState.Data.FacilityLevels == null)
        {
            _gameState.Data.FacilityLevels = new Dictionary<string, int>();
        }

        if (_gameState.Data.FacilityUpgradeQueue == null)
        {
            _gameState.Data.FacilityUpgradeQueue = new List<FacilityUpgradeProgress>();
        }
    }

    private string GetDefaultRole(FacilityType type)
    {
        switch (type)
        {
            case FacilityType.Workshop:
                return "CRAFTING";
            case FacilityType.Square:
                return "MORALE";
            case FacilityType.Dorms:
                return "RECOVERY";
            case FacilityType.Crucible:
                return "SACRIFICE";
            case FacilityType.TrainingHall:
                return "GROWTH";
            case FacilityType.FlyingDock:
                return "EXPEDITION";
            case FacilityType.MemorialHall:
                return "LEGACY";
            case FacilityType.TacticalStation:
                return "COMMAND";
            default:
                return "GENERAL";
        }
    }

    private string GetDefaultEmotion(FacilityType type)
    {
        switch (type)
        {
            case FacilityType.Workshop:
                return "The smell of metal and ash clings to every upgrade.";
            case FacilityType.Square:
                return "This is where the Master speaks and the roster listens.";
            case FacilityType.Dorms:
                return "Quiet rooms where wounded heroes recover their spirit.";
            case FacilityType.Crucible:
                return "A place where sacrifice is made visible and irreversible.";
            case FacilityType.TrainingHall:
                return "Preparation is its own form of survival.";
            case FacilityType.FlyingDock:
                return "Every sortie begins here and every mistake echoes here.";
            case FacilityType.MemorialHall:
                return "The dead remain present as legacy, not absence.";
            case FacilityType.TacticalStation:
                return "The Master's hand reaches into the battle from here.";
            default:
                return "A room waiting to gain meaning.";
        }
    }
}
