using System;
using System.Collections.Generic;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.Core
{
    /// <summary>
    /// Represents all saved game data for persistence.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        [Header("Player Info")]
        public string PlayerId;
        public long CreatedAt;
        public long LastPlayedAt;

        [Header("Resources")]
        public int Gold;
        public int Gems;
        public int Stones;
        public int Stamina;
        public long LastStaminaRegenTime;

        [Header("Hero Roster")]
        public List<HeroInstance> Heroes = new List<HeroInstance>();

        [Header("Inventory")]
        public List<ItemInstance> Items = new List<ItemInstance>();

        [Header("Discovery")]
        public HashSet<string> DiscoveredHeroIds = new HashSet<string>();

        [Header("Tower Progress")]
        public int CurrentTowerFloor;
        public int HighestTowerFloor;

        [Header("Dungeon Progress")]
        public Dictionary<string, int> DungeonProgress = new Dictionary<string, int>();

        [Header("Facilities")]
        public Dictionary<string, int> FacilityLevels = new Dictionary<string, int>();
        public List<FacilityUpgradeProgress> FacilityUpgradeQueue = new List<FacilityUpgradeProgress>();
        public long LastGoldCollectionTime;
        public long LastExpCollectionTime;
        public long LastHospitalProcessTime;

        [Header("Gacha")]
        public int StandardPityCounter;
        public int PremiumPityCounter;
        public int TotalPulls;
        public List<string> PullHistory = new List<string>();

        /// <summary>
        /// Creates a new save data with default values.
        /// </summary>
        public static GameSaveData CreateNew()
        {
            return new GameSaveData
            {
                PlayerId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now.ToBinary(),
                LastPlayedAt = DateTime.Now.ToBinary(),
                Gold = 5000, // Starting gold
                Gems = 100, // Starting gems
                Stones = 0,
                Stamina = 100,
                LastStaminaRegenTime = DateTime.Now.ToBinary(),
                CurrentTowerFloor = 1,
                HighestTowerFloor = 0,
                StandardPityCounter = 0,
                PremiumPityCounter = 0,
                TotalPulls = 0
            };
        }

        /// <summary>
        /// Updates the last played timestamp.
        /// </summary>
        public void UpdateLastPlayed()
        {
            LastPlayedAt = DateTime.Now.ToBinary();
        }
    }
}