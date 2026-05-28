using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the Roster scene.
/// - Displays a scrollable grid of HeroCards
/// - Filter bar (Class dropdown, Alive/Dead toggle, sort by Morale/Rarity/Date)
/// - Detail panel on right with full info, promote, and synthesize buttons
/// </summary>
public class RosterView : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI goldLabel;
    [SerializeField] private TextMeshProUGUI gemsLabel;
    [SerializeField] private TextMeshProUGUI stonesLabel;
    [SerializeField] private TextMeshProUGUI heroCountLabel;
    [SerializeField] private Button backBtn;

    [Header("Filter Bar")]
    [SerializeField] private TMP_Dropdown classDropdown;
    [SerializeField] private Toggle aliveDeadToggle;
    [SerializeField] private TMP_Dropdown sortDropdown;

    [Header("Grid")]
    [SerializeField] private Transform gridContent;
    [SerializeField] private GameObject heroCardPrefab;

    [Header("Detail Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private RectTransform detailPanelRT;
    [SerializeField] private Image detailPortrait;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailClass;
    [SerializeField] private TextMeshProUGUI detailTraitBadgeText;
    [SerializeField] private Image detailTraitBadgeBg;
    [SerializeField] private Slider detailMoraleBar;
    [SerializeField] private TextMeshProUGUI detailPotentialText;
    [SerializeField] private TextMeshProUGUI detailStars;
    [SerializeField] private TextMeshProUGUI detailEquipmentText;
    [SerializeField] private Button promoteBtn;
    [SerializeField] private TextMeshProUGUI promoteCostText;
    [SerializeField] private Button synthesizeBtn;
    [SerializeField] private Button detailCloseBtn;

    [Header("Synth Warning")]
    [SerializeField] private GameObject synthWarningPanel;
    [SerializeField] private TextMeshProUGUI synthWarningText;
    [SerializeField] private Button synthConfirmBtn;
    [SerializeField] private Button synthCancelBtn;

    private IRosterService _roster;
    private ICurrencyService _currency;
    private IInventoryService _inventory;
    private readonly List<RosterHeroCard> _spawnedCards = new List<RosterHeroCard>();

    private HeroInstance _selectedHero;
    private HeroDefinition _selectedDef;
    private Coroutine _detailAnim;

    private const float DetailSlideDuration = 0.25f;

    private void Start()
    {
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();
        _inventory = ServiceRegistry.Instance.Resolve<IInventoryService>();

        if (classDropdown != null) classDropdown.onValueChanged.AddListener(_ => BuildGrid());
        if (aliveDeadToggle != null) aliveDeadToggle.onValueChanged.AddListener(_ => BuildGrid());
        if (sortDropdown != null) sortDropdown.onValueChanged.AddListener(_ => BuildGrid());

        if (detailCloseBtn != null) detailCloseBtn.onClick.AddListener(HideDetail);
        if (backBtn != null) backBtn.onClick.AddListener(() => SceneManager.LoadScene("Hub"));

        if (promoteBtn != null) promoteBtn.onClick.AddListener(OnPromote);
        if (synthesizeBtn != null) synthesizeBtn.onClick.AddListener(OnSynthesizeClicked);
        if (synthConfirmBtn != null) synthConfirmBtn.onClick.AddListener(OnSynthesizeConfirm);
        if (synthCancelBtn != null) synthCancelBtn.onClick.AddListener(() => synthWarningPanel.SetActive(false));

        if (detailPanel != null) detailPanel.SetActive(false);
        if (synthWarningPanel != null) synthWarningPanel.SetActive(false);

        RefreshCurrencyBar();
        BuildGrid();
    }

    private void RefreshCurrencyBar()
    {
        if (goldLabel != null) goldLabel.text = $"Gold: {_currency.GetGold():N0}";
        if (gemsLabel != null) gemsLabel.text = $"Gems: {_currency.GetGems():N0}";
        if (stonesLabel != null) stonesLabel.text = $"Stones: {_currency.GetAttributeStones():N0}";
    }

    private void BuildGrid()
    {
        foreach (RosterHeroCard card in _spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        _spawnedCards.Clear();

        List<HeroInstance> heroes = _roster.GetAll();

        if (aliveDeadToggle != null)
        {
            bool showAlive = aliveDeadToggle.isOn;
            heroes = heroes.Where(h => h.IsAlive == showAlive).ToList();
        }

        if (classDropdown != null && classDropdown.value > 0)
        {
            HeroClass targetClass = (HeroClass)(classDropdown.value - 1);
            heroes = heroes.Where(h =>
            {
                HeroDefinition def = Resources.Load<HeroDefinition>($"Heroes/{h.HeroDefId}");
                return def != null && def.BaseClass == targetClass;
            }).ToList();
        }

        if (sortDropdown != null)
        {
            if (sortDropdown.value == 0)
            {
                heroes = heroes.OrderByDescending(h => h.Morale).ToList();
            }
            else if (sortDropdown.value == 1)
            {
                heroes = heroes.OrderByDescending(h => h.CurrentStarRank).ToList();
            }
            else if (sortDropdown.value == 2)
            {
                heroes = heroes.OrderByDescending(h => h.AcquiredTimestampTicks).ToList();
            }
        }

        if (heroCountLabel != null) heroCountLabel.text = $"Heroes: {heroes.Count}";

        foreach (HeroInstance hero in heroes)
        {
            if (heroCardPrefab == null || gridContent == null)
            {
                break;
            }

            GameObject go = Instantiate(heroCardPrefab, gridContent);
            RosterHeroCard card = go.GetComponent<RosterHeroCard>();
            if (card != null)
            {
                card.SetupCard(hero, OnCardTapped);
                _spawnedCards.Add(card);
            }
        }
    }

    private void OnCardTapped(HeroInstance hero, HeroDefinition def)
    {
        _selectedHero = hero;
        _selectedDef = def;
        PopulateDetail();
        ShowDetail();
    }

    private void PopulateDetail()
    {
        if (_selectedHero == null)
        {
            return;
        }

        if (_selectedDef != null)
        {
            if (detailName != null)
            {
                detailName.text = HeroPresentationUtility.GetDisplayName(_selectedDef, _selectedHero.HeroDefId).ToUpperInvariant();
            }

            if (detailClass != null)
            {
                detailClass.text = HeroPresentationUtility.GetHeroSubtitle(_selectedDef);
            }

            if (detailPortrait != null)
            {
                if (!string.IsNullOrEmpty(_selectedDef.PortraitSpritePath))
                {
                    Sprite sp = Resources.Load<Sprite>(_selectedDef.PortraitSpritePath);
                    if (sp != null)
                    {
                        detailPortrait.sprite = sp;
                    }
                }

                detailPortrait.color = _selectedHero.IsAlive ? Color.white : new Color(0.35f, 0.35f, 0.35f);
            }
        }
        else
        {
            if (detailName != null) detailName.text = _selectedHero.HeroDefId.ToUpperInvariant();
            if (detailClass != null) detailClass.text = "???";
        }

        if (detailTraitBadgeText != null)
        {
            detailTraitBadgeText.text = _selectedHero.Personality.ToString().ToUpperInvariant();
        }

        if (detailTraitBadgeBg != null)
        {
            detailTraitBadgeBg.color = GetTraitColor(_selectedHero.Personality);
        }

        if (detailEquipmentText != null)
        {
            detailEquipmentText.text =
                "Weapon: " + GetEquipmentLabel(_selectedHero.WeaponId) + "\n" +
                "Armor: " + GetEquipmentLabel(_selectedHero.ArmorId) + "\n" +
                "Accessory: " + GetEquipmentLabel(_selectedHero.AccessoryId) + "\n" +
                "Lock: " + (_selectedHero.IsLocked ? "Locked" : "Unlocked");
        }

        if (detailMoraleBar != null)
        {
            detailMoraleBar.minValue = 0;
            detailMoraleBar.maxValue = 100;
            detailMoraleBar.value = _selectedHero.Morale;
        }

        if (detailPotentialText != null)
        {
            detailPotentialText.text = _selectedHero.HiddenPotential switch
            {
                < 0.3f => "\"This unit's ceiling appears limited.\"",
                < 0.7f => "\"Something stirs beneath the surface.\"",
                _ => "\"The System struggles to quantify this unit.\"",
            };
        }

        if (detailStars != null)
        {
            int filled = Mathf.Clamp(_selectedHero.CurrentStarRank, 0, 5);
            detailStars.text = new string('★', filled) + new string('☆', 5 - filled);
            detailStars.color = GetRarityColor(_selectedHero.CurrentStarRank);
        }

        int cost = _selectedHero.CurrentStarRank * 10;
        if (promoteCostText != null)
        {
            promoteCostText.text = $"{cost} Stones";
        }

        if (promoteBtn != null) promoteBtn.interactable = _selectedHero.IsAlive && _selectedHero.CurrentStarRank < 5;
        if (synthesizeBtn != null) synthesizeBtn.interactable = _selectedHero.IsAlive;
    }

    private Color GetTraitColor(PersonalityTrait trait)
    {
        return trait switch
        {
            PersonalityTrait.Brave => new Color(0.8f, 0.3f, 0.3f),
            PersonalityTrait.Cowardly => new Color(0.5f, 0.5f, 0.6f),
            PersonalityTrait.Reckless => new Color(0.85f, 0.5f, 0.15f),
            PersonalityTrait.Disciplined => new Color(0.2f, 0.5f, 0.75f),
            PersonalityTrait.Loyal => new Color(0.3f, 0.7f, 0.4f),
            PersonalityTrait.Traumatized => new Color(0.45f, 0.3f, 0.55f),
            _ => Color.gray
        };
    }

    private Color GetRarityColor(int starRank)
    {
        return starRank switch
        {
            1 => new Color(0.55f, 0.55f, 0.55f),
            2 => new Color(0.27f, 0.65f, 0.27f),
            3 => new Color(0.25f, 0.50f, 0.90f),
            4 => new Color(0.60f, 0.25f, 0.85f),
            5 => new Color(0.95f, 0.75f, 0.10f),
            _ => Color.white
        };
    }

    private string GetEquipmentLabel(string itemInstanceId)
    {
        if (_inventory == null || string.IsNullOrEmpty(itemInstanceId))
        {
            return "None";
        }

        ItemInstance item = _inventory.GetItem(itemInstanceId);
        if (item == null)
        {
            return "None";
        }

        ItemDefinition def = _inventory.GetItemDefinition(item.DefinitionId);
        return def != null ? def.DisplayName : item.DefinitionId;
    }

    private void ShowDetail()
    {
        if (detailPanel == null)
        {
            return;
        }

        detailPanel.SetActive(true);
        if (_detailAnim != null) StopCoroutine(_detailAnim);
        if (detailPanelRT != null)
        {
            _detailAnim = StartCoroutine(Slide(detailPanelRT, new Vector2(1000, 0), Vector2.zero));
        }
    }

    private void HideDetail()
    {
        if (detailPanel == null)
        {
            return;
        }

        if (_detailAnim != null) StopCoroutine(_detailAnim);
        if (detailPanelRT != null)
        {
            _detailAnim = StartCoroutine(Slide(detailPanelRT, Vector2.zero, new Vector2(1000, 0), () => detailPanel.SetActive(false)));
        }
        else
        {
            detailPanel.SetActive(false);
        }
    }

    private IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, System.Action onComplete = null)
    {
        float elapsed = 0;
        while (elapsed < DetailSlideDuration)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / DetailSlideDuration));
            yield return null;
        }

        rt.anchoredPosition = to;
        onComplete?.Invoke();
    }

    private void OnPromote()
    {
        int cost = _selectedHero.CurrentStarRank * 10;
        if (_currency.GetAttributeStones() >= cost)
        {
            _currency.SpendAttributeStones(cost);
            _selectedHero.CurrentStarRank++;
            ServiceRegistry.Instance.Resolve<GameStateService>().Save();
            RefreshCurrencyBar();
            PopulateDetail();
            BuildGrid();
        }
        else
        {
            Debug.LogWarning("Not enough Attribute Stones to promote!");
        }
    }

    private void OnSynthesizeClicked()
    {
        if (synthWarningText != null)
        {
            bool severe = _selectedHero != null && (_selectedHero.Personality == PersonalityTrait.Loyal || _selectedHero.Morale > 70);
            string name = _selectedHero != null
                ? HeroPresentationUtility.GetDisplayName(_selectedDef, _selectedHero.HeroDefId)
                : "THIS HERO";

            synthWarningText.text = severe
                ? $"{name} carries deep value. Synthesizing now will permanently erase their legacy."
                : $"{name} will be consumed in the Synthesis Lab. This cannot be undone.";
        }

        if (synthWarningPanel != null) synthWarningPanel.SetActive(true);
    }

    private void OnSynthesizeConfirm()
    {
        if (synthWarningPanel != null)
        {
            synthWarningPanel.SetActive(false);
        }

        SceneManager.LoadScene("SynthesisLab");
    }
}
