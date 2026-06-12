using UnityEngine;

[CreateAssetMenu(fileName = "HeroClassColorConfig", menuName = "PickMeUp/Config/Class Colors")]
public class HeroClassColorConfig : ScriptableObject
{
    [Header("Legacy Class Colors")]
    public Color noviceColor     = new Color(0.50f, 0.50f, 0.50f);
    public Color vanguardColor   = new Color(0.90f, 0.30f, 0.30f);
    public Color scoutColor      = new Color(0.20f, 0.70f, 0.60f);
    public Color mageColor       = new Color(0.30f, 0.30f, 0.90f);
    public Color berserkerColor  = new Color(0.80f, 0.60f, 0.20f);
    public Color assassinColor   = new Color(0.60f, 0.20f, 0.70f);
    public Color supportColor    = new Color(0.30f, 0.90f, 0.50f);
    public Color specialistColor = new Color(0.20f, 0.70f, 0.80f);

    [Header("New Combat Role Colors")]
    public Color warriorColor = new Color(0.90f, 0.30f, 0.30f);
    public Color rangerColor  = new Color(0.20f, 0.75f, 0.45f);
    public Color healerColor  = new Color(0.30f, 0.85f, 0.55f);
    public Color tankColor    = new Color(0.65f, 0.65f, 0.20f);

    public Color GetClassColor(HeroClass heroClass)
    {
        return heroClass switch
        {
            // Legacy
            HeroClass.Novice      => noviceColor,
            HeroClass.Vanguard    => vanguardColor,
            HeroClass.Scout       => scoutColor,
            HeroClass.Berserker   => berserkerColor,
            HeroClass.Assassin    => assassinColor,
            HeroClass.Support     => supportColor,
            HeroClass.Specialist  => specialistColor,
            // New combat roles
            HeroClass.Warrior     => warriorColor,
            HeroClass.Mage        => mageColor,
            HeroClass.Ranger      => rangerColor,
            HeroClass.Healer      => healerColor,
            HeroClass.Tank        => tankColor,
            _                     => Color.gray
        };
    }
}
