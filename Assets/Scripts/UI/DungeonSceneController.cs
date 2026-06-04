using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonSceneController : MonoBehaviour
{
    [SerializeField] private Button backToHubButton;
    [SerializeField] private string hubSceneName = "Hub";

    private void Awake()
    {
        if (backToHubButton == null)
        {
            return;
        }

        backToHubButton.onClick.RemoveListener(OnBackToHubClicked);
        backToHubButton.onClick.AddListener(OnBackToHubClicked);
    }

    private void OnBackToHubClicked()
    {
        IDungeonService dungeonService = ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IDungeonService>()
            ? ServiceRegistry.Instance.Resolve<IDungeonService>()
            : null;

        if (dungeonService != null && dungeonService.IsInDungeon)
        {
            dungeonService.AbandonDungeon();
        }

        SceneManager.LoadScene(hubSceneName);
    }
}
