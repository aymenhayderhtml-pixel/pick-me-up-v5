using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies RarityVisualConfig data to summon screen visual elements.
/// Handles glow colors, text colors, particle bursts, and pulsing animations.
/// </summary>
public class RarityVisualApplier : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private RarityVisualConfig config;

    [Header("UI Elements")]
    [SerializeField] private Image ritualCircleImage;
    [SerializeField] private Image environmentGlow;
    [SerializeField] private Image materializeImage;
    [SerializeField] private Image silhouetteImage;
    [SerializeField] private Image dimOverlay;

    [Header("Result Text")]
    [SerializeField] private TMPro.TextMeshProUGUI statusResultTMP;
    [SerializeField] private TMPro.TextMeshProUGUI heroNameTMP;
    [SerializeField] private TMPro.TextMeshProUGUI heroClassTMP;

    [Header("Particle System")]
    [SerializeField] private GameObject particleBurstPrefab;
    [SerializeField] private Transform particleSpawnPoint;

    private bool isPulsing = false;
    private Coroutine pulseCoroutine;
    private RarityVisualConfig.RarityData currentRarityData;

    public void ApplyRarity(int starRank)
    {
        if (config == null)
        {
            Debug.LogWarning("[RarityVisualApplier] No config assigned.");
            return;
        }

        currentRarityData = config.GetRarityDataClamped(starRank);

        if (ritualCircleImage != null)
        {
            ritualCircleImage.color = new Color(
                currentRarityData.circleGlowColor.r,
                currentRarityData.circleGlowColor.g,
                currentRarityData.circleGlowColor.b,
                currentRarityData.circleGlowIntensity
            );
        }

        if (environmentGlow != null)
        {
            environmentGlow.color = new Color(
                currentRarityData.environmentGlowColor.r,
                currentRarityData.environmentGlowColor.g,
                currentRarityData.environmentGlowColor.b,
                currentRarityData.environmentGlowIntensity
            );
        }

        if (statusResultTMP != null)
            statusResultTMP.color = currentRarityData.textColor;
        if (heroNameTMP != null)
            heroNameTMP.color = currentRarityData.textColor;
        if (heroClassTMP != null)
            heroClassTMP.color = currentRarityData.textColor;

        if (currentRarityData.useParticles && particleBurstPrefab != null && particleSpawnPoint != null)
        {
            GameObject particles = Instantiate(particleBurstPrefab, particleSpawnPoint.position, Quaternion.identity, particleSpawnPoint);
            Destroy(particles, 2f);
        }

        if (currentRarityData.enablePulsingGlow)
            StartPulsing();
        else
            StopPulsing();
    }

    public void ResetVisuals()
    {
        StopPulsing();
        if (ritualCircleImage != null) SetImageAlpha(ritualCircleImage, 0f);
        if (environmentGlow != null) SetImageAlpha(environmentGlow, 0f);
        if (silhouetteImage != null) SetImageAlpha(silhouetteImage, 0f);
        if (materializeImage != null) SetImageAlpha(materializeImage, 0f);
        if (dimOverlay != null) SetImageAlpha(dimOverlay, 0f);
    }

    private void StartPulsing()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        isPulsing = true;
        pulseCoroutine = StartCoroutine(PulseGlowRoutine());
    }

    private void StopPulsing()
    {
        isPulsing = false;
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        if (environmentGlow != null)
            environmentGlow.transform.localScale = Vector3.one;
    }

    private System.Collections.IEnumerator PulseGlowRoutine()
    {
        float duration = 2f;
        float elapsed = 0f;
        while (isPulsing)
        {
            elapsed += Time.deltaTime;
            float t = (Mathf.Sin(elapsed / duration * Mathf.PI * 2f) + 1f) * 0.5f;
            float scale = Mathf.Lerp(1f, currentRarityData.pulsingScale, t);
            if (environmentGlow != null)
                environmentGlow.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
    }

    private void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }

    public void SetCircleGlow(Color color, float alpha)
    {
        if (ritualCircleImage != null)
            ritualCircleImage.color = new Color(color.r, color.g, color.b, alpha);
    }

    public void SetEnvironmentGlow(Color color, float alpha)
    {
        if (environmentGlow != null)
            environmentGlow.color = new Color(color.r, color.g, color.b, alpha);
    }

    public RarityVisualConfig.RarityData GetCurrentRarityData() => currentRarityData;
}