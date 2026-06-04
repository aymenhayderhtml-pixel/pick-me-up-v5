using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDungeonLootTable", menuName = "PickMeUp/Dungeon/Loot Table")]
public class DungeonLootTableSO : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        [Tooltip("ItemId of the ItemDefinition this entry rolls. Must exist in Resources/Items or ScriptableObjects/Items.")]
        public string ItemId;

        [Tooltip("Display name fallback if the ItemDefinition cannot be resolved.")]
        public string FallbackName;

        [Tooltip("Minimum quantity dropped per successful roll.")]
        [Min(1)] public int MinQuantity = 1;

        [Tooltip("Maximum quantity dropped per successful roll.")]
        [Min(1)] public int MaxQuantity = 1;

        [Tooltip("Relative weight inside the table. Higher = more likely.")]
        [Min(0)] public int Weight = 1;

        [Tooltip("Chance 0..1 that the entry will be rolled at all on a given drop.")]
        [Range(0f, 1f)] public float DropChance = 1f;

        [Tooltip("If true, always grant at least one of this item on dungeon clear, ignoring DropChance.")]
        public bool Guaranteed = false;

        public bool IsValid => !string.IsNullOrEmpty(ItemId) && Weight > 0;
    }

    [Header("Drop Settings")]
    [SerializeField, Min(0)] private int rolls = 3;
    [SerializeField, Min(0f)] private float bonusDropChance = 0f;
    [SerializeField] private List<LootEntry> entries = new List<LootEntry>();

    public int Rolls => Mathf.Max(0, rolls);
    public float BonusDropChance => Mathf.Clamp01(bonusDropChance);
    public IReadOnlyList<LootEntry> Entries => entries;
}

[System.Serializable]
public class DungeonRewardItem
{
    public string ItemId;
    public string DisplayName;
    public int Quantity;
}

[System.Serializable]
public class DungeonRewardResult
{
    public int Gold;
    public int Exp;
    public List<DungeonRewardItem> Items = new List<DungeonRewardItem>();

    public bool IsEmpty
    {
        get
        {
            return Gold <= 0 && Exp <= 0 && (Items == null || Items.Count == 0);
        }
    }
}
