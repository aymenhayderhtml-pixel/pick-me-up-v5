using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SynthView : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI successRateText;
    [SerializeField] private RectTransform baseHeroArea;
    [SerializeField] private RectTransform materialArea;
    [SerializeField] private RectTransform previewArea;
    [SerializeField] private Button synthButton;

    private IRosterService _rosterService;
    private ISynthesizerService _synthService;
    private readonly List<HeroInstance> _selectedMaterials = new List<HeroInstance>();
    private HeroInstance _selectedBaseHero;
    private readonly List<Button> _baseButtons = new List<Button>();
    private readonly List<Button> _materialButtons = new List<Button>();

    private void Start()
    {
        _rosterService = ServiceRegistry.Instance.Resolve<IRosterService>();
        _synthService = ServiceRegistry.Instance.Resolve<ISynthesizerService>();

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        }

        if (synthButton != null)
        {
            synthButton.onClick.RemoveAllListeners();
            synthButton.onClick.AddListener(OnSynthesizePressed);
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (titleText != null)
        {
            titleText.text = "SYNTHESIS LAB";
        }

        BuildHeroLists();
        UpdatePreview();
        UpdateSuccessRate();
    }

    private void BuildHeroLists()
    {
        if (_rosterService == null)
        {
            return;
        }

        List<HeroInstance> heroes = _rosterService.GetAlive();

        if (baseHeroArea != null)
        {
            foreach (Transform child in baseHeroArea) Destroy(child.gameObject);
            _baseButtons.Clear();
            foreach (HeroInstance hero in heroes)
            {
                GameObject card = CreateHeroButton(hero, true);
                card.transform.SetParent(baseHeroArea, false);
            }
        }

        if (materialArea != null)
        {
            foreach (Transform child in materialArea) Destroy(child.gameObject);
            _materialButtons.Clear();
            foreach (HeroInstance hero in heroes.Where(hero => _selectedBaseHero == null || hero.InstanceId != _selectedBaseHero.InstanceId))
            {
                GameObject card = CreateHeroButton(hero, false);
                card.transform.SetParent(materialArea, false);
            }
        }
    }

    private GameObject CreateHeroButton(HeroInstance hero, bool isBase)
    {
        GameObject card = new GameObject((isBase ? "Base_" : "Material_") + hero.InstanceId, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 100);
        card.GetComponent<Image>().color = isBase
            ? (_selectedBaseHero != null && _selectedBaseHero.InstanceId == hero.InstanceId ? new Color(0.3f, 0.6f, 0.85f, 0.95f) : new Color(0.2f, 0.35f, 0.55f, 0.9f))
            : (_selectedMaterials.Any(material => material.InstanceId == hero.InstanceId) ? new Color(0.35f, 0.8f, 0.4f, 0.95f) : new Color(0.35f, 0.2f, 0.2f, 0.9f));

        Button button = card.GetComponent<Button>();
        if (isBase)
        {
            button.onClick.AddListener(() => SelectBaseHero(hero));
            _baseButtons.Add(button);
        }
        else
        {
            button.onClick.AddListener(() => ToggleMaterial(hero));
            _materialButtons.Add(button);
        }

        CreateTMP("Name", hero.HeroDefId, 16, card.transform, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.85f));
        CreateTMP("Info", "STAR " + hero.CurrentStarRank, 14, card.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.35f));
        return card;
    }

    private void SelectBaseHero(HeroInstance hero)
    {
        _selectedBaseHero = hero;
        _selectedMaterials.RemoveAll(material => material.InstanceId == hero.InstanceId);
        RefreshUI();
    }

    private void ToggleMaterial(HeroInstance hero)
    {
        if (_selectedBaseHero != null && hero.InstanceId == _selectedBaseHero.InstanceId)
        {
            return;
        }

        HeroInstance existing = _selectedMaterials.FirstOrDefault(material => material.InstanceId == hero.InstanceId);
        if (existing != null)
        {
            _selectedMaterials.Remove(existing);
        }
        else if (_selectedMaterials.Count < _synthService.GetMaxMaterialCount())
        {
            _selectedMaterials.Add(hero);
        }

        RefreshUI();
    }

    private void UpdatePreview()
    {
        if (previewArea == null)
        {
            return;
        }

        foreach (Transform child in previewArea) Destroy(child.gameObject);

        GameObject preview = new GameObject("PreviewText", typeof(RectTransform), typeof(TextMeshProUGUI));
        preview.transform.SetParent(previewArea, false);
        RectTransform rt = preview.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = preview.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 18;

        if (_selectedBaseHero == null)
        {
            text.text = "Select a base hero.";
        }
        else
        {
            SynthesisRecipe recipe = _synthService.CalculateRecipe(_selectedBaseHero, _selectedMaterials);
            text.text = "Base: " + _selectedBaseHero.HeroDefId + "\nMaterials: " + _selectedMaterials.Count + "\nPromotion " + _selectedBaseHero.PromotionRank + " -> " + (_selectedBaseHero.PromotionRank + recipe.PromotionRankIncrease) + "\nStar " + _selectedBaseHero.CurrentStarRank + " -> " + Mathf.Clamp(_selectedBaseHero.CurrentStarRank + recipe.StarRankIncrease, 1, 5);
        }
    }

    private void UpdateSuccessRate()
    {
        if (successRateText == null)
        {
            return;
        }

        if (_selectedBaseHero == null)
        {
            successRateText.text = "Select Heroes";
            return;
        }

        SynthesisRecipe recipe = _synthService.CalculateRecipe(_selectedBaseHero, _selectedMaterials);
        successRateText.text = "Success: " + recipe.SuccessRate.ToString("F0") + "%";
    }

    private void OnSynthesizePressed()
    {
        if (_selectedBaseHero == null || _selectedMaterials.Count == 0)
        {
            return;
        }

        SynthesisResult result = _synthService.ExecuteSynthesis(_selectedBaseHero, new List<HeroInstance>(_selectedMaterials));
        if (result.Success)
        {
            _selectedBaseHero = null;
            _selectedMaterials.Clear();
        }

        RefreshUI();
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
