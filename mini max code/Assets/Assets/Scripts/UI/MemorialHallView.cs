using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.Core;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.UI
{
    /// <summary>
    /// Main UI controller for the Memorial Hall (Collection Gallery).
    /// Shows all discovered heroes with lore and stats.
    /// </summary>
    public class MemorialHallView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform HeaderArea;
        public RectTransform FilterSortArea;
        public RectTransform GridArea;
        public RectTransform DetailPanelArea;

        [Header("UI Elements")]
        public Button BackButton;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI CompletionText;
        public TextMeshProUGUI FilterStatusText;

        [Header("Filter Tabs")]
        public Button AllHeroesButton;
        public Button DiscoveredButton;
        public Button UndiscoveredButton;

        [Header("Sort Options")]
        public Button SortByAcquisitionButton;
        public Button SortByNameButton;
        public Button SortByClassButton;
        public Button SortByRankButton;

        [Header("Prefabs")]
        public RectTransform HeroCardPrefab;
        public RectTransform DetailPanelPrefab;

        [Header("Colors")]
        public Color DiscoveredCardColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        public Color UndiscoveredCardColor = new Color(0.05f, 0.05f, 0.08f, 0.6f);
        public Color SelectedCardColor = new Color(0.4f, 0.7f, 1f, 1f);
        public Color RarityCommonColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        public Color RarityRareColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        public Color RarityEpicColor = new Color(0.7f, 0.3f, 0.9f, 1f);
        public Color RarityLegendaryColor = new Color(1f, 0.6f, 0.1f, 1f);

        // Services
        private IRosterService _rosterService;
        private IGachaService _gachaService;
        private IGameStateService _gameStateService;
        private ISaveLoadService _saveLoadService;

        // State
        private List<HeroDefinition> _allHeroDefinitions;
        private HashSet<string> _discoveredHeroIds;
        private Dictionary<string, int> _acquisitionCounts;

        private string _selectedHeroId;
        private HeroDefinition _selectedHeroDef;

        // Filter and sort state
        private enum FilterMode { All, Discovered, Undiscovered }
        private enum SortMode { Acquisition, Name, Class, Rank }

        private FilterMode _currentFilter = FilterMode.All;
        private SortMode _currentSort = SortMode.Acquisition;

        // UI State
        private RectTransform _detailPanel;
        private List<RectTransform> _heroCards = new List<RectTransform>();
        private GridLayoutGroup _gridLayout;

        public void Initialize(
            IRosterService rosterService,
            IGachaService gachaService,
            IGameStateService gameStateService,
            ISaveLoadService saveLoadService)
        {
            _rosterService = rosterService;
            _gachaService = gachaService;
            _gameStateService = gameStateService;
            _saveLoadService = saveLoadService;

            LoadHeroData();
            SetupUI();
            SetupEventListeners();
            RefreshUI();
        }

        private void LoadHeroData()
        {
            // Load all hero definitions
            _allHeroDefinitions = new List<HeroDefinition>(Resources.LoadAll<HeroDefinition>("ScriptableObjects/Heroes"));

            // Load discovered heroes from save
            var saveData = _saveLoadService.LoadGame();
            _discoveredHeroIds = saveData.DiscoveredHeroIds ?? new HashSet<string>();

            // Calculate acquisition counts
            _acquisitionCounts = new Dictionary<string, int>();
            var allHeroes = _rosterService.GetAllHeroes();
            foreach (var hero in allHeroes)
            {
                if (_acquisitionCounts.ContainsKey(hero.HeroId))
                {
                    _acquisitionCounts[hero.HeroId]++;
                }
                else
                {
                    _acquisitionCounts[hero.HeroId] = 1;
                }
            }
        }

        private void SetupUI()
        {
            ClearExistingUI();

            // Create header
            CreateHeader();

            // Create filter and sort area
            CreateFilterSortArea();

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
            _selectedHeroId = null;
            _selectedHeroDef = null;
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
            TitleText.text = "Memorial Hall";
            TitleText.fontSize = 32;
            TitleText.color = Color.white;

            // Completion percentage
            var completionObj = new GameObject("Completion", typeof(RectTransform), typeof(TextMeshProUGUI));
            completionObj.transform.SetParent(headerRT, false);
            var completionRT = completionObj.GetComponent<RectTransform>();
            completionRT.anchorMin = new Vector2(0.6f, 0.5f);
            completionRT.anchorMax = new Vector2(1, 0.5f);
            completionRT.pivot = new Vector2(1, 0.5f);
            completionRT.anchoredPosition = new Vector2(-20, 0);
            completionRT.sizeDelta = new Vector2(250, 50);

            CompletionText = completionObj.GetComponent<TextMeshProUGUI>();
            CompletionText.fontSize = 22;
            CompletionText.color = Color.yellow;
            CompletionText.alignment = TextAlignmentOptions.Right;
        }

        private void CreateFilterSortArea()
        {
            var filterContainer = new GameObject("FilterSortContainer", typeof(RectTransform));
            filterContainer.transform.SetParent(FilterSortArea, false);

            var filterRT = filterContainer.GetComponent<RectTransform>();
            filterRT.anchorMin = Vector2.zero;
            filterRT.anchorMax = Vector2.one;
            filterRT.offsetMin = Vector2.zero;
            filterRT.offsetMax = Vector2.zero;

            // Filter section (left)
            CreateFilterSection(filterRT, 0, 0.5f);

            // Sort section (right)
            CreateSortSection(filterRT, 0.5f, 1f);
        }

        private void CreateFilterSection(RectTransform parent, float xMin, float xMax)
        {
            var filterSection = new GameObject("FilterSection", typeof(RectTransform));
            filterSection.transform.SetParent(parent, false);
            var filterRT = filterSection.GetComponent<RectTransform>();
            filterRT.anchorMin = new Vector2(xMin, 0);
            filterRT.anchorMax = new Vector2(xMax, 1);
            filterRT.offsetMin = Vector2.zero;
            filterRT.offsetMax = Vector2.zero;

            string[] filterNames = { "All", "Found", "Hidden" };

            for (int i = 0; i < filterNames.Length; i++)
            {
                var filterObj = new GameObject($"Filter_{filterNames[i]}", typeof(RectTransform), typeof(Button));
                filterObj.transform.SetParent(filterRT, false);
                var filterBtnRT = filterObj.GetComponent<RectTransform>();
                filterBtnRT.anchorMin = new Vector2(i * 0.33f, 0.1f);
                filterBtnRT.anchorMax = new Vector2((i + 1) * 0.33f, 0.9f);
                filterBtnRT.offsetMin = Vector2.zero;
                filterBtnRT.offsetMax = Vector2.zero;

                var filterBtn = filterObj.GetComponent<Button>();
                int capturedIndex = i;
                filterBtn.onClick.AddListener(() => OnFilterClicked(capturedIndex));

                var filterBg = filterObj.AddComponent<Image>();
                filterBg.color = i == 0 ? new Color(0.3f, 0.5f, 0.9f, 0.8f) : new Color(0.2f, 0.2f, 0.3f, 0.6f);

                var filterTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                filterTextObj.transform.SetParent(filterObj.transform, false);
                var filterTextRT = filterTextObj.GetComponent<RectTransform>();
                filterTextRT.anchorMin = Vector2.zero;
                filterTextRT.anchorMax = Vector2.one;
                filterTextRT.offsetMin = new Vector2(5, 3);
                filterTextRT.offsetMax = new Vector2(-5, -3);

                var filterText = filterTextObj.GetComponent<TextMeshProUGUI>();
                filterText.text = filterNames[i];
                filterText.fontSize = 14;
                filterText.color = Color.white;
                filterText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void CreateSortSection(RectTransform parent, float xMin, float xMax)
        {
            var sortSection = new GameObject("SortSection", typeof(RectTransform));
            sortSection.transform.SetParent(parent, false);
            var sortRT = sortSection.GetComponent<RectTransform>();
            sortRT.anchorMin = new Vector2(xMin, 0);
            sortRT.anchorMax = new Vector2(xMax, 1);
            sortRT.offsetMin = Vector2.zero;
            sortRT.offsetMax = Vector2.zero;

            // Title
            var sortTitleObj = new GameObject("SortTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            sortTitleObj.transform.SetParent(sortRT, false);
            var sortTitleRT = sortTitleObj.GetComponent<RectTransform>();
            sortTitleRT.anchorMin = new Vector2(0, 0.5f);
            sortTitleRT.anchorMax = new Vector2(0.25f, 1);
            sortTitleRT.offsetMin = Vector2.zero;
            sortTitleRT.offsetMax = Vector2.zero;

            var sortTitle = sortTitleObj.GetComponent<TextMeshProUGUI>();
            sortTitle.text = "Sort:";
            sortTitle.fontSize = 14;
            sortTitle.color = Color.gray;

            string[] sortNames = { "Acq", "Name", "Class", "Rank" };
            SortMode[] sortModes = { SortMode.Acquisition, SortMode.Name, SortMode.Class, SortMode.Rank };

            for (int i = 0; i < sortNames.Length; i++)
            {
                var sortObj = new GameObject($"Sort_{sortNames[i]}", typeof(RectTransform), typeof(Button));
                sortObj.transform.SetParent(sortRT, false);
                var sortBtnRT = sortObj.GetComponent<RectTransform>();
                sortBtnRT.anchorMin = new Vector2(0.25f + i * 0.18f, 0.2f);
                sortBtnRT.anchorMax = new Vector2(0.4f + i * 0.18f, 0.8f);
                sortBtnRT.offsetMin = Vector2.zero;
                sortBtnRT.offsetMax = Vector2.zero;

                var sortBtn = sortObj.GetComponent<Button>();
                int capturedIndex = i;
                sortBtn.onClick.AddListener(() => OnSortClicked(capturedIndex));

                var sortBg = sortObj.AddComponent<Image>();
                sortBg.color = i == 0 ? new Color(0.3f, 0.5f, 0.9f, 0.6f) : new Color(0.15f, 0.15f, 0.2f, 0.5f);

                var sortTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                sortTextObj.transform.SetParent(sortObj.transform, false);
                var sortTextRT = sortTextObj.GetComponent<RectTransform>();
                sortTextRT.anchorMin = Vector2.zero;
                sortTextRT.anchorMax = Vector2.one;
                sortTextRT.offsetMin = new Vector2(3, 2);
                sortTextRT.offsetMax = new Vector2(-3, -2);

                var sortText = sortTextObj.GetComponent<TextMeshProUGUI>();
                sortText.text = sortNames[i];
                sortText.fontSize = 12;
                sortText.color = Color.white;
                sortText.alignment = TextAlignmentOptions.Center;
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

            // Get filtered and sorted heroes
            var displayHeroes = GetFilteredSortedHeroes();

            // Create cards
            foreach (var heroDef in displayHeroes)
            {
                var card = CreateHeroCard(heroDef);
                _heroCards.Add(card);
            }

            // Update completion text
            int discovered = _discoveredHeroIds.Count;
            int total = _allHeroDefinitions.Count;
            float percentage = total > 0 ? (float)discovered / total * 100f : 0f;
            CompletionText.text = $"{discovered}/{total} ({percentage:F1}%)";
        }

        private List<HeroDefinition> GetFilteredSortedHeroes()
        {
            var result = new List<HeroDefinition>();

            // Apply filter
            foreach (var heroDef in _allHeroDefinitions)
            {
                bool isDiscovered = _discoveredHeroIds.Contains(heroDef.HeroId);

                switch (_currentFilter)
                {
                    case FilterMode.Discovered:
                        if (!isDiscovered) continue;
                        break;
                    case FilterMode.Undiscovered:
                        if (isDiscovered) continue;
                        break;
                }

                result.Add(heroDef);
            }

            // Apply sort
            switch (_currentSort)
            {
                case SortMode.Name:
                    result.Sort((a, b) => string.Compare(a.HeroName, b.HeroName, StringComparison.Ordinal));
                    break;

                case SortMode.Class:
                    result.Sort((a, b) =>
                    {
                        int classCompare = string.Compare(a.HeroClass, b.HeroClass, StringComparison.Ordinal);
                        if (classCompare != 0) return classCompare;
                        return b.StarRank.CompareTo(a.StarRank); // Higher stars first
                    });
                    break;

                case SortMode.Rank:
                    result.Sort((a, b) => b.StarRank.CompareTo(a.StarRank)); // Higher stars first
                    break;

                case SortMode.Acquisition:
                default:
                    // Sort by acquisition order (discovered first, then by ID)
                    result.Sort((a, b) =>
                    {
                        bool aDiscovered = _discoveredHeroIds.Contains(a.HeroId);
                        bool bDiscovered = _discoveredHeroIds.Contains(b.HeroId);
                        if (aDiscovered && !bDiscovered) return -1;
                        if (!aDiscovered && bDiscovered) return 1;
                        return string.Compare(a.HeroId, b.HeroId, StringComparison.Ordinal);
                    });
                    break;
            }

            return result;
        }

        private RectTransform CreateHeroCard(HeroDefinition heroDef)
        {
            var cardObj = Instantiate(HeroCardPrefab, _gridLayout.transform, false);
            cardObj.name = $"HeroCard_{heroDef.HeroId}";

            bool isDiscovered = _discoveredHeroIds.Contains(heroDef.HeroId);
            bool isSelected = _selectedHeroId == heroDef.HeroId;

            // Card background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(cardObj, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = isSelected ? SelectedCardColor :
                           (isDiscovered ? DiscoveredCardColor : UndiscoveredCardColor);

            // Rarity border
            var borderObj = new GameObject("RarityBorder", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(cardObj, false);
            var borderRT = borderObj.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(4, 4);
            borderRT.offsetMax = new Vector2(-4, -4);

            var borderImage = borderObj.GetComponent<Image>();
            borderImage.color = GetRarityColor(heroDef.StarRank);
            borderImage.fillCenter = false;

            // Star rank display
            var starsObj = new GameObject("StarRank", typeof(RectTransform), typeof(TextMeshProUGUI));
            starsObj.transform.SetParent(cardObj, false);
            var starsRT = starsObj.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(0.05f, 0.88f);
            starsRT.anchorMax = new Vector2(0.95f, 0.97f);
            starsRT.offsetMin = Vector2.zero;
            starsRT.offsetMax = Vector2.zero;

            var starsText = starsObj.GetComponent<TextMeshProUGUI>();
            starsText.text = new string('★', heroDef.StarRank);
            starsText.fontSize = 14;
            starsText.color = GetRarityColor(heroDef.StarRank);
            starsText.alignment = TextAlignmentOptions.Center;

            if (isDiscovered)
            {
                // Hero portrait
                var portraitObj = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
                portraitObj.transform.SetParent(cardObj, false);
                var portraitRT = portraitObj.GetComponent<RectTransform>();
                portraitRT.anchorMin = new Vector2(0.1f, 0.3f);
                portraitRT.anchorMax = new Vector2(0.9f, 0.85f);
                portraitRT.offsetMin = Vector2.zero;
                portraitRT.offsetMax = Vector2.zero;

                var portraitImage = portraitObj.GetComponent<Image>();
                if (heroDef.Portrait != null)
                {
                    portraitImage.sprite = heroDef.Portrait;
                }
                else
                {
                    portraitImage.color = GetClassColor(heroDef.HeroClass);
                }
                portraitImage.preserveAspect = true;

                // Hero name
                var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(cardObj, false);
                var nameRT = nameObj.GetComponent<RectTransform>();
                nameRT.anchorMin = new Vector2(0.05f, 0.08f);
                nameRT.anchorMax = new Vector2(0.95f, 0.28f);
                nameRT.offsetMin = Vector2.zero;
                nameRT.offsetMax = Vector2.zero;

                var nameText = nameObj.GetComponent<TextMeshProUGUI>();
                nameText.text = heroDef.HeroName;
                nameText.fontSize = 13;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.enableWordWrapping = true;

                // Acquisition count
                if (_acquisitionCounts.ContainsKey(heroDef.HeroId))
                {
                    var countObj = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
                    countObj.transform.SetParent(cardObj, false);
                    var countRT = countObj.GetComponent<RectTransform>();
                    countRT.anchorMin = new Vector2(0.7f, 0.02f);
                    countRT.anchorMax = new Vector2(0.95f, 0.07f);
                    countRT.offsetMin = Vector2.zero;
                    countRT.offsetMax = Vector2.zero;

                    var countText = countObj.GetComponent<TextMeshProUGUI>();
                    countText.text = $"x{_acquisitionCounts[heroDef.HeroId]}";
                    countText.fontSize = 11;
                    countText.color = Color.green;
                    countText.alignment = TextAlignmentOptions.Right;
                }
            }
            else
            {
                // Undiscovered silhouette
                var silhouetteObj = new GameObject("Silhouette", typeof(RectTransform), typeof(Image));
                silhouetteObj.transform.SetParent(cardObj, false);
                var silhouetteRT = silhouetteObj.GetComponent<RectTransform>();
                silhouetteRT.anchorMin = new Vector2(0.1f, 0.3f);
                silhouetteRT.anchorMax = new Vector2(0.9f, 0.85f);
                silhouetteRT.offsetMin = Vector2.zero;
                silhouetteRT.offsetMax = Vector2.zero;

                var silhouetteImage = silhouetteObj.GetComponent<Image>();
                silhouetteImage.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

                // Question marks
                var unknownObj = new GameObject("Unknown", typeof(RectTransform), typeof(TextMeshProUGUI));
                unknownObj.transform.SetParent(cardObj, false);
                var unknownRT = unknownObj.GetComponent<RectTransform>();
                unknownRT.anchorMin = new Vector2(0.1f, 0.3f);
                unknownRT.anchorMax = new Vector2(0.9f, 0.85f);
                unknownRT.offsetMin = Vector2.zero;
                unknownRT.offsetMax = Vector2.zero;

                var unknownText = unknownObj.GetComponent<TextMeshProUGUI>();
                unknownText.text = "???";
                unknownText.fontSize = 36;
                unknownText.color = new Color(0.4f, 0.4f, 0.5f, 0.8f);
                unknownText.alignment = TextAlignmentOptions.Center;

                // Class hint
                var classHintObj = new GameObject("ClassHint", typeof(RectTransform), typeof(TextMeshProUGUI));
                classHintObj.transform.SetParent(cardObj, false);
                var classHintRT = classHintObj.GetComponent<RectTransform>();
                classHintRT.anchorMin = new Vector2(0.05f, 0.08f);
                classHintRT.anchorMax = new Vector2(0.95f, 0.28f);
                classHintRT.offsetMin = Vector2.zero;
                classHintRT.offsetMax = Vector2.zero;

                var classHintText = classHintObj.GetComponent<TextMeshProUGUI>();
                classHintText.text = heroDef.HeroClass;
                classHintText.fontSize = 13;
                classHintText.color = GetClassColor(heroDef.HeroClass);
                classHintText.alignment = TextAlignmentOptions.Center;
            }

            // Add click handler
            var button = cardObj.gameObject.AddComponent<Button>();
            int capturedIndex = _heroCards.Count;
            button.onClick.AddListener(() => OnHeroCardClicked(heroDef));

            return cardObj;
        }

        private Color GetRarityColor(int starRank)
        {
            switch (starRank)
            {
                case 1: return RarityCommonColor;
                case 2: return RarityCommonColor;
                case 3: return RarityRareColor;
                case 4: return RarityRareColor;
                case 5: return RarityEpicColor;
                default:
                    return starRank >= 6 ? RarityLegendaryColor : RarityCommonColor;
            }
        }

        private Color GetClassColor(string heroClass)
        {
            switch (heroClass?.ToLower())
            {
                case "warrior": return new Color(0.9f, 0.3f, 0.3f, 1f);
                case "mage": return new Color(0.3f, 0.3f, 0.9f, 1f);
                case "support": return new Color(0.3f, 0.9f, 0.5f, 1f);
                case "tank": return new Color(0.8f, 0.6f, 0.2f, 1f);
                default: return Color.gray;
            }
        }

        private void CreateDetailPanel()
        {
            _detailPanel = Instantiate(DetailPanelPrefab, Canvas.transform, false);
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

        private void OnFilterClicked(int index)
        {
            _currentFilter = (FilterMode)index;
            RefreshHeroGrid();

            // Update filter button colors
            var filterContainer = FilterSortArea.Find("FilterSortContainer/FilterSection");
            if (filterContainer != null)
            {
                for (int i = 0; i < filterContainer.childCount; i++)
                {
                    var filter = filterContainer.GetChild(i);
                    var image = filter.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = i == (int)_currentFilter
                            ? new Color(0.3f, 0.5f, 0.9f, 0.8f)
                            : new Color(0.2f, 0.2f, 0.3f, 0.6f);
                    }
                }
            }
        }

        private void OnSortClicked(int index)
        {
            _currentSort = (SortMode)index;
            RefreshHeroGrid();
        }

        private void OnHeroCardClicked(HeroDefinition heroDef)
        {
            if (!_discoveredHeroIds.Contains(heroDef.HeroId))
            {
                // Can't view undiscovered heroes in detail
                return;
            }

            _selectedHeroId = heroDef.HeroId;
            _selectedHeroDef = heroDef;

            // Highlight selected card
            foreach (var card in _heroCards)
            {
                var bg = card.Find("Background")?.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = card.name.Contains(heroDef.HeroId)
                        ? SelectedCardColor
                        : DiscoveredCardColor;
                }
            }

            ShowHeroDetail(heroDef);
        }

        private void ShowHeroDetail(HeroDefinition heroDef)
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
            titleText.text = heroDef.HeroName;
            titleText.fontSize = 32;
            titleText.color = GetRarityColor(heroDef.StarRank);

            // Class and rank
            var classObj = new GameObject("ClassRank", typeof(RectTransform), typeof(TextMeshProUGUI));
            classObj.transform.SetParent(_detailPanel, false);
            var classRT = classObj.GetComponent<RectTransform>();
            classRT.anchorMin = new Vector2(0, 0.88f);
            classRT.anchorMax = new Vector2(1, 0.92f);
            classRT.offsetMin = new Vector2(20, 0);
            classRT.offsetMax = new Vector2(-20, 0);

            var classText = classObj.GetComponent<TextMeshProUGUI>();
            classText.text = $"{heroDef.HeroClass} - {new string('★', heroDef.StarRank)}";
            classText.fontSize = 20;
            classText.color = GetClassColor(heroDef.HeroClass);

            // Lore text
            var loreObj = new GameObject("Lore", typeof(RectTransform), typeof(TextMeshProUGUI));
            loreObj.transform.SetParent(_detailPanel, false);
            var loreRT = loreObj.GetComponent<RectTransform>();
            loreRT.anchorMin = new Vector2(0, 0.55f);
            loreRT.anchorMax = new Vector2(1, 0.86f);
            loreRT.offsetMin = new Vector2(20, 0);
            loreRT.offsetMax = new Vector2(-20, 0);

            var loreText = loreObj.GetComponent<TextMeshProUGUI>();
            loreText.text = heroDef.LoreText ?? "No lore available.";
            loreText.fontSize = 16;
            loreText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // Stats section
            CreateStatsSection(_detailPanel, heroDef);

            // Acquisition count
            var countObj = new GameObject("AcquisitionCount", typeof(RectTransform), typeof(TextMeshProUGUI));
            countObj.transform.SetParent(_detailPanel, false);
            var countRT = countObj.GetComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0, 0.45f);
            countRT.anchorMax = new Vector2(1, 0.5f);
            countRT.offsetMin = new Vector2(20, 0);
            countRT.offsetMax = new Vector2(-20, 0);

            var countText = countObj.GetComponent<TextMeshProUGUI>();
            countText.text = _acquisitionCounts.ContainsKey(heroDef.HeroId)
                ? $"Times Acquired: {_acquisitionCounts[heroDef.HeroId]}"
                : "Times Acquired: 0";
            countText.fontSize = 18;
            countText.color = Color.yellow;

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
        }

        private void CreateStatsSection(RectTransform parent, HeroDefinition heroDef)
        {
            var statsObj = new GameObject("StatsSection", typeof(RectTransform));
            statsObj.transform.SetParent(parent, false);
            var statsRT = statsObj.GetComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0, 0.25f);
            statsRT.anchorMax = new Vector2(1, 0.44f);
            statsRT.offsetMin = Vector2.zero;
            statsRT.offsetMax = Vector2.zero;

            string[] statLabels = { "HP", "ATK", "DEF", "SPD" };
            int[] statValues = { heroDef.BaseHealth, heroDef.BaseAttack, heroDef.BaseDefense, heroDef.BaseSpeed };

            for (int i = 0; i < statLabels.Length; i++)
            {
                var statObj = new GameObject($"Stat_{statLabels[i]}", typeof(RectTransform), typeof(TextMeshProUGUI));
                statObj.transform.SetParent(statsRT, false);
                var statRT = statObj.GetComponent<RectTransform>();
                statRT.anchorMin = new Vector2(i * 0.25f, 0);
                statRT.anchorMax = new Vector2((i + 1) * 0.25f, 1);
                statRT.offsetMin = Vector2.zero;
                statRT.offsetMax = Vector2.zero;

                var statText = statObj.GetComponent<TextMeshProUGUI>();
                statText.text = $"{statLabels[i]}: {statValues[i]}";
                statText.fontSize = 16;
                statText.color = Color.green;
                statText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void HideDetailPanel()
        {
            _detailPanel?.gameObject.SetActive(false);
            _selectedHeroId = null;
            _selectedHeroDef = null;
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

        public void RefreshUI()
        {
            if (_allHeroDefinitions != null)
            {
                RefreshHeroGrid();
            }
        }
    }
}