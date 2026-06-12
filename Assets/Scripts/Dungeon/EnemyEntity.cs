using System;
using System.Collections;
using UnityEngine;

// ---------------------------------------------------------------------------
//  EnemyEntity  — scene-side representation of one enemy.
//
//  State machine (coroutine-based, no DOTween):
//    Idle → Attacking → Dead
//
//  CombatEngine drives damage via ForceKill / the CombatEnemyState.
//  EnemyEntity owns its own visual fade and collider disable on death.
// ---------------------------------------------------------------------------
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyEntity : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Events
    // -----------------------------------------------------------------------
    public event Action<EnemyEntity> OnDeath;

    // -----------------------------------------------------------------------
    //  State
    // -----------------------------------------------------------------------
    public enum State { Idle, Attacking, Dead }

    public EnemyDataSO Data       { get; private set; }
    public State        CurrentState { get; private set; } = State.Idle;
    public bool         IsDead    => CurrentState == State.Dead;

    public int  MaxHP    { get; private set; }
    public int  CurrentHP { get; private set; }

    // -----------------------------------------------------------------------
    //  Inspector
    // -----------------------------------------------------------------------
    [Header("Visual Feedback")]
    [SerializeField] private float deathFadeDuration = 0.6f;

    private SpriteRenderer _sprite;
    private Coroutine      _stateRoutine;

    // -----------------------------------------------------------------------
    //  Initialization
    // -----------------------------------------------------------------------
    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    /// <summary>Called by DungeonSpawnManager (or CombatBridge) after Instantiate.</summary>
    public void Initialize(EnemyDataSO enemyData)
    {
        Data         = enemyData;
        MaxHP        = enemyData != null ? enemyData.BaseHp : 1;
        CurrentHP    = MaxHP;
        CurrentState = State.Idle;

        if (_stateRoutine != null) StopCoroutine(_stateRoutine);
        _stateRoutine = StartCoroutine(IdleRoutine());
    }

    // -----------------------------------------------------------------------
    //  Public damage API  (used by legacy non-combat code paths)
    // -----------------------------------------------------------------------
    public void TakeDamage(int amount)
    {
        if (IsDead || Data == null) return;

        int dmg = Mathf.Max(1, amount - Data.BaseDefense);
        CurrentHP -= dmg;

        if (CurrentHP <= 0) TriggerDeath();
    }

    /// <summary>
    /// Called by CombatEngine after it has already applied damage to
    /// CombatEnemyState.  Entity just needs to run its death visuals.
    /// </summary>
    public void ForceKill()
    {
        if (IsDead) return;
        TriggerDeath();
    }

    // -----------------------------------------------------------------------
    //  State machine coroutines
    // -----------------------------------------------------------------------
    private IEnumerator IdleRoutine()
    {
        CurrentState = State.Idle;
        // Idle: just wait; CombatEngine decides when attacks happen
        yield return null;
    }

    private IEnumerator AttackRoutine()
    {
        CurrentState = State.Attacking;
        // Brief visual "lunge" using local position
        Vector3 origin = transform.localPosition;
        Vector3 lunge  = origin + new Vector3(-0.15f, 0f, 0f);

        float t = 0f;
        float duration = 0.15f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(origin, lunge, t / duration);
            yield return null;
        }

        // Return
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(lunge, origin, t / duration);
            yield return null;
        }

        transform.localPosition = origin;
        CurrentState = State.Idle;
    }

    private void TriggerDeath()
    {
        if (IsDead) return;
        CurrentState = State.Dead;

        if (_stateRoutine != null) StopCoroutine(_stateRoutine);
        _stateRoutine = StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Disable collider immediately so nothing targets it
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        OnDeath?.Invoke(this);

        // Fade out sprite
        if (_sprite != null)
        {
            float elapsed = 0f;
            Color startColor = _sprite.color;
            while (elapsed < deathFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / deathFadeDuration);
                _sprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    //  Utility — play the attack animation from outside (e.g. CombatBridge)
    // -----------------------------------------------------------------------
    public void PlayAttackAnim()
    {
        if (IsDead) return;
        if (_stateRoutine != null) StopCoroutine(_stateRoutine);
        _stateRoutine = StartCoroutine(AttackRoutine());
    }

    public void StartAttackRoutine(Transform target, System.Action onComplete)
    {
        if (IsDead)
        {
            onComplete?.Invoke();
            return;
        }
        PlayAttackAnim();
        StartCoroutine(AttackCompleteDelayed(onComplete));
    }

    private IEnumerator AttackCompleteDelayed(System.Action onComplete)
    {
        yield return new WaitForSeconds(0.35f);
        onComplete?.Invoke();
    }
}
