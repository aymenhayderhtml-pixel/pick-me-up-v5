using UnityEngine;

// ---------------------------------------------------------------------------
//  HeroStatCalculator
//  Pure-static helper. No MonoBehaviour, no service registration needed.
//
//  Stat formula:
//    resolved = (classBase + defOverride) * starMult * levelMult
//
//  starMult  = 1.0 + (star-1) * 0.20   → 1-star = 1.0x, 5-star = 1.80x
//  levelMult = 1.0 + (level-1) * 0.025 → level 1 = 1.0x, level 60 = 2.475x
// ---------------------------------------------------------------------------
public static class HeroStatCalculator
{
    // -----------------------------------------------------------------------
    //  Class base stats (Level 1, Star 1)
    // -----------------------------------------------------------------------
    private static void GetClassDefaults(HeroClass heroClass,
        out int hp, out int atk, out int def, out int spd)
    {
        switch (heroClass)
        {
            case HeroClass.Warrior:
                hp = 320; atk = 80; def = 50; spd = 55;
                break;
            case HeroClass.Mage:
                hp = 220; atk = 110; def = 30; spd = 60;
                break;
            case HeroClass.Ranger:
                hp = 260; atk = 90; def = 35; spd = 75;
                break;
            case HeroClass.Healer:
                hp = 280; atk = 50; def = 40; spd = 65;
                break;
            case HeroClass.Tank:
                hp = 500; atk = 55; def = 90; spd = 35;
                break;

            case HeroClass.Vanguard:
                hp = 340; atk = 75; def = 55; spd = 50;
                break;
            case HeroClass.Berserker:
                hp = 280; atk = 105; def = 25; spd = 60;
                break;
            case HeroClass.Scout:
                hp = 240; atk = 85; def = 30; spd = 80;
                break;
            case HeroClass.Assassin:
                hp = 200; atk = 100; def = 20; spd = 85;
                break;
            case HeroClass.Support:
                hp = 260; atk = 45; def = 45; spd = 60;
                break;
            case HeroClass.Specialist:
                hp = 230; atk = 95; def = 35; spd = 65;
                break;

            default:
                hp = 240; atk = 60; def = 40; spd = 50;
                break;
        }
    }

    private static float StarMultiplier(int star)
    {
        int clamped = Mathf.Clamp(star, 1, 5);
        return 1f + (clamped - 1) * 0.20f;
    }

    private static float LevelMultiplier(int level)
    {
        int clamped = Mathf.Clamp(level, 1, 60);
        return 1f + (clamped - 1) * 0.025f;
    }

    public static HeroResolvedStats Resolve(HeroDefinition def, int level, int star)
    {
        if (def == null)
        {
            return new HeroResolvedStats();
        }

        GetClassDefaults(def.BaseClass, out int bHp, out int bAtk, out int bDef, out int bSpd);

        if (def.BaseHP  > 0) bHp  = def.BaseHP;
        if (def.BaseATK > 0) bAtk = def.BaseATK;
        if (def.BaseDEF > 0) bDef = def.BaseDEF;
        if (def.BaseSPD > 0) bSpd = def.BaseSPD;

        float sm = StarMultiplier(star);
        float lm = LevelMultiplier(level);
        float combined = sm * lm;

        return new HeroResolvedStats
        {
            MaxHP = Mathf.Max(1, Mathf.RoundToInt(bHp  * combined)),
            ATK   = Mathf.Max(1, Mathf.RoundToInt(bAtk * combined)),
            DEF   = Mathf.Max(0, Mathf.RoundToInt(bDef * combined)),
            SPD   = Mathf.Max(1, Mathf.RoundToInt(bSpd * combined))
        };
    }

    public static HeroResolvedStats Resolve(HeroDefinition def, HeroInstance instance)
    {
        if (instance == null) return Resolve(def, 1, 1);
        int level = Mathf.Max(1, instance.PromotionRank + 1);
        return Resolve(def, level, instance.CurrentStarRank);
    }

    public static HeroResolvedStats ResolveWithEquipment(HeroDefinition def, HeroInstance instance, IInventoryService inventory)
    {
        if (instance == null) return Resolve(def, 1, 1);
        int level = Mathf.Max(1, instance.PromotionRank + 1);
        HeroResolvedStats stats = Resolve(def, level, instance.CurrentStarRank);
        if (inventory == null) return stats;
        ItemStatBonus bonus = inventory.GetHeroEquipmentBonus(instance.InstanceId);
        stats.MaxHP += bonus.TotalHealth;
        stats.ATK   += bonus.TotalAttack;
        stats.DEF   += bonus.TotalDefense;
        stats.SPD   += bonus.TotalSpeed;
        return stats;
    }
}

[System.Serializable]
public class HeroResolvedStats
{
    public int MaxHP;
    public int ATK;
    public int DEF;
    public int SPD;
}
