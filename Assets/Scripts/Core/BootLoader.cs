using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        // 1. Ensure ServiceRegistry is available (fallback if not placed in scene manually)
        if (ServiceRegistry.Instance == null)
        {
            GameObject registryObj = new GameObject("ServiceRegistry");
            registryObj.AddComponent<ServiceRegistry>();
        }


        // 2. Register SaveLoadService into ServiceRegistry
        ISaveLoadService saveLoadService = new SaveLoadService();
        ServiceRegistry.Instance.Register<ISaveLoadService>(saveLoadService);

        // 3. Call Load() to initialize save data / read from disk
        GameSaveData initialData = saveLoadService.Load();

        // 4. Register GameStateService to hold the live data in memory
        GameStateService gameState = new GameStateService(initialData);
        ServiceRegistry.Instance.Register<GameStateService>(gameState);

        // 5. Register CurrencyService
        ICurrencyService currencyService = new CurrencyService();
        ServiceRegistry.Instance.Register<ICurrencyService>(currencyService);

        // 6. Register GachaService
        IGachaService gachaService = new GachaService();
        ServiceRegistry.Instance.Register<IGachaService>(gachaService);

        // 7. Register RosterService
        IRosterService rosterService = new RosterService();
        ServiceRegistry.Instance.Register<IRosterService>(rosterService);

        // 8. Register TowerService
        ITowerService towerService = new TowerService();
        ServiceRegistry.Instance.Register<ITowerService>(towerService);
        towerService.LoadProgress();

        // 9. Register InventoryService
        IInventoryService inventoryService = new InventoryService();
        ServiceRegistry.Instance.Register<IInventoryService>(inventoryService);

        // 10. Register FacilityService
        IFacilityService facilityService = new FacilityService();
        ServiceRegistry.Instance.Register<IFacilityService>(facilityService);

        // 11. Register DungeonService
        IDungeonService dungeonService = new DungeonService();
        ServiceRegistry.Instance.Register<IDungeonService>(dungeonService);

        // 12. Register SynthesizerService
        ISynthesizerService synthesizerService = new SynthesizerService();
        ServiceRegistry.Instance.Register<ISynthesizerService>(synthesizerService);

        // 13. Load Hub scene with error handling
        try
        {
            SceneManager.LoadScene("Hub");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load Hub scene. Ensure 'Hub' is added to Build Settings. Error: {ex.Message}");
        }
    }
}
