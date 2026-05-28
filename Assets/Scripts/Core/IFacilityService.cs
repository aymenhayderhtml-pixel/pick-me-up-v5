using System;
using System.Collections.Generic;

public interface IFacilityService
{
    List<FacilityDefinition> GetAllFacilities();
    FacilityDefinition GetFacility(string facilityId);
    int GetFacilityLevel(string facilityId);
    UpgradeCost GetUpgradeCost(string facilityId);
    bool CanUpgrade(string facilityId);
    bool StartUpgrade(string facilityId);
    bool InstantUpgrade(string facilityId);
    float GetCurrentBenefit(string facilityId);
    float GetNextLevelBenefit(string facilityId);
    string GetFacilityRole(string facilityId);
    string GetFacilityEmotion(string facilityId);
    bool IsShadowFacility(string facilityId);
    DockSortieType GetQueuedSortieType();
    bool ToggleSortieType(DockSortieType sortieType);
    bool LaunchSortie(out DockSortieResult result);
    void ProcessPassiveGeneration();
    int CollectGold();
    int CollectExp();
    int CollectMemorialFragments();
    void ProcessHospitalRecovery();
    void RecoverHeroMorale(string heroInstanceId, int amount);
    void ResetProgress();
}

[Serializable]
public enum DockSortieType
{
    Recon,
    Supply,
    Extraction
}

[Serializable]
public class DockSortieResult
{
    public DockSortieType SortieType;
    public bool Success;
    public string Summary;
    public int GoldSpent;
    public int GemsSpent;
    public int AttributeStonesSpent;
    public int MemorialFragmentsGained;
    public int StaminaRestored;
    public int MoraleChange;
}
