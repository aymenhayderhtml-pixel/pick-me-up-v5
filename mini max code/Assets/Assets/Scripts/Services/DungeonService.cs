using System;
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Game.Core;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.Services
{
    /// <summary>
    /// Service that handles all dungeon-related logic including exploration, rewards, and stamina management.
    /// </summary>
    public class DungeonService : IDungeonService
    {
        private readonly ISaveLoadService _saveLoadService;
        private readonly IGameStateService _gameStateService;
        private readonly ICurrencyService _currencyService;
        private readonly IRosterService _rosterService;
        private readonly IInventoryService _inventoryService;

        private Dictionary<string, DungeonDefinition> _dungeons;
        private Dictionary<string, int> _dungeonProgress;
        private int _currentStamina;
        private int _maxStamina;
        private DateTime _lastStaminaRegenTime;
        private const int StaminaRegenPerMinute = 1;
        private const int DefaultMaxStamina = 100;

        // Dungeon instance data for runtime
        private List<DungeonRunData> _activeRuns = new List<DungeonRunData>();

        public DungeonService(
            ISaveLoadService saveLoadService,
            IGameStateService gameStateService,
            ICurrencyService currencyService,
            IRosterService rosterService,
            IInventoryService inventoryService)
        {
            _saveLoadService = saveLoadService;
            _gameStateService = gameStateService;
            _currencyService = currencyService;
            _rosterService = rosterService;
            _inventoryService = inventoryService;

            LoadDungeons();
            LoadProgress();
        }

        private void LoadDungeons()
        {
            _dungeons = new Dictionary<string, DungeonDefinition>();

            // Load all dungeon definitions from resources
            DungeonDefinition[] dungeonDefs = Resources.LoadAll<DungeonDefinition>("ScriptableObjects/Dungeons");

            foreach (var dungeon in dungeonDefs)
            {
                if (!string.IsNullOrEmpty(dungeon.DungeonId))
                {
                    _dungeons[dungeon.DungeonId] = dungeon;
                }
            }

            // Ensure default dungeons exist
            EnsureDefaultDungeons();
        }

        private void EnsureDefaultDungeons()
        {
            if (!_dungeons.ContainsKey("resource_dungeon"))
            {
                CreateDefaultDungeon("resource_dungeon", "Gold Mine", DungeonType.Resource, 100, 50, 5);
            }
            if (!_dungeons.ContainsKey("artifact_dungeon"))
            {
                CreateDefaultDungeon("artifact_dungeon", "Artifact Vault", DungeonType.Artifact, 150, 75, 10);
            }
            if (!_dungeons.ContainsKey("exp_dungeon"))
            {
                CreateDefaultDungeon("exp_dungeon", "Training Grounds", DungeonType.Experience, 80, 40, 3);
            }
        }

        private void CreateDefaultDungeon(string id, string name, DungeonType type, int gold, int exp, int stones)
        {
            var dungeon = ScriptableObject.CreateInstance<DungeonDefinition>();
            dungeon.DungeonId = id;
            dungeon.DisplayName = name;
            dungeon.DungeonType = type;
            dungeon.BaseGoldReward = gold;
            dungeon.BaseExpReward = exp;
            dungeon.BaseStonesReward = stones;

            _dungeons[id] = dungeon;
        }

        private void LoadProgress()
        {
            var saveData = _saveLoadService.LoadGame();
            _dungeonProgress = new Dictionary<string, int>();

            if (saveData.DungeonProgress != null)
            {
                foreach (var kvp in saveData.DungeonProgress)
                {
                    _dungeonProgress[kvp.Key] = kvp.Value;
                }
            }

            // Load stamina
            _currentStamina = saveData.Stamina;
            _maxStamina = DefaultMaxStamina;

            // Load last regen time
            if (saveData.LastStaminaRegenTime > 0)
            {
                _lastStaminaRegenTime = DateTime.FromBinary(saveData.LastStaminaRegenTime);
            }
            else
            {
                _lastStaminaRegenTime = DateTime.Now;
            }
        }

        private void SaveProgress()
        {
            var saveData = _saveLoadService.LoadGame();
            saveData.Stamina = _currentStamina;
            saveData.DungeonProgress = _dungeonProgress;
            saveData.LastStaminaRegenTime = _lastStaminaRegenTime.ToBinary();
            _saveLoadService.SaveGame(saveData);
        }

        public List<DungeonDefinition> GetAllDungeons()
        {
            return new List<DungeonDefinition>(_dungeons.Values);
        }

        public DungeonDefinition GetDungeon(string dungeonId)
        {
            return _dungeons.ContainsKey(dungeonId) ? _dungeons[dungeonId] : null;
        }

        public int GetDungeonProgress(string dungeonId)
        {
            return _dungeonProgress.ContainsKey(dungeonId) ? _dungeonProgress[dungeonId] : 0;
        }

        public int GetStamina()
        {
            return _currentStamina;
        }

        public int GetMaxStamina()
        {
            return _maxStamina;
        }

        public bool CanRunDungeon(string dungeonId, int staminaCost)
        {
            if (!_dungeons.ContainsKey(dungeonId))
                return false;

            return _currentStamina >= staminaCost;
        }

        public DungeonRunResult RunDungeon(string dungeonId, List<string> heroInstanceIds)
        {
            var result = new DungeonRunResult
            {
                DungeonId = dungeonId,
                Success = false
            };

            var dungeon = GetDungeon(dungeonId);
            if (dungeon == null)
            {
                result.FailureReason = "Invalid dungeon";
                return result;
            }

            // Calculate stamina cost (increases with floor)
            int currentFloor = GetDungeonProgress(dungeonId);
            int staminaCost = CalculateStaminaCost(dungeon, currentFloor);

            // Check stamina
            if (_currentStamina < staminaCost)
            {
                result.FailureReason = "Not enough stamina";
                return result;
            }

            // Validate team
            if (heroInstanceIds == null || heroInstanceIds.Count < dungeon.MinHeroCount)
            {
                result.FailureReason = $"Need at least {dungeon.MinHeroCount} heroes";
                return result;
            }

            if (heroInstanceIds.Count > dungeon.MaxHeroCount)
            {
                result.FailureReason = $"Maximum {dungeon.MaxHeroCount} heroes allowed";
                return result;
            }

            // Consume stamina
            _currentStamina -= staminaCost;

            // Generate floor data
            FloorData floorData = GenerateFloor(dungeonId, currentFloor + 1);

            // Calculate team power
            int teamPower = CalculateTeamPower(heroInstanceIds);

            // Run simulation
            result = SimulateDungeonRun(dungeon, floorData, heroInstanceIds, teamPower);

            // Apply morale effects
            if (result.Success)
            {
                result.MoraleEffects = ApplyMoraleEffects(heroInstanceIds, dungeon.DungeonType, result.Success);
                _dungeonProgress[dungeonId] = currentFloor + 1;
            }
            else
            {
                result.MoraleEffects = ApplyMoraleEffects(heroInstanceIds, dungeon.DungeonType, false);
            }

            // Save progress
            SaveProgress();

            return result;
        }

        private int CalculateStaminaCost(DungeonDefinition dungeon, int currentFloor)
        {
            float scaling = 1f + (currentFloor * 0.05f);
            return Mathf.CeilToInt(dungeon.BaseStaminaCost * scaling);
        }

        public FloorData GenerateFloor(string dungeonId, int floorNumber)
        {
            var dungeon = GetDungeon(dungeonId);
            if (dungeon == null)
                return null;

            var floorData = new FloorData
            {
                DungeonId = dungeonId,
                FloorNumber = floorNumber
            };

            // Calculate difficulty scaling
            float diffMultiplier = 1f + (floorNumber - 1) * dungeon.DifficultyScaling;
            float rewardMultiplier = 1f + (floorNumber - 1) * dungeon.RewardScaling;

            // Generate enemies based on dungeon type
            List<FloorEnemy> enemies = GenerateEnemies(dungeon, floorNumber, diffMultiplier);
            floorData.Enemies = enemies;

            // Calculate total enemy power for recommended team power
            floorData.RecommendedPower = CalculateEnemyPower(enemies);

            // Calculate rewards
            floorData.GoldReward = Mathf.CeilToInt(dungeon.BaseGoldReward * rewardMultiplier);
            floorData.ExpReward = Mathf.CeilToInt(dungeon.BaseExpReward * rewardMultiplier);
            floorData.StonesReward = Mathf.CeilToInt(dungeon.BaseStonesReward * rewardMultiplier);

            // Add loot table entries
            AddLootTables(floorData, dungeon, floorNumber);

            return floorData;
        }

        private List<FloorEnemy> GenerateEnemies(DungeonDefinition dungeon, int floor, float diffMultiplier)
        {
            var enemies = new List<FloorEnemy>();

            // Generate 1-3 waves of enemies
            int waveCount = Mathf.Min(1 + (floor / 10), 3);

            for (int w = 0; w < waveCount; w++)
            {
                if (dungeon.EnemyWaves.Count == 0)
                {
                    // Generate default enemies
                    var enemy = new FloorEnemy
                    {
                        Name = $"Floor {floor} Creature",
                        MaxHealth = Mathf.CeilToInt(100 * diffMultiplier),
                        CurrentHealth = Mathf.CeilToInt(100 * diffMultiplier),
                        Attack = Mathf.CeilToInt(15 * diffMultiplier),
                        Defense = Mathf.CeilToInt(10 * diffMultiplier)
                    };
                    enemies.Add(enemy);
                }
                else
                {
                    // Use weighted random to select enemy type
                    var selectedWave = SelectWeightedEnemy(dungeon.EnemyWaves);

                    var enemy = new FloorEnemy
                    {
                        Name = selectedWave.EnemyName,
                        MaxHealth = Mathf.CeilToInt(selectedWave.BaseHealth * diffMultiplier),
                        CurrentHealth = Mathf.CeilToInt(selectedWave.BaseHealth * diffMultiplier),
                        Attack = Mathf.CeilToInt(selectedWave.BaseAttack * diffMultiplier),
                        Defense = Mathf.CeilToInt(selectedWave.BaseDefense * diffMultiplier)
                    };

                    // Add multiple of same enemy based on quantity
                    for (int i = 0; i < selectedWave.Quantity; i++)
                    {
                        enemies.Add(new FloorEnemy
                        {
                            Name = enemy.Name,
                            MaxHealth = enemy.MaxHealth,
                            CurrentHealth = enemy.CurrentHealth,
                            Attack = enemy.Attack,
                            Defense = enemy.Defense
                        });
                    }
                }
            }

            return enemies;
        }

        private EnemyWave SelectWeightedEnemy(List<EnemyWave> waves)
        {
            float totalWeight = 0f;
            foreach (var wave in waves)
            {
                totalWeight += wave.SpawnWeight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (var wave in waves)
            {
                cumulativeWeight += wave.SpawnWeight;
                if (randomValue <= cumulativeWeight)
                {
                    return wave;
                }
            }

            return waves[waves.Count - 1];
        }

        private int CalculateEnemyPower(List<FloorEnemy> enemies)
        {
            int totalPower = 0;
            foreach (var enemy in enemies)
            {
                totalPower += enemy.MaxHealth / 10 + enemy.Attack + enemy.Defense;
            }
            return totalPower;
        }

        private void AddLootTables(FloorData floorData, DungeonDefinition dungeon, int floor)
        {
            // Add type-specific loot
            switch (dungeon.DungeonType)
            {
                case DungeonType.Artifact:
                    floorData.LootTables.Add(new FloorLootTable
                    {
                        ItemId = "artifact_shard",
                        ItemName = "Artifact Shard",
                        DropChance = 30 + floor,
                        MinQuantity = 1,
                        MaxQuantity = 3
                    });
                    break;

                case DungeonType.Experience:
                    floorData.LootTables.Add(new FloorLootTable
                    {
                        ItemId = "exp_scroll",
                        ItemName = "EXP Scroll",
                        DropChance = 40 + floor,
                        MinQuantity = 1,
                        MaxQuantity = 2
                    });
                    break;

                case DungeonType.Resource:
                default:
                    // More gold drops
                    floorData.GoldReward = Mathf.CeilToInt(floorData.GoldReward * 1.2f);
                    break;
            }
        }

        private DungeonRunResult SimulateDungeonRun(DungeonDefinition dungeon, FloorData floorData, List<string> heroIds, int teamPower)
        {
            var result = new DungeonRunResult
            {
                DungeonId = dungeon.DungeonId,
                FloorCleared = floorData.FloorNumber,
                Success = false
            };

            // Calculate combat outcome
            int enemyPower = floorData.RecommendedPower;
            float powerRatio = (float)teamPower / enemyPower;

            // Victory if team power >= 70% of enemy power
            bool victory = powerRatio >= 0.7f;

            if (victory)
            {
                result.Success = true;

                // Calculate rewards based on power ratio (bonus for overgearing)
                float rewardBonus = powerRatio > 1f ? (powerRatio - 1f) * 0.5f : 0f;

                result.GoldEarned = Mathf.CeilToInt(floorData.GoldReward * (1f + rewardBonus));
                result.ExpEarned = Mathf.CeilToInt(floorData.ExpReward * (1f + rewardBonus));

                // Stone drop chance based on dungeon type
                if (dungeon.DungeonType == DungeonType.Resource || UnityEngine.Random.value < 0.5f)
                {
                    result.StonesEarned = floorData.StonesReward;
                }

                // Process loot drops
                result.EarnedItemIds = ProcessLootDrops(floorData.LootTables);

                // Apply rewards
                _currencyService.AddGold(result.GoldEarned);
                _currencyService.AddStones(result.StonesEarned);

                result.AffectedHeroIds = heroIds;
            }
            else
            {
                result.Success = false;
                result.FailureReason = "Team power too low to clear this floor";

                // Partial rewards for attempting
                result.GoldEarned = Mathf.CeilToInt(floorData.GoldReward * 0.3f);
                result.ExpEarned = Mathf.CeilToInt(floorData.ExpReward * 0.2f);
                _currencyService.AddGold(result.GoldEarned);

                result.AffectedHeroIds = heroIds;
            }

            return result;
        }

        private List<string> ProcessLootDrops(List<FloorLootTable> lootTables)
        {
            var earnedItems = new List<string>();

            foreach (var lootTable in lootTables)
            {
                // Roll for drop
                if (UnityEngine.Random.Range(0, 100) < lootTable.DropChance)
                {
                    int quantity = UnityEngine.Random.Range(lootTable.MinQuantity, lootTable.MaxQuantity + 1);

                    // Create item instances
                    for (int i = 0; i < quantity; i++)
                    {
                        string itemId = _inventoryService.CreateItemInstance(lootTable.ItemId);
                        if (!string.IsNullOrEmpty(itemId))
                        {
                            earnedItems.Add(itemId);
                        }
                    }
                }
            }

            return earnedItems;
        }

        private List<HeroMoraleEffect> ApplyMoraleEffects(List<string> heroIds, DungeonType dungeonType, bool success)
        {
            var effects = new List<HeroMoraleEffect>();

            int moraleChange = success ? -10 : -25;
            string reason = success ? "Dungeon exploration fatigue" : "Failed dungeon attempt";

            // Different effects based on dungeon type
            switch (dungeonType)
            {
                case DungeonType.Experience:
                    moraleChange = success ? -5 : -15; // Training is less exhausting
                    reason = success ? "Intensive training" : "Training failure";
                    break;

                case DungeonType.Artifact:
                    moraleChange = success ? -15 : -30; // Artifact hunting is risky
                    reason = success ? "Artifact hunting exertion" : "Dangerous expedition";
                    break;
            }

            foreach (var heroId in heroIds)
            {
                var hero = _rosterService.GetHero(heroId);
                if (hero != null)
                {
                    hero.CurrentMorale = Mathf.Max(0, hero.CurrentMorale + moraleChange);
                    _rosterService.UpdateHero(hero);

                    effects.Add(new HeroMoraleEffect
                    {
                        HeroInstanceId = heroId,
                        MoraleChange = moraleChange,
                        Reason = reason
                    });
                }
            }

            return effects;
        }

        public int CalculateTeamPower(List<string> heroInstanceIds)
        {
            int totalPower = 0;

            foreach (var heroId in heroInstanceIds)
            {
                var hero = _rosterService.GetHero(heroId);
                if (hero != null)
                {
                    // Calculate hero power based on stats and level
                    int heroPower = hero.Level * 10;
                    heroPower += hero.Attack + hero.Defense + hero.Health;
                    heroPower += hero.StarRank * 20; // Bonus from star rank

                    // Equipment bonuses
                    if (!string.IsNullOrEmpty(hero.WeaponId))
                        heroPower += 10;
                    if (!string.IsNullOrEmpty(hero.ArmorId))
                        heroPower += 10;
                    if (!string.IsNullOrEmpty(hero.AccessoryId))
                        heroPower += 5;

                    totalPower += heroPower;
                }
            }

            return totalPower;
        }

        public void RechargeStamina(int amount)
        {
            _currentStamina = Mathf.Min(_currentStamina + amount, _maxStamina);
            SaveProgress();
        }

        public void ProcessStaminaRegeneration()
        {
            var now = DateTime.Now;
            var timeSinceLastRegen = now - _lastStaminaRegenTime;

            int minutesPassed = (int)timeSinceLastRegen.TotalMinutes;
            if (minutesPassed > 0)
            {
                int staminaToAdd = minutesPassed * StaminaRegenPerMinute;
                _currentStamina = Mathf.Min(_currentStamina + staminaToAdd, _maxStamina);
                _lastStaminaRegenTime = now;
                SaveProgress();
            }
        }

        public void ResetProgress()
        {
            _dungeonProgress.Clear();
            _currentStamina = _maxStamina;
            _lastStaminaRegenTime = DateTime.Now;
            SaveProgress();
        }
    }

    /// <summary>
    /// Internal class for tracking active dungeon runs.
    /// </summary>
    [Serializable]
    internal class DungeonRunData
    {
        public string DungeonId;
        public int CurrentFloor;
        public List<string> HeroIds;
        public DateTime StartTime;
        public bool Completed;
    }
}