using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ---------------------------------------------------------------------------
//  CombatBridge
//  Attach to the same root GameObject as DungeonSpawnManager in the Dungeon scene.
//
//  Responsibility:
//    • Listens for DungeonService.OnWaveStarted
//    • After a short delay (to let DungeonSpawnManager finish spawning),
//      finds all active EnemyEntity objects under spawnRoot
//    • Reads the chosen formation from FormationService
//    • Resolves hero stats via HeroStatCalculator + HeroDefinition assets
//    • Feeds CombatHeroState + CombatEnemyState lists to CombatEngine.StartBattle
//    • On CombatEngine.OnBattleDefeat → calls DungeonService.FailDungeon
//    • Victory is handled automatically: EnemyEntity.OnDeath fires into
//      DungeonSpawnManager.HandleEnemyDeath → DungeonService.RegisterEnemyKill
//      → wave advance → OnDungeonCleared  (existing flow preserved)
// ---------------------------------------------------------------------------
public class CombatBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnRoot;

    [Header("Timing")]
    [Tooltip("Seconds to wait after OnWaveStarted before scanning for spawned enemies.")]
    [SerializeField] private float spawnScanDelay = 0.8f;

    // Services
    private IDungeonService   _dungeonService;
    private IFormationService _formationService;
    private ICombatEngine     _combatEngine;
    private IRosterService    _rosterService;

    // Hero definitions loaded once from Resources/Heroes
    private HeroDefinition[] _allDefinitions;

    // -----------------------------------------------------------------------
    //  Unity lifecycle
    // -----------------------------------------------------------------------
    private void Awake()
    {
        _allDefinitions = Resources.LoadAll<HeroDefinition>("Heroes");
    }

    private void OnEnable()
    {
        ResolveServices();
        if (_dungeonService != null)
            _dungeonService.OnWaveStarted += HandleWaveStarted;
        if (_combatEngine != null)
            _combatEngine.OnBattleDefeat += HandleBattleDefeat;
        if (_combatEngine != null)
            _combatEngine.OnBattleVictory += HandleBattleVictory;
    }

    private void OnDisable()
    {
        if (_dungeonService != null)
            _dungeonService.OnWaveStarted -= HandleWaveStarted;
        if (_combatEngine != null)
            _combatEngine.OnBattleDefeat -= HandleBattleDefeat;
        if (_combatEngine != null)
            _combatEngine.OnBattleVictory -= HandleBattleVictory;
    }

    // -----------------------------------------------------------------------
    //  Service resolution (safe – services may not exist yet at Awake)
    // -----------------------------------------------------------------------
    private void ResolveServices()
    {
        if (ServiceRegistry.Instance == null) return;

        if (_dungeonService == null && ServiceRegistry.Instance.HasService<IDungeonService>())
            _dungeonService = ServiceRegistry.Instance.Resolve<IDungeonService>();

        if (_formationService == null && ServiceRegistry.Instance.HasService<IFormationService>())
            _formationService = ServiceRegistry.Instance.Resolve<IFormationService>();

        if (_combatEngine == null && ServiceRegistry.Instance.HasService<ICombatEngine>())
            _combatEngine = ServiceRegistry.Instance.Resolve<ICombatEngine>();

        if (_rosterService == null && ServiceRegistry.Instance.HasService<IRosterService>())
            _rosterService = ServiceRegistry.Instance.Resolve<IRosterService>();
    }

    // -----------------------------------------------------------------------
    //  Wave started → wait for spawn → begin combat
    // -----------------------------------------------------------------------
    private void HandleWaveStarted(EnemyWaveSO waveData)
    {
        StopAllCoroutines();
        StartCoroutine(BeginCombatAfterSpawn());
    }

    private IEnumerator BeginCombatAfterSpawn()
    {
        // Let DungeonSpawnManager's SpawnWaveRoutine finish placing enemies
        yield return new WaitForSeconds(spawnScanDelay);

        ResolveServices();

        if (_combatEngine == null)
        {
            Debug.LogWarning("[CombatBridge] No ICombatEngine found – combat will not run.");
            yield break;
        }

        List<CombatHeroState>  heroes  = BuildHeroStates();
        List<CombatEnemyState> enemies = BuildEnemyStates();

        if (heroes.Count == 0)
        {
            Debug.LogWarning("[CombatBridge] No heroes in formation – combat skipped.");
            yield break;
        }

        if (enemies.Count == 0)
        {
            Debug.LogWarning("[CombatBridge] No enemies found in scene – combat skipped.");
            yield break;
        }

        _combatEngine.StartBattle(heroes, enemies);
    }

    // -----------------------------------------------------------------------
    //  Build hero states from FormationService + HeroDefinition assets
    // -----------------------------------------------------------------------
    private List<CombatHeroState> BuildHeroStates()
    {
        List<CombatHeroState> result = new List<CombatHeroState>();

        if (_formationService == null || _rosterService == null)
        {
            Debug.LogWarning("[CombatBridge] FormationService or RosterService not found.");
            return result;
        }

        List<HeroInstance> allHeroes = _rosterService.GetAll();

        for (int s = 0; s < _formationService.Slots.Count; s++)
        {
            FormationSlot slot = _formationService.Slots[s];
            if (!slot.IsOccupied) continue;

            HeroInstance instance = allHeroes.Find(h => h.InstanceId == slot.HeroInstanceId);
            if (instance == null || !instance.IsAlive) continue;

            HeroDefinition def = FindDefinition(instance.HeroDefId);
            if (def == null) continue;

            IInventoryService inventory = ServiceRegistry.Instance?.Resolve<IInventoryService>();
            HeroResolvedStats stats = HeroStatCalculator.ResolveWithEquipment(def, instance, inventory);

            result.Add(new CombatHeroState
            {
                InstanceId = instance.InstanceId,
                HeroName   = def.HeroName,
                Class      = def.BaseClass,
                MaxHP      = stats.MaxHP,
                CurrentHP  = stats.MaxHP,
                ATK        = stats.ATK,
                DEF        = stats.DEF,
                SPD        = stats.SPD,
                IsFrontRow = slot.IsFrontRow
            });
        }

        return result;
    }

    // -----------------------------------------------------------------------
    //  Collect all EnemyEntity objects that were just spawned under spawnRoot
    // -----------------------------------------------------------------------
    private List<CombatEnemyState> BuildEnemyStates()
    {
        List<CombatEnemyState> result = new List<CombatEnemyState>();

        // Search scope: under spawnRoot if assigned, else entire scene
        EnemyEntity[] entities = spawnRoot != null
            ? spawnRoot.GetComponentsInChildren<EnemyEntity>(includeInactive: false)
            : FindObjectsByType<EnemyEntity>(FindObjectsSortMode.None);

        for (int i = 0; i < entities.Length; i++)
        {
            EnemyEntity e = entities[i];
            if (e == null || e.IsDead || e.Data == null) continue;

            result.Add(new CombatEnemyState
            {
                Entity    = e,
                EnemyId   = e.Data.Id,
                EnemyName = e.Data.DisplayName,
                MaxHP     = e.Data.BaseHp,
                CurrentHP = e.Data.BaseHp,
                ATK       = e.Data.BaseAttack,
                DEF       = e.Data.BaseDefense,
                SPD       = e.Data.BaseSPD
            });
        }

        return result;
    }

    // -----------------------------------------------------------------------
    //  Events
    // -----------------------------------------------------------------------
    private void HandleBattleDefeat()
    {
        if (_dungeonService != null && _dungeonService.IsInDungeon)
        {
            _dungeonService.FailDungeon();
        }
    }

    private void HandleBattleVictory()
    {
        Debug.Log("[CombatBridge] Battle victory — rewards handled by DungeonService.");
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------
    private HeroDefinition FindDefinition(string heroDefId)
    {
        if (_allDefinitions == null) return null;
        for (int i = 0; i < _allDefinitions.Length; i++)
        {
            if (_allDefinitions[i] != null && _allDefinitions[i].HeroId == heroDefId)
                return _allDefinitions[i];
        }
        return null;
    }
}
