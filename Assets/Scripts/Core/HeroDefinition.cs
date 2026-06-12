using UnityEngine;

// ---------------------------------------------------------------------------
//  HeroClass enum
//  Contains BOTH the new combat roles AND the legacy display classes.
//  Old save data / ScriptableObject assets that stored the legacy names will
//  continue to deserialize without warnings.
// ---------------------------------------------------------------------------
public enum HeroClass
{
    // ── New combat roles (Phase 1) ──────────────────────────────────────────
    Warrior,
    Mage,
    Ranger,
    Healer,
    Tank,

    // ── Legacy display classes (kept for HeroClassColorConfig / HeroColorUtility) ─
    Vanguard,
    Scout,
    Berserker,
    Assassin,
    Support,
    Specialist,

    // ── Catch-all ───────────────────────────────────────────────────────────
    Novice
}

// ---------------------------------------------------------------------------
//  HeroDefinition — ScriptableObject, one per hero archetype
// ---------------------------------------------------------------------------
[CreateAssetMenu(fileName = "NewHeroDefinition", menuName = "PickMeUp/Hero Definition")]
public class HeroDefinition : ScriptableObject
{
    [Header("Identity")]
    public string HeroId;
    public string HeroName;

    // -----------------------------------------------------------------------
    //  Class & Rarity
    //  BaseClass drives combat stats (HeroStatCalculator).
    //  HeroClass is a legacy string field kept for HeroPresentationUtility /
    //  RosterView so existing code compiles without changes.
    // -----------------------------------------------------------------------
    [Header("Class & Rarity")]
    public HeroClass BaseClass;
    public int BaseStarRank = 1;          // 1–5

    [Header("Legacy Class Label (optional – overrides BaseClass display name)")]
    [Tooltip("Leave empty to use BaseClass.ToString() as the display label.")]
    public string HeroClass;              // ← kept for backward compatibility

    // -----------------------------------------------------------------------
    //  Base stats at Level 1, Star 1.
    //  Leave at 0 to use the class default from HeroStatCalculator.
    //  Non-zero values override the class default.
    // -----------------------------------------------------------------------
    [Header("Base Stats (0 = use class defaults from HeroStatCalculator)")]
    public int BaseHP;
    public int BaseATK;
    public int BaseDEF;
    public int BaseSPD;

    [Header("Asset Paths (Resources-based loading)")]
    public string PortraitSpritePath;
    public string CardFrameSpritePath;

    [Header("Collection / Memorial Hall")]
    public Sprite Portrait;
    [TextArea(3, 10)]
    public string LoreText;

    // =======================================================================
    //  PHASE 1 — COMBAT STAT FORMULAS (ADDITIVE)
    //  These methods compute scaled stats without touching serialized fields.
    //  Call these from CombatUnit.FromHero() instead of reading raw fields.
    // =======================================================================

    public int GetMaxHp(int level, int star)
    {
        float rarityMult = GetRarityMultiplier(star);
        float levelMult = 1f + (level - 1) * 0.05f;
        float growthBonus = (level - 1) * 8f;
        return Mathf.RoundToInt((BaseHP + growthBonus) * levelMult * rarityMult);
    }

    public int GetAtk(int level, int star)
    {
        float rarityMult = GetRarityMultiplier(star);
        float levelMult = 1f + (level - 1) * 0.05f;
        float growthBonus = (level - 1) * 1.2f;
        return Mathf.RoundToInt((BaseATK + growthBonus) * levelMult * rarityMult);
    }

    public int GetDef(int level, int star)
    {
        float rarityMult = GetRarityMultiplier(star);
        float levelMult = 1f + (level - 1) * 0.05f;
        float growthBonus = (level - 1) * 0.6f;
        return Mathf.RoundToInt((BaseDEF + growthBonus) * levelMult * rarityMult);
    }

    public int GetSpd(int level, int star)
    {
        float rarityMult = GetRarityMultiplier(star);
        float levelMult = 1f + (level - 1) * 0.03f;
        float growthBonus = (level - 1) * 0.3f;
        return Mathf.RoundToInt((BaseSPD + growthBonus) * levelMult * rarityMult);
    }

    private float GetRarityMultiplier(int star)
    {
        switch (Mathf.Clamp(star, 1, 5))
        {
            case 1: return 1.0f;
            case 2: return 1.3f;
            case 3: return 1.7f;
            case 4: return 2.3f;
            case 5: return 3.0f;
            default: return 1.0f;
        }
    }
}
