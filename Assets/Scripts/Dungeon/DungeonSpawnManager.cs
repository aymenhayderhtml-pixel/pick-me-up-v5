using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSpawnManager : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private Transform spawnRoot;
    [SerializeField] private Vector2 spawnBounds = new Vector2(5f, 5f);

    [Header("Prefab Resolution")]
    [SerializeField] private List<EnemyPrefabMapping> prefabMappings = new List<EnemyPrefabMapping>();

    [System.Serializable]
    public class EnemyPrefabMapping
    {
        public EnemyDataSO Data;
        public GameObject Prefab;
    }

    private IDungeonService dungeonService;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IDungeonService>())
        {
            dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
        }
    }

    private void OnEnable()
    {
        if (dungeonService == null && ServiceRegistry.Instance != null && ServiceRegistry.Instance.HasService<IDungeonService>())
        {
            dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();
        }

        if (dungeonService == null)
        {
            return;
        }

        dungeonService.OnDungeonStarted += HandleDungeonStarted;
        dungeonService.OnWaveStarted += HandleWaveStarted;
        dungeonService.OnWaveCleared += HandleWaveCleared;
        dungeonService.OnDungeonFailed += HandleDungeonEnded;
        dungeonService.OnDungeonCleared += HandleDungeonEnded;
    }

    private void OnDisable()
    {
        if (dungeonService == null)
        {
            return;
        }

        dungeonService.OnDungeonStarted -= HandleDungeonStarted;
        dungeonService.OnWaveStarted -= HandleWaveStarted;
        dungeonService.OnWaveCleared -= HandleWaveCleared;
        dungeonService.OnDungeonFailed -= HandleDungeonEnded;
        dungeonService.OnDungeonCleared -= HandleDungeonEnded;
    }

    private void HandleDungeonStarted(DungeonDataSO data)
    {
        ClearAllEnemies();
    }

    private void HandleWaveStarted(EnemyWaveSO waveData)
    {
        StopAllCoroutines();
        StartCoroutine(SpawnWaveRoutine(waveData));
    }

    private IEnumerator SpawnWaveRoutine(EnemyWaveSO waveData)
    {
        if (waveData == null || waveData.Spawns == null)
        {
            yield break;
        }

        List<EnemyWaveSO.SpawnEntry> sortedEntries = new List<EnemyWaveSO.SpawnEntry>(waveData.Spawns);
        sortedEntries.Sort((a, b) => a.SpawnDelay.CompareTo(b.SpawnDelay));

        float elapsed = 0f;
        foreach (EnemyWaveSO.SpawnEntry entry in sortedEntries)
        {
            if (entry == null || !entry.IsValid)
            {
                continue;
            }

            float wait = entry.SpawnDelay - elapsed;
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
                elapsed += wait;
            }

            SpawnEnemyGroup(entry);
        }
    }

    private void SpawnEnemyGroup(EnemyWaveSO.SpawnEntry entry)
    {
        GameObject prefab = GetPrefab(entry.EnemyData);
        if (prefab == null)
        {
            Debug.LogWarning("[DungeonSpawnManager] No prefab mapped for " + entry.EnemyData.DisplayName + ".");
            return;
        }

        for (int i = 0; i < entry.Quantity; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemyObject = Instantiate(prefab, spawnPos, Quaternion.identity, spawnRoot);

            EnemyEntity entity = enemyObject.GetComponent<EnemyEntity>();
            if (entity != null)
            {
                entity.Initialize(entry.EnemyData);
                entity.OnDeath += HandleEnemyDeath;
            }

            activeEnemies.Add(enemyObject);
        }
    }

    private void HandleWaveCleared()
    {
    }

    private void HandleEnemyDeath(EnemyEntity deadEnemy)
    {
        if (deadEnemy == null)
        {
            return;
        }

        deadEnemy.OnDeath -= HandleEnemyDeath;
        activeEnemies.Remove(deadEnemy.gameObject);

        if (dungeonService != null)
        {
            dungeonService.RegisterEnemyKill();
        }
    }

    private void HandleDungeonEnded()
    {
        StopAllCoroutines();
        ClearAllEnemies();
    }

    private void ClearAllEnemies()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            GameObject enemy = activeEnemies[i];
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        activeEnemies.Clear();
    }

    private GameObject GetPrefab(EnemyDataSO data)
    {
        for (int i = 0; i < prefabMappings.Count; i++)
        {
            EnemyPrefabMapping mapping = prefabMappings[i];
            if (mapping != null && mapping.Data == data)
            {
                return mapping.Prefab;
            }
        }

        return null;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnRoot == null)
        {
            return Vector3.zero;
        }

        float x = Random.Range(-spawnBounds.x, spawnBounds.x);
        float y = Random.Range(-spawnBounds.y, spawnBounds.y);
        return spawnRoot.TransformPoint(new Vector3(x, y, 0f));
    }
}
