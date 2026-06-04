using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// LEGACY BOOTSTRAP — Now just creates the minimal canvas + SummonView.
/// The new SummonView builds ALL UI procedurally in Start().
///
/// Run from: Tools > Pick Me Up > Setup Summon UI
/// </summary>
public static class SetupSummonUI
{
    [MenuItem("Tools/Pick Me Up/Setup Summon UI")]
    public static void CreateSummonUI()
    {
        EditorUiSetupUtility.EnsureEventSystem();

        // ── Root canvas ────────────────────────────────────────────────────
        GameObject canvasGo = new GameObject("SummonCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2400, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Add SummonView (builds all UI procedurally) ─────────────────────
        canvasGo.AddComponent<SummonView>();

        // ── Save scene ─────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupSummonUI] SummonCanvas created with SummonView. UI will build procedurally on play.");
    }
}