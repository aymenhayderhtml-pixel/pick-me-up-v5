using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AddDungeonToBuild
{
    [MenuItem("Tools/Add Dungeon To Build")]
    public static void DoIt()
    {
        string scenePath = "Assets/Scenes/Dungeon.unity";
        var scenes = EditorBuildSettings.scenes.ToList();
        
        if (!scenes.Any(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[AddDungeonToBuild] Added " + scenePath + " to Build Settings.");
        }
        else
        {
            Debug.Log("[AddDungeonToBuild] " + scenePath + " is already in Build Settings.");
        }
    }
}
