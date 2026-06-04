using UnityEngine;

[CreateAssetMenu(fileName = "FeaturedUnitConfig", menuName = "PickMeUp/Banners/Featured Unit Config")]
public class FeaturedUnitConfig : ScriptableObject
{
    [System.Serializable]
    public class FeaturedUnitData
    {
        public string heroName = "Belquist";
        public string heroTitle = "Berserker";
        [TextArea(2, 4)]
        public string description = "Rate Up!";
        public string subtitle = "2x drop chance this week";
        public Sprite heroPortrait;
        public string heroPortraitPath;
    }

    [System.Serializable]
    public class DropRateEntry
    {
        public string label = "Common (1-2★)";
        public float percentage = 60f;
        public Color barColor = Color.gray;
    }

    public FeaturedUnitData featuredUnit = new FeaturedUnitData();
    public DropRateEntry[] dropRates = new DropRateEntry[]
    {
        new DropRateEntry { label = "Common (1-2★)", percentage = 60f, barColor = new Color(0.62f, 0.62f, 0.62f) },
        new DropRateEntry { label = "Rare (3★)", percentage = 30f, barColor = new Color(0.18f, 0.59f, 0.95f) },
        new DropRateEntry { label = "Epic (4★)", percentage = 8f, barColor = new Color(0.61f, 0.15f, 0.69f) },
        new DropRateEntry { label = "Legendary (5★+)", percentage = 2f, barColor = new Color(1f, 0.76f, 0.03f) }
    };

    public float[] GetNormalizedRates()
    {
        float total = 0f;
        for (int i = 0; i < dropRates.Length; i++)
            total += dropRates[i].percentage;
        float[] normalized = new float[dropRates.Length];
        for (int i = 0; i < dropRates.Length; i++)
            normalized[i] = dropRates[i].percentage / Mathf.Max(total, 0.01f);
        return normalized;
    }
}