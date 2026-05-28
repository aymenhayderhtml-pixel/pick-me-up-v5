using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using PickMeUp.Game.Core;
using PickMeUp.Game.Services;

namespace PickMeUp.Game
{
    /// <summary>
    /// Bootstraps the game by initializing all services in the correct order.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [Header("Service References")]
        public bool AutoInitialize = true;

        // Services
        private ISaveLoadService _saveLoadService;
        private IGameStateService _gameStateService;
        private ICurrencyService _currencyService;
        private IGachaService _gachaService;
        private IRosterService _rosterService;
        private ITowerService _towerService;
        private IInventoryService _inventoryService;
        private IDungeonService _dungeonService;
        private IFacilityService _facilityService;
        private ISynthesizerService _synthesizerService;

        // State
        private bool _isInitialized;
        private float _initTimer;

        private void Start()
        {
            if (AutoInitialize)
            {
                Initialize();
            }
        }

        private void Update()
        {
            // Process time-based systems
            if (_isInitialized)
            {
                _initTimer += Time.deltaTime;

                // Process every 60 seconds
                if (_initTimer >= 60f)
                {
                    _initTimer = 0f;
                    ProcessTimedUpdates();
                }
            }
        }

        /// <summary>
        /// Initializes all game services in the correct order.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("BootLoader: Already initialized!");
                return;
            }

            Debug.Log("BootLoader: Starting initialization...");

            try
            {
                // 1. Save/Load Service (must be first)
                _saveLoadService = new SaveLoadService();
                ServiceLocator.Register<ISaveLoadService>(_saveLoadService);
                Debug.Log("BootLoader: SaveLoadService initialized");

                // 2. Game State Service
                _gameStateService = new GameStateService();
                ServiceLocator.Register<IGameStateService>(_gameStateService);
                Debug.Log("BootLoader: GameStateService initialized");

                // 3. Currency Service
                _currencyService = new CurrencyService(_saveLoadService);
                ServiceLocator.Register<ICurrencyService>(_currencyService);
                Debug.Log("BootLoader: CurrencyService initialized");

                // 4. Gacha Service
                _gachaService = new GachaService(_saveLoadService, _currencyService);
                ServiceLocator.Register<IGachaService>(_gachaService);
                Debug.Log("BootLoader: GachaService initialized");

                // 5. Roster Service (needs GachaService for tracking)
                _rosterService = new RosterService(_saveLoadService);
                ServiceLocator.Register<IRosterService>(_rosterService);
                Debug.Log("BootLoader: RosterService initialized");

                // 6. Tower Service
                _towerService = new TowerService(_saveLoadService, _rosterService);
                ServiceLocator.Register<ITowerService>(_towerService);
                Debug.Log("BootLoader: TowerService initialized");

                // 7. Inventory Service
                _inventoryService = new InventoryService(_saveLoadService, _rosterService, _currencyService);
                ServiceLocator.Register<IInventoryService>(_inventoryService);
                Debug.Log("BootLoader: InventoryService initialized");

                // 8. Dungeon Service
                _dungeonService = new DungeonService(_saveLoadService, _gameStateService, _currencyService, _rosterService, _inventoryService);
                ServiceLocator.Register<IDungeonService>(_dungeonService);
                Debug.Log("BootLoader: DungeonService initialized");

                // 9. Facility Service
                _facilityService = new FacilityService(_saveLoadService, _currencyService, _rosterService, _gameStateService);
                ServiceLocator.Register<IFacilityService>(_facilityService);
                Debug.Log("BootLoader: FacilityService initialized");

                // 10. Synthesizer Service
                _synthesizerService = new SynthesizerService(_saveLoadService, _rosterService, _currencyService);
                ServiceLocator.Register<ISynthesizerService>(_synthesizerService);
                Debug.Log("BootLoader: SynthesizerService initialized");

                // Load saved game data
                var saveData = _saveLoadService.LoadGame();
                Debug.Log($"BootLoader: Loaded save data. Heroes: {saveData.Heroes.Count}, Gold: {saveData.Gold}");

                // Process initial stamina regeneration
                _dungeonService.ProcessStaminaRegeneration();

                // Process facility passive generation
                _facilityService.ProcessPassiveGeneration();

                _isInitialized = true;
                Debug.Log("BootLoader: Initialization complete!");

                // Load the Hub scene
                LoadHubScene();
            }
            catch (Exception e)
            {
                Debug.LogError($"BootLoader: Initialization failed - {e.Message}");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Loads the Hub scene after initialization.
        /// </summary>
        private void LoadHubScene()
        {
            Debug.Log("BootLoader: Loading Hub scene...");
            SceneManager.LoadScene("Hub");
        }

        /// <summary>
        /// Process periodic updates for time-based systems.
        /// </summary>
        private void ProcessTimedUpdates()
        {
            if (_dungeonService != null)
            {
                _dungeonService.ProcessStaminaRegeneration();
            }

            if (_facilityService != null)
            {
                _facilityService.ProcessPassiveGeneration();
            }
        }

        /// <summary>
        /// Gets a service by type.
        /// </summary>
        public T GetService<T>() where T : class
        {
            return ServiceLocator.Get<T>();
        }

        /// <summary>
        /// Resets all game data and starts fresh.
        /// </summary>
        public void ResetGame()
        {
            Debug.Log("BootLoader: Resetting game data...");

            var newSave = GameSaveData.CreateNew();
            _saveLoadService.SaveGame(newSave);

            // Reset services
            _dungeonService?.ResetProgress();
            _facilityService?.ResetProgress();
            _towerService?.ResetProgress();

            // Reload
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            // Save before destroy
            if (_saveLoadService != null && _isInitialized)
            {
                _saveLoadService.SaveGame(_saveLoadService.LoadGame());
            }
        }
    }
}