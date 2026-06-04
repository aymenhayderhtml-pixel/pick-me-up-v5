using UnityEngine;

/// <summary>
/// Configurable rarity color/glow/particle data.
/// Replaces the hardcoded RarityColors array in SummonView.
/// </summary>
[CreateAssetMenu(fileName = "RarityVisualConfig", menuName = "PickMeUp/Rarity/Visual Config")]
public class RarityVisualConfig : ScriptableObject
{
    [System.Serializable]
    public class RarityData
    {
        public int rarity;
        public Color textColor = Color.white;
        public Color circleGlowColor = Color.clear;
        public Color environmentGlowColor = Color.clear;
        public float environmentGlowIntensity = 0.3f;
        public float circleGlowIntensity = 0.65f;
        public bool useParticles;
        public bool enablePulsingGlow;
        public float pulsingScale = 1.05f;
    }

    public RarityData[] rarityTiers = new RarityData[]
    {
        new RarityData
        {
            rarity = 1,
            textColor = new Color(0.62f, 0.62f, 0.62f),
            circleGlowColor = new Color(0.35f, 0.35f, 0.38f),
            environmentGlowColor = new Color(0.25f, 0.25f, 0.28f),
            circleGlowIntensity = 0.40f,
            environmentGlowIntensity = 0.15f
        },
        new RarityData
        {
            rarity = 2,
            textColor = new Color(0.30f, 0.69f, 0.31f),
            circleGlowColor = new Color(0.20f, 0.50f, 0.22f),
            environmentGlowColor = new Color(0.15f, 0.35f, 0.17f),
            circleGlowIntensity = 0.45f,
            environmentGlowIntensity = 0.20f
        },
        new RarityData
        {
            rarity = 3,
            textColor = new Color(0.18f, 0.59f, 0.95f),
            circleGlowColor = new Color(0.18f, 0.38f, 0.72f),
            environmentGlowColor = new Color(0.12f, 0.28f, 0.52f),
            circleGlowIntensity = 0.55f,
            environmentGlowIntensity = 0.30f
        },
        new RarityData
        {
            rarity = 4,
            textColor = new Color(0.61f, 0.15f, 0.69f),
            circleGlowColor = new Color(0.48f, 0.18f, 0.72f),
            environmentGlowColor = new Color(0.35f, 0.12f, 0.55f),
            circleGlowIntensity = 0.75f,
            environmentGlowIntensity = 0.50f,
            useParticles = true,
            enablePulsingGlow = true,
            pulsingScale = 1.05f
        },
        new RarityData
        {
            rarity = 5,
            textColor = new Color(1f, 0.76f, 0.03f),
            circleGlowColor = new Color(0.75f, 0.60f, 0.05f),
            environmentGlowColor = new Color(0.55f, 0.40f, 0.03f),
            circleGlowIntensity = 0.85f,
            environmentGlowIntensity = 0.70f,
            useParticles = true,
            enablePulsingGlow = true,
            pulsingScale = 1.08f
        }
    };

    public RarityData GetRarityData(int rarity)
    {
        for (int i = 0; i < rarityTiers.Length; i++)
            if (rarityTiers[i].rarity == rarity)
                return rarityTiers[i];
        return rarityTiers[0];
    }

    /// <summary>Clamped index for backward compat with arrays (rarity 1-5 maps to index 0-4).</summary>
    public RarityData GetRarityDataClamped(int rarity)
    {
        int idx = Mathf.Clamp(rarity - 1, 0, rarityTiers.Length - 1);
        return rarityTiers[idx];
    }
}