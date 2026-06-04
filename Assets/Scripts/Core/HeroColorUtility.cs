using UnityEngine;

/// <summary>
/// Single source of truth for hero-related colors.
/// Replaces the 5 duplicated color systems across views.
/// </summary>
public static class HeroColorUtility
{
    private static HeroClassColorConfig _classConfig;
    private static bool _configLoaded;

    private static readonly Color[] RarityColors = new Color[]
    {
        new Color(0.55f, 0.55f, 0.55f), // 1★ grey
        new Color(0.27f, 0.65f, 0.27f), // 2★ green
        new Color(0.25f, 0.50f, 0.90f), // 3★ blue
        new Color(0.60f, 0.25f, 0.85f), // 4★ purple
        new Color(0.95f, 0.75f, 0.10f), // 5★ gold
    };

    private static readonly Color[] TraitColors = new Color[]
    {
        new Color(0.80f, 0.30f, 0.30f), // Brave      → red
        new Color(0.50f, 0.50f, 0.60f), // Cowardly   → grey-blue
        new Color(0.85f, 0.50f, 0.15f), // Reckless   → orange
        new Color(0.20f, 0.50f, 0.75f), // Disciplined → blue
        new Color(0.30f, 0.70f, 0.40f), // Loyal      → green
        new Color(0.45f, 0.30f, 0.55f), // Traumatized → purple
    };

    private static void EnsureConfig()
    {
        if (_configLoaded) return;
        _classConfig = Resources.Load<HeroClassColorConfig>("Configs/HeroClassColorConfig");

        if (_classConfig == null)
            Debug.LogWarning("[HeroColorUtility] HeroClassColorConfig not found at Resources/Configs/. Create it with Tools > Pick Me Up > Setup Class Color Config.");

        _configLoaded = true;
    }

    // ── Class colors ──────────────────────────────────────────────────────

    public static Color GetClassColor(HeroClass heroClass)
    {
        EnsureConfig();
        if (_classConfig != null)
            return _classConfig.GetClassColor(heroClass);
        return heroClass switch
        {
            HeroClass.Novice => new Color(0.50f, 0.50f, 0.50f),
            HeroClass.Vanguard => new Color(0.90f, 0.30f, 0.30f),
            HeroClass.Scout => new Color(0.20f, 0.70f, 0.60f),
            HeroClass.Mage => new Color(0.30f, 0.30f, 0.90f),
            HeroClass.Berserker => new Color(0.80f, 0.60f, 0.20f),
            HeroClass.Assassin => new Color(0.60f, 0.20f, 0.70f),
            HeroClass.Support => new Color(0.30f, 0.90f, 0.50f),
            HeroClass.Specialist => new Color(0.20f, 0.70f, 0.80f),
            _ => Color.gray
        };
    }

    // ── Rarity colors ─────────────────────────────────────────────────────

    public static Color GetRarityColor(int starRank)
    {
        int index = Mathf.Clamp(starRank - 1, 0, 4);
        return RarityColors[index];
    }

    // ── Trait colors ──────────────────────────────────────────────────────

    public static Color GetTraitColor(PersonalityTrait trait)
    {
        int index = (int)trait;
        if (index >= 0 && index < TraitColors.Length)
            return TraitColors[index];
        return Color.gray;
    }
}
