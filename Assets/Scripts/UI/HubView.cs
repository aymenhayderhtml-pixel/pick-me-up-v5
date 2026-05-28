using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HubView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldLabel;
    [SerializeField] private TextMeshProUGUI gemsLabel;
    [SerializeField] private TextMeshProUGUI stonesLabel;
    [SerializeField] private TextMeshProUGUI staminaLabel;
    [SerializeField] private Button rosterBtn;
    [SerializeField] private Button synthBtn;
    [SerializeField] private Button trainBtn;
    [SerializeField] private Button towerBtn;
    [SerializeField] private Button summonBtn;
    [SerializeField] private Button dungeonBtn;
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button memorialBtn;

    private ICurrencyService _currencyService;
    private IDungeonService _dungeonService;

    private void Start()
    {
        _currencyService = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();

        WireButton(rosterBtn, "Roster");
        WireButton(synthBtn, "SynthesisLab");
        WireButton(trainBtn, "Facilities");
        WireButton(towerBtn, "Tower");
        WireButton(summonBtn, "Summon");
        WireButton(dungeonBtn, "Dungeon");
        WireButton(inventoryBtn, "Inventory");
        WireButton(memorialBtn, "MemorialHall");

        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
        if (_dungeonService != null)
        {
            _dungeonService.ProcessStaminaRegeneration();
        }
    }

    private void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (_currencyService == null)
        {
            return;
        }

        if (goldLabel != null)
        {
            goldLabel.text = "Gold: " + _currencyService.GetGold().ToString("N0");
        }

        if (gemsLabel != null)
        {
            gemsLabel.text = "Gems: " + _currencyService.GetGems().ToString("N0");
        }

        if (stonesLabel != null)
        {
            stonesLabel.text = "Stones: " + _currencyService.GetAttributeStones().ToString("N0");
        }

        if (staminaLabel != null && _dungeonService != null)
        {
            staminaLabel.text = "Stamina: " + _dungeonService.GetStamina() + "/" + _dungeonService.GetMaxStamina();
        }
    }

    private void WireButton(Button button, string sceneName)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
    }
}
