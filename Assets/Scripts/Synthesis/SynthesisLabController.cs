using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynthesisLabController : MonoBehaviour
{
    [Header("Hero Selection")]
    [SerializeField] private RectTransform heroGridContainer;
    [SerializeField] private GameObject heroCardPrefab;

    [Header("Merge Preview")]
    [SerializeField] private TextMeshProUGUI resultNameText;
    [SerializeField] private TextMeshProUGUI resultStarsText;
    [SerializeField] private TextMeshProUGUI resultStatsText;
    [SerializeField] private Image resultPortrait;
    [SerializeField] private GameObject warningPanel;

    [Header("Buttons")]
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button backButton;

    private IRosterService rosterService;
    private List<HeroInstance> selectedHeroes = new List<HeroInstance>();
    private const int MaxMergeSlots = 2;
    private const int HighValueWarningThreshold = 3;

    private void Awake()
    {
        if (mergeButton != null)
        {
            mergeButton.onClick.RemoveListener(OnMergeClicked);
            mergeButton.onClick.AddListener(OnMergeClicked);
        }
        if (clearButton != null)
        {
            clearButton.onClick.RemoveListener(OnClearClicked);
            clearButton.onClick.AddListener(OnClearClicked);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnEnable()
    {
        rosterService = ServiceRegistry.Instance?.Resolve<IRosterService>();
        selectedHeroes.Clear();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (mergeButton != null) mergeButton.onClick.RemoveListener(OnMergeClicked);
        if (clearButton != null) clearButton.onClick.RemoveListener(OnClearClicked);
        if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void RefreshUI()
    {
        BuildHeroGrid();
        UpdatePreview();
        UpdateMergeButton();
    }

    private void BuildHeroGrid()
    {
        // Clear old
        for (int i = heroGridContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(heroGridContainer.GetChild(i).gameObject);
        }

        if (rosterService == null) return;

        List<HeroInstance> heroes = rosterService.GetAlive();
        for (int i = 0; i < heroes.Count; i++)
        {
            HeroInstance hero = heroes[i];
            if (hero == null) continue;

            GameObject cardObj = Instantiate(heroCardPrefab, heroGridContainer);
            SetupHeroCard(cardObj, hero);

            Button btn = cardObj.GetComponent<Button>();
            if (btn != null)
            {
                string capturedId = hero.InstanceId;
                btn.onClick.AddListener(() => OnHeroCardClicked(capturedId));
            }
        }
    }

    private void SetupHeroCard(GameObject cardObj, HeroInstance hero)
    {
        HeroDefinition def = FindDefinition(hero.HeroDefId);
        if (def == null) return;

        TextMeshProUGUI nameText = FindTMP(cardObj, "Name");
        TextMeshProUGUI starsText = FindTMP(cardObj, "Stars");
        TextMeshProUGUI classText = FindTMP(cardObj, "Class");
        Image portrait = FindImage(cardObj, "Portrait");

        if (nameText != null) nameText.text = def.HeroName;
        if (starsText != null) starsText.text = new string('★', hero.CurrentStarRank);
        if (classText != null) classText.text = def.BaseClass.ToString();
        if (portrait != null)
        {
            portrait.sprite = def.Portrait;
            portrait.color = GetClassColor(def.BaseClass);
        }

        // Highlight if selected
        bool isSelected = selectedHeroes.Exists(h => h.InstanceId == hero.InstanceId);
        Image bg = cardObj.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = isSelected ? new Color(0.3f, 0.6f, 1f, 0.5f) : new Color(0.15f, 0.15f, 0.2f, 0.9f);
        }
    }

    private void OnHeroCardClicked(string heroInstanceId)
    {
        if (rosterService == null) return;

        HeroInstance hero = rosterService.GetAll().Find(h => h.InstanceId == heroInstanceId);
        if (hero == null) return;

        // Toggle selection
        int existingIndex = selectedHeroes.FindIndex(h => h.InstanceId == heroInstanceId);
        if (existingIndex >= 0)
        {
            selectedHeroes.RemoveAt(existingIndex);
        }
        else
        {
            if (selectedHeroes.Count >= MaxMergeSlots)
            {
                Debug.Log("[SynthesisLab] Max 2 heroes for merge.");
                return;
            }

            // RULE: Must be same hero ID (same archetype)
            if (selectedHeroes.Count > 0 && selectedHeroes[0].HeroDefId != hero.HeroDefId)
            {
                Debug.Log("[SynthesisLab] Can only merge identical heroes.");
                return;
            }

            // RULE: Cannot merge locked heroes
            if (hero.IsLocked)
            {
                Debug.Log("[SynthesisLab] Cannot merge locked heroes.");
                return;
            }

            selectedHeroes.Add(hero);
        }

        RefreshUI();
    }

    private void UpdatePreview()
    {
        if (selectedHeroes.Count < 2)
        {
            if (resultNameText != null) resultNameText.text = "Select 2 identical heroes";
            if (resultStarsText != null) resultStarsText.text = "";
            if (resultStatsText != null) resultStatsText.text = "";
            if (resultPortrait != null)
            {
                resultPortrait.sprite = null;
                resultPortrait.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            }
            if (warningPanel != null) warningPanel.SetActive(false);
            return;
        }

        HeroInstance baseHero = selectedHeroes[0];
        HeroDefinition def = FindDefinition(baseHero.HeroDefId);
        if (def == null) return;

        int newStar = Mathf.Min(5, baseHero.CurrentStarRank + 1);

        if (resultNameText != null) resultNameText.text = def.HeroName + " +" + newStar;
        if (resultStarsText != null) resultStarsText.text = new string('★', newStar);
        if (resultStatsText != null)
        {
            HeroResolvedStats stats = HeroStatCalculator.Resolve(def, 1, newStar);
            resultStatsText.text = "HP " + stats.MaxHP + "  ATK " + stats.ATK + "\nDEF " + stats.DEF + "  SPD " + stats.SPD;
        }
        if (resultPortrait != null)
        {
            resultPortrait.sprite = def.Portrait;
            resultPortrait.color = GetClassColor(def.BaseClass);
        }

        // Warning for high-value heroes
        bool showWarning = false;
        for (int i = 0; i < selectedHeroes.Count; i++)
        {
            if (selectedHeroes[i].CurrentStarRank >= HighValueWarningThreshold)
            {
                showWarning = true;
                break;
            }
        }
        if (warningPanel != null) warningPanel.SetActive(showWarning);
    }

    private void UpdateMergeButton()
    {
        if (mergeButton == null) return;
        mergeButton.interactable = selectedHeroes.Count == MaxMergeSlots;
    }

    private void OnMergeClicked()
    {
        if (selectedHeroes.Count < 2 || rosterService == null) return;

        HeroInstance baseHero = selectedHeroes[0];
        HeroInstance sacrifice = selectedHeroes[1];

        // Consume sacrifice
        rosterService.Remove(sacrifice.InstanceId);

        // Promote base hero
        baseHero.CurrentStarRank = Mathf.Min(5, baseHero.CurrentStarRank + 1);

        // Clear selection
        selectedHeroes.Clear();
        RefreshUI();

        Debug.Log("[SynthesisLab] Merge complete! " + baseHero.HeroDefId + " is now " + baseHero.CurrentStarRank + "★");
    }

    private void OnClearClicked()
    {
        selectedHeroes.Clear();
        RefreshUI();
    }

    private void OnBackClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }

    // --- Helpers ---

    private HeroDefinition FindDefinition(string heroDefId)
    {
        HeroDefinition[] defs = Resources.LoadAll<HeroDefinition>("Heroes");
        for (int i = 0; i < defs.Length; i++)
        {
            if (defs[i] != null && defs[i].HeroId == heroDefId)
                return defs[i];
        }
        return null;
    }

    private Color GetClassColor(HeroClass heroClass)
    {
        switch (heroClass)
        {
            case HeroClass.Warrior: return new Color(0.9f, 0.3f, 0.2f);
            case HeroClass.Mage: return new Color(0.3f, 0.4f, 0.9f);
            case HeroClass.Ranger: return new Color(0.2f, 0.8f, 0.3f);
            case HeroClass.Healer: return new Color(0.9f, 0.8f, 0.2f);
            case HeroClass.Tank: return new Color(0.5f, 0.5f, 0.5f);
            default: return Color.white;
        }
    }

    private TextMeshProUGUI FindTMP(GameObject root, string childName)
    {
        if (root == null) return null;
        Transform t = root.transform.Find(childName);
        if (t != null) return t.GetComponent<TextMeshProUGUI>();
        return null;
    }

    private Image FindImage(GameObject root, string childName)
    {
        if (root == null) return null;
        Transform t = root.transform.Find(childName);
        if (t != null) return t.GetComponent<Image>();
        return null;
    }
}
