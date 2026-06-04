using UnityEngine;

[CreateAssetMenu(fileName = "HeroClassColorConfig", menuName = "PickMeUp/Config/Class Colors")]
public class HeroClassColorConfig : ScriptableObject
{
    [Header("Class Colors")]
    public Color noviceColor = new Color(0.50f, 0.50f, 0.50f);
    public Color vanguardColor = new Color(0.90f, 0.30f, 0.30f);
    public Color scoutColor = new Color(0.20f, 0.70f, 0.60f);
    public Color mageColor = new Color(0.30f, 0.30f, 0.90f);
    public Color berserkerColor = new Color(0.80f, 0.60f, 0.20f);
    public Color assassinColor = new Color(0.60f, 0.20f, 0.70f);
    public Color supportColor = new Color(0.30f, 0.90f, 0.50f);
    public Color specialistColor = new Color(0.20f, 0.70f, 0.80f);

    public Color GetClassColor(HeroClass heroClass)
    {
        return heroClass switch
        {
            HeroClass.Novice => noviceColor,
            HeroClass.Vanguard => vanguardColor,
            HeroClass.Scout => scoutColor,
            HeroClass.Mage => mageColor,
            HeroClass.Berserker => berserkerColor,
            HeroClass.Assassin => assassinColor,
            HeroClass.Support => supportColor,
            HeroClass.Specialist => specialistColor,
            _ => Color.gray
        };
    }
}
