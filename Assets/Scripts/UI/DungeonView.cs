using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DungeonView : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI selectedDungeonText;
    [SerializeField] private TextMeshProUGUI scheduleText;
    [SerializeField] private TextMeshProUGUI todayScheduleText;
    [SerializeField] private RectTransform dungeonGridContent;
    [SerializeField] private RectTransform teamSlotsContent;
    [SerializeField] private Button startRunButton;
    [SerializeField] private TextMeshProUGUI startRunButtonText;
    [SerializeField] private TextMeshProUGUI tacticalSummaryText;
    [SerializeField] private Button scanButton;
    [SerializeField] private Button focusButton;
    [SerializeField] private Button rallyButton;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private Button closeResultsButton;

    private IDungeonService _dungeonService;
    private IRosterService _rosterService;
    private GameStateService _gameState;
    private readonly List<DungeonTeamSlot> _teamSlots = new List<DungeonTeamSlot>();
    private readonly List<Button> _dungeonButtons = new List<Button>();
    private DungeonDefinition _selectedDungeon;

    private void Start()
    {
        _dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
        _rosterService = ServiceRegistry.Instance.Resolve<IRosterService>();
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        }

        if (startRunButton != null)
        {
            startRunButton.onClick.RemoveAllListeners();
            startRunButton.onClick.AddListener(OnStartRunPressed);
        }

        if (closeResultsButton != null)
        {
            closeResultsButton.onClick.RemoveAllListeners();
            closeResultsButton.onClick.AddListener(() => SetResultsVisible(false));
        }

        if (scanButton != null)
        {
            scanButton.onClick.RemoveAllListeners();
            scanButton.onClick.AddListener(() => ToggleSignal(TacticalSignal.Scan));
        }

        if (focusButton != null)
        {
            focusButton.onClick.RemoveAllListeners();
            focusButton.onClick.AddListener(() => ToggleSignal(TacticalSignal.Focus));
        }

        if (rallyButton != null)
        {
            rallyButton.onClick.RemoveAllListeners();
            rallyButton.onClick.AddListener(() => ToggleSignal(TacticalSignal.Rally));
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        BuildDungeonGrid();
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
        if (_dungeonService != null)
        {
            _dungeonService.ProcessStaminaRegeneration();
        }

        RefreshTacticalUI();
    }

    private void Update()
    {
        if (Time.frameCount % 60 == 0 && _dungeonService != null)
        {
            _dungeonService.ProcessStaminaRegeneration();
            RefreshUI();
        }
    }

    private void BuildDungeonGrid()
    {
        if (dungeonGridContent == null || _dungeonService == null)
        {
            return;
        }

        foreach (Transform child in dungeonGridContent)
        {
            Destroy(child.gameObject);
        }

        _dungeonButtons.Clear();

        foreach (DungeonDefinition dungeon in _dungeonService.GetAllDungeons())
        {
            if (dungeon == null)
            {
                continue;
            }

            GameObject card = CreateDungeonCard(dungeon);
            card.transform.SetParent(dungeonGridContent, false);
        }
    }

    private GameObject CreateDungeonCard(DungeonDefinition dungeon)
    {
        GameObject card = new GameObject("DungeonCard_" + dungeon.DungeonId, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 160);

        bool availableToday = DungeonCalendarUtility.IsAvailableToday(dungeon.AvailableDays);

        Image bg = card.GetComponent<Image>();
        bg.color = availableToday
            ? new Color(dungeon.DungeonThemeColor.r * 0.34f, dungeon.DungeonThemeColor.g * 0.34f, dungeon.DungeonThemeColor.b * 0.34f, 0.95f)
            : new Color(0.16f, 0.16f, 0.18f, 0.88f);

        GameObject border = new GameObject("Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(card.transform, false);
        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(3f, 3f);
        borderRT.offsetMax = new Vector2(-3f, -3f);
        Image borderImage = border.GetComponent<Image>();
        borderImage.color = availableToday
            ? new Color(0.88f, 0.75f, 0.32f, 0.45f)
            : new Color(0.45f, 0.25f, 0.25f, 0.35f);

        GameObject badge = new GameObject("OpenBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(card.transform, false);
        RectTransform badgeRT = badge.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.62f, 0.80f);
        badgeRT.anchorMax = new Vector2(0.95f, 0.95f);
        badgeRT.offsetMin = Vector2.zero;
        badgeRT.offsetMax = Vector2.zero;
        Image badgeImage = badge.GetComponent<Image>();
        badgeImage.color = availableToday
            ? new Color(0.20f, 0.38f, 0.18f, 0.95f)
            : new Color(0.40f, 0.16f, 0.16f, 0.95f);

        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => SelectDungeon(dungeon));
        _dungeonButtons.Add(button);

        CreateTMP("Name", dungeon.DisplayName.ToUpperInvariant(), 22, card.transform, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.90f));
        CreateTMP("Info", "FLOOR 1/" + dungeon.TotalFloors, 16, card.transform, new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.54f));
        CreateTMP("Cost", dungeon.BaseStaminaCost + " STAMINA", 16, card.transform, new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.34f));
        CreateTMP("Schedule", DungeonCalendarUtility.GetWeekText(dungeon.AvailableDays).ToUpperInvariant(), 11, card.transform, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.16f));
        CreateTMP("Today", availableToday ? "OPEN TODAY" : "CLOSED TODAY", 12, badge.transform, new Vector2(0f, 0f), new Vector2(1f, 1f));
        card.GetComponent<Button>().interactable = true;
        return card;
    }

    private void SelectDungeon(DungeonDefinition dungeon)
    {
        _selectedDungeon = dungeon;
        if (selectedDungeonText != null)
        {
            selectedDungeonText.text = dungeon.DisplayName;
        }

        if (scheduleText != null)
        {
            scheduleText.text = "Weekly Window: " + DungeonCalendarUtility.GetWeekText(dungeon.AvailableDays);
            scheduleText.color = DungeonCalendarUtility.IsAvailableToday(dungeon.AvailableDays)
                ? new Color(0.90f, 0.82f, 0.45f)
                : new Color(0.85f, 0.45f, 0.45f);
        }

        if (todayScheduleText != null)
        {
            todayScheduleText.text = DungeonCalendarUtility.IsAvailableToday(dungeon.AvailableDays)
                ? "TODAY: OPEN"
                : "TODAY: CLOSED";
            todayScheduleText.color = DungeonCalendarUtility.IsAvailableToday(dungeon.AvailableDays)
                ? new Color(0.85f, 0.95f, 0.70f)
                : new Color(0.95f, 0.55f, 0.55f);
        }

        BuildTeamSlots(dungeon);
        RefreshUI();
    }

    private void BuildTeamSlots(DungeonDefinition dungeon)
    {
        if (teamSlotsContent == null || dungeon == null)
        {
            return;
        }

        foreach (Transform child in teamSlotsContent)
        {
            Destroy(child.gameObject);
        }

        _teamSlots.Clear();
        int slotCount = Mathf.Max(dungeon.MinHeroCount, dungeon.MaxHeroCount);
        for (int i = 0; i < slotCount; i++)
        {
            bool required = i < dungeon.MinHeroCount;
            GameObject slot = CreateTeamSlotObject(i, required);
            slot.transform.SetParent(teamSlotsContent, false);
            _teamSlots.Add(slot.GetComponent<DungeonTeamSlot>());
        }
    }

    private GameObject CreateTeamSlotObject(int slotIndex, bool required)
    {
        GameObject slot = new GameObject("TeamSlot_" + slotIndex, typeof(RectTransform), typeof(Image), typeof(Button), typeof(DungeonTeamSlot));
        RectTransform rt = slot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 180);

        DungeonTeamSlot slotComponent = slot.GetComponent<DungeonTeamSlot>();
        slotComponent.SlotIndex = slotIndex;
        slotComponent.IsRequired = required;
        slotComponent.DungeonView = this;

        Image bg = slot.GetComponent<Image>();
        bg.color = required ? new Color(0.25f, 0.18f, 0.15f, 0.95f) : new Color(0.18f, 0.18f, 0.22f, 0.95f);

        CreateTMP("SlotNumber", (slotIndex + 1).ToString(), 20, slot.transform, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.98f));
        CreateTMP("HeroName", "Empty", 16, slot.transform, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.7f));
        CreateTMP("HeroLevel", "", 14, slot.transform, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.42f));
        CreateTMP("Status", required ? "Required" : "Optional", 14, slot.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.2f));

        return slot;
    }

    public void OnSlotClicked(DungeonTeamSlot slot)
    {
        if (_selectedDungeon == null || slot == null || _rosterService == null)
        {
            return;
        }

        if (slot.HasHero)
        {
            slot.ClearHero();
            RefreshUI();
            return;
        }

        List<HeroInstance> heroes = _rosterService.GetAlive();
        List<string> usedIds = _teamSlots.Where(teamSlot => teamSlot.HasHero).Select(teamSlot => teamSlot.HeroInstanceId).ToList();
        HeroInstance candidate = heroes.FirstOrDefault(hero => !usedIds.Contains(hero.InstanceId));
        if (candidate != null)
        {
            slot.SetHero(candidate.InstanceId);
        }

        RefreshUI();
    }

    private void OnStartRunPressed()
    {
        if (_selectedDungeon == null)
        {
            return;
        }

        List<string> selectedHeroes = _teamSlots.Where(slot => slot.HasHero).Select(slot => slot.HeroInstanceId).ToList();
        DungeonRunResult result = _dungeonService.RunDungeon(_selectedDungeon.DungeonId, selectedHeroes);
        if (resultsText != null)
        {
            resultsText.text = result.Success
                ? "Cleared floor " + result.FloorCleared + "\nGold +" + result.GoldEarned + "\nStones +" + result.StonesEarned
                : result.FailureReason;
        }

        SetResultsVisible(true);
        RefreshUI();
    }

    private void SetResultsVisible(bool visible)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(visible);
        }
    }

    private void RefreshUI()
    {
        if (staminaText != null && _dungeonService != null)
        {
            staminaText.text = _dungeonService.GetStamina() + "/" + _dungeonService.GetMaxStamina();
        }

        if (startRunButton != null)
        {
            bool openToday = _selectedDungeon != null && DungeonCalendarUtility.IsAvailableToday(_selectedDungeon.AvailableDays);
            startRunButton.interactable = _selectedDungeon != null && openToday && _dungeonService.CanRunDungeon(_selectedDungeon.DungeonId, _selectedDungeon.BaseStaminaCost);
            if (startRunButtonText != null)
            {
                startRunButtonText.text = openToday ? "START RUN" : "CLOSED";
            }
        }

        if (titleText != null)
        {
            titleText.text = _selectedDungeon != null ? _selectedDungeon.DisplayName.ToUpperInvariant() : "DUNGEONS";
        }

        if (scheduleText != null && _dungeonService != null && _selectedDungeon == null)
        {
            List<DungeonDefinition> openToday = _dungeonService.GetAvailableDungeonsToday();
            scheduleText.text = "Today's open dungeons: " + string.Join(", ", openToday.Select(d => d.DisplayName));
            scheduleText.color = openToday.Count > 0 ? new Color(0.90f, 0.82f, 0.45f) : new Color(0.85f, 0.45f, 0.45f);
        }

        if (todayScheduleText != null && _dungeonService != null && _selectedDungeon == null)
        {
            List<DungeonDefinition> openToday = _dungeonService.GetAvailableDungeonsToday();
            todayScheduleText.text = openToday.Count > 0 ? "TODAY: " + openToday.Count + " OPEN" : "TODAY: NO OPEN DUNGEONS";
            todayScheduleText.color = openToday.Count > 0 ? new Color(0.85f, 0.95f, 0.70f) : new Color(0.95f, 0.55f, 0.55f);
        }

        RefreshTacticalUI();
    }

    private void ToggleSignal(TacticalSignal signal)
    {
        if (_dungeonService == null)
        {
            return;
        }

        _dungeonService.ToggleTacticalSignal(signal);
        RefreshTacticalUI();
    }

    private void RefreshTacticalUI()
    {
        if (_dungeonService == null)
        {
            return;
        }

        if (tacticalSummaryText != null)
        {
            tacticalSummaryText.text = _selectedDungeon == null
                ? $"Tactical Station: {_dungeonService.GetTacticalSummary()}"
                : $"{_dungeonService.GetTacticalSummary()}";
        }

        if (scanButton != null)
        {
            scanButton.interactable = _dungeonService.GetQueuedTacticalSignalCount() < _dungeonService.GetTacticalSignalLimit() || GetSignalQueued(TacticalSignal.Scan);
        }

        if (focusButton != null)
        {
            focusButton.interactable = _dungeonService.GetQueuedTacticalSignalCount() < _dungeonService.GetTacticalSignalLimit() || GetSignalQueued(TacticalSignal.Focus);
        }

        if (rallyButton != null)
        {
            rallyButton.interactable = _dungeonService.GetQueuedTacticalSignalCount() < _dungeonService.GetTacticalSignalLimit() || GetSignalQueued(TacticalSignal.Rally);
        }
    }

    private bool GetSignalQueued(TacticalSignal signal)
    {
        return _dungeonService != null && _dungeonService.HasTacticalSignal(signal);
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
