using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.Core;

namespace PickMeUp.Game.UI
{
    /// <summary>
    /// UI controller for the Roster scene.
    /// Shows hero grid with filter/sort, detail panel, and navigation to synthesis.
    /// </summary>
    public class RosterView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform HeaderArea;
        public RectTransform FilterArea;
        public RectTransform GridArea;
        public RectTransform DetailPanelArea;

        [Header("UI Elements")]
        public Button BackButton;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI HeroCountText;

        [Header("Filter Tabs")]
        public Button AllTabButton;
        public Button WarriorTabButton;
        public Button MageTabButton;
        public Button SupportTabButton;
        public Button TankTabButton;

        [Header("Action Buttons")]
        public Button PromoteButton;
        public Button SynthesizeButton; // Now leads to dedicated Synth scene

        [Header("Colors")]
        public Color TabActiveColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        public Color TabInactiveColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        public Color SelectedCardColor = new Color(0.4f, 0.7f, 1f, 1f);
        public Color RarityCommonColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        public Color RarityRareColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        public Color RarityEpicColor = new Color(0.7f, 0.3f, 0.9f, 1f);
        public Color RarityLegendaryColor = new Color(1f, 0.6f, 0.1f, 1f);

        // Services
        private IRosterService _rosterService;
        private ISynthesizerService _synthesizerService;
        private IGameStateService _gameStateService;

        // State
        private List<HeroInstance> _allHeroes;
        private List<HeroInstance> _displayedHeroes;
        private HeroInstance _selectedHero;
        private string _currentFilter = "All";

        // UI State
        private List<RectTransform> _heroCards = new List<RectTransform>();
        private RectTransform _detailPanel;
        private GridLayoutGroup _gridLayout;

        public void Initialize(
            IRosterService rosterService,
            ISynthesizerService synthesizerService,
            IGameStateService gameStateService)
        {
            _rosterService = rosterService;
            _synthesizerService = synthesizerService;
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

            // Create filter tabs
            CreateFilterTabs();

            // Create hero grid
            CreateHeroGrid();

            // Create detail panel (hidden initially)
            CreateDetailPanel();
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

            _heroCards.Clear();
            _selectedHero = null;
        }

        private void CreateHeader()
        {
            var headerContainer = new GameObject("HeaderContainer", typeof(RectTransform));
            headerContainer.transform.SetParent(HeaderArea, false);

            var headerRT = headerContainer.GetComponent<RectTransform>();
            headerRT.anchorMin = Vector2.zero;
            headerRT.anchorMax = Vector2.one;
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            // Background
            var bgObj = new GameObject("HeaderBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(headerRT, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

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
            backText.color = Color.white;

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(headerRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.25f, 0.5f);
            titleRT.anchorMax = new Vector2(0.55f, 0.5f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            titleRT.sizeDelta = new Vector2(0, 50);

            TitleText = titleObj.GetComponent<TextMeshProUGUI>();
            TitleText.text = "Hero Roster";
            TitleText.fontSize = 32;
            TitleText.color = Color.white;

            // Hero count
            var countObj = new GameObject("HeroCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            countObj.transform.SetParent(headerRT, false);
            var countRT = countObj.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0.6f, 0.5f);
            countRT.anchorMax = new Vector2(1, 0.5f);
            countRT.pivot = new Vector2(1, 0.5f);
            countRT.anchoredPosition = new Vector2(-20, 0);
            countRT.sizeDelta = new Vector2(200, 40);

            HeroCountText = countObj.GetComponent<TextMeshProUGUI>();
            HeroCountText.fontSize = 20;
            HeroCountText.color = Color.gray;
            HeroCountText.alignment = TextAlignmentOptions.Right;
        }

        private void CreateFilterTabs()
        {
            var filterContainer = new GameObject("FilterTabs", typeof(RectTransform));
            filterContainer.transform.SetParent(FilterArea, false);

            var filterRT = filterContainer.GetComponent<RectTransform>();
            filterRT.anchorMin = Vector2.zero;
            filterRT.anchorMax = Vector2.one;
            filterRT.offsetMin = Vector2.zero;
            filterRT.offsetMax = Vector2.zero;

            string[] tabNames = { "All", "Warrior", "Mage", "Support", "Tank" };
            Color[] tabColors = { TabActiveColor, new Color(0.9f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.9f),
                                  new Color(0.3f, 0.9f, 0.5f), new Color(0.8f, 0.6f, 0.2f) };

            for (int i = 0; i < tabNames.Length; i++)
            {
                var tabObj = new GameObject($"Tab_{tabNames[i]}", typeof(RectTransform), typeof(Image), typeof(Button));
                tabObj.transform.SetParent(filterRT, false);
                var tabRT = tabObj.GetComponent<RectTransform>();
                tabRT.anchorMin = new Vector2(i * 0.2f, 0);
                tabRT.anchorMax = new Vector2((i + 1) * 0.2f, 1);
                tabRT.offsetMin = Vector2.zero;
                tabRT.offsetMax = Vector2.zero;

                var tabImage = tabObj.GetComponent<Image>();
                tabImage.color = i == 0 ? TabActiveColor : TabInactiveColor;

                var button = tabObj.GetComponent<Button>();
                int capturedIndex = i;
                string capturedName = tabNames[i];
                button.onClick.AddListener(() => OnFilterClicked(capturedIndex, capturedName));

                var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(tabObj.transform, false);
                var textRT = textObj.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = new Vector2(5, 5);
                textRT.offsetMax = new Vector2(-5, -5);

                var text = textObj.GetComponent<TextMeshProUGUI>();
                text.text = tabNames[i];
                text.fontSize = 16;
                text.color = i == 0 ? Color.white : new Color(1, 1, 1, 0.7f);
                text.alignment = TextAlignmentOptions.Center;
            }
        }

        private void CreateHeroGrid()
        {
            var gridContainer = new GameObject("HeroGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridContainer.transform.SetParent(GridArea, false);

            var gridRT = gridContainer.GetComponent<RectTransform>();
            gridRT.anchorMin = Vector2.zero;
            gridRT.anchorMax = Vector2.one;
            gridRT.offsetMin = Vector2.zero;
            gridRT.offsetMax = Vector2.zero;

            _gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            _gridLayout.cellSize = new Vector2(160, 220);
            _gridLayout.spacing = new Vector2(15, 15);
            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.constraintCount = 4;
            _gridLayout.padding = new RectOffset(20, 20, 20, 20);

            RefreshHeroGrid();
        }

        private void RefreshHeroGrid()
        {
            // Clear existing cards
            foreach (var card in _heroCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _heroCards.Clear();

            // Get all heroes and apply filter
            _allHeroes = _rosterService.GetAllHeroes();

            _displayedHeroes = _allHeroes;
            if (_currentFilter != "All")
            {
                _displayedHeroes = _allHeroes.FindAll(h => h.HeroName.Contains(_currentFilter));
            }

            // Create cards
            foreach (var hero in _displayedHeroes)
            {
                var card = CreateHeroCard(hero);
                _heroCards.Add(card);
            }

            // Update count
            HeroCountText.text = $"{_displayedHeroes.Count}/{_allHeroes.Count} Heroes";
        }

        private RectTransform CreateHeroCard(HeroInstance hero)
        {
            var cardObj = new GameObject($"HeroCard_{hero.InstanceId}", typeof(RectTransform));
            var cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.SetParent(_gridLayout.transform, false);
            cardRT.sizeDelta = new Vector2(160, 220);

            bool isSelected = _selectedHero != null && _selectedHero.InstanceId == hero.InstanceId;

            // Card background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(cardRT, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = isSelected ? SelectedCardColor : new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // Rarity border
            var borderObj = new GameObject("RarityBorder", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(cardRT, false);
            var borderRT = borderObj.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(4, 4);
            borderRT.offsetMax = new Vector2(-4, -4);

            var borderImage = borderObj.GetComponent<Image>();
            borderImage.color = GetRarityColor(hero.StarRank);
            borderImage.fillCenter = false;

            // Star rank
            var starsObj = new GameObject("Stars", typeof(RectTransform), typeof(TextMeshProUGUI));
            starsObj.transform.SetParent(cardRT, false);
            var starsRT = starsObj.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(0.05f, 0.88f);
            starsRT.anchorMax = new Vector2(0.95f, 0.97f);
            starsRT.offsetMin = Vector2.zero;
            starsRT.offsetMax = Vector2.zero;

            var starsText = starsObj.GetComponent<TextMeshProUGUI>();
            starsText.text = new string('★', hero.StarRank);
            starsText.fontSize = 14;
            starsText.color = GetRarityColor(hero.StarRank);
            starsText.alignment = TextAlignmentOptions.Center;

            // Hero portrait placeholder
            var portraitObj = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitObj.transform.SetParent(cardRT, false);
            var portraitRT = portraitObj.GetComponent<RectTransform>();
            portraitRT.anchorMin = new Vector2(0.1f, 0.3f);
            portraitRT.anchorMax = new Vector2(0.9f, 0.85f);
            portraitRT.offsetMin = Vector2.zero;
            portraitRT.offsetMax = Vector2.zero;

            var portraitImage = portraitObj.GetComponent<Image>();
            portraitImage.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);

            // Hero name
            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(cardRT, false);
            var nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.08f);
            nameRT.anchorMax = new Vector2(0.95f, 0.25f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            var nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = hero.HeroName;
            nameText.fontSize = 13;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;

            // Level
            var levelObj = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(cardRT, false);
            var levelRT = levelObj.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0.05f, 0.02f);
            levelRT.anchorMax = new Vector2(0.5f, 0.07f);
            levelRT.offsetMin = Vector2.zero;
            levelRT.offsetMax = Vector2.zero;

            var levelText = levelObj.GetComponent<TextMeshProUGUI>();
            levelText.text = $"Lv.{hero.Level}";
            levelText.fontSize = 12;
            levelText.color = Color.yellow;

            // Morale indicator
            var moraleObj = new GameObject("Morale", typeof(RectTransform), typeof(TextMeshProUGUI));
            moraleObj.transform.SetParent(cardRT, false);
            var moraleRT = moraleObj.GetComponent<RectTransform>();
            moraleRT.anchorMin = new Vector2(0.5f, 0.02f);
            moraleRT.anchorMax = new Vector2(0.95f, 0.07f);
            moraleRT.offsetMin = Vector2.zero;
            moraleRT.offsetMax = Vector2.zero;

            var moraleText = moraleObj.GetComponent<TextMeshProUGUI>();
            moraleText.text = hero.CurrentMorale < 50 ? $"⚠{hero.CurrentMorale}" : $"{hero.CurrentMorale}";
            moraleText.fontSize = 11;
            moraleText.color = hero.CurrentMorale < 50 ? Color.red : Color.green;
            moraleText.alignment = TextAlignmentOptions.Right;

            // Click handler
            var button = cardObj.AddComponent<Button>();
            var heroCopy = hero; // Capture for closure
            button.onClick.AddListener(() => OnHeroClicked(heroCopy));

            return cardRT;
        }

        private Color GetRarityColor(int starRank)
        {
            switch (starRank)
            {
                case 1: case 2: return RarityCommonColor;
                case 3: case 4: return RarityRareColor;
                case 5: return RarityEpicColor;
                default: return starRank >= 6 ? RarityLegendaryColor : RarityCommonColor;
            }
        }

        private void CreateDetailPanel()
        {
            _detailPanel = new GameObject("DetailPanel", typeof(RectTransform)).GetComponent<RectTransform>();
            _detailPanel.transform.SetParent(Canvas.transform, false);
            _detailPanel.anchorMin = new Vector2(0.05f, 0.1f);
            _detailPanel.anchorMax = new Vector2(0.95f, 0.9f);
            _detailPanel.offsetMin = Vector2.zero;
            _detailPanel.offsetMax = Vector2.zero;
            _detailPanel.gameObject.SetActive(false);

            // Background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(_detailPanel, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);
        }

        private void SetupEventListeners()
        {
            if (BackButton != null)
            {
                BackButton.onClick.RemoveAllListeners();
                BackButton.onClick.AddListener(OnBackPressed);
            }
        }

        private void RefreshUI()
        {
            RefreshHeroGrid();
        }

        private void OnFilterClicked(int index, string filterName)
        {
            _currentFilter = filterName;

            // Update tab colors
            var filterContainer = FilterArea.Find("FilterTabs");
            if (filterContainer != null)
            {
                for (int i = 0; i < filterContainer.childCount; i++)
                {
                    var tab = filterContainer.GetChild(i);
                    var image = tab.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = i == index ? TabActiveColor : TabInactiveColor;
                    }
                }
            }

            RefreshHeroGrid();
        }

        private void OnHeroClicked(HeroInstance hero)
        {
            _selectedHero = hero;
            ShowHeroDetail(hero);
        }

        private void ShowHeroDetail(HeroInstance hero)
        {
            _detailPanel.gameObject.SetActive(true);

            // Clear existing content
            foreach (Transform child in _detailPanel)
            {
                if (child.name != "Background")
                    Destroy(child.gameObject);
            }

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(_detailPanel, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.92f);
            titleRT.anchorMax = new Vector2(1, 0.98f);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = hero.HeroName;
            titleText.fontSize = 32;
            titleText.color = GetRarityColor(hero.StarRank);

            // Level and stars
            var levelObj = new GameObject("LevelStars", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(_detailPanel, false);
            var levelRT = levelObj.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0, 0.88f);
            levelRT.anchorMax = new Vector2(1, 0.92f);
            levelRT.offsetMin = new Vector2(20, 0);
            levelRT.offsetMax = new Vector2(-20, 0);

            var levelText = levelObj.GetComponent<TextMeshProUGUI>();
            levelText.text = $"Level {hero.Level} - {new string('★', hero.StarRank)}";
            levelText.fontSize = 22;
            levelText.color = Color.yellow;

            // Stats
            var statsObj = new GameObject("Stats", typeof(RectTransform), typeof(TextMeshProUGUI));
            statsObj.transform.SetParent(_detailPanel, false);
            var statsRT = statsObj.GetComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0, 0.6f);
            statsRT.anchorMax = new Vector2(1, 0.85f);
            statsRT.offsetMin = new Vector2(20, 0);
            statsRT.offsetMax = new Vector2(-20, 0);

            var statsText = statsObj.GetComponent<TextMeshProUGUI>();
            statsText.text = $"ATK: {hero.Attack}  DEF: {hero.Defense}\n" +
                           $"HP: {hero.CurrentHealth}/{hero.MaxHealth}\n" +
                           $"SPD: {hero.Speed}  CRT: {hero.CritRate * 100:F0}%";
            statsText.fontSize = 18;
            statsText.color = Color.cyan;

            // Morale
            var moraleObj = new GameObject("Morale", typeof(RectTransform), typeof(TextMeshProUGUI));
            moraleObj.transform.SetParent(_detailPanel, false);
            var moraleRT = moraleObj.GetComponent<RectTransform>();
            moraleRT.anchorMin = new Vector2(0, 0.5f);
            moraleRT.anchorMax = new Vector2(1, 0.58f);
            moraleRT.offsetMin = new Vector2(20, 0);
            moraleRT.offsetMax = new Vector2(-20, 0);

            var moraleText = moraleObj.GetComponent<TextMeshProUGUI>();
            moraleText.text = $"Morale: {hero.CurrentMorale}/100";
            moraleText.fontSize = 18;
            moraleText.color = hero.CurrentMorale < 50 ? Color.red : Color.green;

            // Close button
            var closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(_detailPanel, false);
            var closeBtnRT = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(0.35f, 0.02f);
            closeBtnRT.anchorMax = new Vector2(0.65f, 0.08f);
            closeBtnRT.offsetMin = Vector2.zero;
            closeBtnRT.offsetMax = Vector2.zero;

            var closeBtnImage = closeBtnObj.GetComponent<Image>();
            closeBtnImage.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);

            var closeBtn = closeBtnObj.GetComponent<Button>();
            closeBtn.onClick.AddListener(HideDetailPanel);

            var closeBtnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);
            var closeBtnTextRT = closeBtnTextObj.GetComponent<RectTransform>();
            closeBtnTextRT.anchorMin = Vector2.zero;
            closeBtnTextRT.anchorMax = Vector2.one;
            closeBtnTextRT.offsetMin = new Vector2(5, 5);
            closeBtnTextRT.offsetMax = new Vector2(-5, -5);

            var closeBtnText = closeBtnTextObj.GetComponent<TextMeshProUGUI>();
            closeBtnText.text = "Close";
            closeBtnText.fontSize = 22;
            closeBtnText.color = Color.white;
            closeBtnText.alignment = TextAlignmentOptions.Center;

            // Refresh grid to update selection highlight
            RefreshHeroGrid();
        }

        private void HideDetailPanel()
        {
            _detailPanel?.gameObject.SetActive(false);
            _selectedHero = null;
            RefreshHeroGrid();
        }

        private void OnBackPressed()
        {
            if (_detailPanel != null && _detailPanel.gameObject.activeSelf)
            {
                HideDetailPanel();
            }
            else
            {
                _gameStateService.LoadScene("Hub");
            }
        }

        private void OnEnable()
        {
            RefreshUI();
        }
    }
}