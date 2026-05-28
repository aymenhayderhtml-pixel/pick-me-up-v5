using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles weighted RNG pulls for Standard and Premium banners.
/// 
/// STANDARD BANNER (Gold)
///   Cost: 1,000 Gold single / 9,000 Gold x10
///   Rates: 1★ 60% | 2★ 25% | 3★ 12% | 4★ 2.5% | 5★ 0.5%
///   Pity: 90 pulls → guaranteed 4★+; 180 pulls → guaranteed 5★
///
/// PREMIUM BANNER (Gems)
///   Cost: 300 Gems single / 2,700 Gems x10
///   Rates: 1★ 40% | 2★ 30% | 3★ 18% | 4★ 9%  | 5★ 3%
///   Pity: 50 pulls → guaranteed 4★+; 100 pulls → guaranteed 5★
/// </summary>
public class GachaService : IGachaService
{
    // ── Costs ──────────────────────────────────────────────────────────────
    private const int StandardSingleCost  = 1000;
    private const int StandardTenCost     = 9000;
    private const int PremiumSingleCost   = 300;
    private const int PremiumTenCost      = 2700;

    // ── Pity thresholds ────────────────────────────────────────────────────
    private const int StandardSoftPity   = 90;   // guaranteed 4★+
    private const int StandardHardPity   = 180;  // guaranteed 5★
    private const int PremiumSoftPity    = 50;   // guaranteed 4★+
    private const int PremiumHardPity    = 100;  // guaranteed 5★

    // ── Dependencies ───────────────────────────────────────────────────────
    private readonly GameStateService  _gameState;
    private readonly ICurrencyService  _currency;
    private readonly HeroRoster        _roster;   // all HeroDefinitions loaded from Resources

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
            starRank = Random.value < 0.15f ? 5 : 4;
            if (starRank >= 4) _gameState.Data.Pity.StandardSummonCount = 0;
        }
        else
        {
            starRank = WeightedRoll(new float[] { 0.60f, 0.25f, 0.12f, 0.025f, 0.005f });
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
            starRank = Random.value < 0.30f ? 5 : 4;
            if (starRank >= 4) _gameState.Data.Pity.PremiumSummonCount = 0;
        }
        else
        {
            starRank = WeightedRoll(new float[] { 0.40f, 0.30f, 0.18f, 0.09f, 0.03f });
        }

        return CreateHeroInstance(starRank);
    }

    /// <summary>
    /// Returns a 1-indexed star rank (1–5) based on cumulative probability weights.
    /// weights[0] = P(1★), weights[1] = P(2★), etc.
    /// </summary>
    private int WeightedRoll(float[] weights)
    {
        float roll = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative) return i + 1;
        }
        return 1; // fallback
    }

    /// <summary>
    /// Picks a random HeroDefinition matching the target star rank and creates a HeroInstance.
    /// Falls back to the nearest available rank if none exist for this rank.
    /// </summary>
    private HeroInstance CreateHeroInstance(int starRank)
    {
        HeroDefinition def = _roster.GetRandomByStarRank(starRank);
        if (def == null)
        {
            Debug.LogWarning($"[GachaService] No HeroDefinition found for {starRank}★. Falling back.");
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
        {
            _gameState.Data.DiscoveredHeroIds = new List<string>();
        }

        if (!string.IsNullOrEmpty(hero.HeroDefId) && !_gameState.Data.DiscoveredHeroIds.Contains(hero.HeroDefId))
        {
            _gameState.Data.DiscoveredHeroIds.Add(hero.HeroDefId);
        }
    }
}

/// <summary>
/// Lightweight helper that loads all HeroDefinitions from Resources/Heroes/.
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

    public HeroDefinition GetAny()
    {
        if (_all.Count == 0) return null;
        return _all[Random.Range(0, _all.Count)];
    }
}
