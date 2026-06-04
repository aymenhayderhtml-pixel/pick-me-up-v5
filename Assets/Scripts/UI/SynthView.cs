using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SynthView : MonoBehaviour
{
    private IRosterService _rosterService;
    private ISynthesizerService _synthService;
    private ICurrencyService _currencyService;

    private HeroInstance _selectedBase;
    private List<HeroInstance> _selectedMaterials = new List<HeroInstance>();
    private bool _bonusEventActive = false;
    private int _maxMaterials;

    private Sprite _whiteSprite;

    private Image _ring1;
    private Image _ring2;
    private Image _innerRingImage;
    private Coroutine _glowRoutine;

    private Transform _ritualCenter;
    private GameObject _baseCardSlot;
    private List<GameObject> _materialSlots = new List<GameObject>();

    private TextMeshProUGUI _previewName;
    private TextMeshProUGUI _previewStars;
    private TextMeshProUGUI _previewStarArrow;
    private TextMeshProUGUI _previewPromoArrow;
    private TextMeshProUGUI _previewAtk;
    private TextMeshProUGUI _previewHp;
    private TextMeshProUGUI _previewSuccessRate;
    private TextMeshProUGUI _previewCost;
    private TextMeshProUGUI _previewMatCount;
    private TextMeshProUGUI _previewHint;
    private Image _previewBg;

    private TextMeshProUGUI _synthBtnText;

    private TextMeshProUGUI _topbarGold;
    private TextMeshProUGUI _topbarGems;
    private TextMeshProUGUI _topbarStones;

    private GameObject _selectionModal;
    private Transform _modalContent;
    private TextMeshProUGUI _modalTitle;
    private bool _modalIsBase;

    public class HeroCardUI
    {
        public GameObject Root;
        public Image Background;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI StarsText;
        public TextMeshProUGUI PromoText;
        public TextMeshProUGUI LockedText;
        public HeroInstance Instance;
        public bool IsBase;
    }

    private void Start()
    {
        _rosterService = ServiceRegistry.Instance.Resolve<IRosterService>();
        _synthService = ServiceRegistry.Instance.Resolve<ISynthesizerService>();
        _currencyService = ServiceRegistry.Instance.Resolve<ICurrencyService>();

        _maxMaterials = _synthService.GetMaxMaterialCount();

        CreateWhiteSprite();
        SetupCanvas();
        BuildUI();
        UpdatePreview();
        UpdateCurrencies();
    }

    private void OnEnable()
    {
        if (_rosterService != null)
        {
            UpdatePreview();
            UpdateCurrencies();
        }
    }

    private void Update()
    {
        if (_ring1 != null) _ring1.transform.Rotate(0, 0, 15f * Time.deltaTime);
        if (_ring2 != null) _ring2.transform.Rotate(0, 0, -10f * Time.deltaTime);
        if (_innerRingImage != null) _innerRingImage.transform.Rotate(0, 0, 20f * Time.deltaTime);
    }

    private void CreateWhiteSprite()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float outerR = center;
        float innerR = center - 12f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                bool onRing = dist <= outerR && dist >= innerR;
                tex.SetPixel(x, y, onRing ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        _whiteSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void SetupCanvas()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2400, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = gameObject.GetComponent<GraphicRaycaster>();
        if (raycaster == null) raycaster = gameObject.AddComponent<GraphicRaycaster>();
    }

    private void BuildUI()
    {
        CreateImage("ScreenBg", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.05f, 0.07f, 0.11f, 1f));

        GameObject topbar = CreateImage("Topbar", transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -120), Vector2.zero, new Color(0.08f, 0.10f, 0.16f, 1f));
        
        Button backBtn = CreateButton("BackBtn", topbar.transform, new Vector2(0, 0), new Vector2(0.15f, 1), new Vector2(10, 10), new Vector2(-10, -10), new Color(0.14f, 0.20f, 0.40f, 1f));
        CreateTMP("BackText", "< Back", 24, backBtn.transform, Vector2.zero, Vector2.one);
        backBtn.onClick.AddListener(() => SceneManager.LoadScene("Hub"));

        CreateTMP("Title", "SYNTHESIS LAB", 32, topbar.transform, new Vector2(0.15f, 0), new Vector2(0.6f, 1));

        _topbarGold = CreateTMP("TopGold", "Gold: 0", 20, topbar.transform, new Vector2(0.6f, 0), new Vector2(0.73f, 1)).GetComponent<TextMeshProUGUI>();
        _topbarGems = CreateTMP("TopGems", "Gems: 0", 20, topbar.transform, new Vector2(0.73f, 0), new Vector2(0.86f, 1)).GetComponent<TextMeshProUGUI>();
        _topbarStones = CreateTMP("TopStones", "Stones: 0", 20, topbar.transform, new Vector2(0.86f, 0), new Vector2(1, 1)).GetComponent<TextMeshProUGUI>();

        GameObject bottomBar = CreateImage("BottomBar", transform, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 120), new Color(0.08f, 0.10f, 0.16f, 1f));

        Button synthBtn = CreateButton("SynthBtn", bottomBar.transform, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.55f, 0.25f, 1f));
        _synthBtnText = CreateTMP("SynthText", "SYNTHESIZE", 36, synthBtn.transform, Vector2.zero, Vector2.one).GetComponent<TextMeshProUGUI>();
        synthBtn.onClick.AddListener(() => StartCoroutine(ExecuteSynthesisOnce()));

        GameObject leftPanel = CreateImage("LeftPanel", transform, new Vector2(0, 0), new Vector2(0.52f, 1), new Vector2(0, 150), new Vector2(0, -120), new Color(0.04f, 0.06f, 0.10f, 1f));
        BuildRitualArea(leftPanel.transform);

        GameObject rightPanel = CreateImage("RightPanel", transform, new Vector2(0.52f, 0), new Vector2(1, 1), new Vector2(0, 150), new Vector2(0, -120), new Color(0.07f, 0.09f, 0.14f, 1f));
        BuildPreviewArea(rightPanel.transform);
    }

    private void BuildRitualArea(Transform parent)
    {
        _ritualCenter = new GameObject("RitualCenter", typeof(RectTransform)).transform;
        _ritualCenter.SetParent(parent, false);
        RectTransform centerRt = _ritualCenter.GetComponent<RectTransform>();
        centerRt.anchorMin = new Vector2(0.5f, 0.5f);
        centerRt.anchorMax = new Vector2(0.5f, 0.5f);
        centerRt.pivot = new Vector2(0.5f, 0.5f);
        centerRt.sizeDelta = Vector2.zero;
        centerRt.offsetMin = Vector2.zero;
        centerRt.offsetMax = Vector2.zero;

        _ring1 = CreateRing("Ring1", _ritualCenter, 440f, new Color(0.20f, 0.30f, 0.55f, 0.6f));
        _ring2 = CreateRing("Ring2", _ritualCenter, 320f, new Color(0.25f, 0.40f, 0.70f, 0.5f));
        _innerRingImage = CreateRing("Ring3", _ritualCenter, 200f, new Color(0.30f, 0.50f, 0.85f, 0.4f));

        _baseCardSlot = CreateSlot("BaseSlot", _ritualCenter, Vector2.zero, new Vector2(160, 190), true);
        Button baseSlotBtn = _baseCardSlot.AddComponent<Button>();
        baseSlotBtn.transition = Selectable.Transition.None;
        baseSlotBtn.targetGraphic = _baseCardSlot.GetComponent<Image>();
        baseSlotBtn.onClick.AddListener(() => ShowSelectionModal(true));

        for (int i = 0; i < _maxMaterials; i++)
        {
            float angle = (360f / _maxMaterials) * i - 90f;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(rad) * 200f, Mathf.Sin(rad) * 200f);
            GameObject slot = CreateSlot("MatSlot_" + i, _ritualCenter, pos, new Vector2(110, 130), false);
            Button slotBtn = slot.AddComponent<Button>();
            slotBtn.transition = Selectable.Transition.None;
            slotBtn.targetGraphic = slot.GetComponent<Image>();
            int capturedIndex = i;
            slotBtn.onClick.AddListener(() => ShowSelectionModal(false));
            _materialSlots.Add(slot);
        }
    }

    private Image CreateRing(string name, Transform parent, float size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        Image img = go.GetComponent<Image>();
        img.sprite = _whiteSprite;
        img.color = color;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillAmount = 1f;
        img.raycastTarget = false;
        return img;
    }

    private GameObject CreateSlot(string name, Transform parent, Vector2 anchoredPos, Vector2 size, bool isBase)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.15f, 0.20f, 0.35f, 0.8f);
        img.raycastTarget = true;

        CreateTMP("SlotText", isBase ? "BASE" : "+", isBase ? 30 : 36, go.transform, new Vector2(0, 0.4f), new Vector2(1, 1));
        CreateTMP("SlotSubText", "", isBase ? 22 : 18, go.transform, new Vector2(0, 0), new Vector2(1, 0.4f));

        return go;
    }

    private void BuildPreviewArea(Transform parent)
    {
        _previewBg = CreateImage("PreviewBg", parent, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.14f, 0.22f, 1f)).GetComponent<Image>();

        _previewName = CreateTMP("PreviewName", "Select a hero to begin", 28, parent, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.95f)).GetComponent<TextMeshProUGUI>();
        _previewStars = CreateTMP("PreviewStars", "", 22, parent, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.85f)).GetComponent<TextMeshProUGUI>();
        _previewStars.color = new Color(1f, 0.85f, 0.1f, 1f);
        
        _previewStarArrow = CreateTMP("PreviewStarArrow", "", 20, parent, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.78f)).GetComponent<TextMeshProUGUI>();
        _previewPromoArrow = CreateTMP("PreviewPromoArrow", "", 20, parent, new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.72f)).GetComponent<TextMeshProUGUI>();

        CreateImage("Divider1", parent, new Vector2(0.1f, 0.64f), new Vector2(0.9f, 0.645f), Vector2.zero, Vector2.zero, new Color(0.20f, 0.25f, 0.40f, 1f));

        _previewAtk = CreateTMP("PreviewAtk", "", 20, parent, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.64f)).GetComponent<TextMeshProUGUI>();
        _previewHp = CreateTMP("PreviewHp", "", 20, parent, new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.58f)).GetComponent<TextMeshProUGUI>();

        CreateImage("Divider2", parent, new Vector2(0.1f, 0.50f), new Vector2(0.9f, 0.505f), Vector2.zero, Vector2.zero, new Color(0.20f, 0.25f, 0.40f, 1f));

        _previewSuccessRate = CreateTMP("PreviewSuccess", "0%", 52, parent, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.50f)).GetComponent<TextMeshProUGUI>();
        
        _previewCost = CreateTMP("PreviewCost", "Cost: 0 Gold", 20, parent, new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.35f)).GetComponent<TextMeshProUGUI>();
        _previewMatCount = CreateTMP("PreviewMatCount", "Materials: 0 / 0", 20, parent, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.28f)).GetComponent<TextMeshProUGUI>();
        
        _previewHint = CreateTMP("PreviewHint", "Requires duplicates of same hero", 16, parent, new Vector2(0.05f, 0.16f), new Vector2(0.95f, 0.22f)).GetComponent<TextMeshProUGUI>();
        _previewHint.color = new Color(0.55f, 0.60f, 0.70f, 1f);

        Button autoFillBtn = CreateButton("AutoFillBtn", parent, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.12f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.20f, 0.40f, 1f));
        CreateTMP("AutoFillText", "AUTO-FILL DUPES", 22, autoFillBtn.transform, Vector2.zero, Vector2.one);
        autoFillBtn.onClick.AddListener(AutoFillDupes);
    }

    private void ShowSelectionModal(bool isBase)
    {
        if (_selectionModal != null) Destroy(_selectionModal);
        _modalIsBase = isBase;

        _selectionModal = new GameObject("SelectionModal", typeof(RectTransform), typeof(Image));
        _selectionModal.transform.SetParent(transform, false);
        RectTransform modalRt = _selectionModal.GetComponent<RectTransform>();
        modalRt.anchorMin = Vector2.zero;
        modalRt.anchorMax = Vector2.one;
        modalRt.offsetMin = Vector2.zero;
        modalRt.offsetMax = Vector2.zero;
        _selectionModal.GetComponent<Image>().color = new Color(0.00f, 0.00f, 0.00f, 0.85f);

        CreateTMP("ModalTitle", isBase ? "SELECT BASE HERO" : "SELECT MATERIALS  (tap to toggle)", 30, _selectionModal.transform, new Vector2(0, 0.9f), new Vector2(1, 1));

        ScrollRect scroll = CreateScrollView("ModalScroll", _selectionModal.transform, new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.88f));
        _modalContent = scroll.content;

        if (!isBase)
        {
            Button confirmBtn = CreateButton("ConfirmBtn", _selectionModal.transform, new Vector2(0.35f, 0.10f), new Vector2(0.95f, 0.17f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.55f, 0.25f, 1f));
            CreateTMP("ConfirmText", "CONFIRM", 26, confirmBtn.transform, Vector2.zero, Vector2.one);
            confirmBtn.onClick.AddListener(() => {
                Destroy(_selectionModal);
                UpdateRitualVisuals();
                UpdatePreview();
                UpdateGlow();
            });
        }

        Button closeBtn = CreateButton("CloseBtn", _selectionModal.transform, new Vector2(0.05f, 0.10f), new Vector2(isBase ? 0.95f : 0.32f, 0.17f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.20f, 0.40f, 1f));
        CreateTMP("CloseText", "CLOSE", 26, closeBtn.transform, Vector2.zero, Vector2.one);
        closeBtn.onClick.AddListener(() => {
            Destroy(_selectionModal);
            UpdateRitualVisuals();
            UpdatePreview();
            UpdateGlow();
        });

        PopulateModalCards();
    }

    private void PopulateModalCards()
    {
        foreach (Transform child in _modalContent) Destroy(child.gameObject);

        var heroes = _rosterService.GetAlive();
        foreach (var hero in heroes)
        {
            if (_modalIsBase && _selectedMaterials.Any(m => m.InstanceId == hero.InstanceId)) continue;
            if (!_modalIsBase && _selectedBase != null && hero.InstanceId == _selectedBase.InstanceId) continue;

            HeroCardUI card = CreateModalCard(hero);
            CardInteractor interactor = card.Root.AddComponent<CardInteractor>();
            interactor.Card = card;
            interactor.View = this;
            interactor.IsModal = true;
        }
    }

    private HeroCardUI CreateModalCard(HeroInstance hero)
    {
        HeroCardUI card = new HeroCardUI();
        card.Instance = hero;

        card.Root = new GameObject("Card_" + hero.InstanceId, typeof(RectTransform), typeof(Image));
        card.Root.transform.SetParent(_modalContent, false);
        RectTransform rt = card.Root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 200);
        
        card.Background = card.Root.GetComponent<Image>();
        card.Background.color = HeroColorUtility.GetRarityColor(hero.CurrentStarRank);
        card.Background.raycastTarget = true;

        HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(hero.HeroDefId);
        string heroName = def != null ? def.HeroName : hero.HeroDefId;

        card.NameText = CreateTMP("Name", heroName, 18, card.Root.transform, new Vector2(0, 0.75f), new Vector2(1, 0.95f)).GetComponent<TextMeshProUGUI>();
        
        string stars = new string('*', hero.CurrentStarRank);
        card.StarsText = CreateTMP("Stars", stars, 20, card.Root.transform, new Vector2(0, 0.55f), new Vector2(1, 0.75f)).GetComponent<TextMeshProUGUI>();
        card.StarsText.color = new Color(1f, 0.85f, 0.1f, 1f);

        card.PromoText = CreateTMP("Promo", "P" + hero.PromotionRank, 14, card.Root.transform, new Vector2(0, 0.40f), new Vector2(1, 0.55f)).GetComponent<TextMeshProUGUI>();

        card.LockedText = CreateTMP("Locked", hero.IsLocked ? "[LOCKED]" : "", 14, card.Root.transform, new Vector2(0, 0.1f), new Vector2(1, 0.3f)).GetComponent<TextMeshProUGUI>();
        card.LockedText.color = Color.red;

        return card;
    }

    public void HandleModalSelection(HeroCardUI card)
    {
        if (card.Instance.IsLocked) return;

        if (_modalIsBase)
        {
            _selectedBase = card.Instance;
            _selectedMaterials.Clear();
            Destroy(_selectionModal);
            UpdateRitualVisuals();
            UpdatePreview();
            UpdateGlow();
        }
        else
        {
            var existing = _selectedMaterials.FirstOrDefault(m => m.InstanceId == card.Instance.InstanceId);
            if (existing != null)
            {
                _selectedMaterials.Remove(existing);
            }
            else if (_selectedMaterials.Count < _maxMaterials)
            {
                _selectedMaterials.Add(card.Instance);
            }
            RefreshModalCardVisuals();
            UpdateGlow();
        }
    }

    private void RefreshModalCardVisuals()
    {
        if (_modalContent == null) return;
        foreach (Transform child in _modalContent)
        {
            CardInteractor ci = child.GetComponent<CardInteractor>();
            if (ci == null) continue;
            Image bg = child.GetComponent<Image>();
            bool isSelected = _selectedMaterials.Any(m => m.InstanceId == ci.Card.Instance.InstanceId);
            bg.color = isSelected
                ? Color.Lerp(HeroColorUtility.GetRarityColor(ci.Card.Instance.CurrentStarRank), Color.white, 0.3f)
                : HeroColorUtility.GetRarityColor(ci.Card.Instance.CurrentStarRank);
        }
    }

    private void UpdateRitualVisuals()
    {
        UpdateSlotVisual(_baseCardSlot, _selectedBase, true);

        for (int i = 0; i < _maxMaterials; i++)
        {
            HeroInstance mat = i < _selectedMaterials.Count ? _selectedMaterials[i] : null;
            UpdateSlotVisual(_materialSlots[i], mat, false);
        }
    }

    private void UpdateSlotVisual(GameObject slot, HeroInstance hero, bool isBase)
    {
        Image bg = slot.GetComponent<Image>();
        TextMeshProUGUI[] texts = slot.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI mainText = texts.Length > 0 ? texts[0] : null;
        TextMeshProUGUI subText = texts.Length > 1 ? texts[1] : null;

        if (hero != null)
        {
            Color rarityCol = HeroColorUtility.GetRarityColor(hero.CurrentStarRank);
            bg.color = rarityCol;
            HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(hero.HeroDefId);
            string heroName = def != null ? def.HeroName : hero.HeroDefId;
            if (mainText != null)
            {
                mainText.text = heroName;
                mainText.fontSize = isBase ? 22 : 15;
                mainText.color = Color.white;
            }
            if (subText != null)
            {
                subText.text = new string('*', hero.CurrentStarRank);
                subText.color = new Color(1f, 0.85f, 0.1f, 1f);
                subText.fontSize = isBase ? 20 : 16;
            }
        }
        else
        {
            bg.color = new Color(0.15f, 0.20f, 0.35f, 0.8f);
            if (mainText != null)
            {
                mainText.text = isBase ? "BASE" : "+";
                mainText.fontSize = isBase ? 30 : 36;
                mainText.color = new Color(0.40f, 0.55f, 0.80f, 0.8f);
            }
            if (subText != null)
            {
                subText.text = "";
            }
        }
    }

    private void UpdateGlow()
    {
        if (_glowRoutine != null) StopCoroutine(_glowRoutine);
        _glowRoutine = StartCoroutine(GlowCoroutine(_selectedMaterials.Count > 0));
    }

    private IEnumerator GlowCoroutine(bool active)
    {
        float targetAlpha = active ? 0.9f : 0.4f;
        float currentAlpha = _innerRingImage.color.a;
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(currentAlpha, targetAlpha, t / 0.5f);
            Color c = _innerRingImage.color;
            c.a = a;
            _innerRingImage.color = c;
            yield return null;
        }
    }

    private void UpdatePreview()
    {
        if (_selectedBase == null)
        {
            _previewName.text = "Select a hero to begin";
            _previewName.color = Color.white;
            _previewStars.text = "";
            _previewStarArrow.text = "";
            _previewPromoArrow.text = "";
            _previewAtk.text = "";
            _previewHp.text = "";
            _previewSuccessRate.text = "0%";
            _previewSuccessRate.color = Color.white;
            _previewCost.text = "Cost: 0 Gold";
            _previewMatCount.text = "Materials: 0 / " + _maxMaterials;
            _previewHint.text = "Requires duplicates of same hero";
            _synthBtnText.text = "SYNTHESIZE";
            return;
        }

        HeroDefinition def = HeroPresentationUtility.LoadHeroDefinition(_selectedBase.HeroDefId);
        if (def == null) return;

        _previewName.text = def.HeroName;
        _previewName.color = HeroColorUtility.GetRarityColor(_selectedBase.CurrentStarRank);
        _previewStars.text = new string('*', _selectedBase.CurrentStarRank);

        var recipe = _synthService.CalculateRecipe(_selectedBase, _selectedMaterials);
        if (recipe != null)
        {
            int displayStarIncrease = recipe.StarRankIncrease;
            if (displayStarIncrease == 0 && _selectedMaterials.Count > 0) displayStarIncrease = 1;
            int newStar = _selectedBase.CurrentStarRank + displayStarIncrease;
            int newPromo = _selectedBase.PromotionRank + (recipe.PromotionRankIncrease == 0 && _selectedMaterials.Count > 0 ? 1 : recipe.PromotionRankIncrease);

            _previewStarArrow.text = "*" + _selectedBase.CurrentStarRank + " -> *" + newStar;
            _previewPromoArrow.text = "P" + _selectedBase.PromotionRank + " -> P" + newPromo;

            float starMultCurrent = 1f + (_selectedBase.CurrentStarRank - 1) * 0.3f;
            float starMultNew = 1f + (newStar - 1) * 0.3f;

            int currentATK = Mathf.RoundToInt(def.BaseATK * starMultCurrent);
            int newATK = Mathf.RoundToInt(def.BaseATK * starMultNew);
            _previewAtk.text = "ATK: " + currentATK + " -> " + newATK;
            _previewAtk.color = newATK > currentATK ? new Color(0.2f, 0.85f, 0.3f, 1f) : Color.white;

            int currentHP = Mathf.RoundToInt(def.BaseHP * starMultCurrent);
            int newHP = Mathf.RoundToInt(def.BaseHP * starMultNew);
            _previewHp.text = "HP: " + currentHP + " -> " + newHP;
            _previewHp.color = newHP > currentHP ? new Color(0.2f, 0.85f, 0.3f, 1f) : Color.white;

            float successRate = recipe.SuccessRate;
            if (_bonusEventActive) successRate *= 1.10f;
            successRate = Mathf.Clamp(successRate, 0f, 100f);

            _previewSuccessRate.text = successRate.ToString("F1") + "%";
            if (successRate >= 80f) _previewSuccessRate.color = new Color(0.2f, 0.85f, 0.3f, 1f);
            else if (successRate >= 50f) _previewSuccessRate.color = new Color(0.9f, 0.75f, 0.1f, 1f);
            else _previewSuccessRate.color = new Color(0.85f, 0.2f, 0.2f, 1f);

            _previewCost.text = "Cost: " + recipe.RequiredGold + " Gold";
            _previewMatCount.text = "Materials: " + _selectedMaterials.Count + " / " + _maxMaterials;
            
            int dupesNeeded = Mathf.Max(0, _maxMaterials - _selectedMaterials.Count);
            _previewHint.text = dupesNeeded > 0 ? "Requires " + dupesNeeded + " duplicates of same hero" : "Ready to synthesize!";

            _synthBtnText.text = successRate >= 100f ? "SYNTHESIZE (Guaranteed)" : "SYNTHESIZE";
        }
        else
        {
            _previewStarArrow.text = "";
            _previewPromoArrow.text = "";
            _previewAtk.text = "";
            _previewHp.text = "";
            _previewSuccessRate.text = "0%";
            _previewSuccessRate.color = Color.white;
            _previewCost.text = "Cost: 0 Gold";
            _previewMatCount.text = "Materials: " + _selectedMaterials.Count + " / " + _maxMaterials;
            _previewHint.text = "Requires duplicates of same hero";
            _synthBtnText.text = "SYNTHESIZE";
        }
        UpdateCurrencies();
    }

    private void UpdateCurrencies()
    {
        _topbarGold.text = "Gold: " + _currencyService.GetGold();
        _topbarGems.text = "Gems: " + _currencyService.GetGems();
        _topbarStones.text = "Stones: " + _currencyService.GetAttributeStones();
    }

    private void AutoFillDupes()
    {
        if (_selectedBase == null) return;
        _selectedMaterials.Clear();
        var heroes = _rosterService.GetAlive();
        foreach (var hero in heroes)
        {
            if (hero.InstanceId == _selectedBase.InstanceId) continue;
            if (hero.IsLocked) continue;
            if (hero.HeroDefId == _selectedBase.HeroDefId)
            {
                _selectedMaterials.Add(hero);
                if (_selectedMaterials.Count >= _maxMaterials) break;
            }
        }
        UpdateRitualVisuals();
        UpdatePreview();
        UpdateGlow();
    }

    private IEnumerator ExecuteSynthesisOnce()
    {
        if (_selectedBase == null) yield break;
        if (!_synthService.CanSynthesize(_selectedBase, _selectedMaterials)) yield break;
        var recipe = _synthService.CalculateRecipe(_selectedBase, _selectedMaterials);
        if (recipe == null) yield break;
        if (_currencyService.GetGold() < recipe.RequiredGold) yield break;

        var result = _synthService.ExecuteSynthesis(_selectedBase, _selectedMaterials);

        yield return new WaitForSeconds(0.3f);

        _selectedBase = _rosterService.GetAlive()
            .FirstOrDefault(h => h.InstanceId == _selectedBase?.InstanceId);

        _selectedMaterials.Clear();
        UpdateRitualVisuals();
        UpdatePreview();
        UpdateGlow();

        if (result.Success) FlashPreview(new Color(0.1f, 0.7f, 0.2f, 1f));
        else FlashPreview(new Color(0.7f, 0.1f, 0.1f, 1f));

        ShowResultSummary(result.Success ? 1 : 0, result.Success ? 0 : 1);
    }

    private void FlashPreview(Color color)
    {
        StartCoroutine(FlashPreviewCoroutine(color));
    }

    private IEnumerator FlashPreviewCoroutine(Color color)
    {
        Color original = _previewBg.color;
        _previewBg.color = color;
        yield return new WaitForSeconds(1.2f);
        _previewBg.color = original;
    }

    private void ShowResultSummary(int successes, int failures)
    {
        GameObject overlay = new GameObject("ResultOverlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(transform, false);
        RectTransform rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        Image img = overlay.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.85f);

        CreateTMP("SummaryTitle", "Synthesis Complete!", 36, overlay.transform, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.75f));

        TextMeshProUGUI successText = CreateTMP("SuccessText", "Successes: " + successes, 28, overlay.transform, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.6f)).GetComponent<TextMeshProUGUI>();
        successText.color = new Color(0.2f, 0.85f, 0.3f, 1f);

        TextMeshProUGUI failText = CreateTMP("FailText", "Failures: " + failures, 28, overlay.transform, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.45f)).GetComponent<TextMeshProUGUI>();
        failText.color = new Color(0.85f, 0.2f, 0.2f, 1f);

        Button closeBtn = CreateButton("CloseBtn", overlay.transform, new Vector2(0.3f, 0.15f), new Vector2(0.7f, 0.25f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.20f, 0.40f, 1f));
        CreateTMP("CloseText", "OK", 30, closeBtn.transform, Vector2.zero, Vector2.one);
        closeBtn.onClick.AddListener(() => Destroy(overlay));
    }

    public void ActivateBonusEvent()
    {
        _bonusEventActive = true;
        UpdatePreview();
    }

    private ScrollRect CreateScrollView(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject scrollObj = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(parent, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = anchorMin;
        scrollRt.anchorMax = anchorMax;
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = Vector2.zero;
        scrollObj.GetComponent<Image>().color = Color.clear;
        scrollObj.GetComponent<Image>().raycastTarget = true;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = Color.white;
        viewport.GetComponent<Image>().raycastTarget = true;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(180, 200);
        grid.spacing = new Vector2(8, 8);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = content.GetComponent<RectTransform>();
        sr.viewport = viewport.GetComponent<RectTransform>();
        sr.vertical = true;
        sr.horizontal = false;
        sr.scrollSensitivity = 30f;
        return sr;
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
        tmp.raycastTarget = false;
        return go;
    }

    private GameObject CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        Image img = go.GetComponent<Image>();
        img.color = color;
        Button btn = go.GetComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.targetGraphic = img;
        return btn;
    }
}

public class CardInteractor : MonoBehaviour, IPointerClickHandler
{
    public SynthView.HeroCardUI Card;
    public SynthView View;
    public bool IsModal;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsModal)
        {
            View.HandleModalSelection(Card);
        }
    }
}