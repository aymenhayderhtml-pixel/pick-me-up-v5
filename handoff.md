Handoff: Dungeon Scene + Reward Flow — pick-me-up-v5
=====================================================

State as of: session end — dungeon reward/loot flow complete, enemy prefab wired, text wrap fix applied.

---

What is now live
----------------
- Dungeon.unity is rebuilt from `DungeonSceneBuilder` — full combat layout (TopBar / CenterStage / BottomBand / VictoryPanel / DefeatPanel / RewardPanel / --Controllers-- with 5 controllers)
- All 4 default dungeons (Isralta Mine, Kendert Forest, Singmirel Plateau, Main Tower) have `typedLootTable → loot_basic`
- 3 item definitions exist: Minor Heal Potion, Iron Ore, Attribute Shard
- Enemy prefab exists (`Assets/Resources/EnemyPrefabs/DefaultEnemy.prefab` — white square, SpriteRenderer + BoxCollider2D + EnemyEntity)
- `DungeonSpawnManager.prefabMappings` has 1 entry: Goblin → DefaultEnemy
- Reward flow works: dungeon clear → LootRoller rolls gold/exp/items → CurrencyService.AddGold/AddDungeonExp → InventoryService.CreateItemInstance → DungeonRewardView shows reward panel → CLAIM hides panels + returns to list
- Dungeon list text wrapping fixed (`enableWordWrapping = false` + `overflowMode = Ellipsis` in DungeonListView.CreateTMP)

---

File inventory (all changes vs initial commit ae85e03)
---------------------------------------------------

**Modified:**
- Assets/Scripts/Core/BootLoader.cs — IDungeonRewardService registered before IDungeonService
- Assets/Scripts/Core/GameSaveData.cs — added int DungeonExp
- Assets/Scripts/Core/IDungeonService.cs — added event Action<DungeonRewardResult> OnRewardGranted
- Assets/Scripts/Services/ICurrencyService.cs — added GetDungeonExp / SpendDungeonExp / AddDungeonExp
- Assets/Scripts/Services/CurrencyService.cs — implemented the 3 new EXP methods
- Assets/Scripts/Services/DungeonService.cs — full rewrite (reward hook in ClearDungeon, OnRewardGranted event)
- Assets/Scripts/Dungeon/DungeonDataSO.cs — added typedLootTable field
- Assets/Scripts/Dungeon/DungeonHUD.cs — added rewardView reference + event forwarding
- Assets/Scripts/Dungeon/DungeonListView.cs — button sizing (180px, icon slot), text wrapping fix
- Assets/Editor/DungeonSceneBuilder.cs — TopBar layout fix, portrait frame, RewardPanel, reward view wiring, enemy prefab mapping
- Assets/Scripts/Editor/CreateDefaultDungeons.cs — item/loot factory, CreateEnemyPrefab, typedLootTable wiring

**New:**
- Assets/Scripts/Dungeon/DungeonLootTableSO.cs — loot table + DungeonRewardItem + DungeonRewardResult
- Assets/Scripts/Dungeon/LootRoller.cs — static weighted random roller
- Assets/Scripts/Dungeon/DungeonRewardView.cs — runtime reward panel UI (rows + CLAIM button)
- Assets/Scripts/Core/IDungeonRewardService.cs — reward service interface
- Assets/Scripts/Services/DungeonRewardService.cs — reward service (gold, exp, items)
- Assets/Scripts/UI/DungeonSceneController.cs — Back to Hub button handler

**Assets (generated):**
- Assets/Resources/Dungeons/loot_basic.asset
- Assets/Resources/Dungeons/isralta_mine/kendert_forest/singmirel_plateau/main_tower.asset (typedLootTable bound)
- Assets/Resources/Items/item_minor_heal.asset
- Assets/Resources/Items/item_iron_ore.asset
- Assets/Resources/Items/item_attribute_shard.asset
- Assets/Resources/EnemyPrefabs/DefaultEnemy.prefab
- Assets/Resources/EnemyPrefabs/DefaultEnemySprite.png

---

Known issues / next steps
-------------------------
1. No player combat system — enemies spawn (6 clones under SpawnRoot) but cannot be killed. Victory/reward flow works if you manually clear waves. Need a player attack input or auto-attack system.
2. Enemy portrait is still a dark placeholder block (no real sprites).
3. Dungeon icons are null (no sprites on DungeonDataSO.icon).
4. "Back to Hub" button navigates to Hub scene but does not save dungeon progress first.
5. DungeonRunState is not serialized/restored across scene loads (no mid-run save).

---

Editor menu items (run in order first session of next day)
----------------------------------------------------------
1. Tools > Pick Me Up > Create Default Enemy Prefab
2. Tools > Build Dungeon Scene
3. Tools > Add Dungeon To Build (no-op if already there)
4. Check Console — should be clean (ignore MCP-FOR-UNITY websocket noise)

---

Quick constraints reminder
--------------------------
- Unity 6, Android landscape 2340x1080, DX11 editor
- No namespaces, no DOTween, use ServiceRegistry.Instance.Resolve<T>()
- Keep UI procedurally built through Editor tools (SerializedObject wiring)
- Prefer extend/fix over rewrite for completed files
