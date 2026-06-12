using System.Collections.Generic;

// ---------------------------------------------------------------------------
//  IFormationService — persists the team the player chose before a run.
// ---------------------------------------------------------------------------
public interface IFormationService
{
    // Slots: index 0-1 = front row, index 2-4 = back row (max 5 heroes)
    IReadOnlyList<FormationSlot> Slots { get; }

    bool IsSlotOccupied(int slotIndex);

    /// <summary>Assign a hero to a slot. Pass null instanceId to clear.</summary>
    void AssignSlot(int slotIndex, string heroInstanceId, bool isFrontRow);

    void ClearSlot(int slotIndex);
    void ClearAll();

    /// <summary>Returns all assigned instance IDs in slot order.</summary>
    List<string> GetAssignedInstanceIds();

    int GetAssignedCount();
}

[System.Serializable]
public class FormationSlot
{
    public int    SlotIndex;
    public string HeroInstanceId;   // empty string = vacant
    public bool   IsFrontRow;

    public bool IsOccupied => !string.IsNullOrEmpty(HeroInstanceId);

    public FormationSlot(int index, bool isFrontRow)
    {
        SlotIndex       = index;
        HeroInstanceId  = string.Empty;
        IsFrontRow      = isFrontRow;
    }
}
