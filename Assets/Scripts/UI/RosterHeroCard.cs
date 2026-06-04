using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A single hero card in the Roster grid.
/// Builds its own child UI in Awake(). No [SerializeField].
/// Call Setup() to populate with hero data and wire tap callback.
/// </summary>
public class RosterHeroCard : MonoBehaviour
{
    // ── Colors ──────────────────────────────────────────────────────────
    private static readonly Color CardFillColor = new Color(0.102f, 0.118f, 0.180f);    // #1A1E2E
    private static readonly Color BorderColor = new Color(0.40f, 0.45f, 0.60f, 0.4f);
    private static readonly Color AccentGold = new Color(0.784f, 0.659f, 0.294f);        // #C8A84B
    private static readonly Color SteelBlue = new Color(0.478f, 0.690f, 0.800f);         // #7AB0CC
    private static readonly Color Parchment = new Color(0.929f, 0.878f, 0.769f);         // #EDE0C4
    private static readonly Color DeadOverlayColor = new Color(0f, 0f, 0f, 0.65f);
    private static readonly Color FallenRed = new Color(0.85f, 0.20f, 0.20f);

    // ── Internal refs ───────────────────────────────────────────────────
    private Image _rootImage;
    private Image _portraitImage;
    private TextMeshProUGUI _nameLabel;
    private TextMeshProUGUI _classLabel;
    private TextMeshProUGUI _starsLabel;
    private GameObject _deadOverlay;
    private TextMeshProUGUI _deadText;
    private Button _cardButton;
    private Image _borderImage;
    private Image _innerImage;

    private HeroInstance _hero;
    private HeroDefinition _def;
    private System.Action<HeroInstance> _onTap;

    // ════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        // ── Root card fill ──────────────────────────────────────────────
        _rootImage = gameObject.AddComponent<Image>();
        _rootImage.color = CardFillColor;

        _cardButton = gameObject.AddComponent<Button>();
        _cardButton.transition = Selectable.Transition.None;
        _cardButton.targetGraphic = _rootImage;

        // ── Border (stretched full, sits behind content visually via color) ──
        GameObject border = CreateChild("Border", transform);
        _borderImage = border.AddComponent<Image>();
        _borderImage.color = new Color(0.251f, 0.271f, 0.439f, 0.4f); // #404570 at alpha 0.4
        _borderImage.raycastTarget = false;
        Stretch(border);

        // ── Inner fill (slightly inset to show border) ──────────────────
        GameObject inner = CreateChild("InnerFill", transform);
        _innerImage = inner.AddComponent<Image>();
        _innerImage.color = CardFillColor;
        _innerImage.raycastTarget = false;
        RectTransform innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(2f, 2f);
        innerRt.offsetMax = new Vector2(-2f, -2f);

        // ── Portrait area (top 58%) ─────────────────────────────────────
        GameObject portraitObj = CreateChild("Portrait", inner.transform);
        _portraitImage = portraitObj.AddComponent<Image>();
        _portraitImage.preserveAspect = true;
        _portraitImage.raycastTarget = false;
        RectTransform portraitRt = portraitObj.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0f, 0.42f);
        portraitRt.anchorMax = Vector2.one;
        portraitRt.offsetMin = new Vector2(4f, 4f);
        portraitRt.offsetMax = new Vector2(-4f, -4f);

        // ── Name label (below portrait) ─────────────────────────────────
        GameObject nameObj = CreateChild("NameLabel", inner.transform);
        _nameLabel = nameObj.AddComponent<TextMeshProUGUI>();
        _nameLabel.fontSize = 22;
        _nameLabel.fontStyle = FontStyles.Bold;
        _nameLabel.color = Parchment;
        _nameLabel.alignment = TextAlignmentOptions.Center;
        _nameLabel.enableWordWrapping = true;
        _nameLabel.overflowMode = TextOverflowModes.Ellipsis;
        _nameLabel.raycastTarget = false;
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0.04f, 0.24f);
        nameRt.anchorMax = new Vector2(0.96f, 0.42f);
        nameRt.offsetMin = Vector2.zero;
        nameRt.offsetMax = Vector2.zero;

        // ── Class label ─────────────────────────────────────────────────
        GameObject classObj = CreateChild("ClassLabel", inner.transform);
        _classLabel = classObj.AddComponent<TextMeshProUGUI>();
        _classLabel.fontSize = 16;
        _classLabel.color = SteelBlue;
        _classLabel.alignment = TextAlignmentOptions.Center;
        _classLabel.raycastTarget = false;
        RectTransform classRt = classObj.GetComponent<RectTransform>();
        classRt.anchorMin = new Vector2(0.04f, 0.12f);
        classRt.anchorMax = new Vector2(0.96f, 0.24f);
        classRt.offsetMin = Vector2.zero;
        classRt.offsetMax = Vector2.zero;

        // ── Stars row ───────────────────────────────────────────────────
        GameObject starsObj = CreateChild("StarsLabel", inner.transform);
        _starsLabel = starsObj.AddComponent<TextMeshProUGUI>();
        _starsLabel.fontSize = 18;
        _starsLabel.color = AccentGold;
        _starsLabel.alignment = TextAlignmentOptions.Center;
        _starsLabel.raycastTarget = false;
        RectTransform starsRt = starsObj.GetComponent<RectTransform>();
        starsRt.anchorMin = new Vector2(0.04f, 0.02f);
        starsRt.anchorMax = new Vector2(0.96f, 0.12f);
        starsRt.offsetMin = Vector2.zero;
        starsRt.offsetMax = Vector2.zero;

        // ── Dead overlay (hidden by default) ────────────────────────────
        _deadOverlay = CreateChild("DeadOverlay", inner.transform);
        Image deadImg = _deadOverlay.AddComponent<Image>();
        deadImg.color = DeadOverlayColor;
        deadImg.raycastTarget = false;
        Stretch(_deadOverlay);

        GameObject deadLabelObj = CreateChild("FallenLabel", _deadOverlay.transform);
        _deadText = deadLabelObj.AddComponent<TextMeshProUGUI>();
        _deadText.text = "FALLEN";
        _deadText.fontSize = 28;
        _deadText.fontStyle = FontStyles.Bold;
        _deadText.color = FallenRed;
        _deadText.alignment = TextAlignmentOptions.Center;
        _deadText.raycastTarget = false;
        Stretch(deadLabelObj);

        _deadOverlay.SetActive(false);
    }

    // ════════════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ════════════════════════════════════════════════════════════════════

    public void SetSelected(bool selected)
    {
        if (_borderImage != null)
        {
            _borderImage.color = selected 
                ? new Color(0.78f, 0.66f, 0.29f, 1f) 
                : new Color(0.251f, 0.271f, 0.439f, 0.4f);

            RectTransform borderRt = _borderImage.GetComponent<RectTransform>();
            if (borderRt != null)
            {
                borderRt.anchorMin = Vector2.zero;
                borderRt.anchorMax = Vector2.one;
                borderRt.offsetMin = Vector2.zero;
                borderRt.offsetMax = Vector2.zero;
            }
        }
    }

    public HeroInstance Hero => _hero;

    public void Setup(HeroInstance hero, System.Action<HeroInstance> onTap)
    {
        _hero = hero;
        _onTap = onTap;
        _def = HeroPresentationUtility.LoadHeroDefinition(hero.HeroDefId);

        // ── Name ────────────────────────────────────────────────────────
        string displayName = HeroPresentationUtility.GetDisplayName(_def, hero.HeroDefId);
        _nameLabel.text = displayName.ToUpperInvariant();

        // ── Class ───────────────────────────────────────────────────────
        if (_def != null)
        {
            _classLabel.text = HeroPresentationUtility.GetRoleLabel(_def);
        }
        else
        {
            _classLabel.text = "???";
        }

        // ── Stars ───────────────────────────────────────────────────────
        int starCount = Mathf.Clamp(hero.CurrentStarRank, 0, 5);
        _starsLabel.text = starCount > 0 ? "+ + + + +".Substring(0, starCount * 2 - 1) : "";

        // ── Card Rarity Color Tint ──────────────────────────────────────
        Color cardColor = starCount <= 2 ? new Color(0.18f, 0.20f, 0.28f, 1f) :
                          starCount <= 4 ? new Color(0.18f, 0.20f, 0.35f, 1f) :
                                           new Color(0.28f, 0.22f, 0.12f, 1f);
        _rootImage.color = cardColor;
        if (_innerImage != null) _innerImage.color = cardColor;

        // ── Portrait ────────────────────────────────────────────────────
        _portraitImage.sprite = null;
        _portraitImage.color = GetRarityPlaceholderColor(hero.CurrentStarRank);

        if (_def != null)
        {
            // Try sprite path first
            if (!string.IsNullOrEmpty(_def.PortraitSpritePath))
            {
                Sprite sp = Resources.Load<Sprite>(_def.PortraitSpritePath);
                if (sp != null)
                {
                    _portraitImage.sprite = sp;
                    _portraitImage.color = Color.white;
                }
            }

            // Fallback to direct portrait reference
            if (_portraitImage.sprite == null && _def.Portrait != null)
            {
                _portraitImage.sprite = _def.Portrait;
                _portraitImage.color = Color.white;
            }
        }

        // ── Dead state ──────────────────────────────────────────────────
        if (!hero.IsAlive)
        {
            _deadOverlay.SetActive(true);
            _portraitImage.color = new Color(0.35f, 0.35f, 0.35f, 1f);
        }
        else
        {
            _deadOverlay.SetActive(false);
        }

        // ── Button tap ──────────────────────────────────────────────────
        _cardButton.onClick.RemoveAllListeners();
        _cardButton.onClick.AddListener(() => _onTap?.Invoke(_hero));
    }

    // ════════════════════════════════════════════════════════════════════
    //  UTILITY
    // ════════════════════════════════════════════════════════════════════

    private static GameObject CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Color GetRarityPlaceholderColor(int starRank)
    {
        return starRank switch
        {
            1 => new Color(0.290f, 0.290f, 0.353f),  // #4A4A5A
            2 => new Color(0.227f, 0.353f, 0.290f),  // #3A5A4A
            3 => new Color(0.165f, 0.290f, 0.416f),  // #2A4A6A
            4 => new Color(0.353f, 0.227f, 0.478f),  // #5A3A7A
            5 => new Color(0.478f, 0.290f, 0.165f),  // #7A4A2A
            _ => new Color(0.290f, 0.290f, 0.353f),
        };
    }
}
