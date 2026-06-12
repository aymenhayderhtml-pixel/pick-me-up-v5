#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CreateHeroDefinitions : EditorWindow
{
    [MenuItem("PickMeUp/Create Hero Definitions")]
    public static void ShowWindow()
    {
        GetWindow<CreateHeroDefinitions>("Hero Factory");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hero Definition Factory", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("Creates 10 canonical hero ScriptableObjects in Resources/Heroes/", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("CREATE ALL 10 HEROES", GUILayout.Height(50)))
        {
            CreateAllHeroes();
        }
    }

    private static void CreateAllHeroes()
    {
        string folder = "Assets/Resources/Heroes";
        if (!System.IO.Directory.Exists(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        foreach (HeroTemplate template in HeroCanon.Heroes)
        {
            string path = $"{folder}/{template.HeroId}.asset";
            
            // Check if already exists
            HeroDefinition existing = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
            if (existing != null)
            {
                Debug.Log($"[HeroFactory] Skipping {template.HeroId} - already exists.");
                continue;
            }

            HeroDefinition def = ScriptableObject.CreateInstance<HeroDefinition>();
            def.HeroId = template.HeroId;
            def.HeroName = template.HeroName;
            def.BaseClass = template.BaseClass;
            def.BaseStarRank = template.BaseStarRank;
            def.BaseHP = template.BaseHP;
            def.BaseATK = template.BaseATK;
            def.BaseDEF = template.BaseDEF;
            def.BaseSPD = template.BaseSPD;
            def.LoreText = template.LoreText;

            // Auto-generate class label for legacy compatibility
            def.HeroClass = template.BaseClass.ToString();

            AssetDatabase.CreateAsset(def, path);
            Debug.Log($"[HeroFactory] Created {template.HeroId} - {template.HeroName} ({template.BaseClass})");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", "Hero definitions created in Assets/Resources/Heroes/", "OK");
    }
}
#endif
