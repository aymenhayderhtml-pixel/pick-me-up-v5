# Architecture

## Overview
Pick Me Up is a Unity 6 Android portrait 2D gacha RPG. The project uses a service-based architecture managed through `ServiceRegistry` with scenes loaded additively around a persistent boot scene.

## Core Architecture

- **BootLoader.cs** — Entry point. Registers all services in order, loads persistent managers, then transitions to Hub.
- **ServiceRegistry** — Central service locator (not ServiceLocator). Services register themselves; consumers request via interface.
- **No namespaces** — All game scripts live in the global namespace per project convention.
- **No DOTween** — All tweening uses Unity coroutines.
- **Editor tooling over Inspector wiring** — UI is procedurally built through Editor scripts using `SerializedObject`.

## Scene Flow

```
Boot → Hub (main menu)
         ├── SummonScene
         ├── RosterScene
         ├── TowerScene
         ├── DungeonScene
         ├── InventoryScene
         ├── MemorialHallScene
         ├── FacilitiesScene
         └── SynthesisLabScene
```

All gameplay scenes load additively over the Hub. The Hub handles navigation and currency display.

## Key Services

| Service | Responsibility |
|---|---|
| SaveLoadService | JSON-based persistence with versioned save data |
| CurrencyService | Gold, Gems, and premium currency tracking |
| StaminaService | Stamina regen and cost validation |
| HeroService | Hero roster, stats, class, star level |
| SummonService | Gacha logic, banners, pity system |
| DungeonService | Daily dungeon scheduling and rewards |
| TowerService | Tower progression and floors |
| InventoryService | Item storage and equipment |
| MemorialService | Hero discovery tracking and lore |
| FacilityService | Facility upgrades and passive generation |
| SynthesisService | Hero combining, promotion, removal |

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
- **BalanceData** — Economy tuning values (costs, stamina, drop rates)

## Save System

- Single JSON file per profile
- Versioned schema with default-fill for new fields
- Saved on scene transitions and critical actions

## Constraints

- 2D sprites only, no 3D models
- Landscape orientation fixed
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

- Portrait-only layouts (1080×2340).
- Mobile-first: touch input, safe area padding, responsive scaling.
- Dark fantasy aesthetic.
- Avoid hardcoded UI references — prefer procedural generation via Editor tools.
- Reuse existing UI prefabs when possible.

## Important Folders

```
Assets/Scripts/Core      — BootLoader, ServiceRegistry, save system
Assets/Scripts/Services  — All service implementations
Assets/Scripts/UI        — Procedural UI builders and scene scripts
Assets/Scripts/Data      — ScriptableObjects and data definitions
Assets/Scripts/Editor    — Editor tools, setup scripts, validators
Assets/Resources         — Prefabs, sprites, and content loaded at runtime
```
