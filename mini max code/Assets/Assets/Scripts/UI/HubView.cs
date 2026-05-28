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
    /// Main UI controller for the Hub scene.
    /// Manages navigation to all other scenes and displays player resources.
    /// </summary>
    public class HubView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform TopBarArea;
        public RectTransform CenterArea;
        public RectTransform BottomDockArea;

        [Header("UI Elements")]
        public TextMeshProUGUI GoldText;
        public TextMeshProUGUI GemsText;
        public TextMeshProUGUI StonesText;
        public TextMeshProUGUI StaminaText;

        [Header("Navigation Buttons")]
        public Button RosterButton;
        public Button SynthButton;
        public Button TrainButton; // Now leads to Facilities
        public Button TowerButton;
        public Button SummonButton;
        public Button DungeonButton;
        public Button InventoryButton;
        public Button MemorialButton;

        [Header("Colors")]
        public Color ButtonActiveColor = new Color(0.2f, 0.4f, 0.6f, 0.9f);
        public Color ButtonInactiveColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        public Color GoldColor = new Color(1f, 0.85f, 0.2f);
        public Color GemsColor = new Color(0.3f, 0.8f, 1f);
        public Color StaminaColor = new Color(0.3f, 1f, 0.5f);

        // Services
        private ICurrencyService _currencyService;
        private IDungeonService _dungeonService;
        private IGameStateService _gameStateService;
        private IFacilityService _facilityService;

        // UI State
        private List<Button> _navButtons = new List<Button>();

        public void Initialize(
            ICurrencyService currencyService,
            IDungeonService dungeonService,
            IGameStateService gameStateService,
            IFacilityService facilityService)
        {
            _currencyService = currencyService;
            _dungeonService = dungeonService;
            _gameStateService = gameStateService;
            _facilityService = facilityService;

            SetupUI();
            SetupNavigation();
            RefreshUI();
        }

        private void SetupUI()
        {
            ClearExistingUI();

            // Create top bar
            CreateTopBar();

            // Create center area (welcome panel)
            CreateCenterArea();

            // Create bottom dock with navigation
            CreateBottomDock();
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
        }

        private void CreateTopBar()
        {
            var topBarContainer = new GameObject("TopBarContainer", typeof(RectTransform));
            topBarContainer.transform.SetParent(TopBarArea, false);

            var topBarRT = topBarContainer.GetComponent<RectTransform>();
            topBarRT.anchorMin = Vector2.zero;
            topBarRT.anchorMax = Vector2.one;
            topBarRT.offsetMin = Vector2.zero;
            topBarRT.offsetMax = Vector2.zero;

            // Background
            var bgObj = new GameObject("TopBarBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(topBarRT, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(topBarRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(0, 0.5f);
            titleRT.pivot = new Vector2(0, 0.5f);
            titleRT.anchoredPosition = new Vector2(20, 0);
            titleRT.sizeDelta = new Vector2(200, 50);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "Pick Me Up!";
            titleText.fontSize = 28;
            titleText.color = Color.white;

            // Currency display (right side)
            CreateCurrencyDisplay(topBarRT, "Stamina", () => $"{_dungeonService.GetStamina()}/{_dungeonService.GetMaxStamina()}", StaminaColor, 0.4f, 0.5f);
            CreateCurrencyDisplay(topBarRT, "Gold", () => $"{_currencyService.GetGold():N0}", GoldColor, 0.55f, 0.5f);
            CreateCurrencyDisplay(topBarRT, "Gems", () => $"{_currencyService.GetGems():N0}", GemsColor, 0.7f, 0.5f);
            CreateCurrencyDisplay(topBarRT, "Stones", () => $"{_currencyService.GetStones():N0}", Color.gray, 0.85f, 0.5f);
        }

        private void CreateCurrencyDisplay(RectTransform parent, string label, Func<string> valueFunc, Color color, float xMin, float xMax)
        {
            var currencyObj = new GameObject($"{label}Display", typeof(RectTransform));
            currencyObj.transform.SetParent(parent, false);
            var currencyRT = currencyObj.GetComponent<RectTransform>();
            currencyRT.anchorMin = new Vector2(xMin, 0.5f);
            currencyRT.anchorMax = new Vector2(xMax, 0.5f);
            currencyRT.offsetMin = Vector2.zero;
            currencyRT.offsetMax = Vector2.zero;
            currencyRT.sizeDelta = new Vector2(50, 40);

            var currencyText = currencyObj.AddComponent<TextMeshProUGUI>();
            currencyText.fontSize = 18;
            currencyText.color = color;
            currencyText.alignment = TextAlignmentOptions.Right;

            // Store reference for updating
            switch (label)
            {
                case "Gold":
                    GoldText = currencyText;
                    break;
                case "Gems":
                    GemsText = currencyText;
                    break;
                case "Stones":
                    StonesText = currencyText;
                    break;
                case "Stamina":
                    StaminaText = currencyText;
                    break;
            }
        }

        private void CreateCenterArea()
        {
            var centerContainer = new GameObject("CenterArea", typeof(RectTransform));
            centerContainer.transform.SetParent(CenterArea, false);

            var centerRT = centerContainer.GetComponent<RectTransform>();
            centerRT.anchorMin = Vector2.zero;
            centerRT.anchorMax = Vector2.one;
            centerRT.offsetMin = Vector2.zero;
            centerRT.offsetMax = Vector2.zero;

            // Welcome banner
            var bannerObj = new GameObject("WelcomeBanner", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(centerRT, false);
            var bannerRT = bannerObj.GetComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0.1f, 0.6f);
            bannerRT.anchorMax = new Vector2(0.9f, 0.85f);
            bannerRT.offsetMin = Vector2.zero;
            bannerRT.offsetMax = Vector2.zero;

            var bannerImage = bannerObj.GetComponent<Image>();
            bannerImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var bannerTextObj = new GameObject("BannerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bannerTextObj.transform.SetParent(bannerObj.transform, false);
            var bannerTextRT = bannerTextObj.GetComponent<RectTransform>();
            bannerTextRT.anchorMin = Vector2.zero;
            bannerTextRT.anchorMax = Vector2.one;
            bannerTextRT.offsetMin = new Vector2(20, 10);
            bannerTextRT.offsetMax = new Vector2(-20, -10);

            var bannerText = bannerTextObj.GetComponent<TextMeshProUGUI>();
            bannerText.text = "Welcome to the Hub!\nSelect an option below to begin.";
            bannerText.fontSize = 24;
            bannerText.color = Color.white;
            bannerText.alignment = TextAlignmentOptions.Center;

            // Memorial Hall quick access
            var memorialObj = new GameObject("MemorialButton", typeof(RectTransform), typeof(Image), typeof(Button));
            memorialObj.transform.SetParent(centerRT, false);
            var memorialRT = memorialObj.GetComponent<RectTransform>();
            memorialRT.anchorMin = new Vector2(0.1f, 0.35f);
            memorialRT.anchorMax = new Vector2(0.9f, 0.55f);
            memorialRT.offsetMin = Vector2.zero;
            memorialRT.offsetMax = Vector2.zero;

            var memorialImage = memorialObj.GetComponent<Image>();
            memorialImage.color = new Color(0.3f, 0.2f, 0.4f, 0.8f);

            MemorialButton = memorialObj.GetComponent<Button>();
            MemorialButton.onClick.AddListener(() => _gameStateService.LoadScene("MemorialHall"));

            var memorialTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            memorialTextObj.transform.SetParent(memorialObj.transform, false);
            var memorialTextRT = memorialTextObj.GetComponent<RectTransform>();
            memorialTextRT.anchorMin = Vector2.zero;
            memorialTextRT.anchorMax = Vector2.one;
            memorialTextRT.offsetMin = new Vector2(10, 10);
            memorialTextRT.offsetMax = new Vector2(-10, -10);

            var memorialText = memorialTextObj.GetComponent<TextMeshProUGUI>();
            memorialText.text = "View Hero Collection (Memorial Hall)";
            memorialText.fontSize = 22;
            memorialText.color = Color.white;
            memorialText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateBottomDock()
        {
            var dockContainer = new GameObject("BottomDock", typeof(RectTransform));
            dockContainer.transform.SetParent(BottomDockArea, false);

            var dockRT = dockContainer.GetComponent<RectTransform>();
            dockRT.anchorMin = Vector2.zero;
            dockRT.anchorMax = Vector2.one;
            dockRT.offsetMin = Vector2.zero;
            dockRT.offsetMax = Vector2.zero;

            // Background
            var bgObj = new GameObject("DockBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(dockRT, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // Navigation buttons - 7 buttons (may need 2 rows)
            string[] buttonLabels = { "ROSTER", "SYNTH", "TRAIN", "TOWER", "SUMMON", "DUNGEON", "INVENTORY" };
            string[] scenes = { "Roster", "SynthesisLab", "Facilities", "Tower", "Summon", "Dungeon", "Inventory" };

            // Row 1: 4 buttons
            CreateNavButtonRow(dockRT, buttonLabels.Take(4).ToArray(), scenes.Take(4).ToArray(), 0, 4, 0.25f);

            // Row 2: 3 buttons
            CreateNavButtonRow(dockRT, buttonLabels.Skip(4).ToArray(), scenes.Skip(4).ToArray(), 0.25f, 4, 0.55f);
        }

        private void CreateNavButtonRow(RectTransform parent, string[] labels, string[] scenes, float startX, int count, float yMin)
        {
            float buttonWidth = 0.22f;
            float spacing = 0.01f;
            float startPos = startX + (1f - (count * buttonWidth + (count - 1) * spacing)) / 2f;

            for (int i = 0; i < labels.Length; i++)
            {
                var btnObj = new GameObject($"NavButton_{labels[i]}", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(parent, false);
                var btnRT = btnObj.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(startPos + i * (buttonWidth + spacing), yMin);
                btnRT.anchorMax = new Vector2(startPos + i * (buttonWidth + spacing) + buttonWidth, yMin + 0.5f);
                btnRT.offsetMin = Vector2.zero;
                btnRT.offsetMax = Vector2.zero;

                var btnImage = btnObj.GetComponent<Image>();
                btnImage.color = ButtonActiveColor;

                var button = btnObj.GetComponent<Button>();
                string sceneName = scenes[i];
                button.onClick.AddListener(() => OnNavButtonClicked(sceneName));

                _navButtons.Add(button);

                var btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                btnTextObj.transform.SetParent(btnObj.transform, false);
                var btnTextRT = btnTextObj.GetComponent<RectTransform>();
                btnTextRT.anchorMin = Vector2.zero;
                btnTextRT.anchorMax = Vector2.one;
                btnTextRT.offsetMin = new Vector2(5, 5);
                btnTextRT.offsetMax = new Vector2(-5, -5);

                var btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
                btnText.text = labels[i];
                btnText.fontSize = 14;
                btnText.color = Color.white;
                btnText.alignment = TextAlignmentOptions.Center;
            }

            // Store button references
            if (labels.Contains("ROSTER")) RosterButton = _navButtons[0];
            if (labels.Contains("SYNTH")) SynthButton = _navButtons[1];
            if (labels.Contains("TRAIN")) TrainButton = _navButtons[2];
            if (labels.Contains("TOWER")) TowerButton = _navButtons[3];
            if (labels.Contains("SUMMON")) SummonButton = _navButtons[4];
            if (labels.Contains("DUNGEON")) DungeonButton = _navButtons[5];
            if (labels.Contains("INVENTORY")) InventoryButton = _navButtons[6];
        }

        private void SetupNavigation()
        {
            // Additional button setup if needed
            if (SummonButton != null)
            {
                SummonButton.onClick.RemoveAllListeners();
                SummonButton.onClick.AddListener(() => OnNavButtonClicked("Summon"));
            }

            if (RosterButton != null)
            {
                RosterButton.onClick.RemoveAllListeners();
                RosterButton.onClick.AddListener(() => OnNavButtonClicked("Roster"));
            }
        }

        private void OnNavButtonClicked(string sceneName)
        {
            Debug.Log($"HubView: Navigating to {sceneName}");
            _gameStateService.LoadScene(sceneName);
        }

        private void RefreshUI()
        {
            if (GoldText != null)
                GoldText.text = $"{_currencyService.GetGold():N0}";

            if (GemsText != null)
                GemsText.text = $"{_currencyService.GetGems():N0}";

            if (StonesText != null)
                StonesText.text = $"{_currencyService.GetStones():N0}";

            if (StaminaText != null)
                StaminaText.text = $"{_dungeonService.GetStamina()}/{_dungeonService.GetMaxStamina()}";
        }

        private void OnEnable()
        {
            RefreshUI();
            _dungeonService.ProcessStaminaRegeneration();
        }

        private void Update()
        {
            // Periodic UI refresh
            if (Time.frameCount % 30 == 0)
            {
                RefreshUI();
            }
        }
    }
}