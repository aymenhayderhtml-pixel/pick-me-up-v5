using System;
using System.Collections.Generic;

// ---------------------------------------------------------------------------
//  ICombatEngine — registered in ServiceRegistry as ICombatEngine
// ---------------------------------------------------------------------------
public interface ICombatEngine
{
    // Events
    event Action OnBattleStart;
    event Action<CombatHeroState> OnHeroTurn;
    event Action<CombatEnemyState> OnEnemyTurn;
    event Action OnTurnEnd;
    event Action OnBattleVictory;
    event Action OnBattleDefeat;
    event Action<CombatHeroState, int> OnHeroHit;      // hero, damage
    event Action<CombatEnemyState, int> OnEnemyHit;    // enemy, damage
    event Action<CombatHeroState> OnHeroDied;
    event Action<CombatEnemyState> OnEnemyDied;

    // State accessors
    bool IsRunning { get; }
    float SpeedScale { get; }
    IReadOnlyList<CombatHeroState> Heroes { get; }
    IReadOnlyList<CombatEnemyState> Enemies { get; }

    // Control
    void StartBattle(IReadOnlyList<CombatHeroState> heroes,
                     IReadOnlyList<CombatEnemyState> enemies);
    void SetSpeedScale(float scale);   // 1x / 2x / 3x
    void StopBattle();
}

// ---------------------------------------------------------------------------
//  Combat state bags — live data during a fight
// ---------------------------------------------------------------------------
[System.Serializable]
public class CombatHeroState
{
    public string InstanceId;
    public string HeroName;
    public HeroClass Class;
    public int MaxHP;
    public int CurrentHP;
    public int ATK;
    public int DEF;
    public int SPD;
    public bool IsDead => CurrentHP <= 0;

    // Formation slot
    public bool IsFrontRow;

    public void TakeDamage(int raw)
    {
        int dmg = UnityEngine.Mathf.Max(1, raw - (int)(DEF * 0.5f));
        CurrentHP = UnityEngine.Mathf.Max(0, CurrentHP - dmg);
    }
}

[System.Serializable]
public class CombatEnemyState
{
    public EnemyEntity Entity;           // live scene reference
    public string EnemyId;
    public string EnemyName;
    public int MaxHP;
    public int CurrentHP;
    public int ATK;
    public int DEF;
    public int SPD;
    public bool IsDead => CurrentHP <= 0;

    public int TakeDamage(int raw)
    {
        int dmg = UnityEngine.Mathf.Max(1, raw - (int)(DEF * 0.5f));
        CurrentHP = UnityEngine.Mathf.Max(0, CurrentHP - dmg);
        return dmg;
    }
}
