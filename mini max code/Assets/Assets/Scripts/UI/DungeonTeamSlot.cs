using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PickMeUp.Game.Core;

namespace PickMeUp.Game.UI
{
    /// <summary>
    /// Represents a single hero slot in the dungeon team selection.
    /// </summary>
    public class DungeonTeamSlot : MonoBehaviour
    {
        public int SlotIndex;
        public bool IsRequired;
        public DungeonView DungeonView;

        [Header("UI References")]
        public RectTransform SlotContainer;
        public Image SlotBackground;
        public Image HeroIcon;
        public TextMeshProUGUI HeroNameText;
        public TextMeshProUGUI HeroLevelText;
        public TextMeshProUGUI SlotNumberText;
        public TextMeshProUGUI StatusText;

        // Runtime data
        private string _heroInstanceId;
        private bool _isEmpty;
        private IRosterService _rosterService;

        public bool HasHero => !string.IsNullOrEmpty(_heroInstanceId);
        public string HeroInstanceId => _heroInstanceId;

        public void Initialize()
        {
            _rosterService = ServiceLocator.Get<IRosterService>();

            // Setup click handler
            var button = gameObject.GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClicked);

            UpdateDisplay();
        }

        public void SetHero(string heroInstanceId)
        {
            _heroInstanceId = heroInstanceId;
            _isEmpty = false;
            UpdateDisplay();
        }

        public void ClearHero()
        {
            _heroInstanceId = null;
            _isEmpty = true;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (HasHero)
            {
                DisplayHero();
            }
            else
            {
                DisplayEmpty();
            }
        }

        private void DisplayHero()
        {
            if (SlotBackground != null)
            {
                SlotBackground.color = new Color(0.2f, 0.6f, 0.8f, 0.9f); // Hero selected color
            }

            if (HeroIcon != null)
            {
                HeroIcon.gameObject.SetActive(true);
                // Would load hero sprite here
                HeroIcon.color = Color.white;
            }

            var hero = _rosterService?.GetHero(_heroInstanceId);
            if (hero != null)
            {
                if (HeroNameText != null)
                {
                    HeroNameText.text = hero.HeroName;
                    HeroNameText.gameObject.SetActive(true);
                }

                if (HeroLevelText != null)
                {
                    HeroLevelText.text = $"Lv.{hero.Level}";
                    HeroLevelText.gameObject.SetActive(true);
                }
            }

            if (StatusText != null)
            {
                StatusText.text = "";
                StatusText.gameObject.SetActive(false);
            }

            if (SlotNumberText != null)
            {
                SlotNumberText.text = $"{SlotIndex + 1}";
                SlotNumberText.gameObject.SetActive(true);
            }
        }

        private void DisplayEmpty()
        {
            if (SlotBackground != null)
            {
                SlotBackground.color = IsRequired
                    ? new Color(0.8f, 0.4f, 0.2f, 0.6f) // Required - orange tint
                    : new Color(0.3f, 0.3f, 0.3f, 0.5f); // Optional - gray
            }

            if (HeroIcon != null)
            {
                HeroIcon.gameObject.SetActive(false);
            }

            if (HeroNameText != null)
            {
                HeroNameText.text = "Empty";
                HeroNameText.color = new Color(1, 1, 1, 0.5f);
            }

            if (HeroLevelText != null)
            {
                HeroLevelText.gameObject.SetActive(false);
            }

            if (StatusText != null)
            {
                StatusText.text = IsRequired ? "Required" : "Optional";
                StatusText.gameObject.SetActive(true);
                StatusText.color = IsRequired ? Color.yellow : Color.gray;
            }

            if (SlotNumberText != null)
            {
                SlotNumberText.text = $"{SlotIndex + 1}";
            }
        }

        private void OnSlotClicked()
        {
            DungeonView?.OnSlotClicked(this);
        }

        private void Start()
        {
            Initialize();
        }
    }
}