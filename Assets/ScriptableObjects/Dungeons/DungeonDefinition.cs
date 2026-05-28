using System;
using System.Collections.Generic;
using UnityEngine;

public enum DungeonType
{
    Resource,
    Artifact,
    Experience
}

[CreateAssetMenu(fileName = "NewDungeonDefinition", menuName = "PickMeUp/Dungeon Definition")]
public class DungeonDefinition : ScriptableObject
{
    public string DungeonId;
    public string DisplayName;
    public string Description;
    public DungeonType DungeonType;
    public int MinHeroCount = 3;
    public int MaxHeroCount = 5;
    public int BaseStaminaCost = 20;
    public int TotalFloors = 50;
    public float DifficultyScaling = 0.15f;
    public float RewardScaling = 0.10f;
    public int BaseGoldReward = 100;
    public int BaseExpReward = 50;
    public int BaseStonesReward = 5;
    public List<EnemyWave> EnemyWaves = new List<EnemyWave>();
    public List<DayOfWeek> AvailableDays = new List<DayOfWeek>();
    public Sprite DungeonIcon;
    public Color DungeonThemeColor = Color.gray;
}

[Serializable]
public class EnemyWave
{
    public string EnemyName;
    public int BaseHealth;
    public int BaseAttack;
    public int BaseDefense;
    public int Quantity;
    public float SpawnWeight = 1.0f;
}

[Serializable]
public class FloorData
{
    public string DungeonId;
    public int FloorNumber;
    public int RecommendedPower;
    public List<FloorEnemy> Enemies = new List<FloorEnemy>();
    public int GoldReward;
    public int ExpReward;
    public int StonesReward;
    public List<FloorLootTable> LootTables = new List<FloorLootTable>();
}

[Serializable]
public class FloorEnemy
{
    public string Name;
    public int MaxHealth;
    public int CurrentHealth;
    public int Attack;
    public int Defense;
}

[Serializable]
public class FloorLootTable
{
    public string ItemId;
    public string ItemName;
    public int DropChance;
    public int MinQuantity;
    public int MaxQuantity;
}
