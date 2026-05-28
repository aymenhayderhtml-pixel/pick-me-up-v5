using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemorialHallView : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private TextMeshProUGUI echoSummaryText;
    [SerializeField] private TextMeshProUGUI legacySummaryText;
    [SerializeField] private Button allButton;
    [SerializeField] private Button discoveredButton;
    [SerializeField] private Button undiscoveredButton;
    [SerializeField] private RectTransform heroGridContent;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailPortrait;
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailClassText;
    [SerializeField] private TextMeshProUGUI detailStarsText;
    [SerializeField] private TextMeshProUGUI detailLoreText;
    [SerializeField] private TextMeshProUGUI detailCountText;
    [SerializeField] private TextMeshProUGUI detailEchoText;
    [SerializeField] private TextMeshProUGUI detailLegacyText;
    [SerializeField] private Button detailCloseButton;

    private GameStateService _gameState;
    private IRosterService _roster;
    private readonly List<Button> _heroButtons = new List<Button>();
    private List<HeroDefinition> _allDefinitions = new List<HeroDefinition>();
    private string _selectedHeroId;
    private FilterMode _filter = FilterMode.All;

    private enum FilterMode
    {
        All,
        Discovered,
        Undiscovered
    }

    private void Start()
    {
        _gameState = ServiceRegistry.Instance.Resolve<GameStateService>();
        _roster = ServiceRegistry.Instance.Resolve<IRosterService>();
        _allDefinitions = new List<HeroDefinition>(Resources.LoadAll<HeroDefinition>("Heroes"));

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        }

        if (detailCloseButton != null)
        {
            detailCloseButton.onClick.RemoveAllListeners();
            detailCloseButton.onClick.AddListener(HideDetail);
        }

        if (allButton != null) allButton.onClick.AddListener(() => SetFilter(FilterMode.All));
        if (discoveredButton != null) discoveredButton.onClick.AddListener(() => SetFilter(FilterMode.Discovered));
        if (undiscoveredButton != null) undiscoveredButton.onClick.AddListener(() => SetFilter(FilterMode.Undiscovered));

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        if (_gameState == null)
        {
            return;
        }

        RefreshUI();
    }

    private void SetFilter(FilterMode filter)
    {
        _filter = filter;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_gameState == null)
        {
            return;
        }

        SyncEchoProgress();

        if (titleText != null)
        {
            titleText.text = "MEMORIAL HALL";
        }

        UpdateSummaries();
        BuildGrid();
        UpdateCompletion();
    }

    private void BuildGrid()
    {
        if (heroGridContent == null)
        {
            return;
        }

        foreach (Transform child in heroGridContent)
        {
            Destroy(child.gameObject);
        }

        _heroButtons.Clear();

        foreach (HeroDefinition def in GetFilteredDefinitions())
        {
            GameObject card = CreateHeroCard(def);
            card.transform.SetParent(heroGridContent, false);
        }
    }

    private List<HeroDefinition> GetFilteredDefinitions()
    {
        if (_allDefinitions == null)
        {
            return new List<HeroDefinition>();
        }

        List<string> discovered = _gameState.Data != null && _gameState.Data.DiscoveredHeroIds != null
            ? _gameState.Data.DiscoveredHeroIds
            : new List<string>();

        return _allDefinitions.Where(def =>
        {
            bool isDiscovered = discovered.Contains(def.HeroId);
            if (_filter == FilterMode.Discovered) return isDiscovered;
            if (_filter == FilterMode.Undiscovered) return !isDiscovered;
            return true;
        }).OrderBy(def => HeroPresentationUtility.GetDisplayName(def, def.HeroId)).ToList();
    }

    private GameObject CreateHeroCard(HeroDefinition def)
    {
        bool discovered = _gameState.Data != null && _gameState.Data.DiscoveredHeroIds != null && _gameState.Data.DiscoveredHeroIds.Contains(def.HeroId);
        bool echoed = _gameState.Data != null && _gameState.Data.MemorialEchoHeroIds != null && _gameState.Data.MemorialEchoHeroIds.Contains(def.HeroId);

        GameObject card = new GameObject("HeroCard_" + def.HeroId, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 220);

        Image image = card.GetComponent<Image>();
        image.color = discovered ? new Color(0.15f, 0.15f, 0.2f, 0.9f) : new Color(0.06f, 0.06f, 0.08f, 0.75f);
        if (echoed)
        {
            image.color = new Color(image.color.r + 0.08f, image.color.g + 0.02f, image.color.b + 0.12f, 0.95f);
        }

        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => SelectHero(def));
        _heroButtons.Add(button);

        CreateTMP("Name", discovered ? HeroPresentationUtility.GetDisplayName(def, def.HeroId).ToUpperInvariant() : "???", 16, card.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.22f));
        CreateTMP("Class", discovered ? HeroPresentationUtility.GetHeroSubtitle(def) : "LOCKED", 11, card.transform, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.37f));
        CreateTMP("Stars", new string('★', Mathf.Clamp(def.BaseStarRank, 1, 5)), 14, card.transform, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.97f));
        if (echoed)
        {
            CreateTMP("Echo", "ECHO", 12, card.transform, new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.8f));
        }

        return card;
    }

    private void SelectHero(HeroDefinition def)
    {
        if (_gameState.Data == null || _gameState.Data.DiscoveredHeroIds == null || !_gameState.Data.DiscoveredHeroIds.Contains(def.HeroId))
        {
            return;
        }

        _selectedHeroId = def.HeroId;
        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        bool echoed = _gameState.Data.MemorialEchoHeroIds != null && _gameState.Data.MemorialEchoHeroIds.Contains(def.HeroId);
        HeroInstance fallenHero = _roster.GetDead().FirstOrDefault(hero => hero != null && hero.HeroDefId == def.HeroId);

        if (detailPortrait != null)
        {
            if (def.Portrait != null)
            {
                detailPortrait.sprite = def.Portrait;
                detailPortrait.color = Color.white;
            }
            else
            {
                detailPortrait.sprite = null;
                detailPortrait.color = GetClassColor(def.BaseClass);
            }
        }

        if (detailNameText != null)
        {
            detailNameText.text = HeroPresentationUtility.GetDisplayName(def, def.HeroId);
        }

        if (detailClassText != null)
        {
            detailClassText.text = HeroPresentationUtility.GetHeroSubtitle(def);
        }

        if (detailStarsText != null)
        {
            detailStarsText.text = new string('★', Mathf.Clamp(def.BaseStarRank, 1, 5));
        }

        if (detailLoreText != null)
        {
            detailLoreText.text = string.IsNullOrEmpty(def.LoreText) ? "No lore available." : def.LoreText;
        }

        if (detailCountText != null)
        {
            int discoveredCount = _gameState.Data != null && _gameState.Data.DiscoveredHeroIds != null ? _gameState.Data.DiscoveredHeroIds.Count : 0;
            detailCountText.text = $"Acquired: {discoveredCount}/{_allDefinitions.Count}";
        }

        if (detailEchoText != null)
        {
            detailEchoText.text = echoed
                ? $"Echo Resonance: ACTIVE\nThis hero has fallen before and now strengthens the Memorial."
                : "Echo Resonance: DORMANT\nNo fallen record yet.";
        }

        if (detailLegacyText != null)
        {
            if (fallenHero != null)
            {
                detailLegacyText.text =
                    $"Fell In Battle: {GetFallSummary(fallenHero)}\n" +
                    $"Morale At Rest: {fallenHero.Morale}\n" +
                    $"Star Rank At Loss: {fallenHero.CurrentStarRank}";
            }
            else if (echoed)
            {
                detailLegacyText.text = "Legacy recorded in the Memorial archive.\nThe system preserves the outline of this hero.";
            }
            else
            {
                detailLegacyText.text = "No memorial record yet.\nThis hero still has a chance to become an Echo.";
            }
        }
    }

    private string GetFallSummary(HeroInstance hero)
    {
        if (hero == null)
        {
            return "Unknown";
        }

        string acquired = new System.DateTime(hero.AcquiredTimestampTicks, System.DateTimeKind.Utc).ToString("yyyy-MM-dd");
        return $"Acquired {acquired}";
    }

    private Color GetClassColor(HeroClass heroClass)
    {
        switch (heroClass)
        {
            case HeroClass.Vanguard:
                return new Color(0.9f, 0.3f, 0.3f);
            case HeroClass.Mage:
                return new Color(0.3f, 0.3f, 0.9f);
            case HeroClass.Support:
                return new Color(0.3f, 0.9f, 0.5f);
            case HeroClass.Berserker:
                return new Color(0.8f, 0.6f, 0.2f);
            default:
                return Color.gray;
        }
    }

    private void UpdateCompletion()
    {
        if (completionText == null || _allDefinitions == null || _gameState == null || _gameState.Data == null || _gameState.Data.DiscoveredHeroIds == null)
        {
            return;
        }

        int discovered = _gameState.Data.DiscoveredHeroIds.Count;
        int total = _allDefinitions.Count;
        completionText.text = $"{discovered}/{total}";
    }

    private void UpdateSummaries()
    {
        int echoCount = GetEchoCount();
        int deadCount = _roster != null ? _roster.GetDead().Count : 0;

        if (echoSummaryText != null)
        {
            echoSummaryText.text = $"Echoes: {echoCount}\nFallen Heroes: {deadCount}";
        }

        if (legacySummaryText != null)
        {
            legacySummaryText.text = echoCount > 0
                ? "Legacy is active. Fallen heroes now shape future progression."
                : "No legacy recorded yet. Defeats will begin to build the archive.";
        }
    }

    private int GetEchoCount()
    {
        if (_gameState == null || _gameState.Data == null || _gameState.Data.MemorialEchoHeroIds == null)
        {
            return 0;
        }

        return _gameState.Data.MemorialEchoHeroIds.Count;
    }

    private void SyncEchoProgress()
    {
        if (_gameState == null || _gameState.Data == null || _roster == null)
        {
            return;
        }

        if (_gameState.Data.MemorialEchoHeroIds == null)
        {
            _gameState.Data.MemorialEchoHeroIds = new List<string>();
        }

        bool changed = false;
        List<HeroInstance> deadHeroes = _roster.GetDead();
        foreach (HeroInstance hero in deadHeroes)
        {
            if (hero != null && !string.IsNullOrEmpty(hero.HeroDefId) && !_gameState.Data.MemorialEchoHeroIds.Contains(hero.HeroDefId))
            {
                _gameState.Data.MemorialEchoHeroIds.Add(hero.HeroDefId);
                changed = true;
            }
        }

        if (changed)
        {
            _gameState.Save();
        }
    }

    private void HideDetail()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
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
