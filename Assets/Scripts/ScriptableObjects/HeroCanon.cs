using UnityEngine;

// ---------------------------------------------------------------------------
//  HeroCanon - static data for the 10 canonical heroes.
//  Used by editor tools to generate HeroDefinition .asset files.
//  Not a MonoBehaviour; just a data container.
// ---------------------------------------------------------------------------
public static class HeroCanon
{
    public static readonly HeroTemplate[] Heroes = new HeroTemplate[]
    {
        // ── Fire ──────────────────────────────────────────────────────
        new HeroTemplate
        {
            HeroId = "hero_kael",
            HeroName = "Kael",
            BaseClass = HeroClass.Warrior,
            Element = HeroElement.Fire,
            BaseStarRank = 3,
            BaseHP = 340, BaseATK = 85, BaseDEF = 45, BaseSPD = 50,
            LoreText = "A disgraced knight who still carries the flame of his fallen order. His blade burns with vengeance."
        },
        new HeroTemplate
        {
            HeroId = "hero_lyra",
            HeroName = "Lyra",
            BaseClass = HeroClass.Mage,
            Element = HeroElement.Fire,
            BaseStarRank = 4,
            BaseHP = 230, BaseATK = 120, BaseDEF = 25, BaseSPD = 62,
            LoreText = "Pyromancer of the Ashen Circle. She claims fire speaks to her, and it always demands more fuel."
        },

        // ── Water ─────────────────────────────────────────────────────
        new HeroTemplate
        {
            HeroId = "hero_maren",
            HeroName = "Maren",
            BaseClass = HeroClass.Healer,
            Element = HeroElement.Water,
            BaseStarRank = 3,
            BaseHP = 290, BaseATK = 55, BaseDEF = 42, BaseSPD = 58,
            LoreText = "Tide-witch of the Singmirel coast. Her waters mend wounds, but never erase the memory of pain."
        },
        new HeroTemplate
        {
            HeroId = "hero_brin",
            HeroName = "Brin",
            BaseClass = HeroClass.Tank,
            Element = HeroElement.Water,
            BaseStarRank = 2,
            BaseHP = 520, BaseATK = 50, BaseDEF = 95, BaseSPD = 32,
            LoreText = "Deep-dweller who surfaced once and found the surface world too bright, too loud, too fragile."
        },

        // ── Wind ──────────────────────────────────────────────────────
        new HeroTemplate
        {
            HeroId = "hero_sylas",
            HeroName = "Sylas",
            BaseClass = HeroClass.Ranger,
            Element = HeroElement.Wind,
            BaseStarRank = 3,
            BaseHP = 265, BaseATK = 95, BaseDEF = 32, BaseSPD = 78,
            LoreText = "Wind-runner who never touches the ground longer than a heartbeat. The sky is his only loyalty."
        },
        new HeroTemplate
        {
            HeroId = "hero_aira",
            HeroName = "Aira",
            BaseClass = HeroClass.Mage,
            Element = HeroElement.Wind,
            BaseStarRank = 2,
            BaseHP = 210, BaseATK = 100, BaseDEF = 28, BaseSPD = 70,
            LoreText = "Storm-caller still learning control. Every lightning strike is a conversation with something vast and angry."
        },

        // ── Earth ─────────────────────────────────────────────────────
        new HeroTemplate
        {
            HeroId = "hero_thorne",
            HeroName = "Thorne",
            BaseClass = HeroClass.Tank,
            Element = HeroElement.Earth,
            BaseStarRank = 4,
            BaseHP = 550, BaseATK = 60, BaseDEF = 100, BaseSPD = 30,
            LoreText = "Living mountain who remembers every footstep across his skin. Patient. Unforgiving. Eternal."
        },
        new HeroTemplate
        {
            HeroId = "hero_fern",
            HeroName = "Fern",
            BaseClass = HeroClass.Healer,
            Element = HeroElement.Earth,
            BaseStarRank = 2,
            BaseHP = 270, BaseATK = 48, BaseDEF = 45, BaseSPD = 55,
            LoreText = "Moss-witch of Kendert Forest. She grows medicines in her own hair and speaks to roots."
        },

        // ── Light ─────────────────────────────────────────────────────
        new HeroTemplate
        {
            HeroId = "hero_sera",
            HeroName = "Sera",
            BaseClass = HeroClass.Warrior,
            Element = HeroElement.Light,
            BaseStarRank = 5,
            BaseHP = 360, BaseATK = 95, BaseDEF = 55, BaseSPD = 58,
            LoreText = "Last paladin of the Luminous Order. Her light does not comfort - it judges, and it burns."
        },

        // ── Dark ──────────────────────────────────────────────────────
        new HeroTemplate
        {
            HeroId = "hero_vex",
            HeroName = "Vex",
            BaseClass = HeroClass.Ranger,
            Element = HeroElement.Dark,
            BaseStarRank = 5,
            BaseHP = 250, BaseATK = 110, BaseDEF = 25, BaseSPD = 88,
            LoreText = "Shadow-thief who steals secrets, not gold. Every truth he uncovers costs someone their life."
        }
    };
}

[System.Serializable]
public class HeroTemplate
{
    public string HeroId;
    public string HeroName;
    public HeroClass BaseClass;
    public HeroElement Element;
    public int BaseStarRank;
    public int BaseHP;
    public int BaseATK;
    public int BaseDEF;
    public int BaseSPD;
    public string LoreText;
}
