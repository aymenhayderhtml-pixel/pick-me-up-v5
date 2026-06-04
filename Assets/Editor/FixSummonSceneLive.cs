using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class FixSummonSceneLive
{
    static FixSummonSceneLive()
    {
        // Run once after compilation finishes
        EditorApplication.delayCall += DoCleanup;
    }

    private static void DoCleanup()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Summon") return;

        bool modified = false;
        string[] namesToDestroy = new string[] { 
            "InvokeSingle", "InvokeTen", "BottomPanel", 
            "PremiumPanel", "SummonPremiumBtn", "SummonPremiumTenBtn" 
        };
        
        // Collect them first to avoid modifying collection while iterating
        System.Collections.Generic.List<GameObject> toDestroy = new System.Collections.Generic.List<GameObject>();

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            Transform[] children = rootObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child == null) continue;
                
                bool match = false;
                foreach (string name in namesToDestroy)
                {
                    if (child.name == name) match = true;
                }

                if (!match)
                {
                    Image img = child.GetComponent<Image>();
                    Button btn = child.GetComponent<Button>();
                    if (img != null && btn != null)
                    {
                        Color c = img.color;
                        bool isPurple1 = Mathf.Abs(c.r - 0.48f) < 0.05f && Mathf.Abs(c.g - 0.12f) < 0.05f && Mathf.Abs(c.b - 0.63f) < 0.05f;
                        bool isPurple2 = Mathf.Abs(c.r - 0.19f) < 0.05f && Mathf.Abs(c.g - 0.11f) < 0.05f && Mathf.Abs(c.b - 0.57f) < 0.05f;
                        
                        if (isPurple1 || isPurple2)
                        {
                            match = true;
                        }
                    }
                }

                if (match)
                {
                    toDestroy.Add(child.gameObject);
                }
            }
        }

        foreach (GameObject go in toDestroy)
        {
            if (go != null)
            {
                Debug.Log("[FixSummonSceneLive] Destroying hardcoded scene object: " + go.name);
                GameObject.DestroyImmediate(go);
                modified = true;
            }
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[FixSummonSceneLive] Cleanup complete. Please save the scene.");
        }
    }
}
