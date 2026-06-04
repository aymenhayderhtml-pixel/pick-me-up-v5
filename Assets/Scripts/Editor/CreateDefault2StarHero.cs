using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to create the missing 2★ HeroDefinition.
/// Run this once via Tools > PickMeUp > Create 2★ Default Hero.
/// </summary>
public static class CreateDefault2StarHero
{
    [MenuItem("Tools/PickMeUp/Create 2★ Default Hero")]
    public static void Execute()
    {
        string path = "Assets/Resources/Heroes/hero_2star_default.asset";
        HeroDefinition existing = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
        if (existing != null)
        {
            Debug.Log($"[CreateDefault2StarHero] Already exists at {path}");
            EditorGUIUtility.PingObject(existing);
            return;
        }

        HeroDefinition def = ScriptableObject.CreateInstance<HeroDefinition>();
        def.HeroId = "hero_2star_default";
        def.HeroName = "Trainee";
        def.BaseStarRank = 2;
        def.BaseClass = HeroClass.Vanguard;
        def.HeroClass = "Fighter";
        def.BaseHP = 80;
        def.BaseATK = 12;
        def.BaseDEF = 8;
        def.PortraitSpritePath = "";
        def.CardFrameSpritePath = "";
        def.LoreText = "A fresh recruit from the summoning pool. Unremarkable but determined.";

        System.IO.Directory.CreateDirectory("Assets/Resources/Heroes");
        AssetDatabase.CreateAsset(def, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateDefault2StarHero] Created 2★ default hero at {path}");
        EditorGUIUtility.PingObject(def);
    }
}
