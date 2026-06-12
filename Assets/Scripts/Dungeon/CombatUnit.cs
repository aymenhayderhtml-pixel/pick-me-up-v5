using System;
using UnityEngine;

public enum CombatUnitType
{
    Hero,
    Enemy
}

[Serializable]
public class CombatUnit
{
    public CombatUnitType UnitType;
    public string UnitId;
    public string DisplayName;
    public int CurrentHp;
    public int MaxHp;
    public int CurrentAtk;
    public int CurrentDef;
    public int CurrentSpd;
    public int Level;
    public int Star;
    public Sprite Portrait;

    public HeroInstance HeroInstance;
    public EnemyEntity EnemyEntity;
    public Transform VisualTransform;

    public bool IsDead => CurrentHp <= 0;
    public float HpPercent => MaxHp > 0 ? (float)CurrentHp / MaxHp : 0f;

    public static CombatUnit FromHero(HeroInstance hero, HeroDefinition definition)
    {
        if (hero == null || definition == null) return null;

        CombatUnit unit = new CombatUnit
        {
            UnitType = CombatUnitType.Hero,
            UnitId = definition.HeroId,
            DisplayName = definition.HeroName,
            Level = hero.Level,
            Star = hero.CurrentStarRank,
            Portrait = definition.Portrait,
            HeroInstance = hero
        };

        unit.MaxHp = definition.GetMaxHp(hero.Level, hero.CurrentStarRank);
        unit.CurrentHp = unit.MaxHp;
        unit.CurrentAtk = definition.GetAtk(hero.Level, hero.CurrentStarRank);
        unit.CurrentDef = definition.GetDef(hero.Level, hero.CurrentStarRank);
        unit.CurrentSpd = definition.GetSpd(hero.Level, hero.CurrentStarRank);

        return unit;
    }

    public static CombatUnit FromEnemy(EnemyEntity enemyEntity)
    {
        if (enemyEntity == null || enemyEntity.Data == null) return null;

        EnemyDataSO data = enemyEntity.Data;

        CombatUnit unit = new CombatUnit
        {
            UnitType = CombatUnitType.Enemy,
            UnitId = data.Id,
            DisplayName = data.DisplayName,
            Level = 1,
            Star = 1,
            Portrait = data.Portrait,
            EnemyEntity = enemyEntity,
            VisualTransform = enemyEntity.transform
        };

        unit.MaxHp = data.BaseHp;
        unit.CurrentHp = enemyEntity.IsDead ? 0 : data.BaseHp;
        unit.CurrentAtk = data.BaseAttack;
        unit.CurrentDef = data.BaseDefense;
        unit.CurrentSpd = data.BaseSPD;

        return unit;
    }

    public void TakeDamage(int damage)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        EnemyEntity?.TakeDamage(damage);
    }

    public void TriggerAttackAnimation()
    {
        if (EnemyEntity != null && !EnemyEntity.IsDead)
        {
            EnemyEntity.PlayAttackAnim();
        }
    }
}
