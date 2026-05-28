using UnityEngine;

public enum HeroClass
{
    Vanguard,
    Scout,
    Mage,
    Berserker,
    Assassin,
    Support,
    Specialist
}

[CreateAssetMenu(fileName = "NewHeroDefinition", menuName = "PickMeUp/Hero Definition")]
public class HeroDefinition : ScriptableObject
{
    public string HeroId;
    public string HeroName;
    public int BaseStarRank;
    public HeroClass BaseClass;
    public int BaseHP;
    public int BaseATK;
    public int BaseDEF;
    
    [Header("Asset Paths")]
    public string PortraitSpritePath;
    public string CardFrameSpritePath;

    [Header("Collection")]
    public string HeroClass;
    public Sprite Portrait;
    [TextArea(3, 10)]
    public string LoreText;
}
