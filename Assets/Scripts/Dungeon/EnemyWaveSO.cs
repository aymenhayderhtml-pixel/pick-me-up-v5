using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyWave", menuName = "PickMeUp/Dungeon/Enemy Wave")]
public class EnemyWaveSO : ScriptableObject
{
    [System.Serializable]
    public class SpawnEntry
    {
        [SerializeField] private EnemyDataSO enemyData;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField, Min(0f)] private float spawnDelay = 0f;
        [SerializeField, Min(0)] private int weight = 1;

        public EnemyDataSO EnemyData => enemyData;
        public int Quantity => quantity;
        public float SpawnDelay => spawnDelay;
        public int Weight => weight;
        public bool IsValid => enemyData != null && quantity > 0;
    }

    [Header("Wave Composition")]
    [SerializeField] private List<SpawnEntry> spawns = new List<SpawnEntry>();

    public IReadOnlyList<SpawnEntry> Spawns => spawns;

    public int TotalEnemyCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < spawns.Count; i++)
            {
                SpawnEntry entry = spawns[i];
                if (entry != null && entry.IsValid)
                {
                    count += entry.Quantity;
                }
            }

            return count;
        }
    }
}
