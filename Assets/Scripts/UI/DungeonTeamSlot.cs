using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonTeamSlot : MonoBehaviour
{
    public int SlotIndex;
    public bool IsRequired;
    public DungeonView DungeonView;

    [SerializeField] private Image slotBackground;
    [SerializeField] private Image heroIcon;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI heroLevelText;
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI statusText;

    private IRosterService _rosterService;
    private Button _button;
    private string _heroInstanceId;

    public bool HasHero => !string.IsNullOrEmpty(_heroInstanceId);
    public string HeroInstanceId => _heroInstanceId;

    private void Awake()
    {
        ResolveRoster();
        CacheButton();
    }

    private void Start()
    {
        ResolveRoster();
        CacheButton();
        UpdateDisplay();
    }

    public void SetHero(string heroInstanceId)
    {
        _heroInstanceId = heroInstanceId;
        UpdateDisplay();
    }

    public void ClearHero()
    {
        _heroInstanceId = string.Empty;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        ResolveRoster();
        if (HasHero)
        {
            DisplayHero();
        }
        else
        {
            DisplayEmpty();
        }
    }

    private void ResolveRoster()
    {
        if (_rosterService == null && ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IRosterService>())
        {
            _rosterService = ServiceRegistry.Instance.Resolve<IRosterService>();
        }
    }

    private void CacheButton()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                _button = gameObject.AddComponent<Button>();
            }
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnSlotClicked);
    }

    private HeroInstance FindHero()
    {
        if (_rosterService == null || string.IsNullOrEmpty(_heroInstanceId))
        {
            return null;
        }

        return _rosterService.GetAll().Find(hero => hero.InstanceId == _heroInstanceId);
    }

    private void DisplayHero()
    {
        HeroInstance hero = FindHero();
        if (slotBackground != null)
        {
            slotBackground.color = new Color(0.2f, 0.6f, 0.8f, 0.9f);
        }

        if (heroIcon != null)
        {
            heroIcon.gameObject.SetActive(true);
            heroIcon.color = Color.white;
        }

        if (heroNameText != null)
        {
            heroNameText.gameObject.SetActive(true);
            if (hero != null)
            {
                HeroDefinition def = Resources.Load<HeroDefinition>($"Heroes/{hero.HeroDefId}");
                heroNameText.text = HeroPresentationUtility.GetDisplayName(def, hero.HeroDefId).ToUpperInvariant();
            }
            else
            {
                heroNameText.text = "MISSING";
            }
        }

        if (heroLevelText != null)
        {
            heroLevelText.gameObject.SetActive(true);
            heroLevelText.text = hero != null ? "STAR " + hero.CurrentStarRank : "STAR ?";
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }

        if (slotNumberText != null)
        {
            slotNumberText.gameObject.SetActive(true);
            slotNumberText.text = (SlotIndex + 1).ToString();
        }
    }

    private void DisplayEmpty()
    {
        if (slotBackground != null)
        {
            slotBackground.color = IsRequired ? new Color(0.8f, 0.4f, 0.2f, 0.6f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }

        if (heroIcon != null)
        {
            heroIcon.gameObject.SetActive(false);
        }

        if (heroNameText != null)
        {
            heroNameText.gameObject.SetActive(true);
            heroNameText.text = "Empty";
        }

        if (heroLevelText != null)
        {
            heroLevelText.gameObject.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = IsRequired ? "Required" : "Optional";
            statusText.color = IsRequired ? Color.yellow : Color.gray;
        }

        if (slotNumberText != null)
        {
            slotNumberText.gameObject.SetActive(true);
            slotNumberText.text = (SlotIndex + 1).ToString();
        }
    }

    private void OnSlotClicked()
    {
        if (DungeonView != null)
        {
            DungeonView.OnSlotClicked(this);
        }
    }
}
