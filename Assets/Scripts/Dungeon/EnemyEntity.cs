using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyEntity : MonoBehaviour
{
    public event Action<EnemyEntity> OnDeath;

    private EnemyDataSO data;
    private int currentHp;

    public EnemyDataSO Data => data;
    public bool IsDead { get; private set; }

    public void Initialize(EnemyDataSO enemyData)
    {
        data = enemyData;
        currentHp = enemyData != null ? enemyData.BaseHp : 0;
        IsDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || data == null)
        {
            return;
        }

        int finalDamage = Mathf.Max(1, amount - data.BaseDefense);
        currentHp -= finalDamage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        OnDeath?.Invoke(this);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 1f);
    }
}
