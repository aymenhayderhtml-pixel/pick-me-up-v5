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
    /// Main UI controller for the Facilities system.
    /// Shows all buildings, their levels, and upgrade options.
    /// </summary>
    public class FacilityView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform HeaderArea;
        public RectTransform FacilityListArea;
        public RectTransform DetailArea;

        [Header("UI Elements")]
        public Button BackButton;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI GoldDisplayText;
        public TextMeshProUGUI GemsDisplayText;

        [Header("Prefabs")]
        public RectTransform FacilityCardPrefab;
        public RectTransform UpgradeProgressPrefab;

        [Header("Colors")]
        public Color FacilityPanelColor = new Color(0.12f, 0.12f, 0.18f, 0.95f);
        public Color UpgradeButtonColor = new Color(0.2f, 0.5f, 0.2f, 0.9f);
        public Color MaxLevelColor = new Color(0.6f, 0.5f, 0.2f, 0.9f);
        public Color ProgressBarColor = new Color(0.3f, 0.6f, 0.9f, 1f);

        // Services
        private IFacilityService _facilityService;
        private ICurrencyService _currencyService;
        private IGameStateService _gameStateService;

        // State
        private List<FacilityDefinition> _facilities;
        private string _selectedFacilityId;
        private List<RectTransform> _facilityCards = new List<RectTransform>();

        // UI State
        private RectTransform _detailPanel;
        private RectTransform _upgradeProgressBar;

        public void Initialize(
            IFacilityService facilityService,
            ICurrencyService currencyService,
            IGameStateService gameStateService)
        {
            _facilityService = facilityService;
            _currencyService = currencyService;
            _gameStateService = gameStateService;

            _facilities = _facilityService.GetAllFacilities();

            SetupUI();
            SetupEventListeners();
            RefreshUI();
        }

        private void SetupUI()
        {
            ClearExistingUI();

            // Create header
            CreateHeader();

            // Create facility list
            CreateFacilityList();

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

            _facilityCards.Clear();
            _selectedFacilityId = null;
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
            bgImage.color = FacilityPanelColor;

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
            titleRT.anchorMin = new Vector2(0.2f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            titleRT.sizeDelta = new Vector2(0, 50);

            TitleText = titleObj.GetComponent<TextMeshProUGUI>();
            TitleText.text = "Facilities";
            TitleText.fontSize = 32;
            TitleText.color = Color.white;

            // Currency display
            var currencyObj = new GameObject("CurrencyDisplay", typeof(RectTransform));
            currencyObj.transform.SetParent(headerRT, false);
            var currencyRT = currencyObj.GetComponent<RectTransform>();
            currencyRT.anchorMin = new Vector2(0.6f, 0.3f);
            currencyRT.anchorMax = new Vector2(1, 0.7f);
            currencyRT.offsetMin = Vector2.zero;
            currencyRT.offsetMax = Vector2.zero;

            // Gold display
            var goldObj = new GameObject("Gold", typeof(RectTransform), typeof(TextMeshProUGUI));
            goldObj.transform.SetParent(currencyRT, false);
            var goldRT = goldObj.GetComponent<RectTransform>();
            goldRT.anchorMin = Vector2.zero;
            goldRT.anchorMax = new Vector2(0.5f, 1);
            goldRT.offsetMin = Vector2.zero;
            goldRT.offsetMax = Vector2.zero;

            GoldDisplayText = goldObj.GetComponent<TextMeshProUGUI>();
            GoldDisplayText.text = "0 Gold";
            GoldDisplayText.fontSize = 20;
            GoldDisplayText.color = new Color(1f, 0.85f, 0.2f); // Gold color

            // Gems display
            var gemsObj = new GameObject("Gems", typeof(RectTransform), typeof(TextMeshProUGUI));
            gemsObj.transform.SetParent(currencyRT, false);
            var gemsRT = gemsObj.GetComponent<RectTransform>();
            gemsRT.anchorMin = new Vector2(0.5f, 0);
            gemsRT.anchorMax = new Vector2(1, 1);
            gemsRT.offsetMin = Vector2.zero;
            gemsRT.offsetMax = Vector2.zero;

            GemsDisplayText = gemsObj.GetComponent<TextMeshProUGUI>();
            GemsDisplayText.text = "0 Gems";
            GemsDisplayText.fontSize = 20;
            GemsDisplayText.color = new Color(0.3f, 0.8f, 1f); // Blue/Gem color
        }

        private void CreateFacilityList()
        {
            var listContainer = new GameObject("FacilityList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listContainer.transform.SetParent(FacilityListArea, false);

            var listRT = listContainer.GetComponent<RectTransform>();
            listRT.anchorMin = Vector2.zero;
            listRT.anchorMax = Vector2.one;
            listRT.offsetMin = Vector2.zero;
            listRT.offsetMax = Vector2.zero;

            var layout = listContainer.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            // Create facility cards
            foreach (var facility in _facilities)
            {
                var card = CreateFacilityCard(facility);
                _facilityCards.Add(card);
            }
        }

        private RectTransform CreateFacilityCard(FacilityDefinition facility)
        {
            var cardObj = Instantiate(FacilityCardPrefab, FacilityListArea, false);
            cardObj.name = $"FacilityCard_{facility.FacilityId}";

            // Background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(cardObj, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(
                facility.FacilityColor.r * 0.3f,
                facility.FacilityColor.g * 0.3f,
                facility.FacilityColor.b * 0.3f,
                0.8f
            );

            // Icon
            if (facility.FacilityIcon != null)
            {
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardObj, false);
                var iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.05f, 0.1f);
                iconRT.anchorMax = new Vector2(0.2f, 0.9f);
                iconRT.offsetMin = Vector2.zero;
                iconRT.offsetMax = Vector2.zero;

                var iconImage = iconObj.GetComponent<Image>();
                iconImage.sprite = facility.FacilityIcon;
                iconImage.preserveAspect = true;
            }

            // Name
            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(cardObj, false);
            var nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.25f, 0.6f);
            nameRT.anchorMax = new Vector2(0.7f, 0.85f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            var nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = facility.DisplayName;
            nameText.fontSize = 24;
            nameText.color = Color.white;

            // Level
            int level = _facilityService.GetFacilityLevel(facility.FacilityId);
            var levelObj = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(cardObj, false);
            var levelRT = levelObj.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0.25f, 0.35f);
            levelRT.anchorMax = new Vector2(0.5f, 0.55f);
            levelRT.offsetMin = Vector2.zero;
            levelRT.offsetMax = Vector2.zero;

            var levelText = levelObj.GetComponent<TextMeshProUGUI>();
            levelText.text = $"Lv.{level}/{facility.MaxLevel}";
            levelText.fontSize = 18;
            levelText.color = Color.yellow;

            // Current benefit
            float currentBenefit = _facilityService.GetCurrentBenefit(facility.FacilityId);
            var benefitObj = new GameObject("Benefit", typeof(RectTransform), typeof(TextMeshProUGUI));
            benefitObj.transform.SetParent(cardObj, false);
            var benefitRT = benefitObj.GetComponent<RectTransform>();
            benefitRT.anchorMin = new Vector2(0.25f, 0.1f);
            benefitRT.anchorMax = new Vector2(0.7f, 0.3f);
            benefitRT.offsetMin = Vector2.zero;
            benefitRT.offsetMax = Vector2.zero;

            var benefitText = benefitObj.GetComponent<TextMeshProUGUI>();
            benefitText.text = facility.GetBenefitDescription(level);
            benefitText.fontSize = 16;
            benefitText.color = Color.cyan;

            // Upgrade button
            var upgradeBtnObj = new GameObject("UpgradeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            upgradeBtnObj.transform.SetParent(cardObj, false);
            var upgradeBtnRT = upgradeBtnObj.GetComponent<RectTransform>();
            upgradeBtnRT.anchorMin = new Vector2(0.75f, 0.2f);
            upgradeBtnRT.anchorMax = new Vector2(0.95f, 0.8f);
            upgradeBtnRT.offsetMin = Vector2.zero;
            upgradeBtnRT.offsetMax = Vector2.zero;

            var upgradeBtnImage = upgradeBtnObj.GetComponent<Image>();
            upgradeBtnImage.color = level >= facility.MaxLevel ? MaxLevelColor : UpgradeButtonColor;

            var upgradeBtn = upgradeBtnObj.GetComponent<Button>();
            string capturedId = facility.FacilityId;
            upgradeBtn.onClick.AddListener(() => OnUpgradeClicked(capturedId));

            var upgradeBtnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            upgradeBtnTextObj.transform.SetParent(upgradeBtnObj.transform, false);
            var upgradeBtnTextRT = upgradeBtnTextObj.GetComponent<RectTransform>();
            upgradeBtnTextRT.anchorMin = Vector2.zero;
            upgradeBtnTextRT.anchorMax = Vector2.one;
            upgradeBtnTextRT.offsetMin = new Vector2(5, 5);
            upgradeBtnTextRT.offsetMax = new Vector2(-5, -5);

            var upgradeBtnText = upgradeBtnTextObj.GetComponent<TextMeshProUGUI>();
            upgradeBtnText.text = level >= facility.MaxLevel ? "MAX" : "Upgrade";
            upgradeBtnText.fontSize = 16;
            upgradeBtnText.color = Color.white;
            upgradeBtnText.alignment = TextAlignmentOptions.Center;

            // Add click handler to card
            var cardButton = cardObj.gameObject.AddComponent<Button>();
            cardButton.onClick.AddListener(() => OnFacilityClicked(facility.FacilityId));

            return cardObj;
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

            // Upgrade progress bar (shown during upgrade)
            _upgradeProgressBar = new GameObject("UpgradeProgress", typeof(RectTransform), typeof(Image));
            _upgradeProgressBar.transform.SetParent(_detailPanel, false);
            var progressRT = _upgradeProgressBar.GetComponent<RectTransform>();
            progressRT.anchorMin = new Vector2(0.1f, 0.75f);
            progressRT.anchorMax = new Vector2(0.9f, 0.85f);
            progressRT.offsetMin = Vector2.zero;
            progressRT.offsetMax = Vector2.zero;

            var progressBgImage = _upgradeProgressBar.GetComponent<Image>();
            progressBgImage.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

            // Progress fill
            var progressFillObj = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
            progressFillObj.transform.SetParent(_upgradeProgressBar, false);
            var fillRT = progressFillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.5f, 1);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            var fillImage = progressFillObj.GetComponent<Image>();
            fillImage.color = ProgressBarColor;

            _upgradeProgressBar.gameObject.SetActive(false);
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
            // Update currency display
            GoldDisplayText.text = $"{_currencyService.GetGold():N0} Gold";
            GemsDisplayText.text = $"{_currencyService.GetGems():N0} Gems";

            // Update facility cards
            foreach (var card in _facilityCards)
            {
                UpdateFacilityCard(card);
            }

            // Update detail panel if open
            if (!string.IsNullOrEmpty(_selectedFacilityId))
            {
                UpdateDetailPanel(_selectedFacilityId);
            }
        }

        private void UpdateFacilityCard(RectTransform card)
        {
            string facilityId = card.name.Replace("FacilityCard_", "");
            var facility = _facilityService.GetFacility(facilityId);
            if (facility == null) return;

            int level = _facilityService.GetFacilityLevel(facilityId);

            // Update level text
            var levelText = card.Find("Level")?.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}/{facility.MaxLevel}";
            }

            // Update benefit text
            var benefitText = card.Find("Benefit")?.GetComponent<TextMeshProUGUI>();
            if (benefitText != null)
            {
                benefitText.text = facility.GetBenefitDescription(level);
            }

            // Update upgrade button
            var upgradeBtn = card.Find("UpgradeButton")?.GetComponent<Button>();
            var upgradeBtnImage = card.Find("UpgradeButton")?.GetComponent<Image>();
            var upgradeBtnText = card.Find("UpgradeButton/Text")?.GetComponent<TextMeshProUGUI>();

            if (level >= facility.MaxLevel)
            {
                if (upgradeBtnText != null) upgradeBtnText.text = "MAX";
                if (upgradeBtnImage != null) upgradeBtnImage.color = MaxLevelColor;
                if (upgradeBtn != null) upgradeBtn.interactable = false;
            }
            else
            {
                var cost = _facilityService.GetUpgradeCost(facilityId);
                if (upgradeBtnText != null)
                {
                    upgradeBtnText.text = $"{cost.Gold:N0}";
                }
                if (upgradeBtnImage != null)
                {
                    upgradeBtnImage.color = _facilityService.CanUpgrade(facilityId)
                        ? UpgradeButtonColor
                        : new Color(0.3f, 0.3f, 0.3f, 0.8f);
                }
                if (upgradeBtn != null)
                {
                    upgradeBtn.interactable = _facilityService.CanUpgrade(facilityId);
                }
            }

            // Show upgrading indicator if applicable
            var upgradeIndicator = card.Find("Upgrading");
            if (upgradeIndicator != null)
            {
                upgradeIndicator.gameObject.SetActive(_facilityService.IsUpgrading(facilityId));
            }
        }

        private void UpdateDetailPanel(string facilityId)
        {
            var facility = _facilityService.GetFacility(facilityId);
            if (facility == null) return;

            int level = _facilityService.GetFacilityLevel(facilityId);
            var upgradeProgress = _facilityService.GetUpgradeProgress(facilityId);

            // Show/hide upgrade progress bar
            if (_upgradeProgressBar != null)
            {
                _upgradeProgressBar.gameObject.SetActive(upgradeProgress != null);

                if (upgradeProgress != null)
                {
                    // Update progress bar fill
                    var fill = _upgradeProgressBar.Find("ProgressFill");
                    if (fill != null)
                    {
                        var fillRT = fill.GetComponent<RectTransform>();
                        var totalTime = (float)(upgradeProgress.CompletionTime - upgradeProgress.StartTime).TotalSeconds;
                        var elapsed = (float)(DateTime.Now - upgradeProgress.StartTime).TotalSeconds;
                        float progress = Mathf.Clamp01(elapsed / totalTime);

                        fillRT.anchorMax = new Vector2(progress, 1);
                    }
                }
            }
        }

        private void OnFacilityClicked(string facilityId)
        {
            _selectedFacilityId = facilityId;
            ShowFacilityDetail(facilityId);
        }

        private void ShowFacilityDetail(string facilityId)
        {
            var facility = _facilityService.GetFacility(facilityId);
            if (facility == null) return;

            _detailPanel.gameObject.SetActive(true);

            // Clear existing content
            foreach (Transform child in _detailPanel)
            {
                if (child.name != "Background" && child.name != "UpgradeProgress")
                    Destroy(child.gameObject);
            }

            int level = _facilityService.GetFacilityLevel(facilityId);

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(_detailPanel, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = new Vector2(1, 0.98f);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = facility.DisplayName;
            titleText.fontSize = 28;
            titleText.color = facility.FacilityColor;

            // Level
            var levelObj = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(_detailPanel, false);
            var levelRT = levelObj.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0, 0.82f);
            levelRT.anchorMax = new Vector2(1, 0.89f);
            levelRT.offsetMin = new Vector2(20, 0);
            levelRT.offsetMax = new Vector2(-20, 0);

            var levelText = levelObj.GetComponent<TextMeshProUGUI>();
            levelText.text = $"Level {level} / {facility.MaxLevel}";
            levelText.fontSize = 22;
            levelText.color = Color.yellow;

            // Description
            var descObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(_detailPanel, false);
            var descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.65f);
            descRT.anchorMax = new Vector2(1, 0.8f);
            descRT.offsetMin = new Vector2(20, 0);
            descRT.offsetMax = new Vector2(-20, 0);

            var descText = descObj.GetComponent<TextMeshProUGUI>();
            descText.text = facility.Description;
            descText.fontSize = 16;
            descText.color = Color.gray;

            // Current benefit
            var currentBenefitObj = new GameObject("CurrentBenefit", typeof(RectTransform), typeof(TextMeshProUGUI));
            currentBenefitObj.transform.SetParent(_detailPanel, false);
            var currentBenefitRT = currentBenefitObj.GetComponent<RectTransform>();
            currentBenefitRT.anchorMin = new Vector2(0, 0.45f);
            currentBenefitRT.anchorMax = new Vector2(1, 0.55f);
            currentBenefitRT.offsetMin = new Vector2(20, 0);
            currentBenefitRT.offsetMax = new Vector2(-20, 0);

            var currentBenefitText = currentBenefitObj.GetComponent<TextMeshProUGUI>();
            currentBenefitText.text = $"Current: {facility.GetBenefitDescription(level)}";
            currentBenefitText.fontSize = 20;
            currentBenefitText.color = Color.green;

            // Next level benefit
            if (level < facility.MaxLevel)
            {
                var nextBenefitObj = new GameObject("NextBenefit", typeof(RectTransform), typeof(TextMeshProUGUI));
                nextBenefitObj.transform.SetParent(_detailPanel, false);
                var nextBenefitRT = nextBenefitObj.GetComponent<RectTransform>();
                nextBenefitRT.anchorMin = new Vector2(0, 0.35f);
                nextBenefitRT.anchorMax = new Vector2(1, 0.45f);
                nextBenefitRT.offsetMin = new Vector2(20, 0);
                nextBenefitRT.offsetMax = new Vector2(-20, 0);

                var nextBenefitText = nextBenefitObj.GetComponent<TextMeshProUGUI>();
                nextBenefitText.text = $"Next: {facility.GetNextLevelBenefitDescription(level)}";
                nextBenefitText.fontSize = 18;
                nextBenefitText.color = Color.cyan;
            }

            // Upgrade cost
            if (level < facility.MaxLevel)
            {
                var cost = _facilityService.GetUpgradeCost(facilityId);

                var costObj = new GameObject("UpgradeCost", typeof(RectTransform), typeof(TextMeshProUGUI));
                costObj.transform.SetParent(_detailPanel, false);
                var costRT = costObj.GetComponent<RectTransform>();
                costRT.anchorMin = new Vector2(0, 0.15f);
                costRT.anchorMax = new Vector2(1, 0.28f);
                costRT.offsetMin = new Vector2(20, 0);
                costRT.offsetMax = new Vector2(-20, 0);

                var costText = costObj.GetComponent<TextMeshProUGUI>();
                costText.text = $"Upgrade Cost: {cost.Gold:N0} Gold";
                if (cost.Gems > 0)
                {
                    costText.text += $" + {cost.Gems} Gems";
                }
                costText.text += $" ({cost.TimeSeconds / 60:F0} min)";
                costText.fontSize = 18;
                costText.color = Color.white;
            }

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

        private void HideDetailPanel()
        {
            _detailPanel?.gameObject.SetActive(false);
            _selectedFacilityId = null;
        }

        private void OnUpgradeClicked(string facilityId)
        {
            if (_facilityService.StartUpgrade(facilityId))
            {
                Debug.Log($"Started upgrade for {facilityId}");
                RefreshUI();
                UpdateDetailPanel(facilityId);
            }
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
            _facilityService.ProcessPassiveGeneration();
        }

        private void Update()
        {
            // Periodically update UI
            if (Time.frameCount % 60 == 0)
            {
                RefreshUI();
                _facilityService.ProcessPassiveGeneration();
            }
        }
    }
}