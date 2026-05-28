# Pick Me Up — Infinite Gacha
## Agent Instructions

You are a Unity 6 game development agent working on an idle gacha RPG 
based on the manhwa "Pick Me Up, Infinite Gacha".
Target: Android portrait 1080x2340, Samsung Galaxy A34.

## Workflow Rules
- Claude audits all new files before they are applied
- You handle: applying approved code, wiring, MCP operations, bug fixes
- Never rewrite a completed file from scratch — only extend or fix
- After every file change, check Unity console for errors before continuing
- If errors exist, fix them before moving to the next task

## Architecture — Never Break These
- Service Locator pattern via ServiceRegistry (DontDestroyOnLoad)
- All UI built procedurally via Editor scripts in Assets/Scripts/Editor/
- No DOTween — Coroutines only
- No 3D models — 2D sprites only
- No manual Inspector wiring — all wired in code via SerializedObject
- Every new service must be registered in BootLoader.cs in correct order

## Scene Structure
- Boot (index 0) — loads all services, routes to Hub
- Hub (index 1) — main lobby, navigation, currency display
- Summon (index 2) — full screen gacha, 3-phase animation
- Roster (index 3) — hero collection, detail panel, promote/synthesize
- Tower (index 4) — combat/floors (not built yet)

## Service Registration Order (BootLoader.cs)
1. ServiceRegistry
2. ISaveLoadService → SaveLoadService
3. GameSaveData (loaded from disk)
4. GameStateService
5. ICurrencyService → CurrencyService
6. IGachaService → GachaService
7. IRosterService → RosterService

## Folder Structure
Assets/
  Scripts/
    Core/        — GameSaveData, HeroInstance, HeroDefinition, BootLoader
    Services/    — all service interfaces and implementations
    UI/          — HubView, SummonView, RosterView, RosterHeroCard
    Editor/      — SetupHubUI, SetupSummonUI, SetupRosterUI, ProjectSetupTool
  Scenes/        — Boot, Hub, Summon, Roster
  ScriptableObjects/Heroes/ — HeroDefinition assets
  Resources/
    Heroes/      — HeroDefinition ScriptableObjects
    UI/          — card frames, icons, prefabs

## Data Rules (from manhwa — never break)
- No duplicate heroes — every summon is a unique individual
- Permadeath is permanent — IsAlive=false means combat death, never revived
- Synthesis = permanent deletion from roster (Remove, not MarkDead)
- Starting Morale on summon = 40-60 randomized
- HiddenPotential never shown as number — flavor text only:
  - 0.0-0.3 = "This unit's ceiling appears limited."
  - 0.3-0.7 = "Something stirs beneath the surface."
  - 0.7-1.0 = "The System struggles to quantify this unit."
- Training refines skills only — does NOT increase stats

## Enums
HeroClass: Vanguard, Scout, Mage, Berserker, Assassin, Support, Specialist
PersonalityTrait: Brave, Cowardly, Reckless, Disciplined, Loyal, Traumatized

## Hero Roster (ScriptableObjects in Resources/Heroes/)
Han Israt — 1★ Scout Disciplined
Jenna Shirai — 1★ Assassin Brave
Aaron Delkard — 1★ Vanguard Disciplined
Belquist — 4★ Berserker Reckless
Nerissa — 4★ Assassin Disciplined
Iolka — 4★ Mage Brave
Kishasha — 4★ Berserker Reckless
Edith — 4★ Vanguard Loyal
Katio — 4★ Support Traumatized

## Completed Systems
- Boot scene + ServiceRegistry + SaveLoadService + GameStateService
- CurrencyService (Gold, Gems, AttributeStones)
- GachaService (dual banner, pity, weighted RNG)
- RosterService (GetAll, GetAlive, GetDead, MarkDead, Remove)
- Hub scene UI (TopBar, CenterArea, BottomDock with 5 nav buttons)
- Summon scene UI (3-phase animation: crack, drop, flip)
- Roster scene UI (grid, filters, detail panel, promote, synthesize)

## Known Issues / TODO
- SummonView.ShowAffordError is a Debug.LogWarning placeholder — needs UI toast
- Roster has wiring and UI issues (currently being fixed)
- Object pooling not yet implemented for Roster grid (needed at 200+ heroes)
- Resources.Load for sprites — migrate to Addressables later
- Tower scene not built yet

## Tools Available
- Unity MCP: read hierarchy, create GameObjects, modify components, run Editor scripts
- Always use MCP to apply changes rather than just showing code
- Run Editor tool scripts via MCP after generating them