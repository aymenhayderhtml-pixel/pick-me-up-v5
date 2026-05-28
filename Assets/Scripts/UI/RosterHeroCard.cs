using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single hero card in the Roster grid.
/// Call SetupCard() with a HeroInstance to populate it.
/// Pooling-safe: all state is reset inside SetupCard().
/// </summary>
public class RosterHeroCard : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private Image frame;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI classLabel;
    [SerializeField] private Transform starsContainer;
    [SerializeField] private GameObject starIconPrefab;
    [SerializeField] private GameObject deadOverlay;
    [SerializeField] private TextMeshProUGUI deadLabel;
    [SerializeField] private Image traitBadgeBg;
    [SerializeField] private TextMeshProUGUI traitBadgeText;
    [SerializeField] private Button cardButton;

    private static readonly Color[] RarityColors = new Color[]
    {
        new Color(0.55f, 0.55f, 0.55f), // 1★ grey
        new Color(0.27f, 0.65f, 0.27f), // 2★ green
        new Color(0.25f, 0.50f, 0.90f), // 3★ blue
        new Color(0.60f, 0.25f, 0.85f), // 4★ purple
        new Color(0.95f, 0.75f, 0.10f), // 5★ gold
    };

    private HeroInstance _hero;
    private HeroDefinition _def;

    public void SetupCard(HeroInstance hero, System.Action<HeroInstance, HeroDefinition> onTap)
    {
        _hero = hero;
        _def = Resources.Load<HeroDefinition>($"Heroes/{hero.HeroDefId}");

        // Reset pooling state
        portrait.color = Color.white;
        portrait.sprite = null;
        if (deadOverlay != null) deadOverlay.SetActive(false);

        foreach (Transform child in starsContainer)
            Destroy(child.gameObject);

        if (_def != null)
        {
            if (nameLabel != null) nameLabel.text = HeroPresentationUtility.GetDisplayName(_def, hero.HeroDefId).ToUpper();
            if (classLabel != null) classLabel.text = HeroPresentationUtility.GetRoleLabel(_def).ToUpper();

            if (!string.IsNullOrEmpty(_def.PortraitSpritePath))
            {
                Sprite sp = Resources.Load<Sprite>(_def.PortraitSpritePath);
                if (sp != null) portrait.sprite = sp;
            }

            Color tint = RarityColors[Mathf.Clamp(_def.BaseStarRank - 1, 0, 4)];
            if (frame != null) frame.color = tint;

            for (int i = 0; i < _hero.CurrentStarRank; i++)
            {
                if (starIconPrefab != null)
                    Instantiate(starIconPrefab, starsContainer);
            }
        }
        else
        {
            if (nameLabel != null) nameLabel.text = hero.HeroDefId.ToUpper();
            if (classLabel != null) classLabel.text = "???";
        }

        // Trait Badge
        if (traitBadgeText != null) traitBadgeText.text = hero.Personality.ToString().Substring(0, 1);
        if (traitBadgeBg != null) traitBadgeBg.color = GetTraitColor(hero.Personality);

        // Dead State
        if (!hero.IsAlive)
        {
            portrait.color = new Color(0.35f, 0.35f, 0.35f, 1f);
            if (deadOverlay != null) deadOverlay.SetActive(true);
            if (deadLabel != null) deadLabel.text = "DEAD";
            if (frame != null) frame.color = new Color(0.30f, 0.30f, 0.30f, 1f);
        }

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => onTap?.Invoke(_hero, _def));
        }
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
}
