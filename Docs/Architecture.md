# Architecture

## Overview
Pick Me Up is a Unity 6 Android landscape 2D gacha RPG. The project uses a service-based architecture managed through `ServiceRegistry` with scenes loaded additively around a persistent boot scene.

## Core Architecture

- **BootLoader.cs** — Entry point. Registers all services in order, loads persistent managers, forces landscape orientation, then transitions to Hub.
- **ServiceRegistry** — Central service locator (not ServiceLocator). Services register themselves; consumers request via interface.
- **No namespaces** — All game scripts live in the global namespace per project convention.
- **No DOTween** — All tweening uses Unity coroutines.
- **Editor tooling over Inspector wiring** — UI is procedurally built through Editor scripts using `SerializedObject`.

## Scene Flow

```
Boot → Hub (main menu)
         ├── SummonScene (Moebius animation)
         ├── RosterScene
         ├── TowerScene
         ├── DungeonScene (wave-based combat)
         ├── InventoryScene
         ├── MemorialHallScene
         ├── FacilitiesScene (Citadel HQ)
         └── SynthesisLabScene
```

All gameplay scenes load additively over the Hub. The Hub handles navigation and currency display.

## Key Services

| Service | Responsibility |
|---|---|
| GameStateService | Holds live GameSaveData in memory, central state |
| SaveLoadService | JSON-based persistence with versioned save data |
| CurrencyService | Gold, Gems, and premium currency tracking |
| HeroService (in RosterService) | Hero roster, stats, class, star level |
| GachaService | Gacha logic, banners, pity system |
| DungeonService | Daily dungeon scheduling and rewards |
| DungeonRewardService | Loot tables and reward distribution |
| TowerService | Tower progression and floors |
| InventoryService | Item storage and equipment |
| MemorialService (in FacilityService) | Hero discovery tracking and lore |
| FacilityService | Facility upgrades and passive generation |
| SynthesizerService | Hero combining, promotion, removal |

## Data Flow

```
Save/Load ←→ Service Layer ←→ UI (procedural)
                  ↕
            GameState (persistent)
```

- Services hold runtime state and persist via `SaveLoadService`.
- UI reads from services and invokes service methods.
- Scene transitions preserve service state (services are boot-scene singletons).

## Configuration

- **SummonData** — Banner rates, pity thresholds, costs
- **HeroData** — Base stats, growth curves, class
- **DungeonData** — Daily schedules, encounter tables, rewards
- **FacilityData** — Upgrade costs, generation rates, level caps
- **FeaturedUnitConfig** — Active banner featured hero rotations
- **RarityVisualConfig** — Color/material presets per star rarity tier
- **HeroClassColorConfig** — Color mapping for hero classes (Combat, Support, Special)
- **BalanceData** — Economy tuning values (costs, stamina, drop rates)

## Summon System

The summon scene uses a Moebius-style animation system:

- **SummonAnimationController** — Orchestrates the multi-stage summon sequence (idle → charge → reveal)
- **SummonIdleState / SummonIdleCircle** — Ambient ring/portal visuals while waiting
- **RarityVisualApplier** — Applies color/material tiers based on star rarity at reveal
- **RarityVisualConfig** — Data-driven color/effect presets per rarity level
- **FeaturedUnitConfig** — Which heroes are rate-up on the current banner

## Dungeon System

Wave-based combat with a modular data-driven structure:

- **DungeonDataSO** — Per-dungeon definition (schedule, encounter table, difficulty)
- **EnemyDataSO** — Enemy stat blocks (HP, attack, abilities)
- **EnemyWaveSO** — Wave composition and spawn timing
- **DungeonSpawnManager** — Handles wave spawning and encounter lifecycle
- **DungeonRunState** — Tracks current run progress, wave index, rewards
- **EnemyEntity** — Runtime enemy behavior (movement, targeting, death)
- **LootRoller** — Probability-based loot distribution from loot tables
- **DungeonLootTableSO** — Drop table definitions (items, quantities, weights)
- **DungeonHUD** — In-run UI (health bars, wave counter, rewards)
- **DungeonListView** — Dungeon selection screen
- **DungeonRewardView** — Post-run reward summary screen

## Editor Tools

Procedural scene and asset generation tools under `Assets/Scripts/Editor/`:

| Tool | Purpose |
|---|---|
| PortraitGenerator | Generates hero portrait placeholder sprites |
| CanonizeHeroDefinitions | Validates and fixes hero definitions against canon |
| CreateClassColorConfig | Creates the HeroClassColorConfig asset |
| CreateDefault2StarHero | Creates a default 2-star hero template |
| CreateDefaultDungeons | Populates dungeon, enemy, and loot table assets |
| DungeonSceneBuilder | Builds the Dungeon scene UI programmatically |
| SetupRosterScene | Builds the Roster scene UI |
| SetupSummonUI / SetupMoebiusSummonUI | Builds Summon scene UI |
| SetupFacilityUI / SetupHubUI / SetupInventoryUI | Scene UI builders |
| SetupMemorialHallUI / SetupSynthUI / SetupTowerUI | Scene UI builders |
| AddDungeonToBuild | Adds dungeon scenes to build settings |
| FixSummonSceneLive | Hotfix for summon scene runtime wiring |
| ProjectSetupTool | One-click project validation and setup |

## Save System

- Single JSON file per profile
- Versioned schema with default-fill for new fields
- Saved on scene transitions and critical actions

## Constraints

- 2D sprites only, no 3D models
- Landscape orientation (BootLoader enforces LandscapeLeft, no auto-rotation)
- Target: Samsung Galaxy A34
- Android-only

## Coding Rules

- No namespaces in game scripts.
- Use `ServiceRegistry` for all service access — do not create duplicate managers.
- Prefer extending existing services before creating new ones.
- Use interfaces for service access, not direct class references.
- No static gameplay state outside boot-managed services.
- No DOTween — use Unity coroutines for animations and delays.
- No manual Inspector wiring — use `SerializedObject` in Editor scripts.
- Check Unity Console after every change; fix errors before moving on.

## UI Rules

- Landscape-only layouts (2340×1080).
- Mobile-first: touch input, safe area padding, responsive scaling.
- Dark fantasy aesthetic.
- Avoid hardcoded UI references — prefer procedural generation via Editor tools.
- Reuse existing UI prefabs when possible.

## Important Folders

```
Assets/Scripts/Core       — BootLoader, ServiceRegistry, save system, interfaces
Assets/Scripts/Services   — All service implementations
Assets/Scripts/UI         — Procedural UI builders and scene scripts
Assets/Scripts/Editor     — Editor tools, setup scripts, validators
Assets/Scripts/Dungeon    — Wave-based combat system (SOs, spawner, HUD, enemies)
Assets/Scripts/Summon     — Moebius summon animation system
Assets/Scripts/TeamSelect — Team composition screens
Assets/Resources          — Prefabs, sprites, heroes, dungeons, items, portraits
Assets/Configs            — Runtime config assets (featured units)
Assets/ScriptableObjects  — Class definitions (hero classes, player profile)
```
`
