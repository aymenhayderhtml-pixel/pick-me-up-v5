using System.Collections.Generic;

public static class HeroPresentationUtility
{
    private static readonly Dictionary<string, string> CanonicalNames = new Dictionary<string, string>
    {
        { "han_israt", "Han Israt" },
        { "islat_han", "Islat Han" },
        { "jenna_shirai", "Jenna Shirai" },
        { "jenna_cirai", "Jenna Cirai" },
        { "aaron_delkard", "Aaron Delkard" },
        { "aaron_delcut", "Aaron Delcut" },
        { "kendert_forest", "Kendert Forest" },
        { "kendert_forst", "Kendert Forst" },
    };

    private static readonly Dictionary<string, string> RoleBadges = new Dictionary<string, string>
    {
        { "Vanguard", "FRONTLINE" },
        { "Scout", "SKIRMISH" },
        { "Mage", "ARCANE" },
        { "Berserker", "FURY" },
        { "Assassin", "HUNTER" },
        { "Support", "AID" },
        { "Specialist", "TACTIC" }
    };

    public static string GetDisplayName(HeroDefinition def, string fallbackId = null)
    {
        if (def == null)
        {
            return string.IsNullOrEmpty(fallbackId) ? "UNKNOWN" : fallbackId.ToUpperInvariant();
        }

        if (!string.IsNullOrEmpty(def.HeroName))
        {
            return NormalizeName(def.HeroName);
        }

        if (!string.IsNullOrEmpty(def.HeroId) && CanonicalNames.TryGetValue(def.HeroId, out string mapped))
        {
            return mapped;
        }

        return !string.IsNullOrEmpty(fallbackId) ? fallbackId.ToUpperInvariant() : "UNKNOWN";
    }

    public static string GetRoleLabel(HeroDefinition def)
    {
        if (def == null)
        {
            return "UNKNOWN";
        }

        if (!string.IsNullOrEmpty(def.HeroClass))
        {
            return def.HeroClass.ToUpperInvariant();
        }

        return def.BaseClass.ToString().ToUpperInvariant();
    }

    public static string GetRoleBadge(HeroDefinition def)
    {
        if (def == null)
        {
            return "UNKNOWN";
        }

        string key = !string.IsNullOrEmpty(def.HeroClass) ? def.HeroClass : def.BaseClass.ToString();
        if (RoleBadges.TryGetValue(key, out string badge))
        {
            return badge;
        }

        return key.ToUpperInvariant();
    }

    public static string GetHeroSubtitle(HeroDefinition def)
    {
        if (def == null)
        {
            return "UNKNOWN";
        }

        return $"{GetRoleLabel(def)} | {GetRoleBadge(def)}";
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "UNKNOWN";
        }

        return char.ToUpperInvariant(value[0]) + (value.Length > 1 ? value.Substring(1) : string.Empty);
    }
}
