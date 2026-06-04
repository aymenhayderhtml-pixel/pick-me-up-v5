using System.Collections.Generic;

using System.Collections.Generic;
using UnityEngine;

public static class HeroPresentationUtility
{
    // Maps old/alternate HeroDefIds to canonical HeroDefinitions
    // Used for backward-compatible save data that references old IDs
    private static readonly Dictionary<string, string> RedirectDefIds = new Dictionary<string, string>
    {
        { "jenna_shirai", "jenna_cirai" },
        { "aaron_delkard", "aaron_delcut" },
        { "kendert_forest", "kendert_forst" },
    };

    private static readonly Dictionary<string, string> CanonicalNames = new Dictionary<string, string>
    {
        { "han_israt", "Han Israt" },
        { "islat_han", "Islat Han" },
        { "jenna_cirai", "Jenna Cirai" },
        { "aaron_delcut", "Aaron Delcut" },
        { "kendert_forst", "Kendert Forst" },
        { "enok", "Enok" },
        { "chloe", "Chloe" },
        { "gide", "Gide" },
        { "hansen", "Hansen" },
        { "dika", "Dika" },
        { "antaris", "Antaris" },
    };

    private static readonly Dictionary<string, string> RoleBadges = new Dictionary<string, string>
    {
        { "Novice", "INITIATE" },
        { "Vanguard", "FRONTLINE" },
        { "Scout", "SKIRMISH" },
        { "Mage", "ARCANE" },
        { "Berserker", "FURY" },
        { "Assassin", "HUNTER" },
        { "Support", "AID" },
        { "Specialist", "TACTIC" },
    };

    /// <summary>
    /// Resolves a HeroDefId to its canonical form.
    /// Handles old/alternate IDs used in previous save data.
    /// </summary>
    public static string ResolveDefId(string heroDefId)
    {
        if (string.IsNullOrEmpty(heroDefId)) return null;
        if (RedirectDefIds.TryGetValue(heroDefId, out string resolved))
            return resolved;
        return heroDefId;
    }

    public static string GetDisplayName(HeroDefinition def, string fallbackId = null)
    {
        if (def == null)
        {
            if (!string.IsNullOrEmpty(fallbackId))
            {
                string resolved = ResolveDefId(fallbackId);
                if (CanonicalNames.TryGetValue(resolved, out string mapped))
                    return mapped;
                return resolved.ToUpperInvariant();
            }
            return "UNKNOWN";
        }

        if (!string.IsNullOrEmpty(def.HeroName))
            return NormalizeName(def.HeroName);

        if (!string.IsNullOrEmpty(def.HeroId) && CanonicalNames.TryGetValue(def.HeroId, out string mappedName))
            return mappedName;

        return !string.IsNullOrEmpty(fallbackId) ? fallbackId.ToUpperInvariant() : "UNKNOWN";
    }

    public static string GetRoleLabel(HeroDefinition def)
    {
        if (def == null) return "UNKNOWN";

        if (!string.IsNullOrEmpty(def.HeroClass))
            return def.HeroClass.ToUpperInvariant();

        return def.BaseClass.ToString().ToUpperInvariant();
    }

    public static string GetRoleBadge(HeroDefinition def)
    {
        if (def == null) return "UNKNOWN";

        string key = !string.IsNullOrEmpty(def.HeroClass) ? def.HeroClass : def.BaseClass.ToString();
        if (RoleBadges.TryGetValue(key, out string badge))
            return badge;

        return key.ToUpperInvariant();
    }

    public static string GetHeroSubtitle(HeroDefinition def)
    {
        if (def == null) return "UNKNOWN";

        return $"{GetRoleLabel(def)} | {GetRoleBadge(def)}";
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value)) return "UNKNOWN";
        return char.ToUpperInvariant(value[0]) + (value.Length > 1 ? value.Substring(1) : string.Empty);
    }

    /// <summary>
    /// Loads a HeroDefinition with backward-compatible ID resolution.
    /// If the asset doesn't exist for the given heroDefId, checks redirect table.
    /// </summary>
    public static HeroDefinition LoadHeroDefinition(string heroDefId)
    {
        if (string.IsNullOrEmpty(heroDefId)) return null;

        HeroDefinition def = Resources.Load<HeroDefinition>($"Heroes/{heroDefId}");
        if (def != null) return def;

        string resolved = ResolveDefId(heroDefId);
        if (resolved != heroDefId)
            def = Resources.Load<HeroDefinition>($"Heroes/{resolved}");

        return def;
    }
}
