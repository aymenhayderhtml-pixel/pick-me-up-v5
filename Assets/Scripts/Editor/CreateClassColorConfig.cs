using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates the HeroClassColorConfig asset if missing.
/// </summary>
public static class CreateClassColorConfig
{
    private const string AssetPath = "Assets/Resources/Configs/HeroClassColorConfig.asset";

    [MenuItem("Tools/Pick Me Up/Setup Class Color Config")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Configs"))
            AssetDatabase.CreateFolder("Assets/Resources", "Configs");

        var existing = AssetDatabase.LoadAssetAtPath<HeroClassColorConfig>(AssetPath);
        if (existing != null)
        {
            Debug.Log("[CreateClassColorConfig] Already exists.");
            EditorGUIUtility.PingObject(existing);
            return;
        }

        var config = ScriptableObject.CreateInstance<HeroClassColorConfig>();
        AssetDatabase.CreateAsset(config, AssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateClassColorConfig] Created at {AssetPath}");
        EditorGUIUtility.PingObject(config);
    }
}
