using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ---------------------------------------------------------------------------
//  BattleHUD
//  Attach to a Canvas GameObject in the Dungeon scene.
//
//  Displays:
//    • One HP bar per active hero  (built procedurally at battle start)
//    • Wave counter label  (e.g. "Wave 2 / 3")
//    • Speed toggle button cycling 1x → 2x → 3x → 1x
//
//  Connects to:
//    • ICombatEngine events for HP updates and battle end
//    • IDungeonService for wave progress text
// ---------------------------------------------------------------------------
public class BattleHUD : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Inspector — assign the panel roots in the Unity Editor via the
    //  BattleHUD Editor Setup tool (see Editor/BattleHUDSetupEditor.cs)
    // -----------------------------------------------------------------------
    [Header("Panels")]
    [SerializeField] private RectTransform heroBarsPanel;    // horizontal row at bottom
    [SerializeField] private TextMeshProUGUI waveLabel;      // "Wave 1 / 3"
    [SerializeField] private Button speedToggleButton;
    [SerializeField] private TextMeshProUGUI speedToggleLabel;

    [Header("HP Bar Prefab-like settings")]
    [SerializeField] private float barWidth  = 160f;
    [SerializeField] private float barHeight = 28f;
    [SerializeField] private float barSpacing = 8f;
    [SerializeField] private Color frontRowColor = new Color(0.27f, 0.78f, 0.42f);
    [SerializeField] private Color backRowColor  = new Color(0.29f, 0.58f, 0.90f);
    [SerializeField] private Color lowHpColor    = new Color(0.90f, 0.25f, 0.25f);

    // -----------------------------------------------------------------------
    //  Runtime state
    // -----------------------------------------------------------------------
    private ICombatEngine     _combatEngine;
    private IDungeonService   _dungeonService;

    private readonly List<HeroBarEntry> _heroBars = new List<HeroBarEntry>();

    private float[] _speedOptions = { 1f, 2f, 3f };
    private int     _speedIndex   = 0;

    private class HeroBarEntry
    {
        public string      InstanceId;
        public Image       Fill;
        public TextMeshProUGUI Label;
        public int         MaxHP;
        public bool        IsFrontRow;
    }

    // -----------------------------------------------------------------------
    //  Unity lifecycle
    // -----------------------------------------------------------------------
    private void Awake()
    {
        if (speedToggleButton != null)
            speedToggleButton.onClick.AddListener(OnSpeedToggle);

        SetSpeedLabel(_speedOptions[_speedIndex]);
    }

    private void OnEnable()
    {
        ResolveServices();
        SubscribeEvents();

        // Refresh wave label if a dungeon is already in progress
        if (_dungeonService != null && _dungeonService.IsInDungeon)
            RefreshWaveLabel();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    // -----------------------------------------------------------------------
    //  Service wiring
    // -----------------------------------------------------------------------
    private void ResolveServices()
    {
        if (ServiceRegistry.Instance == null) return;

        if (_combatEngine == null && ServiceRegistry.Instance.HasService<ICombatEngine>())
            _combatEngine = ServiceRegistry.Instance.Resolve<ICombatEngine>();

        if (_dungeonService == null && ServiceRegistry.Instance.HasService<IDungeonService>())
            _dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
    }

    private void SubscribeEvents()
    {
        if (_combatEngine != null)
        {
            _combatEngine.OnBattleStart    += HandleBattleStart;
            _combatEngine.OnHeroHit        += HandleHeroHit;
            _combatEngine.OnHeroDied       += HandleHeroDied;
            _combatEngine.OnBattleVictory  += HandleBattleEnd;
            _combatEngine.OnBattleDefeat   += HandleBattleEnd;
        }

        if (_dungeonService != null)
        {
            _dungeonService.OnWaveStarted += _ => RefreshWaveLabel();
            _dungeonService.OnWaveCleared += RefreshWaveLabel;
        }
    }

    private void UnsubscribeEvents()
    {
        if (_combatEngine != null)
        {
            _combatEngine.OnBattleStart    -= HandleBattleStart;
            _combatEngine.OnHeroHit        -= HandleHeroHit;
            _combatEngine.OnHeroDied       -= HandleHeroDied;
            _combatEngine.OnBattleVictory  -= HandleBattleEnd;
            _combatEngine.OnBattleDefeat   -= HandleBattleEnd;
        }
    }

    // -----------------------------------------------------------------------
    //  Combat event handlers
    // -----------------------------------------------------------------------
    private void HandleBattleStart()
    {
        BuildHeroBars();
        RefreshWaveLabel();
    }

    private void HandleHeroHit(CombatHeroState hero, int damage)
    {
        UpdateHeroBar(hero);
    }

    private void HandleHeroDied(CombatHeroState hero)
    {
        UpdateHeroBar(hero);
    }

    private void HandleBattleEnd()
    {
        // Bars stay visible to show final HP state; wave label already updated
    }

    // -----------------------------------------------------------------------
    //  Hero HP bars
    // -----------------------------------------------------------------------
    private void BuildHeroBars()
    {
        // Clear old bars
        foreach (HeroBarEntry old in _heroBars)
        {
            if (old.Fill != null && old.Fill.transform.parent != null)
                Destroy(old.Fill.transform.parent.gameObject);
        }
        _heroBars.Clear();

        if (heroBarsPanel == null || _combatEngine == null) return;

        IReadOnlyList<CombatHeroState> heroes = _combatEngine.Heroes;
        float totalWidth = heroes.Count * (barWidth + barSpacing) - barSpacing;
        float startX     = -totalWidth * 0.5f + barWidth * 0.5f;

        for (int i = 0; i < heroes.Count; i++)
        {
            CombatHeroState hero = heroes[i];
            float x = startX + i * (barWidth + barSpacing);

            // --- Container ---
            GameObject container = new GameObject("HeroBar_" + hero.HeroName, typeof(RectTransform));
            container.transform.SetParent(heroBarsPanel, worldPositionStays: false);

            RectTransform rt = container.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(barWidth, barHeight + 20f);
            rt.anchoredPosition = new Vector2(x, 0f);

            // --- Name label ---
            GameObject labelObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(container.transform, worldPositionStays: false);
            TextMeshProUGUI nameLabel = labelObj.GetComponent<TextMeshProUGUI>();
            nameLabel.text      = hero.HeroName;
            nameLabel.fontSize  = 14f;
            nameLabel.alignment = TextAlignmentOptions.Center;
            nameLabel.color     = Color.white;
            RectTransform nameRt = labelObj.GetComponent<RectTransform>();
            nameRt.anchorMin        = new Vector2(0f, 1f);
            nameRt.anchorMax        = new Vector2(1f, 1f);
            nameRt.pivot            = new Vector2(0.5f, 1f);
            nameRt.anchoredPosition = new Vector2(0f, 0f);
            nameRt.sizeDelta        = new Vector2(0f, 20f);

            // --- Background bar ---
            GameObject bgObj = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(container.transform, worldPositionStays: false);
            Image bg = bgObj.GetComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin        = new Vector2(0f, 0f);
            bgRt.anchorMax        = new Vector2(1f, 0f);
            bgRt.pivot            = new Vector2(0.5f, 0f);
            bgRt.anchoredPosition = new Vector2(0f, 0f);
            bgRt.sizeDelta        = new Vector2(0f, barHeight);

            // --- Fill bar ---
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(bgObj.transform, worldPositionStays: false);
            Image fill = fillObj.GetComponent<Image>();
            fill.color     = hero.IsFrontRow ? frontRowColor : backRowColor;
            fill.type      = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin        = Vector2.zero;
            fillRt.anchorMax        = Vector2.one;
            fillRt.offsetMin        = Vector2.zero;
            fillRt.offsetMax        = Vector2.zero;

            // --- HP text inside fill ---
            GameObject hpTextObj = new GameObject("HPText", typeof(RectTransform), typeof(TextMeshProUGUI));
            hpTextObj.transform.SetParent(bgObj.transform, worldPositionStays: false);
            TextMeshProUGUI hpLabel = hpTextObj.GetComponent<TextMeshProUGUI>();
            hpLabel.text      = hero.CurrentHP + " / " + hero.MaxHP;
            hpLabel.fontSize  = 12f;
            hpLabel.alignment = TextAlignmentOptions.Center;
            hpLabel.color     = Color.white;
            RectTransform hpRt = hpTextObj.GetComponent<RectTransform>();
            hpRt.anchorMin = Vector2.zero;
            hpRt.anchorMax = Vector2.one;
            hpRt.offsetMin = Vector2.zero;
            hpRt.offsetMax = Vector2.zero;

            _heroBars.Add(new HeroBarEntry
            {
                InstanceId = hero.InstanceId,
                Fill       = fill,
                Label      = hpLabel,
                MaxHP      = hero.MaxHP,
                IsFrontRow = hero.IsFrontRow
            });
        }
    }

    private void UpdateHeroBar(CombatHeroState hero)
    {
        for (int i = 0; i < _heroBars.Count; i++)
        {
            HeroBarEntry entry = _heroBars[i];
            if (entry.InstanceId != hero.InstanceId) continue;

            float ratio = entry.MaxHP > 0
                ? (float)hero.CurrentHP / entry.MaxHP
                : 0f;

            entry.Fill.fillAmount = ratio;

            // Go red when HP is below 25%
            entry.Fill.color = ratio < 0.25f
                ? lowHpColor
                : (entry.IsFrontRow ? frontRowColor : backRowColor);

            if (entry.Label != null)
                entry.Label.text = hero.CurrentHP + " / " + entry.MaxHP;

            break;
        }
    }

    // -----------------------------------------------------------------------
    //  Wave label
    // -----------------------------------------------------------------------
    private void RefreshWaveLabel()
    {
        if (waveLabel == null || _dungeonService == null) return;

        DungeonRunState state = _dungeonService.CurrentRunState;
        if (state == null)
        {
            waveLabel.text = string.Empty;
            return;
        }

        int current = state.CurrentWaveIndex + 1;
        int total   = Mathf.Max(1, state.TotalWaves);
        waveLabel.text = "Wave " + current + " / " + total;
    }

    // -----------------------------------------------------------------------
    //  Speed toggle
    // -----------------------------------------------------------------------
    private void OnSpeedToggle()
    {
        _speedIndex = (_speedIndex + 1) % _speedOptions.Length;
        float newScale = _speedOptions[_speedIndex];
        SetSpeedLabel(newScale);

        if (_combatEngine != null)
            _combatEngine.SetSpeedScale(newScale);
    }

    private void SetSpeedLabel(float scale)
    {
        if (speedToggleLabel != null)
            speedToggleLabel.text = (int)scale + "x";
    }
}
