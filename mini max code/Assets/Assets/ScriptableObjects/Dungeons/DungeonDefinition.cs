using System;
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Game.ScriptableObjects
{
    /// <summary>
    /// Defines a dungeon type available in the game.
    /// </summary>
    public enum DungeonType
    {
        Resource,
        Artifact,
        Experience
    }

    /// <summary>
    /// Defines the structure and rewards for a dungeon.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDungeon", menuName = "PickMeUp/Dungeon Definition")]
    public class DungeonDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string DungeonId;
        public string DisplayName;
        public string Description;
        public DungeonType DungeonType;

        [Header("Requirements")]
        public int MinHeroCount = 3;
        public int MaxHeroCount = 5;
        public int BaseStaminaCost = 20;

        [Header("Scaling")]
        public int TotalFloors = 50;
        public float DifficultyScaling = 0.15f; // 15% increase per floor
        public float RewardScaling = 0.10f; // 10% increase per floor

        [Header("Base Rewards (per floor cleared)")]
        public int BaseGoldReward = 100;
        public int BaseExpReward = 50;
        public int BaseStonesReward = 5;

        [Header("Enemy Settings")]
        public List<EnemyWave> EnemyWaves = new List<EnemyWave>();

        [Header("Visual")]
        public Sprite DungeonIcon;
        public Color DungeonThemeColor = Color.gray;
    }

    /// <summary>
    /// Defines an enemy wave configuration for a floor.
    /// </summary>
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

    /// <summary>
    /// Generated floor data for dungeon exploration.
    /// </summary>
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

    /// <summary>
    /// Enemy instance for a floor.
    /// </summary>
    [Serializable]
    public class FloorEnemy
    {
        public string Name;
        public int MaxHealth;
        public int CurrentHealth;
        public int Attack;
        public int Defense;
    }

    /// <summary>
    /// Loot table entry for floor rewards.
    /// </summary>
    [Serializable]
    public class FloorLootTable
    {
        public string ItemId;
        public string ItemName;
        public int DropChance; // 0-100
        public int MinQuantity;
        public int MaxQuantity;
    }
}