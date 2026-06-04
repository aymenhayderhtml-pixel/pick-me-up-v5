using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonHUD : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private Image currentEnemyPortrait;

    [Header("Bottom Bar")]
    [SerializeField] private Button abandonButton;

    [Header("Popups")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Forwarded Events")]
    [Tooltip("Optional. If assigned, the HUD will forward IDungeonService.OnRewardGranted to it so the reward view does not need its own service reference.")]
    [SerializeField] private DungeonRewardView rewardView;

    private IDungeonService dungeonService;

    private void Awake()
    {
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IDungeonService>())
        {
            dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
        }

        if (abandonButton != null)
        {
            abandonButton.onClick.RemoveListener(OnAbandonClicked);
            abandonButton.onClick.AddListener(OnAbandonClicked);
        }

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (dungeonService == null && ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IDungeonService>())
        {
            dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
        }

        if (dungeonService == null)
        {
            return;
        }

        dungeonService.OnWaveStarted += HandleWaveStarted;
        dungeonService.OnWaveCleared += HandleWaveCleared;
        dungeonService.OnDungeonCleared += HandleDungeonCleared;
        dungeonService.OnDungeonFailed += HandleDungeonFailed;
        if (rewardView != null)
        {
            dungeonService.OnRewardGranted += rewardView.HandleRewardGranted;
        }

        if (dungeonService.IsInDungeon)
        {
            UpdateWaveUI();
        }
    }

    private void OnDisable()
    {
        if (dungeonService == null)
        {
            return;
        }

        dungeonService.OnWaveStarted -= HandleWaveStarted;
        dungeonService.OnWaveCleared -= HandleWaveCleared;
        dungeonService.OnDungeonCleared -= HandleDungeonCleared;
        dungeonService.OnDungeonFailed -= HandleDungeonFailed;
        if (rewardView != null)
        {
            dungeonService.OnRewardGranted -= rewardView.HandleRewardGranted;
        }

        if (abandonButton != null)
        {
            abandonButton.onClick.RemoveListener(OnAbandonClicked);
        }
    }

    private void HandleWaveStarted(EnemyWaveSO waveData)
    {
        UpdateWaveUI();

        if (currentEnemyPortrait == null)
        {
            return;
        }

        EnemyDataSO primaryEnemy = GetFirstValidEnemy(waveData);
        if (primaryEnemy != null && primaryEnemy.Portrait != null)
        {
            currentEnemyPortrait.sprite = primaryEnemy.Portrait;
            currentEnemyPortrait.gameObject.SetActive(true);
        }
        else
        {
            currentEnemyPortrait.gameObject.SetActive(false);
        }
    }

    private EnemyDataSO GetFirstValidEnemy(EnemyWaveSO waveData)
    {
        if (waveData == null || waveData.Spawns == null)
        {
            return null;
        }

        foreach (EnemyWaveSO.SpawnEntry entry in waveData.Spawns)
        {
            if (entry != null && entry.IsValid)
            {
                return entry.EnemyData;
            }
        }

        return null;
    }

    private void HandleWaveCleared()
    {
        UpdateWaveUI();
    }

    private void HandleDungeonCleared()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private void HandleDungeonFailed()
    {
        if (defeatPanel != null) defeatPanel.SetActive(true);
    }

    private void UpdateWaveUI()
    {
        if (dungeonService == null || dungeonService.CurrentRunState == null)
        {
            return;
        }

        DungeonRunState state = dungeonService.CurrentRunState;
        int current = state.CurrentWaveIndex + 1;
        int total = Mathf.Max(1, state.TotalWaves);

        if (waveText != null)
        {
            waveText.text = "Wave " + current + " / " + total;
        }

        if (enemyCountText != null)
        {
            int remaining = Mathf.Max(0, state.TotalEnemiesInCurrentWave - state.EnemiesDefeatedInCurrentWave);
            enemyCountText.text = "Enemies: " + remaining;
        }
    }

    private void OnAbandonClicked()
    {
        if (dungeonService != null)
        {
            dungeonService.AbandonDungeon();
        }
    }
}
