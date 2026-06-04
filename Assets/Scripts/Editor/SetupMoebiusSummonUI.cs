#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// MenuItem: PickMeUp/Setup Moebius Summon UI
///
/// CLEANUP ONLY — The new SummonView builds all UI procedurally.
/// This script finds and removes old scene objects that would conflict
/// with the procedural build, then triggers a full rebuild on play.
///
/// Run this once after importing the new SummonView.cs to clean up old
/// Inspector-wired objects.
/// </summary>
public static class SetupMoebiusSummonUI
{
    private static readonly string[] OldObjectsToRemove = new string[]
    {
        "SummonStandardBtn",
        "SummonStandardTenBtn",
        "SummonPremiumBtn",
        "SummonPremiumTenBtn",
        "BackButton",
        "GoldCostLabel",
        "GemsCostLabel",
        "PityStandardLabel",
        "PityPremiumLabel",
        "CardRevealPanel",
        "CardRoot",
        "CardPortrait",
        "CardFrame",
        "CardNameLabel",
        "CardClassLabel",
        "StarsContainer",
        "CrackOverlay",
        "InvokePanel",
        "RitualLayer",
        "MagicCircle",
        "EnvironmentGlow",
        "Silhouette",
        "MaterializeImage",
        "BondLabel",
        "TotalCounter",
        "FiveStarCounter",
        "StatusResult",
        "DimOverlay",
    };

    [MenuItem("PickMeUp/Setup Moebius Summon UI")]
    public static void Setup()
    {
        string scenePath = "Assets/Scenes/Summon.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("[SetupMoebiusSummonUI] Summon.unity not found at: " + scenePath);
            return;
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Scene scene = SceneManager.GetActiveScene();

        // Remove old objects
        int removedCount = 0;
        foreach (string objName in OldObjectsToRemove)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] children = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.name == objName)
                    {
                        GameObject.DestroyImmediate(child.gameObject);
                        removedCount++;
                    }
                }
            }
        }

        // Ensure SummonView exists (it should be on an existing GameObject in the scene)
        SummonView view = Object.FindFirstObjectByType<SummonView>();
        if (view == null)
        {
            // Create a new GameObject with SummonView if none exists
            GameObject go = new GameObject("SummonView", typeof(SummonView));
            Debug.Log("[SetupMoebiusSummonUI] Created SummonView on new GameObject.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[SetupMoebiusSummonUI] Cleanup complete. Removed {removedCount} old objects. SummonView will build UI procedurally on play.");
    }
}
#endif