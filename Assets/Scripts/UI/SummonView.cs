using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Manages the full-screen summon overlay and the 3-phase card reveal animation.
/// Attach to the SummonCanvas root GameObject in the Summon scene.
///
/// Phase 1 — Space-time crack opens (color tinted by rarity)
/// Phase 2 — Card drops from crack with rotation + landing shake
/// Phase 3 — Card flips, name banner slides up, stars pop in one by one
/// </summary>
public class SummonView : MonoBehaviour
{
    // ── Inspector references (wired by SetupSummonUI) ──────────────────────
    [SerializeField] private Button    summonStandardBtn;
    [SerializeField] private Button    summonStandardTenBtn;
    [SerializeField] private Button    summonPremiumBtn;
    [SerializeField] private Button    summonPremiumTenBtn;
    [SerializeField] private Button    backBtn;

    [SerializeField] private TextMeshProUGUI goldCostLabel;
    [SerializeField] private TextMeshProUGUI gemsCostLabel;
    [SerializeField] private TextMeshProUGUI pityStandardLabel;
    [SerializeField] private TextMeshProUGUI pityPremiumLabel;

    // Card reveal panel
    [SerializeField] private GameObject cardRevealPanel;
    [SerializeField] private Image      cardPortrait;
    [SerializeField] private Image      cardFrame;
    [SerializeField] private TextMeshProUGUI cardNameLabel;
    [SerializeField] private TextMeshProUGUI cardClassLabel;
    [SerializeField] private Transform  starsContainer;
    [SerializeField] private GameObject starIconPrefab;

    // VFX
    [SerializeField] private Image      crackOverlay;
    [SerializeField] private GameObject cardRoot;

    // ── Rarity colors ──────────────────────────────────────────────────────
    private static readonly Color[] RarityColors = new Color[]
    {
        new Color(0.55f, 0.55f, 0.55f), // 1★ grey
        new Color(0.27f, 0.65f, 0.27f), // 2★ green
        new Color(0.25f, 0.50f, 0.90f), // 3★ blue
        new Color(0.60f, 0.25f, 0.85f), // 4★ purple
        new Color(0.95f, 0.75f, 0.10f), // 5★ gold
    };

    // ── Runtime ────────────────────────────────────────────────────────────
    private IGachaService  _gacha;
    private ICurrencyService _currency;
    private bool _isAnimating;

    // ── Constants ──────────────────────────────────────────────────────────
    private const float CrackFadeInDuration  = 0.4f;
    private const float CardDropDuration     = 0.5f;
    private const float CardFlipDuration     = 0.35f;
    private const float StarPopInterval      = 0.12f;
    private const float ShakeDuration        = 0.25f;
    private const float ShakeMagnitude       = 18f;
    private const float CardDropStartY       = 1200f;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        _gacha    = ServiceRegistry.Instance.Resolve<IGachaService>();
        _currency = ServiceRegistry.Instance.Resolve<ICurrencyService>();

        summonStandardBtn.onClick.AddListener(OnStandardSingle);
        summonStandardTenBtn.onClick.AddListener(OnStandardTen);
        summonPremiumBtn.onClick.AddListener(OnPremiumSingle);
        summonPremiumTenBtn.onClick.AddListener(OnPremiumTen);
        backBtn.onClick.AddListener(OnBack);

        cardRevealPanel.SetActive(false);
        RefreshUI();
    }

    // ── Button handlers ────────────────────────────────────────────────────

    private void OnStandardSingle()
    {
        if (_isAnimating) return;
        HeroInstance hero = _gacha.SummonStandard();
        if (hero == null) { ShowAffordError("Not enough Gold!"); return; }
        StartCoroutine(RevealSingle(hero));
    }

    private void OnStandardTen()
    {
        if (_isAnimating) return;
        List<HeroInstance> heroes = _gacha.SummonStandardTen();
        if (heroes == null) { ShowAffordError("Not enough Gold!"); return; }
        StartCoroutine(RevealTen(heroes));
    }

    private void OnPremiumSingle()
    {
        if (_isAnimating) return;
        HeroInstance hero = _gacha.SummonPremium();
        if (hero == null) { ShowAffordError("Not enough Gems!"); return; }
        StartCoroutine(RevealSingle(hero));
    }

    private void OnPremiumTen()
    {
        if (_isAnimating) return;
        List<HeroInstance> heroes = _gacha.SummonPremiumTen();
        if (heroes == null) { ShowAffordError("Not enough Gems!"); return; }
        StartCoroutine(RevealTen(heroes));
    }

    private void OnBack()
    {
        if (_isAnimating) return;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }

    // ── Reveal coroutines ──────────────────────────────────────────────────

    private IEnumerator RevealSingle(HeroInstance hero)
    {
        _isAnimating = true;
        SetButtonsInteractable(false);

        cardRevealPanel.SetActive(true);
        yield return StartCoroutine(PlayCrackOpen(hero.CurrentStarRank));
        yield return StartCoroutine(PlayCardDrop());
        yield return StartCoroutine(PlayCardFlip(hero));
        yield return StartCoroutine(PlayStarPop(hero.CurrentStarRank));

        RefreshUI();
        _isAnimating = false;
        SetButtonsInteractable(true);
    }

    private IEnumerator RevealTen(List<HeroInstance> heroes)
    {
        _isAnimating = true;
        SetButtonsInteractable(false);

        // Show each card sequentially, briefly
        for (int i = 0; i < heroes.Count; i++)
        {
            HeroInstance hero = heroes[i];
            cardRevealPanel.SetActive(true);

            yield return StartCoroutine(PlayCrackOpen(hero.CurrentStarRank));
            yield return StartCoroutine(PlayCardDrop());
            yield return StartCoroutine(PlayCardFlip(hero));

            bool isLast = (i == heroes.Count - 1);
            if (isLast)
                yield return StartCoroutine(PlayStarPop(hero.CurrentStarRank));
            else
                yield return new WaitForSeconds(0.3f);
        }

        RefreshUI();
        _isAnimating = false;
        SetButtonsInteractable(true);
    }

    // ── Animation phases ───────────────────────────────────────────────────

    /// <summary>Phase 1: crack flashes in, tinted by rarity color.</summary>
    private IEnumerator PlayCrackOpen(int starRank)
    {
        Color rarityColor = RarityColors[Mathf.Clamp(starRank - 1, 0, 4)];
        crackOverlay.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0f);
        crackOverlay.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < CrackFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / CrackFadeInDuration;
            crackOverlay.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, Mathf.SmoothStep(0f, 0.85f, t));
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        crackOverlay.gameObject.SetActive(false);
    }

    /// <summary>Phase 2: card drops from above with rotation, lands with shake.</summary>
    private IEnumerator PlayCardDrop()
    {
        RectTransform cardRT = cardRoot.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(0f, CardDropStartY);
        Vector2 endPos   = Vector2.zero;

        cardRT.anchoredPosition = startPos;
        cardRoot.SetActive(true);

        float elapsed = 0f;
        while (elapsed < CardDropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / CardDropDuration;
            float ease = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
            cardRT.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
            cardRT.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(15f, 0f, ease));
            yield return null;
        }

        cardRT.anchoredPosition = endPos;
        cardRT.localRotation = Quaternion.identity;

        yield return StartCoroutine(Shake(cardRT, ShakeDuration, ShakeMagnitude));
    }

    /// <summary>Phase 3: card flips to reveal portrait and name.</summary>
    private IEnumerator PlayCardFlip(HeroInstance hero)
    {
        RectTransform cardRT = cardRoot.GetComponent<RectTransform>();

        // Flip first half — card "turns away"
        float elapsed = 0f;
        float half = CardFlipDuration * 0.5f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            cardRT.localScale = new Vector3(1f - t, 1f, 1f);
            yield return null;
        }

        // Swap content at midpoint
        PopulateCard(hero);

        // Flip second half — card "faces us"
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            cardRT.localScale = new Vector3(t, 1f, 1f);
            yield return null;
        }

        cardRT.localScale = Vector3.one;
    }

    /// <summary>Phase 3b: stars pop in one at a time.</summary>
    private IEnumerator PlayStarPop(int starRank)
    {
        // Clear previous stars
        foreach (Transform child in starsContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < starRank; i++)
        {
            if (starIconPrefab != null)
            {
                GameObject star = Instantiate(starIconPrefab, starsContainer);
                star.transform.localScale = Vector3.zero;
                yield return StartCoroutine(ScaleTo(star.transform, Vector3.one, 0.08f));
            }
            yield return new WaitForSeconds(StarPopInterval);
        }

        yield return new WaitForSeconds(0.6f);
    }

    // ── Card population ────────────────────────────────────────────────────

    private void PopulateCard(HeroInstance hero)
    {
        // Load HeroDefinition for display data
        HeroDefinition def = Resources.Load<HeroDefinition>($"Heroes/{hero.HeroDefId}");

        if (def != null)
        {
            cardNameLabel.text  = HeroPresentationUtility.GetDisplayName(def, hero.HeroDefId).ToUpper();
            cardClassLabel.text = HeroPresentationUtility.GetRoleLabel(def).ToUpper();

            // Portrait
            if (!string.IsNullOrEmpty(def.PortraitSpritePath))
            {
                Sprite portrait = Resources.Load<Sprite>(def.PortraitSpritePath);
                if (portrait != null) cardPortrait.sprite = portrait;
            }

            // Apply rarity tint to frame
            Color tint = RarityColors[Mathf.Clamp(def.BaseStarRank - 1, 0, 4)];
            if (cardFrame != null) cardFrame.color = tint;
        }
        else
        {
            cardNameLabel.text  = hero.HeroDefId.ToUpper();
            cardClassLabel.text = "???";
            Debug.LogWarning($"[SummonView] HeroDefinition not found for id: {hero.HeroDefId}");
        }
    }

    // ── UI helpers ─────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        PityData pity = _gacha.GetPity();
        if (goldCostLabel  != null) goldCostLabel.text  = $"Gold: {_currency.GetGold():N0}";
        if (gemsCostLabel  != null) gemsCostLabel.text  = $"Gems: {_currency.GetGems():N0}";
        if (pityStandardLabel != null) pityStandardLabel.text = $"Pity: {pity.StandardSummonCount}/180";
        if (pityPremiumLabel  != null) pityPremiumLabel.text  = $"Pity: {pity.PremiumSummonCount}/100";
    }

    private GameObject _toastGo;
    private Coroutine _toastCoroutine;

    private void ShowAffordError(string msg)
    {
        if (_toastGo == null)
            CreateToast();

        _toastGo.SetActive(true);
        TextMeshProUGUI tmp = _toastGo.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = msg;

        if (_toastCoroutine != null)
            StopCoroutine(_toastCoroutine);
        _toastCoroutine = StartCoroutine(FadeToast());
    }

    private void CreateToast()
    {
        _toastGo = new GameObject("AffordToast");
        _toastGo.transform.SetParent(transform, false);
        RectTransform rt = _toastGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject bg = new GameObject("ToastBg");
        bg.transform.SetParent(_toastGo.transform, false);
        RectTransform bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.1f, 0.7f);
        bgRT.anchorMax = new Vector2(0.9f, 0.8f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        GameObject textGo = new GameObject("ToastText");
        textGo.transform.SetParent(bg.transform, false);
        RectTransform textRT = textGo.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 40;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        _toastGo.SetActive(false);
    }

    private IEnumerator FadeToast()
    {
        yield return new WaitForSeconds(2f);
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        CanvasGroup cg = _toastGo.GetComponent<CanvasGroup>();
        if (cg == null) cg = _toastGo.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        _toastGo.SetActive(false);
        // Reset alpha for next use
        cg.alpha = 1f;
    }

    private void SetButtonsInteractable(bool value)
    {
        summonStandardBtn.interactable    = value;
        summonStandardTenBtn.interactable = value;
        summonPremiumBtn.interactable     = value;
        summonPremiumTenBtn.interactable  = value;
        backBtn.interactable              = value;
    }

    // ── Animation utilities ────────────────────────────────────────────────

    private IEnumerator Shake(RectTransform rt, float duration, float magnitude)
    {
        Vector2 origin = rt.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            rt.anchoredPosition = origin + Random.insideUnitCircle * strength;
            yield return null;
        }
        rt.anchoredPosition = origin;
    }

    private IEnumerator ScaleTo(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        t.localScale = target;
    }
}
