using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles weighted RNG pulls for Standard and Premium banners.
/// 
/// STANDARD BANNER (Gold)  — Moebius Standard Invocation
///   Cost: 1,000 Gold single / 9,000 Gold x10
///   Rates: 1* 68.9% | 2* 22% | 3* 8% | 4* 0.8% | 5* 0.1%
///   Pity: 270 pulls → guaranteed 4*+; 300 pulls → guaranteed 5*
///
/// PREMIUM BANNER (Gems) — Moebius Advanced Invocation
///   Cost: 300 Gems single / 2,700 Gems x10
///   Rates: 1* 50% | 2* 35% | 3* 13.2% | 4* 0.8% | 5* 0.1%  
///   Pity: 150 pulls → guaranteed 4*+; 200 pulls → guaranteed 5*
/// </summary>
public class GachaService : IGachaService
{
    // ── Costs ──────────────────────────────────────────────────────────────
    private const int StandardSingleCost  = 1000;
    private const int StandardTenCost     = 9000;
    private const int PremiumSingleCost   = 300;
    private const int PremiumTenCost      = 2700;

    // ── Pity thresholds ────────────────────────────────────────────────────
    private const int StandardSoftPity   = 270;  // guaranteed 4*+
    private const int StandardHardPity   = 300;  // guaranteed 5*
    private const int PremiumSoftPity    = 150;  // guaranteed 4*+
    private const int PremiumHardPity    = 200;  // guaranteed 5*

    // ── Bond event (10x invocation) ────────────────────────────────────────
    private const float BondEventChance  = 0.15f;

    // ── Dependencies ───────────────────────────────────────────────────────
    private readonly GameStateService  _gameState;
    private readonly ICurrencyService  _currency;
    private readonly HeroRoster        _roster;

    public GachaService()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _currency  = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _roster    = new HeroRoster();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public HeroInstance SummonStandard()
    {
        if (!_currency.SpendGold(StandardSingleCost)) return null;
        _gameState.Data.Pity.StandardSummonCount++;
        HeroInstance result = RollStandard();
        AddHeroToRoster(result);
        _gameState.Save();
        return result;
    }

    public List<HeroInstance> SummonStandardTen()
    {
        if (!_currency.SpendGold(StandardTenCost)) return null;
        List<HeroInstance> results = new List<HeroInstance>(10);
        for (int i = 0; i < 10; i++)
        {
            _gameState.Data.Pity.StandardSummonCount++;
            results.Add(RollStandard());
        }
        foreach (HeroInstance h in results) AddHeroToRoster(h);
        _gameState.Save();
        return results;
    }

    public HeroInstance SummonPremium()
    {
        if (!_currency.SpendGems(PremiumSingleCost)) return null;
        _gameState.Data.Pity.PremiumSummonCount++;
        HeroInstance result = RollPremium();
        AddHeroToRoster(result);
        _gameState.Save();
        return result;
    }

    public List<HeroInstance> SummonPremiumTen()
    {
        if (!_currency.SpendGems(PremiumTenCost)) return null;
        List<HeroInstance> results = new List<HeroInstance>(10);
        for (int i = 0; i < 10; i++)
        {
            _gameState.Data.Pity.PremiumSummonCount++;
            results.Add(RollPremium());
        }
        foreach (HeroInstance h in results) AddHeroToRoster(h);
        _gameState.Save();
        return results;
    }

    public PityData GetPity() => _gameState.Data.Pity;

    // ── Roll logic ─────────────────────────────────────────────────────────

    private HeroInstance RollStandard()
    {
        int pity = _gameState.Data.Pity.StandardSummonCount;
        int starRank;

        if (pity >= StandardHardPity)
        {
            starRank = 5;
            _gameState.Data.Pity.StandardSummonCount = 0;
        }
        else if (pity >= StandardSoftPity)
        {
            starRank = Random.value < 0.10f ? 5 : 4;
            if (starRank >= 4) _gameState.Data.Pity.StandardSummonCount = 0;
        }
        else
        {
            starRank = WeightedRoll(new float[] { 0.689f, 0.22f, 0.08f, 0.008f, 0.001f });
        }

        return CreateHeroInstance(starRank);
    }

    private HeroInstance RollPremium()
    {
        int pity = _gameState.Data.Pity.PremiumSummonCount;
        int starRank;

        if (pity >= PremiumHardPity)
        {
            starRank = 5;
            _gameState.Data.Pity.PremiumSummonCount = 0;
        }
        else if (pity >= PremiumSoftPity)
        {
            starRank = Random.value < 0.10f ? 5 : 4;
            if (starRank >= 4) _gameState.Data.Pity.PremiumSummonCount = 0;
        }
        else
        {
            starRank = WeightedRoll(new float[] { 0.50f, 0.35f, 0.132f, 0.008f, 0.001f });
        }

        return CreateHeroInstance(starRank);
    }

    public bool RollBondEvent() => Random.value < BondEventChance;

    private int WeightedRoll(float[] weights)
    {
        float roll = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative) return i + 1;
        }
        return 1;
    }

    /// <summary>
    /// Creates a HeroInstance for the given star rank.
    /// Falls back through: matching rank → fallback placeholder → any hero.
    /// </summary>
    private HeroInstance CreateHeroInstance(int starRank)
    {
        HeroDefinition def = _roster.GetRandomByStarRank(starRank);

        if (def == null)
        {
            // Try fallback/placeholder for this rank
            def = _roster.GetFallbackByStarRank(starRank);
        }

        if (def == null)
        {
            Debug.LogWarning($"[GachaService] No HeroDefinition found for {starRank}★. Falling back to any hero.");
            def = _roster.GetAny();
        }

        if (def == null)
        {
            Debug.LogError("[GachaService] HeroRoster is empty. Cannot create HeroInstance.");
            return null;
        }

        return new HeroInstance(def.HeroId, def.BaseStarRank);
    }

    private void AddHeroToRoster(HeroInstance hero)
    {
        if (hero == null) return;
        _gameState.Data.Heroes.Add(hero);
        if (_gameState.Data.DiscoveredHeroIds == null)
            _gameState.Data.DiscoveredHeroIds = new List<string>();

        if (!string.IsNullOrEmpty(hero.HeroDefId) && !_gameState.Data.DiscoveredHeroIds.Contains(hero.HeroDefId))
        {
            _gameState.Data.DiscoveredHeroIds.Add(hero.HeroDefId);

            // Grant discovery reward
            ICurrencyService currency = ServiceRegistry.Instance?.Resolve<ICurrencyService>();
            IInventoryService inventory = ServiceRegistry.Instance?.Resolve<IInventoryService>();
            MemorialHallReward.GrantDiscoveryReward(hero.HeroDefId, currency, inventory);
        }
    }
}

/// <summary>
/// Lightweight helper that loads all HeroDefinitions from Resources/Heroes/.
/// Includes fallback support for missing star ranks.
/// </summary>
public class HeroRoster
{
    private readonly List<HeroDefinition> _all;

    public HeroRoster()
    {
        HeroDefinition[] loaded = Resources.LoadAll<HeroDefinition>("Heroes");
        _all = new List<HeroDefinition>(loaded);

        if (_all.Count == 0)
            Debug.LogWarning("[HeroRoster] No HeroDefinitions found in Resources/Heroes/. Summons will fail.");
    }

    public HeroDefinition GetRandomByStarRank(int starRank)
    {
        List<HeroDefinition> pool = _all.FindAll(h => h.BaseStarRank == starRank);
        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    /// <summary>
    /// Returns a fallback/placeholder HeroDefinition for the given star rank.
    /// Looks for the specific fallback asset pattern "hero_{rank}star_default" or any hero of that rank.
    /// </summary>
    public HeroDefinition GetFallbackByStarRank(int starRank)
    {
        string fallbackId = $"hero_{starRank}star_default";
        foreach (HeroDefinition h in _all)
        {
            if (h.HeroId == fallbackId)
                return h;
        }
        return null;
    }

    public HeroDefinition GetAny()
    {
        if (_all.Count == 0) return null;
        return _all[Random.Range(0, _all.Count)];
    }
}