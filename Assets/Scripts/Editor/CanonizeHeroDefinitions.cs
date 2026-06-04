using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 1 Identity Lock tool.
/// Ensures all 10 canonical 1-star Novice heroes exist with correct names, classes, and stats.
/// Run from Tools > Pick Me Up > Phase 1: Canonize Hero Definitions
/// </summary>
public static class CanonizeHeroDefinitions
{
    private static readonly string HeroesFolder = "Assets/Resources/Heroes";

    private static readonly List<CanonicalHero> CanonicalHeroes = new List<CanonicalHero>
    {
        new CanonicalHero { HeroId = "han_israt",  HeroName = "Han Israt",  BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "islat_han",  HeroName = "Islat Han",  BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "enok",       HeroName = "Enok",       BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "chloe",      HeroName = "Chloe",      BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "gide",       HeroName = "Gide",       BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "hansen",     HeroName = "Hansen",     BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "dika",       HeroName = "Dika",       BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "jenna_cirai",HeroName = "Jenna Cirai",BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
        new CanonicalHero { HeroId = "aaron_delcut",HeroName = "Aaron Delcut",BaseHP=12, BaseATK=14, BaseDEF=10 },
        new CanonicalHero { HeroId = "antaris",    HeroName = "Antaris",    BaseHP = 12, BaseATK = 14, BaseDEF = 10 },
    };

    [MenuItem("Tools/Pick Me Up/Phase 1: Canonize Hero Definitions")]
    public static void Execute()
    {
        Directory.CreateDirectory(HeroesFolder);
        AssetDatabase.Refresh();

        int created = 0, updated = 0;
        var existingPaths = AssetDatabase.FindAssets("t:HeroDefinition", new[] { HeroesFolder })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .ToDictionary(path => Path.GetFileNameWithoutExtension(path), path => path);

        foreach (CanonicalHero hero in CanonicalHeroes)
        {
            if (existingPaths.TryGetValue(hero.HeroId, out string assetPath))
            {
                // Update existing
                HeroDefinition def = AssetDatabase.LoadAssetAtPath<HeroDefinition>(assetPath);
                if (def != null)
                {
                    def.HeroName = hero.HeroName;
                    def.BaseStarRank = 1;
                    def.BaseClass = HeroClass.Novice;
                    def.BaseHP = hero.BaseHP;
                    def.BaseATK = hero.BaseATK;
                    def.BaseDEF = hero.BaseDEF;
                    def.PortraitSpritePath = string.IsNullOrEmpty(def.PortraitSpritePath) ? "" : def.PortraitSpritePath;
                    if (string.IsNullOrEmpty(def.CardFrameSpritePath))
                        def.CardFrameSpritePath = "frame_1";
                    EditorUtility.SetDirty(def);
                    updated++;
                }
            }
            else
            {
                // Create new
                HeroDefinition def = ScriptableObject.CreateInstance<HeroDefinition>();
                def.HeroId = hero.HeroId;
                def.HeroName = hero.HeroName;
                def.BaseStarRank = 1;
                def.BaseClass = HeroClass.Novice;
                def.BaseHP = hero.BaseHP;
                def.BaseATK = hero.BaseATK;
                def.BaseDEF = hero.BaseDEF;
                def.CardFrameSpritePath = "frame_1";
                def.PortraitSpritePath = "";

                string path = Path.Combine(HeroesFolder, hero.HeroId + ".asset");
                AssetDatabase.CreateAsset(def, path);
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CanonizeHeroDefinitions] Phase 1 complete. Created: {created}, Updated: {updated}. " +
                  $"Total canonical 1-star Novice heroes: {CanonicalHeroes.Count}");
        EditorUtility.DisplayDialog("Phase 1 Complete",
            $"Created: {created}\nUpdated: {updated}\nTotal canonical 1-star Novice heroes: {CanonicalHeroes.Count}",
            "OK");
    }

    [MenuItem("Tools/Pick Me Up/Phase 1: Remove Old Redirect Assets")]
    public static void DeleteOldRedirects()
    {
        string[] oldIds = { "jenna_shirai", "aaron_delkard" };
        int deleted = 0;

        foreach (string oldId in oldIds)
        {
            string path = Path.Combine(HeroesFolder, oldId + ".asset");
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                deleted++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[CanonizeHeroDefinitions] Deleted {deleted} old redirect assets.");
        EditorUtility.DisplayDialog("Cleanup Complete", $"Deleted {deleted} old redirect assets.", "OK");
    }

    private class CanonicalHero
    {
        public string HeroId;
        public string HeroName;
        public int BaseHP;
        public int BaseATK;
        public int BaseDEF;
    }
}
