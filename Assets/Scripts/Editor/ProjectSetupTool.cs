using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ProjectSetupTool
{
    [MenuItem("Tools/Pick Me Up/Setup Project")]
    public static void SetupProject()
    {
        EnsureScenesFolder();

        CreateBootScene();
        CreateScene("Assets/Scenes/Hub.unity", SetupHubUI.Execute);
        CreateScene("Assets/Scenes/Summon.unity", SetupSummonUI.CreateSummonUI);
        CreateScene("Assets/Scenes/Roster.unity", SetupRosterUI.Execute);
        CreateScene("Assets/Scenes/Tower.unity", SetupTowerUI.Execute);
        CreateScene("Assets/Scenes/Dungeon.unity", SetupDungeonUI.Execute);
        CreateScene("Assets/Scenes/Inventory.unity", SetupInventoryUI.Execute);
        CreateScene("Assets/Scenes/MemorialHall.unity", SetupMemorialHallUI.Execute);
        CreateScene("Assets/Scenes/Facilities.unity", SetupFacilityUI.Execute);
        CreateScene("Assets/Scenes/SynthesisLab.unity", SetupSynthUI.Execute);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Hub.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Summon.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Roster.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Tower.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Dungeon.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Inventory.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MemorialHall.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Facilities.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/SynthesisLab.unity", true)
        };

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity", OpenSceneMode.Single);

        Debug.Log("Project setup complete! All scenes created, wired, and added to Build Settings.");
    }

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    private static void CreateBootScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootLoaderObj = new GameObject("BootLoader");
        bootLoaderObj.AddComponent<BootLoader>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
    }

    private static void CreateScene(string scenePath, Action setupAction)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        setupAction?.Invoke();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
