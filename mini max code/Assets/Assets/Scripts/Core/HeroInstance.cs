using System;
using UnityEngine;

namespace PickMeUp.Game.Core
{
    /// <summary>
    /// Serializable representation of a hero instance in the player's roster.
    /// </summary>
    [Serializable]
    public class HeroInstance
    {
        [Header("Identity")]
        public string InstanceId;
        public string HeroId; // Reference to HeroDefinition
        public string HeroName;

        [Header("Stats")]
        public int Level = 1;
        public int BaseAttack;
        public int BaseDefense;
        public int BaseHealth;
        public int CurrentHealth;
        public int MaxHealth;
        public int Attack;
        public int Defense;
        public int Speed;
        public float CritRate;
        public float CritDamage;

        [Header("Progression")]
        public int CurrentExp;
        public int RequiredExp = 100;
        public int StarRank = 1;

        [Header("Morale")]
        public int CurrentMorale = 100;
        public const int MaxMorale = 100;

        [Header("Equipment")]
        public string WeaponId;
        public string ArmorId;
        public string AccessoryId;

        [Header("State")]
        public bool IsLocked;
        public DateTime AcquiredTime;
        public DateTime LastUsedTime;

        public HeroInstance()
        {
            InstanceId = Guid.NewGuid().ToString();
            AcquiredTime = DateTime.Now;
            ResetStats();
        }

        public HeroInstance(HeroDefinition definition) : this()
        {
            HeroId = definition.HeroId;
            HeroName = definition.HeroName;
            BaseAttack = definition.BaseAttack;
            BaseDefense = definition.BaseDefense;
            BaseHealth = definition.BaseHealth;
            Speed = definition.BaseSpeed;
            CritRate = definition.BaseCritRate;
            CritDamage = definition.BaseCritDamage;
            StarRank = definition.StarRank;

            ResetStats();
        }

        /// <summary>
        /// Resets calculated stats to base values.
        /// </summary>
        public void ResetStats()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Applies level-up bonuses to stats.
        /// </summary>
        public void ApplyLevelBonuses()
        {
            MaxHealth = BaseHealth + (Level - 1) * 20;
            Attack = BaseAttack + (Level - 1) * 5;
            Defense = BaseDefense + (Level - 1) * 3;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }

        /// <summary>
        /// Checks if the hero can be used (has health and morale).
        /// </summary>
        public bool CanBattle()
        {
            return CurrentHealth > 0 && CurrentMorale >= 20;
        }

        /// <summary>
        /// Gets the hero's current power level.
        /// </summary>
        public int GetPowerLevel()
        {
            return Level * 10 + Attack + Defense + MaxHealth / 10 + (StarRank * 20);
        }

        /// <summary>
        /// Adds experience and handles level up.
        /// </summary>
        public bool AddExperience(int amount)
        {
            CurrentExp += amount;

            while (CurrentExp >= RequiredExp)
            {
                CurrentExp -= RequiredExp;
                Level++;
                RequiredExp = CalculateRequiredExp(Level);
                ApplyLevelBonuses();
            }

            return Level > 1;
        }

        /// <summary>
        /// Calculates required EXP for a given level.
        /// </summary>
        public static int CalculateRequiredExp(int level)
        {
            return 100 + (level * 50);
        }

        /// <summary>
        /// Takes damage and returns remaining health.
        /// </summary>
        public int TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - Defense / 2);
            CurrentHealth = Mathf.Max(0, CurrentHealth - actualDamage);
            return actualDamage;
        }

        /// <summary>
        /// Heals the hero by amount.
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        /// <summary>
        /// Modifies morale by amount (can be negative).
        /// </summary>
        public void ModifyMorale(int amount)
        {
            CurrentMorale = Mathf.Clamp(CurrentMorale + amount, 0, MaxMorale);
        }

        /// <summary>
        /// Checks if hero needs morale recovery.
        /// </summary>
        public bool NeedsMoraleRecovery()
        {
            return CurrentMorale < 50;
        }

        /// <summary>
        /// Gets a formatted status string.
        /// </summary>
        public string GetStatusString()
        {
            return $"{HeroName} Lv.{Level} ★{StarRank} HP:{CurrentHealth}/{MaxHealth} Morale:{CurrentMorale}";
        }

        /// <summary>
        /// Creates a copy of this hero instance.
        /// </summary>
        public HeroInstance Clone()
        {
            return new HeroInstance
            {
                InstanceId = Guid.NewGuid().ToString(),
                HeroId = HeroId,
                HeroName = HeroName,
                Level = Level,
                BaseAttack = BaseAttack,
                BaseDefense = BaseDefense,
                BaseHealth = BaseHealth,
                CurrentHealth = CurrentHealth,
                MaxHealth = MaxHealth,
                Attack = Attack,
                Defense = Defense,
                Speed = Speed,
                CritRate = CritRate,
                CritDamage = CritDamage,
                CurrentExp = CurrentExp,
                RequiredExp = RequiredExp,
                StarRank = StarRank,
                CurrentMorale = CurrentMorale,
                WeaponId = WeaponId,
                ArmorId = ArmorId,
                AccessoryId = AccessoryId,
                IsLocked = false,
                AcquiredTime = DateTime.Now,
                LastUsedTime = DateTime.Now
            };
        }
    }
}