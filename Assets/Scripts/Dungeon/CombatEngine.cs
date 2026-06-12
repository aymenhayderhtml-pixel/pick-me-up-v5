using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEngine : MonoBehaviour, ICombatEngine
{
    public event Action OnBattleStart;
    public event Action<CombatHeroState> OnHeroTurn;
    public event Action<CombatEnemyState> OnEnemyTurn;
    public event Action OnTurnEnd;
    public event Action OnBattleVictory;
    public event Action OnBattleDefeat;
    public event Action<CombatHeroState, int> OnHeroHit;
    public event Action<CombatEnemyState, int> OnEnemyHit;
    public event Action<CombatHeroState> OnHeroDied;
    public event Action<CombatEnemyState> OnEnemyDied;

    [Header("Timing (seconds between turns at 1x)")]
    [SerializeField] private float turnInterval = 1.2f;
    [SerializeField] private float attackAnimDelay = 0.4f;

    public bool IsRunning { get; private set; }
    public float SpeedScale { get; private set; } = 1f;

    private List<CombatHeroState>  _heroes  = new List<CombatHeroState>();
    private List<CombatEnemyState> _enemies = new List<CombatEnemyState>();

    public IReadOnlyList<CombatHeroState>  Heroes  => _heroes;
    public IReadOnlyList<CombatEnemyState> Enemies => _enemies;

    private Coroutine _battleRoutine;

    public void StartBattle(IReadOnlyList<CombatHeroState> heroes,
                            IReadOnlyList<CombatEnemyState> enemies)
    {
        if (IsRunning) StopBattle();

        _heroes.Clear();
        _enemies.Clear();

        if (heroes != null)
        {
            for (int i = 0; i < heroes.Count; i++)
                _heroes.Add(heroes[i]);
        }

        if (enemies != null)
        {
            for (int i = 0; i < enemies.Count; i++)
                _enemies.Add(enemies[i]);
        }

        if (_heroes.Count == 0 || _enemies.Count == 0)
        {
            Debug.LogWarning("[CombatEngine] StartBattle called with empty heroes or enemies list.");
            return;
        }

        IsRunning = true;
        OnBattleStart?.Invoke();
        _battleRoutine = StartCoroutine(BattleLoop());
    }

    public void SetSpeedScale(float scale)
    {
        SpeedScale = Mathf.Clamp(scale, 0.1f, 10f);
    }

    public void StopBattle()
    {
        if (_battleRoutine != null)
        {
            StopCoroutine(_battleRoutine);
            _battleRoutine = null;
        }
        IsRunning = false;
    }

    private IEnumerator BattleLoop()
    {
        yield return new WaitForSeconds(0.5f / Mathf.Max(0.1f, SpeedScale));

        while (IsRunning)
        {
            List<object> turnQueue = BuildTurnQueue();

            for (int t = 0; t < turnQueue.Count; t++)
            {
                if (!IsRunning) yield break;

                object actor = turnQueue[t];

                if (actor is CombatHeroState hero)
                {
                    if (hero.IsDead) continue;
                    yield return StartCoroutine(HeroTurn(hero));
                }
                else if (actor is CombatEnemyState enemy)
                {
                    if (enemy.IsDead) continue;
                    yield return StartCoroutine(EnemyTurn(enemy));
                }

                OnTurnEnd?.Invoke();

                if (CheckVictory())  yield break;
                if (CheckDefeat())   yield break;

                float wait = turnInterval / Mathf.Max(0.1f, SpeedScale);
                yield return new WaitForSeconds(wait);
            }
        }
    }

    private IEnumerator HeroTurn(CombatHeroState hero)
    {
        OnHeroTurn?.Invoke(hero);

        CombatEnemyState target = null;
        if (hero.Class == HeroClass.Warrior)
            target = GetLowestHpEnemy();
        else if (hero.Class == HeroClass.Ranger)
            target = GetBackRowEnemy();
        else
            target = GetRandomLivingEnemy();
        if (target == null) yield break;

        yield return new WaitForSeconds(attackAnimDelay / Mathf.Max(0.1f, SpeedScale));

        int dmg = Mathf.Max(1, hero.ATK - Mathf.RoundToInt(target.DEF * 0.5f));
        int actualDmg = target.TakeDamage(dmg);

        OnEnemyHit?.Invoke(target, actualDmg);

        if (target.IsDead)
        {
            OnEnemyDied?.Invoke(target);
            if (target.Entity != null && !target.Entity.IsDead)
                target.Entity.ForceKill();
        }
    }

    private IEnumerator EnemyTurn(CombatEnemyState enemy)
    {
        OnEnemyTurn?.Invoke(enemy);

        CombatHeroState target = GetHeroTargetForEnemy(enemy);
        if (target == null) yield break;

        yield return new WaitForSeconds(attackAnimDelay / Mathf.Max(0.1f, SpeedScale));

        int dmg = Mathf.Max(1, enemy.ATK - Mathf.RoundToInt(target.DEF * 0.5f));
        target.TakeDamage(dmg);

        OnHeroHit?.Invoke(target, dmg);

        if (target.IsDead)
            OnHeroDied?.Invoke(target);
    }

    private List<object> BuildTurnQueue()
    {
        List<object> queue = new List<object>();

        for (int i = 0; i < _heroes.Count; i++)
        {
            if (!_heroes[i].IsDead) queue.Add(_heroes[i]);
        }
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (!_enemies[i].IsDead) queue.Add(_enemies[i]);
        }

        queue.Sort((a, b) =>
        {
            int spdA = GetSPD(a);
            int spdB = GetSPD(b);
            if (spdB != spdA) return spdB.CompareTo(spdA);
            bool aIsHero = a is CombatHeroState;
            bool bIsHero = b is CombatHeroState;
            if (aIsHero && !bIsHero) return -1;
            if (!aIsHero && bIsHero) return 1;
            return 0;
        });

        return queue;
    }

    private int GetSPD(object actor)
    {
        if (actor is CombatHeroState  h) return h.SPD;
        if (actor is CombatEnemyState e) return e.SPD;
        return 0;
    }

    private CombatEnemyState GetRandomLivingEnemy()
    {
        List<CombatEnemyState> alive = new List<CombatEnemyState>();
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (!_enemies[i].IsDead) alive.Add(_enemies[i]);
        }
        if (alive.Count == 0) return null;
        return alive[UnityEngine.Random.Range(0, alive.Count)];
    }

    private CombatEnemyState GetLowestHpEnemy()
    {
        CombatEnemyState lowest = null;
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].IsDead) continue;
            if (lowest == null || _enemies[i].CurrentHP < lowest.CurrentHP)
                lowest = _enemies[i];
        }
        return lowest;
    }

    private CombatEnemyState GetBackRowEnemy()
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            if (!_enemies[i].IsDead)
                return _enemies[i];
        }
        return null;
    }

    private CombatHeroState GetHeroTargetForEnemy(CombatEnemyState enemy)
    {
        if (enemy == null || enemy.Entity == null || enemy.Entity.Data == null) return GetRandomLivingHero();

        switch (enemy.Entity.Data.Behavior)
        {
            case EnemyBehavior.FocusWeakest:
                return GetLowestHpHero() ?? GetRandomLivingHero();
            case EnemyBehavior.FocusBackRow:
                return GetBackRowHero() ?? GetRandomLivingHero();
            case EnemyBehavior.FocusHealer:
                CombatHeroState healer = GetHealerHero();
                if (healer != null) return healer;
                return GetRandomLivingHero();
            default:
                return GetRandomLivingHeroWithTaunt();
        }
    }

    private CombatHeroState GetBackRowHero()
    {
        List<CombatHeroState> backRow = new List<CombatHeroState>();
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (!_heroes[i].IsDead && !_heroes[i].IsFrontRow)
                backRow.Add(_heroes[i]);
        }
        if (backRow.Count > 0)
            return backRow[UnityEngine.Random.Range(0, backRow.Count)];
        return null;
    }

    private CombatHeroState GetHealerHero()
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (!_heroes[i].IsDead && _heroes[i].Class == HeroClass.Healer)
                return _heroes[i];
        }
        return null;
    }

    private CombatHeroState GetRandomLivingHeroWithTaunt()
    {
        List<CombatHeroState> tanks = new List<CombatHeroState>();
        List<CombatHeroState> others = new List<CombatHeroState>();

        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i].IsDead) continue;
            if (_heroes[i].Class == HeroClass.Tank)
                tanks.Add(_heroes[i]);
            else
                others.Add(_heroes[i]);
        }

        if (tanks.Count > 0 && UnityEngine.Random.value < 0.7f)
            return tanks[UnityEngine.Random.Range(0, tanks.Count)];

        if (others.Count > 0)
            return others[UnityEngine.Random.Range(0, others.Count)];

        if (tanks.Count > 0)
            return tanks[UnityEngine.Random.Range(0, tanks.Count)];

        return null;
    }

    private CombatHeroState GetRandomLivingHero()
    {
        List<CombatHeroState> front = new List<CombatHeroState>();
        List<CombatHeroState> back  = new List<CombatHeroState>();

        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i].IsDead) continue;
            if (_heroes[i].IsFrontRow) front.Add(_heroes[i]);
            else                       back.Add(_heroes[i]);
        }

        if (front.Count > 0)
            return front[UnityEngine.Random.Range(0, front.Count)];
        if (back.Count > 0)
            return back[UnityEngine.Random.Range(0, back.Count)];
        return null;
    }

    private CombatHeroState GetLowestHpHero()
    {
        CombatHeroState lowest = null;
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i].IsDead) continue;
            if (lowest == null || _heroes[i].CurrentHP < lowest.CurrentHP)
                lowest = _heroes[i];
        }
        return lowest;
    }

    private bool CheckVictory()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (!_enemies[i].IsDead) return false;
        }
        IsRunning = false;
        OnBattleVictory?.Invoke();
        return true;
    }

    private bool CheckDefeat()
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (!_heroes[i].IsDead) return false;
        }
        IsRunning = false;
        OnBattleDefeat?.Invoke();
        return true;
    }
}
