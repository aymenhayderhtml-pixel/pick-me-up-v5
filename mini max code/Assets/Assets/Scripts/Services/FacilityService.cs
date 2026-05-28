using System;
using System.Collections.Generic;
using UnityEngine;
using PickMeUp.Game.Core;
using PickMeUp.Game.ScriptableObjects;

namespace PickMeUp.Game.Services
{
    /// <summary>
    /// Service that handles facility management including upgrades and passive generation.
    /// </summary>
    public class FacilityService : IFacilityService
    {
        private readonly ISaveLoadService _saveLoadService;
        private readonly ICurrencyService _currencyService;
        private readonly IRosterService _rosterService;
        private readonly IGameStateService _gameStateService;

        private Dictionary<string, FacilityDefinition> _facilities;
        private Dictionary<string, int> _facilityLevels;
        private List<FacilityUpgradeProgress> _upgradeQueue;
        private DateTime _lastGoldCollectionTime;
        private DateTime _lastExpCollectionTime;
        private DateTime _lastHospitalProcessTime;

        // Passive generation intervals (in seconds)
        private const float GoldMineInterval = 3600f; // 1 hour
        private const float TrainingGroundInterval = 3600f; // 1 hour
        private const float HospitalCheckInterval = 300f; // 5 minutes

        public FacilityService(
            ISaveLoadService saveLoadService,
            ICurrencyService currencyService,
            IRosterService rosterService,
            IGameStateService gameStateService)
        {
            _saveLoadService = saveLoadService;
            _currencyService = currencyService;
            _rosterService = rosterService;
            _gameStateService = gameStateService;

            _facilities = new Dictionary<string, FacilityDefinition>();
            _facilityLevels = new Dictionary<string, int>();
            _upgradeQueue = new List<FacilityUpgradeProgress>();

            LoadFacilities();
            LoadProgress();
        }

        private void LoadFacilities()
        {
            _facilities.Clear();

            // Load all facility definitions from resources
            var definitions = Resources.LoadAll<FacilityDefinition>("ScriptableObjects/Facilities");

            foreach (var facility in definitions)
            {
                if (!string.IsNullOrEmpty(facility.FacilityId))
                {
                    _facilities[facility.FacilityId] = facility;
                }
            }

            // Ensure default facilities exist
            EnsureDefaultFacilities();
        }

        private void EnsureDefaultFacilities()
        {
            CreateDefaultFacility("training_ground", "Training Ground", FacilityType.TrainingGround,
                new float[] { 50, 75, 100, 125, 150, 175, 200, 225, 250, 300 },
                new int[] { 500, 750, 1000, 1500, 2000, 3000, 4000, 5000, 7500, 10000 });

            CreateDefaultFacility("hospital", "Hospital", FacilityType.Hospital,
                new float[] { 10, 15, 20, 25, 30, 35, 40, 45, 50, 60 },
                new int[] { 300, 500, 800, 1200, 1600, 2200, 3000, 4000, 5500, 7000 });

            CreateDefaultFacility("alchemy_lab", "Alchemy Lab", FacilityType.AlchemyLab,
                new float[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 },
                new int[] { 400, 600, 900, 1300, 1800, 2500, 3500, 4500, 6000, 8000 });

            CreateDefaultFacility("gold_mine", "Gold Mine", FacilityType.GoldMine,
                new float[] { 100, 200, 350, 500, 700, 900, 1200, 1500, 2000, 2500 },
                new int[] { 600, 900, 1300, 1800, 2500, 3500, 4500, 6000, 8000, 10000 });
        }

        private void CreateDefaultFacility(string id, string name, FacilityType type, float[] benefits, int[] costs)
        {
            if (!_facilities.ContainsKey(id))
            {
                var facility = ScriptableObject.CreateInstance<FacilityDefinition>();
                facility.FacilityId = id;
                facility.DisplayName = name;
                facility.FacilityType = type;
                facility.BenefitPerLevel = benefits;
                facility.GoldCostPerLevel = costs;
                facility.StartingLevel = 1;

                _facilities[id] = facility;
            }
        }

        private void LoadProgress()
        {
            var saveData = _saveLoadService.LoadGame();

            // Load facility levels
            _facilityLevels.Clear();
            if (saveData.FacilityLevels != null)
            {
                foreach (var kvp in saveData.FacilityLevels)
                {
                    _facilityLevels[kvp.Key] = kvp.Value;
                }
            }

            // Initialize default levels for facilities not in save
            foreach (var facility in _facilities)
            {
                if (!_facilityLevels.ContainsKey(facility.Key))
                {
                    _facilityLevels[facility.Key] = facility.Value.StartingLevel;
                }
            }

            // Load upgrade queue
            _upgradeQueue.Clear();
            if (saveData.FacilityUpgradeQueue != null)
            {
                foreach (var progress in saveData.FacilityUpgradeQueue)
                {
                    _upgradeQueue.Add(progress);
                }
            }

            // Load collection timestamps
            _lastGoldCollectionTime = saveData.LastGoldCollectionTime > 0
                ? DateTime.FromBinary(saveData.LastGoldCollectionTime)
                : DateTime.Now;

            _lastExpCollectionTime = saveData.LastExpCollectionTime > 0
                ? DateTime.FromBinary(saveData.LastExpCollectionTime)
                : DateTime.Now;

            _lastHospitalProcessTime = saveData.LastHospitalProcessTime > 0
                ? DateTime.FromBinary(saveData.LastHospitalProcessTime)
                : DateTime.Now;
        }

        private void SaveProgress()
        {
            var saveData = _saveLoadService.LoadGame();
            saveData.FacilityLevels = _facilityLevels;
            saveData.FacilityUpgradeQueue = _upgradeQueue;
            saveData.LastGoldCollectionTime = _lastGoldCollectionTime.ToBinary();
            saveData.LastExpCollectionTime = _lastExpCollectionTime.ToBinary();
            saveData.LastHospitalProcessTime = _lastHospitalProcessTime.ToBinary();
            _saveLoadService.SaveGame(saveData);
        }

        public List<FacilityDefinition> GetAllFacilities()
        {
            return new List<FacilityDefinition>(_facilities.Values);
        }

        public FacilityDefinition GetFacility(string facilityId)
        {
            return _facilities.ContainsKey(facilityId) ? _facilities[facilityId] : null;
        }

        public int GetFacilityLevel(string facilityId)
        {
            return _facilityLevels.ContainsKey(facilityId) ? _facilityLevels[facilityId] : 1;
        }

        public UpgradeCost GetUpgradeCost(string facilityId)
        {
            var facility = GetFacility(facilityId);
            if (facility == null)
                return new UpgradeCost();

            int currentLevel = GetFacilityLevel(facilityId);
            int nextLevel = currentLevel + 1;

            if (nextLevel > facility.MaxLevel)
            {
                return new UpgradeCost { IsFree = true };
            }

            return new UpgradeCost
            {
                Gold = facility.GetUpgradeCost(nextLevel),
                Gems = facility.GetGemCost(nextLevel),
                TimeSeconds = facility.GetUpgradeTime(nextLevel),
                IsFree = false
            };
        }

        public bool CanUpgrade(string facilityId)
        {
            var facility = GetFacility(facilityId);
            if (facility == null)
                return false;

            int currentLevel = GetFacilityLevel(facilityId);
            if (currentLevel >= facility.MaxLevel)
                return false;

            var cost = GetUpgradeCost(facilityId);
            if (cost.IsFree)
                return true;

            // Check if player has enough resources
            return _currencyService.GetGold() >= cost.Gold &&
                   _currencyService.GetGems() >= cost.Gems;
        }

        public bool StartUpgrade(string facilityId)
        {
            var facility = GetFacility(facilityId);
            if (facility == null)
                return false;

            int currentLevel = GetFacilityLevel(facilityId);
            if (currentLevel >= facility.MaxLevel)
                return false;

            var cost = GetUpgradeCost(facilityId);
            if (!cost.IsFree)
            {
                // Deduct resources
                if (!_currencyService.SpendGold(cost.Gold))
                    return false;

                if (cost.Gems > 0 && !_currencyService.SpendGems(cost.Gems))
                {
                    // Refund gold if gem deduction fails
                    _currencyService.AddGold(cost.Gold);
                    return false;
                }
            }

            // Create upgrade progress
            var progress = new FacilityUpgradeProgress
            {
                FacilityId = facilityId,
                TargetLevel = currentLevel + 1,
                StartTime = DateTime.Now,
                CompletionTime = DateTime.Now.AddSeconds(cost.TimeSeconds),
                IsCompleted = cost.TimeSeconds <= 0
            };

            _upgradeQueue.Add(progress);
            SaveProgress();

            Debug.Log($"Started upgrade for {facility.DisplayName} to level {progress.TargetLevel}");

            return true;
        }

        public bool InstantUpgrade(string facilityId)
        {
            var facility = GetFacility(facilityId);
            if (facility == null)
                return false;

            // Find upgrade in queue
            var upgrade = _upgradeQueue.Find(u => u.FacilityId == facilityId && !u.IsCompleted);
            if (upgrade == null)
                return false;

            // Calculate gem cost for instant completion
            int gemCost = Mathf.CeilToInt(upgrade.TimeSeconds / 60f); // 1 gem per minute
            if (!_currencyService.SpendGems(gemCost))
                return false;

            // Complete upgrade
            CompleteUpgrade(upgrade);
            return true;
        }

        public float GetCurrentBenefit(string facilityId)
        {
            var facility = GetFacility(facilityId);
            if (facility == null)
                return 0f;

            int level = GetFacilityLevel(facilityId);
            return facility.GetBenefitAtLevel(level);
        }

        public float GetNextLevelBenefit(string facilityId)
        {
            var facility = GetFacility(facilityId);
            if (facility == null)
                return 0f;

            int currentLevel = GetFacilityLevel(facilityId);
            if (currentLevel >= facility.MaxLevel)
                return facility.GetBenefitAtLevel(currentLevel);

            return facility.GetBenefitAtLevel(currentLevel + 1);
        }

        public void ProcessPassiveGeneration()
        {
            var now = DateTime.Now;

            // Process upgrade completions
            ProcessUpgradeCompletions(now);

            // Process Gold Mine
            ProcessGoldMineGeneration(now);

            // Process Training Ground
            ProcessTrainingGroundGeneration(now);

            // Process Hospital
            ProcessHospitalRecovery(now);
        }

        private void ProcessUpgradeCompletions(DateTime now)
        {
            var completedUpgrades = new List<FacilityUpgradeProgress>();

            foreach (var upgrade in _upgradeQueue)
            {
                if (!upgrade.IsCompleted && now >= upgrade.CompletionTime)
                {
                    CompleteUpgrade(upgrade);
                    completedUpgrades.Add(upgrade);
                }
            }

            // Remove completed upgrades from queue
            foreach (var completed in completedUpgrades)
            {
                _upgradeQueue.Remove(completed);
            }

            if (completedUpgrades.Count > 0)
            {
                SaveProgress();
            }
        }

        private void CompleteUpgrade(FacilityUpgradeProgress upgrade)
        {
            _facilityLevels[upgrade.FacilityId] = upgrade.TargetLevel;
            upgrade.IsCompleted = true;

            var facility = GetFacility(upgrade.FacilityId);
            if (facility != null)
            {
                Debug.Log($"Completed upgrade: {facility.DisplayName} -> Level {upgrade.TargetLevel}");
            }
        }

        private void ProcessGoldMineGeneration(DateTime now)
        {
            var goldMine = GetFacility("gold_mine");
            if (goldMine == null)
                return;

            int level = GetFacilityLevel("gold_mine");
            float goldPerHour = goldMine.GetBenefitAtLevel(level);

            // Calculate accumulated gold
            var timePassed = now - _lastGoldCollectionTime;
            float hoursPassed = (float)timePassed.TotalHours;

            // Gold is accumulated continuously (stored as pending gold in save)
            // Just update the timestamp
            _lastGoldCollectionTime = now;
            SaveProgress();
        }

        public int CollectGold()
        {
            var goldMine = GetFacility("gold_mine");
            if (goldMine == null)
                return 0;

            int level = GetFacilityLevel("gold_mine");
            float goldPerHour = goldMine.GetBenefitAtLevel(level);

            var now = DateTime.Now;
            var timePassed = now - _lastGoldCollectionTime;
            float hoursPassed = (float)timePassed.TotalHours;

            int goldToCollect = Mathf.FloorToInt(goldPerHour * hoursPassed);

            if (goldToCollect > 0)
            {
                _currencyService.AddGold(goldToCollect);
                _lastGoldCollectionTime = now;
                SaveProgress();

                Debug.Log($"Collected {goldToCollect} gold from Gold Mine");
            }

            return goldToCollect;
        }

        private void ProcessTrainingGroundGeneration(DateTime now)
        {
            var trainingGround = GetFacility("training_ground");
            if (trainingGround == null)
                return;

            int level = GetFacilityLevel("training_ground");
            float expPerHour = trainingGround.GetBenefitAtLevel(level);

            // Calculate accumulated EXP
            var timePassed = now - _lastExpCollectionTime;
            float hoursPassed = (float)timePassed.TotalHours;

            // Update timestamp
            _lastExpCollectionTime = now;
            SaveProgress();
        }

        public int CollectExp()
        {
            var trainingGround = GetFacility("training_ground");
            if (trainingGround == null)
                return 0;

            int level = GetFacilityLevel("training_ground");
            float expPerHour = trainingGround.GetBenefitAtLevel(level);

            var now = DateTime.Now;
            var timePassed = now - _lastExpCollectionTime;
            float hoursPassed = (float)timePassed.TotalHours;

            int expToCollect = Mathf.FloorToInt(expPerHour * hoursPassed);

            if (expToCollect > 0)
            {
                _lastExpCollectionTime = now;
                SaveProgress();

                Debug.Log($"Accumulated {expToCollect} EXP in Training Ground");
            }

            return expToCollect;
        }

        private void ProcessHospitalRecovery(DateTime now)
        {
            var hospital = GetFacility("hospital");
            if (hospital == null)
                return;

            int level = GetFacilityLevel("hospital");
            float recoveryBonusPercent = hospital.GetBenefitAtLevel(level);

            var timePassed = now - _lastHospitalProcessTime;
            float secondsPassed = (float)timePassed.TotalSeconds;

            if (secondsPassed >= 60f) // Only process every minute
            {
                // Recovery rate: base 1 morale per minute, multiplied by facility bonus
                float recoveryMultiplier = 1f + (recoveryBonusPercent / 100f);
                float recoveryPerMinute = 1f * recoveryMultiplier;
                float totalRecovery = recoveryPerMinute * (secondsPassed / 60f);

                _lastHospitalProcessTime = now;
                SaveProgress();

                Debug.Log($"Hospital generating {totalRecovery:F2} morale recovery per hero");
            }
        }

        public void ProcessHospitalRecovery()
        {
            ProcessHospitalRecovery(DateTime.Now);
        }

        public void RecoverHeroMorale(string heroInstanceId, int amount)
        {
            var hero = _rosterService.GetHero(heroInstanceId);
            if (hero == null)
                return;

            hero.CurrentMorale = Mathf.Min(100, hero.CurrentMorale + amount);
            _rosterService.UpdateHero(hero);

            Debug.Log($"Hospital recovered {amount} morale for {hero.HeroName}");
        }

        /// <summary>
        /// Gets the pending gold from gold mine (not yet collected).
        /// </summary>
        public int GetPendingGold()
        {
            var goldMine = GetFacility("gold_mine");
            if (goldMine == null)
                return 0;

            int level = GetFacilityLevel("gold_mine");
            float goldPerHour = goldMine.GetBenefitAtLevel(level);

            var now = DateTime.Now;
            var timePassed = now - _lastGoldCollectionTime;
            float hoursPassed = (float)timePassed.TotalHours;

            return Mathf.FloorToInt(goldPerHour * hoursPassed);
        }

        /// <summary>
        /// Gets the pending EXP from training ground (not yet collected).
        /// </summary>
        public int GetPendingExp()
        {
            var trainingGround = GetFacility("training_ground");
            if (trainingGround == null)
                return 0;

            int level = GetFacilityLevel("training_ground");
            float expPerHour = trainingGround.GetBenefitAtLevel(level);

            var now = DateTime.Now;
            var timePassed = now - _lastExpCollectionTime;
            float hoursPassed = (float)timePassed.TotalHours;

            return Mathf.FloorToInt(expPerHour * hoursPassed);
        }

        /// <summary>
        /// Gets upgrade progress for a facility.
        /// </summary>
        public FacilityUpgradeProgress GetUpgradeProgress(string facilityId)
        {
            return _upgradeQueue.Find(u => u.FacilityId == facilityId && !u.IsCompleted);
        }

        /// <summary>
        /// Checks if a facility is currently upgrading.
        /// </summary>
        public bool IsUpgrading(string facilityId)
        {
            return _upgradeQueue.Exists(u => u.FacilityId == facilityId && !u.IsCompleted);
        }

        public void ResetProgress()
        {
            // Reset all facility levels to starting values
            foreach (var facility in _facilities)
            {
                _facilityLevels[facility.Key] = facility.Value.StartingLevel;
            }

            _upgradeQueue.Clear();
            _lastGoldCollectionTime = DateTime.Now;
            _lastExpCollectionTime = DateTime.Now;
            _lastHospitalProcessTime = DateTime.Now;

            SaveProgress();
        }
    }
}