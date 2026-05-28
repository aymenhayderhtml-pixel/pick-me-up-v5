using UnityEngine;



using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixRosterScene
{
    [MenuItem("Tools/Pick Me Up/Fix Roster Scene")]
    public static void FixRoster()
    {
        string scenePath = "Assets/Scenes/Roster.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

        GameObject canvas = null;
        foreach (var go in rootObjects)
        {
            if (go.name == "RosterCanvas")
            {
                canvas = go;
                break;
            }
        }

        if (canvas == null)
        {
            Debug.LogError("RosterCanvas not found in scene! Run SetupRosterUI first.");
            return;
        }

        // --- Fix 1: DetailPanel hidden by default ---
        Transform detailPanel = null;
        foreach (Transform child in canvas.transform)
        {
            if (child.name == "DetailPanel")
            {
                detailPanel = child;
                break;
            }
        }
        if (detailPanel != null)
        {
            detailPanel.gameObject.SetActive(false);
            Debug.Log("Fix 1: DetailPanel set to inactive.");
        }
        else
        {
            Debug.LogError("Fix 1: DetailPanel not found in RosterCanvas.");
        }

        // --- Fix 2: ScrollRect white background ---
        Transform scrollRect = null;
        foreach (Transform child in canvas.transform)
        {
            if (child.name == "ScrollRect")
            {
                scrollRect = child;
                break;
            }
        }
        if (scrollRect != null)
        {
            // Fix the ScrollRect's own Image if it has a white background
            Image scrollImg = scrollRect.GetComponent<Image>();
            if (scrollImg != null)
            {
                scrollImg.color = new Color(0, 0, 0, 0); // transparent
                Debug.Log("Fix 2: ScrollRect Image made transparent.");
            }

            // Fix the Viewport's Image
            Transform viewport = scrollRect.Find("Viewport");
            if (viewport != null)
            {
                Image viewportImg = viewport.GetComponent<Image>();
                if (viewportImg != null)
                {
                    viewportImg.color = new Color(0, 0, 0, 0);
                    Debug.Log("Fix 2: Viewport Image made transparent.");
                }
            }
            else
            {
                Debug.LogError("Fix 2: Viewport child not found under ScrollRect.");
            }
        }
        else
        {
            Debug.LogError("Fix 2: ScrollRect not found in RosterCanvas.");
        }

        // --- Fix 3: Refresh AssetDatabase (hero definitions already moved) ---
        AssetDatabase.Refresh();
        Debug.Log("Fix 3: AssetDatabase refreshed. Hero definitions are now in Assets/Resources/Heroes/.");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Roster scene fixes applied and saved.");
    }
}
