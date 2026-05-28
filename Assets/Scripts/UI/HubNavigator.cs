using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Wires the Hub bottom dock navigation buttons to their target scenes.
/// Attach to HubCanvas. All button references are wired by SetupHubUI editor script.
///
/// Scene name → Build Settings index:
///   Boot    → 0
///   Hub     → 1
///   Summon  → 2
///   Roster  → 3
/// </summary>
public class HubNavigator : MonoBehaviour
{
    [SerializeField] private Button rosterBtn;
    [SerializeField] private Button synthBtn;
    [SerializeField] private Button trainBtn;
    [SerializeField] private Button towerBtn;
    [SerializeField] private Button summonBtn;

    private void Start()
    {
        if (rosterBtn != null) rosterBtn.onClick.AddListener(() => LoadScene("Roster"));
        if (summonBtn != null) summonBtn.onClick.AddListener(() => LoadScene("Summon"));

        // Stub: show "Coming Soon" log until scene is built
        if (synthBtn  != null) synthBtn.onClick.AddListener(() => Debug.Log("[HubNavigator] SYNTH — not yet built."));
        if (trainBtn  != null) trainBtn.onClick.AddListener(() => Debug.Log("[HubNavigator] TRAIN — not yet built."));
        if (towerBtn  != null) towerBtn.onClick.AddListener(() => Debug.Log("[HubNavigator] TOWER — not yet built."));
    }

    private void LoadScene(string sceneName)
    {
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HubNavigator] Failed to load scene '{sceneName}'. Ensure it is in Build Settings. Error: {ex.Message}");
        }
    }
}
