using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class FacilitySceneAutoFixer
{
    static FacilitySceneAutoFixer()
    {
        EditorApplication.delayCall += TryFixFacilityScene;
    }

    [MenuItem("Tools/Pick Me Up/Fix Facility Scene Now")]
    public static void TryFixFacilityScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || scene.name != "Facilities")
        {
            return;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .Where(canvas => canvas != null && canvas.gameObject.name == "FacilityCanvas")
            .ToArray();

        if (canvases.Length <= 1)
        {
            return;
        }

        Canvas keep = canvases
            .OrderByDescending(canvas => canvas.GetComponent<FacilityView>() != null)
            .ThenByDescending(canvas => canvas.transform.childCount)
            .First();

        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas != keep)
            {
                Object.DestroyImmediate(canvas.gameObject);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Pick Me Up: cleaned duplicate FacilityCanvas objects.");
    }
}
