using System;
using UnityEngine;

public enum PersonalityTrait
{
    Brave,
    Cowardly,
    Reckless,
    Disciplined,
    Loyal,
    Traumatized
}

[Serializable]
public class HeroInstance
{
    public string InstanceId;
    public string HeroDefId;
    public bool IsAlive;
    public int CurrentStarRank;
    
    [Range(0f, 1f)]
    public float HiddenPotential;
    
    public PersonalityTrait Personality;
    public string SpecialOrgan;
    public int Morale;
    public long AcquiredTimestampTicks;
    public int PromotionRank;
    public string WeaponId;
    public string ArmorId;
    public string AccessoryId;
    public bool IsLocked;

    // Parameterless constructor required for JsonUtility deserialization
    public HeroInstance() 
    {
        IsAlive = true;
        WeaponId = string.Empty;
        ArmorId = string.Empty;
        AccessoryId = string.Empty;
        IsLocked = false;
    }

    public HeroInstance(string heroDefId, int baseStarRank)
    {
        InstanceId = Guid.NewGuid().ToString();
        HeroDefId = heroDefId;
        IsAlive = true;
        CurrentStarRank = baseStarRank;
        HiddenPotential = UnityEngine.Random.Range(0f, 1f);
        SpecialOrgan = string.Empty;
        PromotionRank = 0;

        // Assigned 40-60 random on creation
        Morale = UnityEngine.Random.Range(40, 61); 
        
        // DateTime stored as long ticks
        AcquiredTimestampTicks = DateTime.UtcNow.Ticks;
        WeaponId = string.Empty;
        ArmorId = string.Empty;
        AccessoryId = string.Empty;
        IsLocked = false;

        // Random personality trait assignment
        Array traits = Enum.GetValues(typeof(PersonalityTrait));
        Personality = (PersonalityTrait)traits.GetValue(UnityEngine.Random.Range(0, traits.Length));
    }

    public Guid GetInstanceIdGuid()
    {
        return Guid.Parse(InstanceId);
    }

    public DateTime GetAcquiredDateTime()
    {
        return new DateTime(AcquiredTimestampTicks, DateTimeKind.Utc);
    }
}
