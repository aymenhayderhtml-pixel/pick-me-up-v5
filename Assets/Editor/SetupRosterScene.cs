using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor menu item: Tools/Pick Me Up/Setup Roster Scene
/// Opens Assets/Scenes/Roster.unity, clears it, creates a
/// Canvas + RosterView (landscape 2340x1080), and saves the scene.
/// </summary>
public class SetupRosterScene
{
    [MenuItem("Tools/Pick Me Up/Setup Roster Scene")]
    public static void Execute()
    {
        // 1. Open the scene
        string scenePath = "Assets/Scenes/Roster.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 2. Destroy all existing root GameObjects
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Object.DestroyImmediate(root);
        }

        // 3. Create RosterCanvas
        GameObject canvasObj = new GameObject("RosterCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2340, 1080);
        scaler.matchWidthOrHeight = 1.0f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 4. Create RosterView child stretched to fill canvas
        GameObject rosterViewObj = new GameObject("RosterView");
        rosterViewObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = rosterViewObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        rosterViewObj.AddComponent<RosterView>();

        // 5. Add EventSystem if none exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 6. Save scene
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[SetupRosterScene] Done.");
    }
}
