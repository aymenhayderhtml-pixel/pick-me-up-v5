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
    /// Main UI controller for the Synthesis Lab (dedicated scene).
    /// Handles hero synthesis with base hero + material heroes.
    /// </summary>
    public class SynthView : MonoBehaviour
    {
        [Header("References")]
        public Canvas Canvas;
        public RectTransform ContentArea;
        public RectTransform HeaderArea;
        public RectTransform BaseHeroArea;
        public RectTransform MaterialArea;
        public RectTransform PreviewArea;
        public RectTransform AnimationArea;
        public RectTransform FooterArea;

        [Header("UI Elements")]
        public Button BackButton;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI SuccessRateText;

        [Header("Prefabs")]
        public RectTransform HeroSelectCardPrefab;
        public RectTransform MaterialCardPrefab;
        public RectTransform ResultPreviewPrefab;

        [Header("Colors")]
        public Color BaseHeroSlotColor = new Color(0.2f, 0.5f, 0.8f, 0.8f);
        public Color MaterialSlotColor = new Color(0.5f, 0.2f, 0.2f, 0.6f);
        public Color SelectedSlotColor = new Color(0.3f, 0.8f, 0.4f, 0.9f);
        public Color SuccessHighColor = Color.green;
        public Color SuccessMediumColor = Color.yellow;
        public Color SuccessLowColor = Color.red;

        // Services
        private IRosterService _rosterService;
        private ISynthesizerService _synthesizerService;
        private IGameStateService _gameStateService;
        private ICurrencyService _currencyService;

        // State
        private HeroInstance _selectedBaseHero;
        private List<HeroInstance> _selectedMaterials = new List<HeroInstance>();
        private List<HeroInstance> _availableHeroes = new List<HeroInstance>();
        private SynthesisRecipe _currentRecipe;
        private bool _isAnimating;

        // UI State
        private RectTransform _baseHeroSlot;
        private List<RectTransform> _materialSlots = new List<RectTransform>();
        private RectTransform _previewPanel;

        public void Initialize(
            IRosterService rosterService,
            ISynthesizerService synthesizerService,
            IGameStateService gameStateService,
            ICurrencyService currencyService)
        {
            _rosterService = rosterService;
            _synthesizerService = synthesizerService;
            _gameStateService = gameStateService;
            _currencyService = currencyService;

            LoadAvailableHeroes();
            SetupUI();
            SetupEventListeners();
            RefreshUI();
        }

        private void LoadAvailableHeroes()
        {
            _availableHeroes = _rosterService.GetAllHeroes();
        }

        private void SetupUI()
        {
            ClearExistingUI();

            // Create header
            CreateHeader();

            // Create base hero selection area
            CreateBaseHeroArea();

            // Create material selection area
            CreateMaterialArea();

            // Create preview area
            CreatePreviewArea();

            // Create footer with action buttons
            CreateFooter();

            // Create animation area (for synthesis effects)
            CreateAnimationArea();
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

            _selectedBaseHero = null;
            _selectedMaterials.Clear();
            _materialSlots.Clear();
            _isAnimating = false;
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
            titleRT.anchorMin = new Vector2(0.3f, 0.5f);
            titleRT.anchorMax = new Vector2(0.7f, 0.5f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            titleRT.sizeDelta = new Vector2(0, 50);

            TitleText = titleObj.GetComponent<TextMeshProUGUI>();
            TitleText.text = "Synthesis Lab";
            TitleText.fontSize = 32;
            TitleText.color = Color.white;

            // Success rate display
            var rateObj = new GameObject("SuccessRate", typeof(RectTransform), typeof(TextMeshProUGUI));
            rateObj.transform.SetParent(headerRT, false);
            var rateRT = rateObj.GetComponent<RectTransform>();
            rateRT.anchorMin = new Vector2(0.7f, 0.5f);
            rateRT.anchorMax = new Vector2(1, 0.5f);
            rateRT.pivot = new Vector2(1, 0.5f);
            rateRT.anchoredPosition = new Vector2(-20, 0);
            rateRT.sizeDelta = new Vector2(200, 50);

            SuccessRateText = rateObj.GetComponent<TextMeshProUGUI>();
            SuccessRateText.text = "Select Heroes";
            SuccessRateText.fontSize = 20;
            SuccessRateText.color = Color.gray;
            SuccessRateText.alignment = TextAlignmentOptions.Right;
        }

        private void CreateBaseHeroArea()
        {
            var baseHeroContainer = new GameObject("BaseHeroArea", typeof(RectTransform));
            baseHeroContainer.transform.SetParent(BaseHeroArea, false);

            var containerRT = baseHeroContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            // Section title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(containerRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.85f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "Base Hero (will be enhanced)";
            titleText.fontSize = 22;
            titleText.color = Color.cyan;

            // Base hero slot
            _baseHeroSlot = new GameObject("BaseHeroSlot", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
            _baseHeroSlot.transform.SetParent(containerRT, false);
            _baseHeroSlot.anchorMin = new Vector2(0.2f, 0.1f);
            _baseHeroSlot.anchorMax = new Vector2(0.8f, 0.8f);
            _baseHeroSlot.offsetMin = Vector2.zero;
            _baseHeroSlot.offsetMax = Vector2.zero;

            var slotImage = _baseHeroSlot.GetComponent<Image>();
            slotImage.color = BaseHeroSlotColor;

            var slotButton = _baseHeroSlot.GetComponent<Button>();
            slotButton.onClick.AddListener(OnBaseHeroSlotClicked);

            // Slot content
            var slotContentObj = new GameObject("SlotContent", typeof(RectTransform), typeof(TextMeshProUGUI));
            slotContentObj.transform.SetParent(_baseHeroSlot, false);
            var slotContentRT = slotContentObj.GetComponent<RectTransform>();
            slotContentRT.anchorMin = Vector2.zero;
            slotContentRT.anchorMax = Vector2.one;
            slotContentRT.offsetMin = Vector2.zero;
            slotContentRT.offsetMax = Vector2.zero;

            var slotContentText = slotContentObj.GetComponent<TextMeshProUGUI>();
            slotContentText.text = "Tap to Select Base Hero";
            slotContentText.fontSize = 24;
            slotContentText.color = Color.white;
            slotContentText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateMaterialArea()
        {
            var materialContainer = new GameObject("MaterialArea", typeof(RectTransform));
            materialContainer.transform.SetParent(MaterialArea, false);

            var containerRT = materialContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            // Section title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(containerRT, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "Material Heroes (will be consumed)";
            titleText.fontSize = 20;
            titleText.color = new Color(1f, 0.5f, 0.5f);

            // Material slots (3 slots)
            float slotWidth = 160;
            float startX = (1080 - slotWidth * 3 - 40) / 2;

            for (int i = 0; i < 3; i++)
            {
                var slotObj = new GameObject($"MaterialSlot_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                slotObj.transform.SetParent(containerRT, false);
                var slotRT = slotObj.GetComponent<RectTransform>();
                slotRT.anchorMin = new Vector2(0.5f, 0.2f);
                slotRT.anchorMax = new Vector2(0.5f, 0.2f);
                slotRT.pivot = new Vector2(0.5f, 0.5f);
                slotRT.anchoredPosition = new Vector2(startX + i * (slotWidth + 20) - 540 + slotWidth/2, 0);
                slotRT.sizeDelta = new Vector2(slotWidth, 200);

                var slotImage = slotObj.GetComponent<Image>();
                slotImage.color = MaterialSlotColor;

                var slotButton = slotObj.GetComponent<Button>();
                int slotIndex = i;
                slotButton.onClick.AddListener(() => OnMaterialSlotClicked(slotIndex));

                // Slot number
                var numObj = new GameObject("SlotNumber", typeof(RectTransform), typeof(TextMeshProUGUI));
                numObj.transform.SetParent(slotObj.transform, false);
                var numRT = numObj.GetComponent<RectTransform>();
                numRT.anchorMin = new Vector2(0.05f, 0.05f);
                numRT.anchorMax = new Vector2(0.15f, 0.15f);
                numRT.offsetMin = Vector2.zero;
                numRT.offsetMax = Vector2.zero;

                var numText = numObj.GetComponent<TextMeshProUGUI>();
                numText.text = $"{i + 1}";
                numText.fontSize = 16;
                numText.color = Color.white;

                _materialSlots.Add(slotRT);
            }
        }

        private void CreatePreviewArea()
        {
            _previewPanel = Instantiate(ResultPreviewPrefab, PreviewArea, false);
            _previewPanel.gameObject.SetActive(true);

            // Background
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(_previewPanel, false);
            var bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            // Preview content (initially hidden)
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            // Clear existing preview content
            foreach (Transform child in _previewPanel)
            {
                if (child.name != "Background")
                    Destroy(child.gameObject);
            }

            if (_selectedBaseHero == null)
            {
                // Show instruction
                var instructionObj = new GameObject("Instruction", typeof(RectTransform), typeof(TextMeshProUGUI));
                instructionObj.transform.SetParent(_previewPanel, false);
                var instructionRT = instructionObj.GetComponent<RectTransform>();
                instructionRT.anchorMin = Vector2.zero;
                instructionRT.anchorMax = Vector2.one;
                instructionRT.offsetMin = Vector2.zero;
                instructionRT.offsetMax = Vector2.zero;

                var instructionText = instructionObj.GetComponent<TextMeshProUGUI>();
                instructionText.text = "Select a base hero and material heroes to see synthesis preview";
                instructionText.fontSize = 18;
                instructionText.color = Color.gray;
                instructionText.alignment = TextAlignmentOptions.Center;
                return;
            }

            // Calculate recipe
            var recipe = _synthesizerService.CalculateRecipe(_selectedBaseHero, _selectedMaterials);

            // Preview title
            var titleObj = new GameObject("PreviewTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(_previewPanel, false);
            var titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.85f);
            titleRT.anchorMax = new Vector2(1, 0.95f);
            titleRT.offsetMin = new Vector2(20, 0);
            titleRT.offsetMax = new Vector2(-20, 0);

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "Synthesis Result Preview";
            titleText.fontSize = 22;
            titleText.color = Color.cyan;

            // Current stats
            var currentObj = new GameObject("CurrentStats", typeof(RectTransform), typeof(TextMeshProUGUI));
            currentObj.transform.SetParent(_previewPanel, false);
            var currentRT = currentObj.GetComponent<RectTransform>();
            currentRT.anchorMin = new Vector2(0, 0.6f);
            currentRT.anchorMax = new Vector2(0.5f, 0.8f);
            currentRT.offsetMin = new Vector2(20, 0);
            currentRT.offsetMax = new Vector2(-10, 0);

            var currentText = currentObj.GetComponent<TextMeshProUGUI>();
            currentText.text = $"Current:\n" +
                             $"ATK: {_selectedBaseHero.Attack}\n" +
                             $"DEF: {_selectedBaseHero.Defense}\n" +
                             $"HP: {_selectedBaseHero.MaxHealth}";
            currentText.fontSize = 16;
            currentText.color = Color.white;

            // New stats
            var newObj = new GameObject("NewStats", typeof(RectTransform), typeof(TextMeshProUGUI));
            newObj.transform.SetParent(_previewPanel, false);
            var newRT = newObj.GetComponent<RectTransform>();
            newRT.anchorMin = new Vector2(0.5f, 0.6f);
            newRT.anchorMax = new Vector2(1, 0.8f);
            newRT.offsetMin = new Vector2(10, 0);
            newRT.offsetMax = new Vector2(-20, 0);

            int newAttack = Mathf.CeilToInt(_selectedBaseHero.Attack * recipe.StatMultiplier);
            int newDefense = Mathf.CeilToInt(_selectedBaseHero.Defense * recipe.StatMultiplier);
            int newHealth = Mathf.CeilToInt(_selectedBaseHero.MaxHealth * recipe.StatMultiplier);

            var newText = newObj.GetComponent<TextMeshProUGUI>();
            newText.text = $"After Synth:\n" +
                         $"ATK: {newAttack} (+{newAttack - _selectedBaseHero.Attack})\n" +
                         $"DEF: {newDefense} (+{newDefense - _selectedBaseHero.Defense})\n" +
                         $"HP: {newHealth} (+{newHealth - _selectedBaseHero.MaxHealth})";
            newText.fontSize = 16;
            newText.color = Color.green;

            // Morale penalty notice
            var moraleObj = new GameObject("MoralePenalty", typeof(RectTransform), typeof(TextMeshProUGUI));
            moraleObj.transform.SetParent(_previewPanel, false);
            var moraleRT = moraleObj.GetComponent<RectTransform>();
            moraleRT.anchorMin = new Vector2(0, 0.35f);
            moraleRT.anchorMax = new Vector2(1, 0.5f);
            moraleRT.offsetMin = new Vector2(20, 0);
            moraleRT.offsetMax = new Vector2(-20, 0);

            var moraleText = moraleObj.GetComponent<TextMeshProUGUI>();
            moraleText.text = $"Base Hero Morale: -{recipe.MoralePenalty}%";
            moraleText.fontSize = 16;
            moraleText.color = new Color(1f, 0.5f, 0.5f);

            // Success rate
            var rateObj = new GameObject("SuccessRatePreview", typeof(RectTransform), typeof(TextMeshProUGUI));
            rateObj.transform.SetParent(_previewPanel, false);
            var rateRT = rateObj.GetComponent<RectTransform>();
            rateRT.anchorMin = new Vector2(0, 0.2f);
            rateRT.anchorMax = new Vector2(1, 0.32f);
            rateRT.offsetMin = new Vector2(20, 0);
            rateRT.offsetMax = new Vector2(-20, 0);

            var rateText = rateObj.GetComponent<TextMeshProUGUI>();
            rateText.text = $"Success Rate: {recipe.SuccessRate:F0}%";
            rateText.fontSize = 24;
            rateText.color = GetSuccessRateColor(recipe.SuccessRate);

            // Star rank increase
            if (recipe.StarRankIncrease > 0)
            {
                var starsObj = new GameObject("StarIncrease", typeof(RectTransform), typeof(TextMeshProUGUI));
                starsObj.transform.SetParent(_previewPanel, false);
                var starsRT = starsObj.GetComponent<RectTransform>();
                starsRT.anchorMin = new Vector2(0, 0.05f);
                starsRT.anchorMax = new Vector2(1, 0.15f);
                starsRT.offsetMin = new Vector2(20, 0);
                starsRT.offsetMax = new Vector2(-20, 0);

                var starsText = starsObj.GetComponent<TextMeshProUGUI>();
                starsText.text = $"Star Rank: {_selectedBaseHero.StarRank} → {_selectedBaseHero.StarRank + recipe.StarRankIncrease} ★";
                starsText.fontSize = 20;
                starsText.color = Color.yellow;
            }
        }

        private Color GetSuccessRateColor(float rate)
        {
            if (rate >= 80f) return SuccessHighColor;
            if (rate >= 50f) return SuccessMediumColor;
            return SuccessLowColor;
        }

        private void CreateFooter()
        {
            var footerContainer = new GameObject("FooterContainer", typeof(RectTransform));
            footerContainer.transform.SetParent(FooterArea, false);

            var footerRT = footerContainer.GetComponent<RectTransform>();
            footerRT.anchorMin = Vector2.zero;
            footerRT.anchorMax = Vector2.one;
            footerRT.offsetMin = Vector2.zero;
            footerRT.offsetMax = Vector2.zero;

            // Synthesize button
            var synthBtnObj = new GameObject("SynthesizeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            synthBtnObj.transform.SetParent(footerRT, false);
            var synthBtnRT = synthBtnObj.GetComponent<RectTransform>();
            synthBtnRT.anchorMin = new Vector2(0.2f, 0.2f);
            synthBtnRT.anchorMax = new Vector2(0.8f, 0.8f);
            synthBtnRT.offsetMin = Vector2.zero;
            synthBtnRT.offsetMax = Vector2.zero;

            var synthBtnImage = synthBtnObj.GetComponent<Image>();
            synthBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f);

            var synthBtn = synthBtnObj.GetComponent<Button>();
            synthBtn.interactable = false;
            synthBtn.onClick.AddListener(OnSynthesizePressed);

            var synthBtnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            synthBtnTextObj.transform.SetParent(synthBtnObj.transform, false);
            var synthBtnTextRT = synthBtnTextObj.GetComponent<RectTransform>();
            synthBtnTextRT.anchorMin = Vector2.zero;
            synthBtnTextRT.anchorMax = Vector2.one;
            synthBtnTextRT.offsetMin = new Vector2(10, 10);
            synthBtnTextRT.offsetMax = new Vector2(-10, -10);

            var synthBtnText = synthBtnTextObj.GetComponent<TextMeshProUGUI>();
            synthBtnText.text = "Synthesize";
            synthBtnText.fontSize = 28;
            synthBtnText.color = Color.white;
            synthBtnText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateAnimationArea()
        {
            AnimationArea.gameObject.SetActive(false);
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
            UpdateBaseHeroSlot();
            UpdateMaterialSlots();
            UpdatePreview();
            UpdateSuccessRate();
            UpdateSynthButton();
        }

        private void UpdateBaseHeroSlot()
        {
            if (_baseHeroSlot == null) return;

            // Clear existing content
            foreach (Transform child in _baseHeroSlot)
            {
                if (child.name != "")
                    Destroy(child.gameObject);
            }

            if (_selectedBaseHero != null)
            {
                // Show selected hero info
                var nameObj = new GameObject("HeroName", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(_baseHeroSlot, false);
                var nameRT = nameObj.GetComponent<RectTransform>();
                nameRT.anchorMin = new Vector2(0.1f, 0.6f);
                nameRT.anchorMax = new Vector2(0.9f, 0.85f);
                nameRT.offsetMin = Vector2.zero;
                nameRT.offsetMax = Vector2.zero;

                var nameText = nameObj.GetComponent<TextMeshProUGUI>();
                nameText.text = _selectedBaseHero.HeroName;
                nameText.fontSize = 26;
                nameText.color = Color.white;

                var levelObj = new GameObject("HeroLevel", typeof(RectTransform), typeof(TextMeshProUGUI));
                levelObj.transform.SetParent(_baseHeroSlot, false);
                var levelRT = levelObj.GetComponent<RectTransform>();
                levelRT.anchorMin = new Vector2(0.1f, 0.35f);
                levelRT.anchorMax = new Vector2(0.9f, 0.55f);
                levelRT.offsetMin = Vector2.zero;
                levelRT.offsetMax = Vector2.zero;

                var levelText = levelObj.GetComponent<TextMeshProUGUI>();
                levelText.text = $"Lv.{_selectedBaseHero.Level} ★{_selectedBaseHero.StarRank}";
                levelText.fontSize = 22;
                levelText.color = Color.yellow;

                var statsObj = new GameObject("HeroStats", typeof(RectTransform), typeof(TextMeshProUGUI));
                statsObj.transform.SetParent(_baseHeroSlot, false);
                var statsRT = statsObj.GetComponent<RectTransform>();
                statsRT.anchorMin = new Vector2(0.1f, 0.1f);
                statsRT.anchorMax = new Vector2(0.9f, 0.3f);
                statsRT.offsetMin = Vector2.zero;
                statsRT.offsetMax = Vector2.zero;

                var statsText = statsObj.GetComponent<TextMeshProUGUI>();
                statsText.text = $"ATK: {_selectedBaseHero.Attack} | DEF: {_selectedBaseHero.Defense}";
                statsText.fontSize = 18;
                statsText.color = Color.cyan;

                // Update slot color
                _baseHeroSlot.GetComponent<Image>().color = SelectedSlotColor;
            }
            else
            {
                // Show placeholder
                var placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
                placeholderObj.transform.SetParent(_baseHeroSlot, false);
                var placeholderRT = placeholderObj.GetComponent<RectTransform>();
                placeholderRT.anchorMin = Vector2.zero;
                placeholderRT.anchorMax = Vector2.one;
                placeholderRT.offsetMin = Vector2.zero;
                placeholderRT.offsetMax = Vector2.zero;

                var placeholderText = placeholderObj.GetComponent<TextMeshProUGUI>();
                placeholderText.text = "Tap to Select Base Hero";
                placeholderText.fontSize = 24;
                placeholderText.color = Color.white;
                placeholderText.alignment = TextAlignmentOptions.Center;

                _baseHeroSlot.GetComponent<Image>().color = BaseHeroSlotColor;
            }
        }

        private void UpdateMaterialSlots()
        {
            for (int i = 0; i < _materialSlots.Count; i++)
            {
                var slot = _materialSlots[i];
                if (slot == null) continue;

                // Clear existing content
                foreach (Transform child in slot)
                {
                    if (child.name != "SlotNumber")
                        Destroy(child.gameObject);
                }

                if (i < _selectedMaterials.Count)
                {
                    var material = _selectedMaterials[i];

                    var nameObj = new GameObject("MaterialName", typeof(RectTransform), typeof(TextMeshProUGUI));
                    nameObj.transform.SetParent(slot, false);
                    var nameRT = nameObj.GetComponent<RectTransform>();
                    nameRT.anchorMin = new Vector2(0.1f, 0.5f);
                    nameRT.anchorMax = new Vector2(0.9f, 0.8f);
                    nameRT.offsetMin = Vector2.zero;
                    nameRT.offsetMax = Vector2.zero;

                    var nameText = nameObj.GetComponent<TextMeshProUGUI>();
                    nameText.text = material.HeroName;
                    nameText.fontSize = 16;
                    nameText.color = Color.white;

                    var levelObj = new GameObject("MaterialLevel", typeof(RectTransform), typeof(TextMeshProUGUI));
                    levelObj.transform.SetParent(slot, false);
                    var levelRT = levelObj.GetComponent<RectTransform>();
                    levelRT.anchorMin = new Vector2(0.1f, 0.2f);
                    levelRT.anchorMax = new Vector2(0.9f, 0.45f);
                    levelRT.offsetMin = Vector2.zero;
                    levelRT.offsetMax = Vector2.zero;

                    var levelText = levelObj.GetComponent<TextMeshProUGUI>();
                    levelText.text = $"Lv.{material.Level}";
                    levelText.fontSize = 14;
                    levelText.color = Color.gray;

                    slot.GetComponent<Image>().color = SelectedSlotColor;
                }
                else
                {
                    var emptyObj = new GameObject("EmptySlot", typeof(RectTransform), typeof(TextMeshProUGUI));
                    emptyObj.transform.SetParent(slot, false);
                    var emptyRT = emptyObj.GetComponent<RectTransform>();
                    emptyRT.anchorMin = Vector2.zero;
                    emptyRT.anchorMax = Vector2.one;
                    emptyRT.offsetMin = Vector2.zero;
                    emptyRT.offsetMax = Vector2.zero;

                    var emptyText = emptyObj.GetComponent<TextMeshProUGUI>();
                    emptyText.text = "Tap to Add";
                    emptyText.fontSize = 16;
                    emptyText.color = Color.gray;
                    emptyText.alignment = TextAlignmentOptions.Center;

                    slot.GetComponent<Image>().color = MaterialSlotColor;
                }
            }
        }

        private void UpdateSuccessRate()
        {
            if (_selectedBaseHero == null || _selectedMaterials.Count == 0)
            {
                SuccessRateText.text = "Select Heroes";
                SuccessRateText.color = Color.gray;
                return;
            }

            var recipe = _synthesizerService.CalculateRecipe(_selectedBaseHero, _selectedMaterials);
            SuccessRateText.text = $"Success: {recipe.SuccessRate:F0}%";
            SuccessRateText.color = GetSuccessRateColor(recipe.SuccessRate);
        }

        private void UpdateSynthButton()
        {
            var synthBtn = FooterArea.Find("FooterContainer/SynthesizeButton")?.GetComponent<Button>();
            if (synthBtn != null)
            {
                synthBtn.interactable = _selectedBaseHero != null && _selectedMaterials.Count > 0 && !_isAnimating;
            }
        }

        private void OnBaseHeroSlotClicked()
        {
            ShowHeroSelector(true);
        }

        private void OnMaterialSlotClicked(int slotIndex)
        {
            if (slotIndex < _selectedMaterials.Count)
            {
                // Remove material at this slot
                _selectedMaterials.RemoveAt(slotIndex);
            }
            else
            {
                // Add material
                ShowHeroSelector(false);
            }
            RefreshUI();
        }

        private void ShowHeroSelector(bool isBaseHero)
        {
            // Simplified: select first available hero
            // In a real implementation, this would show a modal with hero grid
            var availableHeroes = _availableHeroes.FindAll(h =>
            {
                if (isBaseHero)
                {
                    return h != null;
                }
                else
                {
                    // For materials, exclude the base hero
                    return h != null && h != _selectedBaseHero && !_selectedMaterials.Contains(h);
                }
            });

            if (availableHeroes.Count > 0)
            {
                if (isBaseHero)
                {
                    _selectedBaseHero = availableHeroes[0];
                    // Remove base from material options
                    _selectedMaterials.RemoveAll(m => m == _selectedBaseHero);
                }
                else
                {
                    _selectedMaterials.Add(availableHeroes[0]);
                }
                RefreshUI();
            }
        }

        private void OnSynthesizePressed()
        {
            if (_selectedBaseHero == null || _selectedMaterials.Count == 0)
                return;

            if (_isAnimating)
                return;

            _isAnimating = true;

            // Start synthesis animation
            StartCoroutine(PlaySynthesisAnimation());
        }

        private System.Collections.IEnumerator PlaySynthesisAnimation()
        {
            // Show animation area
            AnimationArea.gameObject.SetActive(true);

            // Flash effect
            for (int i = 0; i < 3; i++)
            {
                AnimationArea.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(0.2f);
                AnimationArea.GetComponent<Image>().color = new Color(0.5f, 0.5f, 1, 0.3f);
                yield return new WaitForSeconds(0.2f);
            }

            // Execute synthesis
            var result = _synthesizerService.ExecuteSynthesis(_selectedBaseHero, _selectedMaterials);

            // Hide animation area
            AnimationArea.gameObject.SetActive(false);

            // Show result
            ShowSynthesisResult(result);
        }

        private void ShowSynthesisResult(SynthesisResult result)
        {
            _isAnimating = false;

            if (result.Success)
            {
                Debug.Log($"Synthesis successful! New stats: ATK={result.NewAttack}, DEF={result.NewDefense}");
            }
            else
            {
                Debug.Log($"Synthesis failed: {result.FailureReason}");
            }

            // Reset selection
            _selectedBaseHero = null;
            _selectedMaterials.Clear();
            RefreshUI();
        }

        private void OnBackPressed()
        {
            if (_isAnimating)
                return;

            _gameStateService.LoadScene("Hub");
        }

        private void OnEnable()
        {
            LoadAvailableHeroes();
            RefreshUI();
        }
    }
}