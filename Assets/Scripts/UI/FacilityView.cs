using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FacilityView : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI memorialFragmentsText;
    [SerializeField] private TextMeshProUGUI moraleText;
    [SerializeField] private RectTransform facilityGridContent;
    [SerializeField] private Button commandTabButton;
    [SerializeField] private Button shadowTabButton;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailRoleText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    [SerializeField] private TextMeshProUGUI detailLevelText;
    [SerializeField] private TextMeshProUGUI detailBenefitText;
    [SerializeField] private TextMeshProUGUI detailEmotionText;
    [SerializeField] private TextMeshProUGUI detailCostText;
    [SerializeField] private TextMeshProUGUI detailWarningText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject dockPanel;
    [SerializeField] private TextMeshProUGUI dockStatusText;
    [SerializeField] private Button reconButton;
    [SerializeField] private Button supplyButton;
    [SerializeField] private Button extractionButton;
    [SerializeField] private Button launchSortieButton;

    private IFacilityService _facilityService;
    private ICurrencyService _currencyService;
    private readonly List<Button> _facilityButtons = new List<Button>();
    private string _selectedFacilityId;
    private bool _showShadowFacilities = true;

    private void Start()
    {
        _facilityService = ServiceRegistry.Instance.Resolve<IFacilityService>();
        _currencyService = ServiceRegistry.Instance.Resolve<ICurrencyService>();

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        }

        if (commandTabButton != null)
        {
            commandTabButton.onClick.RemoveAllListeners();
            commandTabButton.onClick.AddListener(() => SetTab(false));
        }

        if (shadowTabButton != null)
        {
            shadowTabButton.onClick.RemoveAllListeners();
            shadowTabButton.onClick.AddListener(() => SetTab(true));
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradePressed);
        }

        if (reconButton != null)
        {
            reconButton.onClick.RemoveAllListeners();
            reconButton.onClick.AddListener(() => ToggleSortie(DockSortieType.Recon));
        }

        if (supplyButton != null)
        {
            supplyButton.onClick.RemoveAllListeners();
            supplyButton.onClick.AddListener(() => ToggleSortie(DockSortieType.Supply));
        }

        if (extractionButton != null)
        {
            extractionButton.onClick.RemoveAllListeners();
            extractionButton.onClick.AddListener(() => ToggleSortie(DockSortieType.Extraction));
        }

        if (launchSortieButton != null)
        {
            launchSortieButton.onClick.RemoveAllListeners();
            launchSortieButton.onClick.AddListener(LaunchSortie);
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        if (dockPanel != null)
        {
            dockPanel.SetActive(false);
        }

        BuildFacilityGrid();
        RefreshUI();
    }

    private void OnEnable()
    {
        if (_currencyService == null)
        {
            return;
        }

        RefreshUI();
    }

    private void SetTab(bool shadow)
    {
        _showShadowFacilities = shadow;
        _selectedFacilityId = string.Empty;
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        BuildFacilityGrid();
        RefreshUI();
    }

    private void BuildFacilityGrid()
    {
        if (facilityGridContent == null || _facilityService == null)
        {
            return;
        }

        foreach (Transform child in facilityGridContent)
        {
            Destroy(child.gameObject);
        }

        _facilityButtons.Clear();

        foreach (FacilityDefinition facility in _facilityService.GetAllFacilities())
        {
            if (facility == null)
            {
                continue;
            }

            bool isShadow = _facilityService.IsShadowFacility(facility.FacilityId);
            if (isShadow != _showShadowFacilities)
            {
                continue;
            }

            GameObject card = CreateFacilityCard(facility);
            card.transform.SetParent(facilityGridContent, false);
        }
    }

    private GameObject CreateFacilityCard(FacilityDefinition facility)
    {
        int level = GetLevel(facility.FacilityId);
        GameObject card = new GameObject("FacilityCard_" + facility.FacilityId, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 170);
        card.GetComponent<Image>().color = new Color(facility.FacilityColor.r * 0.3f, facility.FacilityColor.g * 0.3f, facility.FacilityColor.b * 0.3f, 0.88f);

        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => SelectFacility(facility.FacilityId));
        _facilityButtons.Add(button);

        CreateTMP("Name", facility.DisplayName, 20, card.transform, new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.92f));
        CreateTMP("Role", _facilityService.GetFacilityRole(facility.FacilityId), 14, card.transform, new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.58f));
        CreateTMP("Info", facility.GetBenefitDescription(level), 14, card.transform, new Vector2(0.05f, 0.23f), new Vector2(0.95f, 0.40f));
        CreateTMP("Level", $"Lv. {level}", 16, card.transform, new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.20f));
        return card;
    }

    private int GetLevel(string facilityId)
    {
        return _facilityService != null ? _facilityService.GetFacilityLevel(facilityId) : 0;
    }

    private void SelectFacility(string facilityId)
    {
        _selectedFacilityId = facilityId;
        FacilityDefinition facility = _facilityService.GetFacility(facilityId);
        if (facility == null)
        {
            return;
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        bool isDock = facility.FacilityType == FacilityType.FlyingDock;
        if (dockPanel != null)
        {
            dockPanel.SetActive(isDock);
        }

        int level = _facilityService.GetFacilityLevel(facilityId);
        UpgradeCost cost = _facilityService.GetUpgradeCost(facilityId);
        bool canUpgrade = _facilityService.CanUpgrade(facilityId);

        if (detailNameText != null)
        {
            detailNameText.text = facility.DisplayName;
        }

        if (detailRoleText != null)
        {
            detailRoleText.text = $"{_facilityService.GetFacilityRole(facilityId)} | {(_facilityService.IsShadowFacility(facilityId) ? "SHADOW" : "COMMAND")}";
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = facility.Description;
        }

        if (detailLevelText != null)
        {
            detailLevelText.text = $"Level {level} / {facility.MaxLevel}";
        }

        if (detailBenefitText != null)
        {
            detailBenefitText.text = facility.GetBenefitDescription(level);
        }

        if (detailEmotionText != null)
        {
            detailEmotionText.text = _facilityService.GetFacilityEmotion(facilityId);
        }

        if (detailCostText != null)
        {
            detailCostText.text = $"Cost: {cost.Gold:N0} Gold{(cost.Gems > 0 ? $" + {cost.Gems:N0} Gems" : string.Empty)}";
        }

        if (detailWarningText != null)
        {
            detailWarningText.text = _facilityService.IsShadowFacility(facilityId)
                ? "Shadow facilities change the shape of the roster. Use them with intent."
                : "Command facilities support daily growth and long-term stability.";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = canUpgrade;
            TextMeshProUGUI buttonText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canUpgrade ? "COMMIT UPGRADE" : "UNAVAILABLE";
            }
        }

        RefreshDockUI();
    }

    private void OnUpgradePressed()
    {
        if (string.IsNullOrEmpty(_selectedFacilityId))
        {
            return;
        }

        _facilityService.StartUpgrade(_selectedFacilityId);
        SelectFacility(_selectedFacilityId);
        RefreshUI();
        BuildFacilityGrid();
    }

    private void RefreshUI()
    {
        if (_currencyService == null)
        {
            return;
        }

        if (goldText != null)
        {
            goldText.text = _currencyService.GetGold().ToString("N0");
        }

        if (gemsText != null)
        {
            gemsText.text = _currencyService.GetGems().ToString("N0");
        }

        if (memorialFragmentsText != null)
        {
            memorialFragmentsText.text = _currencyService.GetMemorialFragments().ToString("N0");
        }

        if (moraleText != null)
        {
            int averageMorale = GetAverageMorale();
            moraleText.text = $"Morale {averageMorale}/100";
            moraleText.color = averageMorale < 30 ? new Color(0.9f, 0.35f, 0.35f) : averageMorale < 60 ? new Color(0.9f, 0.8f, 0.35f) : new Color(0.55f, 0.9f, 0.55f);
        }

        if (titleText != null)
        {
            titleText.text = _showShadowFacilities ? "SHADOW FACILITIES" : "COMMAND FACILITIES";
        }

        RefreshDockUI();
    }

    private int GetAverageMorale()
    {
        if (_facilityService == null)
        {
            return 0;
        }

        List<HeroInstance> heroes = ServiceRegistry.Instance.Resolve<IRosterService>().GetAlive();
        if (heroes.Count == 0)
        {
            return 0;
        }

        return Mathf.RoundToInt((float)heroes.Average(hero => hero.Morale));
    }

    private void ToggleSortie(DockSortieType sortieType)
    {
        if (_facilityService == null)
        {
            return;
        }

        _facilityService.ToggleSortieType(sortieType);
        RefreshDockUI();
    }

    private void LaunchSortie()
    {
        if (_facilityService == null)
        {
            return;
        }

        if (_facilityService.LaunchSortie(out DockSortieResult result))
        {
            if (detailWarningText != null)
            {
                detailWarningText.text = result.Summary;
            }

            RefreshUI();
            SelectFacility("flying_dock");
        }
        else if (detailWarningText != null)
        {
            detailWarningText.text = result.Summary;
        }
    }

    private void RefreshDockUI()
    {
        if (dockPanel == null)
        {
            return;
        }

        bool active = _selectedFacilityId == "flying_dock";
        dockPanel.SetActive(active);

        if (!active || _facilityService == null)
        {
            return;
        }

        DockSortieType sortieType = _facilityService.GetQueuedSortieType();
        if (dockStatusText != null)
        {
            dockStatusText.text = $"Queued Sortie: {sortieType.ToString().ToUpperInvariant()}";
        }

        SetButtonLabel(reconButton, sortieType == DockSortieType.Recon ? "RECON ✓" : "RECON");
        SetButtonLabel(supplyButton, sortieType == DockSortieType.Supply ? "SUPPLY ✓" : "SUPPLY");
        SetButtonLabel(extractionButton, sortieType == DockSortieType.Extraction ? "EXTRACTION ✓" : "EXTRACTION");

        if (launchSortieButton != null)
        {
            launchSortieButton.interactable = true;
        }
    }

    private void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = label;
        }
    }

    private GameObject CreateTMP(string name, string text, float fontSize, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }
}
