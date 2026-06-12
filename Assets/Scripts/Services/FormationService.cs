using System.Collections.Generic;
using UnityEngine;

// ---------------------------------------------------------------------------
//  FormationService
//  In-memory only (not serialised to disk — the player re-picks each session).
//  Slot layout:  0 = front-left, 1 = front-right, 2-4 = back row
// ---------------------------------------------------------------------------
public class FormationService : IFormationService
{
    private const int TotalSlots    = 5;
    private const int FrontRowCount = 2;

    private readonly List<FormationSlot> _slots;

    public IReadOnlyList<FormationSlot> Slots => _slots;

    public FormationService()
    {
        _slots = new List<FormationSlot>(TotalSlots);
        for (int i = 0; i < TotalSlots; i++)
        {
            _slots.Add(new FormationSlot(i, isFrontRow: i < FrontRowCount));
        }
    }

    // -----------------------------------------------------------------------
    public bool IsSlotOccupied(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return false;
        return _slots[slotIndex].IsOccupied;
    }

    public void AssignSlot(int slotIndex, string heroInstanceId, bool isFrontRow)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count)
        {
            Debug.LogWarning($"[FormationService] AssignSlot: invalid index {slotIndex}");
            return;
        }

        // Prevent double-assignment of the same hero
        if (!string.IsNullOrEmpty(heroInstanceId))
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].HeroInstanceId == heroInstanceId)
                {
                    _slots[i].HeroInstanceId = string.Empty;
                }
            }
        }

        _slots[slotIndex].HeroInstanceId = heroInstanceId ?? string.Empty;
        _slots[slotIndex].IsFrontRow      = isFrontRow;
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;
        _slots[slotIndex].HeroInstanceId = string.Empty;
    }

    public void ClearAll()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].HeroInstanceId = string.Empty;
        }
    }

    public List<string> GetAssignedInstanceIds()
    {
        List<string> ids = new List<string>();
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].IsOccupied)
                ids.Add(_slots[i].HeroInstanceId);
        }
        return ids;
    }

    public int GetAssignedCount()
    {
        int count = 0;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].IsOccupied) count++;
        }
        return count;
    }
}
