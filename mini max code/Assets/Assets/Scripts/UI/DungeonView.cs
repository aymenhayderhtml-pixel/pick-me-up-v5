using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using PickMeUp.Game.Core;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.UI
{
    /// <summary>
    /// Main UI controller for the Dungeon system.
    /// Handles dungeon selection, team building, and results display.
    /// </summary>
    public class DungeonView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform HeaderArea;
        public RectTransform BodyArea;
        public RectTransform FooterArea;

        [Header("UI Elements")]
        public Button BackButton;
        public TextMeshProUGUI StaminaText;
        public TextMeshProUGUI DungeonTitleText;
        public GridLayoutGroup DungeonGrid;
        public GridLayoutGroup TeamSlotsGrid;
        public Button StartRunButton;
        public Button CloseResultsButton;

        [Header("Prefabs")]
        public RectTransform DungeonCardPrefab;
        public RectTransform TeamSlotPrefab;
        public RectTransform ResultsPanelPrefab;
        public RectTransform FloorPreviewPrefab;

        [Header("Colors")]
        public Color SuccessColor = Color.green;
        public Color FailureColor = Color.red;
        public Color StaminaFullColor = Color.green;
        public Color StaminaLowColor = Color.yellow;
        public Color StaminaEmptyColor = Color.red;

        // Services
        private IDungeonService _dungeonService;
        private IRosterService _rosterService;
        private ICurrencyService _currencyService;
        private IGameStateService _gameStateService;

        // State
        private DungeonDefinition _selectedDungeon;
        private List<DungeonTeamSlot> _teamSlots = new List<DungeonTeamSlot>();
        private List<string> _selectedHeroIds = new List<string>();
        private FloorData _currentFloorData;
        private bool _isInTeamSelectionMode;
        private bool _isInResultsMode;

        // UI State
        private RectTransform _resultsPanel;
        private List<RectTransform> _dungeonCards = new List<RectTransform>();
        private RectTransform _floorPreview;

        public void Initialize(
            IDungeonService dungeonService,
            IRosterService rosterService,
            ICurrencyService currencyService,
            IGameStateService gameStateService)
        {
            _dungeonService = dungeonService;
            _rosterService = rosterService;
            _currencyService = currencyService;
            _gameStateService = gameStateService;

            SetupUI();
            SetupEventListeners();
            RefreshUI();
        }

        private void SetupUI()
        {
            ClearExistingUI();

            // Create header
            CreateHeader();

            // Create dungeon selection area
            CreateDungeonSelectionArea();

            // Create team selection area (hidden initially)
            CreateTeamSelectionArea();

            // Create results panel (hidden initially)
            CreateResultsPanel();
        }

        private void ClearExistingUI()
        {
            if (ContentArea != null)
            {
                foreach (Transform child in ContentArea)
                {
                    Destroy(child.gameObject);
                }
            }

            _dungeonCards.Clear();
            _teamSlots.Clear();
        }

        private void CreateHeader()
        {
            var headerContainer = new GameObject("HeaderContainer", typeof(RectTransform));
            headerContainer.transform.SetParent(HeaderArea, false);

            var headerRT = headerContainer.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 0.7f);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            // Back button
            var backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnObj.transform.SetParent(headerRT, false);
            var backBtnRT = backBtnObj.GetComponent<RectTransform>();
            backBtnRT.anchorMin = new Vector2(0, 0.5f);
            backBtnRT.anchorMax = new Vector2(0, 0.5f);
            backBtnRT.pivot = new Vector2(0, 0.5f);
            backBtnRT.anchoredPosition = new Vector2(30, 0);
            backBtnRT.sizeDelta = new Vector2(120, 60);

            var backBtnImage = backBtnObj.GetComponent<Image>();
            backBtnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            BackButton = backBtnObj.GetComponent<Button>();
            BackButton.onClick.AddListener(OnBackPressed);

            var backTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            var backText = backTextObj.GetComponent<TextMeshProUGUI>();
            backText.text = "< Back";
            backText.fontSize = 24;
            backText.alignment = TextAlignmentOptions.Center;
            backText.color = Color.white;

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(headerRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.3f, 0.5f);
            titleRT.anchorMax = new Vector2(0.7f, 0.5f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            titleRT.sizeDelta = new Vector2(0, 50);

            DungeonTitleText = titleObj.GetComponent<TextMeshProUGUI>();
            DungeonTitleText.text = "Dungeons";
            DungeonTitleText.fontSize = 32;
            DungeonTitleText.alignment = TextAlignmentOptions.Center;
            DungeonTitleText.color = Color.white;

            // Stamina display
            var staminaObj = new GameObject("StaminaDisplay", typeof(RectTransform), typeof(Image));
            staminaObj.transform.SetParent(headerRT, false);
            var staminaRT = staminaObj.GetComponent<RectTransform>();
            staminaRT.anchorMin = new Vector2(0.8f, 0.5f);
            staminaRT.anchorMax = new Vector2(1, 0.5f);
            staminaRT.pivot = new Vector2(1, 0.5f);
            staminaRT.anchoredPosition = new Vector2(-30, 0);
            staminaRT.sizeDelta = new Vector2(200, 60);

            var staminaBg = staminaObj.GetComponent<Image>();
            staminaBg.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

            var staminaTextObj = new GameObject("StaminaText", typeof(RectTransform), typeof(TextMeshProUGUI));
            staminaTextObj.transform.SetParent(staminaObj.transform, false);
            StaminaText = staminaTextObj.GetComponent<TextMeshProUGUI>();
            StaminaText.text = "100/100";
            StaminaText.fontSize = 24;
            StaminaText.alignment = TextAlignmentOptions.Center;
            StaminaText.color = StaminaFullColor;
        }

        private void CreateDungeonSelectionArea()
        {
            var selectionContainer = new GameObject("DungeonSelection", typeof(RectTransform));
            selectionContainer.transform.SetParent(BodyArea, false);

            var containerRT = selectionContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            // Title
            var sectionTitleObj = new GameObject("SectionTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            sectionTitleObj.transform.SetParent(containerRT, false);
            var sectionTitleRT = sectionTitleObj.GetComponent<RectTransform>();
            sectionTitleRT.anchorMin = new Vector2(0, 0.9f);
            sectionTitleRT.anchorMax = new Vector2(1, 1);
            sectionTitleRT.offsetMin = new Vector2(20, 0);
            sectionTitleRT.offsetMax = new Vector2(-20, -10);

            var sectionTitle = sectionTitleObj.GetComponent<TextMeshProUGUI>();
            sectionTitle.text = "Select Dungeon";
            sectionTitle.fontSize = 28;
            sectionTitle.color = Color.white;

            // Dungeon grid
            var gridObj = new GameObject("DungeonGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(containerRT, false);
            var gridRT = gridObj.GetComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0, 0);
            gridRT.anchorMax = new Vector2(1, 0.9f);
            gridRT.offsetMin = new Vector2(20, 20);
            gridRT.offsetMax = new Vector2(-20, 0);

            DungeonGrid = gridObj.GetComponent<GridLayoutGroup>();
            DungeonGrid.cellSize = new Vector2(300, 180);
            DungeonGrid.spacing = new Vector2(20, 20);
            DungeonGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            DungeonGrid.constraintCount = 2;
            DungeonGrid.startAxis = GridLayoutGroup.Axis.Vertical;
            DungeonGrid.padding = new RectOffset(10, 10, 10, 10);

            // Populate dungeon cards
            var dungeons = _dungeonService.GetAllDungeons();
            foreach (var dungeon in dungeons)
            {
                CreateDungeonCard(dungeon);
            }
        }

        private void CreateDungeonCard(DungeonDefinition dungeon)
        {
            var cardObj = Instantiate(DungeonCardPrefab, DungeonGrid.transform, false);
            cardObj.name = $"DungeonCard_{dungeon.DungeonId}";

            var cardContainer = cardObj.AddComponent<DungeonCardData>();
            cardContainer.Dungeon = dungeon;

            // Card background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(cardObj, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(
                dungeon.DungeonThemeColor.r * 0.3f,
                dungeon.DungeonThemeColor.g * 0.3f,
                dungeon.DungeonThemeColor.b * 0.3f,
                0.8f
            );

            // Dungeon icon
            if (dungeon.DungeonIcon != null)
            {
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardObj, false);
                var iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.1f, 0.5f);
                iconRT.anchorMax = new Vector2(0.3f, 0.9f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;

                var iconImage = iconObj.GetComponent<Image>();
                iconImage.sprite = dungeon.DungeonIcon;
                iconImage.preserveAspect = true;
            }

            // Name
            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(cardObj, false);
            var nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.35f, 0.6f);
            nameRT.anchorMax = new Vector2(0.95f, 0.85f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            var nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = dungeon.DisplayName;
            nameText.fontSize = 22;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;

            // Progress
            var progressObj = new GameObject("Progress", typeof(RectTransform), typeof(TextMeshProUGUI));
            progressObj.transform.SetParent(cardObj, false);
            var progressRT = progressObj.GetComponent<RectTransform>();
            progressRT.anchorMin = new Vector2(0.35f, 0.35f);
            progressRT.anchorMax = new Vector2(0.95f, 0.55f);
            progressRT.offsetMin = Vector2.zero;
            progressRT.offsetMax = Vector2.zero;

            int progress = _dungeonService.GetDungeonProgress(dungeon.DungeonId);
            var progressText = progressObj.GetComponent<TextMeshProUGUI>();
            progressText.text = $"Floor: {progress}/{dungeon.TotalFloors}";
            progressText.fontSize = 18;
            progressText.color = Color.yellow;
            progressText.alignment = TextAlignmentOptions.Left;

            // Type badge
            var typeObj = new GameObject("TypeBadge", typeof(RectTransform), typeof(Image));
            typeObj.transform.SetParent(cardObj, false);
            var typeRT = typeObj.GetComponent<RectTransform>();
            typeRT.anchorMin = new Vector2(0.35f, 0.1f);
            typeRT.anchorMax = new Vector2(0.65f, 0.3f);
            typeRT.offsetMin = Vector2.zero;
            typeRT.offsetMax = Vector2.zero;

            var typeBg = typeObj.GetComponent<Image>();
            typeBg.color = GetDungeonTypeColor(dungeon.DungeonType);

            var typeTextObj = new GameObject("TypeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            typeTextObj.transform.SetParent(typeObj.transform, false);
            var typeText = typeTextObj.GetComponent<TextMeshProUGUI>();
            typeText.text = dungeon.DungeonType.ToString();
            typeText.fontSize = 14;
            typeText.color = Color.white;
            typeText.alignment = TextAlignmentOptions.Center;

            // Stamina cost
            var staminaCostObj = new GameObject("StaminaCost", typeof(RectTransform), typeof(TextMeshProUGUI));
            staminaCostObj.transform.SetParent(cardObj, false);
            var staminaCostRT = staminaCostObj.GetComponent<RectTransform>();
            staminaCostRT.anchorMin = new Vector2(0.7f, 0.1f);
            staminaCostRT.anchorMax = new Vector2(0.95f, 0.3f);
            staminaCostRT.offsetMin = Vector2.zero;
            staminaCostRT.offsetMax = Vector2.zero;

            var staminaCostText = staminaCostObj.GetComponent<TextMeshProUGUI>();
            staminaCostText.text = $"{(int)(dungeon.BaseStaminaCost * (1 + progress * 0.05f))} ⚡";
            staminaCostText.fontSize = 16;
            staminaCostText.color = Color.cyan;
            staminaCostText.alignment = TextAlignmentOptions.Right;

            // Button component for interaction
            var button = cardObj.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => OnDungeonSelected(dungeon));

            _dungeonCards.Add(cardObj);
        }

        private void CreateTeamSelectionArea()
        {
            var teamContainer = new GameObject("TeamSelection", typeof(RectTransform));
            teamContainer.transform.SetParent(BodyArea, false);
            var teamRT = teamContainer.GetComponent<RectTransform>();
            teamRT.anchorMin = Vector2.zero;
            teamRT.anchorMax = Vector2.one;
            teamRT.offsetMin = Vector2.zero;
            teamRT.offsetMax = Vector2.zero;
            teamRT.gameObject.SetActive(false);

            _isInTeamSelectionMode = false;

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(teamRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.85f);
            titleRT.anchorMax = new Vector2(1, 0.95f);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "Select Your Team";
            titleText.fontSize = 28;
            titleText.color = Color.white;

            // Team slots grid
            var slotsGridObj = new GameObject("TeamSlotsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            slotsGridObj.transform.SetParent(teamRT, false);
            var slotsGridRT = slotsGridObj.GetComponent<RectTransform>();
            slotsGridRT.anchorMin = new Vector2(0, 0.5f);
            slotsGridRT.anchorMax = new Vector2(1, 0.85f);
            slotsGridRT.offsetMin = new Vector2(20, 10);
            slotsGridRT.offsetMax = new Vector2(-20, 0);

            TeamSlotsGrid = slotsGridObj.GetComponent<GridLayoutGroup>();
            TeamSlotsGrid.cellSize = new Vector2(150, 180);
            TeamSlotsGrid.spacing = new Vector2(15, 15);
            TeamSlotsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            TeamSlotsGrid.constraintCount = 5;
            TeamSlotsGrid.startAxis = GridLayoutGroup.Axis.Horizontal;

            // Create team slots
            if (_selectedDungeon != null)
            {
                for (int i = 0; i < _selectedDungeon.MaxHeroCount; i++)
                {
                    CreateTeamSlot(i, i < _selectedDungeon.MinHeroCount);
                }
            }

            // Hero selection area (placeholder for hero picker)
            var heroSelectObj = new GameObject("HeroSelectArea", typeof(RectTransform), typeof(Image));
            heroSelectObj.transform.SetParent(teamRT, false);
            var heroSelectRT = heroSelectObj.GetComponent<RectTransform>();
            heroSelectRT.anchorMin = new Vector2(0, 0.05f);
            heroSelectRT.anchorMax = new Vector2(1, 0.5f);
            heroSelectRT.offsetMin = new Vector2(20, 10);
            heroSelectRT.offsetMax = new Vector2(-20, 0);

            var heroSelectImage = heroSelectObj.GetComponent<Image>();
            heroSelectImage.color = new Color(0.15f, 0.15f, 0.2f, 0.5f);

            var heroSelectTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            heroSelectTextObj.transform.SetParent(heroSelectObj.transform, false);
            var heroSelectText = heroSelectTextObj.GetComponent<TextMeshProUGUI>();
            heroSelectText.text = "Tap a slot to select a hero";
            heroSelectText.fontSize = 20;
            heroSelectText.color = new Color(1, 1, 1, 0.5f);
            heroSelectText.alignment = TextAlignmentOptions.Center;

            // Start run button
            var startBtnObj = new GameObject("StartRunButton", typeof(RectTransform), typeof(Image), typeof(Button));
            startBtnObj.transform.SetParent(FooterArea, false);
            var startBtnRT = startBtnObj.GetComponent<RectTransform>();
            startBtnRT.anchorMin = new Vector2(0.25f, 0.2f);
            startBtnRT.anchorMax = new Vector2(0.75f, 0.8f);
            startBtnRT.offsetMin = Vector2.zero;
            startBtnRT.offsetMax = Vector2.zero;

            var startBtnImage = startBtnObj.GetComponent<Image>();
            startBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f);

            StartRunButton = startBtnObj.GetComponent<Button>();
            StartRunButton.interactable = false;
            StartRunButton.onClick.AddListener(OnStartRunPressed);

            var startBtnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            startBtnTextObj.transform.SetParent(startBtnObj.transform, false);
            var startBtnText = startBtnTextObj.GetComponent<TextMeshProUGUI>();
            startBtnText.text = "Start Run";
            startBtnText.fontSize = 28;
            startBtnText.color = Color.white;
            startBtnText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateTeamSlot(int index, bool isRequired)
        {
            var slotObj = Instantiate(TeamSlotPrefab, TeamSlotsGrid.transform, false);
            slotObj.name = $"TeamSlot_{index}";

            var slotData = slotObj.gameObject.AddComponent<DungeonTeamSlot>();
            slotData.SlotIndex = index;
            slotData.IsRequired = isRequired;
            slotData.DungeonView = this;

            _teamSlots.Add(slotData);
        }

        private void CreateResultsPanel()
        {
            _resultsPanel = Instantiate(ResultsPanelPrefab, Canvas.transform, false);
            _resultsPanel.gameObject.SetActive(false);
            _isInResultsMode = false;

            // Results content
            var resultsContent = new GameObject("ResultsContent", typeof(RectTransform));
            resultsContent.transform.SetParent(_resultsPanel, false);
            var contentRT = resultsContent.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            // Results title
            var resultsTitleObj = new GameObject("ResultsTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            resultsTitleObj.transform.SetParent(resultsContent.transform, false);
            var resultsTitleRT = resultsTitleObj.GetComponent<RectTransform>();
            resultsTitleRT.anchorMin = new Vector2(0, 0.85f);
            resultsTitleRT.anchorMax = new Vector2(1, 0.95f);
            resultsTitleRT.offsetMin = new Vector2(20, 0);
            resultsTitleRT.offsetMax = new Vector2(-20, 0);

            var resultsTitle = resultsTitleObj.GetComponent<TextMeshProUGUI>();
            resultsTitle.text = "Dungeon Complete!";
            resultsTitle.fontSize = 32;
            resultsTitle.color = SuccessColor;
            resultsTitle.alignment = TextAlignmentOptions.Center;

            // Rewards section
            var rewardsObj = new GameObject("RewardsSection", typeof(RectTransform));
            rewardsObj.transform.SetParent(resultsContent.transform, false);
            var rewardsRT = rewardsObj.GetComponent<RectTransform>();
            rewardsRT.anchorMin = new Vector2(0, 0.4f);
            rewardsRT.anchorMax = new Vector2(1, 0.8f);
            rewardsRT.offsetMin = new Vector2(40, 0);
            rewardsRT.offsetMax = new Vector2(-40, 0);

            // Gold reward
            CreateRewardLine(rewardsRT, "GoldReward", "Gold:", "0", 0.7f);
            // Exp reward
            CreateRewardLine(rewardsRT, "ExpReward", "EXP:", "0", 0.5f);
            // Stones reward
            CreateRewardLine(rewardsRT, "StonesReward", "Stones:", "0", 0.3f);

            // Morale effects section
            var moraleObj = new GameObject("MoraleSection", typeof(RectTransform), typeof(TextMeshProUGUI));
            moraleObj.transform.SetParent(resultsContent.transform, false);
            var moraleRT = moraleObj.GetComponent<RectTransform>();
            moraleRT.anchorMin = new Vector2(0, 0.15f);
            moraleRT.anchorMax = new Vector2(1, 0.35f);
            moraleRT.offsetMin = new Vector2(20, 0);
            moraleRT.offsetMax = new Vector2(-20, 0);

            var moraleText = moraleObj.GetComponent<TextMeshProUGUI>();
            moraleText.text = "Heroes took fatigue damage";
            moraleText.fontSize = 18;
            moraleText.color = new Color(1, 0.5f, 0.5f);
            moraleText.alignment = TextAlignmentOptions.Center;

            // Close button
            var closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(resultsContent.transform, false);
            var closeBtnRT = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(0.25f, 0.02f);
            closeBtnRT.anchorMax = new Vector2(0.75f, 0.12f);
            closeBtnRT.offsetMin = Vector2.zero;
            closeBtnRT.offsetMax = Vector2.zero;

            var closeBtnImage = closeBtnObj.GetComponent<Image>();
            closeBtnImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);

            CloseResultsButton = closeBtnObj.GetComponent<Button>();
            CloseResultsButton.onClick.AddListener(OnCloseResultsPressed);

            var closeBtnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);
            var closeBtnText = closeBtnTextObj.GetComponent<TextMeshProUGUI>();
            closeBtnText.text = "Continue";
            closeBtnText.fontSize = 24;
            closeBtnText.color = Color.white;
            closeBtnText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateRewardLine(RectTransform parent, string name, string label, string value, float yPosition)
        {
            var lineObj = new GameObject(name, typeof(RectTransform));
            lineObj.transform.SetParent(parent, false);
            var lineRT = lineObj.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0, yPosition);
            lineRT.anchorMax = new Vector2(1, yPosition + 0.15f);
            lineRT.offsetMin = Vector2.zero;
            lineRT.offsetMax = Vector2.zero;

            var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(lineObj.transform, false);
            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.5f, 1);
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var labelText = labelObj.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 22;
            labelText.color = Color.white;

            var valueObj = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            valueObj.transform.SetParent(lineObj.transform, false);
            var valueRT = valueObj.GetComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0.5f, 0);
            valueRT.anchorMax = new Vector2(1, 1);
            valueRT.offsetMin = Vector2.zero;
            valueRT.offsetMax = Vector2.zero;

            var valueText = valueObj.GetComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 22;
            valueText.color = Color.yellow;
            valueText.alignment = TextAlignmentOptions.Right;
        }

        private Color GetDungeonTypeColor(DungeonType type)
        {
            switch (type)
            {
                case DungeonType.Resource:
                    return new Color(0.8f, 0.6f, 0.2f, 1f); // Gold
                case DungeonType.Artifact:
                    return new Color(0.6f, 0.3f, 0.8f, 1f); // Purple
                case DungeonType.Experience:
                    return new Color(0.2f, 0.6f, 0.8f, 1f); // Blue
                default:
                    return Color.gray;
            }
        }

        private void SetupEventListeners()
        {
            if (BackButton != null)
            {
                BackButton.onClick.RemoveAllListeners();
                BackButton.onClick.AddListener(OnBackPressed);
            }

            if (StartRunButton != null)
            {
                StartRunButton.onClick.RemoveAllListeners();
                StartRunButton.onClick.AddListener(OnStartRunPressed);
            }

            if (CloseResultsButton != null)
            {
                CloseResultsButton.onClick.RemoveAllListeners();
                CloseResultsButton.onClick.AddListener(OnCloseResultsPressed);
            }
        }

        private void RefreshUI()
        {
            // Update stamina display
            int currentStamina = _dungeonService.GetStamina();
            int maxStamina = _dungeonService.GetMaxStamina();

            if (StaminaText != null)
            {
                StaminaText.text = $"{currentStamina}/{maxStamina}";
                float ratio = (float)currentStamina / maxStamina;
                StaminaText.color = ratio > 0.5f ? StaminaFullColor : (ratio > 0.2f ? StaminaLowColor : StaminaEmptyColor);
            }

            // Update dungeon cards
            var dungeons = _dungeonService.GetAllDungeons();
            for (int i = 0; i < _dungeonCards.Count && i < dungeons.Count; i++)
            {
                int progress = _dungeonService.GetDungeonProgress(dungeons[i].DungeonId);
                var cardText = _dungeonCards[i].Find("Progress")?.GetComponent<TextMeshProUGUI>();
                if (cardText != null)
                {
                    cardText.text = $"Floor: {progress}/{dungeons[i].TotalFloors}";
                }
            }
        }

        private void OnDungeonSelected(DungeonDefinition dungeon)
        {
            _selectedDungeon = dungeon;

            // Switch to team selection mode
            ShowTeamSelection(true);
            UpdateTeamSelectionUI();
        }

        private void ShowTeamSelection(bool show)
        {
            var dungeonSelection = BodyArea.Find("DungeonSelection");
            if (dungeonSelection != null)
            {
                dungeonSelection.gameObject.SetActive(!show);
            }

            var teamSelection = BodyArea.Find("TeamSelection");
            if (teamSelection != null)
            {
                teamSelection.gameObject.SetActive(show);
                _isInTeamSelectionMode = show;
            }

            if (show)
            {
                DungeonTitleText.text = $"{dungeon.DisplayName} - Team Selection";
            }
        }

        private void UpdateTeamSelectionUI()
        {
            StartRunButton.interactable = CanStartRun();

            // Update slot states
            foreach (var slot in _teamSlots)
            {
                slot.UpdateDisplay();
            }
        }

        private bool CanStartRun()
        {
            if (_selectedDungeon == null)
                return false;

            int selectedCount = 0;
            foreach (var slot in _teamSlots)
            {
                if (slot.HasHero)
                    selectedCount++;
            }

            return selectedCount >= _selectedDungeon.MinHeroCount;
        }

        private void OnStartRunPressed()
        {
            if (!CanStartRun())
                return;

            // Collect selected hero IDs
            _selectedHeroIds.Clear();
            foreach (var slot in _teamSlots)
            {
                if (slot.HasHero && !string.IsNullOrEmpty(slot.HeroInstanceId))
                {
                    _selectedHeroIds.Add(slot.HeroInstanceId);
                }
            }

            if (_selectedHeroIds.Count < _selectedDungeon.MinHeroCount)
                return;

            // Execute dungeon run
            var result = _dungeonService.RunDungeon(_selectedDungeon.DungeonId, _selectedHeroIds);

            // Show results
            ShowResults(result);
        }

        private void ShowResults(DungeonRunResult result)
        {
            _isInResultsMode = true;
            _resultsPanel.gameObject.SetActive(true);

            // Update results content
            var resultsContent = _resultsPanel.Find("ResultsContent");
            if (resultsContent != null)
            {
                var title = resultsContent.Find("ResultsTitle")?.GetComponent<TextMeshProUGUI>();
                if (title != null)
                {
                    title.text = result.Success ? "Victory!" : "Defeat";
                    title.color = result.Success ? SuccessColor : FailureColor;
                }

                var goldReward = resultsContent.Find("GoldReward/Value")?.GetComponent<TextMeshProUGUI>();
                if (goldReward != null)
                    goldReward.text = $"+{result.GoldEarned}";

                var expReward = resultsContent.Find("ExpReward/Value")?.GetComponent<TextMeshProUGUI>();
                if (expReward != null)
                    expReward.text = $"+{result.ExpEarned}";

                var stonesReward = resultsContent.Find("StonesReward/Value")?.GetComponent<TextMeshProUGUI>();
                if (stonesReward != null)
                    stonesReward.text = $"+{result.StonesEarned}";

                var moraleText = resultsContent.Find("MoraleSection")?.GetComponent<TextMeshProUGUI>();
                if (moraleText != null && result.MoraleEffects != null && result.MoraleEffects.Count > 0)
                {
                    moraleText.text = $"Heroes took fatigue damage ({result.MoraleEffects.Count} heroes affected)";
                }
            }
        }

        private void OnCloseResultsPressed()
        {
            _isInResultsMode = false;
            _resultsPanel.gameObject.SetActive(false);

            // Return to dungeon selection
            ShowTeamSelection(false);
            DungeonTitleText.text = "Dungeons";

            // Clear selections
            foreach (var slot in _teamSlots)
            {
                slot.ClearHero();
            }
            _selectedHeroIds.Clear();

            RefreshUI();
        }

        private void OnBackPressed()
        {
            if (_isInResultsMode)
            {
                OnCloseResultsPressed();
                return;
            }

            if (_isInTeamSelectionMode)
            {
                ShowTeamSelection(false);
                DungeonTitleText.text = "Dungeons";
                return;
            }

            // Navigate back to Hub
            _gameStateService.LoadScene("Hub");
        }

        public void OnSlotClicked(DungeonTeamSlot slot)
        {
            if (slot.HasHero)
            {
                // Show hero details or remove
                slot.ClearHero();
            }
            else
            {
                // Open hero selection
                ShowHeroPicker(slot);
            }
            UpdateTeamSelectionUI();
        }

        private void ShowHeroPicker(DungeonTeamSlot targetSlot)
        {
            // Implementation would show a hero selection modal
            // For now, we'll use a simplified approach
            var availableHeroes = _rosterService.GetAllHeroes();

            foreach (var hero in availableHeroes)
            {
                bool alreadySelected = false;
                foreach (var slot in _teamSlots)
                {
                    if (slot.HeroInstanceId == hero.InstanceId)
                    {
                        alreadySelected = true;
                        break;
                    }
                }

                if (!alreadySelected)
                {
                    targetSlot.SetHero(hero.InstanceId);
                    break;
                }
            }

            UpdateTeamSelectionUI();
        }

        private void OnEnable()
        {
            RefreshUI();
            _dungeonService.ProcessStaminaRegeneration();
        }

        private void Update()
        {
            // Periodically update stamina display
            if (Time.frameCount % 60 == 0)
            {
                _dungeonService.ProcessStaminaRegeneration();
                RefreshUI();
            }
        }
    }

    /// <summary>
    /// Component attached to dungeon card for easy data access.
    /// </summary>
    public class DungeonCardData : MonoBehaviour
    {
        public DungeonDefinition Dungeon;
    }
}