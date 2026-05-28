using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DungeonSceneAutoFixer
{
    [MenuItem("Tools/Pick Me Up/Fix Dungeon Scene Now")]
    public static void FixNow()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded || !scene.name.Equals("Dungeon"))
        {
            Debug.LogWarning("Open the Dungeon scene before running this fixer.");
            return;
        }

        GameObject[] canvases = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(go => go.name == "DungeonCanvas")
            .ToArray();

        if (canvases.Length > 1)
        {
            GameObject keep = canvases.FirstOrDefault(go => go.GetComponent<DungeonView>() != null) ?? canvases[0];
            foreach (GameObject canvas in canvases)
            {
                if (canvas != keep)
                {
                    Object.DestroyImmediate(canvas);
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Dungeon scene checked. Use Tools > Pick Me Up > Setup Dungeon UI to rebuild the live layout if needed.");
    }
}
