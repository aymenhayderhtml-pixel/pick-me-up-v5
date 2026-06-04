using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDungeonData", menuName = "PickMeUp/Dungeon/Dungeon Data")]
public class DungeonDataSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id = "dungeon_unnamed";
    [SerializeField] private string displayName = "Unnamed Dungeon";
    [SerializeField, TextArea(2, 4)] private string description = string.Empty;

    [Header("Visuals")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite backgroundArt;

    [Header("Entry Requirements")]
    [SerializeField, Min(0)] private int staminaCost = 10;
    [SerializeField, Min(1)] private int minPlayerLevel = 1;

    [Header("Progression")]
    [SerializeField] private List<EnemyWaveSO> waves = new List<EnemyWaveSO>();

    [Header("Base Rewards")]
    [SerializeField, Min(0)] private int baseGoldReward = 100;
    [SerializeField, Min(0)] private int baseExpReward = 50;
    [SerializeField] private ScriptableObject lootTable;
    [SerializeField] private DungeonLootTableSO typedLootTable;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public Sprite BackgroundArt => backgroundArt;
    public int StaminaCost => staminaCost;
    public int MinPlayerLevel => minPlayerLevel;
    public IReadOnlyList<EnemyWaveSO> Waves => waves;
    public int TotalWaves => waves != null ? waves.Count : 0;
    public int BaseGoldReward => baseGoldReward;
    public int BaseExpReward => baseExpReward;
    public ScriptableObject LootTable => lootTable;
    public DungeonLootTableSO TypedLootTable => typedLootTable;

    public bool CanAttempt(int currentStamina, int currentLevel)
    {
        return currentStamina >= staminaCost && currentLevel >= minPlayerLevel;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = string.IsNullOrWhiteSpace(displayName)
                ? "dungeon_unnamed"
                : "dungeon_" + displayName.Trim().ToLowerInvariant().Replace(" ", "_");
        }

        if (staminaCost < 0) staminaCost = 0;
        if (minPlayerLevel < 1) minPlayerLevel = 1;
    }
}
