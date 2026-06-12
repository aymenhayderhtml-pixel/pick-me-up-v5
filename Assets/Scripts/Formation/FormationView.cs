using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormationView : MonoBehaviour
{
    [Header("Slot Grid")]
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Roster List")]
    [SerializeField] private RectTransform rosterListContainer;
    [SerializeField] private GameObject rosterCardPrefab;

    [Header("Team Stats")]
    [SerializeField] private TextMeshProUGUI totalHpText;
    [SerializeField] private TextMeshProUGUI totalAtkText;
    [SerializeField] private TextMeshProUGUI totalDefText;
    [SerializeField] private TextMeshProUGUI selectedCountText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button backButton;

    private IFormationService formationService;
    private IRosterService rosterService;
    private List<HeroDefinition> heroDefinitions;
    private List<SlotEntry> slotEntries = new List<SlotEntry>();
    private List<RosterCardEntry> rosterEntries = new List<RosterCardEntry>();
    private string selectedHeroId;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);
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
        formationService = ServiceRegistry.Instance?.Resolve<IFormationService>();
        rosterService = ServiceRegistry.Instance?.Resolve<IRosterService>();
        heroDefinitions = LoadHeroDefinitions();

        BuildSlots();
        BuildRosterList();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (clearButton != null) clearButton.onClick.RemoveListener(OnClearClicked);
        if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
    }

    // --- Build UI ---

    private void BuildSlots()
    {
        ClearSlots();

        if (slotContainer == null || slotPrefab == null) return;

        for (int i = 0; i < 5; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            SlotEntry entry = new SlotEntry
            {
                Index = i,
                Root = slotObj,
                Button = slotObj.GetComponent<Button>(),
                PortraitImage = FindImage(slotObj, "Portrait"),
                NameText = FindTMP(slotObj, "Name"),
                RowLabel = FindTMP(slotObj, "RowLabel")
            };

            if (entry.RowLabel != null)
                entry.RowLabel.text = i < 2 ? "FRONT" : "BACK";

            if (entry.Button != null)
            {
                int capturedIndex = i;
                entry.Button.onClick.AddListener(() => OnSlotClicked(capturedIndex));
            }

            slotEntries.Add(entry);
        }
    }

    private void BuildRosterList()
    {
        ClearRosterList();

        if (rosterListContainer == null || rosterCardPrefab == null || rosterService == null) return;

        List<HeroInstance> heroes = rosterService.GetAlive();
        for (int i = 0; i < heroes.Count; i++)
        {
            HeroInstance hero = heroes[i];
            if (hero == null) continue;

            HeroDefinition def = GetDefinition(hero.HeroDefId);
            if (def == null) continue;

            GameObject cardObj = Instantiate(rosterCardPrefab, rosterListContainer);
            RosterCardEntry entry = new RosterCardEntry
            {
                HeroInstance = hero,
                Definition = def,
                Root = cardObj,
                Button = cardObj.GetComponent<Button>(),
                PortraitImage = FindImage(cardObj, "Portrait"),
                NameText = FindTMP(cardObj, "Name"),
                ClassText = FindTMP(cardObj, "Class"),
                StarsText = FindTMP(cardObj, "Stars")
            };

            if (entry.NameText != null) entry.NameText.text = def.HeroName;
            if (entry.ClassText != null) entry.ClassText.text = def.BaseClass.ToString();
            if (entry.StarsText != null) entry.StarsText.text = new string('★', hero.CurrentStarRank);
            if (entry.PortraitImage != null && def.Portrait != null)
                entry.PortraitImage.sprite = def.Portrait;

            if (entry.Button != null)
            {
                string capturedId = hero.InstanceId;
                entry.Button.onClick.AddListener(() => OnRosterCardClicked(capturedId));
            }

            rosterEntries.Add(entry);
        }
    }

    // --- Interaction ---

    private void OnSlotClicked(int slotIndex)
    {
        if (formationService == null) return;

        if (string.IsNullOrEmpty(selectedHeroId))
        {
            formationService.ClearSlot(slotIndex);
        }
        else
        {
            bool isFront = slotIndex < 2;
            formationService.AssignSlot(slotIndex, selectedHeroId, isFront);
            selectedHeroId = null;
        }

        RefreshUI();
    }

    private void OnRosterCardClicked(string heroInstanceId)
    {
        if (formationService == null) return;

        // If already in formation, remove it
        for (int i = 0; i < formationService.Slots.Count; i++)
        {
            if (formationService.Slots[i].HeroInstanceId == heroInstanceId)
            {
                formationService.ClearSlot(i);
                selectedHeroId = null;
                RefreshUI();
                return;
            }
        }

        // Otherwise select it for placement
        selectedHeroId = heroInstanceId;
        RefreshUI();
    }

    private void OnConfirmClicked()
    {
        if (formationService == null) return;

        int count = formationService.GetAssignedCount();
        if (count < 3)
        {
            Debug.Log("[FormationView] Need at least 3 heroes.");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
    }

    private void OnClearClicked()
    {
        formationService?.ClearAll();
        selectedHeroId = null;
        RefreshUI();
    }

    private void OnBackClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }

    // --- Refresh ---

    private void RefreshUI()
    {
        UpdateSlotHighlights();
        UpdateRosterHighlights();
        UpdateTeamStats();
        UpdateConfirmButton();
    }

    private void UpdateSlotHighlights()
    {
        if (formationService == null) return;

        for (int i = 0; i < slotEntries.Count; i++)
        {
            SlotEntry slot = slotEntries[i];
            FormationSlot formationSlot = formationService.Slots[i];

            if (formationSlot.IsOccupied)
            {
                HeroInstance hero = rosterService?.GetAll().Find(h => h.InstanceId == formationSlot.HeroInstanceId);
                HeroDefinition def = hero != null ? GetDefinition(hero.HeroDefId) : null;

                if (slot.NameText != null) slot.NameText.text = def != null ? def.HeroName : "???";
                if (slot.PortraitImage != null && def != null && def.Portrait != null)
                    slot.PortraitImage.sprite = def.Portrait;
            }
            else
            {
                if (slot.NameText != null) slot.NameText.text = "Empty";
                if (slot.PortraitImage != null) slot.PortraitImage.sprite = null;
            }
        }
    }

    private void UpdateRosterHighlights()
    {
        for (int i = 0; i < rosterEntries.Count; i++)
        {
            RosterCardEntry entry = rosterEntries[i];
            bool inFormation = false;
            for (int s = 0; s < formationService.Slots.Count; s++)
            {
                if (formationService.Slots[s].HeroInstanceId == entry.HeroInstance.InstanceId)
                {
                    inFormation = true;
                    break;
                }
            }
            bool selected = entry.HeroInstance.InstanceId == selectedHeroId;

            Image img = entry.Root != null ? entry.Root.GetComponent<Image>() : null;
            if (img != null)
            {
                img.color = selected ? new Color(0.3f, 0.6f, 1f, 0.5f) :
                            inFormation ? new Color(0.3f, 1f, 0.3f, 0.3f) :
                            new Color(0.15f, 0.15f, 0.2f, 0.9f);
            }
        }
    }

    private void UpdateTeamStats()
    {
        if (formationService == null || rosterService == null || heroDefinitions == null) return;

        int totalHp = 0;
        int totalAtk = 0;
        int totalDef = 0;

        List<HeroInstance> heroes = rosterService.GetAlive();
        for (int i = 0; i < formationService.Slots.Count; i++)
        {
            FormationSlot slot = formationService.Slots[i];
            if (!slot.IsOccupied) continue;

            HeroInstance hero = heroes.Find(h => h.InstanceId == slot.HeroInstanceId);
            if (hero == null) continue;

            HeroDefinition def = GetDefinition(hero.HeroDefId);
            if (def == null) continue;

            totalHp += def.GetMaxHp(hero.Level, hero.CurrentStarRank);
            totalAtk += def.GetAtk(hero.Level, hero.CurrentStarRank);
            totalDef += def.GetDef(hero.Level, hero.CurrentStarRank);
        }

        if (totalHpText != null) totalHpText.text = "HP: " + totalHp;
        if (totalAtkText != null) totalAtkText.text = "ATK: " + totalAtk;
        if (totalDefText != null) totalDefText.text = "DEF: " + totalDef;
        if (selectedCountText != null) selectedCountText.text = formationService.GetAssignedCount() + " / 5";
    }

    private void UpdateConfirmButton()
    {
        if (confirmButton == null) return;
        int count = formationService != null ? formationService.GetAssignedCount() : 0;
        confirmButton.interactable = count >= 3;
    }

    // --- Helpers ---

    private HeroDefinition GetDefinition(string heroDefId)
    {
        if (heroDefinitions == null) return null;
        for (int i = 0; i < heroDefinitions.Count; i++)
        {
            if (heroDefinitions[i] != null && heroDefinitions[i].HeroId == heroDefId)
                return heroDefinitions[i];
        }
        return null;
    }

    private List<HeroDefinition> LoadHeroDefinitions()
    {
        HeroDefinition[] defs = Resources.LoadAll<HeroDefinition>("Heroes");
        if (defs != null && defs.Length > 0)
            return new List<HeroDefinition>(defs);

        defs = Resources.LoadAll<HeroDefinition>("");
        List<HeroDefinition> result = new List<HeroDefinition>();
        if (defs != null)
        {
            for (int i = 0; i < defs.Length; i++)
            {
                if (defs[i] != null) result.Add(defs[i]);
            }
        }
        return result;
    }

    private Image FindImage(GameObject root, string childName)
    {
        if (root == null) return null;
        Transform t = root.transform.Find(childName);
        if (t != null) return t.GetComponent<Image>();
        return null;
    }

    private TextMeshProUGUI FindTMP(GameObject root, string childName)
    {
        if (root == null) return null;
        Transform t = root.transform.Find(childName);
        if (t != null) return t.GetComponent<TextMeshProUGUI>();
        return null;
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slotEntries.Count; i++)
        {
            if (slotEntries[i].Root != null) Destroy(slotEntries[i].Root);
        }
        slotEntries.Clear();
    }

    private void ClearRosterList()
    {
        for (int i = 0; i < rosterEntries.Count; i++)
        {
            if (rosterEntries[i].Root != null) Destroy(rosterEntries[i].Root);
        }
        rosterEntries.Clear();
    }

    // --- Inner Classes ---

    private class SlotEntry
    {
        public int Index;
        public GameObject Root;
        public Button Button;
        public Image PortraitImage;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI RowLabel;
    }

    private class RosterCardEntry
    {
        public HeroInstance HeroInstance;
        public HeroDefinition Definition;
        public GameObject Root;
        public Button Button;
        public Image PortraitImage;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ClassText;
        public TextMeshProUGUI StarsText;
    }
}
